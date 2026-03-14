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
    }
}
