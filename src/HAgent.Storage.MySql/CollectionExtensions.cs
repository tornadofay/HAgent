using System.Collections.Generic;

namespace HAgent.Storage.MySql
{
    internal static class CollectionExtensions
    {
        internal static void AddRange<T>(this IList<T> target, IEnumerable<T> values)
        {
            if (target == null || values == null)
                return;

            foreach (var value in values)
                target.Add(value);
        }
    }
}
