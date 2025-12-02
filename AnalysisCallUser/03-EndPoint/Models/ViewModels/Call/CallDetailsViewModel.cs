using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._01_Domain.Core.Enums;

namespace AnalysisCallUser._03_EndPoint.Models.ViewModels.Call
{
    public class CallDetailsViewModel
    {
        public CallDetailDto _dto { get; set; }
        public CallDetailsViewModel(CallDetailDto dto)
        {
            _dto = dto;
        }
        public string ANumber => _dto.ANumber;
        public string BNumber => _dto.BNumber;
        public DateTime AccountingTime_Date => _dto.AccountingTime_Date;
        public TimeSpan AccountingTime_Time => _dto.AccountingTime_Time;
        public int Length => _dto.Length;
        public string OriginCountryName => _dto.OriginCountryName;
        public string DestCountryName => _dto.DestCountryName;
        public CallAnswerStatus Answer => _dto.Answer;
    }
}
