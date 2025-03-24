
namespace Scheduling.Solver.Interfaces
{
    public interface IAlgorithmFactory
    {
        IFlexibleJobShopSchedulingSolver GetSolverAlgorithm();
    }
}
