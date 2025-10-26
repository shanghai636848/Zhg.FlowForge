using Microsoft.AspNetCore.Mvc;
using Zhg.FlowForge.Application.Contract;

namespace Zhg.FlowForge.Api;

/// <summary>
/// 项目相关端点
/// </summary>
public static class ProjectEndpoints
{
    public static void MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects");

        // 获取项目列表
        group.MapGet("/", async (
            [FromServices] IProjectService projectService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var projects = await projectService.GetListAsync(cancellationToken);
                return Results.Ok(ApiResponse<List<ProjectDto>>.Ok(projects));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new ApiErrorResponse
                {
                    Success = false,
                    Message = "Failed to get projects",
                    Detail = ex.Message
                });
            }
        })
        .WithName("GetProjects")
        .Produces<ApiResponse<List<ProjectDto>>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest);

        // 获取单个项目
        group.MapGet("/{id}", async (
            string id,
            [FromServices] IProjectService projectService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var project = await projectService.GetAsync(id, cancellationToken);
                if (project == null)
                {
                    return Results.NotFound(new ApiErrorResponse
                    {
                        Success = false,
                        Message = $"Project with id {id} not found"
                    });
                }

                return Results.Ok(ApiResponse<ProjectDto>.Ok(project));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new ApiErrorResponse
                {
                    Success = false,
                    Message = "Failed to get project",
                    Detail = ex.Message
                });
            }
        })
        .WithName("GetProject")
        .Produces<ApiResponse<ProjectDto>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        // 创建项目
        group.MapPost("/", async (
            [FromBody] CreateProjectDto input,
            [FromServices] IProjectService projectService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var project = await projectService.CreateAsync(input, cancellationToken);
                return Results.Created($"/api/projects/{project.Id}",
                    ApiResponse<ProjectDto>.Ok(project, "Project created successfully"));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new ApiErrorResponse
                {
                    Success = false,
                    Message = "Failed to create project",
                    Detail = ex.Message
                });
            }
        })
        .WithName("CreateProject")
        .Produces<ApiResponse<ProjectDto>>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest);

        // 更新项目
        group.MapPut("/{id}", async (
            string id,
            [FromBody] UpdateProjectDto input,
            [FromServices] IProjectService projectService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var project = await projectService.UpdateAsync(id, input, cancellationToken);
                return Results.Ok(ApiResponse<ProjectDto>.Ok(project, "Project updated successfully"));
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound(new ApiErrorResponse
                {
                    Success = false,
                    Message = $"Project with id {id} not found"
                });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new ApiErrorResponse
                {
                    Success = false,
                    Message = "Failed to update project",
                    Detail = ex.Message
                });
            }
        })
        .WithName("UpdateProject")
        .Produces<ApiResponse<ProjectDto>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        // 删除项目
        group.MapDelete("/{id}", async (
            string id,
            [FromServices] IProjectService projectService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                await projectService.DeleteAsync(id, cancellationToken);
                return Results.Ok(ApiResponse<string>.Ok("Project deleted successfully"));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new ApiErrorResponse
                {
                    Success = false,
                    Message = "Failed to delete project",
                    Detail = ex.Message
                });
            }
        })
        .WithName("DeleteProject")
        .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest);

        // 搜索项目
        group.MapGet("/search/{query}", async (
            string query,
            [FromServices] IProjectService projectService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var projects = await projectService.SearchAsync(query, cancellationToken);
                return Results.Ok(ApiResponse<List<ProjectDto>>.Ok(projects));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new ApiErrorResponse
                {
                    Success = false,
                    Message = "Failed to search projects",
                    Detail = ex.Message
                });
            }
        })
        .WithName("SearchProjects")
        .Produces<ApiResponse<List<ProjectDto>>>(StatusCodes.Status200OK);

        // 获取项目统计
        group.MapGet("/{id}/statistics", async (
            string id,
            [FromServices] IProjectService projectService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var stats = await projectService.GetStatisticsAsync(id, cancellationToken);
                return Results.Ok(ApiResponse<ProjectStatisticsDto>.Ok(stats));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new ApiErrorResponse
                {
                    Success = false,
                    Message = "Failed to get project statistics",
                    Detail = ex.Message
                });
            }
        })
        .WithName("GetProjectStatistics")
        .Produces<ApiResponse<ProjectStatisticsDto>>(StatusCodes.Status200OK);

        // 保存项目到本地
        group.MapPost("/{id}/save-local", async (
            string id,
            [FromQuery] string? customPath,
            [FromServices] IProjectService projectService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var success = await projectService.SaveToLocalAsync(id, customPath, cancellationToken);
                if (success)
                {
                    return Results.Ok(ApiResponse<string>.Ok("Project saved to local successfully"));
                }

                return Results.BadRequest(new ApiErrorResponse
                {
                    Success = false,
                    Message = "Failed to save project to local"
                });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new ApiErrorResponse
                {
                    Success = false,
                    Message = "Failed to save project to local",
                    Detail = ex.Message
                });
            }
        })
        .WithName("SaveProjectToLocal")
        .Produces<ApiResponse<string>>(StatusCodes.Status200OK);

        // 从本地加载项目
        group.MapPost("/load-local", async (
            [FromQuery] string localPath,
            [FromServices] IProjectService projectService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var project = await projectService.LoadFromLocalAsync(localPath, cancellationToken);
                return Results.Ok(ApiResponse<ProjectDto>.Ok(project, "Project loaded from local successfully"));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new ApiErrorResponse
                {
                    Success = false,
                    Message = "Failed to load project from local",
                    Detail = ex.Message
                });
            }
        })
        .WithName("LoadProjectFromLocal")
        .Produces<ApiResponse<ProjectDto>>(StatusCodes.Status200OK);
    }
}