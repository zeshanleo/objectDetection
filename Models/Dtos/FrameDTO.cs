namespace VideoDetectionPOC.Models.Dtos
{
    public class FrameDto
    {
        public Guid FrameId { get; set; }

        public string FramePath { get; set; } = String.Empty;

        public int FrameIndex { get; set; }

        public Guid VideoId { get; set; }

        public string VideoFile { get; set; } = String.Empty;

        public string ObjectType { get; set; } = String.Empty;

        public string Label { get; set; } = String.Empty;

        public float Confidence { get; set; }

        public DateTimeOffset DetectionTime { get; set; }

        public int X1 { get; set; }

        public int Y1 { get; set; }

        public int X2 { get; set; }

        public int Y2 { get; set; }
    }
}
