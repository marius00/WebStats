using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace WebStats.Pages.errors {
    public class NotFoundModel : PageModel {

        private readonly ILogger<IndexModel> _logger;



        public NotFoundModel(ILogger<IndexModel> logger) {
            _logger = logger;
        }
    }
}
