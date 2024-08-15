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
        public void ExportConjunctiveGraphToGraphviz(ConjunctiveGraphModel graph, string outputFile)
        {
            //var colors = Enum.GetValues(typeof(KnownColor))
            //   .Cast<KnownColor>()
            //   .Select(Color.FromKnownColor).ToList();
            void exportAlgorithmConjunctive(GraphvizAlgorithm<Node, Conjunction> graphviz)
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

                };

                graphviz.FormatEdge += (sender, args) =>
                {
                    if (args.Edge is Conjunction arc)
                    {
                        args.EdgeFormat.TailArrow = new GraphvizArrow(GraphvizArrowShape.None);
                        args.EdgeFormat.HeadArrow = new GraphvizArrow(GraphvizArrowShape.Normal);
                        args.EdgeFormat.Label = new GraphvizEdgeLabel { Value = arc.Weight.ToString() };

                    }
                };

                //var location = Path.Combine(dir, $"{filename}.dot");
                graphviz.Generate(new FileDotEngine(), outputFile);
            }
            graph.ToGraphviz(exportAlgorithmConjunctive);
        }

        public void ExportDisjunctiveGraphToGraphviz(DisjunctiveGraphModel graph, string outputFile)
        {
            var colors = Enum.GetValues(typeof(KnownColor))
               .Cast<KnownColor>()
               .Select(Color.FromKnownColor).ToList();
            void exportAlgorithm(GraphvizAlgorithm<Node, BaseEdge> graphviz)
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
                        if(edge.Machine is not null)
                        {
                            if (!colorDictionary.ContainsKey(edge.Machine.Id))
                            {
                                var color = colors[Random.Shared.Next(0, colors.Count)];
                                colors.Remove(color);
                                var graphColor = new GraphvizColor(color.A, color.R, color.G, color.B);
                                colorDictionary.Add(edge.Machine.Id, graphColor);
                            }

                            args.EdgeFormat.StrokeColor  = colorDictionary[edge.Machine.Id];
                            //args.EdgeFormat.Label = new GraphvizEdgeLabel { Value = $"({edge.ProcessingTime.Item1};{edge.ProcessingTime.Item2})", FontColor = colorDictionary[edge.Machine.Id] };
                        }
                    }
                    else if (args.Edge is Conjunction arc)
                    {
                        args.EdgeFormat.TailArrow = new GraphvizArrow(GraphvizArrowShape.None);
                        args.EdgeFormat.HeadArrow = new GraphvizArrow(GraphvizArrowShape.Normal);
                        //args.EdgeFormat.Label = new GraphvizEdgeLabel { Value = arc.Weight.ToString() };

                    }
                };

                //var location = Path.Combine(dir, $"{filename}.dot");
                graphviz.Generate(new FileDotEngine(true), outputFile);
            }
            graph.ToGraphviz(exportAlgorithm);
        }
    }

    public class FileDotEngine : IDotEngine
    {
        public FileDotEngine(bool forceDigraph = false)
        {
            ForceDigraph = forceDigraph;
        }

        public bool ForceDigraph { get; private set; }

        public string Run(GraphvizImageType imageType, string dot, string outputFileName)
        {
            using (StreamWriter writer = new(outputFileName))
            {
                var content = dot;
                if (ForceDigraph)
                {
                    content = content.Replace("graph G", "digraph G");
                    content = Regex.Replace(content, "--", "->");
                }

                writer.Write(content);
            }

            return Path.GetFileName(outputFileName);
        }
    }
}
