using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Text;

namespace Zhg.FlowForge.Domain;

// Application/Services/ILocalizationService.cs
public interface ILocalizationService
{
    event Action<string> OnLanguageChanged;
    CultureInfo CurrentCulture { get; }
    string CurrentCultureName { get; }

    Task InitializeAsync();
    Task SetLanguageAsync(string culture);
    string GetString(string key, params object[] args);
    string this[string key] { get; }
    string this[string key, params object[] args] { get; }

    Task<IEnumerable<string>> GetAvailableCulturesAsync();
    Task<IEnumerable<LanguageInfo>> GetLanguageInfoAsync();
    Task<Dictionary<string, string>> GetCurrentResourcesAsync();
    Task<bool> AddOrUpdateResourceAsync(string key, string value, string? culture = null);
    Task<bool> ReloadResourcesAsync();

    // 高级功能
    Task<string> TranslateAsync(string text, string targetCulture);
    Task<bool> ExportResourcesAsync(string culture, string format, Stream outputStream);
    Task<bool> ImportResourcesAsync(string culture, string format, Stream inputStream);
}

// Application/Services/LocalizationService.cs
public class LocalizationService : ILocalizationService
{
    private readonly IResourceStrategyManager _strategyManager;
    private readonly IPreferenceService _preferenceService;
    private readonly ILogger<LocalizationService> _logger;
    private readonly LocalizationOptions _options;
    private readonly ICacheService _cacheService;

    private CultureInfo _currentCulture;
    private Dictionary<string, Dictionary<string, string>> _resources;

    public event Action<string>? OnLanguageChanged;

    public CultureInfo CurrentCulture => _currentCulture;
    public string CurrentCultureName => _currentCulture.Name;

    public LocalizationService(
        IResourceStrategyManager strategyManager,
        IPreferenceService preferenceService,
        IOptions<LocalizationOptions> options,
        ICacheService cacheService,
        ILogger<LocalizationService> logger)
    {
        _strategyManager = strategyManager;
        _preferenceService = preferenceService;
        _options = options.Value;
        _cacheService = cacheService;
        _logger = logger;

        _currentCulture = new CultureInfo(_options.DefaultCulture);
        _resources = new Dictionary<string, Dictionary<string, string>>();
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing localization service");

        // 初始化策略管理器
        var strategyInitialized = await _strategyManager.InitializeAsync();
        if (!strategyInitialized)
        {
            _logger.LogWarning("Strategy manager initialization failed");
        }

        // 加载用户保存的语言偏好
        var savedLanguage = await _preferenceService.GetAsync("user_language", _options.DefaultCulture);
        await SetLanguageAsync(savedLanguage);

        _logger.LogInformation("Localization service initialized with culture: {Culture}", _currentCulture.Name);
    }

    public async Task SetLanguageAsync(string culture)
    {
        if (string.IsNullOrEmpty(culture) || culture == _currentCulture.Name)
            return;

        try
        {
            var availableCultures = await GetAvailableCulturesAsync();
            if (!availableCultures.Contains(culture))
            {
                _logger.LogWarning("Culture {Culture} not available, falling back to {Default}",
                    culture, _options.DefaultCulture);
                culture = _options.DefaultCulture;
            }

            // 加载资源
            await LoadCultureResourcesAsync(culture);

            var oldCulture = _currentCulture.Name;
            _currentCulture = new CultureInfo(culture);

            // 保存偏好
            await _preferenceService.SetAsync("user_language", culture);

            _logger.LogInformation("Language changed from {OldCulture} to {NewCulture}", oldCulture, culture);

            // 触发事件
            OnLanguageChanged?.Invoke(culture);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set language to {Culture}", culture);
            throw;
        }
    }

    private async Task LoadCultureResourcesAsync(string culture)
    {
        if (_resources.ContainsKey(culture))
            return;

        var resources = await _strategyManager.LoadResourcesAsync(culture, _options.PreferredSource);
        var resourceDict = resources.ToDictionary(r => r.Key, r => r.Value);

        _resources[culture] = resourceDict;
        _logger.LogDebug("Loaded {Count} resources for culture {Culture}", resourceDict.Count, culture);
    }

    public string GetString(string key, params object[] args)
    {
        if (string.IsNullOrEmpty(key))
            return string.Empty;

        var value = GetResourceValue(key);

        if (args.Length > 0 && !string.IsNullOrEmpty(value))
        {
            try
            {
                return string.Format(value, args);
            }
            catch (FormatException ex)
            {
                _logger.LogWarning(ex, "Format error for resource key {Key} with value: {Value}", key, value);
                return value;
            }
        }

        return value;
    }

    public string this[string key] => GetString(key);

    public string this[string key, params object[] args] => GetString(key, args);

    private string GetResourceValue(string key)
    {
        // 当前文化
        if (_resources.TryGetValue(_currentCulture.Name, out var cultureResources) &&
            cultureResources.TryGetValue(key, out var value))
        {
            return value;
        }

        // 后备文化
        if (_options.UseFallback && _currentCulture.Name != _options.FallbackCulture)
        {
            if (_resources.TryGetValue(_options.FallbackCulture, out var fallbackResources) &&
                fallbackResources.TryGetValue(key, out var fallbackValue))
            {
                _logger.LogDebug("Using fallback resource for key {Key}", key);
                return fallbackValue;
            }
        }

        _logger.LogWarning("Resource not found for key {Key} in culture {Culture}", key, _currentCulture.Name);
        return $"[{key}]";
    }

    public async Task<IEnumerable<string>> GetAvailableCulturesAsync()
    {
        return await _strategyManager.GetAvailableCulturesAsync();
    }

    public async Task<IEnumerable<LanguageInfo>> GetLanguageInfoAsync()
    {
        var cultures = await GetAvailableCulturesAsync();
        var languageInfo = new List<LanguageInfo>();

        foreach (var culture in cultures)
        {
            try
            {
                var cultureInfo = new CultureInfo(culture);
                var resources = await _strategyManager.LoadResourcesAsync(culture, _options.PreferredSource);

                languageInfo.Add(new LanguageInfo
                {
                    Culture = culture,
                    NativeName = cultureInfo.NativeName,
                    EnglishName = cultureInfo.EnglishName,
                    IsRTL = cultureInfo.TextInfo.IsRightToLeft,
                    Icon = GetCultureIcon(culture),
                    ResourceCount = resources.Count()
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get culture info for {Culture}", culture);
            }
        }

        return languageInfo;
    }

    private string GetCultureIcon(string culture)
    {
        return culture.ToLower() switch
        {
            "zh-cn" => "🇨🇳",
            "en" => "🇺🇸",
            "es" => "🇪🇸",
            "fr" => "🇫🇷",
            "de" => "🇩🇪",
            "ja" => "🇯🇵",
            "ko" => "🇰🇷",
            _ => "🌐"
        };
    }

    public async Task<Dictionary<string, string>> GetCurrentResourcesAsync()
    {
        await LoadCultureResourcesAsync(_currentCulture.Name);
        return new Dictionary<string, string>(_resources[_currentCulture.Name]);
    }

    public async Task<bool> AddOrUpdateResourceAsync(string key, string value, string? culture = null)
    {
        culture ??= _currentCulture.Name;

        try
        {
            var resource = new LanguageResource
            {
                Key = key,
                Value = value,
                Culture = culture,
                Module = "Common",
                UpdatedAt = DateTime.UtcNow
            };

            // 保存到首选策略
            var success = await _strategyManager.SaveResourcesAsync(culture, new[] { resource }, _options.PreferredSource);

            if (success)
            {
                // 更新内存中的资源
                if (_resources.ContainsKey(culture))
                {
                    _resources[culture][key] = value;
                }

                // 清除缓存
                await ClearResourceCache(key, culture);

                _logger.LogInformation("Updated resource {Key} for culture {Culture}", key, culture);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update resource {Key} for culture {Culture}", key, culture);
            return false;
        }
    }

    public async Task<bool> ReloadResourcesAsync()
    {
        try
        {
            var culture = _currentCulture.Name;
            _resources.Remove(culture); // 强制重新加载
            await LoadCultureResourcesAsync(culture);

            // 触发更新事件
            OnLanguageChanged?.Invoke(culture);

            _logger.LogInformation("Reloaded resources for culture {Culture}", culture);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reload resources");
            return false;
        }
    }

    // 高级功能实现
    public async Task<string> TranslateAsync(string text, string targetCulture)
    {
        // 简化的翻译功能 - 实际项目中可以集成外部翻译API
        _logger.LogInformation("Translation requested: '{Text}' to {TargetCulture}", text, targetCulture);

        // 这里可以实现真正的翻译逻辑
        await Task.Delay(100); // 模拟翻译延迟

        return $"[{targetCulture}]{text}";
    }

    public async Task<bool> ExportResourcesAsync(string culture, string format, Stream outputStream)
    {
        try
        {
            var resources = await _strategyManager.LoadResourcesAsync(culture);
            var strategy = _strategyManager.GetStrategy(format);

            if (strategy == null)
            {
                _logger.LogError("Export format not supported: {Format}", format);
                return false;
            }

            // 创建临时文件进行导出
            var tempPath = Path.GetTempFileName();
            await strategy.SaveResourcesAsync(culture, resources);

            // 读取文件内容到输出流
            var fileContent = await File.ReadAllBytesAsync(tempPath);
            await outputStream.WriteAsync(fileContent);

            File.Delete(tempPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export resources for culture {Culture} in format {Format}", culture, format);
            return false;
        }
    }

    public async Task<bool> ImportResourcesAsync(string culture, string format, Stream inputStream)
    {
        try
        {
            // 创建临时文件
            var tempPath = Path.GetTempFileName();
            using var fileStream = File.Create(tempPath);
            await inputStream.CopyToAsync(fileStream);
            await fileStream.FlushAsync();

            // 使用对应策略加载资源
            var strategy = _strategyManager.GetStrategy(format);
            if (strategy == null)
            {
                _logger.LogError("Import format not supported: {Format}", format);
                return false;
            }

            var resources = await strategy.LoadResourcesAsync(culture);
            await _strategyManager.SaveResourcesAsync(culture, resources, _options.PreferredSource);

            File.Delete(tempPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import resources for culture {Culture} in format {Format}", culture, format);
            return false;
        }
    }

    private async Task ClearResourceCache(string key, string culture)
    {
        var cacheKeys = new[]
        {
            $"resource_{culture}_{key}",
            $"resources_culture_{culture}"
        };

        foreach (var cacheKey in cacheKeys)
        {
            await _cacheService.RemoveAsync(cacheKey);
        }
    }
}