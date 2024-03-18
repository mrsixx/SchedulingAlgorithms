namespace Scheduling.Core.Models
{
    public class Job
    {
        public Job(int id)
        {
            Id = id;
            Operations = new List<Operation>();
        }

        public int Id { get; set; }

        public decimal Weight { get; set; }

        public DateTime ReleaseDate { get; set; }

        public DateTime DueDate { get; set; }

        public List<Operation> Operations { get; }
    }
}
