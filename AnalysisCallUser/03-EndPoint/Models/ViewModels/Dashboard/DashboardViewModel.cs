using AnalysisCallUser._03_EndPoint.Models.ViewModels.Analytics;
using AnalysisCallUser._03_EndPoint.Models.ViewModels.Call;

namespace AnalysisCallUser._03_EndPoint.Models.ViewModels.Dashboard
{
    public class DashboardViewModel
    {
        public int TotalCalls { get; set; }
        public int AnsweredCalls { get; set; }
        public double AverageCallDuration { get; set; }
        public List<ChartPoint> TopCountries { get; set; }
        public List<ChartPoint> RecentActivity { get; set; }
        public List<CallDetailsViewModel> RecentCalls { get; set; } = new();

    }
}
