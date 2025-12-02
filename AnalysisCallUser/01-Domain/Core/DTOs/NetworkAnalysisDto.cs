namespace AnalysisCallUser._01_Domain.Core.DTOs
{
    public class NetworkAnalysisDto
    {
        public List<string> PhoneNumbers { get; set; } = new List<string>();
        public int MaxDepth { get; set; } = 2;
    }
}
