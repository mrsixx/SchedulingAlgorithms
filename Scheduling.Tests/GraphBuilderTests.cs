using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
using Scheduling.Core.Interfaces;
using Scheduling.Core.Services;

namespace Scheduling.Tests
{
    public class GraphBuilderTests
    {
        private readonly IGraphBuilderService _graphBuilderService = new GraphBuilderService();
        private readonly IGraphExporterService _graphExporterService = new GraphExporterService();

        [Fact]
        public void BuildDisjunctiveGraph_GraphSize_MustCorrespondInstance()
        {

            var instance = BuildInstance();
            var disjunctiveGraphModel = _graphBuilderService.BuildDisjunctiveGraph(instance);


            Assert.Equal(10, disjunctiveGraphModel.VertexCount); // 8 operations + source and sink nodes
            Assert.Equal(42, disjunctiveGraphModel.EdgeCount);
            Assert.Equal(11, disjunctiveGraphModel.ConjuntionCount);
            Assert.Equal(31, disjunctiveGraphModel.DisjuntionCount);
        }

        [Fact]
        public void BuildDisjunctiveGraph_OperationsSequence_MustTurnsIntoConjunctions()
        {

            var instance = BuildInstance();
            var disjunctiveGraphModel = _graphBuilderService.BuildDisjunctiveGraph(instance);

            // subsequent operations turns into conjunctions
            foreach (var job in instance.Jobs)
            {
                var operationsLength = job.Operations.Length;
                for (int i = 1; i < operationsLength; i++)
                {
                    var previousOperation = job.Operations[i - 1];
                    var currentOperation = job.Operations[i];
                    Assert.True(disjunctiveGraphModel.HasConjunction(previousOperation.Id, currentOperation.Id));
                }
            }
        }


        [Fact]
        public void BuildDisjunctiveGraph_SameMachinePoolOperations_MustTurnsIntoDisjunctions()
        {

            var instance = BuildInstance();
            var disjunctiveGraphModel = _graphBuilderService.BuildDisjunctiveGraph(instance);

            // subsequent operations turned into conjunctions
            foreach (var (o1, o2) in disjunctiveGraphModel.OperationsAssimetricCartesianProduct())
            {
                if (o1 != o2 && o1.Operation.BelongsToTheSameMachinePoolThan(o2.Operation))
                    Assert.True(disjunctiveGraphModel.HasDisjunction(o1.Id, o2.Id));
                else
                    Assert.False(disjunctiveGraphModel.HasDisjunction(o1.Id, o2.Id));

            }
        }

        private static Instance BuildInstance()
        {
            Dictionary<Machine, long> weight = [];
            Action<Machine> setWeight = m => weight.Add(m, 1);

            var pool1 = new List<Machine> { new(1), new(2) };
            var pool2 = new List<Machine> { new(3) };
            var pool3 = new List<Machine> { new(4), new(5) };
            var pool4 = new List<Machine> { new(6) };
            Machine[] machines = [.. pool1, .. pool2, .. pool3, .. pool4];
            machines.ForEach(setWeight);

            Job job1 = new(1), job2 = new(2), job3 = new(3);
            Operation o1 = new(1, weight), o2 = new(2, weight), o3 = new(3, weight);
            Operation o4 = new(4, weight), o5 = new(5, weight);
            Operation o6 = new(6, weight), o7 = new(7, weight), o8 = new(8, weight);

            o1.EligibleMachines.AddRange(pool1);
            o6.EligibleMachines.AddRange(pool1);

            o2.EligibleMachines.AddRange(pool2);
            o4.EligibleMachines.AddRange(pool2);
            o7.EligibleMachines.AddRange(pool2);

            o5.EligibleMachines.AddRange(pool3);
            o8.EligibleMachines.AddRange(pool3);

            o3.EligibleMachines.AddRange(pool4);

            job1.Operations = [o1, o2, o3];
            job2.Operations = [o4, o5];
            job3.Operations = [o6, o7, o8];

            var jobs = new List<Job> { job1, job2, job3 };

            return new(jobs, machines);
        }
    }
}