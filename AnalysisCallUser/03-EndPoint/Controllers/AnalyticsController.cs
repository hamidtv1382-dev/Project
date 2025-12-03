using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._03_EndPoint.Models.ViewModels.Analytics;
using AnalysisCallUser._03_EndPoint.Models.ViewModels.Dashboard;
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
            if (model == null)
            {
                model = new NetworkAnalysisViewModel();
            }

            var networkAnalysisDto = new NetworkAnalysisDto
            {
                PhoneNumbers = model.PhoneNumbers,
                MaxDepth = model.MaxDepth
            };

            var data = await _analyticsService.GetCallNetworkAnalysisAsync(networkAnalysisDto);

            // تابع کمکی برای تعیین رنگ بر اساس اندازه
            string GetNodeColor(int size)
            {
                // اگر اندازه بزرگتر از 10 بود، قرمز (بسیار مهم)
                if (size > 10) return "#c0392b";
                // اگر بین 5 تا 10 بود، نارنجی (مهم)
                if (size > 5) return "#e67e22";
                // در غیر این صورت، آبی (عادی)
                return "#3498db";
            }

            var networkViewModel = new NetworkViewModel
            {
                Nodes = data.Nodes.Select(n => new Node
                {
                    Id = n.Id,
                    Label = n.Label,
                    Size = n.Size,
                    // رنگ را بر اساس اندازه محاسبه کن
                    Color = GetNodeColor(n.Size)
                }).ToList(),
                Edges = data.Edges.Select(e => new Edge
                {
                    From = e.From,
                    To = e.To,
                    Weight = e.Weight
                }).ToList()
            };

            model.NetworkGraph = networkViewModel;

            return View(model);
        }
        public async Task<IActionResult> GeographicAnalysis(GeographicAnalysisViewModel model)
        {
            if (model == null)
            {
                model = new GeographicAnalysisViewModel();
            }

            var filter = new GeographicAnalysisDto
            {
                CountryIds = model.CountryIds
            };

            var mapData = await _analyticsService.GetGeographicDistributionAsync(filter);

            var viewModel = new GeographicAnalysisViewModel
            {
                CountryIds = model.CountryIds,

                MapData = new MapViewModel
                {
                    Points = mapData.Points.Select(p => new MapViewModel.MapPoint
                    {
                        CountryName = p.CountryName,
                        CityName = p.CityName,
                        CallCount = p.CallCount,
                        AvgDuration = p.AvgDuration,
                        AnswerRate = p.AnswerRate
                    }).ToList()
                },

                // داده‌های نمونه نمودار
                ContinentData = new List<int> { 30, 25, 15, 20, 7, 3 },
                DomesticIntlData = new List<int> { 65, 35 },
                DistanceData = new List<int> { 40, 30, 15, 10, 5 }
            };

            return View(viewModel);
        }


        public async Task<IActionResult> TimeAnalysis(TimeAnalysisViewModel model)
        {
            // تبدیل ViewModel به DTO
            var timeAnalysisDto = new TimeAnalysisDto
            {
                StartDate = model.StartDate,
                EndDate = model.EndDate
            };

            var chartData = await _analyticsService.GetCallVolumeOverTimeAsync(timeAnalysisDto);

            var viewModel = new TimeAnalysisViewModel
            {
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                CallVolumeOverTime = new ChartDataViewModel
                {
                    ChartLabel = chartData.ChartLabel,
                    Data = chartData.Data.Select(d => new ChartPoint
                    {
                        Label = d.Label,
                        Value = d.Value
                    }).ToList()
                },
                SeasonalData = new List<double> { 1200, 1500, 1100, 1300 } // داده‌های پیش‌فرض فصلی
            };

            return View(viewModel);
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
