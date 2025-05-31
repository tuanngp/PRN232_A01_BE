using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories.Common;

namespace Repositories.Extensions
{
    public static class RepositoryExtensions
    {
        private static readonly ConcurrentDictionary<string, object> _cache = new();
        private static readonly TimeSpan _defaultCacheExpiration = TimeSpan.FromMinutes(30);

        public static async Task LogAndExecuteAsync<T>(
            this IGenericRepository<T> repository,
            Func<Task> operation,
            ILogger logger,
            string operationName
        )
            where T : class
        {
            try
            {
                logger.LogInformation($"Starting {operationName} operation for {typeof(T).Name}");
                await operation();
                logger.LogInformation($"Completed {operationName} operation for {typeof(T).Name}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error during {operationName} operation for {typeof(T).Name}");
                throw;
            }
        }

        public static async Task<TResult> GetWithCacheAsync<T, TResult>(
            this IGenericRepository<T> repository,
            string cacheKey,
            Func<Task<TResult>> getData
        )
            where T : class
        {
            if (_cache.TryGetValue(cacheKey, out var cachedResult))
            {
                return (TResult)cachedResult;
            }

            var result = await getData();
            _cache.TryAdd(cacheKey, result);

            // Schedule cache removal after expiration
            Task.Delay(_defaultCacheExpiration)
                .ContinueWith(_ =>
                {
                    object removedValue;
                    _cache.TryRemove(cacheKey, out removedValue);
                });

            return result;
        }

        public static void InvalidateCache<T>(
            this IGenericRepository<T> repository,
            string cacheKey
        )
            where T : class
        {
            object removedValue;
            _cache.TryRemove(cacheKey, out removedValue);
        }
    }
}
