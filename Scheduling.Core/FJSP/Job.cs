namespace Scheduling.Core.FJSP
{
    public class Job(int id)
    {
        public Job(int id, DateTime releaseDate) : this(id)
        {
            ReleaseDate = releaseDate;
        }

        public int Id { get; set; } = id;

        public DateTime ReleaseDate { get; set; } = DateTime.Now;

        public List<Operation> Operations { get; } = [];
    }
}
