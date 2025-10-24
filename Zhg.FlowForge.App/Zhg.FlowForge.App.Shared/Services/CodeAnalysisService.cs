using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Zhg.FlowForge.App.Shared.Services;

public class CodeAnalysisService : ICodeAnalysisService
{
    private readonly ILogger<CodeAnalysisService> _logger;

    public CodeAnalysisService(ILogger<CodeAnalysisService> logger)
    {
        _logger = logger;
    }

    public async Task<List<Diagnostic>> AnalyzeCodeAsync(string code, string language = "csharp")
    {
        await Task.Delay(100); // 模拟分析延迟

        var diagnostics = new List<Diagnostic>();

        if (language == "csharp")
        {
            diagnostics.AddRange(AnalyzeCSharpCode(code));
        }

        return diagnostics;
    }

    public async Task<List<CodeSymbol>> GetCodeOutlineAsync(string code, string language = "csharp")
    {
        await Task.Delay(50);

        var symbols = new List<CodeSymbol>();

        if (language == "csharp")
        {
            symbols.AddRange(ParseCSharpSymbols(code));
        }

        return symbols;
    }

    public async Task<string> FormatCodeAsync(string code, string language = "csharp")
    {
        await Task.Delay(100);

        // 简单的格式化示例
        var lines = code.Split('\n');
        var formatted = new List<string>();
        int indentLevel = 0;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            if (trimmed.StartsWith("}"))
            {
                indentLevel = Math.Max(0, indentLevel - 1);
            }

            formatted.Add(new string(' ', indentLevel * 4) + trimmed);

            if (trimmed.EndsWith("{") && !trimmed.StartsWith("//"))
            {
                indentLevel++;
            }
        }

        return string.Join("\n", formatted);
    }

    public async Task<List<CodeReference>> FindReferencesAsync(string symbol, string projectId)
    {
        await Task.Delay(200);

        // 模拟查找引用
        return new List<CodeReference>
        {
            new CodeReference
            {
                File = "Program.cs",
                Line = 15,
                Column = 20,
                Context = $"var result = {symbol}.Execute();"
            },
            new CodeReference
            {
                File = "Startup.cs",
                Line = 32,
                Column = 15,
                Context = $"services.AddSingleton<{symbol}>();"
            }
        };
    }

    public async Task<List<CodeSuggestion>> GetSuggestionsAsync(string code, int position)
    {
        await Task.Delay(50);

        // 返回一些基本的代码建议
        return new List<CodeSuggestion>
        {
            new CodeSuggestion
            {
                Label = "Console.WriteLine",
                Kind = "Method",
                InsertText = "Console.WriteLine($\"\");",
                Documentation = "将指定的数据写入标准输出流"
            },
            new CodeSuggestion
            {
                Label = "Task.Run",
                Kind = "Method",
                InsertText = "Task.Run(() => { });",
                Documentation = "在线程池线程上异步执行操作"
            }
        };
    }

    // 私有辅助方法

    private List<Diagnostic> AnalyzeCSharpCode(string code)
    {
        var diagnostics = new List<Diagnostic>();
        var lines = code.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var lineNumber = i + 1;

            // 检查未使用的 using
            if (line.TrimStart().StartsWith("using "))
            {
                if (new Random().Next(10) > 8)
                {
                    diagnostics.Add(new Diagnostic
                    {
                        Severity = "Warning",
                        Code = "CS8019",
                        Message = "不必要的 using 指令",
                        File = "current",
                        Line = lineNumber,
                        Column = 1
                    });
                }
            }

            // 检查命名约定
            var classMatch = Regex.Match(line, @"class\s+([a-z]\w*)");
            if (classMatch.Success)
            {
                diagnostics.Add(new Diagnostic
                {
                    Severity = "Warning",
                    Code = "CA1711",
                    Message = "类名应使用 PascalCase 命名",
                    File = "current",
                    Line = lineNumber,
                    Column = classMatch.Groups[1].Index
                });
            }

            // 检查空行
            if (string.IsNullOrWhiteSpace(line) && i > 0 && i < lines.Length - 1)
            {
                if (string.IsNullOrWhiteSpace(lines[i - 1]) || string.IsNullOrWhiteSpace(lines[i + 1]))
                {
                    diagnostics.Add(new Diagnostic
                    {
                        Severity = "Info",
                        Code = "IDE0055",
                        Message = "多余的空行",
                        File = "current",
                        Line = lineNumber,
                        Column = 1
                    });
                }
            }
        }

        return diagnostics;
    }

    private List<CodeSymbol> ParseCSharpSymbols(string code)
    {
        var symbols = new List<CodeSymbol>();
        var lines = code.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            var lineNumber = i + 1;

            // 解析类
            var classMatch = Regex.Match(line, @"(public|private|internal|protected)?\s*(static)?\s*class\s+(\w+)");
            if (classMatch.Success)
            {
                symbols.Add(new CodeSymbol
                {
                    Name = classMatch.Groups[3].Value,
                    Kind = "Class",
                    Line = lineNumber,
                    Column = classMatch.Groups[3].Index,
                    Modifiers = classMatch.Groups[1].Value
                });
            }

            // 解析方法
            var methodMatch = Regex.Match(line, @"(public|private|internal|protected)\s+(static\s+)?(\w+)\s+(\w+)\s*\(");
            if (methodMatch.Success && !line.Contains("class"))
            {
                symbols.Add(new CodeSymbol
                {
                    Name = methodMatch.Groups[4].Value,
                    Kind = "Method",
                    Line = lineNumber,
                    Column = methodMatch.Groups[4].Index,
                    Modifiers = methodMatch.Groups[1].Value
                });
            }

            // 解析属性
            var propertyMatch = Regex.Match(line, @"(public|private|internal|protected)\s+(\w+)\s+(\w+)\s*\{\s*get");
            if (propertyMatch.Success)
            {
                symbols.Add(new CodeSymbol
                {
                    Name = propertyMatch.Groups[3].Value,
                    Kind = "Property",
                    Line = lineNumber,
                    Column = propertyMatch.Groups[3].Index,
                    Modifiers = propertyMatch.Groups[1].Value
                });
            }
        }

        return symbols;
    }
}