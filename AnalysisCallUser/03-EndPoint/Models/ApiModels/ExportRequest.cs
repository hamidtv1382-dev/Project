using System.ComponentModel.DataAnnotations;

namespace AnalysisCallUser._03_EndPoint.Models.ApiModels
{
    public class ExportRequest
    {
        [Required]
        public FilterRequest Filter { get; set; }

        [Required]
        public string ExportType { get; set; } // "CSV", "Excel", etc.
    }
}
