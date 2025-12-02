namespace AnalysisCallUser._01_Domain.Core.DTOs
{
    public class NetworkDto
    {
        public class Node
        {
            public string Id { get; set; }
            public string Label { get; set; }
            public int Size { get; set; }
        }

        public class Edge
        {
            public string From { get; set; }
            public string To { get; set; }
            public int Weight { get; set; }
        }

        public List<Node> Nodes { get; set; } = new List<Node>();
        public List<Edge> Edges { get; set; } = new List<Edge>();
    }
}
