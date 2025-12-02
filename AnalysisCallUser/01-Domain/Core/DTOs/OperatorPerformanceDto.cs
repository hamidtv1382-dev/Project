namespace AnalysisCallUser._01_Domain.Core.DTOs
{
    public partial class OperatorPerformanceDto
    {
        public class OperatorStat
        {
            public string OperatorName { get; set; }
            public int TotalCalls { get; set; }
            public double AverageDuration { get; set; }
            public double AnswerRate { get; set; }
        }

        public List<OperatorStat> Operators { get; set; } = new List<OperatorStat>();
    }
}
