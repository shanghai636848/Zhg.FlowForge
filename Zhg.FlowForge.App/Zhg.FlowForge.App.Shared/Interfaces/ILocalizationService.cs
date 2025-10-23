using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Zhg.FlowForge.App.Shared.Interfaces;

public interface ILocalizationService
{
    CultureInfo CurrentCulture { get; }
    string CurrentCultureName { get; }
    event Action OnLanguageChanged;
    Task InitializeAsync();
    Task SetLanguageAsync(string culture);
    string GetString(string key);
    string this[string key] { get; }
    Task<List<string>> GetAvailableCulturesAsync();
    Task<bool> AddOrUpdateResourceAsync(string key, string value, string culture = "");
    Task<Dictionary<string, string>> GetCurrentResourcesAsync();
    Task<bool> ReloadResourcesAsync();
}