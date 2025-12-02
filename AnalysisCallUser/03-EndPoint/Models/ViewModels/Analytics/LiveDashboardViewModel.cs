using AnalysisCallUser._01_Domain.Core.DTOs;

namespace AnalysisCallUser._03_EndPoint.Models.ViewModels.Analytics
{
    public class LiveDashboardViewModel
    {
        public int ActiveCallsCount { get; set; }
        public List<CallDetailDto> RecentCalls { get; set; }
    }
}
