using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._01_Domain.Core.Entities;

namespace AnalysisCallUser._01_Domain.Core.Contracts
{
    public interface ICallAnalysisService
    {
        // تغییر نوع بازگشتی از Dictionary به ChartDataDto
        Task<ChartDataDto> GetTopCallingCountriesAsync(int count = 10);

        // تغییر نوع بازگشتی از Dictionary به ChartDataDto
        Task<ChartDataDto> GetAnsweredVsNotAnsweredCallsAsync();

        // تغییر نوع بازگشتی از Dictionary به OperatorPerformanceDto
        Task<OperatorPerformanceDto> GetAverageCallDurationByOperatorAsync();

        // تغییر نوع بازگشتی از IEnumerable<CallDetail> به IEnumerable<CallDetailDto>
        Task<IEnumerable<CallDetailDto>> GetLongestCallsAsync(int count = 10);
    }
}
