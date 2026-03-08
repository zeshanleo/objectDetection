namespace VideoDetectionPOC.Models.Dtos
{
    public class SearchSummaryDto
    {
        public int TotalVideos { get; set; }
        public int TotalDetections { get; set; }
        public int TotalDistinctObjects { get; set; }
    }
}
