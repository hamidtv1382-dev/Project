using AnalysisCallUser._03_EndPoint.Models.ViewModels.Dashboard;
using static AnalysisCallUser._01_Domain.Core.DTOs.AnalysisCallUser._01_Domain.Core.DTOs.MapDataDto;

namespace AnalysisCallUser._03_EndPoint.Models.ViewModels.Analytics
{
    public class GeographicAnalysisViewModel
    {
        public List<int> CountryIds { get; set; } = new List<int>();
        public MapViewModel MapData { get; set; }
        public List<MapPoint> Points { get; set; } = new List<MapPoint>();
        public List<int> DistanceData { get; set; } = new List<int>();
        public List<int> ContinentData { get; set; } = new List<int>();
        public List<int> DomesticIntlData { get; set; } = new List<int>();
    }

}
