using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._01_Domain.Core.Entities;
using AnalysisCallUser._03_EndPoint.Models.ViewModels.Call;

namespace AnalysisCallUser._01_Domain.Core.Contracts
{
    public interface ICallDetailRepository : IRepository<CallDetail>
    {
        Task<IEnumerable<CallDetail>> GetFilteredAsync(CallFilterDto filter);
        Task<int> GetFilteredCountAsync(CallFilterDto filter);
        IQueryable<CallDetail> GetAll();

    }
}
