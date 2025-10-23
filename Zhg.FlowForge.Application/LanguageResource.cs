using System;
using System.Collections.Generic;
using System.Text;

namespace Zhg.FlowForge.Domain;

public class LanguageResource : IEquatable<LanguageResource>
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Culture { get; set; } = string.Empty;
    public string Module { get; set; } = "Common";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string Description { get; set; } = string.Empty;
    public ResourceType Type { get; set; } = ResourceType.Text;

    public bool Equals(LanguageResource? other)
    {
        return Key == other?.Key && Culture == other?.Culture;
    }

    public override bool Equals(object? obj) => Equals(obj as LanguageResource);
    public override int GetHashCode() => HashCode.Combine(Key, Culture);
}

public enum ResourceType
{
    Text,
    Html,
    Markdown,
    FormatString
}

public class LanguageInfo
{
    public string Culture { get; set; } = string.Empty;
    public string NativeName { get; set; } = string.Empty;
    public string EnglishName { get; set; } = string.Empty;
    public bool IsRTL { get; set; }
    public string Icon { get; set; } = "🌐";
    public int ResourceCount { get; set; }
}

public class LocalizationOptions
{
    public string DefaultCulture { get; set; } = "zh-CN";
    public string FallbackCulture { get; set; } = "en";
    public bool UseFallback { get; set; } = true;
    public bool CacheResources { get; set; } = true;
    public TimeSpan CacheDuration { get; set; } = TimeSpan.FromHours(1);
    public string PreferredSource { get; set; } = "Auto"; // Auto, JSON, XML, SQLite, API
}