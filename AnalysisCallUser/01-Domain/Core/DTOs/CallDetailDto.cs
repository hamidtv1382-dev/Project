using AnalysisCallUser._01_Domain.Core.Enums;

namespace AnalysisCallUser._01_Domain.Core.DTOs
{
    public class CallDetailDto
    {
        public int DetailID { get; set; }
        public string ANumber { get; set; }
        public string BNumber { get; set; }
        public DateTime AccountingTime_Date { get; set; }
        public TimeSpan AccountingTime_Time { get; set; }
        public int Length { get; set; }
        public string OriginCountryName { get; set; }
        public string OriginCityName { get; set; }
        public string OriginOperatorName { get; set; }
        public string DestCountryName { get; set; }
        public string DestCityName { get; set; }
        public string DestOperatorName { get; set; }
        public string TypeName { get; set; }
        public CallAnswerStatus Answer { get; set; }
    }
}
