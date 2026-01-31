using System.ComponentModel.DataAnnotations;
using VideoDetectionPOC.Services;

namespace VideoDetectionPOC.Models
{
    public class Video
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        public string FileName { get; set; } = "";
        public string? DeviceId { get; set; }
        public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;
        public bool Processed { get; set; } = false;
        public int? DurationSeconds { get; set; }

        public ICollection<Frame> Frames { get; set; } = new List<Frame>();
        public ICollection<Detection> Detections { get; set; } = new List<Detection>();
    }
}
