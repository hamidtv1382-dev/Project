namespace AnalysisCallUser._03_EndPoint.Models.ViewModels.Analytics
{
    public class CallPatternViewModel
    {
        public ChartDataViewModel HourlyCallVolume { get; set; }
        public ChartDataViewModel DailyCallVolume { get; set; }
        public ChartDataViewModel WeeklyCallVolume { get; set; }
        public ChartDataViewModel MonthlyCallVolume { get; set; }

    }
}
