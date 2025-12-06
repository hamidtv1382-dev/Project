using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._01_Domain.Core.Enums;

namespace AnalysisCallUser._03_EndPoint.Models.ViewModels.Call
{
    public class CallDetailsViewModel
    {
        public int DetailID { get; set; }
        public string? ANumber { get; set; }
        public string? BNumber { get; set; }
        public DateTime AccountingTime { get; set; }
        public int? Length { get; set; }
        public string? OriginCountryName { get; set; }
        public string? DestCountryName { get; set; }

        // +++ فیلدهای جدید اضافه شده +++
        public string? OriginOperatorName { get; set; }
        public string? DestOperatorName { get; set; }

        public CallAnswerStatus Answer { get; set; }

        // سازنده پیش‌فرض
        public CallDetailsViewModel() { }

        // سازنده برای پر کردن ویومدل از DTO
        public CallDetailsViewModel(CallDetailDto dto)
        {
            DetailID = dto.DetailID;
            ANumber = dto.ANumber;
            BNumber = dto.BNumber;
            AccountingTime = dto.AccountingTime;
            Length = dto.Length;
            OriginCountryName = dto.OriginCountryName;
            DestCountryName = dto.DestCountryName;

            // +++ انتقال مقادیر اپراتورها از DTO +++
            OriginOperatorName = dto.OriginOperatorName;
            DestOperatorName = dto.DestOperatorName;

            Answer = dto.Answer;
        }
    }
}
