namespace VideoDetectionPOC.ViewModel
{
    public class VideoItemViewModel
    {
        public string FileName { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";   // Pending | Processing | Completed
        public DateTime UploadedAt { get; set; }
        public int TotalFrames { get; set; }
        public int DetectedObjects { get; set; }
    }
}
