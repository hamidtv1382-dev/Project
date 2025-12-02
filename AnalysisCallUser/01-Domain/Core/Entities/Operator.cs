namespace AnalysisCallUser._01_Domain.Core.Entities
{

    public class Operator
    {
        public int OperatorID { get; set; }
        public string OperatorName { get; set; }

        public ICollection<CallDetail> OriginCallDetails { get; set; }
        public ICollection<CallDetail> DestCallDetails { get; set; }
    }
}


