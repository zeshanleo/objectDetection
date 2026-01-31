using System.ComponentModel.DataAnnotations;

namespace VideoDetectionPOC.Models
{
    public class Detection
    {
        [Key] 
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid VideoId { get; set; }
        public Video? Video { get; set; }

        public Guid? FrameId { get; set; }
        public Frame? Frame { get; set; }

        public string? Label { get; set; }
        public int X1 { get; set; }
        public int Y1 { get; set; }
        public int X2 { get; set; }
        public int Y2 { get; set; }

        public int ObjectTypeId { get; set; }
        public ObjectType? ObjectType { get; set; }

        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public float Confidence { get; set; }

        // Store boxes as JSON string, EF maps to jsonb on PostgreSQL if configured
        public string BoundingBoxesJson { get; set; } = "";

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public ICollection<Embedding>? Embeddings { get; set; }
    }
}
