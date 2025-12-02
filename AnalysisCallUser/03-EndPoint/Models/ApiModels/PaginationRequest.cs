using System.ComponentModel.DataAnnotations;

namespace AnalysisCallUser._03_EndPoint.Models.ApiModels
{
    public class PaginationRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0.")]
        public int Page { get; set; } = 1;

        [Range(1, 1000, ErrorMessage = "Page size must be between 1 and 1000.")]
        public int PageSize { get; set; } = 50;
    }
}
