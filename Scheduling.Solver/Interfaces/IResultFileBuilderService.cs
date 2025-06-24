namespace Scheduling.Solver.Interfaces
{
    public interface IResultFileBuilderService
    {
        void Export(string instanceFile, string solverName, bool parallelApproach, IEnumerable<IFjspSolution> solutions, string outputDir = "");

        void ExportSolution (string instanceFile, string solverName, bool parallelApproach, IFjspSolution solution, bool withLocalSearch, string outputDir = "");
    }
}
