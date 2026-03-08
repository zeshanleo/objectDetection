using Microsoft.AspNetCore.Mvc.Rendering;
using VideoDetectionPOC.Models.Dtos;

namespace VideoDetectionPOC.ViewModel
{
    public class DetectionSearchViewModel
    {
        public int? SelectedObjectTypeId { get; set; }
        public List<int>? SelectedObjectTypeIds { get; set; }

        public double? MinConfidence { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public string? VideoName { get; set; }

        public int? FromSecond { get; set; }
        public int? ToSecond { get; set; }

        public int? MinObjectCount { get; set; }

        public List<SelectListItem> ObjectTypes { get; set; } = new();

        public List<VideoSearchResultDto> Results { get; set; } = new();
    }
}
