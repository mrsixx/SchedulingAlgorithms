using System.Diagnostics;
using Scheduling.Core.Graph;
using Scheduling.Solver.AntColonyOptimization.ListSchedulingV1;
using Scheduling.Solver.Interfaces;

namespace Scheduling.Solver.AntColonyOptimization
{
    public class Colony(DisjunctiveGraphModel disjunctiveGraph) : IColony<AntV1>
    {       
        /// <summary>
        /// Ant whose found best path
        /// </summary>
        public AntV1? EmployeeOfTheMonth { get; private set; }

        public Dictionary<int, AntV1> IterationBests { get; } = [];

        public AntV1 BestSoFar => IterationBests.MinBy(a => a.Value.Makespan).Value;


        public DisjunctiveGraphModel DisjunctiveGraph { get; } = disjunctiveGraph;

        public ConjunctiveGraphModel BestGraph { get; private set; }

        public int LastProductiveGeneration { get; private set; } = 0;

        public Stopwatch Watch { get; private set; } = new();
        
        public void UpdateBestPath(AntV1[] ants)
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
