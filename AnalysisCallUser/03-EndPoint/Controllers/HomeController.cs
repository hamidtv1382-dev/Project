using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._03_EndPoint.Models.ViewModels.Call;
using AnalysisCallUser._03_EndPoint.Models.ViewModels.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static AnalysisCallUser._01_Domain.Core.DTOs.AnalyticsDataDto;

namespace AnalysisCallUser._03_EndPoint.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public HomeController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            // گرفتن UserId از Claim
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            // گرفتن داده‌ها از سرویس
            DashboardDto dashboardDto = await _dashboardService.GetDashboardDataAsync(userId);

            // تبدیل DTO به ViewModel
            var model = new DashboardViewModel
            {
                TotalCalls = dashboardDto.TotalCalls,
                AnsweredCalls = dashboardDto.AnsweredCalls,
                AverageCallDuration = dashboardDto.AverageCallDuration,
                TopCountries = dashboardDto.TopCountries
                .Select(c => new AnalysisCallUser._03_EndPoint.Models.ViewModels.Analytics.ChartPoint
                {
                    Label = c.Label,
                    Value = (int)c.Value // cast اگر لازم است
                })
                .ToList(),

                            RecentActivity = dashboardDto.RecentActivity
                .Select(a => new AnalysisCallUser._03_EndPoint.Models.ViewModels.Analytics.ChartPoint
                {
                    Label = a.Label,
                    Value = (int)a.Value // cast اگر لازم است
                })
                .ToList(),

                RecentCalls = new List<CallDetailsViewModel>() // هنوز خالی
            };

            return View(model);
        }

    }
}
