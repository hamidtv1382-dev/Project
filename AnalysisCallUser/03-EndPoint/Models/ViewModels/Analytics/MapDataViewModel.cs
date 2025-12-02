namespace AnalysisCallUser._03_EndPoint.Models.ViewModels.Analytics
{
    public class MapDataViewModel
    {
        public List<MapDataViewModel.MapPoint> Points { get; set; } = new List<MapPoint>();

        public class MapPoint
        {
            public string CountryCode { get; set; }
            public string CityName { get; set; }
            public int CallCount { get; set; }
        }
    }
}
