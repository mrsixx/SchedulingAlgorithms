namespace Scheduling.Core.FJSP
{
    public class MachinePool
    {
        public MachinePool(int id)
        {
            Id = id;
        }

        public int Id { get; set; }
        public List<Machine> Machines { get; } = new List<Machine>();
    }
}
