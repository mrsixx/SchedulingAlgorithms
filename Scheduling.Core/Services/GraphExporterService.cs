using QuickGraph;
using QuickGraph.Graphviz;
using QuickGraph.Graphviz.Dot;
using Scheduling.Core.Graph;
using Scheduling.Core.Interfaces;

namespace Scheduling.Core.Services
{
    public class GraphExporterService : IGraphExporterService
    {
        public void ExportTableGraphToGraphviz(DisjunctiveGraphModel graph, string dir, string filename)
        {
            void exportAlgorithm(GraphvizAlgorithm<Node, IEdge<Node>> graphviz)
            {
                graphviz.GraphFormat.IsCompounded = true;
                graphviz.CommonVertexFormat.Font = new GraphvizFont("Arial", 8);
                graphviz.CommonVertexFormat.FixedSize = false;
                graphviz.CommonVertexFormat.FillColor = GraphvizColor.White;
                graphviz.CommonVertexFormat.FontColor = GraphvizColor.Black;
                graphviz.CommonVertexFormat.Shape = GraphvizVertexShape.Circle;
                graphviz.CommonEdgeFormat.Font = new GraphvizFont("Arial", 3);
                graphviz.CommonEdgeFormat.Label.Angle = 0;

                graphviz.FormatVertex += (sender, args) =>
                {
                    if (args.Vertex.Id == DisjunctiveGraphModel.SOURCE_ID)
                        args.VertexFormatter.Label = "⊗";
                    else if (args.Vertex.Id == DisjunctiveGraphModel.SINK_ID)
                        args.VertexFormatter.Label = "⊥";
                    else
                        args.VertexFormatter.Label = args.Vertex.Id.ToString();

                    //args.VertexFormatter.ToolTip = "banana";
                };

                graphviz.FormatEdge += (sender, args) =>
                {
                    if (args.Edge is Disjunction edge)
                    {
                        args.EdgeFormatter.TailArrow = new GraphvizArrow(GraphvizArrowShape.None);
                        args.EdgeFormatter.HeadArrow = new GraphvizArrow(GraphvizArrowShape.None);
                        args.EdgeFormatter.Style = GraphvizEdgeStyle.Dashed;

                    }
                    else if (args.Edge is Conjunction arc)
                    {
                        args.EdgeFormatter.TailArrow = new GraphvizArrow(GraphvizArrowShape.None);
                        args.EdgeFormatter.HeadArrow = new GraphvizArrow(GraphvizArrowShape.Normal);

                    }
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
