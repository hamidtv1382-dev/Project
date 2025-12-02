using AnalysisCallUser._01_Domain.Core.Entities;

namespace AnalysisCallUser._01_Domain.Core.Contracts
{
    public interface ICountryRepository : IRepository<Country>
    {
        IQueryable<Country> GetAll();

    }
}
