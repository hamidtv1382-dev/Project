using AnalysisCallUser._01_Domain.Core.Enums;

namespace AnalysisCallUser._01_Domain.Core.Entities
{
    public class DashboardWidget
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string WidgetType { get; set; }
        public string Title { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Configuration { get; set; }
        public bool IsVisible { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public AnalyticsType AnalyticsType { get; set; } 
        public NodeType NodeType { get; set; } 

        public ApplicationUser User { get; set; }
    }
}
