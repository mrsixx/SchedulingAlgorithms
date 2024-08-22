using Scheduling.Core.FJSP;
using static Scheduling.Core.Enums.DirectionEnum;

namespace Scheduling.Core.Graph
{
    public class Orientation : AntEdge
    {
        public Orientation(Disjunction disjunction, Direction fixedDirection) 
        {
            Source = disjunction.Source;
            Target = disjunction.Target;
            if(fixedDirection == Direction.TargetToSource)
            {
                (Source, Target) = (Target, Source);
            }
            
            Machine = disjunction.Machine;
            AssociatedDisjunction = disjunction;
            FixedDirection = fixedDirection;
        }

        public override Node Source { get; }

        public override Node Target { get; }

        public double Weight
        {
            get
            {
                return Source.Operation.GetProcessingTime(Machine);
            }
        }

        public Machine Machine { get; set; }

        public override string Log => $"{Source.Id} -[{Weight}]-> {Target.Id}";

        public Disjunction AssociatedDisjunction { get; private set; }

        public Direction FixedDirection { get; private set; }


        public override void EvaporatePheromone(double rate)
        {
            AssociatedDisjunction.EvaporatePheromone(rate);
        }

        public void DepositPheromone(double amount)
        {
            AssociatedDisjunction.DepositPheromone(amount, FixedDirection);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;

            if (obj is not Orientation orientation) return false;

            return Source.Id == orientation.Source.Id && Target.Id == orientation.Target.Id;
        }

        public override int GetHashCode()
        {
            return $"{Source.Id}-[${Machine?.Id}]->{Target.Id}".GetHashCode();
        }
    }
}
