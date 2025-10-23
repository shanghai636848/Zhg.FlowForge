using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Zhg.FlowForge.Domain;

// Infrastructure/Strategies/JsonResourceStrategy.cs
public class JsonResourceStrategy : BaseResourceStorageStrategy
{
    public override string Name => "JSON";
    public override int Priority => 100;

    private readonly string _resourcesPath;

    public JsonResourceStrategy(
        ILogger<JsonResourceStrategy> logger,
        IFileSystem fileSystem,
        IOptions<LocalizationOptions> options)
        : base(logger, fileSystem)
    {
        _resourcesPath = _fileSystem.CombinePaths(
            _fileSystem.GetAppDataDirectory(),
            "localization",
            "json"
        );
    }

    protected override async Task<bool> PerformInitializationAsync()
    {
        await _fileSystem.CreateDirectoryAsync(_resourcesPath);
        await EnsureDefaultResourcesAsync();
        return true;
    }

    private async Task EnsureDefaultResourcesAsync()
    {
        var cultures = new[] { "zh-CN", "en", "es" };
        foreach (var culture in cultures)
        {
            var filePath = GetResourceFilePath(culture);
            if (!await _fileSystem.FileExistsAsync(filePath))
            {
                var builtInResources = GetBuiltInResources(culture);
                var resources = builtInResources.Select(kvp => new LanguageResource
                {
                    Key = kvp.Key,
                    Value = kvp.Value,
                    Culture = culture,
                    Module = "Common",
                    Type = kvp.Key.StartsWith("error.") || kvp.Key.StartsWith("validation.")
                        ? ResourceType.FormatString
                        : ResourceType.Text
                });

                await SaveResourcesAsync(culture, resources);
            }
        }
    }

    public override async Task<IEnumerable<LanguageResource>> LoadResourcesAsync(string culture)
    {
        var filePath = GetResourceFilePath(culture);

        if (!await _fileSystem.FileExistsAsync(filePath))
        {
            _logger.LogWarning("Resource file not found for culture {Culture}: {FilePath}", culture, filePath);
            return Enumerable.Empty<LanguageResource>();
        }

        try
        {
            var json = await _fileSystem.ReadFileAsync(filePath);
            var resources = JsonSerializer.Deserialize<List<LanguageResource>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return resources ?? Enumerable.Empty<LanguageResource>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load JSON resources for culture {Culture}", culture);
            return Enumerable.Empty<LanguageResource>();
        }
    }

    public override async Task<bool> SaveResourcesAsync(string culture, IEnumerable<LanguageResource> resources)
    {
        try
        {
            var filePath = GetResourceFilePath(culture);
            var json = JsonSerializer.Serialize(resources.ToList(), new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await _fileSystem.WriteFileAsync(filePath, json);
            _logger.LogInformation("Saved {Count} resources for culture {Culture} to JSON", resources.Count(), culture);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save JSON resources for culture {Culture}", culture);
            return false;
        }
    }

    public override async Task<bool> SupportsCultureAsync(string culture)
    {
        var filePath = GetResourceFilePath(culture);
        return await _fileSystem.FileExistsAsync(filePath);
    }

    public override async Task<IEnumerable<string>> GetSupportedCulturesAsync()
    {
        var cultures = new List<string>();

        if (!await _fileSystem.DirectoryExistsAsync(_resourcesPath))
            return cultures;

        var files = await _fileSystem.GetFilesAsync(_resourcesPath, "*.json");
        foreach (var file in files)
        {
            var culture = Path.GetFileNameWithoutExtension(file);
            cultures.Add(culture);
        }

        return cultures;
    }

    private string GetResourceFilePath(string culture)
    {
        return _fileSystem.CombinePaths(_resourcesPath, $"{culture}.json");
    }
}