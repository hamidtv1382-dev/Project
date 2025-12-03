namespace AnalysisCallUser._01_Domain.Core.DTOs
{
    namespace AnalysisCallUser._01_Domain.Core.DTOs
    {
        public class MapDataDto
        {
            public List<MapPoint> Points { get; set; } = new List<MapPoint>();

            public class MapPoint
            {
                public string CountryName { get; set; }
                public string CountryCode { get; set; }  
                public string CityName { get; set; }
                public int CallCount { get; set; }
                public double AvgDuration { get; set; }  
                public double AnswerRate { get; set; }   
            }
        }
    }


}
