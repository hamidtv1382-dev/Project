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
            var totalCalls = await _unitOfWork.CallDetails.GetAll().CountAsync();

            var answeredCalls = await _unitOfWork.CallDetails.GetAll()
                .Where(cd => cd.Answer == CallAnswerStatus.Answered)
                .CountAsync();

            var answeredCallsQuery = _unitOfWork.CallDetails.GetAll().Where(cd => cd.Answer == CallAnswerStatus.Answered);
            var averageCallDuration = await answeredCallsQuery.AnyAsync()
                ? await answeredCallsQuery.AverageAsync(cd => cd.Length)
                : 0;


            var recentActivityChart = await GetRecentActivityAsync(5);

            var topCitiesChart = await GetTopCallingCitiesWithIranAsync(3);

            var recentCalls = await GetRecentCallsAsync(100);

            return new DashboardDto
            {
                TotalCalls = totalCalls,
                AnsweredCalls = answeredCalls,
                AverageCallDuration = averageCallDuration,
                TopCities = topCitiesChart?.Data ?? new List<ChartDataDto.ChartPoint>(),
                RecentActivity = recentActivityChart?.Data ?? new List<ChartDataDto.ChartPoint>(),
                RecentCalls = recentCalls ?? new List<CallDetailDto>()
            };
        }

        private async Task<ChartDataDto> GetRecentActivityAsync(int count)
        {
            var thirtyDaysAgo = DateTime.Today.AddDays(-30);
            var recentActivityQuery = _unitOfWork.CallDetails.GetAll()
                .Where(cd => cd.AccountingTime >= thirtyDaysAgo)
                .GroupBy(cd => cd.AccountingTime.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Date) 
                .Take(count); 

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


        private async Task<ChartDataDto> GetTopCallingCitiesWithIranAsync(int count)
        {
            var iranCountry = await _unitOfWork.Countries.GetAll()
               .FirstOrDefaultAsync(c => c.CountryName == "Iran");

            if (iranCountry == null)
            {
                return new ChartDataDto { Data = new List<ChartDataDto.ChartPoint>() };
            }

            var callsToIran = _unitOfWork.CallDetails.GetAll()
                .Where(cd => cd.DestCountryID == iranCountry.CountryID);

            var topCitiesQuery = callsToIran
                .GroupBy(cd => cd.OriginCountry)
                .Select(g => new
                {
                    Label = g.Key.CountryName,
                    CallCount = g.Count()
                })
                .OrderByDescending(g => g.CallCount)
                .Take(count);

            var topCities = await topCitiesQuery.ToListAsync();

            var data = topCities.Select(city => new ChartDataDto.ChartPoint
            {
                Label = city.Label,
                Value = city.CallCount
            }).ToList();

            return new ChartDataDto { Data = data };
        }

        private async Task<IEnumerable<CallDetailDto>> GetRecentCallsAsync(int count)
        {
            var recentCallsQuery = _unitOfWork.CallDetails.GetAll()
                .OrderByDescending(cd => cd.AccountingTime)
                .Take(count)
                .Select(cd => new CallDetailDto
                {
                    DetailID = cd.DetailID,
                    ANumber = cd.ANumber,
                    BNumber = cd.BNumber,
                    AccountingTime = cd.AccountingTime,
                    Length = cd.Length,
                    OriginCountryName = cd.OriginCountry.CountryName,
                    DestCountryName = cd.DestCountry.CountryName,
                    Answer = cd.Answer
                });

            return await recentCallsQuery.ToListAsync();
        }

        public async Task<AnalyticsDto> GetAnalyticsDataAsync(CallFilterDto filter)
        {
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