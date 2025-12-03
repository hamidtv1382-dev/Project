using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._03_EndPoint.Models.ViewModels.Analytics;
using AnalysisCallUser._03_EndPoint.Models.ViewModels.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using static AnalysisCallUser._01_Domain.Core.DTOs.AnalysisCallUser._01_Domain.Core.DTOs.MapDataDto;

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

        [HttpGet]
        // متد اکشن اصلی که ویو را برمی‌گرداند
        public async Task<IActionResult> Analytics()
        {
            // 1. دریافت داده‌ها از سرویس به صورت DTO
            var analyticsDto = await _dashboardService.GetAnalyticsDataAsync(new CallFilterDto());

            // 2. مپ کردن DTO به ViewModel
            var analyticsViewModel = MapAnalyticsDtoToViewModel(analyticsDto);

            // 3. ارسال ViewModel به View
            return View(analyticsViewModel);
        }

        // متد کمکی برای مپ کردن DTO به ViewModel
        private AnalyticsViewModel MapAnalyticsDtoToViewModel(AnalyticsDto dto)
        {
            if (dto == null)
            {
                // اگر داده‌ای از سرویس دریافت نشد، یک ViewModel خالی برگردان
                return new AnalyticsViewModel();
            }

            var viewModel = new AnalyticsViewModel
            {
                // --- مپ کردن CallVolumeChart ---
                CallVolumeChart = dto.CallVolumeChart != null ? new ChartDataViewModel
                {
                    ChartLabel = dto.CallVolumeChart.ChartLabel,
                    Data = dto.CallVolumeChart.Data.Select(p => new ChartPoint { Label = p.Label, Value = p.Value }).ToList()
                } : new ChartDataViewModel(), // اگر null بود، یک نمونه خالی بساز

                // --- مپ کردن NetworkGraph ---
                NetworkGraph = dto.NetworkGraph != null ? new NetworkViewModel
                {
                    Nodes = dto.NetworkGraph.Nodes.Select(n => new Node
                    {
                        Id = n.Id,
                        Label = n.Label,
                        Size = n.Size,
                        // فیلد Color در DTO وجود ندارد، یک مقدار پیش‌فرض قرار می‌دهیم
                        Color = "#007bff"
                    }).ToList(),
                    Edges = dto.NetworkGraph.Edges.Select(e => new Edge
                    {
                        From = e.From,
                        To = e.To,
                        Weight = e.Weight
                    }).ToList()
                } : new NetworkViewModel(),

                // --- مپ کردن GeographicMap ---
                GeographicMap = dto.GeographicMap != null ? new GeographicAnalysisViewModel
                {
                    // فیلدهایی که در DTO وجود ندارند را با لیست‌های خالی مقداردهی می‌کنیم
                    CountryIds = new List<int>(),
                    DistanceData = new List<int>(),
                    ContinentData = new List<int>(),
                    DomesticIntlData = new List<int>(),

                    // مپ کردن نقاط روی نقشه
                    Points = dto.GeographicMap.Points.Select(p => new MapPoint
                    {
                        // در DTO فقط CountryCode و CallCount وجود دارد. بقیه را با مقادیر پیش‌فرض پر می‌کنیم
                        CountryName = p.CountryCode, // فرض می‌کنیم CountryCode به عنوان نام کشور کافی است
                        CallCount = p.CallCount,
                        AvgDuration = 0,
                        AnswerRate = 0,
                    }).ToList()
                } : new GeographicAnalysisViewModel()
            };

            // --- مقداردهی به ویژگی‌هایی که در DTO وجود ندارند ---
            // این کار از بروز خطای NullReferenceException در View جلوگیری می‌کند
            viewModel.CountryChart = new ChartDataViewModel();
            viewModel.OperatorChart = new OperatorPerformanceViewModel();
            viewModel.AnswerRateChart = new ChartDataViewModel();

            return viewModel;
        }


        public async Task<IActionResult> NetworkChart()
        {
            var data = await _dashboardService.GetNetworkGraphDataAsync();
            // این متد هم باید مپ شود، اما فعلاً طبق درخواست شما روی Analytics تمرکز می‌کنیم
            return View(data);
        }

        public async Task<IActionResult> MapView()
        {
            var data = await _dashboardService.GetGeographicDataAsync();
            // این متد هم باید مپ شود، اما فعلاً طبق درخواست شما روی Analytics تمرکز می‌کنیم
            return View(data);
        }

        public async Task<IActionResult> LiveData()
        {
            return View();
        }
    }
}