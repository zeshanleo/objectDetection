using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.Design;
using VideoDetectionPOC.DataAccess;
using VideoDetectionPOC.Services;
using VideoDetectionPOC.ViewModel;

namespace VideoDetectionPOC.Controllers
{
    public class SearchController : Controller
    {
        private readonly ISearchService _searchService;
        private readonly ApplicationDBContext _context;
        public SearchController(ApplicationDBContext context, ISearchService searchService)
        {
            _searchService = searchService;
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var vm = new DetectionSearchViewModel
            {
                ObjectTypes = await _context.ObjectTypes
                    .Select(o => new SelectListItem
                    {
                        Value = o.Id.ToString(),
                        Text = o.Name
                    }).ToListAsync()
            };

            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> Index(DetectionSearchViewModel model)
        {
            model.ObjectTypes = await _context.ObjectTypes
                .Select(o => new SelectListItem
                {
                    Value = o.Id.ToString(),
                    Text = o.Name
                }).ToListAsync();

            model.Results = await _searchService.SearchAsync(model);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var timeline = await _searchService.GetTimelineAsync(id);

            var video = await _context.Videos
                .Where(v => v.Id == id)
                .Select(v => new
                {
                    v.Id,
                    v.FileName,
                    v.UploadedAt.DateTime,
                    v.DurationSeconds
                })
                .FirstOrDefaultAsync();

            if (video == null)
                return NotFound();

            var vm = new TimelineViewModel
            {
                VideoId = video.Id,
                VideoName = video.FileName,
                UploadDate = video.DateTime,
                DurationSeconds = video.DurationSeconds,
                Timeline = timeline
            };

            return View(vm);
        }
    }
}
