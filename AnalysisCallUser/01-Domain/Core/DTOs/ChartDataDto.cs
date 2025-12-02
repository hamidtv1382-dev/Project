namespace AnalysisCallUser._01_Domain.Core.DTOs
{
    public class ChartDataDto
    {
        public class ChartPoint
        {
            public string Label { get; set; }
            public double Value { get; set; }
        }

        public List<ChartPoint> Data { get; set; } = new List<ChartPoint>();
        public string ChartLabel { get; set; }
    }
}
