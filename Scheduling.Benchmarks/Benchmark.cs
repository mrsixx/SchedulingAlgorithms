using static System.Reflection.Metadata.BlobBuilder;

namespace Scheduling.Benchmarks
{
    public class Benchmark
    {
        public const long INFINITY = Int64.MaxValue;
        

        public string Name { get; private set; } = "";

        /// <summary>
        /// |T| - Number of jobs
        /// </summary>
        public int JobCount { get; private set; }

        /// <summary>
        /// |M| - Number of machines
        /// </summary>
        public int MachineCount { get; private set; }

        /// <summary>
        /// |O| - Number of tasks
        /// </summary>
        public int OperationCount { get; private set; }

        /// <summary>
        /// Task id of operation O_to for each task o of each job t
        /// </summary>
        public int[][] OperationId { get; private set; } = Array.Empty<int[]>();

        /// <summary>
        /// Number of operations n_t of each job t
        /// </summary>
        public int[] JobOperationCount { get; private set; } = Array.Empty<int>();

        /// <summary>
        /// Processing time p_om of each operation o on each machine m
        /// Obs.: If m can't process o, p_om = INFINITY.
        /// </summary>
        public long[][] ProcessingTime { get; private set; } = Array.Empty<long[]>();

        public long TrivialUpperBound { get; private set; }

        public static Benchmark FromFile(string fileName)
        {
            var benchmark = new Benchmark();
            using StreamReader input = new(fileName);
            var separators = new char[] { '\t', ' ' };
            string[] splitted = input.ReadLine()
                .Split(separators, StringSplitOptions.RemoveEmptyEntries);

            benchmark.Name = fileName;
            benchmark.JobCount = int.Parse(splitted[0]);
            benchmark.MachineCount = int.Parse(splitted[1]);

            benchmark.OperationCount = 0;

            long[][][] processingTime = new long[benchmark.JobCount][][];
            benchmark.OperationId = new int[benchmark.JobCount][];
            benchmark.JobOperationCount = new int[benchmark.JobCount];
            for (int j = 0; j < benchmark.JobCount; ++j)
            {
                splitted = input.ReadLine()
                    .Split(separators, StringSplitOptions.RemoveEmptyEntries);
                benchmark.JobOperationCount[j] = int.Parse(splitted[0]);
                benchmark.OperationId[j] = new int[benchmark.JobOperationCount[j]];
                processingTime[j] = new long[benchmark.JobOperationCount[j]][];
                int k = 1;
                for (int o = 0; o < benchmark.JobOperationCount[j]; ++o)
                {
                    int nbMachinesOperation = int.Parse(splitted[k]);
                    k++;
                    processingTime[j][o] = Enumerable.Repeat(INFINITY, benchmark.MachineCount).ToArray();
                    for (int m = 0; m < nbMachinesOperation; ++m)
                    {
                        int machine = int.Parse(splitted[k]) - 1;
                        long time = long.Parse(splitted[k + 1]);
                        processingTime[j][o][machine] = time;
                        k += 2;
                    }
                    benchmark.OperationId[j][o] = benchmark.OperationCount;
                    benchmark.OperationCount++;
                }
            }

            // Trivial upper bound for the start times of the tasks
            long maxSumProcessingTimes = 0;
            benchmark.ProcessingTime = new long[benchmark.OperationCount][];
            for (int j = 0; j < benchmark.JobCount; ++j)
            {
                long maxProcessingTime = 0;
                for (int o = 0; o < benchmark.JobOperationCount[j]; ++o)
                {
                    int task = benchmark.OperationId[j][o];
                    benchmark.ProcessingTime[task] = new long[benchmark.MachineCount];
                    for (int m = 0; m < benchmark.MachineCount; ++m)
                    {
                        benchmark.ProcessingTime[task][m] = processingTime[j][o][m];
                        if (
                            processingTime[j][o][m] != INFINITY
                            && processingTime[j][o][m] > maxProcessingTime
                        )
                        {
                            maxProcessingTime = processingTime[j][o][m];
                        }
                    }
                    maxSumProcessingTimes += maxProcessingTime;
                }
            }
            //TrivialUpperBound = maxSumProcessingTimes + OperationCount * maxSetup;
            benchmark.TrivialUpperBound = maxSumProcessingTimes;
            return benchmark;
        }
    }
}
