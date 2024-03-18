using QuickGraph.Graphviz;
using QuickGraph.Graphviz.Dot;
using Scheduling.Core.Graph.Interfaces;

namespace Scheduling.Core.Graph
{
    public class GraphExporter : IGraphExporter
    {
        public void ExportTableGraphToGraphviz(DisjunctiveGraphModel graph, string dir, string filename)
        {
            void exportAlgorithm(GraphvizAlgorithm<OperationVertex, VertexEdge> graphviz)
            {
                graphviz.GraphFormat.IsCompounded = true;
                graphviz.CommonVertexFormat.Font = new GraphvizFont("Arial", 8);
                graphviz.CommonVertexFormat.FixedSize = false;
                graphviz.CommonVertexFormat.FillColor = GraphvizColor.White;
                graphviz.CommonVertexFormat.FontColor = GraphvizColor.Black;
                graphviz.CommonVertexFormat.Shape = GraphvizVertexShape.Circle;
                graphviz.CommonEdgeFormat.Font = new GraphvizFont("Arial", 3);
                graphviz.CommonEdgeFormat.Label.Angle = 0;
                graphviz.CommonEdgeFormat.TailArrow = new GraphvizArrow(GraphvizArrowShape.Normal);

                graphviz.FormatVertex += (sender, args) =>
                {
                    if (args.Vertex.Id == 0)
                        args.VertexFormatter.Label = "src";
                    else if (args.Vertex.Id == Int32.MaxValue)
                        args.VertexFormatter.Label = "sink";
                    else
                        args.VertexFormatter.Label = args.Vertex.Id.ToString();

                    args.VertexFormatter.ToolTip = "banana";
                };

                graphviz.FormatEdge += (sender, args) =>
                {
                    //args.EdgeFormat.Label.Value = args.Edge.;
                };

                var location = Path.Combine(dir, $"{filename}.dot");
                graphviz.Generate(new FileDotEngine(), location);
            }
            graph.ToGraphviz(exportAlgorithm);
        }
    }

    public class FileDotEngine : IDotEngine
    {
        public string Run(GraphvizImageType imageType, string dot, string outputFileName)
        {
            using (StreamWriter writer = new StreamWriter(outputFileName))
                writer.Write(dot);

            return Path.GetFileName(outputFileName);
        }
    }
}
