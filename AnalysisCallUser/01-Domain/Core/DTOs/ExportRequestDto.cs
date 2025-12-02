using AnalysisCallUser._01_Domain.Core.Enums;

namespace AnalysisCallUser._01_Domain.Core.DTOs
{
    public class ExportRequestDto
    {
        public CallFilterDto Filter { get; set; }
        public ExportType ExportType { get; set; }
        public int TotalRecords { get; set; }
    }
}
