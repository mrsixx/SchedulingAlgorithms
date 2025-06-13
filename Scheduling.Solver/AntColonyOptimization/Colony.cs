using Scheduling.Solver.Interfaces;
using System.Diagnostics;

namespace Scheduling.Solver.AntColonyOptimization
{
    public class Colony<TAnt> : IColony<TAnt> where TAnt : BaseAnt<TAnt>
    {
        /// <summary>
        /// Ant whose found best path
        /// </summary>
        public TAnt? EmployeeOfTheMonth { get; private set; }

        public Dictionary<int, TAnt> IterationBests { get; } = [];

        public TAnt BestSoFar => IterationBests.MinBy(a => a.Value.Solution.Makespan).Value;

        public int LastProductiveGeneration { get; private set; } = 0;

        public Stopwatch Watch { get; } = new();

        public void UpdateBestPath(TAnt[] ants)
        {
            foreach (var ant in ants)
            {
                IterationBests.TryAdd(ant.Generation, ant);

                if (ant.Solution.Makespan < IterationBests[ant.Generation].Solution.Makespan)
                    IterationBests[ant.Generation] = ant;

                var hasBestSolution = EmployeeOfTheMonth is not null;
                var antFoundBetterPath = ant.Solution.Makespan < EmployeeOfTheMonth?.Solution.Makespan;
                if (hasBestSolution && !antFoundBetterPath) continue;

                EmployeeOfTheMonth = ant;
                LastProductiveGeneration = ant.Generation;
            }
        }

    }
}
