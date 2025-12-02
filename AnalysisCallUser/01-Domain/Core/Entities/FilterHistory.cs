namespace AnalysisCallUser._01_Domain.Core.Entities
{
    public class FilterHistory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string FilterName { get; set; }
        public string FilterParameters { get; set; }
        public int ResultCount { get; set; }

        public ApplicationUser User { get; set; }
    }
}
