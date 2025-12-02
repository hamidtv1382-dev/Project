namespace AnalysisCallUser._03_EndPoint.Models.ViewModels.Call
{
    public class CallResultViewModel
    {
        public int DetailID { get; set; }
        public string ANumber { get; set; }
        public string BNumber { get; set; }
        public DateTime AccountingTime_Date { get; set; }
        public TimeSpan AccountingTime_Time { get; set; }
        public int Length { get; set; }
        public string OriginCountryName { get; set; }
        public string DestCountryName { get; set; }
        public bool Answer { get; set; }
    }
}
