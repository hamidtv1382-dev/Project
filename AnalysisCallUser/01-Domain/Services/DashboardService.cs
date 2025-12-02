using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._01_Domain.Core.DTOs;

namespace AnalysisCallUser._01_Domain.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ICallAnalysisService _callAnalysisService;
        private readonly IAnalyticsService _analyticsService;

        public DashboardService(ICallAnalysisService callAnalysisService, IAnalyticsService analyticsService)
        {
            _callAnalysisService = callAnalysisService;
            _analyticsService = analyticsService;
        }

        public async Task<DashboardDto> GetDashboardDataAsync(int userId)
        {
            // این متد داده‌های آماری کلی را برای داشبورد اصلی جمع‌آوری می‌کند
            var topCountriesChart = await _callAnalysisService.GetTopCallingCountriesAsync(5);
            var answeredCallsChart = await _callAnalysisService.GetAnsweredVsNotAnsweredCallsAsync();
            var geographicMap = await _analyticsService.GetGeographicDistributionAsync(new GeographicAnalysisDto());

            return new DashboardDto
            {
                TotalCalls = 0, // باید از یک ریپازیتوری یا سرویس دیگر خوانده شود
                AnsweredCalls = 0, // باید از یک ریپازیتوری یا سرویس دیگر خوانده شود
                AverageCallDuration = 0, // باید از یک ریپازیتوری یا سرویس دیگر خوانده شود
                TopCountries = topCountriesChart.Data,
                RecentActivity = answeredCallsChart.Data // این فقط یک مثال است
            };
        }

        public async Task<AnalyticsDto> GetAnalyticsDataAsync(CallFilterDto filter)
        {
            var callVolumeChart = await _analyticsService.GetCallVolumeOverTimeAsync(new TimeAnalysisDto { StartDate = filter.StartDate.Value, EndDate = filter.EndDate.Value });
            var geographicMap = await _analyticsService.GetGeographicDistributionAsync(new GeographicAnalysisDto());

            // 1. ورودی متد را به NetworkAnalysisDto اصلاح کنید
            var networkGraph = await _analyticsService.GetCallNetworkAnalysisAsync(new NetworkAnalysisDto());

            return new AnalyticsDto
            {
                CallVolumeChart = callVolumeChart,
                GeographicMap = geographicMap,
                NetworkGraph = networkGraph
            };
        }
        public async Task<NetworkDto> GetNetworkGraphDataAsync()
        {
            var data = await _analyticsService.GetCallNetworkAnalysisAsync(new NetworkAnalysisDto());
            return new NetworkDto { Nodes = data.Nodes, Edges = data.Edges };
        }

        public async Task<MapDto> GetGeographicDataAsync()
        {
            var data = await _analyticsService.GetGeographicDistributionAsync(new GeographicAnalysisDto());
            return new MapDto { Points = data.Points };
        }
    }
}
