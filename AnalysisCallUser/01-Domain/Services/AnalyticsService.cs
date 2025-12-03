using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._01_Domain.Core.DTOs.AnalysisCallUser._01_Domain.Core.DTOs;
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

            // فیلتر کشور
            if (filter.CountryIds?.Any() == true)
                query = query.Where(cd => filter.CountryIds.Contains(cd.OriginCountryID));

            // گروه‌بندی بر اساس کشور و شهر
            var data = await query
                .GroupBy(cd => new
                {
                    Country = cd.OriginCountry.CountryName,
                    City = cd.OriginCity.CityName
                })
                .Select(g => new MapDataDto.MapPoint
                {
                    CountryName = g.Key.Country,
                    CityName = g.Key.City,
                    CallCount = g.Count(),

                    // میانگین مدت مکالمه
                    AvgDuration = g.Average(x => x.Length),

                    // درصد پاسخ
                    AnswerRate = g.Count(x => x.Answer == CallAnswerStatus.Answered)
                                 * 100.0 / g.Count()
                })
                .OrderByDescending(x => x.CallCount)
                .Take(100)
                .ToListAsync();

            return new MapDataDto { Points = data };
        }

        // در کلاس AnalyticsService
        public async Task<NetworkDto> GetCallNetworkAnalysisAsync(NetworkAnalysisDto filter)
        {
            var query = _unitOfWork.CallDetails.GetAll();

            if (filter.PhoneNumbers?.Any() == true)
            {
                query = query.Where(cd => filter.PhoneNumbers.Contains(cd.ANumber) || filter.PhoneNumbers.Contains(cd.BNumber));
            }

            var sampleData = await query.Take(200).ToListAsync();

            // تمام شماره‌های منحصر به فرد
            var allNodes = sampleData.SelectMany(cd => new[] { cd.ANumber, cd.BNumber }).Distinct().ToList();

            // محاسبه تعداد ارتباطات برای هر شماره (Degree)
            var nodeDegrees = sampleData
                .SelectMany(cd => new[] { new { Node = cd.ANumber }, new { Node = cd.BNumber } })
                .GroupBy(x => x.Node)
                .ToDictionary(g => g.Key, g => g.Count());

            var nodes = allNodes.Select(number => new NetworkDto.Node
            {
                Id = number,
                Label = number,
                // اندازه گره بر اساس تعداد کل ارتباطات (ورودی و خروجی)
                Size = nodeDegrees.ContainsKey(number) ? nodeDegrees[number] : 0
            }).ToList();

            // گروه‌بندی یال‌ها برای محاسبه وزن (تعداد تماس بین دو گره)
            var edges = sampleData
                .GroupBy(cd => new { From = cd.ANumber, To = cd.BNumber })
                .Select(g => new NetworkDto.Edge
                {
                    From = g.Key.From,
                    To = g.Key.To,
                    Weight = g.Count() // وزن بر اساس تعداد تماس‌ها
                })
                .ToList();

            return new NetworkDto { Nodes = nodes, Edges = edges };
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

