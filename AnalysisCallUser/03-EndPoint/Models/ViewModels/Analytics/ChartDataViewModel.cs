namespace AnalysisCallUser._03_EndPoint.Models.ViewModels.Analytics
{
    public class ChartDataViewModel
    {
        public string ChartLabel { get; set; }
        public List<ChartPoint> Data { get; set; } = new List<ChartPoint>();
    }

    public class ChartPoint
    {
        public string Label { get; set; }
        public double Value { get; set; }
    }
}
