using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._03_EndPoint.Models.ViewModels.Analytics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnalysisCallUser._03_EndPoint.Controllers
{
    [Authorize]
    public class AnalyticsController : Controller
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<IActionResult> NetworkAnalysis(NetworkAnalysisViewModel model)
        {
            // تبدیل ViewModel به DTO
            var networkAnalysisDto = new NetworkAnalysisDto
            {
                PhoneNumbers = model.PhoneNumbers,
                MaxDepth = model.MaxDepth
            };

            var data = await _analyticsService.GetCallNetworkAnalysisAsync(networkAnalysisDto);
            return View(data);
        }

        public async Task<IActionResult> GeographicAnalysis(GeographicAnalysisViewModel model)
        {
            // تبدیل ViewModel به DTO
            var geographicAnalysisDto = new GeographicAnalysisDto
            {
                CountryIds = model.CountryIds
            };

            var data = await _analyticsService.GetGeographicDistributionAsync(geographicAnalysisDto);
            return View(data);
        }

        public async Task<IActionResult> TimeAnalysis(TimeAnalysisViewModel model)
        {
            // تبدیل ViewModel به DTO
            var timeAnalysisDto = new TimeAnalysisDto
            {
                StartDate = model.StartDate,
                EndDate = model.EndDate
            };

            var data = await _analyticsService.GetCallVolumeOverTimeAsync(timeAnalysisDto);
            return View(data);
        }

        public async Task<IActionResult> OperatorPerformance()
        {
            var data = await _analyticsService.GetOperatorPerformanceDataAsync();
            return View(data);
        }

        public async Task<IActionResult> LiveDashboard()
        {
            return View();
        }
    }
}
