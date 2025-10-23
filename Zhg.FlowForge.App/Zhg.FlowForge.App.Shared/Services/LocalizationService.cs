

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using Zhg.FlowForge.App.Shared.Interfaces;
using Zhg.FlowForge.App.Shared.Models;

namespace Zhg.FlowForge.App.Shared.Services;



public class LocalizationService : ILocalizationService
{
    private readonly IJsonResourceService _jsonService;
    private readonly IPreferenceService _preferenceService;
    private readonly IOptions<AppSettings> _settings;
    private readonly ILogger<LocalizationService> _logger;

    private readonly Dictionary<string, Dictionary<string, string>> _resources;
    private CultureInfo _currentCulture;

    public CultureInfo CurrentCulture => _currentCulture;
    public string CurrentCultureName => _currentCulture.Name;
    public event Action OnLanguageChanged;

    public LocalizationService(
        IJsonResourceService jsonService,
        IPreferenceService preferenceService,
        IOptions<AppSettings> settings,
        ILogger<LocalizationService> logger)
    {
        _jsonService = jsonService;
        _preferenceService = preferenceService;
        _settings = settings;
        _logger = logger;
        _resources = new Dictionary<string, Dictionary<string, string>>();
        _currentCulture = new CultureInfo(settings.Value.DefaultCulture);
    }

    public async Task InitializeAsync()
    {
        try
        {
            // 初始化JSON服务
            var jsonInitialized = await _jsonService.InitializeAsync();
            if (!jsonInitialized)
            {
                _logger.LogWarning("JSON resource service initialization failed, using built-in resources only");
            }

            // 加载用户保存的语言偏好
            var savedLanguage = await _preferenceService.GetAsync("user_language", _settings.Value.DefaultCulture);
            await SetLanguageAsync(savedLanguage);

            _logger.LogInformation("Localization service initialized with culture: {Culture}", _currentCulture.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize localization service");
            // 即使初始化失败，也设置默认文化
            _currentCulture = new CultureInfo(_settings.Value.DefaultCulture);
        }
    }

    public async Task SetLanguageAsync(string culture)
    {
        if (string.IsNullOrEmpty(culture))
            return;

        try
        {
            // 验证文化是否可用
            var availableCultures = await GetAvailableCulturesAsync();
            if (!availableCultures.Contains(culture))
            {
                _logger.LogWarning("Culture {Culture} is not available, falling back to {Default}",
                    culture, _settings.Value.DefaultCulture);
                culture = _settings.Value.DefaultCulture;
            }

            // 加载资源（如果尚未加载）
            if (!_resources.ContainsKey(culture))
            {
                await LoadResourcesAsync(culture);
            }

            var oldCulture = _currentCulture.Name;
            _currentCulture = new CultureInfo(culture);

            // 保存用户偏好
            await _preferenceService.SetAsync("user_language", culture);

            _logger.LogInformation("Language changed from {OldCulture} to {NewCulture}", oldCulture, culture);

            // 触发语言变更事件
            OnLanguageChanged?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting language to {Culture}", culture);
        }
    }

    private async Task LoadResourcesAsync(string culture)
    {
        var loadResult = await _jsonService.LoadResourcesAsync(culture);

        if (loadResult.Success)
        {
            _resources[culture] = loadResult.Resources;
            _logger.LogDebug("Loaded {Count} resources for culture {Culture} from {Source}",
                loadResult.Resources.Count, culture, loadResult.Source);
        }
        else
        {
            _logger.LogWarning("Failed to load resources for culture {Culture}: {Error}",
                culture, loadResult.ErrorMessage);

            // 使用空字典作为后备
            _resources[culture] = new Dictionary<string, string>();
        }
    }

    public string GetString(string key)
    {
        if (string.IsNullOrEmpty(key))
            return string.Empty;

        var culture = _currentCulture.Name;

        // 首先尝试当前文化
        if (_resources.ContainsKey(culture) && _resources[culture].ContainsKey(key))
        {
            return _resources[culture][key];
        }

        // 后备到默认文化
        var defaultCulture = _settings.Value.DefaultCulture;
        if (culture != defaultCulture &&
            _resources.ContainsKey(defaultCulture) &&
            _resources[defaultCulture].ContainsKey(key))
        {
            return _resources[defaultCulture][key];
        }

        // 最后返回键名
        return $"[{key}]";
    }

    public string this[string key] => GetString(key);

    public async Task<List<string>> GetAvailableCulturesAsync()
    {
        return await _jsonService.GetAvailableCulturesAsync();
    }

    public async Task<bool> AddOrUpdateResourceAsync(string key, string value, string culture = "")
    {
        culture ??= _currentCulture.Name;

        try
        {
            // 获取当前资源
            var loadResult = await _jsonService.LoadResourcesAsync(culture);
            if (!loadResult.Success)
            {
                _logger.LogWarning("Cannot update resources for culture {Culture}: {Error}",
                    culture, loadResult.ErrorMessage);
                return false;
            }

            // 更新资源
            loadResult.Resources[key] = value;

            // 保存回文件
            var saveResult = await _jsonService.SaveResourcesAsync(culture, loadResult.Resources);
            if (!saveResult.Success)
            {
                _logger.LogError("Failed to save updated resources for culture {Culture}: {Error}",
                    culture, saveResult.ErrorMessage);
                return false;
            }

            // 更新内存中的资源
            if (_resources.ContainsKey(culture))
            {
                _resources[culture][key] = value;
            }

            _logger.LogInformation("Updated resource {Key} for culture {Culture}", key, culture);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating resource {Key} for culture {Culture}", key, culture);
            return false;
        }
    }

    public async Task<Dictionary<string, string>> GetCurrentResourcesAsync()
    {
        var culture = _currentCulture.Name;

        if (!_resources.ContainsKey(culture))
        {
            await LoadResourcesAsync(culture);
        }

        return _resources.TryGetValue(culture, out var resources)
            ? new Dictionary<string, string>(resources)
            : new Dictionary<string, string>();
    }

    public async Task<bool> ReloadResourcesAsync()
    {
        try
        {
            var culture = _currentCulture.Name;
            _resources.Remove(culture); // 强制重新加载
            await LoadResourcesAsync(culture);

            // 触发更新事件
            OnLanguageChanged?.Invoke();

            _logger.LogInformation("Reloaded resources for culture {Culture}", culture);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reloading resources");
            return false;
        }
    }
}