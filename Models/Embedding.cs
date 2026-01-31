using System.ComponentModel.DataAnnotations;

namespace VideoDetectionPOC.Models
{
    public class Embedding
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        public Guid DetectionId { get; set; }
        public Detection? Detection { get; set; }

        public byte[]? VectorData { get; set; }  
        public int Dimensions { get; set; }      
        public string? Source { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
