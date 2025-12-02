using AnalysisCallUser._03_EndPoint.Models.ViewModels.Dashboard;

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

    public class MapPoint
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string CountryName { get; set; }
        public int CallCount { get; set; }
        public double AvgDuration { get; set; }
        public double AnswerRate { get; set; }
        public int IncomingCalls { get; set; }
        public int OutgoingCalls { get; set; }
    }
}
