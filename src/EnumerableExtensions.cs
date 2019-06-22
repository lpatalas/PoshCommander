using System;
using System.Collections.Generic;
using System.Linq;

namespace PoshCommander
{
    public static class EnumerableExtensions
    {
        public static int? FirstIndexOf<T>(
            this IEnumerable<T> collection,
            Func<T, bool> predicate)
        {
            return collection
                .Select((item, index) => (item, index))
                .Where(tuple => predicate(tuple.item))
                .Select(tuple => tuple.index)
                .FirstOrDefault();
        }

        public static int? FirstIndexOf<T>(
            this IEnumerable<T> collection,
            T searchedItem)
        {
            return collection
                .FirstIndexOf(item => object.Equals(item, searchedItem));
        }
    }
}
