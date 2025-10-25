using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Zhg.FlowForge.App.Shared.Services;

// CodeAnalysisService.cs

public class CodeAnalysisService : ICodeAnalysisService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CodeAnalysisService> _logger;

    public CodeAnalysisService(HttpClient httpClient, ILogger<CodeAnalysisService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<Diagnostic>> AnalyzeCodeAsync(string code, string language = "csharp")
    {
        await Task.Delay(200); // 模拟分析延迟

        var diagnostics = new List<Diagnostic>();

        if (language == "csharp")
        {
            // 简单的语法检查
            if (code.Contains("Console.WritLine")) // 拼写错误
            {
                diagnostics.Add(new Diagnostic
                {
                    Severity = "Error",
                    Code = "CS0103",
                    Message = "'WritLine' 不存在，您是否指的是 'WriteLine'?",
                    Line = GetLineNumber(code, "WritLine"),
                    Column = 1,
                    File = "current"
                });
            }

            if (!code.Contains("namespace"))
            {
                diagnostics.Add(new Diagnostic
                {
                    Severity = "Warning",
                    Code = "IDE0130",
                    Message = "缺少 namespace 声明",
                    Line = 1,
                    Column = 1,
                    File = "current"
                });
            }

            // 检查未使用的 using
            var usingLines = code.Split('\n')
                .Select((line, index) => new { line, index })
                .Where(x => x.line.TrimStart().StartsWith("using "))
                .ToList();
            foreach (var usingLine in usingLines)
            {
                var usedNamespace = usingLine.line.Trim()
                    .Replace("using ", "")
                    .Replace(";", "")
                    .Trim();

                if (!string.IsNullOrEmpty(usedNamespace) &&
                    !code.Substring(usingLine.line.Length).Contains(usedNamespace.Split('.').Last()))
                {
                    diagnostics.Add(new Diagnostic
                    {
                        Severity = "Info",
                        Code = "IDE0005",
                        Message = $"不需要使用指令 '{usedNamespace}'",
                        Line = usingLine.index + 1,
                        Column = 1,
                        File = "current"
                    });
                }
            }
        }

        return diagnostics;
    }
    public async Task<List<CodeSymbol>> GetCodeOutlineAsync(string code, string language = "csharp")
    {
        await Task.Delay(100);

        var symbols = new List<CodeSymbol>();

        if (language == "csharp")
        {
            var lines = code.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();

                // 检测类
                if (line.Contains("class ") && !line.StartsWith("//"))
                {
                    var className = ExtractName(line, "class");
                    symbols.Add(new CodeSymbol
                    {
                        Name = className,
                        Kind = "Class",
                        Line = i + 1,
                        Column = 1,
                        Modifiers = ExtractModifiers(line),
                        Children = new List<CodeSymbol>()
                    });
                }

                // 检测接口
                if (line.Contains("interface ") && !line.StartsWith("//"))
                {
                    var interfaceName = ExtractName(line, "interface");
                    symbols.Add(new CodeSymbol
                    {
                        Name = interfaceName,
                        Kind = "Interface",
                        Line = i + 1,
                        Column = 1,
                        Modifiers = ExtractModifiers(line),
                        Children = new List<CodeSymbol>()
                    });
                }

                // 检测方法
                if ((line.Contains("(") && line.Contains(")") &&
                    (line.Contains("void ") || line.Contains("async ") ||
                     line.Contains("Task") || line.Contains("string ") ||
                     line.Contains("int ") || line.Contains("bool "))) &&
                    !line.StartsWith("//"))
                {
                    var methodName = ExtractMethodName(line);
                    if (!string.IsNullOrEmpty(methodName))
                    {
                        symbols.Add(new CodeSymbol
                        {
                            Name = methodName,
                            Kind = "Method",
                            Line = i + 1,
                            Column = 1,
                            Modifiers = ExtractModifiers(line)
                        });
                    }
                }

                // 检测属性
                if (line.Contains(" { get; ") && !line.StartsWith("//"))
                {
                    var propertyName = ExtractPropertyName(line);
                    if (!string.IsNullOrEmpty(propertyName))
                    {
                        symbols.Add(new CodeSymbol
                        {
                            Name = propertyName,
                            Kind = "Property",
                            Line = i + 1,
                            Column = 1,
                            Modifiers = ExtractModifiers(line)
                        });
                    }
                }
            }
        }

        return symbols;
    }

    public async Task<string> FormatCodeAsync(string code, string language = "csharp")
    {
        await Task.Delay(100);

        // 简单的格式化 - 实际项目中应使用专业的格式化工具
        var lines = code.Split('\n');
        var formatted = new System.Text.StringBuilder();
        int indentLevel = 0;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            if (trimmed.StartsWith("}"))
            {
                indentLevel = Math.Max(0, indentLevel - 1);
            }

            if (!string.IsNullOrWhiteSpace(trimmed))
            {
                formatted.AppendLine(new string(' ', indentLevel * 4) + trimmed);
            }
            else
            {
                formatted.AppendLine();
            }

            if (trimmed.EndsWith("{") && !trimmed.Contains("}"))
            {
                indentLevel++;
            }
        }

        return formatted.ToString();
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
            Column = 8,
            Context = $"使用了 {symbol}"
        },
        new CodeReference
        {
            File = "Startup.cs",
            Line = 23,
            Column = 12,
            Context = $"调用了 {symbol}"
        }
    };
    }

    public async Task<List<CodeSuggestion>> GetSuggestionsAsync(string code, int position)
    {
        await Task.Delay(50);

        var suggestions = new List<CodeSuggestion>
    {
        new CodeSuggestion
        {
            Label = "Console.WriteLine",
            Kind = "Method",
            InsertText = "Console.WriteLine($\"${1:text}\");",
            Documentation = "将指定的数据写入标准输出流"
        },
        new CodeSuggestion
        {
            Label = "Task.Run",
            Kind = "Method",
            InsertText = "Task.Run(() => ${1:/* code */});",
            Documentation = "在线程池线程上异步运行代码"
        },
        new CodeSuggestion
        {
            Label = "async",
            Kind = "Keyword",
            InsertText = "async ",
            Documentation = "用于声明异步方法"
        }
    };

        return suggestions;
    }

    // 辅助方法
    private int GetLineNumber(string code, string search)
    {
        var lines = code.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains(search))
            {
                return i + 1;
            }
        }
        return 1;
    }

    private string ExtractName(string line, string keyword)
    {
        var index = line.IndexOf(keyword);
        if (index == -1) return "";

        var afterKeyword = line.Substring(index + keyword.Length).Trim();
        var endIndex = afterKeyword.IndexOfAny(new[] { ' ', ':', '{', '<' });

        return endIndex > 0 ? afterKeyword.Substring(0, endIndex) : afterKeyword;
    }

    private string ExtractMethodName(string line)
    {
        var openParen = line.IndexOf('(');
        if (openParen == -1) return "";

        var beforeParen = line.Substring(0, openParen).Trim();
        var parts = beforeParen.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

        return parts.Length > 0 ? parts[^1] : "";
    }

    private string ExtractPropertyName(string line)
    {
        var getIndex = line.IndexOf("{ get;");
        if (getIndex == -1) return "";

        var beforeGet = line.Substring(0, getIndex).Trim();
        var parts = beforeGet.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

        return parts.Length > 0 ? parts[^1] : "";
    }

    private string ExtractModifiers(string line)
    {
        var modifiers = new List<string>();
        var keywords = new[] { "public", "private", "protected", "internal", "static", "async", "virtual", "override", "abstract" };

        foreach (var keyword in keywords)
        {
            if (line.Contains($"{keyword} "))
            {
                modifiers.Add(keyword);
            }
        }

        return string.Join(" ", modifiers);
    }
}


///// <summary>
///// 诊断信息
///// </summary>
//public class Diagnostic
//{
//    public string Severity { get; set; } = ""; // Error, Warning, Info
//    public string Code { get; set; } = "";
//    public string Message { get; set; } = "";
//    public int Line { get; set; }
//    public int Column { get; set; }
//    public string File { get; set; } = "";
//}