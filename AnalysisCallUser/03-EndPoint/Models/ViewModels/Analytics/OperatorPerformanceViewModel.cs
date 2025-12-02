namespace AnalysisCallUser._03_EndPoint.Models.ViewModels.Analytics
{
    public class OperatorPerformanceViewModel
    {
        public List<OperatorStat> Operators { get; set; } = new List<OperatorStat>();
    }

    public class OperatorStat
    {
        public string OperatorName { get; set; }
        public int TotalCalls { get; set; }
        public double AverageDuration { get; set; }
        public double AnswerRate { get; set; }
    }
}
