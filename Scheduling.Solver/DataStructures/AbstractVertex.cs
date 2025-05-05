namespace Scheduling.Solver.DataStructures
{
    public abstract class AbstractVertex(int id, string label)
    {
        public int Id { get; set; } = id;

        public string Label { get; set; } = label;

        public override string ToString() => Label;
    }
}
