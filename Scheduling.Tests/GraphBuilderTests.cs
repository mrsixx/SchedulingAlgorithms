using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;
using Scheduling.Core.Interfaces;
using Scheduling.Core.Services;

namespace Scheduling.Tests
{
    public class GraphBuilderTests
    {
        private readonly IGraphBuilderService _graphBuilderService;
        private readonly IGraphExporterService _graphExporterService;

        public GraphBuilderTests()
        {

            _graphBuilderService = new GraphBuilderService();
            _graphExporterService = new GraphExporterService();
        }

        [Fact]
        public void BuildDisjunctiveGraph_GraphSize_MustCorrespondInstance()
        {

            var (jobs, machines) = BuildInstance();
            var disjunctiveGraphModel = _graphBuilderService.BuildDisjunctiveGraph(jobs, machines);

            Assert.Equal(10, disjunctiveGraphModel.VertexCount); // 8 operations + source and sink nodes
            Assert.Equal(18, disjunctiveGraphModel.EdgeCount);
        }

        [Fact]
        public void BuildDisjunctiveGraph_OperationsSequence_MustTurnsIntoConjunctions()
        {

            var (jobs, machines) = BuildInstance();
            var disjunctiveGraphModel = _graphBuilderService.BuildDisjunctiveGraph(jobs, machines);

            // subsequent operations turns into conjunctions
            foreach (var job in jobs)
                for (int i = 0; i < job.Operations.Count - 1; i++)
                    Assert.True(disjunctiveGraphModel.HasConjunction(job.Operations[i].Id, job.Operations[i + 1].Id));
        }


        [Fact]
        public void BuildDisjunctiveGraph_SameMachinePoolOperations_MustTurnsIntoDisjunctions()
        {

            var (jobs, machines) = BuildInstance();
            var disjunctiveGraphModel = _graphBuilderService.BuildDisjunctiveGraph(jobs, machines);

            // subsequent operations turned into conjunctions
            foreach (var (o1, o2) in jobs.SelectMany(j => j.Operations).CartesianProductOfOperations())
            {
                if(o1.Id != o2.Id && o1.MachinePoolId == o2.MachinePoolId)
                    Assert.True(disjunctiveGraphModel.HasDisjunction(o1.Id, o2.Id));
                else
                    Assert.False(disjunctiveGraphModel.HasDisjunction(o1.Id, o2.Id));

            }
        }

        private static (List<Job>, List<MachinePool>) BuildInstance()
        {
            Func<Machine, double> weight = machine => 1.0;
            MachinePool pool1 = new(1), pool2 = new(2), pool3 = new(3), pool4 = new(4);
            pool1.Machines.AddRange(new List<Machine> { new Machine(1), new Machine(2) });
            pool2.Machines.AddRange(new List<Machine> { new Machine(3) });
            pool3.Machines.AddRange(new List<Machine> { new Machine(4), new Machine(5) });
            pool4.Machines.AddRange(new List<Machine> { new Machine(6) });

            Job job1 = new(1), job2 = new(2), job3 = new(3);
            job1.Operations.AddRange(new List<Operation> { new Operation(1, pool1.Id, weight), new Operation(2, pool2.Id, weight), new Operation(3, pool4.Id, weight) });
            job2.Operations.AddRange(new List<Operation> { new Operation(4, pool2.Id, weight), new Operation(5, pool3.Id, weight) });
            job3.Operations.AddRange(new List<Operation> { new Operation(6, pool1.Id, weight), new Operation(7, pool2.Id, weight), new Operation(8, pool3.Id, weight) });

            var jobs = new List<Job> { job1, job2, job3 };
            var pools = new List<MachinePool> { pool1, pool2, pool3, pool4 };
            return (jobs, pools);
        }
    }
}