// See https://aka.ms/new-console-template for more information
using CommandLine;
using Newtonsoft.Json;
using Scheduling.Console;
using Scheduling.Core.Interfaces;
using Scheduling.Core.Services;
using Scheduling.Solver.AntColonyOptimization;

Parser.Default.ParseArguments<Arguments>(args)
    .WithParsed(opt =>
    {
        var logger = opt.Verbose ? new Logger() : null;
        IGraphBuilderService graphBuilderService = new GraphBuilderService(logger);
        IGraphExporterService graphExporterService = new GraphExporterService();

        var graph = graphBuilderService.BuildDisjunctiveGraphByBenchmarkFile(opt.InstanceFile);
        graphExporterService.ExportDisjunctiveGraphToGraphviz(graph, opt.OutputFile);

        //var graph = graphBuilderService.BuildDisjunctiveGraph(CustomInstances.SampleInstance());
        var solution = new IterativeAntColonyOptimizatonSolver(graph, opt.Alpha, opt.Beta,
                opt.Rho, opt.Phi, opt.Tau0, opt.Ants, opt.Iterations)
            .Verbose(logger)
            .Solve();
        
        solution.Context.EmployeeOfTheMonth.Log();
        graphExporterService.ExportConjunctiveGraphToGraphviz(
            solution.Context.BestGraph,
            solution.Context.BestSoFar.MachineAssignment,
            solution.Context.BestSoFar.StartTimes,
            solution.Context.BestSoFar.CompletionTimes,
            $"{opt.OutputFile}.sol");

        var json = JsonConvert.SerializeObject(solution.GetOutput());

    });

Console.ReadKey();
