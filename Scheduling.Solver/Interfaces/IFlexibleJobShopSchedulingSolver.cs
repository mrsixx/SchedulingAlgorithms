using Scheduling.Core.FJSP;
using Scheduling.Core.Graph;
using Scheduling.Solver.Models;

namespace Scheduling.Solver.Interfaces
{
    public interface IFlexibleJobShopSchedulingSolver
    {
        /// <summary>
        /// Disjunctive graph model representing problem instance
        /// </summary>
        DisjunctiveGraphModel DisjunctiveGraph { get; }

        /// <summary>
        /// Solve a flexible job shop with sequence dependent setup times problem instance 
        /// </summary>
        /// <returns>A object representing solution with machine alocation and start dates of each operation</returns>
        FjspSolution Solve();
    }
}
