using AnalysisCallUser._01_Domain.Core.Entities;

namespace AnalysisCallUser._01_Domain.Core.Contracts
{
    public interface ICityRepository : IRepository<City>
    {
        Task<IEnumerable<City>> GetCitiesByCountryIdAsync(int countryId);
    }
}
