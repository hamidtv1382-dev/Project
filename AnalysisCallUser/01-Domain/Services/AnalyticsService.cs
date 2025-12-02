using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._01_Domain.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace AnalysisCallUser._01_Domain.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AnalyticsService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ChartDataDto> GetCallVolumeOverTimeAsync(TimeAnalysisDto filter)
        {
            var data = await _unitOfWork.CallDetails
                .GetAll()
                .Where(cd => cd.AccountingTime_Date >= filter.StartDate && cd.AccountingTime_Date <= filter.EndDate)
                .GroupBy(cd => cd.AccountingTime_Date.Date)
                .Select(g => new ChartDataDto.ChartPoint { Label = g.Key.ToString("yyyy-MM-dd"), Value = g.Count() })
                .ToListAsync();

            return new ChartDataDto { ChartLabel = "Call Volume Over Time", Data = data };
        }

        public async Task<MapDataDto> GetGeographicDistributionAsync(GeographicAnalysisDto filter)
        {
            var query = _unitOfWork.CallDetails.GetAll();
            if (filter.CountryIds?.Any() == true)
            {
                query = query.Where(cd => filter.CountryIds.Contains(cd.OriginCountryID));
            }

            var data = await query
                .GroupBy(cd => new { cd.OriginCountry.CountryName, cd.OriginCity.CityName })
                .Select(g => new MapDataDto.MapPoint { CountryCode = g.Key.CountryName.Substring(0, 2), CityName = g.Key.CityName, CallCount = g.Count() })
                .Take(100)
                .ToListAsync();

            return new MapDataDto { Points = data };
        }

        public async Task<NetworkDto> GetCallNetworkAnalysisAsync(NetworkAnalysisDto filter)
        {
            // ورودی از نوع NetworkAnalysisDto دریافت می‌شود
            var query = _unitOfWork.CallDetails.GetAll();

            if (filter.PhoneNumbers?.Any() == true)
            {
                query = query.Where(cd => filter.PhoneNumbers.Contains(cd.ANumber) || filter.PhoneNumbers.Contains(cd.BNumber));
            }

            var sampleData = await query.Take(200).ToListAsync();

            // خروجی از نوع NetworkDto ساخته می‌شود
            var nodes = new List<NetworkDto.Node>();
            var edges = new List<NetworkDto.Edge>();

            foreach (var call in sampleData)
            {
                nodes.Add(new NetworkDto.Node { Id = call.ANumber, Label = call.ANumber, Size = 1 });
                nodes.Add(new NetworkDto.Node { Id = call.BNumber, Label = call.BNumber, Size = 1 });
                edges.Add(new NetworkDto.Edge { From = call.ANumber, To = call.BNumber, Weight = 1 });
            }

            return new NetworkDto { Nodes = nodes.Distinct().ToList(), Edges = edges };
        }

        public async Task<OperatorPerformanceDto> GetOperatorPerformanceDataAsync()
        {
            var performance = await _unitOfWork.CallDetails
                .GetAll()
                .Where(cd => cd.Answer == CallAnswerStatus.Answered)
                .GroupBy(cd => cd.OriginOperator.OperatorName)
                .Select(g => new OperatorPerformanceDto.OperatorStat
                {
                    OperatorName = g.Key,
                    TotalCalls = g.Count(),
                    AverageDuration = g.Average(cd => cd.Length)
                })
                .ToListAsync();

            return new OperatorPerformanceDto { Operators = performance };
        }
    }
}

