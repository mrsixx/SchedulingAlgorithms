using Scheduling.Core.FJSP;
using Scheduling.Core.Graph;

namespace Scheduling.Core.Extensions
{
    public static class CollectionsExtensions
    {
        public static bool IsEmpty<T>(this IEnumerable<T> @list) => !@list.Any();

        public static bool DoesNotContain<T>(this IEnumerable<T> @list, T item) => !list.Contains(item);

        public static bool DoesNotContain<T>(this IEnumerable<T> @list, Func<T, bool> predicate) => !list.Any(predicate);

        public static void ForEach<T>(this IEnumerable<T> @list, Action<T> action)
        {
            foreach (var item in list)
                action(item);
        }


        public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> enumerable)
        {
            foreach (var item in enumerable)
                set.Add(item);
        }
        public static void AddRange<T>(this LinkedList<T> list, IEnumerable<T> enumerable)
        {
            foreach (var item in enumerable)
                list.AddLast(item);
        }
    }
}
