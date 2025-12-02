namespace AnalysisCallUser._03_EndPoint.Models.ViewModels.Analytics
{
    using System;

    public class AnalyticsFilterViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? CountryID { get; set; }
        public int? OperatorID { get; set; }
    }
}
