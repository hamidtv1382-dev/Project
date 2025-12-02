using AnalysisCallUser._03_EndPoint.Models.ViewModels.Analytics;

namespace AnalysisCallUser._03_EndPoint.Models.ViewModels.Dashboard
{
    public class AnalyticsViewModel
    {
        public ChartDataViewModel CallVolumeChart { get; set; }
        public ChartDataViewModel CountryChart { get; set; }
        public NetworkViewModel NetworkGraph { get; set; }
        public GeographicAnalysisViewModel GeographicMap { get; set; }
        public OperatorPerformanceViewModel OperatorChart { get; set; }
        public ChartDataViewModel AnswerRateChart { get; set; }
    }
}
