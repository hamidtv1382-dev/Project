namespace AnalysisCallUser._03_EndPoint.Models.ViewModels.Dashboard
{
    public class MapViewModel
    {
        public List<MapPoint> Points { get; set; }

        public class MapPoint
        {
            public string CountryName { get; set; }
            public string CityName { get; set; }
            public int CallCount { get; set; }
            public double AvgDuration { get; set; }
            public double AnswerRate { get; set; }
        }
    }

}
