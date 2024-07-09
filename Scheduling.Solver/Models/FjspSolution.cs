using Scheduling.Core.Graph;

namespace Scheduling.Solver.Models
{
    public class FjspSolution
    {
        public List<Conjunction> Path { get; } = [];
        public double Makespan { get; internal set; }
    }
}
