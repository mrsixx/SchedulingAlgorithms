// See https://aka.ms/new-console-template for more information
using CommandLine;
using Scheduling.Benchmarks;
using Scheduling.Benchmarks.Interfaces;
using Scheduling.Console;
using Scheduling.Core.Interfaces;
using Scheduling.Core.Services;
using Scheduling.Solver.Interfaces;
using Scheduling.Solver.Services;

Parser.Default.ParseArguments<Arguments>(args)
    .WithParsed(opt =>
    {
        ILogger logger = new Logger();
        IGraphExporterService graphExporterService = new GraphExporterService();
        IBenchmarkReaderService benchmarkReaderService = new BenchmarkReaderService(logger);
        IAlgorithmFactory algorithmFactory = new AlgorithmFactory(opt);
        IResultFileBuilderService resultFileBuilderService = new ResultFileBuilderService();

        var problemInstance = benchmarkReaderService.ReadInstance(opt.InstanceFile);
    
        var solver = algorithmFactory.GetSolverAlgorithm()
                        .WithLogger(logger, with: opt.Verbose);


        List<IFjspSolution> solutions = [];
        for (int i = 0; i < opt.Runs; i++)
        {
            Console.WriteLine($"Run {i+1}/{opt.Runs}");
            var solution = solver.Solve(problemInstance);
                solution.Log();
                solutions.Add(solution);
        }

        resultFileBuilderService.Export(opt.InstanceFile, opt.SolverName, opt.UseParallelApproach, solutions, outputDir: opt.OutputPath);
        resultFileBuilderService.ExportSolution(
                opt.InstanceFile, opt.SolverName, 
                opt.UseParallelApproach, solutions.MinBy(s => s.Makespan), 
                outputDir: opt.OutputPath);
    });