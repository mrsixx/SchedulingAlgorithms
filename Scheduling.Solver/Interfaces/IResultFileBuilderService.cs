namespace Scheduling.Solver.Interfaces
{
    public interface IResultFileBuilderService
    {
        void Export(string instanceFile, string solverName, bool parallelApproach, IEnumerable<IFjspSolution> solutions, string outputDir = "");
    }
}
