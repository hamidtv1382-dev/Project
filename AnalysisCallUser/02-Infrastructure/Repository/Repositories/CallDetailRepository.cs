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

        // متد GetAll بدون تغییر باقی می‌ماند تا سایر بخش‌های برنامه دچار مشکل نشوند
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

        // متد GetFilteredAsync بدون تغییر باقی می‌ماند
        public async Task<IEnumerable<CallDetail>> GetFilteredAsync(CallFilterDto filter)
        {
            var query = GetAll()
                .Select(cd => new
                {
                    CallDetail = cd,
                    FullDateTime = cd.AccountingTime
                });

            if (filter != null)
            {
                if (filter.StartDate.HasValue)
                {
                    var startDateTime =
                        filter.StartDate.Value.Date +
                        (filter.StartTime ?? TimeSpan.Zero);

                    query = query.Where(x => x.CallDetail.AccountingTime >= startDateTime);
                }

                if (filter.EndDate.HasValue)
                {
                    var endDateTime =
                        filter.EndDate.Value.Date +
                        (filter.EndTime ?? new TimeSpan(23, 59, 59));

                    query = query.Where(x => x.FullDateTime <= endDateTime);
                }

                if (!string.IsNullOrEmpty(filter.ANumber))
                    query = query.Where(x => x.CallDetail.ANumber.Contains(filter.ANumber));

                if (!string.IsNullOrEmpty(filter.BNumber))
                    query = query.Where(x => x.CallDetail.BNumber.Contains(filter.BNumber));

                if (filter.OriginCountryID.HasValue)
                    query = query.Where(x => x.CallDetail.OriginCountryID == filter.OriginCountryID);

                if (filter.DestCountryID.HasValue)
                    query = query.Where(x => x.CallDetail.DestCountryID == filter.DestCountryID);

                if (filter.OriginCityID.HasValue)
                    query = query.Where(x => x.CallDetail.OriginCityID == filter.OriginCityID);

                if (filter.DestCityID.HasValue)
                    query = query.Where(x => x.CallDetail.DestCityID == filter.DestCityID);

                if (filter.OriginOperatorID.HasValue)
                    query = query.Where(x => x.CallDetail.OriginOperatorID == filter.OriginOperatorID);

                if (filter.DestOperatorID.HasValue)
                    query = query.Where(x => x.CallDetail.DestOperatorID == filter.DestOperatorID);

                if (filter.TypeID.HasValue)
                    query = query.Where(x => x.CallDetail.TypeID == filter.TypeID);

                if (filter.Answer.HasValue)
                    query = query.Where(x => x.CallDetail.Answer == filter.Answer);
            }

            return await query
                .OrderByDescending(x => x.FullDateTime)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(x => x.CallDetail)
                .ToListAsync();
        }

        // متد GetFilteredCountAsync بدون تغییر باقی می‌ماند
        public async Task<int> GetFilteredCountAsync(CallFilterDto filter)
        {
            var query = GetAll();

            if (filter != null)
            {
                if (filter.StartDate.HasValue)
                {
                    var startDateTime =
                        filter.StartDate.Value.Date +
                        (filter.StartTime ?? TimeSpan.Zero);

                    query = query.Where(cd => cd.AccountingTime >= startDateTime);
                }

                if (filter.EndDate.HasValue)
                {
                    var endDateTime =
                        filter.EndDate.Value.Date +
                        (filter.EndTime ?? new TimeSpan(23, 59, 59));

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
        // این متد جدید برای حل مشکل NullReferenceException اضافه می‌شود
        public async Task<CallDetail> GetByIdAsync(int id)
        {
            // برای این متد، فقط روابط مورد نیاز را Include می‌کنیم تا کوئری بهینه باشد
            return await _context.CallDetails
                .Include(cd => cd.OriginCountry)
                .Include(cd => cd.DestCountry)
                .FirstOrDefaultAsync(cd => cd.DetailID == id);
        }
    }
}