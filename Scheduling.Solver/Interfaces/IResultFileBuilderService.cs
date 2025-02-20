namespace Scheduling.Solver.Interfaces
{
    public interface IResultFileBuilderService
    {
        void Export(string instanceFile, string solverName, IEnumerable<IFjspSolution> solutions);
    }
}
