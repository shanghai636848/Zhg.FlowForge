using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Zhg.FlowForge.App.Shared.Services;




public class CompilationService : ICompilationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CompilationService> _logger;
    private readonly Dictionary<string, List<CompilationHistory>> _compilationHistory = new();
    private readonly Dictionary<string, CompilationCache> _compilationCache = new();

    public CompilationService(
        HttpClient httpClient,
        ILogger<CompilationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<CompilationResult> CompileAsync(
        string projectId,
        CompilationOptions options,
        IProgress<string>? progress = null)
    {
        var result = new CompilationResult
        {
            StartTime = DateTime.Now
        };

        try
        {
            progress?.Report("正在准备编译环境...");
            await Task.Delay(500); // 模拟准备过程

            progress?.Report($"配置: {options.Configuration}, 平台: {options.Platform}");
            progress?.Report("正在分析项目依赖...");
            await Task.Delay(300);

            progress?.Report("正在编译源代码...");

            // 模拟编译过程
            var compileResult = await SimulateCompilation(projectId, options, progress);

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

                // 更新缓存
                UpdateCompilationCache(projectId, options);
            }
            else
            {
                progress?.Report($"✗ 编译失败: {result.ErrorCount} 个错误");
            }

            result.EndTime = DateTime.Now;

            // 记录编译历史
            AddCompilationHistory(projectId, result, options);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "编译项目 {ProjectId} 时发生错误", projectId);

            result.Success = false;
            result.EndTime = DateTime.Now;
            result.Diagnostics.Add(new Diagnostic
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

    public async Task CleanAsync(string projectId)
    {
        _logger.LogInformation("清理项目 {ProjectId}", projectId);

        // 模拟清理过程
        await Task.Delay(500);

        // 清除缓存
        _compilationCache.Remove(projectId);

        _logger.LogInformation("项目 {ProjectId} 清理完成", projectId);
    }

    public async Task<CompilationResult> RebuildAsync(
        string projectId,
        CompilationOptions options,
        IProgress<string>? progress = null)
    {
        progress?.Report("正在清理项目...");
        await CleanAsync(projectId);

        progress?.Report("开始重新构建...");
        return await CompileAsync(projectId, options, progress);
    }

    public async Task<CompilationResult> CompileFileAsync(
        string projectId,
        string filePath,
        string content)
    {
        var result = new CompilationResult
        {
            StartTime = DateTime.Now
        };

        try
        {
            _logger.LogInformation("编译文件 {FilePath}", filePath);

            // 模拟单文件编译
            var diagnostics = await AnalyzeCodeAsync(content, "csharp");

            result.Success = !diagnostics.Any(d => d.Severity == "Error");
            result.Diagnostics = diagnostics;
            result.EndTime = DateTime.Now;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "编译文件 {FilePath} 时发生错误", filePath);

            result.Success = false;
            result.EndTime = DateTime.Now;
            result.Diagnostics.Add(new Diagnostic
            {
                Severity = "Error",
                Code = "CS9999",
                Message = $"编译异常: {ex.Message}",
                File = filePath,
                Line = 0,
                Column = 0
            });

            return result;
        }
    }

    public async Task<List<CompilationHistory>> GetCompilationHistoryAsync(string projectId)
    {
        await Task.CompletedTask;

        if (_compilationHistory.TryGetValue(projectId, out var history))
        {
            return history.OrderByDescending(h => h.Timestamp).ToList();
        }

        return new List<CompilationHistory>();
    }

    public async Task<List<Diagnostic>> AnalyzeCodeAsync(string code, string language)
    {
        await Task.Delay(100); // 模拟分析延迟

        var diagnostics = new List<Diagnostic>();

        // 简单的代码分析示例
        var lines = code.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            // 检查未使用的 using
            if (line.TrimStart().StartsWith("using ") && i < 10)
            {
                if (new Random().Next(10) > 7)
                {
                    diagnostics.Add(new Diagnostic
                    {
                        Severity = "Warning",
                        Code = "CS8019",
                        Message = "不必要的 using 指令",
                        File = "current",
                        Line = i + 1,
                        Column = 1,
                        HelpLink = "https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/cs8019"
                    });
                }
            }

            // 检查可能的 null 引用
            if (line.Contains("?.") == false && line.Contains(".") && new Random().Next(20) > 18)
            {
                diagnostics.Add(new Diagnostic
                {
                    Severity = "Warning",
                    Code = "CS8602",
                    Message = "可能取消引用 null 引用",
                    File = "current",
                    Line = i + 1,
                    Column = line.IndexOf('.') + 1,
                    HelpLink = "https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/cs8602"
                });
            }

            // 检查命名约定
            if (line.Contains("class ") && line.Contains("class _"))
            {
                diagnostics.Add(new Diagnostic
                {
                    Severity = "Warning",
                    Code = "CA1707",
                    Message = "标识符不应包含下划线",
                    File = "current",
                    Line = i + 1,
                    Column = line.IndexOf("class ") + 6
                });
            }
        }

        return diagnostics;
    }

    public async Task<CompilationCache?> GetCompilationCacheAsync(string projectId)
    {
        await Task.CompletedTask;
        return _compilationCache.TryGetValue(projectId, out var cache) ? cache : null;
    }

    public async Task ClearCompilationCacheAsync(string projectId)
    {
        await Task.CompletedTask;
        _compilationCache.Remove(projectId);
        _logger.LogInformation("清除项目 {ProjectId} 的编译缓存", projectId);
    }

    // 私有辅助方法

    private async Task<(bool Success, List<Diagnostic> Diagnostics)> SimulateCompilation(
        string projectId,
        CompilationOptions options,
        IProgress<string>? progress)
    {
        var diagnostics = new List<Diagnostic>();
        var random = new Random();

        // 模拟编译步骤
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
            await Task.Delay(random.Next(200, 500));

            // 随机生成一些警告
            if (random.Next(10) > 7)
            {
                var warning = GenerateRandomWarning(random);
                diagnostics.Add(warning);
                progress?.Report($"⚠ {warning.Code}: {warning.Message}");
            }
        }

        // 决定是否编译成功（90% 成功率）
        bool success = random.Next(10) > 0;

        if (!success)
        {
            // 生成一些错误
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

    private Diagnostic GenerateRandomWarning(Random random)
    {
        var warnings = new[]
        {
            ("CS8019", "不必要的 using 指令"),
            ("CS0649", "字段从未赋值，始终为 null"),
            ("CS8602", "可能取消引用 null 引用"),
            ("CS8618", "退出构造函数时不可为 null 的字段必须包含非 null 值"),
            ("IDE0005", "不需要 using 指令"),
            ("CA1822", "成员不访问实例数据可标记为 static")
        };

        var (code, message) = warnings[random.Next(warnings.Length)];

        return new Diagnostic
        {
            Severity = "Warning",
            Code = code,
            Message = message,
            File = $"File{random.Next(1, 10)}.cs",
            Line = random.Next(1, 100),
            Column = random.Next(1, 50)
        };
    }

    private Diagnostic GenerateRandomError(Random random)
    {
        var errors = new[]
        {
            ("CS0246", "找不到类型或命名空间名"),
            ("CS1002", "应输入 ;"),
            ("CS0103", "当前上下文中不存在名称"),
            ("CS0029", "无法将类型隐式转换为"),
            ("CS1061", "未包含的定义"),
            ("CS0161", "并非所有代码路径都返回值")
        };

        var (code, message) = errors[random.Next(errors.Length)];

        return new Diagnostic
        {
            Severity = "Error",
            Code = code,
            Message = message,
            File = $"File{random.Next(1, 10)}.cs",
            Line = random.Next(1, 100),
            Column = random.Next(1, 50)
        };
    }

    private void AddCompilationHistory(
        string projectId,
        CompilationResult result,
        CompilationOptions options)
    {
        if (!_compilationHistory.ContainsKey(projectId))
        {
            _compilationHistory[projectId] = new List<CompilationHistory>();
        }

        _compilationHistory[projectId].Add(new CompilationHistory
        {
            Timestamp = result.StartTime,
            Success = result.Success,
            Configuration = options.Configuration,
            Duration = result.Duration,
            ErrorCount = result.ErrorCount,
            WarningCount = result.WarningCount
        });

        // 保留最近 50 次编译记录
        if (_compilationHistory[projectId].Count > 50)
        {
            _compilationHistory[projectId] = _compilationHistory[projectId]
                .OrderByDescending(h => h.Timestamp)
                .Take(50)
                .ToList();
        }
    }

    private void UpdateCompilationCache(string projectId, CompilationOptions options)
    {
        _compilationCache[projectId] = new CompilationCache
        {
            ProjectId = projectId,
            LastCompiled = DateTime.Now,
            Configuration = options.Configuration,
            CachedFiles = new List<string>(),
            CacheSize = 0
        };
    }
}
