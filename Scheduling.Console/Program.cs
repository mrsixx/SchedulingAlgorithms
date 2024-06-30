// See https://aka.ms/new-console-template for more information
using Scheduling.Core.Interfaces;
using Scheduling.Core.Services;

if (args.Length < 2)
    Console.WriteLine("scheduling.exe instanceFile outputFile");

var instanceFile = args[0];
var outputFile = args[1];

IGraphBuilderService graphBuilderService = new GraphBuilderService();
IGraphExporterService graphExporterService = new GraphExporterService();

var graph = graphBuilderService.BuildDisjunctiveGraphByBenchmarkFile(instanceFile);
graphExporterService.ExportDisjunctiveGraphToGraphviz(graph, outputFile);
