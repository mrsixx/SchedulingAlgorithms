// See https://aka.ms/new-console-template for more information
using Scheduling.Console;
using Scheduling.Core.Interfaces;
using Scheduling.Core.Services;
using Scheduling.Solver.AntColonyOptimization;

if (args.Length < 2)
    Console.WriteLine("scheduling.exe instanceFile outputFile");

var instanceFile = args[0];
var outputFile = args[1];
var logger = new Logger();
IGraphBuilderService graphBuilderService = new GraphBuilderService(logger);
IGraphExporterService graphExporterService = new GraphExporterService();

var graph = graphBuilderService.BuildDisjunctiveGraphByBenchmarkFile(instanceFile);
graphExporterService.ExportDisjunctiveGraphToGraphviz(graph, outputFile);

//var graph = graphBuilderService.BuildDisjunctiveGraph(CustomInstances.SampleInstance());
var solution = new AntColonyOptimizationAlgorithmSolver(graph, ants: 1, iterations: 1)
                .Verbose(logger)
                .Solve();

//if(solution.BestSolution != null)
solution.Context.EmployeeOfTheMonth.Log();
graphExporterService.ExportConjunctiveGraphToGraphviz(solution.Context.BestGraph, $"{outputFile}.sol");

Console.ReadKey();
