using System.ComponentModel.DataAnnotations;
using VideoDetectionPOC.Services;

namespace VideoDetectionPOC.Models
{
    public class Frame
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        public Guid VideoId { get; set; }
        public Video? Video { get; set; }

        public int FrameIndex { get; set; }
        public string FramePath { get; set; } = "";
        public string FrameName { get; set; } = "";
        public int? TimestampMs { get; set; }

        public ICollection<Detection> Detections { get; set; } = new List<Detection>();
    }
}
