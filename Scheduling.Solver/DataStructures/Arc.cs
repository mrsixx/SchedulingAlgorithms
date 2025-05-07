namespace Scheduling.Solver.DataStructures
{
    public abstract class Arc<TVertex>(TVertex tail, TVertex head) where TVertex : AbstractVertex
    {
        public TVertex Tail { get; set; } = tail;

        public TVertex Head { get; set; } = head;

        public double Weight { get; set; }

        public void Deconstruct(out TVertex tail, out TVertex head)
        {
            tail = Tail;
            head = Head;
        }
    }
}
