using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zhg.FlowForge.App.Shared.Services;

/// <summary>
/// 代码编译服务接口
/// </summary>
public interface ICompilationService
{
    /// <summary>
    /// 编译项目
    /// </summary>
    Task<CompilationResult> CompileAsync(
        string projectId,
        CompilationOptions options,
        IProgress<string>? progress = null);

    /// <summary>
    /// 清理项目构建输出
    /// </summary>
    Task CleanAsync(string projectId);

    /// <summary>
    /// 重新构建项目
    /// </summary>
    Task<CompilationResult> RebuildAsync(
        string projectId,
        CompilationOptions options,
        IProgress<string>? progress = null);

    /// <summary>
    /// 编译单个文件
    /// </summary>
    Task<CompilationResult> CompileFileAsync(
        string projectId,
        string filePath,
        string content);

    /// <summary>
    /// 获取编译历史
    /// </summary>
    Task<List<CompilationHistory>> GetCompilationHistoryAsync(string projectId);

    /// <summary>
    /// 分析代码（无需完整编译）
    /// </summary>
    Task<List<Diagnostic>> AnalyzeCodeAsync(string code, string language);

    /// <summary>
    /// 获取编译缓存信息
    /// </summary>
    Task<CompilationCache?> GetCompilationCacheAsync(string projectId);

    /// <summary>
    /// 清除编译缓存
    /// </summary>
    Task ClearCompilationCacheAsync(string projectId);
}

/// <summary>
/// 编译选项
/// </summary>
public class CompilationOptions
{
    /// <summary>
    /// 配置类型 (Debug/Release)
    /// </summary>
    public string Configuration { get; set; } = "Debug";

    /// <summary>
    /// 目标平台 (AnyCPU/x64/x86)
    /// </summary>
    public string Platform { get; set; } = "AnyCPU";

    /// <summary>
    /// 警告视为错误
    /// </summary>
    public bool TreatWarningsAsErrors { get; set; }

    /// <summary>
    /// 优化代码
    /// </summary>
    public bool Optimize { get; set; }

    /// <summary>
    /// 生成调试信息
    /// </summary>
    public bool DebugSymbols { get; set; } = true;

    /// <summary>
    /// 输出路径
    /// </summary>
    public string? OutputPath { get; set; }

    /// <summary>
    /// 并行编译
    /// </summary>
    public bool ParallelBuild { get; set; } = true;

    /// <summary>
    /// 详细输出
    /// </summary>
    public bool VerboseOutput { get; set; }
}

/// <summary>
/// 编译结果
/// </summary>
public class CompilationResult
{
    public bool Success { get; set; }
    public string AssemblyName { get; set; } = "";
    public string OutputPath { get; set; } = "";
    public long AssemblySize { get; set; }
    public List<Diagnostic> Diagnostics { get; set; } = new();
    public int ErrorCount => Diagnostics.Count(d => d.Severity == "Error");
    public int WarningCount => Diagnostics.Count(d => d.Severity == "Warning");
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration => EndTime - StartTime;
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// 诊断信息
/// </summary>
public class Diagnostic
{
    public string Severity { get; set; } = ""; // Error, Warning, Info
    public string Code { get; set; } = "";
    public string Message { get; set; } = "";
    public string File { get; set; } = "";
    public int Line { get; set; }
    public int Column { get; set; }
    public int EndLine { get; set; }
    public int EndColumn { get; set; }
    public string? HelpLink { get; set; }
}

/// <summary>
/// 编译历史记录
/// </summary>
public class CompilationHistory
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; }
    public bool Success { get; set; }
    public string Configuration { get; set; } = "";
    public TimeSpan Duration { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
}

/// <summary>
/// 编译缓存
/// </summary>
public class CompilationCache
{
    public string ProjectId { get; set; } = "";
    public DateTime LastCompiled { get; set; }
    public string Configuration { get; set; } = "";
    public List<string> CachedFiles { get; set; } = new();
    public long CacheSize { get; set; }
}