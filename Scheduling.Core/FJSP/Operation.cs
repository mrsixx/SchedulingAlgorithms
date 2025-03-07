namespace Scheduling.Core.FJSP
{
    public class Operation(int id, Dictionary<Machine, long> processingTimes) : IEquatable<Operation>
    {

        public const int SOURCE_ID = 0;
        public const int SINK_ID = Int32.MaxValue;
        
        public int Id { get; set; } = id;

        public int JobId { get; set; }

        public Job Job { get; init; }

        private Dictionary<Machine, long> ProcessingTimes { get; } = processingTimes;

        public HashSet<Machine> EligibleMachines { get; } = [];

        public bool Equals(Operation? other) => Id.Equals(other?.Id);

        public double GetProcessingTime(Machine m)
        {
            try
            {
                if (m is null) return 0;
                if (Id is SINK_ID) return 0;
                if (Id is SOURCE_ID) return Convert.ToDouble(Job.ReleaseDate);

                return ProcessingTimes[m];
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static Operation Source() => new(SOURCE_ID, []);

        public static Operation Sink() => new(SINK_ID, []);
    }
}
