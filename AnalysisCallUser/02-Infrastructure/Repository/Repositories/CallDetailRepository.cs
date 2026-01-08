using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._01_Domain.Core.Entities;
using AnalysisCallUser._01_Domain.Core.Enums;
using AnalysisCallUser._02_Infrastructure.Data;
using AnalysisCallUser._02_Infrastructure.Repository.Base;
using AnalysisCallUser._03_EndPoint.Controllers;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        public IQueryable<CallDetail> GetAll()
        {
            return _context.CallDetails.AsNoTracking();
        }

        // --- تمام متدهای قبلی بدون تغییر هستند ---

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

                // --- منطق جدید: جستجوی لیست عمومی (شماره‌ها در مبدا یا مقصد) ---
                if (filter.GeneralNumbers != null && filter.GeneralNumbers.Any(n => !string.IsNullOrWhiteSpace(n)))
                {
                    var parameter = Expression.Parameter(typeof(CallDetail), "x");
                    Expression finalGeneralFilter = null;

                    var aProp = Expression.Property(parameter, nameof(CallDetail.ANumber));
                    var bProp = Expression.Property(parameter, nameof(CallDetail.BNumber));

                    foreach (var number in filter.GeneralNumbers.Where(n => !string.IsNullOrWhiteSpace(n)))
                    {
                        // شرط: شماره در A باشد
                        var equalsA = Expression.Call(
                            aProp,
                            typeof(string).GetMethod("Equals", new[] { typeof(string) }),
                            Expression.Constant(number)
                        );

                        // شرط: شماره در B باشد
                        var equalsB = Expression.Call(
                            bProp,
                            typeof(string).GetMethod("Equals", new[] { typeof(string) }),
                            Expression.Constant(number)
                        );

                        // ترکیب: A یا B (OR) برای هر شماره
                        var orCondition = Expression.OrElse(equalsA, equalsB);

                        // ترکیب با شماره‌های قبلی (OR)
                        finalGeneralFilter = finalGeneralFilter == null
                            ? orCondition
                            : Expression.OrElse(finalGeneralFilter, orCondition);
                    }

                    if (finalGeneralFilter != null)
                    {
                        var lambda = Expression.Lambda<Func<CallDetail, bool>>(finalGeneralFilter, parameter);
                        query = query.Where(lambda);
                    }
                }
                // ----------------------------------------------------

                // منطق جدید برای جستجوی عمیق
                if (filter.IsDeepSearch &&
                    ((filter.ANumbers != null && filter.ANumbers.Any(n => !string.IsNullOrWhiteSpace(n))) ||
                     (filter.BNumbers != null && filter.BNumbers.Any(n => !string.IsNullOrWhiteSpace(n)))))
                {
                    // حالت ۱: اگر هم شماره مبدأ و هم شماره مقصد وارد شده‌اند
                    if (filter.ANumbers != null && filter.ANumbers.Any(n => !string.IsNullOrWhiteSpace(n)) &&
                        filter.BNumbers != null && filter.BNumbers.Any(n => !string.IsNullOrWhiteSpace(n)))
                    {
                        // ایجاد شرط برای روابط مستقیم بین جفت شماره‌ها
                        var parameter = Expression.Parameter(typeof(CallDetail), "x");
                        Expression finalCondition = null;

                        // تولید تمام ترکیب‌های ممکن بین شماره‌های مبدأ و مقصد
                        foreach (var aNumber in filter.ANumbers.Where(n => !string.IsNullOrWhiteSpace(n)))
                        {
                            foreach (var bNumber in filter.BNumbers.Where(n => !string.IsNullOrWhiteSpace(n)))
                            {
                                // شرط برای تماس از A به B
                                var aToBCondition = Expression.AndAlso(
                                    Expression.Call(
                                        Expression.Property(parameter, nameof(CallDetail.ANumber)),
                                        typeof(string).GetMethod("Equals", new[] { typeof(string) }),
                                        Expression.Constant(aNumber)
                                    ),
                                    Expression.Call(
                                        Expression.Property(parameter, nameof(CallDetail.BNumber)),
                                        typeof(string).GetMethod("Equals", new[] { typeof(string) }),
                                        Expression.Constant(bNumber)
                                    )
                                );

                                // اگر جستجوی دوطرفه فعال باشد، شرط معکوس هم اضافه می‌شود
                                if (filter.BidirectionalSearch)
                                {
                                    var bToACondition = Expression.AndAlso(
                                        Expression.Call(
                                            Expression.Property(parameter, nameof(CallDetail.ANumber)),
                                            typeof(string).GetMethod("Equals", new[] { typeof(string) }),
                                            Expression.Constant(bNumber)
                                        ),
                                        Expression.Call(
                                            Expression.Property(parameter, nameof(CallDetail.BNumber)),
                                            typeof(string).GetMethod("Equals", new[] { typeof(string) }),
                                            Expression.Constant(aNumber)
                                        )
                                    );

                                    aToBCondition = Expression.OrElse(aToBCondition, bToACondition);
                                }

                                finalCondition = finalCondition == null
                                    ? aToBCondition
                                    : Expression.OrElse(finalCondition, aToBCondition);
                            }
                        }

                        if (finalCondition != null)
                        {
                            var lambda = Expression.Lambda<Func<CallDetail, bool>>(finalCondition, parameter);
                            query = query.Where(lambda);
                        }
                    }
                    // حالت ۲: اگر فقط یک نوع شماره وارد شده (تک شماره)
                    else
                    {
                        // منطق قبلی برای جستجوی عادی (OR بین A و B)
                        var parameter = Expression.Parameter(typeof(CallDetail), "x");
                        Expression finalNumberFilter = null;

                        // A Numbers
                        if (filter.ANumbers != null)
                        {
                            var aProp = Expression.Property(parameter, nameof(CallDetail.ANumber));
                            foreach (var number in filter.ANumbers.Where(n => !string.IsNullOrWhiteSpace(n)))
                            {
                                var equals = Expression.Call(
                                    aProp,
                                    typeof(string).GetMethod("Equals", new[] { typeof(string) }),
                                    Expression.Constant(number)
                                );

                                finalNumberFilter = finalNumberFilter == null
                                    ? equals
                                    : Expression.OrElse(finalNumberFilter, equals);
                            }
                        }

                        // B Numbers
                        if (filter.BNumbers != null)
                        {
                            var bProp = Expression.Property(parameter, nameof(CallDetail.BNumber));
                            foreach (var number in filter.BNumbers.Where(n => !string.IsNullOrWhiteSpace(n)))
                            {
                                var equals = Expression.Call(
                                    bProp,
                                    typeof(string).GetMethod("Equals", new[] { typeof(string) }),
                                    Expression.Constant(number)
                                );

                                finalNumberFilter = finalNumberFilter == null
                                    ? equals
                                    : Expression.OrElse(finalNumberFilter, equals);
                            }
                        }

                        if (finalNumberFilter != null)
                        {
                            var lambda = Expression.Lambda<Func<CallDetail, bool>>(finalNumberFilter, parameter);
                            query = query.Where(lambda);
                        }
                    }
                }
                // منطق قبلی برای جستجوی عادی
                else if ((filter.ANumbers != null && filter.ANumbers.Any(n => !string.IsNullOrWhiteSpace(n))) ||
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
                        // اصلاح شده: استفاده از Expression.Property به جای Parameter
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

                // --- منطق جدید برای Count: جستجوی لیست عمومی (شماره‌ها در مبدا یا مقصد) ---
                if (filter.GeneralNumbers != null && filter.GeneralNumbers.Any(n => !string.IsNullOrWhiteSpace(n)))
                {
                    var parameter = Expression.Parameter(typeof(CallDetail), "x");
                    Expression finalGeneralFilter = null;

                    var aProp = Expression.Property(parameter, nameof(CallDetail.ANumber));
                    var bProp = Expression.Property(parameter, nameof(CallDetail.BNumber));

                    foreach (var number in filter.GeneralNumbers.Where(n => !string.IsNullOrWhiteSpace(n)))
                    {
                        // شرط: شماره در A باشد
                        var equalsA = Expression.Call(
                            aProp,
                            typeof(string).GetMethod("Equals", new[] { typeof(string) }),
                            Expression.Constant(number)
                        );

                        // شرط: شماره در B باشد
                        var equalsB = Expression.Call(
                            bProp,
                            typeof(string).GetMethod("Equals", new[] { typeof(string) }),
                            Expression.Constant(number)
                        );

                        // ترکیب: A یا B (OR)
                        var orCondition = Expression.OrElse(equalsA, equalsB);

                        // ترکیب با شماره‌های قبلی (OR)
                        finalGeneralFilter = finalGeneralFilter == null
                            ? orCondition
                            : Expression.OrElse(finalGeneralFilter, orCondition);
                    }

                    if (finalGeneralFilter != null)
                    {
                        var lambda = Expression.Lambda<Func<CallDetail, bool>>(finalGeneralFilter, parameter);
                        query = query.Where(lambda);
                    }
                }
                // -------------------------------------------------------

                // منطق جدید برای جستجوی عمیق
                if (filter.IsDeepSearch &&
                    ((filter.ANumbers != null && filter.ANumbers.Any(n => !string.IsNullOrWhiteSpace(n))) ||
                     (filter.BNumbers != null && filter.BNumbers.Any(n => !string.IsNullOrWhiteSpace(n)))))
                {
                    // حالت ۱: اگر هم شماره مبدأ و هم شماره مقصد وارد شده‌اند
                    if (filter.ANumbers != null && filter.ANumbers.Any(n => !string.IsNullOrWhiteSpace(n)) &&
                        filter.BNumbers != null && filter.BNumbers.Any(n => !string.IsNullOrWhiteSpace(n)))
                    {
                        // ایجاد شرط برای روابط مستقیم بین جفت شماره‌ها
                        var parameter = Expression.Parameter(typeof(CallDetail), "x");
                        Expression finalCondition = null;

                        // تولید تمام ترکیب‌های ممکن بین شماره‌های مبدأ و مقصد
                        foreach (var aNumber in filter.ANumbers.Where(n => !string.IsNullOrWhiteSpace(n)))
                        {
                            foreach (var bNumber in filter.BNumbers.Where(n => !string.IsNullOrWhiteSpace(n)))
                            {
                                // شرط برای تماس از A به B
                                var aToBCondition = Expression.AndAlso(
                                    Expression.Call(
                                        Expression.Property(parameter, nameof(CallDetail.ANumber)),
                                        typeof(string).GetMethod("Equals", new[] { typeof(string) }),
                                        Expression.Constant(aNumber)
                                    ),
                                    Expression.Call(
                                        Expression.Property(parameter, nameof(CallDetail.BNumber)),
                                        typeof(string).GetMethod("Equals", new[] { typeof(string) }),
                                        Expression.Constant(bNumber)
                                    )
                                );

                                // اگر جستجوی دوطرفه فعال باشد، شرط معکوس هم اضافه می‌شود
                                if (filter.BidirectionalSearch)
                                {
                                    var bToACondition = Expression.AndAlso(
                                        Expression.Call(
                                            Expression.Property(parameter, nameof(CallDetail.ANumber)),
                                            typeof(string).GetMethod("Equals", new[] { typeof(string) }),
                                            Expression.Constant(bNumber)
                                        ),
                                        Expression.Call(
                                            Expression.Property(parameter, nameof(CallDetail.BNumber)),
                                            typeof(string).GetMethod("Equals", new[] { typeof(string) }),
                                            Expression.Constant(aNumber)
                                        )
                                    );

                                    aToBCondition = Expression.OrElse(aToBCondition, bToACondition);
                                }

                                finalCondition = finalCondition == null
                                    ? aToBCondition
                                    : Expression.OrElse(finalCondition, aToBCondition);
                            }
                        }

                        if (finalCondition != null)
                        {
                            var lambda = Expression.Lambda<Func<CallDetail, bool>>(finalCondition, parameter);
                            query = query.Where(lambda);
                        }
                    }
                    // حالت ۲: اگر فقط یک نوع شماره وارد شده (تک شماره)
                    else
                    {
                        // منطق قبلی برای جستجوی عادی
                        var parameter = Expression.Parameter(typeof(CallDetail), "x");
                        Expression finalNumberFilter = null;

                        // A Numbers
                        if (filter.ANumbers != null)
                        {
                            var aProp = Expression.Property(parameter, nameof(CallDetail.ANumber));
                            foreach (var number in filter.ANumbers.Where(n => !string.IsNullOrWhiteSpace(n)))
                            {
                                var equals = Expression.Call(
                                    aProp,
                                    typeof(string).GetMethod("Equals", new[] { typeof(string) }),
                                    Expression.Constant(number)
                                );

                                finalNumberFilter = finalNumberFilter == null
                                    ? equals
                                    : Expression.OrElse(finalNumberFilter, equals);
                            }
                        }

                        // B Numbers
                        if (filter.BNumbers != null)
                        {
                            var bProp = Expression.Property(parameter, nameof(CallDetail.BNumber));
                            foreach (var number in filter.BNumbers.Where(n => !string.IsNullOrWhiteSpace(n)))
                            {
                                var equals = Expression.Call(
                                    bProp,
                                    typeof(string).GetMethod("Equals", new[] { typeof(string) }),
                                    Expression.Constant(number)
                                );

                                finalNumberFilter = finalNumberFilter == null
                                    ? equals
                                    : Expression.OrElse(finalNumberFilter, equals);
                            }
                        }

                        if (finalNumberFilter != null)
                        {
                            var lambda = Expression.Lambda<Func<CallDetail, bool>>(finalNumberFilter, parameter);
                            query = query.Where(lambda);
                        }
                    }
                }
                // منطق قبلی برای جستجوی عادی
                else if ((filter.ANumbers != null && filter.ANumbers.Any(n => !string.IsNullOrWhiteSpace(n))) ||
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
                        // اصلاح شده: استفاده از Expression.Property به جای Parameter
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

        public async Task<List<CallDetail>> GetByIdsAsync(List<int> ids)
        {
            return await _context.CallDetails
                .AsNoTracking()
                .Include(cd => cd.OriginCountry)
                .Include(cd => cd.OriginCity)
                .Include(cd => cd.OriginOperator)
                .Include(cd => cd.DestCountry)
                .Include(cd => cd.DestCity)
                .Include(cd => cd.DestOperator)
                .Include(cd => cd.CallType)
                .Where(cd => ids.Contains(cd.DetailID))
                .OrderByDescending(cd => cd.AccountingTime)
                .ToListAsync();
        }

        public async Task<List<WeightedCallResult>> GetWeightedSearchAsync(WeightedSearchDto filter)
        {
            var query = _context.CallDetails.AsNoTracking();

            // فیلتر تاریخ
            if (filter.StartDate.HasValue)
            {
                query = query.Where(x => x.AccountingTime >= filter.StartDate.Value.Date);
            }

            if (filter.EndDate.HasValue)
            {
                var endDateTime = filter.EndDate.Value.Date.AddDays(1).AddSeconds(-1);
                query = query.Where(x => x.AccountingTime <= endDateTime);
            }

            // فقط تماس‌هایی که طول مکالمه بیشتر از 0 دارند
            query = query.Where(x => x.Length > 0);

            // فقط تماس‌های پاسخ داده شده (اگر درخواست شده)
            if (filter.IncludeAnsweredCallsOnly)
            {
                query = query.Where(x => x.Answer == CallAnswerStatus.Answered);
            }

            // تشخیص حالت جستجو
            var hasSourceNumbers = filter.ANumbers != null && filter.ANumbers.Any(n => !string.IsNullOrWhiteSpace(n));
            var hasDestNumbers = filter.BNumbers != null && filter.BNumbers.Any(n => !string.IsNullOrWhiteSpace(n));

            WeightedSearchMode actualMode = filter.SearchMode;

            if (filter.SearchMode == WeightedSearchMode.Auto)
            {
                if (hasSourceNumbers && hasDestNumbers)
                {
                    actualMode = WeightedSearchMode.SourceDestinationPairs;
                }
                else if (hasSourceNumbers)
                {
                    actualMode = WeightedSearchMode.SourceOnly;
                }
                else if (hasDestNumbers)
                {
                    actualMode = WeightedSearchMode.DestinationOnly;
                }
            }

            List<WeightedCallResult> results = new List<WeightedCallResult>();

            switch (actualMode)
            {
                case WeightedSearchMode.SourceOnly:
                    results = await ProcessSourceOnlySearch(query, filter);
                    break;

                case WeightedSearchMode.DestinationOnly:
                    results = await ProcessDestinationOnlySearch(query, filter);
                    break;

                case WeightedSearchMode.SourceDestinationPairs:
                    results = await ProcessSourceDestinationPairsSearch(query, filter);
                    break;
            }

            return results.Where(r => r.Weight >= filter.MinWeight)
                          .OrderByDescending(r => r.Weight)
                          .ToList();
        }

        private async Task<List<WeightedCallResult>> ProcessSourceOnlySearch(IQueryable<CallDetail> query, WeightedSearchDto filter)
        {
            var results = new List<WeightedCallResult>();

            foreach (var sourceNumber in filter.ANumbers.Where(n => !string.IsNullOrWhiteSpace(n)))
            {
                var calls = await query
                    .Where(x => x.ANumber == sourceNumber)
                    .GroupBy(x => x.BNumber)
                    .Select(g => new
                    {
                        BNumber = g.Key,
                        Weight = g.Count(),
                        TotalLength = g.Sum(x => x.Length)
                    })
                    .ToListAsync();

                foreach (var call in calls)
                {
                    results.Add(new WeightedCallResult
                    {
                        ANumber = sourceNumber,
                        BNumber = call.BNumber,
                        Weight = call.Weight,
                        TotalLength = call.TotalLength,
                        IsSourceSearch = true
                    });
                }
            }

            return results;
        }

        private async Task<List<WeightedCallResult>> ProcessDestinationOnlySearch(IQueryable<CallDetail> query, WeightedSearchDto filter)
        {
            var results = new List<WeightedCallResult>();

            foreach (var destNumber in filter.BNumbers.Where(n => !string.IsNullOrWhiteSpace(n)))
            {
                var calls = await query
                    .Where(x => x.BNumber == destNumber)
                    .GroupBy(x => x.ANumber)
                    .Select(g => new
                    {
                        ANumber = g.Key,
                        Weight = g.Count(),
                        TotalLength = g.Sum(x => x.Length)
                    })
                    .ToListAsync();

                foreach (var call in calls)
                {
                    results.Add(new WeightedCallResult
                    {
                        ANumber = call.ANumber,
                        BNumber = destNumber,
                        Weight = call.Weight,
                        TotalLength = call.TotalLength,
                        IsSourceSearch = false
                    });
                }
            }

            return results;
        }

        private async Task<List<WeightedCallResult>> ProcessSourceDestinationPairsSearch(IQueryable<CallDetail> query, WeightedSearchDto filter)
        {
            var results = new List<WeightedCallResult>();

            foreach (var sourceNumber in filter.ANumbers.Where(n => !string.IsNullOrWhiteSpace(n)))
            {
                foreach (var destNumber in filter.BNumbers.Where(n => !string.IsNullOrWhiteSpace(n)))
                {
                    var directCalls = await query
                        .Where(x => x.ANumber == sourceNumber && x.BNumber == destNumber)
                        .CountAsync();

                    int reverseCalls = 0;
                    if (filter.BidirectionalSearch)
                    {
                        reverseCalls = await query
                            .Where(x => x.ANumber == destNumber && x.BNumber == sourceNumber)
                            .CountAsync();
                    }

                    int totalWeight = directCalls + reverseCalls;

                    var directLength = await query
                        .Where(x => x.ANumber == sourceNumber && x.BNumber == destNumber)
                        .SumAsync(x => (int?)x.Length) ?? 0;

                    var reverseLength = 0;
                    if (filter.BidirectionalSearch)
                    {
                        reverseLength = await query
                            .Where(x => x.ANumber == destNumber && x.BNumber == sourceNumber)
                            .SumAsync(x => (int?)x.Length) ?? 0;
                    }

                    results.Add(new WeightedCallResult
                    {
                        ANumber = sourceNumber,
                        BNumber = destNumber,
                        Weight = totalWeight,
                        TotalLength = directLength + reverseLength,
                        DirectCalls = directCalls,
                        ReverseCalls = reverseCalls,
                        IsSourceSearch = true
                    });
                }
            }

            return results;
        }

        // --- متد جدید برای داشبورد (اصلاح شده و بدون خطا) ---
        public async Task<List<TypeBreakdownDto>> GetChartDataAsync(DateTime start, DateTime end)
        {
            // این کوئری از ایندکس ترکیبی (AccountingTime, TypeID, OriginCountryID) استفاده می‌کند
            var query = _context.CallDetails
                .AsNoTracking()
                .Where(x => x.AccountingTime >= start && x.AccountingTime <= end)
                .GroupBy(x => new { x.TypeID, TypeName = x.CallType != null ? x.CallType.TypeName : "ناشناس" })
                .Select(g => new
                {
                    g.Key.TypeID,
                    g.Key.TypeName,
                    TotalCount = g.Count(),
                    SuccessCount = g.Count(x => x.Answer == CallAnswerStatus.Answered),
                    FailCount = g.Count(x => x.Answer != CallAnswerStatus.Answered),

                    // محاسبه آمار کشورها به صورت لیست تو در تو
                    CountryStats = g
                        .GroupBy(x => new
                        {
                            CID = x.OriginCountryID,
                            CName = x.OriginCountry != null ? x.OriginCountry.CountryName : "ناشناس"
                        })
                        .Select(cg => new
                        {
                            CID = cg.Key.CID,
                            CName = cg.Key.CName,
                            TotalCount = cg.Count(),
                            SuccessCount = cg.Count(x => x.Answer == CallAnswerStatus.Answered),
                            FailCount = cg.Count(x => x.Answer != CallAnswerStatus.Answered)
                        })
                        .OrderByDescending(c => c.TotalCount)
                        .ToList()
                })
                .OrderByDescending(x => x.TotalCount);

            var rawData = await query.ToListAsync();

            // مپ کردن به DTO
            var result = rawData.Select(item => new TypeBreakdownDto
            {
                TypeID = item.TypeID,
                TypeName = item.TypeName,
                TotalCount = item.TotalCount,
                SuccessCount = item.SuccessCount,
                FailCount = item.FailCount,
                Countries = item.CountryStats.Select(c => new CountryBreakdownDto
                {
                    CountryID = c.CID,
                    CountryName = c.CName,
                    TotalCount = c.TotalCount,
                    SuccessCount = c.SuccessCount,
                    FailCount = c.FailCount
                }).ToList()
            }).ToList();

            return result;
        }

        // --- DTO Classes for Dashboard ---
        public class TypeBreakdownDto
        {
            public int TypeID { get; set; }
            public string TypeName { get; set; }
            public int TotalCount { get; set; }
            public int SuccessCount { get; set; }
            public int FailCount { get; set; }
            public List<CountryBreakdownDto> Countries { get; set; }
        }

        public class CountryBreakdownDto
        {
            public int CountryID { get; set; }
            public string CountryName { get; set; }
            public int TotalCount { get; set; }
            public int SuccessCount { get; set; }
            public int FailCount { get; set; }
        }
    }
}