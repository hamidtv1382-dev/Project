namespace AnalysisCallUser._01_Domain.Core.DTOs
{
    public class DashboardDto
    {
        public int TotalCalls { get; set; }
        public int AnsweredCalls { get; set; }
        public double AverageCallDuration { get; set; }
        public List<ChartDataDto.ChartPoint> TopCountries { get; set; }
        public List<ChartDataDto.ChartPoint> RecentActivity { get; set; }
    }
}
