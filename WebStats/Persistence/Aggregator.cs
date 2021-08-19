using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using WebStats.Persistence.Model;

namespace WebStats.Persistence {
    public class Aggregator {
        private readonly ILogger<Aggregator> _logger;
        public string ConnectionString { get; set; }


        public Aggregator(string connectionString, ILogger<Aggregator> logger) {
            this.ConnectionString = connectionString;

            // TODO: How do I get a logger instance?
            this._logger = logger;
        }

        private MySqlConnection GetConnection() {
            return new MySqlConnection(ConnectionString);
        }

        public List<int> ListServiceIdentifiers() {
            List<int> list = new List<int>();
            const string query = "SELECT id_service FROM services";


            using (MySqlConnection conn = GetConnection()) {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);

                using (var reader = cmd.ExecuteReader()) {
                    while (reader.Read()) {
                        list.Add(Convert.ToInt32(reader["id_service"]));
                    }
                }
            }

            return list;
        }


        public List<AggregateInfo> GetRecentAggregates() {
            const string query = "SELECT id_aggregate, id_service FROM aggregate WHERE id_aggregate > NOW() - interval 8 day";

            List<AggregateInfo> list = new List<AggregateInfo>();
            

            using (MySqlConnection conn = GetConnection()) {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);

                using (var reader = cmd.ExecuteReader()) {
                    while (reader.Read()) {
                        DateTime dt = Convert.ToDateTime(reader["id_aggregate"]);
                        list.Add(new AggregateInfo {
                                ServiceId = Convert.ToInt32(reader["id_service"]),
                                Date = DateTime.SpecifyKind(dt, DateTimeKind.Utc)
                        });
                    }
                }
            }

            return list;
        }


        public void Cleanup() {
            const string usageEntryQuery = "DELETE FROM usageentry WHERE created_at < NOW() - INTERVAL 90 DAY";
            const string versionsEntryQuery = "DELETE FROM versions WHERE modified_at < NOW() - INTERVAL 1 YEAR";

            using (MySqlConnection conn = GetConnection()) {
                conn.Open();
                int numUsageEntriesDeleted = new MySqlCommand(usageEntryQuery, conn).ExecuteNonQuery();
                int numVersionEntriesDeleted = new MySqlCommand(versionsEntryQuery, conn).ExecuteNonQuery();

                //_logger.LogInformation($"Deleted {numUsageEntriesDeleted} usage entries and {numVersionEntriesDeleted} version entries from db");
            }
        }

        /// <summary>
        /// Returns the service information based on shortname
        /// return null if the service does not exist
        /// </summary>
        public bool CreateAggregateEntry(int serviceId, DateTimeOffset date) {
            const string query = @"INSERT INTO aggregate(id_aggregate, id_service, num_24h, num_7d, num_14d, num_30d) 
	        VALUES (
		        @date, 
		        @serviceId, 
		        (SELECT COUNT(DISTINCT(`uuid`)) FROM usageentry WHERE id_service = @serviceId AND created_at <= @date AND created_at >= @date - INTERVAL 24 HOUR), 
		        (SELECT COUNT(DISTINCT(`uuid`)) FROM usageentry WHERE id_service = @serviceId AND created_at <= @date AND created_at >= @date - INTERVAL 7 DAY), 
		        (SELECT COUNT(DISTINCT(`uuid`)) FROM usageentry WHERE id_service = @serviceId AND created_at <= @date AND created_at >= @date - INTERVAL 14 DAY), 
		        (SELECT COUNT(DISTINCT(`uuid`)) FROM usageentry WHERE id_service = @serviceId AND created_at <= @date AND created_at >= @date - INTERVAL 30 DAY)
	        );";

            using (MySqlConnection conn = GetConnection()) {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@serviceId", serviceId);
                cmd.Parameters.AddWithValue("@date", date);

                var success = cmd.ExecuteNonQuery() > 0;
                /*if (success)
                    _logger.LogInformation($"Successfully created aggregate entry for service {serviceId}, date {date}");
                else
                    _logger.LogWarning($"Failed creating aggregate entry for service {serviceId}, date {date}");*/

                return success;
            }
        }
    }
}