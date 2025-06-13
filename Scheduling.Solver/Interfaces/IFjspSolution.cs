using System.Diagnostics;
using Scheduling.Core.FJSP;

namespace Scheduling.Solver.Interfaces
{
    public interface IFjspSolution
    {
        
        /// <summary>
        /// Start time by operation Id
        /// </summary>
        Dictionary<int, double> StartTimes { get; }

        /// <summary>
        /// Completion Times by operation Id
        /// </summary>
        Dictionary<int, double> CompletionTimes { get; }

        /// <summary>
        /// Machine Assignment by operation Id
        /// </summary>
        Dictionary<int, int> MachineAssignment { get; }

        public Stopwatch Watch { get; }

        double Makespan { get; }

        void Log();
    }
}
