using Scheduling.Core.FJSP;
using Scheduling.Core.Graph;
using System.Linq;
using System.Xml.Linq;

namespace Scheduling.Core.Extensions
{
    public static class FSJPExtensions
    {
        public static IEnumerable<Disjunction> GeneratePossibleDisjunctions(this DisjunctiveGraphModel graph)
        {
            return graph.OperationsAssimetricCartesianProduct()
                .SelectMany(pair =>
                {
                    var (o1, o2) = pair;
                    var intersection = o1.Operation.EligibleMachines.Intersect(o2.Operation.EligibleMachines);
                    return intersection.Select(machine => new Disjunction(o1, o2, machine));
                });
        }

        /// <summary>
        /// Produces O x O, where O is operations filtering loops (e.g. (0,0)) e simetric pairs (e.g. allow (0,1) but not (1,0))
        /// </summary>
        /// <param name="jobs"></param>
        /// <returns></returns>
        public static IEnumerable<Tuple<Node, Node>> OperationsAssimetricCartesianProduct(this DisjunctiveGraphModel graph)
        {
            var cartesianProduct = from o1 in graph.OperationVertices
                                   from o2 in graph.OperationVertices
                                   where o1 < o2
                                   select new Tuple<Node, Node>(o1, o2);
            return cartesianProduct;
        }


        public static bool BelongsToTheSameMachinePoolThan(this Operation @this, Operation that)
            => @this.EligibleMachines.Intersect(that.EligibleMachines).Any();
    }
}
