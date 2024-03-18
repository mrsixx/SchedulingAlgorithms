namespace Scheduling.Core.Graph.Interfaces
{
    public interface IGraphExporter
    {
        void ExportTableGraphToGraphviz(DisjunctiveGraphModel graph, string dir, string filename);
    }
}
