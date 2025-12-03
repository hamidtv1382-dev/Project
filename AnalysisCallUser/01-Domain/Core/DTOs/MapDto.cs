using AnalysisCallUser._01_Domain.Core.DTOs.AnalysisCallUser._01_Domain.Core.DTOs;

namespace AnalysisCallUser._01_Domain.Core.DTOs
{
    public class MapDto
    {
        public List<MapDataDto.MapPoint> Points { get; set; } = new List<MapDataDto.MapPoint>();
    }
}
