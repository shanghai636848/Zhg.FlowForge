using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Zhg.FlowForge.Application.Contract;
using Zhg.FlowForge.Domain;
using static Zhg.FlowForge.Domain.Shared.FlowForgeConstants;

namespace Zhg.FlowForge.Application;

/// <summary>
/// 项目应用服务实现
/// </summary>
public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<ProjectService> _logger;
    private readonly IFileSystemService _fileSystemService;

    public ProjectService(
        IProjectRepository projectRepository,
        ILogger<ProjectService> logger,
        IFileSystemService fileSystemService)
    {
        _projectRepository = projectRepository;
        _logger = logger;
        _fileSystemService = fileSystemService;
    }

    public async Task<List<ProjectDto>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var projects = await _projectRepository.GetAllAsync(cancellationToken);
        return projects.Select(MapToDto).ToList();
    }

    public async Task<ProjectDto?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, cancellationToken);
        return project != null ? MapToDto(project) : null;
    }

    public async Task<ProjectDto> CreateAsync(CreateProjectDto input, CancellationToken cancellationToken = default)
    {
        var project = Project.Create(
            input.Name,
            input.Description,
            input.Namespace,
            input.TargetFramework,
            input.Template,
            "System" // TODO: Get from current user
        );

        await _projectRepository.AddAsync(project, cancellationToken);

        // 如果需要保存到本地
        if (input.SaveToLocal)
        {
            var localPath = input.LocalPath ?? Path.Combine("C:\\FlowForge\\Projects", project.Name);
            await _fileSystemService.SaveProjectToLocalAsync(project.Id, localPath, cancellationToken);
            project.SaveToLocal(localPath);
            await _projectRepository.UpdateAsync(project, cancellationToken);
        }

        _logger.LogInformation("Created project: {ProjectName} ({ProjectId})", project.Name, project.Id);

        return MapToDto(project);
    }

    public async Task<ProjectDto> UpdateAsync(string id, UpdateProjectDto input, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, cancellationToken);
        if (project == null)
        {
            throw new KeyNotFoundException($"Project with id {id} not found");
        }

        project.Update(input.Name, input.Description, input.Status);
        await _projectRepository.UpdateAsync(project, cancellationToken);

        _logger.LogInformation("Updated project: {ProjectId}", id);

        return MapToDto(project);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, cancellationToken);
        if (project != null && project.IsSavedToLocal && !string.IsNullOrEmpty(project.LocalPath))
        {
            await _fileSystemService.DeleteLocalProjectAsync(project.LocalPath, cancellationToken);
        }

        await _projectRepository.DeleteAsync(id, cancellationToken);

        _logger.LogInformation("Deleted project: {ProjectId}", id);
    }

    public async Task<List<ProjectDto>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        var projects = await _projectRepository.SearchAsync(query, cancellationToken);
        return projects.Select(MapToDto).ToList();
    }

    public async Task<ProjectStatisticsDto> GetStatisticsAsync(string id, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, cancellationToken);
        if (project == null)
        {
            throw new KeyNotFoundException($"Project with id {id} not found");
        }

        // TODO: Implement statistics calculation
        return new ProjectStatisticsDto();
    }

    public async Task<bool> SaveToLocalAsync(string id, string? customPath = null, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, cancellationToken);
        if (project == null)
        {
            return false;
        }

        var localPath = customPath ?? Path.Combine("C:\\FlowForge\\Projects", project.Name);
        await _fileSystemService.SaveProjectToLocalAsync(id, localPath, cancellationToken);

        project.SaveToLocal(localPath);
        await _projectRepository.UpdateAsync(project, cancellationToken);

        return true;
    }

    public async Task<ProjectDto> LoadFromLocalAsync(string localPath, CancellationToken cancellationToken = default)
    {
        var project = await _fileSystemService.LoadProjectFromLocalAsync(localPath, cancellationToken);
        await _projectRepository.AddAsync(Project.Create(
        project.Name,
        project.Description,
        project.Namespace,
        project.TargetFramework,
        project.Template,
        project.CreatedBy
            ), cancellationToken);

        return project;
    }

    private static ProjectDto MapToDto(Project project)
    {
        return new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Namespace = project.Namespace,
            TargetFramework = project.TargetFramework,
            Template = project.Template,
            Status = project.Status,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
            CreatedBy = project.CreatedBy,
            LocalPath = project.LocalPath,
            IsSavedToLocal = project.IsSavedToLocal
        };
    }
}