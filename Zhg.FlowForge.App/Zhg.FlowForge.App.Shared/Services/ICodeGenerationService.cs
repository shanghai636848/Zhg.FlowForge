using System;
using System.Collections.Generic;
using System.Text;

namespace Zhg.FlowForge.App.Shared.Services;


/// <summary>
/// 代码生成服务接口
/// </summary>
public interface ICodeGenerationService
{
    /// <summary>
    /// 生成代码
    /// </summary>
    Task<GenerationResult> GenerateAsync(GenerationRequest request, IProgress<GenerationProgress>? progress = null);

    /// <summary>
    /// 预览生成的代码
    /// </summary>
    Task<List<GeneratedFile>> PreviewAsync(GenerationRequest request);

    /// <summary>
    /// 验证生成配置
    /// </summary>
    Task<ValidationResult> ValidateConfigurationAsync(GenerationRequest request);

    /// <summary>
    /// 获取可用模板列表
    /// </summary>
    Task<List<CodeTemplate>> GetTemplatesAsync();

    /// <summary>
    /// 获取模板详情
    /// </summary>
    Task<CodeTemplate?> GetTemplateAsync(string templateId);

    /// <summary>
    /// 自定义模板
    /// </summary>
    Task<CodeTemplate> CreateCustomTemplateAsync(CreateTemplateRequest request);

    /// <summary>
    /// 获取生成历史
    /// </summary>
    Task<List<GenerationHistory>> GetGenerationHistoryAsync(string projectId);

    /// <summary>
    /// 重新生成
    /// </summary>
    Task<GenerationResult> RegenerateAsync(string historyId);
}

/// <summary>
/// 生成请求
/// </summary>
public class GenerationRequest
{
    public string ProcessId { get; set; } = "";
    public ProjectConfig Config { get; set; } = new();
    public string Template { get; set; } = "standard";
    public CodeGenerationOptions Options { get; set; } = new();
    public List<PackageDependency> Dependencies { get; set; } = new();
}

/// <summary>
/// 项目配置
/// </summary>
public class ProjectConfig
{
    public string ProjectName { get; set; } = "";
    public string Namespace { get; set; } = "";
    public string Version { get; set; } = "1.0.0";
    public string Description { get; set; } = "";
    public string TargetFramework { get; set; } = "net10.0";
    public bool EnableNullable { get; set; } = true;
    public bool ImplicitUsings { get; set; } = true;
    public string Author { get; set; } = "";
    public string Company { get; set; } = "";
    public string Copyright { get; set; } = "";
}

/// <summary>
/// 代码生成选项
/// </summary>
public class CodeGenerationOptions
{
    public string NamingStyle { get; set; } = "pascalCase";
    public string ClassPrefix { get; set; } = "";
    public string InterfacePrefix { get; set; } = "I";
    public bool UseFileScoped { get; set; } = true;
    public bool UseRecordTypes { get; set; } = true;
    public bool UseExpressionBodies { get; set; } = true;
    public bool UsePatternMatching { get; set; } = true;
    public bool GenerateAsyncMethods { get; set; } = true;
    public bool UseConfigureAwait { get; set; } = false;
    public bool UseValueTask { get; set; } = false;
    public bool GenerateLogging { get; set; } = true;
    public string LoggingFramework { get; set; } = "microsoft";
    public bool GenerateExceptionHandling { get; set; } = true;
    public bool UseCustomExceptions { get; set; } = false;
    public bool GenerateXmlComments { get; set; } = true;
    public bool IncludeExamples { get; set; } = false;
    public bool EnableAotOptimizations { get; set; } = false;
    public bool EnableTrimming { get; set; } = false;
    public bool GenerateSourceGenerators { get; set; } = false;
}

/// <summary>
/// 包依赖
/// </summary>
public class PackageDependency
{
    public string PackageId { get; set; } = "";
    public string Version { get; set; } = "";
    public string Description { get; set; } = "";
    public double Size { get; set; }
    public bool IsRequired { get; set; }
}

/// <summary>
/// 生成结果
/// </summary>
public class GenerationResult
{
    public bool Success { get; set; }
    public string Error { get; set; } = "";
    public List<GeneratedFile> Files { get; set; } = new();
    public int TotalLines { get; set; }
    public TimeSpan Duration { get; set; }
    public string ProjectPath { get; set; } = "";
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// 生成的文件
/// </summary>
public class GeneratedFile
{
    public string Path { get; set; } = "";
    public string Content { get; set; } = "";
    public int LineCount => Content.Count(c => c == '\n') + 1;
    public long Size => System.Text.Encoding.UTF8.GetByteCount(Content);
}

/// <summary>
/// 生成进度
/// </summary>
public class GenerationProgress
{
    public int Percentage { get; set; }
    public string Message { get; set; } = "";
    public string CurrentFile { get; set; } = "";
}

/// <summary>
/// 验证结果
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationError> Errors { get; set; } = new();
    public List<ValidationWarning> Warnings { get; set; } = new();
}

/// <summary>
/// 验证错误
/// </summary>
public class ValidationError
{
    public string Field { get; set; } = "";
    public string Message { get; set; } = "";
}

/// <summary>
/// 验证警告
/// </summary>
public class ValidationWarning
{
    public string Field { get; set; } = "";
    public string Message { get; set; } = "";
}

/// <summary>
/// 代码模板
/// </summary>
public class CodeTemplate
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Category { get; set; } = "";
    public string Author { get; set; } = "";
    public string Version { get; set; } = "";
    public List<string> Tags { get; set; } = new();
    public List<string> Features { get; set; } = new();
    public string IconClass { get; set; } = "";
    public bool IsCustom { get; set; }
}

/// <summary>
/// 创建模板请求
/// </summary>
public class CreateTemplateRequest
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string TemplateContent { get; set; } = "";
}

/// <summary>
/// 生成历史
/// </summary>
public class GenerationHistory
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ProjectId { get; set; } = "";
    public string Template { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public bool Success { get; set; }
    public int FilesGenerated { get; set; }
    public int LinesGenerated { get; set; }
}