namespace AnalysisCallUser._03_EndPoint.Models.ViewModels.Dashboard
{
    using System.Collections.Generic;

    public class NetworkViewModel
    {
        public List<Node> Nodes { get; set; } = new List<Node>();
        public List<Edge> Edges { get; set; } = new List<Edge>();
    }

    public class Node
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public int Size { get; set; }
        public string Color { get; set; }
    }

    public class Edge
    {
        public string From { get; set; }
        public string To { get; set; }
        public int Weight { get; set; }
    }
}
