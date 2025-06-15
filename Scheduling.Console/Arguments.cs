using CommandLine;

namespace Scheduling.Console
{
    internal class Arguments
    {
        #region Solver 
        
        [Option('i', "input", Required = true, HelpText = "Instance file path.")]
        public string InstanceFile { get; set; } = "";

        [Option('o', "output", Required = false, HelpText = "Output file path.")]
        public string OutputPath { get; set; } = "";

        [Option('s', "solver", Required = true, HelpText = "Solver algorithm name.")]
        public string SolverName { get; set; } = "";

        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        [Option("parallel", Required = false, HelpText = "Use parallel approach to solve.")]
        public bool UseParallelApproach { get; set; }

        [Option("disable-local-search", Required = false, HelpText = "Disable local search.")]
        public bool DisableLocalSearch { get; set; }
        
        [Option("enable-dorigos-touch", Required = false, HelpText = "Enable Dorigo's parametrization.")]
        public bool EnableDorigosTouch { get; set; }

        [Option("runs", Required = false, HelpText = "# of algorithm runs.")]
        public int Runs { get; set; } = 1;

        [Option("ants", Required = false, HelpText = "ACO: Number of ants.")]
        public int Ants { get; set; } = 300;

        [Option("iterations", Required = false, HelpText = "ACO: Number of iterations.")]
        public int Iterations { get; set; } = 100;

        [Option("lazygens", Required = false, HelpText = "ACO: Number of allowed stagnant generations.")]
        public int AllowedStagnantGenerations { get; set; } = 20;

        #endregion


        #region Hiperparameters 

        [Option("alpha", Required = false, HelpText = "ACO: Alpha parameter.")]
        public double Alpha { get; set; } = 0.9;

        [Option("beta", Required = false, HelpText = "ACO: Beta parameter.")]
        public double Beta { get; set; } = 1.2;

        [Option("rho", Required = false, HelpText = "ACO: Pheromone evaporation rate parameter.")]
        public double Rho { get; set; } = 0.01;

        [Option("phi", Required = false, HelpText = "ACS: Pheromone decay rate parameter.")]
        public double Phi { get; set; } = 0.04;

        [Option("ranksize", Required = false, HelpText = "RBAS: Rank size parameter.")]
        public int RankSize { get; set; } = 10;

        [Option("elitistweight", Required = false, HelpText = "EAS: Elitist reinforcement weight.")]
        public double E { get; set; } = 10;

        [Option("tau0", Required = false, HelpText = "ACO: Initial pheromone amount.")]
        public double Tau0 { get; set; } = 0.001;

        [Option("taumax", Required = false, HelpText = "MMAS: Max pheromone amount.")]
        public double TauMax { get; set; } = 100.000;

        [Option("taumin", Required = false, HelpText = "MMAS: Min pheromone amount.")]
        public double TauMin { get; set; } = 0.001;

        #endregion
    }
}
