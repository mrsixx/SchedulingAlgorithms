using Scheduling.Core.FJSP;

namespace Scheduling.Core.Extensions
{
    public static class FSJPExtensions
    {
        public static IEnumerable<Tuple<Operation, Operation, Machine>> GeneratePossibleDisjunctions(this IEnumerable<Operation> operations)
        {
            //var operationsByPool = jobs.SelectMany(job => job.Operations).GroupBy(o => o.MachinePoolId);
            var pairs = operations.CartesianProductOfOperations()
                .Aggregate(new List<Tuple<Operation, Operation>>(), (acc, pair) =>
                {
                    if (acc.Any(p => p.Item1.Id == pair.Item2.Id && p.Item2.Id == pair.Item1.Id))
                        return acc;
                    if (pair.Item1.Id == pair.Item2.Id)
                        return acc;
                    acc.Add(pair);
                    return acc;
                });

            return pairs.SelectMany(pair =>
            {
                var (o1, o2) = pair;
                var intersection = o1.EligibleMachines.IntersectBy(o2.EligibleMachines.Select(m => m.Id), m => m.Id);
                return intersection.Select(machine => new Tuple<Operation, Operation, Machine>(o1, o2, machine));
            });
        }

        /// <summary>
        /// Produces O x O, where O is operations set
        /// </summary>
        /// <param name="jobs"></param>
        /// <returns></returns>
        public static IEnumerable<Tuple<Operation, Operation>> CartesianProductOfOperations(this IEnumerable<Operation> operations)
        {
            var cartesianProduct = from o1 in operations
                                   from o2 in operations
                                   select new Tuple<Operation, Operation>(o1, o2);
            return cartesianProduct;
        }

        public static bool BelongsToTheSameMachinePoolThan(this Operation @this, Operation that)
            => @this.EligibleMachines.Intersect(that.EligibleMachines).Any();
    }
}
