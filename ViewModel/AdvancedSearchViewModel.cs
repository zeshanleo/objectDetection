namespace VideoDetectionPOC.ViewModel
{
    public class AdvancedSearchViewModel
    {
        public string Text { get; set; } = String.Empty;

        public List<FilterItem> Filters { get; set; } = new List<FilterItem>();

        public DateTime? Cursor { get; set; }

        public int Limit { get; set; } = 60;
    }

    public class FilterItem
    {
        public string Key { get; set; } = String.Empty;
        public string Operator { get; set; } = String.Empty;
        public string Value { get; set; } = String.Empty;   
    }
}
