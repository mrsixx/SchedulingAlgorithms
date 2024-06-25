using QuickGraph;
using Scheduling.Core.Graph;
using Scheduling.Core.Models;
using System.Linq;

namespace Scheduling.Core.Extensions
{
    public static class FSJPExtensions
    {

        /// <summary>
        /// Produces D \subset O x O s.t.
        /// (o1, o2) \in D -> o1 != o2 ^ (o2, o1) \not \in D, where O is operations set
        /// </summary>
        /// <param name="jobs"></param>
        /// <returns></returns>
        public static IEnumerable<Tuple<Operation, Operation>> GeneratePossibleDisjunctions(this IEnumerable<Job> jobs)
        {
            return jobs.CartesianProductOfJobsOperations()
                .Aggregate(new List<Tuple<Operation, Operation>>(), (acc, pair) => {
                    if (acc.Any(p => p.Item1.Id == pair.Item2.Id && p.Item2.Id == pair.Item1.Id))
                        return acc;
                    if (pair.Item1.Id == pair.Item2.Id)
                        return acc;
                    acc.Add(pair);
                    return acc;
                });
        }

        /// <summary>
        /// Produces O x O, where O is operations set
        /// </summary>
        /// <param name="jobs"></param>
        /// <returns></returns>
        public static IEnumerable<Tuple<Operation, Operation>> CartesianProductOfJobsOperations(this IEnumerable<Job> jobs)
        {
            return jobs.SelectMany(job => job.Operations)
            .GroupBy(o => o.MachinePoolId)
            .ToList()
            .SelectMany(grp =>
            {
                var machinePoolOperations = grp.ToList();
                var cartesianProduct = from o1 in machinePoolOperations
                                       from o2 in machinePoolOperations
                                       select new Tuple<Operation, Operation>(o1, o2);
                return cartesianProduct;
            });
        }
    }
}
