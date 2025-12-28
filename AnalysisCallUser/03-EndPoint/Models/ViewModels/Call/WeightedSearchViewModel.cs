using AnalysisCallUser._03_EndPoint.Controllers;

namespace AnalysisCallUser._03_EndPoint.Models.ViewModels.Call
{
    public class WeightedSearchViewModel
    {
        public WeightedSearchFilterViewModel Filter { get; set; } = new();
        public List<WeightedCallResultViewModel> WeightedResults { get; set; } = new();
        public int TotalPairs { get; set; }
        public int TotalCalls { get; set; }
        public int TotalLength { get; set; }
    }

}
