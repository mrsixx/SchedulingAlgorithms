using Scheduling.Core.FJSP;
using Scheduling.Core.Graph;
using Scheduling.Core.Interfaces;
using Scheduling.Solver.Models;

namespace Scheduling.Solver.Interfaces
{
    public interface IFlexibleJobShopSchedulingSolver
    {
        IFlexibleJobShopSchedulingSolver WithLogger(ILogger logger, bool with = false);

        /// <summary>
        /// Solve a flexible job shop with sequence dependent setup times problem instance 
        /// </summary>
        /// <returns>A object representing solution with machine alocation and start dates of each operation</returns>
        IFjspSolution Solve(Instance instance);

    }
}
