using System;
using System.Collections.Generic;
using System.Linq;

namespace PoshCommander
{
    public static class CollectionExtensions
    {
        public static void Set<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            collection.Clear();
            foreach (var item in items)
                collection.Add(item);
        }

        public static void Set<T>(this ICollection<T> collection, params T[] items)
            => Set(collection, (IEnumerable<T>)items);

        public static void RemoveWhere<T>(
            this ICollection<T> collection,
            Func<T, bool> predicate)
        {
            var itemsToRemove = collection.Where(predicate).ToList();
            foreach (var item in itemsToRemove)
                collection.Remove(item);
        }
    }
}
