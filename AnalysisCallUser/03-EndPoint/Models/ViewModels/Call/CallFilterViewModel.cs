using AnalysisCallUser._01_Domain.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace AnalysisCallUser._03_EndPoint.Models.ViewModels.Call
{
    public class CallFilterViewModel
    {
        [Display(Name = "تاریخ شروع")]
        public string? StartDate { get; set; }

        [Display(Name = "تاریخ پایان")]
        public string? EndDate { get; set; }

        [Display(Name = "زمان شروع")]
        public TimeSpan? StartTime { get; set; }

        [Display(Name = "زمان پایان")]
        public TimeSpan? EndTime { get; set; }

        [Display(Name = "شماره مبدأ")]
        public string? ANumber { get; set; }

        [Display(Name = "شماره مقصد")]
        public string? BNumber { get; set; }

        [Display(Name = "کشور مبدأ")]
        public int? OriginCountryID { get; set; }

        [Display(Name = "شهر مبدأ")]
        public int? OriginCityID { get; set; }

        [Display(Name = "اپراتور مبدأ")]
        public int? OriginOperatorID { get; set; }

        [Display(Name = "کشور مقصد")]
        public int? DestCountryID { get; set; }

        [Display(Name = "شهر مقصد")]
        public int? DestCityID { get; set; }

        [Display(Name = "اپراتور مقصد")]
        public int? DestOperatorID { get; set; }

        [Display(Name = "نوع تماس")]
        public int? TypeID { get; set; }

        [Display(Name = "وضعیت پاسخ")]
        public CallAnswerStatus? Answer { get; set; }

        [Range(1, 1000, ErrorMessage = "Page size must be between 1 and 1000.")]
        [Display(Name = "تعداد ردیف در هر صفحه")]
        public int PageSize { get; set; } = 50;

        public int Page { get; set; } = 1;
    }
}