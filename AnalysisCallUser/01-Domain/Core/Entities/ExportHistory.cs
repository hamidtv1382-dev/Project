using AnalysisCallUser._01_Domain.Core.Enums;

namespace AnalysisCallUser._01_Domain.Core.Entities
{
    public class ExportHistory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public ExportType ExportType { get; set; }
        public string FilterParameters { get; set; }
        public int RecordCount { get; set; }
        public string FilePath { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsSuccessful { get; set; }
        public string ErrorMessage { get; set; }

        public ApplicationUser User { get; set; }
    }
}
