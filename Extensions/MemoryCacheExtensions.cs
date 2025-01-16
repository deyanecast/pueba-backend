using Microsoft.Extensions.Caching.Memory;
using System.Reflection;

namespace MiBackend.Extensions
{
    public static class MemoryCacheExtensions
    {
        private static readonly Func<MemoryCache, object> GetEntriesCollection = delegate (MemoryCache cache)
        {
            return cache.GetType()
                .GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance)?
                .GetValue(cache);
        };

        public static IEnumerable<string> GetKeys<T>(this IMemoryCache cache)
        {
            var memCache = cache as MemoryCache;
            if (memCache == null) return Enumerable.Empty<string>();

            var entriesCollection = GetEntriesCollection(memCache);
            if (entriesCollection == null) return Enumerable.Empty<string>();

            var keys = entriesCollection.GetType()
                .GetProperty("Keys")?
                .GetValue(entriesCollection) as IEnumerable<object>;

            return keys?.Select(k => k.ToString()) ?? Enumerable.Empty<string>();
        }
    }
} 