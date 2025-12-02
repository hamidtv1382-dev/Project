using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._01_Domain.Core.Entities;
using AnalysisCallUser._02_Infrastructure.Data;
using AnalysisCallUser._02_Infrastructure.Repository.Base;

namespace AnalysisCallUser._02_Infrastructure.Repository.Repositories
{
    public class CountryRepository : Repository<Country>, ICountryRepository
    {
        public CountryRepository(AppDbContext context) : base(context)
        {

        }
       
        // پیاده‌سازی متد GetAll
        public IQueryable<Country> GetAll()
        {
            return _context.Countries;
        }
    }
}
