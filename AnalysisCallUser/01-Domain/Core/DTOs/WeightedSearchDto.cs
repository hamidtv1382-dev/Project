namespace AnalysisCallUser._01_Domain.Core.DTOs
{
    public class WeightedSearchDto
    {
        public List<string> ANumbers { get; set; } = new List<string>();
        public List<string> BNumbers { get; set; } = new List<string>();
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IncludeAnsweredCallsOnly { get; set; } = true;
        public bool SearchInAllDatabase { get; set; } = false;
        public bool BidirectionalSearch { get; set; } = true;
        public int MinWeight { get; set; } = 1;

        // پراپرتی برای تعیین حالت جستجو
        public WeightedSearchMode SearchMode { get; set; } = WeightedSearchMode.Auto;
    }

    public enum WeightedSearchMode
    {
        Auto, // به صورت خودکار تشخیص دهد
        SourceOnly, // فقط در شماره‌های مبدأ جستجو کند
        DestinationOnly, // فقط در شماره‌های مقصد جستجو کند
        SourceDestinationPairs // فقط بین جفت‌های مبدأ و مقصد وارد شده جستجو کند
    }
}
