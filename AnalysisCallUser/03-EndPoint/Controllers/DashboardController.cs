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
        public async Task<IActionResult> Analytics()
        {
            var analyticsDto = await _dashboardService.GetAnalyticsDataAsync(new CallFilterDto());

            var analyticsViewModel = MapAnalyticsDtoToViewModel(analyticsDto);

            return View(analyticsViewModel);
        }

        private AnalyticsViewModel MapAnalyticsDtoToViewModel(AnalyticsDto dto)
        {
            if (dto == null)
            {
               
                return new AnalyticsViewModel();
            }

            var viewModel = new AnalyticsViewModel
            {
                CallVolumeChart = dto.CallVolumeChart != null ? new ChartDataViewModel
                {
                    ChartLabel = dto.CallVolumeChart.ChartLabel,
                    Data = dto.CallVolumeChart.Data.Select(p => new ChartPoint { Label = p.Label, Value = p.Value }).ToList()
                } : new ChartDataViewModel(), 

                NetworkGraph = dto.NetworkGraph != null ? new NetworkViewModel
                {
                    Nodes = dto.NetworkGraph.Nodes.Select(n => new Node
                    {
                        Id = n.Id,
                        Label = n.Label,
                        Size = n.Size,
                        Color = "#007bff"
                    }).ToList(),
                    Edges = dto.NetworkGraph.Edges.Select(e => new Edge
                    {
                        From = e.From,
                        To = e.To,
                        Weight = e.Weight
                    }).ToList()
                } : new NetworkViewModel(),

                GeographicMap = dto.GeographicMap != null ? new GeographicAnalysisViewModel
                {
                    CountryIds = new List<int>(),
                    DistanceData = new List<int>(),
                    ContinentData = new List<int>(),
                    DomesticIntlData = new List<int>(),

                    Points = dto.GeographicMap.Points.Select(p => new MapPoint
                    {
                        CountryName = p.CountryCode, 
                        CallCount = p.CallCount,
                        AvgDuration = 0,
                        AnswerRate = 0,
                    }).ToList()
                } : new GeographicAnalysisViewModel()
            };

            viewModel.CountryChart = new ChartDataViewModel();
            viewModel.OperatorChart = new OperatorPerformanceViewModel();
            viewModel.AnswerRateChart = new ChartDataViewModel();

            return viewModel;
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