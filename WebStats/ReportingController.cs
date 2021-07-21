using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using WebStats.Persistence;

namespace WebStats {
    public class ReportingController : Controller {
        private readonly WebstatsContext _database;

        public ReportingController(WebstatsContext database) {
            _database = database;
        }


        public class PostModel {

            [Required, StringLength(50, MinimumLength = 8)]
            public string Uuid { get; set; }

            [Required, StringLength(25, MinimumLength = 3)]
            public string Version { get; set; }
        }



        [HttpPost]
        public IActionResult OnPost(string id, PostModel model) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }


            var serviceInfo = _database.GetServiceInfo(id);
            if (serviceInfo == null)
                return NotFound();

            _database.SetClientVersion(serviceInfo.ServiceId, model.Uuid, model.Version);
            _database.InsertUsageEntry(serviceInfo.ServiceId, model.Uuid);

            return Ok();
        }
    }
}