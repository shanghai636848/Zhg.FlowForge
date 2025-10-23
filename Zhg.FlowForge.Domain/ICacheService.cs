using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Zhg.FlowForge.Domain;

// Infrastructure/Services/ICacheService.cs
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task<bool> ExistsAsync(string key);
}

// 内存缓存实现
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<MemoryCacheService> _logger;

    public MemoryCacheService(IMemoryCache memoryCache, ILogger<MemoryCacheService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key)
    {
        try
        {
            return Task.FromResult(_memoryCache.Get<T>(key));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get cache item with key {Key}", key);
            return Task.FromResult(default(T));
        }
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var options = new MemoryCacheEntryOptions();
            if (expiration.HasValue)
            {
                options.SetAbsoluteExpiration(expiration.Value);
            }

            _memoryCache.Set(key, value, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set cache item with key {Key}", key);
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        try
        {
            _memoryCache.Remove(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove cache item with key {Key}", key);
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key)
    {
        return Task.FromResult(_memoryCache.TryGetValue(key, out _));
    }
}

// 缓存装饰器 (装饰器模式)
public class CachedResourceRepository : IResourceRepository
{
    private readonly IResourceRepository _innerRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachedResourceRepository> _logger;
    private readonly TimeSpan _cacheDuration;

    public CachedResourceRepository(
        IResourceRepository innerRepository,
        ICacheService cacheService,
        ILogger<CachedResourceRepository> logger,
        IOptions<LocalizationOptions> options)
    {
        _innerRepository = innerRepository;
        _cacheService = cacheService;
        _logger = logger;
        _cacheDuration = options.Value.CacheDuration;
    }

    public async Task<LanguageResource?> GetAsync(string key, string culture)
    {
        var cacheKey = $"resource_{culture}_{key}";

        var cached = await _cacheService.GetAsync<LanguageResource>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Cache hit for resource {Key} in culture {Culture}", key, culture);
            return cached;
        }

        var resource = await _innerRepository.GetAsync(key, culture);
        if (resource != null)
        {
            await _cacheService.SetAsync(cacheKey, resource, _cacheDuration);
        }

        return resource;
    }

    public async Task<IEnumerable<LanguageResource>> GetByCultureAsync(string culture)
    {
        var cacheKey = $"resources_culture_{culture}";

        var cached = await _cacheService.GetAsync<IEnumerable<LanguageResource>>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Cache hit for culture resources {Culture}", culture);
            return cached;
        }

        var resources = await _innerRepository.GetByCultureAsync(culture);
        await _cacheService.SetAsync(cacheKey, resources, _cacheDuration);

        return resources;
    }

    // 其他方法类似实现...
    public Task InitializeAsync() => _innerRepository.InitializeAsync();

    public async Task<bool> AddOrUpdateAsync(LanguageResource resource)
    {
        var result = await _innerRepository.AddOrUpdateAsync(resource);
        if (result)
        {
            // 清除相关缓存
            await ClearCultureCache(resource.Culture);
            await ClearResourceCache(resource.Key, resource.Culture);
        }
        return result;
    }

    private async Task ClearCultureCache(string culture)
    {
        var cacheKey = $"resources_culture_{culture}";
        await _cacheService.RemoveAsync(cacheKey);
    }

    private async Task ClearResourceCache(string key, string culture)
    {
        var cacheKey = $"resource_{culture}_{key}";
        await _cacheService.RemoveAsync(cacheKey);
    }

    // 其他方法的实现...
    public Task<IEnumerable<LanguageResource>> GetByModuleAsync(string module, string culture)
        => _innerRepository.GetByModuleAsync(module, culture);

    public Task<bool> AddOrUpdateRangeAsync(IEnumerable<LanguageResource> resources)
        => _innerRepository.AddOrUpdateRangeAsync(resources);

    public Task<bool> RemoveAsync(string key, string culture)
        => _innerRepository.RemoveAsync(key, culture);

    public Task<IEnumerable<string>> GetAvailableCulturesAsync()
        => _innerRepository.GetAvailableCulturesAsync();

    public Task<int> GetResourceCountAsync(string culture)
        => _innerRepository.GetResourceCountAsync(culture);

    public Task<bool> ExistsAsync(string key, string culture)
        => _innerRepository.ExistsAsync(key, culture);

    public void Dispose() => _innerRepository.Dispose();
}