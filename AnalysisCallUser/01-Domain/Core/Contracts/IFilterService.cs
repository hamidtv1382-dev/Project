using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._01_Domain.Core.Entities;

namespace AnalysisCallUser._01_Domain.Core.Contracts
{
    public interface IFilterService
    {
        Task SaveFilterAsync(FilterHistory filter);
        Task<IEnumerable<FilterHistory>> GetUserFilterHistoryAsync(int userId);
        Task<CallFilterDto> GetDefaultFilterAsync();
    }
}