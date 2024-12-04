using Scheduling.Core.FJSP;
using Scheduling.Core.Graph;
using Scheduling.Solver.AntColonyOptimization;
using System.Drawing;

namespace Scheduling.Solver.Models
{
    public class FjspSolution(Colony colony)
    {
        public Colony Context { get; } = colony;

        public double Makespan => Context.EmployeeOfTheMonth.Makespan;

        public Dictionary<Operation, Machine> MachineAssignment { get; } = [];
        
        public Dictionary<Operation, DateTime> StartTimes { get; } = [];

        public List<Output> GetOutput()
        {
            var output = new List<Output>();
            var colors = Enum.GetValues(typeof(KnownColor))
               .Cast<KnownColor>()
               .Select(Color.FromKnownColor).ToList();


            var jobColors = new Dictionary<int, string>();
            foreach (var pair in Context.IterationBests)
            {
                var iOutput = new Output();
                iOutput.Events.AddRange(pair.Value.Context.DisjunctiveGraph.OperationVertices
                    .Select(o => {
                        var evt = OutputEvent.FromNode(o, pair.Value);
                        if (!jobColors.ContainsKey(o.Operation.JobId))
                        {
                            var color = colors[Random.Shared.Next(0, colors.Count)];
                            colors.Remove(color);
                            jobColors.Add(o.Operation.JobId, $"#{color.R:X2}{color.G:X2}{color.B:X2}");
                        }
                        evt.Color = jobColors[o.Operation.JobId];
                        return evt;
                    }));

                iOutput.Machines.AddRange(pair.Value.Context.DisjunctiveGraph.Machines.Select(m => new OutputMachine(m.Id)));

                int i = 1;
                foreach (var assignment in pair.Value.MachineAssignment)
                    iOutput.Assignments.Add(new OutputAssignment { Id = i++, EventId = assignment.Key.Id, MachineId = assignment.Value.Id });

                output.Add(iOutput);
            }

            return output;
        }

    }
}
