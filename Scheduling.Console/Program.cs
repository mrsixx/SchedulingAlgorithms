// See https://aka.ms/new-console-template for more information
using CommandLine;
using Scheduling.Benchmarks;
using Scheduling.Benchmarks.Interfaces;
using Scheduling.Console;
using Scheduling.Core.Interfaces;
using Scheduling.Solver.Interfaces;
using Scheduling.Solver.Services;

Parser.Default.ParseArguments<Arguments>(args)
    .WithParsed(opt =>
    {
        ILogger logger = new Logger();

        logger.Log($"Processador lógico: {Environment.ProcessorCount}");

        ThreadPool.GetAvailableThreads(out int workerThreads, out int ioThreads);
        ThreadPool.GetMaxThreads(out int maxWorker, out int maxIO);

        logger.Log($"ThreadPool disponível: {workerThreads}/{maxWorker} (Worker)");
        logger.Log($"ThreadPool disponível: {ioThreads}/{maxIO} (IO)");

        IBenchmarkReaderService benchmarkReaderService = new BenchmarkReaderService(logger);
        IAlgorithmFactory algorithmFactory = new AlgorithmFactory(opt);
        IResultFileBuilderService resultFileBuilderService = new ResultFileBuilderService();

        var problemInstance = benchmarkReaderService.ReadInstance(opt.InstanceFile);

        var solver = algorithmFactory.GetSolverAlgorithm()
                        .WithLogger(logger, with: opt.Verbose);

        Console.WriteLine("|--------------------------------------------------------------------------|");
        List<IFjspSolution> solutions = [];
        for (int i = 0; i < opt.Runs; i++)
        {
            Console.WriteLine($"Run {i + 1}/{opt.Runs}");
            var solution = solver.Solve(problemInstance);
            solution.Log();
            solutions.Add(solution);
        }
        Console.WriteLine("|--------------------------------------------------------------------------|");
        resultFileBuilderService.Export(opt.InstanceFile, opt.SolverName, opt.UseParallelApproach, solutions, outputDir: opt.OutputPath);
        resultFileBuilderService.ExportSolution(
                opt.InstanceFile, opt.SolverName,
                opt.UseParallelApproach, solutions.MinBy(s => s.Makespan),
                outputDir: opt.OutputPath);
    });