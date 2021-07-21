using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebStats.Persistence;

namespace WebStats {
    /// <summary>
    /// All modified/added code is tagged with "Non-default", the rest is template.
    /// </summary>
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddRazorPages();
            
            
            // Non-default
            services.AddHostedService<AggregateDataCron>();
            string connectionString = GetConnectionString();
            services.Add(new ServiceDescriptor(typeof(WebstatsContext), new WebstatsContext(connectionString)));
            services.Add(new ServiceDescriptor(typeof(Aggregator), new Aggregator(connectionString, null)));
        }

        // Non-default
        // TODO: Not really the relevant place for it..
        static string GetConnectionString() {
            var server = Environment.GetEnvironmentVariable("DATABASE_HOST");
            var user = Environment.GetEnvironmentVariable("DATABASE_USER");
            var pass = Environment.GetEnvironmentVariable("DATABASE_PASSWORD");
            var db = Environment.GetEnvironmentVariable("DATABASE_NAME");


            return $"server={server};port=3306;database={db};user={user};password={pass};sslmode=None;";
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }
            else {
                
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            }

            app.UseStatusCodePagesWithReExecute("/errors/{0}");
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapRazorPages();

                // Non-default
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "report/{id}",

                    defaults: new { controller = "Reporting", action = "OnPost" }
                    );
            });

        }
    }
}
