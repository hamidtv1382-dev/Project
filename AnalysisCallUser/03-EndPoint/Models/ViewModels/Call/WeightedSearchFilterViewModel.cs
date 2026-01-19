using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._01_Domain.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace AnalysisCallUser._03_EndPoint.Models.ViewModels.Call
{
    public class WeightedCallResultViewModel
    {
        public string ANumber { get; set; }
        public string BNumber { get; set; }
        public int Weight { get; set; }
        public int TotalLength { get; set; }
        public double AverageLength { get; set; }
        public string SearchType { get; set; }

        // فقط خواندنی
        public string TotalLengthFormatted { get; set; }
        public string AverageLengthFormatted { get; set; }

        private string FormatTime(int seconds)
        {
            if (seconds < 60) return $"{seconds} ثانیه";
            if (seconds < 3600) return $"{seconds / 60} دقیقه و {seconds % 60} ثانیه";
            return $"{seconds / 3600} ساعت و {(seconds % 3600) / 60} دقیقه";
        }
    }

    public class WeightedSearchFilterViewModel
    {
        [Display(Name = "تاریخ شروع")]
        public string StartDate { get; set; }

        [Display(Name = "تاریخ پایان")]
        public string EndDate { get; set; }

        [Display(Name = "شماره‌های مبدأ")]
        public string SourceNumbersText { get; set; }

        [Display(Name = "شماره‌های مقصد")]
        public string DestNumbersText { get; set; }

        [Display(Name = "حداقل تعداد تماس")]
        [Range(1, 1000, ErrorMessage = "حداقل تعداد تماس باید بین 1 تا 1000 باشد")]
        public int MinWeight { get; set; } = 1;

        [Display(Name = "نمایش جفت معکوس")]
        public bool IncludeReversePairs { get; set; } = true;

        [Display(Name = "فقط تماس‌های پاسخ‌داده‌شده")]
        public bool IncludeAnsweredCallsOnly { get; set; } = true;

        [Display(Name = "حالت جستجو")]
        public WeightedSearchMode SearchMode { get; set; } = WeightedSearchMode.Auto;
    }
}