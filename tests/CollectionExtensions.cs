using System.Collections.Generic;

namespace PoshCommander.Tests
{
    public static class CollectionExtensions
    {
        public static void Reset<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            collection.Clear();
            foreach (var item in items)
                collection.Add(item);
        }
    }
}
