namespace AnalysisCallUser._01_Domain.Core.DTOs
{
    public class AnalyticsDataDto
    {
        public class ChartPoint
        {
            public string Label { get; set; }
            public int Value { get; set; }
        }

        public List<ChartPoint> TopCountries { get; set; }
        public List<ChartPoint> AnsweredVsNotAnswered { get; set; }
        public List<ChartPoint> CallVolumeOverTime { get; set; }
    }
}
