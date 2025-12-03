using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._03_EndPoint.Models.ViewModels.Call;
using AnalysisCallUser._03_EndPoint.Models.ViewModels.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            DashboardDto dashboardDto = await _dashboardService.GetDashboardDataAsync(userId);

            if (dashboardDto == null ||
                dashboardDto.TopCountries == null ||
                dashboardDto.RecentActivity == null ||
                dashboardDto.RecentCalls == null)
            {
                return View(new DashboardViewModel());
            }

            var model = new DashboardViewModel
            {
                TotalCalls = dashboardDto.TotalCalls,
                AnsweredCalls = dashboardDto.AnsweredCalls,
                AverageCallDuration = dashboardDto.AverageCallDuration,
                TopCountries = dashboardDto.TopCountries
                    .Select(c => new AnalysisCallUser._03_EndPoint.Models.ViewModels.Analytics.ChartPoint
                    {
                        Label = c.Label,
                        Value = (int)c.Value
                    })
                    .ToList(),

                RecentActivity = dashboardDto.RecentActivity
                    .Select(a => new AnalysisCallUser._03_EndPoint.Models.ViewModels.Analytics.ChartPoint
                    {
                        Label = a.Label,
                        Value = (int)a.Value
                    })
                    .ToList(),

                RecentCalls = dashboardDto.RecentCalls
                    .Select(rc => new CallDetailsViewModel
                    {
                        DetailID = rc.DetailID,
                        ANumber = rc.ANumber,
                        BNumber = rc.BNumber,
                        AccountingTime_Date = rc.AccountingTime_Date,
                        AccountingTime_Time = rc.AccountingTime_Time,
                        Length = rc.Length,
                        OriginCountryName = rc.OriginCountryName,
                        DestCountryName = rc.DestCountryName,
                        Answer = rc.Answer
                    })
                    .ToList()
            };

            return View(model);
        }
    }
}