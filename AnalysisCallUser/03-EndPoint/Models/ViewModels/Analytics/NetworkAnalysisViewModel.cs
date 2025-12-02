using AnalysisCallUser._03_EndPoint.Models.ViewModels.Dashboard;

namespace AnalysisCallUser._03_EndPoint.Models.ViewModels.Analytics
{
    public class NetworkAnalysisViewModel
    {
        public List<string> PhoneNumbers { get; set; } = new List<string>();
        public int MaxDepth { get; set; } = 2;
        public NetworkViewModel NetworkGraph { get; set; }
    }
}
