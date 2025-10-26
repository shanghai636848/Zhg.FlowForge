using Microsoft.AspNetCore.Mvc;
using Zhg.FlowForge.Application.Contract;

namespace Zhg.FlowForge.Api;

/// <summary>
/// 代码生成相关端点
/// </summary>
public static class CodeGenerationEndpoints
{
    public static void MapCodeGenerationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/codegen");

        // 生成代码
        group.MapPost("/generate", async (
            [FromBody] GenerationRequestDto request,
            [FromServices] ICodeGenerationService codeGenService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var progress = new Progress<GenerationProgressDto>();
                var result = await codeGenService.GenerateAsync(request, progress, cancellationToken);

                return Results.Ok(ApiResponse<GenerationResultDto>.Ok(
                    result,
                    result.Success ? "Code generated successfully" : "Code generation failed"));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new ApiErrorResponse
                {
                    Success = false,
                    Message = "Failed to generate code",
                    Detail = ex.Message
                });
            }
        })
        .WithName("GenerateCode")
        .Produces<ApiResponse<GenerationResultDto>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest);

        // 预览生成代码
        group.MapPost("/preview", async (
            [FromBody] GenerationRequestDto request,
            [FromServices] ICodeGenerationService codeGenService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var files = await codeGenService.PreviewAsync(request, cancellationToken);
                return Results.Ok(ApiResponse<List<GeneratedFileDto>>.Ok(files));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new ApiErrorResponse
                {
                    Success = false,
                    Message = "Failed to preview code",
                    Detail = ex.Message
                });
            }
        })
        .WithName("PreviewCode")
        .Produces<ApiResponse<List<GeneratedFileDto>>>(StatusCodes.Status200OK);

        // 验证配置
        group.MapPost("/validate", async (
            [FromBody] GenerationRequestDto request,
            [FromServices] ICodeGenerationService codeGenService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await codeGenService.ValidateConfigurationAsync(request, cancellationToken);
                return Results.Ok(ApiResponse<ValidationResultDto>.Ok(result));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new ApiErrorResponse
                {
                    Success = false,
                    Message = "Failed to validate configuration",
                    Detail = ex.Message
                });
            }
        })
        .WithName("ValidateConfiguration")
        .Produces<ApiResponse<ValidationResultDto>>(StatusCodes.Status200OK);

        // 获取模板列表
        group.MapGet("/templates", async (
            [FromServices] ICodeGenerationService codeGenService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var templates = await codeGenService.GetTemplatesAsync(cancellationToken);
                return Results.Ok(ApiResponse<List<CodeTemplateDto>>.Ok(templates));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new ApiErrorResponse
                {
                    Success = false,
                    Message = "Failed to get templates",
                    Detail = ex.Message
                });
            }
        })
        .WithName("GetCodeTemplates")
        .Produces<ApiResponse<List<CodeTemplateDto>>>(StatusCodes.Status200OK);
    }
}
