using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._03_EndPoint.Models.ViewModels.Call;
using AnalysisCallUser._03_EndPoint.Models.ViewModels.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace AnalysisCallUser._03_EndPoint.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly IDistributedCache _distributedCache;

        public HomeController(IDashboardService dashboardService, IDistributedCache distributedCache)
        {
            _dashboardService = dashboardService;
            _distributedCache = distributedCache;
        }

        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            // +++ تغییر کلیدی: کلید کش را برای هر کاربر جداگانه می‌سازیم +++
            string cacheKey = $"DashboardData_{userId}";
            DashboardDto dashboardDto;

            try
            {
                var cachedData = await _distributedCache.GetStringAsync(cacheKey);

                if (cachedData != null)
                {
                    dashboardDto = JsonSerializer.Deserialize<DashboardDto>(cachedData);
                }
                else
                {
                    dashboardDto = await _dashboardService.GetDashboardDataAsync(userId);

                    if (dashboardDto == null)
                    {
                        // اگر سرویس نال برگرداند، یک مدل خالی به ویو ارسال می‌کنیم
                        return View(new DashboardViewModel());
                    }

                    var cacheOptions = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                    };

                    await _distributedCache.SetStringAsync(cacheKey, JsonSerializer.Serialize(dashboardDto), cacheOptions);
                }
            }
            catch (Exception ex)
            {
                // در صورت بروز خطا در کش یا سرویس، لاگ گرفته و یک ویو خالی نمایش دهید
                // _logger.LogError(ex, "Error loading dashboard data for user {UserId}", userId);
                return View(new DashboardViewModel());
            }


            var model = new DashboardViewModel
            {
                TotalCalls = dashboardDto.TotalCalls,
                AnsweredCalls = dashboardDto.AnsweredCalls,
                AverageCallDuration = dashboardDto.AverageCallDuration,

                TopCities = dashboardDto.TopCities?
                    .Select(c => new AnalysisCallUser._03_EndPoint.Models.ViewModels.Analytics.ChartPoint
                    {
                        Label = c.Label,
                        Value = (int)c.Value
                    })
                    .ToList() ?? new List<AnalysisCallUser._03_EndPoint.Models.ViewModels.Analytics.ChartPoint>(),

                RecentActivity = dashboardDto.RecentActivity?
                    .Select(a => new AnalysisCallUser._03_EndPoint.Models.ViewModels.Analytics.ChartPoint
                    {
                        Label = a.Label,
                        Value = (int)a.Value
                    })
                    .ToList() ?? new List<AnalysisCallUser._03_EndPoint.Models.ViewModels.Analytics.ChartPoint>(),

                RecentCalls = dashboardDto.RecentCalls?
                    .Select(rc => new CallDetailsViewModel
                    {
                        DetailID = rc.DetailID,
                        ANumber = rc.ANumber,
                        BNumber = rc.BNumber,
                        AccountingTime = rc.AccountingTime,
                        Length = rc.Length,
                        OriginCountryName = rc.OriginCountryName,
                        DestCountryName = rc.DestCountryName,
                        Answer = rc.Answer
                    })
                    .ToList() ?? new List<CallDetailsViewModel>()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearCache()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Json(new { success = false, message = "کاربر شناسایی نشد." });
            }

            try
            {
                // +++ تغییر کلیدی: کلید کش را برای هر کاربر جداگانه پاک می‌کنیم +++
                string cacheKey = $"DashboardData_{userId}";
                await _distributedCache.RemoveAsync(cacheKey);
                return Json(new { success = true, message = "کش با موفقیت خالی شد. صفحه به‌روزرسانی می‌شود..." });
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error clearing cache for user {UserId}", userId);
                return Json(new { success = false, message = "خطا در خالی کردن کش." });
            }
        }
    }
}