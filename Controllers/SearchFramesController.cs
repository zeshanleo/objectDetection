using Microsoft.AspNetCore.Mvc;
using VideoDetectionPOC.DataAccess;
using VideoDetectionPOC.Services;
using VideoDetectionPOC.ViewModel;

namespace VideoDetectionPOC.Controllers
{
    public class SearchFramesController : Controller
    {
        private readonly ISearchService _searchService;
        private readonly ApplicationDBContext _context;
        public SearchFramesController(ApplicationDBContext context, ISearchService searchService)
        {
            _context = context;
            _searchService = searchService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var vm = new AdvancedSearchViewModel();
            return View(vm);
        }

        [HttpGet]
        public IActionResult Suggest(string q)
        {
            var labels = _context.ObjectTypes
                .Where(o => o.Name.Contains(q))
                .Select(o => o.Name)
                .Distinct()
                .Take(10)
                .ToList();

            return Json(labels);
        }
    }
}
