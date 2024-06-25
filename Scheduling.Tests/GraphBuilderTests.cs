using QuickGraph.Contracts;
using Scheduling.Core.Extensions;
using Scheduling.Core.Graph;
using Scheduling.Core.Interfaces;
using Scheduling.Core.Models;
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
            Assert.Equal(16, disjunctiveGraphModel.EdgeCount);

            // subsequent operations turns into conjunctions
            foreach (var job in jobs)
                for (int i = 0; i < job.Operations.Count - 1; i++)
                    Assert.True(disjunctiveGraphModel.HasConjunction(job.Operations[i].Id, job.Operations[i + 1].Id));
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
            foreach (var (o1, o2) in jobs.CartesianProductOfJobsOperations())
            {
                if (o1.Id == o2.Id)
                    Assert.False(disjunctiveGraphModel.HasDisjunction(o1.Id, o2.Id));
                else
                    Assert.True(disjunctiveGraphModel.HasDisjunction(o1.Id, o2.Id));
            }
        }

        private static (List<Job>, List<Machine>) BuildInstance()
        {
            Func<Machine, double> weight = machine => 1.0;
            var machines = new List<Machine> { new Machine(1, 1), new Machine(2, 1), new Machine(3, 2), new Machine(4, 3), new Machine(5, 3), new Machine(6, 4) };
            var job1 = new Job(1);
            job1.Operations.AddRange(new List<Operation> { new Operation(1, 1, weight), new Operation(2, 2, weight), new Operation(3, 4, weight) });
            var job2 = new Job(2);
            job2.Operations.AddRange(new List<Operation> { new Operation(4, 2, weight), new Operation(5, 3, weight) });
            var job3 = new Job(3);
            job3.Operations.AddRange(new List<Operation> { new Operation(6, 1, weight), new Operation(7, 2, weight), new Operation(8, 3, weight) });
            var jobs = new List<Job> { job1, job2, job3 };
            return (jobs, machines);
        }
    }
}