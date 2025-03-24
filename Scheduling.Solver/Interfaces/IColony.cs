using Scheduling.Solver.AntColonyOptimization.Ants;

namespace Scheduling.Solver.Interfaces
{
    public interface IColony<TAnt> where TAnt : BaseAnt
    {
        /// <summary>
        /// Ant whose found best path
        /// </summary>
        TAnt? EmployeeOfTheMonth { get; }

        Dictionary<int, TAnt> IterationBests { get; }

        TAnt BestSoFar { get; }

        void UpdateBestPath(TAnt[] ants);
    }
}
