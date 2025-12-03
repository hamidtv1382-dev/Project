using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._01_Domain.Core.Enums;

namespace AnalysisCallUser._03_EndPoint.Models.ViewModels.Call
{
    public class CallDetailsViewModel
    {
        public int DetailID { get; set; }
        public string ANumber { get; set; }
        public string BNumber { get; set; }
        public DateTime AccountingTime_Date { get; set; }
        public TimeSpan AccountingTime_Time { get; set; }
        public int Length { get; set; }
        public string OriginCountryName { get; set; }
        public string DestCountryName { get; set; }
        public CallAnswerStatus Answer { get; set; }
        public CallDetailsViewModel() { } // سازنده پیش‌فرض

        public CallDetailsViewModel(CallDetailDto dto)
        {
            DetailID = dto.DetailID;
            ANumber = dto.ANumber;
            BNumber = dto.BNumber;
            AccountingTime_Date = dto.AccountingTime_Date;
            AccountingTime_Time = dto.AccountingTime_Time;
            Length = dto.Length;
            OriginCountryName = dto.OriginCountryName;
            DestCountryName = dto.DestCountryName;
            Answer = dto.Answer;
        }
    }
}
