namespace Scheduling.Core.Models
{
    public class Operation
    {
        public Operation(int id, decimal processingTime)
        {
            Id = id;
            ProcessingTime = processingTime;
        }

        public int Id { get; set; }

        public decimal ProcessingTime { get; set; }
    }
}
