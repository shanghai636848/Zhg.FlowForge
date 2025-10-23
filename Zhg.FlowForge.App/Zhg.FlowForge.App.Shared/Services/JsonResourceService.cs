using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Zhg.FlowForge.App.Shared.Interfaces;
using Zhg.FlowForge.App.Shared.Models;

namespace Zhg.FlowForge.App.Shared.Services;



public class ResourceSaveResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}

public class JsonResourceService : IJsonResourceService
{
    private readonly IFileSystemService _fileSystem;
    private readonly IOptions<AppSettings> _settings;
    private readonly ILogger<JsonResourceService> _logger;
    private string _resourcesDirectory;

    public JsonResourceService(
        IFileSystemService fileSystem,
        IOptions<AppSettings> settings,
        ILogger<JsonResourceService> logger)
    {
        _fileSystem = fileSystem;
        _settings = settings;
        _logger = logger;

        // 构建资源目录路径
        _resourcesDirectory = _fileSystem.CombinePaths(
            _fileSystem.GetAppDataDirectory(),
            _settings.Value.JsonResourcesPath
        );
    }

    public async Task<bool> InitializeAsync()
    {
        try
        {
            // 确保资源目录存在
            if (!await _fileSystem.DirectoryExistsAsync(_resourcesDirectory))
            {
                await _fileSystem.CreateDirectoryAsync(_resourcesDirectory);
                _logger.LogInformation("Created resources directory: {Directory}", _resourcesDirectory);
            }

            // 确保默认资源文件存在
            await EnsureDefaultResourcesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize JSON resource service");
            return false;
        }
    }

    private async Task EnsureDefaultResourcesAsync()
    {
        var defaultResources = GetBuiltInResources();

        foreach (var cultureResources in defaultResources)
        {
            var filePath = GetResourceFilePath(cultureResources.Key);

            // 如果文件不存在，创建默认资源文件
            if (!await _fileSystem.FileExistsAsync(filePath))
            {
                var result = await SaveResourcesAsync(cultureResources.Key, cultureResources.Value);
                if (result.Success)
                {
                    _logger.LogInformation("Created default resource file for culture: {Culture}", cultureResources.Key);
                }
            }
        }
    }

    private Dictionary<string, Dictionary<string, string>> GetBuiltInResources()
    {
        return new Dictionary<string, Dictionary<string, string>>
        {
            ["zh-CN"] = new Dictionary<string, string>
            {
                ["nav.home"] = "首页",
                ["nav.products"] = "产品",
                ["nav.about"] = "关于我们",
                ["nav.contact"] = "联系我们",
                ["action.login"] = "登录",
                ["action.register"] = "注册",
                ["logo.text"] = "",
                ["language.english"] = "English",
                ["language.chinese"] = "中文",
                ["language.spanish"] = "Español",
                ["footer.copyright"] = "© 2024 流程锻造平台. 保留所有权利。",
                ["menu.toggle"] = "切换菜单"
            },
            ["en"] = new Dictionary<string, string>
            {
                ["nav.home"] = "Home",
                ["nav.products"] = "",
                ["nav.about"] = "About Us",
                ["nav.contact"] = "Contact",
                ["action.login"] = "Login",
                ["action.register"] = "Register",
                ["logo.text"] = "Flow Forge",
                ["language.english"] = "English",
                ["language.chinese"] = "中文",
                ["language.spanish"] = "Español",
                ["footer.copyright"] = "© 2024 Flow Forge Platform. All rights reserved.",
                ["menu.toggle"] = "Toggle Menu"
            },
            ["es"] = new Dictionary<string, string>
            {
                ["nav.home"] = "Inicio",
                ["nav.products"] = "Productos",
                ["nav.about"] = "Nosotros",
                ["nav.contact"] = "Contacto",
                ["action.login"] = "Iniciar Sesión",
                ["action.register"] = "Registrarse",
                ["logo.text"] = "Forja de Flujo",
                ["language.english"] = "Inglés",
                ["language.chinese"] = "Chino",
                ["language.spanish"] = "Español",
                ["footer.copyright"] = "© 2024 Plataforma Forja de Flujo. Todos los derechos reservados.",
                ["menu.toggle"] = "Alternar Menú"
            }
        };
    }

    private string GetResourceFilePath(string culture)
    {
        return _fileSystem.CombinePaths(_resourcesDirectory, $"{culture}.json");
    }

    public async Task<ResourceLoadResult> LoadResourcesAsync(string culture)
    {
        var result = new ResourceLoadResult();
        var filePath = GetResourceFilePath(culture);

        try
        {
            // 首先尝试从文件加载
            if (await _fileSystem.FileExistsAsync(filePath))
            {
                var json = await _fileSystem.ReadFileAsync(filePath);
                var resources = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                if (resources != null)
                {
                    result.Resources = resources;
                    result.Success = true;
                    result.Source = "JSON";
                    _logger.LogDebug("Loaded {Count} resources for culture {Culture} from JSON file", resources.Count, culture);
                    return result;
                }
            }

            // 文件不存在或解析失败，使用内置资源
            if (GetBuiltInResources().ContainsKey(culture))
            {
                result.Resources = GetBuiltInResources()[culture];
                result.Success = true;
                result.Source = "BuiltIn";
                _logger.LogInformation("Using built-in resources for culture: {Culture}", culture);

                // 自动保存内置资源到文件
                _ = Task.Run(async () =>
                {
                    await SaveResourcesAsync(culture, result.Resources);
                });
            }
            else
            {
                result.Success = false;
                result.ErrorMessage = $"No resources found for culture: {culture}";
                _logger.LogWarning("No resources available for culture: {Culture}", culture);
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Error loading resources for culture {Culture}", culture);
        }

        return result;
    }

    public async Task<ResourceSaveResult> SaveResourcesAsync(string culture, Dictionary<string, string> resources)
    {
        var result = new ResourceSaveResult();

        try
        {
            var filePath = GetResourceFilePath(culture);
            var json = JsonSerializer.Serialize(resources, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            await _fileSystem.WriteFileAsync(filePath, json);
            result.Success = true;

            _logger.LogInformation("Saved {Count} resources for culture {Culture} to JSON file", resources.Count, culture);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Error saving resources for culture {Culture}", culture);
        }

        return result;
    }

    public async Task<List<string>> GetAvailableCulturesAsync()
    {
        var cultures = new List<string>();

        try
        {
            // 从文件系统获取可用的文化
            if (await _fileSystem.DirectoryExistsAsync(_resourcesDirectory))
            {
                var files = await _fileSystem.GetFilesAsync(_resourcesDirectory, "*.json");
                foreach (var file in files)
                {
                    var culture = Path.GetFileNameWithoutExtension(file);
                    if (!cultures.Contains(culture))
                    {
                        cultures.Add(culture);
                    }
                }
            }

            // 添加内置的文化（如果不存在）
            foreach (var culture in GetBuiltInResources().Keys)
            {
                if (!cultures.Contains(culture))
                {
                    cultures.Add(culture);
                }
            }

            _logger.LogDebug("Found {Count} available cultures: {Cultures}", cultures.Count, string.Join(", ", cultures));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available cultures");
            // 返回内置文化作为后备
            cultures.AddRange(GetBuiltInResources().Keys);
        }

        return cultures.Distinct().ToList();
    }
}