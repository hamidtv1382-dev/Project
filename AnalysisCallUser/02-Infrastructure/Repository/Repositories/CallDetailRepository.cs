using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._01_Domain.Core.Entities;
using AnalysisCallUser._02_Infrastructure.Data;
using AnalysisCallUser._02_Infrastructure.Repository.Base;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnalysisCallUser._02_Infrastructure.Repository.Repositories
{
    public class CallDetailRepository : Repository<CallDetail>, ICallDetailRepository
    {
        public CallDetailRepository(AppDbContext context) : base(context)
        {
        }

        // پیاده‌سازی متد GetAll
        public IQueryable<CallDetail> GetAll()
        {
            return _context.CallDetails
                .Include(cd => cd.OriginCountry)
                .Include(cd => cd.OriginCity)
                .Include(cd => cd.OriginOperator)
                .Include(cd => cd.DestCountry)
                .Include(cd => cd.DestCity)
                .Include(cd => cd.DestOperator)
                .Include(cd => cd.CallType);
        }

        public async Task<IEnumerable<CallDetail>> GetFilteredAsync(CallFilterDto filter)
        {
            var query = GetAll(); // استفاده از متد GetAll برای خوانایی بیشتر

            if (filter != null)
            {
                if (filter.StartDate.HasValue)
                    query = query.Where(cd => cd.AccountingTime_Date >= filter.StartDate.Value);

                if (filter.EndDate.HasValue)
                    query = query.Where(cd => cd.AccountingTime_Date <= filter.EndDate.Value);

                if (filter.StartTime.HasValue)
                    query = query.Where(cd => cd.AccountingTime_Time >= filter.StartTime.Value);

                if (filter.EndTime.HasValue)
                    query = query.Where(cd => cd.AccountingTime_Time <= filter.EndTime.Value);

                if (!string.IsNullOrEmpty(filter.ANumber))
                    query = query.Where(cd => cd.ANumber.Contains(filter.ANumber));

                if (!string.IsNullOrEmpty(filter.BNumber))
                    query = query.Where(cd => cd.BNumber.Contains(filter.BNumber));

                if (filter.OriginCountryID.HasValue)
                    query = query.Where(cd => cd.OriginCountryID == filter.OriginCountryID);

                if (filter.DestCountryID.HasValue)
                    query = query.Where(cd => cd.DestCountryID == filter.DestCountryID);

                if (filter.OriginCityID.HasValue)
                    query = query.Where(cd => cd.OriginCityID == filter.OriginCityID);

                if (filter.DestCityID.HasValue)
                    query = query.Where(cd => cd.DestCityID == filter.DestCityID);

                if (filter.OriginOperatorID.HasValue)
                    query = query.Where(cd => cd.OriginOperatorID == filter.OriginOperatorID);

                if (filter.DestOperatorID.HasValue)
                    query = query.Where(cd => cd.DestOperatorID == filter.DestOperatorID);

                if (filter.TypeID.HasValue)
                    query = query.Where(cd => cd.TypeID == filter.TypeID);

                if (filter.Answer.HasValue)
                    query = query.Where(cd => cd.Answer == filter.Answer.Value);
            }

            return await query
                .OrderByDescending(cd => cd.AccountingTime_Date)
                .ThenByDescending(cd => cd.AccountingTime_Time)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();
        }

        public async Task<int> GetFilteredCountAsync(CallFilterDto filter)
        {
            var query = GetAll(); // استفاده از متد GetAll برای خوانایی بیشتر

            if (filter != null)
            {
                if (filter.StartDate.HasValue)
                    query = query.Where(cd => cd.AccountingTime_Date >= filter.StartDate.Value);

                if (filter.EndDate.HasValue)
                    query = query.Where(cd => cd.AccountingTime_Date <= filter.EndDate.Value);

                if (filter.StartTime.HasValue)
                    query = query.Where(cd => cd.AccountingTime_Time >= filter.StartTime.Value);

                if (filter.EndTime.HasValue)
                    query = query.Where(cd => cd.AccountingTime_Time <= filter.EndTime.Value);

                if (!string.IsNullOrEmpty(filter.ANumber))
                    query = query.Where(cd => cd.ANumber.Contains(filter.ANumber));

                if (!string.IsNullOrEmpty(filter.BNumber))
                    query = query.Where(cd => cd.BNumber.Contains(filter.BNumber));

                if (filter.OriginCountryID.HasValue)
                    query = query.Where(cd => cd.OriginCountryID == filter.OriginCountryID);

                if (filter.DestCountryID.HasValue)
                    query = query.Where(cd => cd.DestCountryID == filter.DestCountryID);

                if (filter.OriginCityID.HasValue)
                    query = query.Where(cd => cd.OriginCityID == filter.OriginCityID);

                if (filter.DestCityID.HasValue)
                    query = query.Where(cd => cd.DestCityID == filter.DestCityID);

                if (filter.OriginOperatorID.HasValue)
                    query = query.Where(cd => cd.OriginOperatorID == filter.OriginOperatorID);

                if (filter.DestOperatorID.HasValue)
                    query = query.Where(cd => cd.DestOperatorID == filter.DestOperatorID);

                if (filter.TypeID.HasValue)
                    query = query.Where(cd => cd.TypeID == filter.TypeID);

                if (filter.Answer.HasValue)
                    query = query.Where(cd => cd.Answer == filter.Answer.Value);
            }

            return await query.CountAsync();
        }
    }
}