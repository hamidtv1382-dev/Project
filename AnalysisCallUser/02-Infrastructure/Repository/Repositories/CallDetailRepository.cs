using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._01_Domain.Core.Entities;
using AnalysisCallUser._02_Infrastructure.Data;
using AnalysisCallUser._02_Infrastructure.Repository.Base;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace AnalysisCallUser._02_Infrastructure.Repository.Repositories
{
    public class CallDetailRepository : Repository<CallDetail>, ICallDetailRepository
    {
        private readonly AppDbContext _context;
        public CallDetailRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public IQueryable<CallDetail> GetAll()
        {
            return _context.CallDetails.AsNoTracking();
        }

        public async Task<IEnumerable<CallDetail>> GetFilteredAsync(CallFilterDto filter)
        {
            var query = _context.CallDetails.AsNoTracking();

            if (filter != null)
            {
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

                // ✅ فیلتر شماره‌ها: OR بین A و B
                if ((filter.ANumbers != null && filter.ANumbers.Any(n => !string.IsNullOrWhiteSpace(n))) ||
                    (filter.BNumbers != null && filter.BNumbers.Any(n => !string.IsNullOrWhiteSpace(n))))
                {
                    var parameter = Expression.Parameter(typeof(CallDetail), "x");
                    Expression finalNumberFilter = null;

                    // A Numbers
                    if (filter.ANumbers != null)
                    {
                        var aProp = Expression.Property(parameter, nameof(CallDetail.ANumber));
                        foreach (var number in filter.ANumbers.Where(n => !string.IsNullOrWhiteSpace(n)))
                        {
                            var contains = Expression.Call(
                                aProp,
                                typeof(string).GetMethod("Contains", new[] { typeof(string) }),
                                Expression.Constant(number)
                            );

                            finalNumberFilter = finalNumberFilter == null
                                ? contains
                                : Expression.OrElse(finalNumberFilter, contains);
                        }
                    }

                    // B Numbers
                    if (filter.BNumbers != null)
                    {
                        var bProp = Expression.Property(parameter, nameof(CallDetail.BNumber));
                        foreach (var number in filter.BNumbers.Where(n => !string.IsNullOrWhiteSpace(n)))
                        {
                            var contains = Expression.Call(
                                bProp,
                                typeof(string).GetMethod("Contains", new[] { typeof(string) }),
                                Expression.Constant(number)
                            );

                            finalNumberFilter = finalNumberFilter == null
                                ? contains
                                : Expression.OrElse(finalNumberFilter, contains);
                        }
                    }

                    if (finalNumberFilter != null)
                    {
                        var lambda = Expression.Lambda<Func<CallDetail, bool>>(finalNumberFilter, parameter);
                        query = query.Where(lambda);
                    }
                }

                // سایر فیلترها
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

            query = query
                .Include(cd => cd.OriginCountry)
                .Include(cd => cd.OriginCity)
                .Include(cd => cd.OriginOperator)
                .Include(cd => cd.DestCountry)
                .Include(cd => cd.DestCity)
                .Include(cd => cd.DestOperator)
                .Include(cd => cd.CallType);

            return await query
                .OrderByDescending(x => x.AccountingTime)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();
        }


        public async Task<int> GetFilteredCountAsync(CallFilterDto filter)
        {
            var query = _context.CallDetails.AsNoTracking();

            if (filter != null)
            {
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

                // ✅ فیلتر شماره‌ها: OR بین A و B (هماهنگ با GetFilteredAsync)
                if ((filter.ANumbers != null && filter.ANumbers.Any(n => !string.IsNullOrWhiteSpace(n))) ||
                    (filter.BNumbers != null && filter.BNumbers.Any(n => !string.IsNullOrWhiteSpace(n))))
                {
                    var parameter = Expression.Parameter(typeof(CallDetail), "x");
                    Expression finalNumberFilter = null;

                    // A Numbers
                    if (filter.ANumbers != null)
                    {
                        var aProp = Expression.Property(parameter, nameof(CallDetail.ANumber));
                        foreach (var number in filter.ANumbers.Where(n => !string.IsNullOrWhiteSpace(n)))
                        {
                            var contains = Expression.Call(
                                aProp,
                                typeof(string).GetMethod("Contains", new[] { typeof(string) }),
                                Expression.Constant(number)
                            );

                            finalNumberFilter = finalNumberFilter == null
                                ? contains
                                : Expression.OrElse(finalNumberFilter, contains);
                        }
                    }

                    // B Numbers
                    if (filter.BNumbers != null)
                    {
                        var bProp = Expression.Property(parameter, nameof(CallDetail.BNumber));
                        foreach (var number in filter.BNumbers.Where(n => !string.IsNullOrWhiteSpace(n)))
                        {
                            var contains = Expression.Call(
                                bProp,
                                typeof(string).GetMethod("Contains", new[] { typeof(string) }),
                                Expression.Constant(number)
                            );

                            finalNumberFilter = finalNumberFilter == null
                                ? contains
                                : Expression.OrElse(finalNumberFilter, contains);
                        }
                    }

                    if (finalNumberFilter != null)
                    {
                        var lambda = Expression.Lambda<Func<CallDetail, bool>>(finalNumberFilter, parameter);
                        query = query.Where(lambda);
                    }
                }

                // سایر فیلترها
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
                    query = query.Where(x => x.Answer == filter.Answer.Value);
            }

            return await query.CountAsync();
        }

        public async Task<CallDetail> GetByIdAsync(int id)
        {
            return await _context.CallDetails
                .AsNoTracking()
                .Include(cd => cd.OriginCountry)
                .Include(cd => cd.DestCountry)
                .Include(cd => cd.OriginCity)
                .Include(cd => cd.DestCity)
                .Include(cd => cd.OriginOperator)
                .Include(cd => cd.DestOperator)
                .Include(cd => cd.CallType)
                .FirstOrDefaultAsync(cd => cd.DetailID == id);
        }
    }
}