namespace AnalysisCallUser._03_EndPoint.Models.ViewModels.Dashboard
{
    public class MapViewModel
    {
        public List<MapViewModel.MapPoint> Points { get; set; } = new List<MapPoint>();

        public class MapPoint
        {
            public double Lat { get; set; }
            public double Lng { get; set; }
            public int CallCount { get; set; }
            public string CountryName { get; set; }
        }
    }
}
