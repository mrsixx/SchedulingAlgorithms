using Scheduling.Core.Graph;

namespace Scheduling.Core.Interfaces
{
    public interface IGraphExporterService
    {
        void ExportTableGraphToGraphviz(DisjunctiveGraphModel graph, string dir, string filename);
    }
}
