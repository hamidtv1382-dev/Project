using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._01_Domain.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace AnalysisCallUser._01_Domain.Services
{
    public class CallAnalysisService : ICallAnalysisService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CallAnalysisService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ChartDataDto> GetTopCallingCountriesAsync(int count = 10)
        {
            var topCountries = await _unitOfWork.CallDetails
                .GetAll()
                .GroupBy(cd => cd.OriginCountry.CountryName)
                .Select(g => new { CountryName = g.Key, CallCount = g.Count() })
                .OrderByDescending(g => g.CallCount)
                .Take(count)
                .ToListAsync();

            return new ChartDataDto
            {
                ChartLabel = "Top Calling Countries",
                Data = topCountries.Select(c => new ChartDataDto.ChartPoint { Label = c.CountryName, Value = c.CallCount }).ToList()
            };
        }

        public async Task<ChartDataDto> GetAnsweredVsNotAnsweredCallsAsync()
        {
            var stats = await _unitOfWork.CallDetails
                .GetAll()
                .GroupBy(cd => cd.Answer)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return new ChartDataDto
            {
                ChartLabel = "Answered vs Not Answered",
                Data = stats.Select(s => new ChartDataDto.ChartPoint { Label = s.Status.ToString(), Value = s.Count }).ToList()
            };
        }

        public async Task<OperatorPerformanceDto> GetAverageCallDurationByOperatorAsync()
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

        public async Task<IEnumerable<CallDetailDto>> GetLongestCallsAsync(int count = 10)
        {
            var longestCalls = await _unitOfWork.CallDetails
                .GetAll()
                .OrderByDescending(cd => cd.Length)
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
                })
                .ToListAsync();

            return longestCalls;
        }
    }
}
