using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._01_Domain.Core.Entities;
using AnalysisCallUser._01_Domain.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace AnalysisCallUser._01_Domain.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ICallAnalysisService _callAnalysisService;
        private readonly IAnalyticsService _analyticsService;
        private readonly IUnitOfWork _unitOfWork;

        public DashboardService(ICallAnalysisService callAnalysisService, IAnalyticsService analyticsService, IUnitOfWork unitOfWork)
        {
            _callAnalysisService = callAnalysisService;
            _analyticsService = analyticsService;
            _unitOfWork = unitOfWork;
        }

        public async Task<DashboardDto> GetDashboardDataAsync(int userId)
        {
            // دریافت کل تماس‌ها
            var totalCalls = await _unitOfWork.CallDetails.GetAll().CountAsync();

            // دریافت تماس‌های پاسخ داده شده
            var answeredCalls = await _unitOfWork.CallDetails.GetAll()
                .Where(cd => cd.Answer == CallAnswerStatus.Answered)
                .CountAsync();

            // محاسبه میانگین مدت تماس
            var answeredCallsQuery = _unitOfWork.CallDetails.GetAll().Where(cd => cd.Answer == CallAnswerStatus.Answered);
            var averageCallDuration = await answeredCallsQuery.AnyAsync()
                ? await answeredCallsQuery.AverageAsync(cd => cd.Length)
                : 0;

            // دریافت کشورهای برتر
            var topCountriesChart = await _callAnalysisService.GetTopCallingCountriesAsync(5);

            // دریافت فعالیت اخیر (تماس‌های 30 روز گذشته)
            var recentActivityChart = await GetRecentActivityAsync();

            // دریافت 100 تماس اخیر
            var recentCalls = await GetRecentCallsAsync(100);

            return new DashboardDto
            {
                TotalCalls = totalCalls,
                AnsweredCalls = answeredCalls,
                AverageCallDuration = averageCallDuration,
                TopCountries = topCountriesChart?.Data ?? new List<ChartDataDto.ChartPoint>(),
                RecentActivity = recentActivityChart?.Data ?? new List<ChartDataDto.ChartPoint>(),
                RecentCalls = recentCalls ?? new List<CallDetailDto>()
            };
        }

        private async Task<ChartDataDto> GetRecentActivityAsync()
        {
            // دریافت تماس‌های 30 روز گذشته گروه‌بندی شده بر اساس روز
            var thirtyDaysAgo = DateTime.Today.AddDays(-30);
            var recentActivityQuery = _unitOfWork.CallDetails.GetAll()
                .Where(cd => cd.AccountingTime_Date >= thirtyDaysAgo)
                .GroupBy(cd => cd.AccountingTime_Date.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(g => g.Date);

            var recentActivity = await recentActivityQuery.ToListAsync();

            return new ChartDataDto
            {
                ChartLabel = "فعالیت اخیر",
                Data = recentActivity.Select(r => new ChartDataDto.ChartPoint
                {
                    Label = r.Date.ToString("yyyy/MM/dd"),
                    Value = r.Count
                }).ToList()
            };
        }

        private async Task<IEnumerable<CallDetailDto>> GetRecentCallsAsync(int count)
        {
            var recentCallsQuery = _unitOfWork.CallDetails.GetAll()
                .OrderByDescending(cd => cd.AccountingTime_Date)
                .ThenByDescending(cd => cd.AccountingTime_Time)
                .Take(count)
                .Select(cd => new CallDetailDto
                {
                    DetailID = cd.DetailID,
                    ANumber = cd.ANumber,
                    BNumber = cd.BNumber,
                    AccountingTime_Date = cd.AccountingTime_Date,
                    AccountingTime_Time = cd.AccountingTime_Time,
                    Length = cd.Length,
                    OriginCountryName = cd.OriginCountry.CountryName,
                    DestCountryName = cd.DestCountry.CountryName,
                    Answer = cd.Answer
                });

            return await recentCallsQuery.ToListAsync();
        }

        public async Task<AnalyticsDto> GetAnalyticsDataAsync(CallFilterDto filter)
        {
            // اگر filter.StartDate null بود، از 30 روز قبل استفاده کن. اگر filter.EndDate null بود، از امروز استفاده کن.
            var callVolumeChart = await _analyticsService.GetCallVolumeOverTimeAsync(new TimeAnalysisDto
            {
                StartDate = filter.StartDate ?? DateTime.Today.AddDays(-30),
                EndDate = filter.EndDate ?? DateTime.Today
            });

            var geographicMap = await _analyticsService.GetGeographicDistributionAsync(new GeographicAnalysisDto());
            var networkGraph = await _analyticsService.GetCallNetworkAnalysisAsync(new NetworkAnalysisDto());

            return new AnalyticsDto
            {
                CallVolumeChart = callVolumeChart,
                GeographicMap = geographicMap,
                NetworkGraph = networkGraph
            };
        }

        public async Task<NetworkDto> GetNetworkGraphDataAsync()
        {
            var data = await _analyticsService.GetCallNetworkAnalysisAsync(new NetworkAnalysisDto());
            return new NetworkDto { Nodes = data.Nodes, Edges = data.Edges };
        }

        public async Task<MapDto> GetGeographicDataAsync()
        {
            var data = await _analyticsService.GetGeographicDistributionAsync(new GeographicAnalysisDto());
            return new MapDto { Points = data.Points };
        }
    }
}