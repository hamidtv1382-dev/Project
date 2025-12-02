namespace AnalysisCallUser._01_Domain.Core.Entities
{
    public class CallType
    {
        public int TypeID { get; set; }
        public string TypeName { get; set; }

        public ICollection<CallDetail> CallDetails { get; set; }
    }
}
