using System;
using System.Collections.Generic;
using System.Text;

namespace Zhg.FlowForge.Application.Contract;

/// <summary>
/// 代码生成应用服务接口
/// </summary>
public interface ICodeGenerationService
{
    Task<GenerationResultDto> GenerateAsync(
        GenerationRequestDto request,
        IProgress<GenerationProgressDto>? progress = null,
        CancellationToken cancellationToken = default);

    Task<List<GeneratedFileDto>> PreviewAsync(
        GenerationRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ValidationResultDto> ValidateConfigurationAsync(
        GenerationRequestDto request,
        CancellationToken cancellationToken = default);

    Task<List<CodeTemplateDto>> GetTemplatesAsync(
        CancellationToken cancellationToken = default);
}


/// <summary>
/// 生成请求 DTO
/// </summary>
public class GenerationRequestDto
{
    public string ProcessId { get; set; } = string.Empty;
    public ProjectConfigDto Config { get; set; } = new();
    public string Template { get; set; } = "standard";
    public CodeGenerationOptionsDto Options { get; set; } = new();
    public List<PackageDependencyDto> Dependencies { get; set; } = new();
}

/// <summary>
/// 项目配置 DTO
/// </summary>
public class ProjectConfigDto
{
    public string ProjectName { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public string Description { get; set; } = string.Empty;
    public string TargetFramework { get; set; } = "net10.0";
    public bool EnableNullable { get; set; } = true;
    public bool ImplicitUsings { get; set; } = true;
    public string Author { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Copyright { get; set; } = string.Empty;
}

/// <summary>
/// 代码生成选项 DTO
/// </summary>
public class CodeGenerationOptionsDto
{
    // 命名风格
    public string NamingStyle { get; set; } = "pascalCase";
    public string ClassPrefix { get; set; } = "";
    public string InterfacePrefix { get; set; } = "I";

    // 文件和代码风格
    public bool UseFileScoped { get; set; } = true;
    public bool UseRecordTypes { get; set; } = true;
    public bool UseExpressionBodies { get; set; } = true;
    public bool UsePatternMatching { get; set; } = true;

    // 异步和性能
    public bool GenerateAsyncMethods { get; set; } = true;
    public bool UseConfigureAwait { get; set; } = false;
    public bool UseValueTask { get; set; } = false;

    // 日志
    public bool GenerateLogging { get; set; } = true;
    public string LoggingFramework { get; set; } = "microsoft";

    // 异常处理
    public bool GenerateExceptionHandling { get; set; } = true;
    public bool UseCustomExceptions { get; set; } = false;

    // 文档
    public bool GenerateXmlComments { get; set; } = true;
    public bool IncludeExamples { get; set; } = false;

    // 优化
    public bool EnableAotOptimizations { get; set; } = false;
    public bool EnableTrimming { get; set; } = false;
    public bool GenerateSourceGenerators { get; set; } = false;
}

/// <summary>
/// 包依赖 DTO
/// </summary>
public class PackageDependencyDto
{
    public string PackageId { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Size { get; set; }
    public bool IsRequired { get; set; }
}

/// <summary>
/// 生成结果 DTO
/// </summary>
public class GenerationResultDto
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public List<GeneratedFileDto> Files { get; set; } = new();
    public int TotalLines { get; set; }
    public TimeSpan Duration { get; set; }
    public string ProjectPath { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// 生成的文件 DTO
/// </summary>
public class GeneratedFileDto
{
    public string Path { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int LineCount => Content.Count(c => c == '\n') + 1;
    public long Size => System.Text.Encoding.UTF8.GetByteCount(Content);
}

/// <summary>
/// 生成进度 DTO
/// </summary>
public class GenerationProgressDto
{
    public int Percentage { get; set; }
    public string Message { get; set; } = string.Empty;
    public string CurrentFile { get; set; } = string.Empty;
}

/// <summary>
/// 代码模板 DTO
/// </summary>
public class CodeTemplateDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public List<string> Features { get; set; } = new();
    public string IconClass { get; set; } = string.Empty;
    public bool IsCustom { get; set; }
}

/// <summary>
/// 创建模板请求 DTO
/// </summary>
public class CreateTemplateRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TemplateContent { get; set; } = string.Empty;
}

/// <summary>
/// 生成历史 DTO
/// </summary>
public class GenerationHistoryDto
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ProjectId { get; set; } = string.Empty;
    public string Template { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool Success { get; set; }
    public int FilesGenerated { get; set; }
    public int LinesGenerated { get; set; }
}

/// <summary>
/// 验证结果 DTO
/// </summary>
public class ValidationResultDto
{
    public bool IsValid { get; set; }
    public List<ValidationErrorDto> Errors { get; set; } = new();
    public List<ValidationWarningDto> Warnings { get; set; } = new();
}