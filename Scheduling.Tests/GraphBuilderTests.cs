using QuikGraph;
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

            var jobs = BuildInstance();
            var disjunctiveGraphModel = _graphBuilderService.BuildDisjunctiveGraph(jobs);

            Assert.Equal(10, disjunctiveGraphModel.VertexCount); // 8 operations + source and sink nodes
            Assert.Equal(20, disjunctiveGraphModel.EdgeCount);
            Assert.Equal(13, disjunctiveGraphModel.ConjuntionCount);
            Assert.Equal(7, disjunctiveGraphModel.DisjuntionCount);
        }

        [Fact]
        public void BuildDisjunctiveGraph_OperationsSequence_MustTurnsIntoConjunctions()
        {

            var jobs = BuildInstance();
            var disjunctiveGraphModel = _graphBuilderService.BuildDisjunctiveGraph(jobs);

            // subsequent operations turns into conjunctions
            foreach (var job in jobs)
                for (int i = 0; i < job.Operations.Count - 1; i++)
                    Assert.True(disjunctiveGraphModel.HasConjunction(job.Operations[i].Id, job.Operations[i + 1].Id));
        }


        [Fact]
        public void BuildDisjunctiveGraph_SameMachinePoolOperations_MustTurnsIntoDisjunctions()
        {

            var jobs = BuildInstance();
            var disjunctiveGraphModel = _graphBuilderService.BuildDisjunctiveGraph(jobs);
            // subsequent operations turned into conjunctions
            foreach (var (o1, o2) in jobs.SelectMany(j => j.Operations).CartesianProductOfOperations())
            {
                if(o1 != o2 && o1.BelongsToTheSameMachinePoolThan(o2))
                    Assert.True(disjunctiveGraphModel.HasDisjunction(o1.Id, o2.Id));
                else
                    Assert.False(disjunctiveGraphModel.HasDisjunction(o1.Id, o2.Id));

            }
        }

        private static List<Job> BuildInstance()
        {
            Func<Machine, double> weight = machine => 1.0;
            var pool1 = new List<Machine> { new Machine(1), new Machine(2) };
            var pool2 = new List<Machine> { new Machine(3) };
            var pool3 = new List<Machine> { new Machine(4), new Machine(5) };
            var pool4 = new List<Machine> { new Machine(6) };

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

            job1.Operations.AddRange(new List<Operation> { o1, o2, o3 });
            job2.Operations.AddRange(new List<Operation> { o4, o5 });
            job3.Operations.AddRange(new List<Operation> { o6, o7, o8 });
            
            var jobs = new List<Job> { job1, job2, job3 };
            return jobs;
        }
    }
}