using Scheduling.Core.Extensions;
using Scheduling.Core.Graph;
using Scheduling.Solver.Extensions;

namespace Scheduling.Solver.AntColonyOptimization
{
    public class Colony
    {
        /// <summary>
        /// Ant whose found best path
        /// </summary>
        public Ant? EmployeeOfTheMonth { get; private set; }

        public ConjunctiveGraphModel BestGraph { get; private set; }

        public int LastProductiveGeneration { get; private set; } = 0;

        public Dictionary<int, Ant> IterationBests { get; private set; } = [];

        public Ant BestSoFar => IterationBests.MinBy(a => a.Value.Makespan).Value;

        public void UpdateBestPath(Ant[] ants)
        {
            foreach (var ant in ants)
            {
                if(!IterationBests.ContainsKey(ant.Generation))
                    IterationBests[ant.Generation] = ant;

                if (ant.Makespan < IterationBests[ant.Generation].Makespan)
                    IterationBests[ant.Generation] = ant;

                var bestGraphIsNull = BestGraph is null;
                var antFoundBetterPath = ant.Makespan < EmployeeOfTheMonth?.Makespan;
                if (bestGraphIsNull || antFoundBetterPath)
                {
                    BestGraph = ant.ConjunctiveGraph;
                    EmployeeOfTheMonth = ant;
                    LastProductiveGeneration = ant.Generation;
                }
            }
        }
    }
}
