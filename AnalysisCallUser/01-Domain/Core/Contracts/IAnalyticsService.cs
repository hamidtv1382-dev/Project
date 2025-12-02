
using AnalysisCallUser._01_Domain.Core.DTOs;

namespace AnalysisCallUser._01_Domain.Core.Contracts
{
    public interface IAnalyticsService
    {
        Task<ChartDataDto> GetCallVolumeOverTimeAsync(TimeAnalysisDto filter);
        Task<MapDataDto> GetGeographicDistributionAsync(GeographicAnalysisDto filter);
        Task<NetworkDto> GetCallNetworkAnalysisAsync(NetworkAnalysisDto filter);
        Task<OperatorPerformanceDto> GetOperatorPerformanceDataAsync();
    }
}