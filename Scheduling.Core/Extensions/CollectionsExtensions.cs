using Scheduling.Core.FJSP;
using Scheduling.Core.Graph;

namespace Scheduling.Core.Extensions
{
    public static class CollectionsExtensions
    {
        public static bool IsEmpty<T>(this IEnumerable<T> @list) => !@list.Any();

        public static bool DoesNotContain<T>(this IEnumerable<T> @list, T item) => !list.Contains(item);

        public static bool DoesNotContain<T>(this IEnumerable<T> @list, Func<T, bool> predicate) => !list.Any(predicate);


        public static void AddRange(this HashSet<Machine> set, IEnumerable<Machine> machines)
        {
            foreach (var machine in machines)
                set.Add(machine);
        }
    }
}
