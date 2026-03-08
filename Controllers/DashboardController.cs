using Microsoft.AspNetCore.Mvc;

namespace VideoDetectionPOC.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
