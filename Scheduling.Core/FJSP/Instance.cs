namespace Scheduling.Core.FJSP
{
    public class Instance(IEnumerable<Job> jobs, IEnumerable<Machine> machines)
    {
        public IEnumerable<Job> Jobs { get; } = jobs;
        
        public IEnumerable<Machine> Machines { get; } = machines;

    }
}
