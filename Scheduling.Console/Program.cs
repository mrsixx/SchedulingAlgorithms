// See https://aka.ms/new-console-template for more information
using CommandLine;
using Scheduling.Benchmarks;
using Scheduling.Benchmarks.Interfaces;
using Scheduling.Console;
using Scheduling.Core.Interfaces;
using Scheduling.Core.Services;
using Scheduling.Solver.AntColonyOptimization;
using Scheduling.Solver.Interfaces;

Parser.Default.ParseArguments<Arguments>(args)
    .WithParsed(opt =>
    {
        ILogger logger = new Logger();
        IGraphExporterService graphExporterService = new GraphExporterService();
        IBenchmarkReaderService benchmarkReaderService = new BenchmarkReaderService(logger);
        IAlgorithmFactory algorithmFactory = new AlgorithmFactory(opt);

        var problemInstance = benchmarkReaderService.ReadInstance(opt.InstanceFile);
        
        var solution = algorithmFactory.GetSolverAlgorithm()
                        .WithLogger(logger, with: opt.Verbose)
                        .Solve(problemInstance);

        solution.Context.EmployeeOfTheMonth.Log();

        if (opt.EnableDebug)
        {
            graphExporterService.ExportDisjunctiveGraphToGraphviz(solution.Context.DisjunctiveGraph, opt.OutputFile);
            graphExporterService.ExportConjunctiveGraphToGraphviz(
                solution.Context.BestGraph,
                solution.Context.BestSoFar.MachineAssignment,
                solution.Context.BestSoFar.StartTimes,
                solution.Context.BestSoFar.CompletionTimes,
                $"{opt.OutputFile}.sol");
        }
    });

Console.ReadKey();
