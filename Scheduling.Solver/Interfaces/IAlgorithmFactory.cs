using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduling.Solver.Interfaces
{
    public interface IAlgorithmFactory
    {
        IFlexibleJobShopSchedulingSolver GetSolverAlgorithm();
    }
}
