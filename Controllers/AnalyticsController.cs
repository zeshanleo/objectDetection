using Microsoft.AspNetCore.Mvc;

namespace VideoDetectionPOC.Controllers
{
    public class AnalyticsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
