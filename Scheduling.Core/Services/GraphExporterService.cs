using QuikGraph;
using QuikGraph.Graphviz;
using QuikGraph.Graphviz.Dot;
using Scheduling.Core.Graph;
using Scheduling.Core.Interfaces;
using System.Drawing;
using System.Text.RegularExpressions;

namespace Scheduling.Core.Services
{
    public class GraphExporterService : IGraphExporterService
    {
        public void ExportDisjunctiveGraphToGraphviz(DisjunctiveGraphModel graph, string outputFile)
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

                var colorDictionary = new Dictionary<int, GraphvizColor>();
                graphviz.FormatVertex += (sender, args) =>
                {
                    if (args.Vertex.IsSourceNode)
                        args.VertexFormat.Label = "⊗";
                    else if (args.Vertex.IsSinkNode)
                        args.VertexFormat.Label = "⊥";
                    else
                        args.VertexFormat.Label = args.Vertex.Id.ToString();

                    //args.VertexFormatter.ToolTip = "banana";
                };

                graphviz.FormatEdge += (sender, args) =>
                {
                    if (args.Edge is Disjunction edge)
                    {
                        args.EdgeFormat.TailArrow = new GraphvizArrow(GraphvizArrowShape.None);
                        args.EdgeFormat.HeadArrow = new GraphvizArrow(GraphvizArrowShape.None);
                        args.EdgeFormat.Style = GraphvizEdgeStyle.Dashed;

                        if (!colorDictionary.ContainsKey(edge.MachineId))
                        {
                            byte[] rgb = new byte[4];
                            Random.Shared.NextBytes(rgb);
                            var color = Color.FromKnownColor(KnownColor.DarkRed);
                            colorDictionary.Add(edge.MachineId, new GraphvizColor((byte)color.A, (byte)color.R, (byte)color.G, (byte)color.B));
                        }

                        args.EdgeFormat.StrokeColor  = colorDictionary[edge.MachineId];

                    }
                    else if (args.Edge is Conjunction arc)
                    {
                        args.EdgeFormat.TailArrow = new GraphvizArrow(GraphvizArrowShape.None);
                        args.EdgeFormat.HeadArrow = new GraphvizArrow(GraphvizArrowShape.Normal);

                    }
                };

                //var location = Path.Combine(dir, $"{filename}.dot");
                graphviz.Generate(new FileDotEngine(), outputFile);
            }
            graph.ToGraphviz(exportAlgorithm);
        }
    }

    public class FileDotEngine : IDotEngine
    {
        public string Run(GraphvizImageType imageType, string dot, string outputFileName)
        {
            using (StreamWriter writer = new(outputFileName))
            {
                var content = dot.Replace("graph G", "digraph G");
                content = Regex.Replace(content, "--", "->");
                writer.Write(content);
            }

            return Path.GetFileName(outputFileName);
        }
    }
}
