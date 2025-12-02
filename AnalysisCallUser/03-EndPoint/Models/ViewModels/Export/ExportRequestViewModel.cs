using AnalysisCallUser._01_Domain.Core.Enums;
using AnalysisCallUser._03_EndPoint.Models.ViewModels.Call;
using System.ComponentModel.DataAnnotations;

namespace AnalysisCallUser._03_EndPoint.Models.ViewModels.Export
{
    public class ExportRequestViewModel
    {
        [Required(ErrorMessage = "Filter is required.")]
        public CallFilterViewModel Filter { get; set; }

        [Required(ErrorMessage = "Export type is required.")]
        public ExportType ExportType { get; set; } // "CSV" or "Excel"

        [Range(1, 100000, ErrorMessage = "Record count must be between 1 and 100,000.")]
        public int TotalRecords { get; set; }

    }
}
