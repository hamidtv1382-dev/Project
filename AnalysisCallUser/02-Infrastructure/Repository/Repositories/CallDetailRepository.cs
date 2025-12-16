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
        private readonly AppDbContext _context;
        public CallDetailRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        // متد GetAll را با AsNoTracking بازنویسی می‌کنیم
        // این متد دیگر شامل Includeها نیست تا بتوانیم در متدهای دیگر به صورت داینامیک Includeها را اعمال کنیم
        public IQueryable<CallDetail> GetAll()
        {
            return _context.CallDetails.AsNoTracking();
        }

        // متد GetFilteredAsync را بهینه می‌کنیم
        public async Task<IEnumerable<CallDetail>> GetFilteredAsync(CallFilterDto filter)
        {
            // ابتدا کوئری پایه را بدون Include می‌گیریم
            var query = _context.CallDetails.AsNoTracking();

            // ابتدا فیلترهای اصلی را اعمال می‌کنیم
            if (filter != null)
            {
                // فیلترهای تاریخ را در اولویت قرار دهید
                if (filter.StartDate.HasValue)
                {
                    var startDateTime = filter.StartDate.Value.Date + (filter.StartTime ?? TimeSpan.Zero);
                    query = query.Where(x => x.AccountingTime >= startDateTime);
                }

                if (filter.EndDate.HasValue)
                {
                    var endDateTime = filter.EndDate.Value.Date + (filter.EndTime ?? new TimeSpan(23, 59, 59));
                    query = query.Where(x => x.AccountingTime <= endDateTime);
                }

                // سپس سایر فیلترها را اعمال کنید
                if (!string.IsNullOrEmpty(filter.ANumber))
                    query = query.Where(x => x.ANumber.StartsWith(filter.ANumber));

                if (!string.IsNullOrEmpty(filter.BNumber))
                    query = query.Where(x => x.BNumber.StartsWith(filter.BNumber));

                if (filter.OriginCountryID.HasValue)
                    query = query.Where(x => x.OriginCountryID == filter.OriginCountryID);

                if (filter.DestCountryID.HasValue)
                    query = query.Where(x => x.DestCountryID == filter.DestCountryID);

                if (filter.OriginCityID.HasValue)
                    query = query.Where(x => x.OriginCityID == filter.OriginCityID);

                if (filter.DestCityID.HasValue)
                    query = query.Where(x => x.DestCityID == filter.DestCityID);

                if (filter.OriginOperatorID.HasValue)
                    query = query.Where(x => x.OriginOperatorID == filter.OriginOperatorID);

                if (filter.DestOperatorID.HasValue)
                    query = query.Where(x => x.DestOperatorID == filter.DestOperatorID);

                if (filter.TypeID.HasValue)
                    query = query.Where(x => x.TypeID == filter.TypeID);

                if (filter.Answer.HasValue)
                    query = query.Where(x => x.Answer == filter.Answer);
            }

            // ابتدا تعداد کل رکوردها را برای صفحه‌بندی محاسبه کنید
            var totalCount = await query.CountAsync();

            // حالا Includeها را اضافه کنید
            query = query
                .Include(cd => cd.OriginCountry)
                .Include(cd => cd.OriginCity)
                .Include(cd => cd.OriginOperator)
                .Include(cd => cd.DestCountry)
                .Include(cd => cd.DestCity)
                .Include(cd => cd.DestOperator)
                .Include(cd => cd.CallType);

            // در نهایت، مرتب‌سازی و صفحه‌بندی را اعمال کنید
            return await query
                .OrderByDescending(x => x.AccountingTime)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();
        }

        // متد GetFilteredCountAsync بدون تغییر باقی می‌ماند، چون بهینه است
        public async Task<int> GetFilteredCountAsync(CallFilterDto filter)
        {
            var query = _context.CallDetails.AsNoTracking();

            if (filter != null)
            {
                if (filter.StartDate.HasValue)
                {
                    var startDateTime = filter.StartDate.Value.Date + (filter.StartTime ?? TimeSpan.Zero);
                    query = query.Where(cd => cd.AccountingTime >= startDateTime);
                }

                if (filter.EndDate.HasValue)
                {
                    var endDateTime = filter.EndDate.Value.Date + (filter.EndTime ?? new TimeSpan(23, 59, 59));
                    query = query.Where(cd => cd.AccountingTime <= endDateTime);
                }

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

        // متد GetByIdAsync را نیز برای یکپارچگی با AsNoTracking اصلاح می‌کنیم
        public async Task<CallDetail> GetByIdAsync(int id)
        {
            return await _context.CallDetails
                .AsNoTracking()
                .Include(cd => cd.OriginCountry)
                .Include(cd => cd.DestCountry)
                .FirstOrDefaultAsync(cd => cd.DetailID == id);
        }
    }
}