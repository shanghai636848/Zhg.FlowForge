using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Zhg.FlowForge.Domain;

// Infrastructure/StrategyManager.cs
public interface IResourceStrategyManager
{
    Task<bool> InitializeAsync();
    Task<IEnumerable<LanguageResource>> LoadResourcesAsync(string culture, string? preferredStrategy = null);
    Task<bool> SaveResourcesAsync(string culture, IEnumerable<LanguageResource> resources, string? preferredStrategy = null);
    Task<IEnumerable<string>> GetAvailableCulturesAsync();
    Task<IEnumerable<IResourceStorageStrategy>> GetAvailableStrategiesAsync();
    IResourceStorageStrategy? GetStrategy(string name);
}

public class ResourceStrategyManager : IResourceStrategyManager
{
    private readonly IEnumerable<IResourceStorageStrategy> _strategies;
    private readonly ILogger<ResourceStrategyManager> _logger;
    private readonly LocalizationOptions _options;

    public ResourceStrategyManager(
        IEnumerable<IResourceStorageStrategy> strategies,
        IOptions<LocalizationOptions> options,
        ILogger<ResourceStrategyManager> logger)
    {
        _strategies = strategies.OrderByDescending(s => s.Priority);
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> InitializeAsync()
    {
        _logger.LogInformation("Initializing resource strategy manager with {Count} strategies", _strategies.Count());

        var results = await Task.WhenAll(_strategies.Select(s => s.InitializeAsync()));
        var successCount = results.Count(r => r);

        _logger.LogInformation("Strategy initialization completed: {SuccessCount}/{TotalCount} successful",
            successCount, _strategies.Count());

        return successCount > 0;
    }

    public async Task<IEnumerable<LanguageResource>> LoadResourcesAsync(string culture, string? preferredStrategy = null)
    {
        // 如果有首选策略，尝试使用它
        if (!string.IsNullOrEmpty(preferredStrategy))
        {
            var strategy = GetStrategy(preferredStrategy);
            if (strategy != null)
            {
                var resources = await strategy.LoadResourcesAsync(culture);
                if (resources.Any())
                {
                    _logger.LogDebug("Loaded resources for {Culture} using preferred strategy {Strategy}",
                        culture, preferredStrategy);
                    return resources;
                }
            }
        }

        // 否则按优先级尝试所有策略
        foreach (var strategy in _strategies)
        {
            try
            {
                if (await strategy.SupportsCultureAsync(culture))
                {
                    var resources = await strategy.LoadResourcesAsync(culture);
                    if (resources.Any())
                    {
                        _logger.LogDebug("Loaded resources for {Culture} using strategy {Strategy}",
                            culture, strategy.Name);
                        return resources;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Strategy {Strategy} failed to load resources for {Culture}",
                    strategy.Name, culture);
            }
        }

        _logger.LogWarning("No resources found for culture {Culture} using any strategy", culture);
        return Enumerable.Empty<LanguageResource>();
    }

    public async Task<bool> SaveResourcesAsync(string culture, IEnumerable<LanguageResource> resources, string? preferredStrategy = null)
    {
        var targetStrategy = !string.IsNullOrEmpty(preferredStrategy)
            ? GetStrategy(preferredStrategy)
            : _strategies.FirstOrDefault(s => s.Name == _options.PreferredSource)
              ?? _strategies.First();

        if (targetStrategy == null)
        {
            _logger.LogError("No available strategy for saving resources");
            return false;
        }

        return await targetStrategy.SaveResourcesAsync(culture, resources);
    }

    public async Task<IEnumerable<string>> GetAvailableCulturesAsync()
    {
        var allCultures = new HashSet<string>();

        foreach (var strategy in _strategies)
        {
            try
            {
                var cultures = await strategy.GetSupportedCulturesAsync();
                foreach (var culture in cultures)
                {
                    allCultures.Add(culture);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Strategy {Strategy} failed to get supported cultures", strategy.Name);
            }
        }

        return allCultures;
    }

    public async Task<IEnumerable<IResourceStorageStrategy>> GetAvailableStrategiesAsync()
    {
        return await Task.FromResult(_strategies);
    }

    public IResourceStorageStrategy? GetStrategy(string name)
    {
        return _strategies.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}