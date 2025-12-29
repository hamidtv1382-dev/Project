using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._01_Domain.Core.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace AnalysisCallUser._03_EndPoint.Controllers
{
    public class AnalysisOverViewController : Controller
    {
        private readonly ICallDetailRepository _callDetailRepository;
        private readonly IServiceProvider _serviceProvider;
        private readonly PersianCalendar _persianCalendar = new();

        public AnalysisOverViewController(
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
            [FromQuery] int maxCountriesPerType = 10)
        {
            try
            {
                // ------------------------------
                // 1. تعیین بازه زمانی
                // ------------------------------
                DateTime start, end;
                GetDateRange(startDateStr, endDateStr, year, month, out start, out end);

                // ------------------------------
                // 2. اجرای کوئری‌ها با DbContextهای جداگانه
                // ------------------------------
                using var scope1 = _serviceProvider.CreateScope();
                using var scope2 = _serviceProvider.CreateScope();

                var dailyStatsTask = GetDailyStatisticsAsync(scope1.ServiceProvider, start, end);
                var aggregatedStatsTask = GetAggregatedStatisticsAsync(scope2.ServiceProvider, start, end, maxCountriesPerType);

                // انتظار برای تکمیل هر دو تسک
                var dailyResult = await dailyStatsTask;
                var aggregatedResult = await aggregatedStatsTask;

                // ------------------------------
                // 3. پردازش داده‌های روزانه
                // ------------------------------
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

        // ------------------------------
        // متدهای اصلی (با dependency injection جداگانه)
        // ------------------------------

        private void GetDateRange(string startDateStr, string endDateStr, int? year, int? month,
            out DateTime start, out DateTime end)
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

        private async Task<(Dictionary<DateTime, (int Success, int Fail)> dailyData, int totalSuccess, int totalFail)>
            GetDailyStatisticsAsync(IServiceProvider serviceProvider, DateTime start, DateTime end)
        {
            // ایجاد repository جدید با scope جدید
            var repository = serviceProvider.GetRequiredService<ICallDetailRepository>();

            // کوئری بهینه‌شده - فقط فیلدهای مورد نیاز
            var query = repository.GetAll()
                .Where(x => x.AccountingTime >= start && x.AccountingTime <= end)
                .Select(x => new
                {
                    Date = x.AccountingTime.Date,
                    IsSuccess = x.Answer == CallAnswerStatus.Answered
                });

            // اجرای کوئری
            var data = await query
                .AsNoTracking()
                .ToListAsync();

            // پردازش در حافظه
            var dailyStats = new Dictionary<DateTime, (int Success, int Fail)>();
            int totalSuccess = 0, totalFail = 0;

            foreach (var item in data)
            {
                if (item.IsSuccess)
                    totalSuccess++;
                else
                    totalFail++;

                if (!dailyStats.ContainsKey(item.Date))
                {
                    dailyStats[item.Date] = (0, 0);
                }

                var current = dailyStats[item.Date];
                if (item.IsSuccess)
                    dailyStats[item.Date] = (current.Success + 1, current.Fail);
                else
                    dailyStats[item.Date] = (current.Success, current.Fail + 1);
            }

            return (dailyStats, totalSuccess, totalFail);
        }

        private async Task<(List<TypeBreakdownDto> typeBreakdown, int totalSuccess, int totalFail)>
            GetAggregatedStatisticsAsync(IServiceProvider serviceProvider, DateTime start, DateTime end, int maxCountriesPerType)
        {
            // ایجاد repository جدید با scope جدید
            var repository = serviceProvider.GetRequiredService<ICallDetailRepository>();

            // استراتژی: ابتدا آمار کلی را بگیریم
            var summaryQuery = repository.GetAll()
                .Where(x => x.AccountingTime >= start && x.AccountingTime <= end)
                .GroupBy(x => 1) // گروه‌بندی کل
                .Select(g => new
                {
                    TotalCount = g.Count(),
                    SuccessCount = g.Count(x => x.Answer == CallAnswerStatus.Answered),
                    FailCount = g.Count(x => x.Answer != CallAnswerStatus.Answered)
                });

            var summary = await summaryQuery.AsNoTracking().FirstOrDefaultAsync();

            if (summary == null)
                return (new List<TypeBreakdownDto>(), 0, 0);

            // سپس داده‌های تفکیک شده را بگیریم
            var rawData = await repository.GetAll()
                .Where(x => x.AccountingTime >= start && x.AccountingTime <= end)
                .Select(x => new
                {
                    x.TypeID,
                    TypeName = x.CallType.TypeName,
                    x.OriginCountryID,
                    CountryName = x.OriginCountry.CountryName,
                    IsSuccess = x.Answer == CallAnswerStatus.Answered
                })
                .AsNoTracking()
                .ToListAsync();

            // پردازش در حافظه
            var typeGroups = rawData
                .GroupBy(x => new { x.TypeID, x.TypeName })
                .Select(g => new
                {
                    g.Key.TypeID,
                    g.Key.TypeName,
                    TotalCount = g.Count(),
                    SuccessCount = g.Count(x => x.IsSuccess),
                    FailCount = g.Count(x => !x.IsSuccess),
                    CountryGroups = g
                        .GroupBy(x => new { x.OriginCountryID, x.CountryName })
                        .Select(cg => new
                        {
                            cg.Key.OriginCountryID,
                            cg.Key.CountryName,
                            TotalCount = cg.Count(),
                            SuccessCount = cg.Count(x => x.IsSuccess),
                            FailCount = cg.Count(x => !x.IsSuccess)
                        })
                        .OrderByDescending(c => c.TotalCount)
                        .Take(maxCountriesPerType)
                        .ToList()
                })
                .OrderByDescending(x => x.TotalCount)
                .ToList();

            var typeBreakdown = typeGroups.Select(g => new TypeBreakdownDto
            {
                TypeID = g.TypeID,
                TypeName = g.TypeName,
                TotalCount = g.TotalCount,
                SuccessCount = g.SuccessCount,
                FailCount = g.FailCount,
                Countries = g.CountryGroups.Select(c => new CountryBreakdownDto
                {
                    CountryID = c.OriginCountryID,
                    CountryName = c.CountryName,
                    TotalCount = c.TotalCount,
                    SuccessCount = c.SuccessCount,
                    FailCount = c.FailCount
                }).ToList()
            }).ToList();

            return (typeBreakdown, summary.SuccessCount, summary.FailCount);
        }

        private List<object> GenerateChartData(DateTime start, DateTime end,
            Dictionary<DateTime, (int Success, int Fail)> dailyData)
        {
            var chartData = new List<object>();

            if (dailyData == null || dailyData.Count == 0)
            {
                // اگر داده‌ای نداریم، حداقل یک روز خالی برگردانیم
                for (DateTime day = start.Date; day <= end.Date && day <= DateTime.Today; day = day.AddDays(1))
                {
                    chartData.Add(new
                    {
                        date = day.Ticks,
                        displayDate = ConvertToPersianDate(day),
                        success = 0,
                        fail = 0
                    });
                }
                return chartData;
            }

            int totalDays = (int)(end - start).TotalDays + 1;

            if (totalDays > 0)
            {
                chartData.Capacity = Math.Min(totalDays, 365); // حداکثر یک سال

                DateTime current = start.Date;
                DateTime lastDate = end.Date > DateTime.Today ? DateTime.Today : end.Date;

                while (current <= lastDate)
                {
                    if (dailyData.TryGetValue(current, out var stats))
                    {
                        chartData.Add(new
                        {
                            date = current.Ticks,
                            displayDate = ConvertToPersianDate(current),
                            success = stats.Success,
                            fail = stats.Fail
                        });
                    }
                    else
                    {
                        chartData.Add(new
                        {
                            date = current.Ticks,
                            displayDate = ConvertToPersianDate(current),
                            success = 0,
                            fail = 0
                        });
                    }
                    current = current.AddDays(1);
                }
            }

            return chartData;
        }

        // ------------------------------
        // DTO Classes
        // ------------------------------
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

        // ------------------------------
        // Helper Methods
        // ------------------------------
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