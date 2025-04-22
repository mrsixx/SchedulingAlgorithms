namespace Scheduling.Core.FJSP
{
    public class Instance(IEnumerable<Job> jobs, IEnumerable<Machine> machines)
    {
        public IEnumerable<Job> Jobs { get; } = jobs;
        
        public IEnumerable<Machine> Machines { get; } = machines;

        public int OperationCount { get; } = jobs.Sum(j => j.Operations.Count);

        public double UpperBound { get;set; } = 0;

        public double TrivialUpperBound { get;set; } = 0;

        public double MachinesPerOperation { get; set; } = 1;
    }
}
