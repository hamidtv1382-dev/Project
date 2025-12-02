namespace AnalysisCallUser._01_Domain.Core.DTOs
{
    public class MapDataDto
    {
        public class MapPoint
        {
            public string CountryCode { get; set; }
            public string CityName { get; set; }
            public int CallCount { get; set; }
        }

        public List<MapPoint> Points { get; set; }
    }
}
