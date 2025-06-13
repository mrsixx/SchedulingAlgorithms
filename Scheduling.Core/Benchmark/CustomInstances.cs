using Scheduling.Core.Extensions;
using Scheduling.Core.FJSP;

namespace Scheduling.Core.Benchmark
{
    public class CustomInstances
    {
        public static IEnumerable<Job> SampleInstance()
        {
            Func<Machine, double> weight = machine => 1.0;
            var pool1 = new List<Machine> { new(1), new(2) };
            var pool2 = new List<Machine> { new(3) };
            var pool3 = new List<Machine> { new(4), new(5) };
            var pool4 = new List<Machine> { new(6) };
            var weights = new Dictionary<Machine, double>();

            Dictionary<Machine, long> ToDict(List<Machine> pool)
            {
                return pool.Aggregate(new Dictionary<Machine, long>(), (dict, m) =>
                {
                    dict.Add(m, 1);
                    return dict;
                });
            }

            Job job1 = new(1), job2 = new(2), job3 = new(3);
            Operation o1 = new(1, ToDict(pool1)), o2 = new(2, ToDict(pool2)), o3 = new(3, ToDict(pool4));
            Operation o4 = new(4, ToDict(pool2)), o5 = new(5, ToDict(pool3));
            Operation o6 = new(6, ToDict(pool1)), o7 = new(7, ToDict(pool2)), o8 = new(8, ToDict(pool3));

            o1.EligibleMachines.AddRange(pool1);
            o6.EligibleMachines.AddRange(pool1);

            o2.EligibleMachines.AddRange(pool2);
            o4.EligibleMachines.AddRange(pool2);
            o7.EligibleMachines.AddRange(pool2);

            o5.EligibleMachines.AddRange(pool3);
            o8.EligibleMachines.AddRange(pool3);

            o3.EligibleMachines.AddRange(pool4);

            //job1.Operations.AddRange([o1, o2, o3]);
            //job2.Operations.AddRange([o4, o5]);
            //job3.Operations.AddRange([o6, o7, o8]);

            var jobs = new List<Job> { job1, job2, job3 };
            return jobs;
        }
    }
}
