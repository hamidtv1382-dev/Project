using AnalysisCallUser._01_Domain.Core.DTOs;


namespace AnalysisCallUser._01_Domain.Core.Contracts
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardDataAsync(int userId);
        Task<AnalyticsDto> GetAnalyticsDataAsync(CallFilterDto filter);

        // نوع بازگشتی به NetworkDto تغییر کرد
        Task<NetworkDto> GetNetworkGraphDataAsync();

        // نوع بازگشتی به MapDto تغییر کرد
        Task<MapDto> GetGeographicDataAsync();
    }
}