using System.ComponentModel.DataAnnotations;

namespace AnalysisCallUser._03_EndPoint.Models.ApiModels
{
    public class NetworkGraphRequest
    {
        public List<string> PhoneNumbers { get; set; } = new List<string>();

        [Range(1, 5, ErrorMessage = "Max depth must be between 1 and 5.")]
        public int MaxDepth { get; set; } = 2;
    }
}
