﻿using Microsoft.Extensions.Caching.Distributed;

namespace TerraMours.Framework.Infrastructure.Redis
{
    public interface IDistributedCacheHelper
    {
        TResult? GetOrCreate<TResult>(string cacheKey, Func<DistributedCacheEntryOptions, TResult?> valueFactory, int expireSeconds = 60);

        Task<TResult?> GetOrCreateAsync<TResult>(string cacheKey, Func<DistributedCacheEntryOptions, Task<TResult?>> valueFactory, int expireSeconds = 60);

        void Remove(string cacheKey);
        Task RemoveAsync(string cacheKey);
    }
}
