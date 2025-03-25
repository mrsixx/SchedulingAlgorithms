using Scheduling.Solver.AntColonyOptimization;
using System.Diagnostics;

namespace Scheduling.Solver.Interfaces
{
    public interface IColony<TAnt> where TAnt : BaseAnt<TAnt>
    {
        /// <summary>
        /// Ant whose found best path
        /// </summary>
        TAnt? EmployeeOfTheMonth { get; }

        Dictionary<int, TAnt> IterationBests { get; }

        TAnt BestSoFar { get; }


        Stopwatch Watch { get; }

        int LastProductiveGeneration { get; }

        void UpdateBestPath(TAnt[] ants);
    }
}
