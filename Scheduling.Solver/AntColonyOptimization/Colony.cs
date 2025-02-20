using System.Diagnostics;
using Scheduling.Core.Graph;
using Scheduling.Solver.AntColonyOptimization.Ants;

namespace Scheduling.Solver.AntColonyOptimization
{
    public class Colony(DisjunctiveGraphModel disjunctiveGraph)
    {
        /// <summary>
        /// Ant whose found best path
        /// </summary>
        public BaseAnt? EmployeeOfTheMonth { get; private set; }

        public DisjunctiveGraphModel DisjunctiveGraph { get; } = disjunctiveGraph;

        public ConjunctiveGraphModel BestGraph { get; private set; }

        public int LastProductiveGeneration { get; private set; } = 0;

        public Dictionary<int, BaseAnt> IterationBests { get; private set; } = [];

        public BaseAnt BestSoFar => IterationBests.MinBy(a => a.Value.Makespan).Value;


        public Stopwatch Watch { get; private set; } = new();
        
        public void UpdateBestPath(BaseAnt[] ants)
        {
            foreach (var ant in ants)
            {
                IterationBests.TryAdd(ant.Generation, ant);

                if (ant.Makespan < IterationBests[ant.Generation].Makespan)
                    IterationBests[ant.Generation] = ant;

                var hasBestGraph = BestGraph is not null;
                var antFoundBetterPath = ant.Makespan < EmployeeOfTheMonth?.Makespan;
                if (hasBestGraph && !antFoundBetterPath) continue;
                
                BestGraph = ant.ConjunctiveGraph;
                EmployeeOfTheMonth = ant;
                LastProductiveGeneration = ant.Generation;
            }
        }
    }
}
