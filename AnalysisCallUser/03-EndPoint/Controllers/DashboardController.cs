using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._01_Domain.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnalysisCallUser._03_EndPoint.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Analytics()
        {
            var data = await _dashboardService.GetAnalyticsDataAsync(new CallFilterDto());
            return View(data);
        }

        public async Task<IActionResult> NetworkChart()
        {
            var data = await _dashboardService.GetNetworkGraphDataAsync();
            return View(data);
        }

        public async Task<IActionResult> MapView()
        {
            var data = await _dashboardService.GetGeographicDataAsync();
            return View(data);
        }

        public async Task<IActionResult> LiveData()
        {
            return View();
        }
    }
}
