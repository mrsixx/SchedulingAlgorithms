using QuikGraph;
using Scheduling.Core.Extensions;
using Scheduling.Core.Graph;
using Scheduling.Core.Interfaces;
using Scheduling.Solver.Extensions;

namespace Scheduling.Solver.AntColonyOptimization
{
    internal class Colony
    {
        /// <summary>
        /// Ant whose found best path
        /// </summary>
        public Ant? EmployeeOfTheMonth { get; private set; }
        public List<Conjunction> BestPath { get; private set; } = new List<Conjunction>();

        public void UpdateBestPath(Ant[] ants)
        {
            foreach (var ant in ants)
            {
                var bestPathIsEmpty = BestPath.IsEmpty();
                var antFoundBetterPath = ant.PathDistance < BestPath.CalculateDistance();
                if (bestPathIsEmpty || antFoundBetterPath)
                {
                    BestPath = ant.Path;
                    EmployeeOfTheMonth = ant;
                }
            }
        }
    }
}
