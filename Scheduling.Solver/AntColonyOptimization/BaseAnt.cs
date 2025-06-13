using Scheduling.Core.FJSP;
using Scheduling.Solver.Models;

namespace Scheduling.Solver.AntColonyOptimization
{
    public abstract class BaseAnt<TSelf> where TSelf : BaseAnt<TSelf>
    {
        public int Id { get; init; }

        public int Generation { get; init; }

        public AntSolution Solution { get; set; } = new AntSolution();

        public AntSolution? ImprovedSolution { get; set; } = null;

        public abstract void WalkAround();

        public abstract void Log();
    }
}
