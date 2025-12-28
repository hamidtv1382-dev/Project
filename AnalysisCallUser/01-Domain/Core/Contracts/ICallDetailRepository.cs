using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._01_Domain.Core.Entities;
using AnalysisCallUser._03_EndPoint.Controllers;
using AnalysisCallUser._03_EndPoint.Models.ViewModels.Call;

namespace AnalysisCallUser._01_Domain.Core.Contracts
{
    public interface ICallDetailRepository : IRepository<CallDetail>
    {
        Task<IEnumerable<CallDetail>> GetFilteredAsync(CallFilterDto filter);
        Task<int> GetFilteredCountAsync(CallFilterDto filter);
        Task<List<WeightedCallResult>> GetWeightedSearchAsync(WeightedSearchDto filter);

        IQueryable<CallDetail> GetAll();
        Task<List<CallDetail>> GetByIdsAsync(List<int> ids);

    }
}