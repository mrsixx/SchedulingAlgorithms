// See https://aka.ms/new-console-template for more information
using Scheduling.Core.Services;
using Scheduling.Core.Interfaces;
using Scheduling.Solver.Interfaces;
using Scheduling.Solver.AntColonyOptimization;
using Scheduling.Console;
using Scheduling.Core.Benchmark;

if (args.Length < 2)
    Console.WriteLine("scheduling.exe instanceFile outputFile");

var instanceFile = args[0];
var outputFile = args[1];

IGraphBuilderService graphBuilderService = new GraphBuilderService();
IGraphExporterService graphExporterService = new GraphExporterService();

var graph = graphBuilderService.BuildDisjunctiveGraphByBenchmarkFile(instanceFile);
graphExporterService.ExportDisjunctiveGraphToGraphviz(graph, outputFile);

//var graph = graphBuilderService.BuildDisjunctiveGraph(CustomInstances.SampleInstance());
var solution = new AntColonyOptimizationAlgorithmSolver(graph, ants: 100, iterations: 50)
                .Verbose(new Logger())
                .Solve();

//if(solution.BestSolution != null)
solution.Context.EmployeeOfTheMonth.Log();
graphExporterService.ExportConjunctiveGraphToGraphviz(solution.Context.BestGraph, $"{outputFile}.sol");

Console.ReadKey();
