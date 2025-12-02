namespace AnalysisCallUser._03_EndPoint.Models.ViewModels.Analytics
{
    public class TimeAnalysisViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ChartDataViewModel CallVolumeOverTime { get; set; }
        public List<double> SeasonalData { get; set; } = new List<double>();

    }
}
