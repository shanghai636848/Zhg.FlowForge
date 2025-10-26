using System.Text.Json.Serialization;
using Zhg.FlowForge.Application.Contract;

namespace Zhg.FlowForge.Api;

/// <summary>
/// JSON 序列化上下文（用于 AOT 编译）
/// </summary>
[JsonSerializable(typeof(ProjectDto))]
[JsonSerializable(typeof(List<ProjectDto>))]
[JsonSerializable(typeof(CreateProjectDto))]
[JsonSerializable(typeof(UpdateProjectDto))]
[JsonSerializable(typeof(ProjectStatisticsDto))]
[JsonSerializable(typeof(ProjectFileDto))]
[JsonSerializable(typeof(List<ProjectFileDto>))]

[JsonSerializable(typeof(BpmnProcessDto))]
[JsonSerializable(typeof(List<BpmnProcessDto>))]
[JsonSerializable(typeof(CreateBpmnProcessDto))]
[JsonSerializable(typeof(UpdateBpmnProcessDto))]
[JsonSerializable(typeof(BpmnValidationResultDto))]
[JsonSerializable(typeof(BpmnActivityDto))]
[JsonSerializable(typeof(BpmnSequenceFlowDto))]
[JsonSerializable(typeof(BpmnGatewayDto))]

[JsonSerializable(typeof(GenerationRequestDto))]
[JsonSerializable(typeof(GenerationResultDto))]
[JsonSerializable(typeof(List<GeneratedFileDto>))]
[JsonSerializable(typeof(GeneratedFileDto))]
[JsonSerializable(typeof(GenerationProgressDto))]
[JsonSerializable(typeof(ProjectConfigDto))]
[JsonSerializable(typeof(CodeGenerationOptionsDto))]
[JsonSerializable(typeof(CodeTemplateDto))]
[JsonSerializable(typeof(List<CodeTemplateDto>))]
[JsonSerializable(typeof(PackageDependencyDto))]

[JsonSerializable(typeof(CompilationOptionsDto))]
[JsonSerializable(typeof(CompilationResultDto))]
[JsonSerializable(typeof(DiagnosticDto))]
[JsonSerializable(typeof(List<DiagnosticDto>))]
[JsonSerializable(typeof(AnalyzeCodeRequestDto))]

[JsonSerializable(typeof(ValidationResultDto))]
[JsonSerializable(typeof(ValidationErrorDto))]
[JsonSerializable(typeof(ValidationWarningDto))]

[JsonSerializable(typeof(ApiResponse<ProjectDto>))]
[JsonSerializable(typeof(ApiResponse<List<ProjectDto>>))]
[JsonSerializable(typeof(ApiResponse<BpmnProcessDto>))]
[JsonSerializable(typeof(ApiResponse<List<BpmnProcessDto>>))]
[JsonSerializable(typeof(ApiResponse<GenerationResultDto>))]
[JsonSerializable(typeof(ApiResponse<CompilationResultDto>))]
[JsonSerializable(typeof(ApiResponse<List<CodeTemplateDto>>))]
[JsonSerializable(typeof(ApiResponse<List<GeneratedFileDto>>))]
[JsonSerializable(typeof(ApiResponse<List<DiagnosticDto>>))]
[JsonSerializable(typeof(ApiResponse<ValidationResultDto>))]
[JsonSerializable(typeof(ApiResponse<BpmnValidationResultDto>))]
[JsonSerializable(typeof(ApiResponse<string>))]
[JsonSerializable(typeof(ApiResponse<ProjectStatisticsDto>))]

[JsonSerializable(typeof(ApiErrorResponse))]

[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(Dictionary<string, int>))]
public partial class AppJsonSerializerContext : JsonSerializerContext;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> Ok(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static ApiResponse<T> Fail(string message)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message
        };
    }
}

/// <summary>
/// API 错误响应
/// </summary>
public class ApiErrorResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Detail { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}