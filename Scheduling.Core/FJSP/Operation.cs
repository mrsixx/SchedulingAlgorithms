namespace Scheduling.Core.FJSP
{
    public class Operation : IEquatable<Operation>
    {
        public Operation(int id, Func<Machine, double> processingTime)
        {
            Id = id;
            ProcessingTime = processingTime;
        }

        public int Id { get; set; }

        public Func<Machine, double> ProcessingTime { get; set; }

        public List<Machine> EligibleMachines { get; } = new List<Machine>();

        public bool Equals(Operation? other) => Id.Equals(other?.Id);
    }
}
