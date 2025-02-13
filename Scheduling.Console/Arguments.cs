using CommandLine;

namespace Scheduling.Console
{
    internal class Arguments
    {
        [Option('i', "input", Required = true, HelpText = "Instance file path.")]
        public string InstanceFile { get; set; } = "";

        [Option('o', "output", Required = true, HelpText = "Output file path.")]

        public string OutputFile { get; set; } = "";

        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        [Option("alpha", Required = false, HelpText = "ACS: Alpha parameter.")]
        public double Alpha { get; set; } = 0.9;

        [Option("beta", Required = false, HelpText = "ACS: Beta parameter.")]
        public double Beta { get; set; } = 1.2;

        [Option("rho", Required = false, HelpText = "ACS: Pheromone evaporation rate parameter.")]
        public double Rho { get; set; } = 0.01;

        [Option("phi", Required = false, HelpText = "ACS: Pheromone decay rate parameter.")]
        public double Phi { get; set; } = 0.04;

        [Option("tau0", Required = false, HelpText = "ACS: Initial pheromone amount.")]
        public double Tau0 { get; set; } = 0.001;

        [Option("ants", Required = false, HelpText = "ACS: Number of ants.")]
        public int Ants { get; set; } = 300;

        [Option("iterations", Required = false, HelpText = "ACS: Number of iterations.")]
        public int Iterations { get; set; } = 100;
    }
}
