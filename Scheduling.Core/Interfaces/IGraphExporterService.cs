using Scheduling.Core.FJSP;
using Scheduling.Core.Graph;

namespace Scheduling.Core.Interfaces
{
    public interface IGraphExporterService
    {
        void ExportDisjunctiveGraphToGraphviz(DisjunctiveGraphModel graph,  string outputFile);

        void ExportConjunctiveGraphToGraphviz(ConjunctiveGraphModel graph, 
            Dictionary<Operation, Machine> mu,
            Dictionary<Operation, double> startTimes,
            Dictionary<Operation, double> completionTimes,
            string outputFile);
    }
}
