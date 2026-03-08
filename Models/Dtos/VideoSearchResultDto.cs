namespace VideoDetectionPOC.Models.Dtos
{
    public class VideoSearchResultDto
    {
        public Guid VideoId { get; set; }

        public string VideoName { get; set; } = string.Empty;

        public DateTime UploadDate { get; set; }

        public int TotalDetections { get; set; }

        public int DistinctObjects { get; set; }

        public double AvgConfidence { get; set; }

        // Optional: Extra analytics fields
        public int TotalFramesWithDetections { get; set; }

        public double MaxConfidence { get; set; }
    }
}
