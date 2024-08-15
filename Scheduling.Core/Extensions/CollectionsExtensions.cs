﻿using Scheduling.Core.FJSP;
using Scheduling.Core.Graph;

namespace Scheduling.Core.Extensions
{
    public static class CollectionsExtensions
    {
        public static bool IsEmpty<T>(this IEnumerable<T> @list) => !@list.Any();

        public static bool DoesNotContain<T>(this IEnumerable<T> @list, T item) => !list.Contains(item);

        public static bool DoesNotContain<T>(this IEnumerable<T> @list, Func<T, bool> predicate) => !list.Any(predicate);



        public static bool DoesNotContainNode(this Dictionary<Machine, Stack<Node>> loadingSequence, Node node)
        {
            return loadingSequence.Values.All(s => s.DoesNotContain(node));

        }

        public static void AddIfDoesNotContain<T>(this IList<T> @list, T item)
        {
            if (list.DoesNotContain(item))
                list.Add(item);
        }

        public static void AddIfDoesNotContain<T>(this IList<T> @list, params T[] itens)
            => itens.ToList().ForEach(item => list.AddIfDoesNotContain(item));
    }
}
