using Scheduling.Core.FJSP;
using Scheduling.Solver.AntColonyOptimization.ListSchedulingV3.Model;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV3.Interfaces
{
    public interface IDigraphBuilderService
    {
        DisjunctiveDigraph BuildDisjunctiveDigraph(Instance fjspInstance);

        ConjunctiveDigraph BuildConjunctiveDigraph(DisjunctiveDigraph disjunctiveDigraph, HashSet<DisjunctiveArc> selection);
    }
}
