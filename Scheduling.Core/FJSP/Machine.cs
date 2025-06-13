
namespace Scheduling.Core.FJSP
{
    public class Machine(int id)
    {
        public int Id { get; set; } = id;

        public int Index { get; init; }
        public override int GetHashCode() => Id;

        public override bool Equals(object obj) {
            if(obj is null) return false;
            if (obj is Machine m) return Id == m.Id;
            return false;
        }
    }
}
