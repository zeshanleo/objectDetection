
using VideoDetectionPOC.Models.Dtos;

namespace VideoDetectionPOC.ViewModel
{
    public class TimelineViewModel
    {
        public Guid VideoId { get; set; }
        public string VideoName { get; set; } = string.Empty;
        public DateTime UploadDate { get; set; }
        public int? DurationSeconds { get; set; }

        public List<TimelineDto> Timeline { get; set; } = new();
    }
}
