namespace Scheduling.Solver.DataStructures
{
    public abstract class Arc(AbstractVertex tail, AbstractVertex head)
    {
        public AbstractVertex Tail { get; set; } = tail;

        public AbstractVertex Head { get; set; } = head;

        public double Weight { get; set; }

        public void Deconstruct(out AbstractVertex tail, out AbstractVertex head)
        {
            tail = Tail;
            head = Head;
        }
    }
}
