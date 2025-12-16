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

                // فیلتر شماره‌های مبدأ
                if (filter.ANumbers != null && filter.ANumbers.Any())
                {
                    // ایجاد یک عبارت برای فیلتر کردن شماره‌های مبدأ
                    var parameter = Expression.Parameter(typeof(CallDetail), "x");
                    var aNumberProperty = Expression.Property(parameter, "ANumber");

                    Expression aNumberFilter = null;
                    foreach (var number in filter.ANumbers)
                    {
                        if (!string.IsNullOrEmpty(number))
                        {
                            var startsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
                            var constant = Expression.Constant(number, typeof(string));
                            var startsWithExpression = Expression.Call(aNumberProperty, startsWithMethod, constant);

                            aNumberFilter = aNumberFilter == null
                                ? startsWithExpression
                                : Expression.OrElse(aNumberFilter, startsWithExpression);
                        }
                    }

                    if (aNumberFilter != null)
                    {
                        var lambda = Expression.Lambda<Func<CallDetail, bool>>(aNumberFilter, parameter);
                        query = query.Where(lambda);
                    }
                }

                // فیلتر شماره‌های مقصد
                if (filter.BNumbers != null && filter.BNumbers.Any())
                {
                    // ایجاد یک عبارت برای فیلتر کردن شماره‌های مقصد
                    var parameter = Expression.Parameter(typeof(CallDetail), "x");
                    var bNumberProperty = Expression.Property(parameter, "BNumber");

                    Expression bNumberFilter = null;
                    foreach (var number in filter.BNumbers)
                    {
                        if (!string.IsNullOrEmpty(number))
                        {
                            var startsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
                            var constant = Expression.Constant(number, typeof(string));
                            var startsWithExpression = Expression.Call(bNumberProperty, startsWithMethod, constant);

                            bNumberFilter = bNumberFilter == null
                                ? startsWithExpression
                                : Expression.OrElse(bNumberFilter, startsWithExpression);
                        }
                    }

                    if (bNumberFilter != null)
                    {
                        var lambda = Expression.Lambda<Func<CallDetail, bool>>(bNumberFilter, parameter);
                        query = query.Where(lambda);
                    }
                }

                // فیلتر ترکیبی شماره‌ها
                if (filter.NumberPairs != null && filter.NumberPairs.Any())
                {
                    var parameter = Expression.Parameter(typeof(CallDetail), "x");
                    var aNumberProperty = Expression.Property(parameter, "ANumber");
                    var bNumberProperty = Expression.Property(parameter, "BNumber");

                    Expression numberPairFilter = null;
                    foreach (var pair in filter.NumberPairs)
                    {
                        if (pair.AIndex >= 0 && pair.AIndex < filter.ANumbers.Count &&
                            pair.BIndex >= 0 && pair.BIndex < filter.BNumbers.Count)
                        {
                            var aNumber = filter.ANumbers[pair.AIndex];
                            var bNumber = filter.BNumbers[pair.BIndex];

                            if (!string.IsNullOrEmpty(aNumber) && !string.IsNullOrEmpty(bNumber))
                            {
                                var aNumberConstant = Expression.Constant(aNumber, typeof(string));
                                var bNumberConstant = Expression.Constant(bNumber, typeof(string));

                                var aNumberEqual = Expression.Equal(aNumberProperty, aNumberConstant);
                                var bNumberEqual = Expression.Equal(bNumberProperty, bNumberConstant);
                                var pairExpression = Expression.AndAlso(aNumberEqual, bNumberEqual);

                                numberPairFilter = numberPairFilter == null
                                    ? pairExpression
                                    : Expression.OrElse(numberPairFilter, pairExpression);
                            }
                        }
                    }

                    if (numberPairFilter != null)
                    {
                        var lambda = Expression.Lambda<Func<CallDetail, bool>>(numberPairFilter, parameter);
                        query = query.Where(lambda);
                    }
                }

                // سایر فیلترها را اعمال کنید
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

        // متد GetFilteredCountAsync را بهینه می‌کنیم
        public async Task<int> GetFilteredCountAsync(CallFilterDto filter)
        {
            var query = _context.CallDetails.AsNoTracking();

            if (filter != null)
            {
                // فیلترهای تاریخ را در اولویت قرار دهید
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

                // فیلتر شماره‌های مبدأ
                if (filter.ANumbers != null && filter.ANumbers.Any())
                {
                    // ایجاد یک عبارت برای فیلتر کردن شماره‌های مبدأ
                    var parameter = Expression.Parameter(typeof(CallDetail), "x");
                    var aNumberProperty = Expression.Property(parameter, "ANumber");

                    Expression aNumberFilter = null;
                    foreach (var number in filter.ANumbers)
                    {
                        if (!string.IsNullOrEmpty(number))
                        {
                            var startsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
                            var constant = Expression.Constant(number, typeof(string));
                            var startsWithExpression = Expression.Call(aNumberProperty, startsWithMethod, constant);

                            aNumberFilter = aNumberFilter == null
                                ? startsWithExpression
                                : Expression.OrElse(aNumberFilter, startsWithExpression);
                        }
                    }

                    if (aNumberFilter != null)
                    {
                        var lambda = Expression.Lambda<Func<CallDetail, bool>>(aNumberFilter, parameter);
                        query = query.Where(lambda);
                    }
                }

                // فیلتر شماره‌های مقصد
                if (filter.BNumbers != null && filter.BNumbers.Any())
                {
                    // ایجاد یک عبارت برای فیلتر کردن شماره‌های مقصد
                    var parameter = Expression.Parameter(typeof(CallDetail), "x");
                    var bNumberProperty = Expression.Property(parameter, "BNumber");

                    Expression bNumberFilter = null;
                    foreach (var number in filter.BNumbers)
                    {
                        if (!string.IsNullOrEmpty(number))
                        {
                            var startsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
                            var constant = Expression.Constant(number, typeof(string));
                            var startsWithExpression = Expression.Call(bNumberProperty, startsWithMethod, constant);

                            bNumberFilter = bNumberFilter == null
                                ? startsWithExpression
                                : Expression.OrElse(bNumberFilter, startsWithExpression);
                        }
                    }

                    if (bNumberFilter != null)
                    {
                        var lambda = Expression.Lambda<Func<CallDetail, bool>>(bNumberFilter, parameter);
                        query = query.Where(lambda);
                    }
                }

                // فیلتر ترکیبی شماره‌ها
                if (filter.NumberPairs != null && filter.NumberPairs.Any())
                {
                    var parameter = Expression.Parameter(typeof(CallDetail), "x");
                    var aNumberProperty = Expression.Property(parameter, "ANumber");
                    var bNumberProperty = Expression.Property(parameter, "BNumber");

                    Expression numberPairFilter = null;
                    foreach (var pair in filter.NumberPairs)
                    {
                        if (pair.AIndex >= 0 && pair.AIndex < filter.ANumbers.Count &&
                            pair.BIndex >= 0 && pair.BIndex < filter.BNumbers.Count)
                        {
                            var aNumber = filter.ANumbers[pair.AIndex];
                            var bNumber = filter.BNumbers[pair.BIndex];

                            if (!string.IsNullOrEmpty(aNumber) && !string.IsNullOrEmpty(bNumber))
                            {
                                var aNumberConstant = Expression.Constant(aNumber, typeof(string));
                                var bNumberConstant = Expression.Constant(bNumber, typeof(string));

                                var aNumberEqual = Expression.Equal(aNumberProperty, aNumberConstant);
                                var bNumberEqual = Expression.Equal(bNumberProperty, bNumberConstant);
                                var pairExpression = Expression.AndAlso(aNumberEqual, bNumberEqual);

                                numberPairFilter = numberPairFilter == null
                                    ? pairExpression
                                    : Expression.OrElse(numberPairFilter, pairExpression);
                            }
                        }
                    }

                    if (numberPairFilter != null)
                    {
                        var lambda = Expression.Lambda<Func<CallDetail, bool>>(numberPairFilter, parameter);
                        query = query.Where(lambda);
                    }
                }

                // سایر فیلترها را اعمال کنید
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
                .Include(cd => cd.OriginCity)
                .Include(cd => cd.DestCity)
                .Include(cd => cd.OriginOperator)
                .Include(cd => cd.DestOperator)
                .Include(cd => cd.CallType)
                .FirstOrDefaultAsync(cd => cd.DetailID == id);
        }
    }
}