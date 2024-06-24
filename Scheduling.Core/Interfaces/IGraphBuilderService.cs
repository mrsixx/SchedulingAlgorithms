using Scheduling.Core.Graph;
using Scheduling.Core.Models;

namespace Scheduling.Core.Interfaces
{
    public interface IGraphBuilderService
    {
        DisjunctiveGraphModel BuildDisjunctiveGraphByBenchmarkFile(string benchmarkFile);

        DisjunctiveGraphModel BuildDisjunctiveGraph(List<Job> jobs, List<Machine> machines);
    }
}
