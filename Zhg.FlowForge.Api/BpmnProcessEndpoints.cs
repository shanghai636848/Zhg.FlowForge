using Microsoft.AspNetCore.Mvc;
using Zhg.FlowForge.Application.Contract;

namespace Zhg.FlowForge.Api;

/// <summary>
/// BPMN 流程相关端点
/// </summary>
public static class BpmnProcessEndpoints
{
    public static void MapBpmnProcessEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/bpmn");

        // 获取流程列表
        group.MapGet("/", async (
            [FromServices] IBpmnProcessService bpmnService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var processes = await bpmnService.GetListAsync(cancellationToken);
                return Results.Ok(ApiResponse<List<BpmnProcessDto>>.Ok(processes));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new ApiErrorResponse
                {
                    Success = false,
                    Message = "Failed to get BPMN processes",
                    Detail = ex.Message
                });
            }
        })
        .WithName("GetBpmnProcesses")
        .Produces<ApiResponse<List<BpmnProcessDto>>>(StatusCodes.Status200OK);

        // 获取单个流程
        group.MapGet("/{id}", async (
            string id,
            [FromServices] IBpmnProcessService bpmnService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var process = await bpmnService.GetAsync(id, cancellationToken);
                if (process == null)
                {
                    return Results.NotFound(new ApiErrorResponse
                    {
                        Success = false,
                        Message = $"BPMN process with id {id} not found"
                    });
                }

                return Results.Ok(ApiResponse<BpmnProcessDto>.Ok(process));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new ApiErrorResponse
                {
                    Success = false,
                    Message = "Failed to get BPMN process",
                    Detail = ex.Message
                });
            }
        })
        .WithName("GetBpmnProcess")
        .Produces<ApiResponse<BpmnProcessDto>>(StatusCodes.Status200OK);

        // 创建流程
        group.MapPost("/", async (
            [FromBody] CreateBpmnProcessDto input,
            [FromServices] IBpmnProcessService bpmnService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var process = await bpmnService.CreateAsync(input, cancellationToken);
                return Results.Created($"/api/bpmn/{process.Id}",
                    ApiResponse<BpmnProcessDto>.Ok(process, "BPMN process created successfully"));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new ApiErrorResponse
                {
                    Success = false,
                    Message = "Failed to create BPMN process",
                    Detail = ex.Message
                });
            }
        })
        .WithName("CreateBpmnProcess")
        .Produces<ApiResponse<BpmnProcessDto>>(StatusCodes.Status201Created);

        // 更新流程
        group.MapPut("/{id}", async (
            string id,
            [FromBody] UpdateBpmnProcessDto input,
            [FromServices] IBpmnProcessService bpmnService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var process = await bpmnService.UpdateAsync(id, input, cancellationToken);
                return Results.Ok(ApiResponse<BpmnProcessDto>.Ok(process, "BPMN process updated successfully"));
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound(new ApiErrorResponse
                {
                    Success = false,
                    Message = $"BPMN process with id {id} not found"
                });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new ApiErrorResponse
                {
                    Success = false,
                    Message = "Failed to update BPMN process",
                    Detail = ex.Message
                });
            }
        })
        .WithName("UpdateBpmnProcess")
        .Produces<ApiResponse<BpmnProcessDto>>(StatusCodes.Status200OK);

        // 删除流程
        group.MapDelete("/{id}", async (
            string id,
            [FromServices] IBpmnProcessService bpmnService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                await bpmnService.DeleteAsync(id, cancellationToken);
                return Results.Ok(ApiResponse<string>.Ok("BPMN process deleted successfully"));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new ApiErrorResponse
                {
                    Success = false,
                    Message = "Failed to delete BPMN process",
                    Detail = ex.Message
                });
            }
        })
        .WithName("DeleteBpmnProcess")
        .Produces<ApiResponse<string>>(StatusCodes.Status200OK);

        // 验证流程
        group.MapPost("/{id}/validate", async (
            string id,
            [FromServices] IBpmnProcessService bpmnService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await bpmnService.ValidateAsync(id, cancellationToken);
                return Results.Ok(ApiResponse<BpmnValidationResultDto>.Ok(result));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new ApiErrorResponse
                {
                    Success = false,
                    Message = "Failed to validate BPMN process",
                    Detail = ex.Message
                });
            }
        })
        .WithName("ValidateBpmnProcess")
        .Produces<ApiResponse<BpmnValidationResultDto>>(StatusCodes.Status200OK);

        // 导出为 XML
        group.MapGet("/{id}/export", async (
            string id,
            [FromServices] IBpmnProcessService bpmnService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var xml = await bpmnService.ExportToXmlAsync(id, cancellationToken);
                return Results.Ok(ApiResponse<string>.Ok(xml));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new ApiErrorResponse
                {
                    Success = false,
                    Message = "Failed to export BPMN process",
                    Detail = ex.Message
                });
            }
        })
        .WithName("ExportBpmnProcess")
        .Produces<ApiResponse<string>>(StatusCodes.Status200OK);

        // 从 XML 导入
        group.MapPost("/import", async (
            [FromBody] string xml,
            [FromServices] IBpmnProcessService bpmnService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var process = await bpmnService.ImportFromXmlAsync(xml, cancellationToken);
                return Results.Ok(ApiResponse<BpmnProcessDto>.Ok(process, "BPMN process imported successfully"));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new ApiErrorResponse
                {
                    Success = false,
                    Message = "Failed to import BPMN process",
                    Detail = ex.Message
                });
            }
        })
        .WithName("ImportBpmnProcess")
        .Produces<ApiResponse<BpmnProcessDto>>(StatusCodes.Status200OK);
    }
}