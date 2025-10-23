using System;
using System.Collections.Generic;
using System.Text;

namespace Zhg.FlowForge.App.Shared.Models;

public class AppSettings
{
    public string DefaultCulture { get; set; } = "zh-CN";
    public string ResourceSource { get; set; } = "JSON"; // JSON 或 SQLite
    public string JsonResourcesPath { get; set; } = "localization"; // 相对路径
    public bool UseBuiltInResources { get; set; } = true;
}

public class ResourceLoadResult
{
    public bool Success { get; set; }
    public Dictionary<string, string> Resources { get; set; } = new();
    public string ErrorMessage { get; set; } = string.Empty;
    public string Source { get; set; } = "BuiltIn"; // BuiltIn, JSON, SQLite
}