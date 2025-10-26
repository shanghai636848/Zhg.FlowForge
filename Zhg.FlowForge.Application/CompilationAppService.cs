using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Zhg.FlowForge.Application.Contract;

namespace Zhg.FlowForge.Application;

/// <summary>
/// 编译应用服务实现
/// </summary>
public class CompilationService : ICompilationService
{
    private readonly ILogger<CompilationService> _logger;

    public CompilationService(ILogger<CompilationService> logger)
    {
        _logger = logger;
    }

    public async Task<CompilationResultDto> CompileAsync(
        string projectId,
        CompilationOptionsDto options,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        var result = new CompilationResultDto();

        try
        {
            progress?.Report("正在准备编译环境...");
            await Task.Delay(500, cancellationToken);

            progress?.Report($"配置: {options.Configuration}, 平台: {options.Platform}");
            progress?.Report("正在分析项目依赖...");
            await Task.Delay(300, cancellationToken);

            progress?.Report("正在编译源代码...");

            // 模拟编译过程
            var compileResult = await SimulateCompilationAsync(projectId, options, progress, cancellationToken);

            result.Success = compileResult.Success;
            result.AssemblyName = $"{projectId}.dll";
            result.OutputPath = $"/bin/{options.Configuration}/{projectId}.dll";
            result.AssemblySize = 524288; // 512 KB
            result.Diagnostics = compileResult.Diagnostics;

            if (result.Success)
            {
                progress?.Report("✓ 编译成功");
                progress?.Report($"生成程序集: {result.AssemblyName}");
                progress?.Report($"输出路径: {result.OutputPath}");
            }
            else
            {
                progress?.Report($"✗ 编译失败: {result.ErrorCount} 个错误");
            }

            result.Duration = DateTime.UtcNow - startTime;

            _logger.LogInformation(
                "编译完成: {ProjectId}, 成功: {Success}, 耗时: {Duration}ms",
                projectId,
                result.Success,
                result.Duration.TotalMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "编译项目 {ProjectId} 时发生错误", projectId);

            result.Success = false;
            result.Duration = DateTime.UtcNow - startTime;
            result.Diagnostics.Add(new DiagnosticDto
            {
                Severity = "Error",
                Code = "CS9999",
                Message = $"编译异常: {ex.Message}",
                File = "",
                Line = 0,
                Column = 0
            });

            return result;
        }
    }

    public async Task CleanAsync(string projectId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("清理项目 {ProjectId}", projectId);
        await Task.Delay(500, cancellationToken);
        _logger.LogInformation("项目 {ProjectId} 清理完成", projectId);
    }

    public async Task<CompilationResultDto> RebuildAsync(
        string projectId,
        CompilationOptionsDto options,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        progress?.Report("正在清理项目...");
        await CleanAsync(projectId, cancellationToken);

        progress?.Report("开始重新构建...");
        return await CompileAsync(projectId, options, progress, cancellationToken);
    }

    public async Task<List<DiagnosticDto>> AnalyzeCodeAsync(
        string code,
        string language,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);

        var diagnostics = new List<DiagnosticDto>();
        var lines = code.Split('\n');
        var random = new Random();

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            // 检查未使用的 using
            if (line.TrimStart().StartsWith("using ") && i < 10 && random.Next(10) > 7)
            {
                diagnostics.Add(new DiagnosticDto
                {
                    Severity = "Warning",
                    Code = "CS8019",
                    Message = "不必要的 using 指令",
                    File = "current",
                    Line = i + 1,
                    Column = 1
                });
            }

            // 检查可能的 null 引用
            if (line.Contains('.') && !line.Contains("?.") && random.Next(20) > 18)
            {
                diagnostics.Add(new DiagnosticDto
                {
                    Severity = "Warning",
                    Code = "CS8602",
                    Message = "可能取消引用 null 引用",
                    File = "current",
                    Line = i + 1,
                    Column = line.IndexOf('.') + 1
                });
            }
        }

        return diagnostics;
    }

    #region Private Methods

    private async Task<(bool Success, List<DiagnosticDto> Diagnostics)> SimulateCompilationAsync(
        string projectId,
        CompilationOptionsDto options,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        var diagnostics = new List<DiagnosticDto>();
        var random = new Random();

        var steps = new[]
        {
            "正在还原 NuGet 包...",
            "正在编译 Domain 层...",
            "正在编译 Application 层...",
            "正在编译 Infrastructure 层...",
            "正在编译 API 层...",
            "正在生成程序集..."
        };

        foreach (var step in steps)
        {
            progress?.Report(step);
            await Task.Delay(random.Next(200, 500), cancellationToken);

            // 随机生成一些警告
            if (random.Next(10) > 7)
            {
                var warning = GenerateRandomWarning(random);
                diagnostics.Add(warning);
                progress?.Report($"⚠ {warning.Code}: {warning.Message}");
            }
        }

        // 90% 成功率
        bool success = random.Next(10) > 0;

        if (!success)
        {
            var errorCount = random.Next(1, 4);
            for (int i = 0; i < errorCount; i++)
            {
                var error = GenerateRandomError(random);
                diagnostics.Add(error);
                progress?.Report($"✗ {error.Code}: {error.Message}");
            }
        }

        return (success, diagnostics);
    }

    private DiagnosticDto GenerateRandomWarning(Random random)
    {
        var warnings = new[]
        {
            ("CS8019", "不必要的 using 指令"),
            ("CS0649", "字段从未赋值，始终为 null"),
            ("CS8602", "可能取消引用 null 引用"),
            ("CS8618", "退出构造函数时不可为 null 的字段必须包含非 null 值"),
            ("IDE0005", "不需要 using 指令")
        };

        var (code, message) = warnings[random.Next(warnings.Length)];

        return new DiagnosticDto
        {
            Severity = "Warning",
            Code = code,
            Message = message,
            File = $"File{random.Next(1, 10)}.cs",
            Line = random.Next(1, 100),
            Column = random.Next(1, 50)
        };
    }

    private DiagnosticDto GenerateRandomError(Random random)
    {
        var errors = new[]
        {
            ("CS0246", "找不到类型或命名空间名"),
            ("CS1002", "应输入 ;"),
            ("CS0103", "当前上下文中不存在名称"),
            ("CS0029", "无法将类型隐式转换为"),
            ("CS1061", "未包含的定义")
        };

        var (code, message) = errors[random.Next(errors.Length)];

        return new DiagnosticDto
        {
            Severity = "Error",
            Code = code,
            Message = message,
            File = $"File{random.Next(1, 10)}.cs",
            Line = random.Next(1, 100),
            Column = random.Next(1, 50)
        };
    }

    #endregion
}