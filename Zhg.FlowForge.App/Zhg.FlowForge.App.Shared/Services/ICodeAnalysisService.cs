using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zhg.FlowForge.App.Shared.Services;

/// <summary>
/// 代码分析服务接口
/// </summary>
public interface ICodeAnalysisService
{
    /// <summary>
    /// 分析代码
    /// </summary>
    Task<List<Diagnostic>> AnalyzeCodeAsync(string code, string language = "csharp");

    /// <summary>
    /// 获取代码大纲（符号）
    /// </summary>
    Task<List<CodeSymbol>> GetCodeOutlineAsync(string code, string language = "csharp");

    /// <summary>
    /// 格式化代码
    /// </summary>
    Task<string> FormatCodeAsync(string code, string language = "csharp");

    /// <summary>
    /// 查找引用
    /// </summary>
    Task<List<CodeReference>> FindReferencesAsync(string symbol, string projectId);

    /// <summary>
    /// 获取代码建议
    /// </summary>
    Task<List<CodeSuggestion>> GetSuggestionsAsync(string code, int position);
}

/// <summary>
/// 代码符号
/// </summary>
public class CodeSymbol
{
    public string Name { get; set; } = "";
    public string Kind { get; set; } = ""; // Class, Method, Property, Field, Interface
    public int Line { get; set; }
    public int Column { get; set; }
    public string Modifiers { get; set; } = "";
    public List<CodeSymbol> Children { get; set; } = new();
}

/// <summary>
/// 代码引用
/// </summary>
public class CodeReference
{
    public string File { get; set; } = "";
    public int Line { get; set; }
    public int Column { get; set; }
    public string Context { get; set; } = "";
}

/// <summary>
/// 代码建议
/// </summary>
public class CodeSuggestion
{
    public string Label { get; set; } = "";
    public string Kind { get; set; } = "";
    public string InsertText { get; set; } = "";
    public string Documentation { get; set; } = "";
}