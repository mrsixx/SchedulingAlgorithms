using static System.Reflection.Metadata.BlobBuilder;

namespace Scheduling.Core.Benchmark
{
    public class BenchmarkReader
    {
        public const long INFINITY = Int64.MaxValue;

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

        public void ReadInstance(string fileName)
        {
            using StreamReader input = new(fileName);
            var separators = new char[] { '\t', ' ' };
            string[] splitted = input.ReadLine()
                .Split(separators, StringSplitOptions.RemoveEmptyEntries);
            JobCount = int.Parse(splitted[0]);
            MachineCount = int.Parse(splitted[1]);

            OperationCount = 0;

            long[][][] processingTime = new long[JobCount][][];
            OperationId = new int[JobCount][];
            JobOperationCount = new int[JobCount];
            for (int j = 0; j < JobCount; ++j)
            {
                splitted = input.ReadLine()
                    .Split(separators, StringSplitOptions.RemoveEmptyEntries);
                JobOperationCount[j] = int.Parse(splitted[0]);
                OperationId[j] = new int[JobOperationCount[j]];
                processingTime[j] = new long[JobOperationCount[j]][];
                int k = 1;
                for (int o = 0; o < JobOperationCount[j]; ++o)
                {
                    int nbMachinesOperation = int.Parse(splitted[k]);
                    k++;
                    processingTime[j][o] = Enumerable.Repeat(INFINITY, MachineCount).ToArray();
                    for (int m = 0; m < nbMachinesOperation; ++m)
                    {
                        int machine = int.Parse(splitted[k]) - 1;
                        long time = long.Parse(splitted[k + 1]);
                        processingTime[j][o][machine] = time;
                        k += 2;
                    }
                    OperationId[j][o] = OperationCount;
                    OperationCount++;
                }
            }

            // Trivial upper bound for the start times of the tasks
            long maxSumProcessingTimes = 0;
            ProcessingTime = new long[OperationCount][];
            for (int j = 0; j < JobCount; ++j)
            {
                long maxProcessingTime = 0;
                for (int o = 0; o < JobOperationCount[j]; ++o)
                {
                    int task = OperationId[j][o];
                    ProcessingTime[task] = new long[MachineCount];
                    for (int m = 0; m < MachineCount; ++m)
                    {
                        ProcessingTime[task][m] = processingTime[j][o][m];
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
            TrivialUpperBound = maxSumProcessingTimes;
        }
    }
}
