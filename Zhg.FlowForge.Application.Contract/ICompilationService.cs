using System;
using System.Collections.Generic;
using System.Text;

namespace Zhg.FlowForge.Application.Contract;

/// <summary>
/// 编译应用服务接口
/// </summary>
public interface ICompilationService
{
    Task<CompilationResultDto> CompileAsync(
        string projectId,
        CompilationOptionsDto options,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default);

    Task CleanAsync(string projectId, CancellationToken cancellationToken = default);

    Task<CompilationResultDto> RebuildAsync(
        string projectId,
        CompilationOptionsDto options,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default);

    Task<List<DiagnosticDto>> AnalyzeCodeAsync(
        string code,
        string language,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 编译选项 DTO
/// </summary>
public class CompilationOptionsDto
{
    public string Configuration { get; set; } = "Debug";
    public string Platform { get; set; } = "AnyCPU";
    public bool TreatWarningsAsErrors { get; set; }
    public bool Optimize { get; set; }
    public bool DebugSymbols { get; set; } = true;
}

/// <summary>
/// 编译结果 DTO
/// </summary>
public class CompilationResultDto
{
    public bool Success { get; set; }
    public string AssemblyName { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;
    public long AssemblySize { get; set; }
    public List<DiagnosticDto> Diagnostics { get; set; } = new();
    public int ErrorCount => Diagnostics.Count(d => d.Severity == "Error");
    public int WarningCount => Diagnostics.Count(d => d.Severity == "Warning");
    public TimeSpan Duration { get; set; }
}

/// <summary>
/// 诊断信息 DTO
/// </summary>
public class DiagnosticDto
{
    public string Severity { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string File { get; set; } = string.Empty;
    public int Line { get; set; }
    public int Column { get; set; }
}