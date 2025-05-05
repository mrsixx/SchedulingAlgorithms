using Scheduling.Core.FJSP;
using Scheduling.Core.Graph;

namespace Scheduling.Core.Interfaces
{
    public interface IGraphExporterService
    {
        void ExportDisjunctiveGraphToGraphviz(DisjunctiveGraphModel graph,  string outputFile);

        void ExportConjunctiveGraphToGraphviz(ConjunctiveGraphModel graph, 
            Dictionary<int, Machine> mu,
            Dictionary<int, double> startTimes,
            Dictionary<int, double> completionTimes,
            string outputFile);
    }
}
