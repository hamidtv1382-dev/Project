namespace AnalysisCallUser._01_Domain.Core.DTOs
{
    public class AnalyticsDto
    {
        public ChartDataDto CallVolumeChart { get; set; }
        public MapDataDto GeographicMap { get; set; }
        public NetworkDto NetworkGraph { get; set; }
    }
}
