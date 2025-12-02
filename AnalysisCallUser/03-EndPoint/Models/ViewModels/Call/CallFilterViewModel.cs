using AnalysisCallUser._01_Domain.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace AnalysisCallUser._03_EndPoint.Models.ViewModels.Call
{
    public class CallFilterViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string ANumber { get; set; }
        public string BNumber { get; set; }
        public int? OriginCountryID { get; set; }
        public int? DestCountryID { get; set; }
        public int? OriginCityID { get; set; }
        public int? DestCityID { get; set; }
        public int? OriginOperatorID { get; set; }
        public int? DestOperatorID { get; set; }
        public int? TypeID { get; set; }
        public CallAnswerStatus? Answer { get; set; }

        [Range(1, 1000, ErrorMessage = "Page size must be between 1 and 1000.")]
        public int PageSize { get; set; } = 50;
        public int Page { get; set; } = 1;
    }
}
