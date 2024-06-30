using QuickGraph;
using QuickGraph.Graphviz;
using QuickGraph.Graphviz.Dot;
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
                        args.VertexFormatter.Label = "⊗";
                    else if (args.Vertex.IsSinkNode)
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

                        if (!colorDictionary.ContainsKey(edge.MachineId))
                        {
                            byte r = Convert.ToByte(Random.Shared.Next(0, 255)), 
                            g = Convert.ToByte(Random.Shared.Next(0, 255)), 
                            b = Convert.ToByte(Random.Shared.Next(0, 255));
                            colorDictionary.Add(edge.MachineId, new GraphvizColor(byte.MinValue, r, g, b));
                        }


                        ///args.EdgeFormatter.StrokeGraphvizColor  = colorDictionary[edge.MachineId];

                    }
                    else if (args.Edge is Conjunction arc)
                    {
                        args.EdgeFormatter.TailArrow = new GraphvizArrow(GraphvizArrowShape.None);
                        args.EdgeFormatter.HeadArrow = new GraphvizArrow(GraphvizArrowShape.Normal);

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
                content = Regex.Replace(content, "GraphvizColor", "color");
                writer.Write(content);
            }

            return Path.GetFileName(outputFileName);
        }
    }
}
