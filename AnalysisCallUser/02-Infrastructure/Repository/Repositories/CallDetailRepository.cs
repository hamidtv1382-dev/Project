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
                    // اگر هم مبدأ و هم مقصد وارد شده، فقط بین آنها جستجو کند
                    actualMode = WeightedSearchMode.SourceDestinationPairs;
                }
                else if (hasSourceNumbers)
                {
                    // اگر فقط مبدأ وارد شده، در کل دیتابیس برای مبدأ جستجو کند
                    actualMode = WeightedSearchMode.SourceOnly;
                }
                else if (hasDestNumbers)
                {
                    // اگر فقط مقصد وارد شده، در کل دیتابیس برای مقصد جستجو کند
                    actualMode = WeightedSearchMode.DestinationOnly;
                }
            }

            List<WeightedCallResult> results = new List<WeightedCallResult>();

            switch (actualMode)
            {
                case WeightedSearchMode.SourceOnly:
                    // جستجو برای شماره‌های مبدأ در کل دیتابیس
                    results = await ProcessSourceOnlySearch(query, filter);
                    break;

                case WeightedSearchMode.DestinationOnly:
                    // جستجو برای شماره‌های مقصد در کل دیتابیس
                    results = await ProcessDestinationOnlySearch(query, filter);
                    break;

                case WeightedSearchMode.SourceDestinationPairs:
                    // جستجو فقط بین جفت‌های وارد شده
                    results = await ProcessSourceDestinationPairsSearch(query, filter);
                    break;
            }

            // فیلتر بر اساس حداقل وزن
            return results.Where(r => r.Weight >= filter.MinWeight)
                          .OrderByDescending(r => r.Weight)
                          .ToList();
        }

        private async Task<List<WeightedCallResult>> ProcessSourceOnlySearch(IQueryable<CallDetail> query, WeightedSearchDto filter)
        {
            var results = new List<WeightedCallResult>();

            foreach (var sourceNumber in filter.ANumbers.Where(n => !string.IsNullOrWhiteSpace(n)))
            {
                // پیدا کردن تمام تماس‌های این شماره مبدأ
                // تغییر: فیلتر MinWeight را حذف کردیم تا همه تماس‌ها (حتی با وزن کم) بیایند
                var calls = await query
                    .Where(x => x.ANumber == sourceNumber)
                    .GroupBy(x => x.BNumber)
                    .Select(g => new
                    {
                        BNumber = g.Key,
                        Weight = g.Count(),
                        TotalLength = g.Sum(x => x.Length)
                    })
                    .ToListAsync(); // فیلتر MinWeight بعداً در متد اصلی اعمال می‌شود

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
                // تغییر: فیلتر MinWeight را حذف کردیم
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

            // ایجاد تمام ترکیب‌های ممکن بین مبدأ و مقصد
            foreach (var sourceNumber in filter.ANumbers.Where(n => !string.IsNullOrWhiteSpace(n)))
            {
                foreach (var destNumber in filter.BNumbers.Where(n => !string.IsNullOrWhiteSpace(n)))
                {
                    // شمارش تماس‌های مستقیم
                    var directCalls = await query
                        .Where(x => x.ANumber == sourceNumber && x.BNumber == destNumber)
                        .CountAsync();

                    // اگر جستجوی دوطرفه فعال باشد، تماس‌های معکوس هم محاسبه شود
                    int reverseCalls = 0;
                    if (filter.BidirectionalSearch)
                    {
                        reverseCalls = await query
                            .Where(x => x.ANumber == destNumber && x.BNumber == sourceNumber)
                            .CountAsync();
                    }

                    int totalWeight = directCalls + reverseCalls;

                    // تغییر: شرط MinWeight را اینجا هم حذف کردیم تا محاسبات دقیق انجام شود
                    // فیلتر نهایی در متد GetWeightedSearchAsync انجام می‌شود

                    // محاسبه طول کل مکالمه
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
    }
}