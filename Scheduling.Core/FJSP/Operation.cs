namespace Scheduling.Core.Models
{
    public class Operation
    {
        public Operation(int id, int machinePoolId, Func<Machine, double> processingTime)
        {
            Id = id;
            MachinePoolId = machinePoolId;
            ProcessingTime = processingTime;
        }

        public int Id { get; set; }

        public Func<Machine, double> ProcessingTime { get; set; }

        public int MachinePoolId { get; set; }
    }
}
