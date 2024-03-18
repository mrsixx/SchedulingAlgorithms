using Scheduling.Core.Graph;
using Scheduling.Core.Models;

namespace Scheduling.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var machines = new List<Machine> { new Machine(1), new Machine(2), new Machine(3) };
            var job1 = new Job(1);
            job1.Operations.AddRange(new List<Operation> { new Operation(1,3), new Operation(2,2), new Operation(3,3) });
            var job2 = new Job(2);
            job2.Operations.AddRange(new List<Operation> { new Operation(4, 3), new Operation(5, 4) });
            var job3 = new Job(3);
            job3.Operations.AddRange(new List<Operation> { new Operation(6,6), new Operation(7, 3), new Operation(8,2) });
            var jobs = new List<Job> { job1, job2, job3 };

            var graphBuilder = new GraphBuilder();
            var graphExporter = new GraphExporter();
            graphExporter.ExportTableGraphToGraphviz(graphBuilder.BuildDisjunctiveGraph(jobs, machines), @"C:\\Users\\mathe\\Desktop", "graph");


        }
    }
}