using Scheduling.Core.FJSP;

namespace Scheduling.Core.Extensions
{
    public static class FSJPExtensions
    {
        public static IEnumerable<Tuple<Operation, Operation, Machine>> GeneratePossibleDisjunctions(this IEnumerable<Operation> operations, IEnumerable<MachinePool> machinePools)
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

            var machinesByOperation = operations.Aggregate(
                new Dictionary<int, IEnumerable<Machine>>(),
                (acc, o) =>
                {
                    var machines = machinePools.Single(pool => pool.Id == o.MachinePoolId).Machines;
                    acc.Add(o.Id, machines);
                    return acc;
                });

            return pairs.SelectMany(pair =>
            {
                var (o1, o2) = pair;
                if(machinesByOperation.TryGetValue(o1.Id, out IEnumerable<Machine> o1Pool) && machinesByOperation.TryGetValue(o2.Id, out IEnumerable<Machine> o2Pool))
                {
                    var intersection = o1Pool.IntersectBy(o2Pool.Select(m => m.Id), m => m.Id);
                    return intersection.Select(machine => new Tuple<Operation, Operation, Machine>(o1, o2, machine));
                }
                
                return new List<Tuple<Operation, Operation, Machine>>();
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
    }
}
