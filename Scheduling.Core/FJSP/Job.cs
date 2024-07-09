namespace Scheduling.Core.FJSP
{
    public class Job(int id)
    {
        public Job(int id, DateTime releaseDate) : this(id)
        {
            ReleaseDate = releaseDate;
        }

        public int Id { get; set; } = id;

        public decimal Weight { get; set; }

        public DateTime ReleaseDate { get; set; } = DateTime.Now;

        public DateTime DueDate { get; set; }

        public List<Operation> Operations { get; } = [];
    }
}
