namespace Scheduling.Core.FJSP
{
    public class Operation(int id, Dictionary<Machine, long> processingTimes) : IEquatable<Operation>
    {

        public const int SOURCE_ID = 0;
        public const int SINK_ID = Int32.MaxValue;
        
        public int Id { get; set; } = id;

        public int JobId { get; set; }

        public int Index { get; set; }

        public Job Job { get; init; }

        public bool FirstOperation => Index < 1;

        public bool LastOperation => Index + 1 >= Job.Operations.Length;

        private Dictionary<Machine, long> ProcessingTimes { get; } = processingTimes;

        public HashSet<Machine> EligibleMachines { get; } = [];

        public bool Equals(Operation? other) => Id.Equals(other?.Id);

        public long GetProcessingTime(Machine m)
        {
            try
            {
                if (m is null) return Int64.MaxValue;
                if (Id is SINK_ID) return 0;
                if (Id is SOURCE_ID) return Job.ReleaseDate;

                return ProcessingTimes[m];
            }
            catch (Exception)
            {
                return Int64.MaxValue;
            }
        }

        public static Operation Source() => new(SOURCE_ID, []);

        public static Operation Sink() => new(SINK_ID, []);
    }
}
