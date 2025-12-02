namespace AnalysisCallUser._01_Domain.Core.DTOs
{
    public class ExportHistoryDto
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string ExportType { get; set; }
        public int RecordCount { get; set; }
        public DateTime RequestDate { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsFailed { get; set; }
        public long? FileSize { get; set; }
        public string FilePath { get; set; }
        public string ErrorMessage { get; set; }
    }
}
