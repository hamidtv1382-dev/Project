using System.ComponentModel.DataAnnotations;

namespace AnalysisCallUser._03_EndPoint.Models.ApiModels
{
    public class AnalyticsRequest
    {
        [Required]
        public string AnalyticsType { get; set; } // "CallVolume", "Geographic", etc.

        public FilterRequest Filter { get; set; }
    }
}
