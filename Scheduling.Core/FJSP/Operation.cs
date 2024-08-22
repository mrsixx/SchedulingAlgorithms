namespace Scheduling.Core.FJSP
{
    public class Operation(int id, Func<Machine, double> processingTime) : IEquatable<Operation>
    {

        public const int SOURCE_ID = 0;
        public const int SINK_ID = Int32.MaxValue;
        
        public int Id { get; set; } = id;

        public Func<Machine, double> ProcessingTime { get; set; } = processingTime;

        public List<Machine> EligibleMachines { get; } = [];

        public bool Equals(Operation? other) => Id.Equals(other?.Id);

        public double GetProcessingTime(Machine m)
        {
            try
            {
                if (m is null) return 0;
                return ProcessingTime(m);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static Operation Source() => new(SOURCE_ID, (m) => 0);

        public static Operation Sink() => new(SINK_ID, (m) => 0);
    }
}
