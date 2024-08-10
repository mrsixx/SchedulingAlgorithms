using Scheduling.Core.Graph;

namespace Scheduling.Core.Interfaces
{
    public interface IGraphExporterService
    {
        void ExportDisjunctiveGraphToGraphviz(DisjunctiveGraphModel graph, string outputFile);

        void ExportConjunctiveGraphToGraphviz(ConjunctiveGraphModel graph, string outputFile);
    }
}
