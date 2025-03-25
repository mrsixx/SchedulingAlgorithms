using Scheduling.Core.Graph;
using Scheduling.Solver.Interfaces;
using System.Diagnostics;

namespace Scheduling.Solver.AntColonyOptimization.ListSchedulingV1
{
    public class ColonyV1<TAnt>(DisjunctiveGraphModel disjunctiveGraph) : IColony<TAnt> where TAnt : BaseAnt<TAnt>
    {
        /// <summary>
        /// Ant whose found best path
        /// </summary>
        public TAnt EmployeeOfTheMonth { get; private set; }

        public Dictionary<int, TAnt> IterationBests { get; } = [];

        public TAnt BestSoFar => IterationBests.MinBy(a => a.Value.Makespan).Value;


        public ConjunctiveGraphModel BestGraph { get; private set; }

        public int LastProductiveGeneration { get; private set; } = 0;

        public Stopwatch Watch { get; } = new();

        public void UpdateBestPath(TAnt[] ants)
        {
            foreach (var ant in ants)
            {
                IterationBests.TryAdd(ant.Generation, ant);

                if (ant.Makespan < IterationBests[ant.Generation].Makespan)
                    IterationBests[ant.Generation] = ant;

                var hasBestGraph = BestGraph is not null;
                var antFoundBetterPath = ant.Makespan < EmployeeOfTheMonth?.Makespan;
                if (hasBestGraph && !antFoundBetterPath) continue;

                //BestGraph = ant.ConjunctiveGraph;
                EmployeeOfTheMonth = ant;
                LastProductiveGeneration = ant.Generation;
            }
        }
    }
}
