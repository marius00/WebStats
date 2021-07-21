using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

using WebStats.Persistence;

namespace WebStats.Pages {
    public class IndexModel : PageModel {
        private readonly WebstatsContext _database;

        private readonly ILogger<IndexModel> _logger;


        public string Title { get; private set; }
        public string ProjectUrl { get; private set; }
        public Aggregate History { get; private set; }
        public List<VersionInfo> VersionInfo { get; private set; }

        public IndexModel(ILogger<IndexModel> logger, WebstatsContext database) {
            _logger = logger;
            _database = database;
        }

        public IActionResult OnGet(string id) {
            var serviceInfo = _database.GetServiceInfo(id);
            if (serviceInfo != null) {
                History = _database.GetAggregate(serviceInfo.ServiceId, 90);
                VersionInfo = _database.GetVersionDistribution(serviceInfo.ServiceId);
                Title = serviceInfo.Description;
                ProjectUrl = serviceInfo.ProjectUrl;
                return Page();
            }

            return NotFound();
        }

    }
}
