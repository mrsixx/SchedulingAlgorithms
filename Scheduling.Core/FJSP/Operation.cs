namespace Scheduling.Core.FJSP
{
    public class Operation(int id, Func<Machine, double> processingTime) : IEquatable<Operation>
    {

        public const int SOURCE_ID = 0;
        public const int SINK_ID = -1;
        public int Id { get; set; } = id;

        public Func<Machine, double> ProcessingTime { get; set; } = processingTime;

        public List<Machine> EligibleMachines { get; } = [];

        public bool Equals(Operation? other) => Id.Equals(other?.Id);
    }
}
