using System;
using System.Collections.Generic;
using System.Linq;
using MySqlConnector;

namespace WebStats.Persistence {
    public class WebstatsContext {
        public string ConnectionString { get; set; }


        public WebstatsContext(string connectionString) {
            this.ConnectionString = connectionString;
        }

        private MySqlConnection GetConnection() {
            return new MySqlConnection(ConnectionString);
        }


        private class InternalAggregateEntry {
            public string Id { get; set; }
            public int Past24H { get; set; }
            public int Past7d { get; set; }
            public int Past14d { get; set; }
            public int Past30d { get; set; }
        }

        /// <summary>
        /// Returns the service information based on shortname
        /// return null if the service does not exist
        /// </summary>
        public ServiceInfo GetServiceInfo(string shortName) {
            const string query = "SELECT id_service, description, project_url FROM services WHERE shortname = @shortname";

            using (MySqlConnection conn = GetConnection()) {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@shortname", shortName);

                using (var reader = cmd.ExecuteReader()) {
                    while (reader.Read()) {
                        return new ServiceInfo() {
                            ServiceId = Convert.ToInt32(reader["id_service"]),
                            Description = reader["description"].ToString(),
                            ProjectUrl = reader["project_url"].ToString() ?? string.Empty,
                        };
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Returns aggregate data for graphs
        /// </summary>
        /// <param name="serviceId"></param>
        /// <param name="numDays"></param>
        /// <returns></returns>
        public Aggregate GetAggregate(int serviceId, int numDays) {
            List<InternalAggregateEntry> list = new List<InternalAggregateEntry>();

            const string query =
                @"SELECT id_aggregate, num_24h, num_7d, num_14d, num_30d
                    FROM aggregate 
                    WHERE id_service = @serviceId
                    AND id_aggregate > NOW() - interval @numDays day
                    ORDER BY id_aggregate ASC
                    ";

            using (MySqlConnection conn = GetConnection()) {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@serviceId", serviceId);
                cmd.Parameters.AddWithValue("@numDays", numDays);

                using (var reader = cmd.ExecuteReader()) {
                    while (reader.Read()) {
                        list.Add(new InternalAggregateEntry() {
                            Id = reader["id_aggregate"].ToString(), // TODO: Need some better conversion here
                            Past24H = Convert.ToInt32(reader["num_24h"]),
                            Past7d = Convert.ToInt32(reader["num_7d"]),
                            Past14d = Convert.ToInt32(reader["num_14d"]),
                            Past30d = Convert.ToInt32(reader["num_30d"]),
                        });
                    }
                }
            }

            return new Aggregate {
                Past24H = list.Select(m => m.Past24H).ToList(),
                Past7d = list.Select(m => m.Past7d).ToList(),
                Past14d = list.Select(m => m.Past14d).ToList(),
                Past30d = list.Select(m => m.Past30d).ToList(),
            };
        }

        /// <summary>
        /// Returns the version distributions
        /// One row per active version in use.
        ///
        /// Returns an empty list if the serviceId does not exist.
        /// </summary>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        public List<VersionInfo> GetVersionDistribution(int serviceId) {
            List<VersionInfo> list = new List<VersionInfo>();
            const string query = "SELECT `version`, COUNT(`uuid`) as N FROM versions WHERE modified_at > NOW() - interval 30 DAY AND id_service = @serviceId GROUP BY `version`";

            using (MySqlConnection conn = GetConnection()) {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@serviceId", serviceId);

                using (var reader = cmd.ExecuteReader()) {
                    while (reader.Read()) {
                        list.Add(new VersionInfo {
                            Version = reader["version"].ToString(),
                            NumUsers = Convert.ToInt32(reader["N"])
                        });
                    }
                }
            }

            return list;
        }

        public void SetClientVersion(int serviceId, string uuid, string version) {
            const string query = @"INSERT INTO versions (id_service,UUID,VERSION,modified_at) 
                    VALUES (@serviceId, @uuid, @version, NOW())
                    ON DUPLICATE KEY UPDATE version=VALUES(VERSION), modified_at = NOW()";

            using (MySqlConnection conn = GetConnection()) {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@serviceId", serviceId);
                cmd.Parameters.AddWithValue("@uuid", uuid);
                cmd.Parameters.AddWithValue("@version", version);
                cmd.ExecuteNonQuery();
            }
        }


        public void InsertUsageEntry(int serviceId, string uuid) {
            const string query = "INSERT INTO usageentry (id_service, uuid) VALUES (@serviceId, @uuid)";

            using (MySqlConnection conn = GetConnection()) {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@serviceId", serviceId);
                cmd.Parameters.AddWithValue("@uuid", uuid);
                cmd.ExecuteNonQuery();
            }
        }
    }
}