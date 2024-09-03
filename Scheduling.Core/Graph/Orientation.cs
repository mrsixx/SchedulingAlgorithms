﻿using Scheduling.Core.FJSP;
using System.Diagnostics;
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
                if (Source.IsSourceNode)
                {
                    //Debug.WriteLine("Hit 1");
                    return 1;
                }

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
            if (ReferenceEquals(this, obj))
                return true;

            if (obj is not Orientation orientation)
                return false;

            return Machine.Id == orientation.Machine.Id
                   && Source.Id == orientation.Source.Id
                   && Target.Id == orientation.Target.Id;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Source.Id.GetHashCode();
                hash = hash * 23 + (Machine?.Id.GetHashCode() ?? 0);
                hash = hash * 23 + Target.Id.GetHashCode();
                return hash;
            }
        }
    }
}
