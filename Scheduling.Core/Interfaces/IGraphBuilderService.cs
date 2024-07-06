using Scheduling.Core.Graph;
using Scheduling.Core.FJSP;

namespace Scheduling.Core.Interfaces
{
    public interface IGraphBuilderService
    {
        DisjunctiveGraphModel BuildDisjunctiveGraphByBenchmarkFile(string benchmarkFile);

        DisjunctiveGraphModel BuildDisjunctiveGraph(List<Job> jobs);
    }
}
