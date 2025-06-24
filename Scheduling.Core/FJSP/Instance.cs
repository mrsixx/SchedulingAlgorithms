namespace Scheduling.Core.FJSP
{
    public class Instance(IEnumerable<Job> jobs, Machine[] machines)
    {
        public IEnumerable<Job> Jobs { get; } = jobs;
        
        public Machine[] Machines { get; } = machines;

        public int OperationCount { get; } = jobs.Sum(j => j.Operations.Length);

        public double UpperBound { get;set; } = 0;

        public double TrivialUpperBound { get;set; } = 0;

        public double MachinesPerOperation { get; set; } = 1;

        public List<Operation> OperationsSet => Jobs.SelectMany(j => j.Operations).ToList();
    }
}
