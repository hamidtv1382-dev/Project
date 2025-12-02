using AnalysisCallUser._01_Domain.Core.Enums;

namespace AnalysisCallUser._01_Domain.Core.Entities
{
    public class CallDetail
    {
        public int DetailID { get; set; }
        public string ANumber { get; set; }
        public string BNumber { get; set; }
        public DateTime AccountingTime_Date { get; set; }
        public TimeSpan AccountingTime_Time { get; set; }
        public int Length { get; set; }
        public int OriginCountryID { get; set; }
        public int OriginCityID { get; set; }
        public int OriginOperatorID { get; set; }
        public int DestCountryID { get; set; }
        public int DestCityID { get; set; }
        public int DestOperatorID { get; set; }
        public int TypeID { get; set; }
        public string AccountingTime_SH_Date { get; set; }
        public string AccountingTime_SH_Time { get; set; }
        public CallAnswerStatus Answer { get; set; } 
        //public CallDirection Direction { get; set; }

        public Country OriginCountry { get; set; }
        public City OriginCity { get; set; }
        public Operator OriginOperator { get; set; }
        public Country DestCountry { get; set; }
        public City DestCity { get; set; }
        public Operator DestOperator { get; set; }
        public CallType CallType { get; set; }
    }

}
