using Microsoft.AspNetCore.Mvc;
using Zhg.FlowForge.Application.Contract;

namespace Zhg.FlowForge.Api;

/// <summary>
/// 编译相关端点
/// </summary>
public static class CompilationEndpoints
{
    public static void MapCompilationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/compilation");

        // 编译项目
        group.MapPost("/{projectId}/compile", async (
            string projectId,
            [FromBody] CompilationOptionsDto options,
            [FromServices] ICompilationService compilationService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var progress = new Progress<string>();
                var result = await compilationService.CompileAsync(projectId, options, progress, cancellationToken);

                return Results.Ok(ApiResponse<CompilationResultDto>.Ok(
                    result,
                    result.Success ? "Compilation successful" : "Compilation failed"));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new ApiErrorResponse
                {
                    Success = false,
                    Message = "Failed to compile project",
                    Detail = ex.Message
                });
            }
        })
        .WithName("CompileProject")
        .Produces<ApiResponse<CompilationResultDto>>(StatusCodes.Status200OK);

        // 清理项目
        group.MapPost("/{projectId}/clean", async (
            string projectId,
            [FromServices] ICompilationService compilationService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                await compilationService.CleanAsync(projectId, cancellationToken);
                return Results.Ok(ApiResponse<string>.Ok("Project cleaned successfully"));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new ApiErrorResponse
                {
                    Success = false,
                    Message = "Failed to clean project",
                    Detail = ex.Message
                });
            }
        })
        .WithName("CleanProject")
        .Produces<ApiResponse<string>>(StatusCodes.Status200OK);

        // 重新构建项目
        group.MapPost("/{projectId}/rebuild", async (
            string projectId,
            [FromBody] CompilationOptionsDto options,
            [FromServices] ICompilationService compilationService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var progress = new Progress<string>();
                var result = await compilationService.RebuildAsync(projectId, options, progress, cancellationToken);

                return Results.Ok(ApiResponse<CompilationResultDto>.Ok(
                    result,
                    result.Success ? "Rebuild successful" : "Rebuild failed"));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new ApiErrorResponse
                {
                    Success = false,
                    Message = "Failed to rebuild project",
                    Detail = ex.Message
                });
            }
        })
        .WithName("RebuildProject")
        .Produces<ApiResponse<CompilationResultDto>>(StatusCodes.Status200OK);

        // 分析代码
        group.MapPost("/analyze", async (
            [FromBody] AnalyzeCodeRequestDto request,
            [FromServices] ICompilationService compilationService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var diagnostics = await compilationService.AnalyzeCodeAsync(
                    request.Code,
                    request.Language,
                    cancellationToken);

                return Results.Ok(ApiResponse<List<DiagnosticDto>>.Ok(diagnostics));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new ApiErrorResponse
                {
                    Success = false,
                    Message = "Failed to analyze code",
                    Detail = ex.Message
                });
            }
        })
        .WithName("AnalyzeCode")
        .Produces<ApiResponse<List<DiagnosticDto>>>(StatusCodes.Status200OK);
    }
}

/// <summary>
/// 分析代码请求 DTO
/// </summary>
public class AnalyzeCodeRequestDto
{
    public string Code { get; set; } = string.Empty;
    public string Language { get; set; } = "csharp";
}