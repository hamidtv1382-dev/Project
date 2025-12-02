using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._01_Domain.Core.Entities;
using AnalysisCallUser._02_Infrastructure.Data;
using AnalysisCallUser._02_Infrastructure.Repository.Base;
using Microsoft.EntityFrameworkCore;

namespace AnalysisCallUser._02_Infrastructure.Repository.Repositories
{
    public class CityRepository : Repository<City>, ICityRepository
    {
        public CityRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<City>> GetCitiesByCountryIdAsync(int countryId)
        {
            return await _dbSet.Where(c => c.CountryID == countryId).ToListAsync();
        }
    }
}
