using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._01_Domain.Core.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalysisCallUser._03_EndPoint.Controllers
{
    public class AnalysisOverviewController : Controller
    {
        private readonly ICallDetailRepository _callDetailRepository;
        private readonly IServiceProvider _serviceProvider;
        private readonly PersianCalendar _persianCalendar = new();

        public AnalysisOverviewController(
            ICallDetailRepository callDetailRepository,
            IServiceProvider serviceProvider)
        {
            _callDetailRepository = callDetailRepository;
            _serviceProvider = serviceProvider;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetChartData(
            [FromQuery] string startDateStr,
            [FromQuery] string endDateStr,
            [FromQuery] int? year = null,
            [FromQuery] int? month = null,
            [FromQuery] int maxItemsPerGroup = 20)
        {
            try
            {
                // 1. تعیین بازه زمانی
                DateTime start, end;
                GetDateRange(startDateStr, endDateStr, year, month, out start, out end);

                // 2. اجرای کوئری‌ها با DbContextهای جداگانه برای موازی‌سازی
                using var scope1 = _serviceProvider.CreateScope();
                using var scope2 = _serviceProvider.CreateScope();

                var dailyStatsTask = GetDailyStatisticsAsync(scope1.ServiceProvider, start, end);
                var aggregatedStatsTask = GetAggregatedStatisticsAsync(scope2.ServiceProvider, start, end, maxItemsPerGroup);

                var dailyResult = await dailyStatsTask;
                var aggregatedResult = await aggregatedStatsTask;

                // 3. پردازش داده‌های روزانه
                var chartData = GenerateChartData(start, end, dailyResult.dailyData);
                int totalCalls = aggregatedResult.totalSuccess + aggregatedResult.totalFail;
                double successRate = totalCalls > 0 ? (double)aggregatedResult.totalSuccess / totalCalls * 100 : 0;

                return Json(new
                {
                    success = true,
                    data = chartData,
                    statistics = new
                    {
                        totalCalls,
                        totalSuccess = aggregatedResult.totalSuccess,
                        totalFail = aggregatedResult.totalFail,
                        successRate = Math.Round(successRate, 2)
                    },
                    dateRange = new
                    {
                        start = start.ToString("yyyy/MM/dd"),
                        end = end.ToString("yyyy/MM/dd"),
                        persianStart = ConvertToPersianDate(start),
                        persianEnd = ConvertToPersianDate(end)
                    },
                    typeBreakdown = aggregatedResult.typeBreakdown
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطای سیستمی: " + ex.Message });
            }
        }

        // --- Action جدید برای خروجی CSV عمومی ---
        [HttpPost]
        public IActionResult ExportToCSV([FromBody] List<object> data)
        {
            if (data == null || !data.Any()) return BadRequest("داده‌ای برای خروجی وجود ندارد.");

            var csv = new StringBuilder();
            var properties = data[0].GetType().GetProperties();

            // هدر (نام ستون‌ها)
            csv.AppendLine(string.Join(",", properties.Select(p => p.Name)));

            // ردیف‌ها
            foreach (var item in data)
            {
                var values = properties.Select(p =>
                {
                    var val = p.GetValue(item, null);
                    // مدیریت کاما در متن
                    var strVal = val?.ToString().Replace(",", " ") ?? "";
                    return $"\"{strVal}\"";
                });
                csv.AppendLine(string.Join(",", values));
            }

            // UTF-8 با BOM برای پشتیبانی از فارسی در اکسل
            byte[] buffer = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();

            return File(buffer, "text/csv", "export_data.csv");
        }

        // --- Helper Methods (بدون تغییر) ---

        // متد GetDateRange باید public باشد تا از بیرون قابل دسترسی باشد یا اینجا کپی شود
        // در اینجا فرض بر این است که همان متد private وجود دارد و ما یک نسخه استفاده می‌کنیم.
        private void GetDateRange(string startDateStr, string endDateStr, int? year, int? month, out DateTime start, out DateTime end)
        {
            if (year.HasValue && month.HasValue && month.Value >= 1 && month.Value <= 12)
            {
                (start, end) = GetPersianMonthDateRange(year.Value, month.Value);
            }
            else if (year.HasValue)
            {
                (start, end) = GetPersianYearDateRange(year.Value);
            }
            else if (!string.IsNullOrEmpty(startDateStr) && !string.IsNullOrEmpty(endDateStr))
            {
                start = ParsePersianDate(startDateStr);
                end = ParsePersianDate(endDateStr).AddDays(1).AddSeconds(-1);
                if (start > end) (start, end) = (end, start);
            }
            else
            {
                int currentYear = _persianCalendar.GetYear(DateTime.Now);
                (start, end) = GetPersianYearDateRange(currentYear);
            }
        }

        private async Task<(Dictionary<DateTime, (int Success, int Fail)> dailyData, int totalSuccess, int totalFail)> GetDailyStatisticsAsync(IServiceProvider serviceProvider, DateTime start, DateTime end)
        {
            var repository = serviceProvider.GetRequiredService<ICallDetailRepository>();

            var query = repository.GetAll()
                .Where(x => x.AccountingTime >= start && x.AccountingTime <= end)
                .GroupBy(x => new { Date = x.AccountingTime.Date, IsSuccess = x.Answer == CallAnswerStatus.Answered })
                .Select(g => new { g.Key.Date, g.Key.IsSuccess, Count = g.Count() });

            var data = await query.AsNoTracking().ToListAsync();

            var dailyStats = new Dictionary<DateTime, (int Success, int Fail)>();
            int totalSuccess = 0, totalFail = 0;

            foreach (var item in data)
            {
                if (item.IsSuccess) totalSuccess += item.Count; else totalFail += item.Count;

                if (!dailyStats.ContainsKey(item.Date)) dailyStats[item.Date] = (0, 0);

                var current = dailyStats[item.Date];
                if (item.IsSuccess) dailyStats[item.Date] = (current.Success + item.Count, current.Fail);
                else dailyStats[item.Date] = (current.Success, current.Fail + item.Count);
            }

            return (dailyStats, totalSuccess, totalFail);
        }

        private async Task<(List<TypeBreakdownDto> typeBreakdown, int totalSuccess, int totalFail)> GetAggregatedStatisticsAsync(IServiceProvider serviceProvider, DateTime start, DateTime end, int maxItems)
        {
            var repository = serviceProvider.GetRequiredService<ICallDetailRepository>();

            var summaryQuery = repository.GetAll()
                .Where(x => x.AccountingTime >= start && x.AccountingTime <= end)
                .GroupBy(x => 1)
                .Select(g => new { TotalCount = g.Count(), SuccessCount = g.Count(x => x.Answer == CallAnswerStatus.Answered), FailCount = g.Count(x => x.Answer != CallAnswerStatus.Answered) });

            var summary = await summaryQuery.AsNoTracking().FirstOrDefaultAsync();
            if (summary == null) return (new List<TypeBreakdownDto>(), 0, 0);

            var typeBreakdown = await GetHierarchicalStatisticsAsync(start, end, maxItems);
            return (typeBreakdown, summary.SuccessCount, summary.FailCount);
        }

        private async Task<List<TypeBreakdownDto>> GetHierarchicalStatisticsAsync(DateTime start, DateTime end, int maxItems)
        {
            var typeData = await GetTypeDataAsync(start, end, maxItems * 2);
            var typeBreakdown = new List<TypeBreakdownDto>();

            foreach (var typeInfo in typeData)
            {
                var countryData = await GetCountryDataForTypeAsync(start, end, typeInfo.TypeID, maxItems);
                typeBreakdown.Add(new TypeBreakdownDto
                {
                    TypeID = typeInfo.TypeID,
                    TypeName = typeInfo.TypeName,
                    TotalCount = typeInfo.TotalCount,
                    SuccessCount = typeInfo.SuccessCount,
                    FailCount = typeInfo.FailCount,
                    Countries = countryData
                });
            }
            return typeBreakdown.OrderByDescending(x => x.TotalCount).ToList();
        }

        private async Task<List<TypeInfo>> GetTypeDataAsync(DateTime start, DateTime end, int maxItems)
        {
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ICallDetailRepository>();

            return await repository.GetAll()
                .Where(x => x.AccountingTime >= start && x.AccountingTime <= end)
                .GroupBy(x => new { x.TypeID, TypeName = x.CallType.TypeName })
                .Select(g => new TypeInfo
                {
                    TypeID = g.Key.TypeID,
                    TypeName = g.Key.TypeName,
                    TotalCount = g.Count(),
                    SuccessCount = g.Count(x => x.Answer == CallAnswerStatus.Answered),
                    FailCount = g.Count(x => x.Answer != CallAnswerStatus.Answered)
                })
                .OrderByDescending(x => x.TotalCount)
                .Take(maxItems)
                .AsNoTracking()
                .ToListAsync();
        }

        private async Task<List<CountryBreakdownDto>> GetCountryDataForTypeAsync(DateTime start, DateTime end, int typeId, int maxItems)
        {
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ICallDetailRepository>();

            var countryData = await repository.GetAll()
                .Where(x => x.AccountingTime >= start && x.AccountingTime <= end && x.TypeID == typeId)
                .GroupBy(x => new { x.OriginCountryID, CountryName = x.OriginCountry.CountryName })
                .Select(g => new CountryInfo
                {
                    CountryID = g.Key.OriginCountryID,
                    CountryName = g.Key.CountryName,
                    TotalCount = g.Count(),
                    SuccessCount = g.Count(x => x.Answer == CallAnswerStatus.Answered),
                    FailCount = g.Count(x => x.Answer != CallAnswerStatus.Answered)
                })
                .OrderByDescending(x => x.TotalCount)
                .Take(maxItems)
                .AsNoTracking()
                .ToListAsync();

            var countryBreakdown = new List<CountryBreakdownDto>();

            foreach (var countryInfo in countryData)
            {
                var cityData = await GetCityDataForCountryAsync(start, end, typeId, countryInfo.CountryID, maxItems);
                countryBreakdown.Add(new CountryBreakdownDto
                {
                    CountryID = countryInfo.CountryID,
                    CountryName = countryInfo.CountryName,
                    TotalCount = countryInfo.TotalCount,
                    SuccessCount = countryInfo.SuccessCount,
                    FailCount = countryInfo.FailCount,
                    Cities = cityData
                });
            }

            return countryBreakdown.OrderByDescending(x => x.TotalCount).ToList();
        }

        private async Task<List<CityBreakdownDto>> GetCityDataForCountryAsync(DateTime start, DateTime end, int typeId, int countryId, int maxItems)
        {
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ICallDetailRepository>();

            var cityData = await repository.GetAll()
                .Where(x => x.AccountingTime >= start && x.AccountingTime <= end &&
                           x.TypeID == typeId && x.OriginCountryID == countryId)
                .GroupBy(x => new { x.OriginCityID, CityName = x.OriginCity.CityName })
                .Select(g => new CityInfo
                {
                    CityID = g.Key.OriginCityID,
                    CityName = g.Key.CityName,
                    TotalCount = g.Count(),
                    SuccessCount = g.Count(x => x.Answer == CallAnswerStatus.Answered),
                    FailCount = g.Count(x => x.Answer != CallAnswerStatus.Answered)
                })
                .OrderByDescending(x => x.TotalCount)
                .Take(maxItems)
                .AsNoTracking()
                .ToListAsync();

            var cityBreakdown = new List<CityBreakdownDto>();

            foreach (var cityInfo in cityData)
            {
                var operatorData = await GetOperatorDataForCityAsync(start, end, typeId, countryId, cityInfo.CityID, maxItems);
                cityBreakdown.Add(new CityBreakdownDto
                {
                    CityID = cityInfo.CityID,
                    CityName = cityInfo.CityName,
                    TotalCount = cityInfo.TotalCount,
                    SuccessCount = cityInfo.SuccessCount,
                    FailCount = cityInfo.FailCount,
                    Operators = operatorData
                });
            }

            return cityBreakdown.OrderByDescending(x => x.TotalCount).ToList();
        }

        private async Task<List<OperatorBreakdownDto>> GetOperatorDataForCityAsync(DateTime start, DateTime end, int typeId, int countryId, int cityId, int maxItems)
        {
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ICallDetailRepository>();

            var operatorData = await repository.GetAll()
                .Where(x => x.AccountingTime >= start && x.AccountingTime <= end &&
                           x.TypeID == typeId && x.OriginCountryID == countryId &&
                           x.OriginCityID == cityId)
                .GroupBy(x => new { x.OriginOperatorID, OperatorName = x.OriginOperator.OperatorName })
                .Select(g => new OperatorInfo
                {
                    OperatorID = g.Key.OriginOperatorID,
                    OperatorName = g.Key.OperatorName,
                    TotalCount = g.Count(),
                    SuccessCount = g.Count(x => x.Answer == CallAnswerStatus.Answered),
                    FailCount = g.Count(x => x.Answer != CallAnswerStatus.Answered)
                })
                .OrderByDescending(x => x.TotalCount)
                .Take(maxItems)
                .AsNoTracking()
                .ToListAsync();

            return operatorData.Select(op => new OperatorBreakdownDto
            {
                OperatorID = op.OperatorID,
                OperatorName = op.OperatorName,
                TotalCount = op.TotalCount,
                SuccessCount = op.SuccessCount,
                FailCount = op.FailCount
            }).ToList();
        }

        private List<object> GenerateChartData(DateTime start, DateTime end, Dictionary<DateTime, (int Success, int Fail)> dailyData)
        {
            var chartData = new List<object>();
            int maxDays = 365;
            var totalDays = (int)(end.Date - start.Date).TotalDays + 1;

            if (totalDays > maxDays) return GenerateMonthlyChartData(start, end, dailyData);

            if (dailyData == null || dailyData.Count == 0)
            {
                for (DateTime day = start.Date; day <= end.Date && day <= DateTime.Today; day = day.AddDays(1))
                {
                    chartData.Add(new { date = day.Ticks, displayDate = ConvertToPersianDate(day), success = 0, fail = 0 });
                }
                return chartData;
            }

            DateTime current = start.Date;
            DateTime lastDate = end.Date > DateTime.Today ? DateTime.Today : end.Date;

            while (current <= lastDate)
            {
                if (dailyData.TryGetValue(current, out var stats))
                {
                    chartData.Add(new { date = current.Ticks, displayDate = ConvertToPersianDate(current), success = stats.Success, fail = stats.Fail });
                }
                else
                {
                    chartData.Add(new { date = current.Ticks, displayDate = ConvertToPersianDate(current), success = 0, fail = 0 });
                }
                current = current.AddDays(1);
            }

            return chartData;
        }

        private List<object> GenerateMonthlyChartData(DateTime start, DateTime end, Dictionary<DateTime, (int Success, int Fail)> dailyData)
        {
            var chartData = new List<object>();
            var monthlyStats = new Dictionary<(int Year, int Month), (int Success, int Fail)>();

            if (dailyData != null)
            {
                foreach (var kvp in dailyData)
                {
                    var date = kvp.Key;
                    var monthKey = (_persianCalendar.GetYear(date), _persianCalendar.GetMonth(date));
                    if (!monthlyStats.ContainsKey(monthKey)) monthlyStats[monthKey] = (0, 0);
                    var current = monthlyStats[monthKey];
                    monthlyStats[monthKey] = (current.Success + kvp.Value.Success, current.Fail + kvp.Value.Fail);
                }
            }

            DateTime currentMonth = new DateTime(_persianCalendar.GetYear(start), _persianCalendar.GetMonth(start), 1);
            DateTime lastMonth = new DateTime(_persianCalendar.GetYear(end), _persianCalendar.GetMonth(end), 1);

            while (currentMonth <= lastMonth)
            {
                var year = _persianCalendar.GetYear(currentMonth);
                var month = _persianCalendar.GetMonth(currentMonth);
                var monthKey = (year, month);

                if (monthlyStats.TryGetValue(monthKey, out var stats))
                {
                    chartData.Add(new { date = currentMonth.Ticks, displayDate = $"{year}/{month:00}", success = stats.Success, fail = stats.Fail });
                }
                else
                {
                    chartData.Add(new { date = currentMonth.Ticks, displayDate = $"{year}/{month:00}", success = 0, fail = 0 });
                }

                currentMonth = _persianCalendar.AddMonths(currentMonth, 1);
            }

            return chartData;
        }

        // --- DTO Classes ---
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
            public List<CityBreakdownDto> Cities { get; set; }
        }

        public class CityBreakdownDto
        {
            public int CityID { get; set; }
            public string CityName { get; set; }
            public int TotalCount { get; set; }
            public int SuccessCount { get; set; }
            public int FailCount { get; set; }
            public List<OperatorBreakdownDto> Operators { get; set; }
        }

        public class OperatorBreakdownDto
        {
            public int OperatorID { get; set; }
            public string OperatorName { get; set; }
            public int TotalCount { get; set; }
            public int SuccessCount { get; set; }
            public int FailCount { get; set; }
        }

        private class TypeInfo
        {
            public int TypeID { get; set; }
            public string TypeName { get; set; }
            public int TotalCount { get; set; }
            public int SuccessCount { get; set; }
            public int FailCount { get; set; }
        }

        private class CountryInfo
        {
            public int CountryID { get; set; }
            public string CountryName { get; set; }
            public int TotalCount { get; set; }
            public int SuccessCount { get; set; }
            public int FailCount { get; set; }
        }

        private class CityInfo
        {
            public int CityID { get; set; }
            public string CityName { get; set; }
            public int TotalCount { get; set; }
            public int SuccessCount { get; set; }
            public int FailCount { get; set; }
        }

        private class OperatorInfo
        {
            public int OperatorID { get; set; }
            public string OperatorName { get; set; }
            public int TotalCount { get; set; }
            public int SuccessCount { get; set; }
            public int FailCount { get; set; }
        }

        private (DateTime Start, DateTime End) GetPersianMonthDateRange(int year, int month)
        {
            int daysInMonth = _persianCalendar.GetDaysInMonth(year, month);
            DateTime start = _persianCalendar.ToDateTime(year, month, 1, 0, 0, 0, 0);
            DateTime end = _persianCalendar.ToDateTime(year, month, daysInMonth, 23, 59, 59, 999);
            return (start, end);
        }

        private (DateTime Start, DateTime End) GetPersianYearDateRange(int year)
        {
            DateTime start = _persianCalendar.ToDateTime(year, 1, 1, 0, 0, 0, 0);
            int lastDayOfEsfand = _persianCalendar.GetDaysInMonth(year, 12);
            DateTime end = _persianCalendar.ToDateTime(year, 12, lastDayOfEsfand, 23, 59, 59, 999);
            return (start, end);
        }

        private DateTime ParsePersianDate(string persianDate)
        {
            var parts = persianDate.Trim().Split('/');
            if (parts.Length != 3) throw new FormatException("فرمت تاریخ اشتباه است");
            return _persianCalendar.ToDateTime(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), 0, 0, 0, 0);
        }

        private string ConvertToPersianDate(DateTime date)
        {
            return $"{_persianCalendar.GetYear(date)}/{_persianCalendar.GetMonth(date):00}/{_persianCalendar.GetDayOfMonth(date):00}";
        }
    }
}