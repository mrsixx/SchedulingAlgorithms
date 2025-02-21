﻿using Scheduling.Core.FJSP;

namespace Scheduling.Solver.Interfaces
{
    public interface IFjspSolution
    {
        Dictionary<Operation, double> StartTimes { get; }

        Dictionary<Operation, double> CompletionTimes { get; }

        Dictionary<Operation, Machine> MachineAssignment { get; }

        double Makespan { get; }

        void Log();
    }
}
