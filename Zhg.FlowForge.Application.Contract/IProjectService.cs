using System;
using System.Collections.Generic;
using System.Text;
using Zhg.FlowForge.Domain.Shared;

namespace Zhg.FlowForge.Application.Contract;

/// <summary>
/// 项目应用服务接口
/// </summary>
public interface IProjectService
{
    Task<List<ProjectDto>> GetListAsync(CancellationToken cancellationToken = default);
    Task<ProjectDto?> GetAsync(string id, CancellationToken cancellationToken = default);
    Task<ProjectDto> CreateAsync(CreateProjectDto input, CancellationToken cancellationToken = default);
    Task<ProjectDto> UpdateAsync(string id, UpdateProjectDto input, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task<List<ProjectDto>> SearchAsync(string query, CancellationToken cancellationToken = default);
    Task<ProjectStatisticsDto> GetStatisticsAsync(string id, CancellationToken cancellationToken = default);
    Task<bool> SaveToLocalAsync(string id, string? customPath = null, CancellationToken cancellationToken = default);
    Task<ProjectDto> LoadFromLocalAsync(string localPath, CancellationToken cancellationToken = default);
}


/// <summary>
/// 项目 DTO
/// </summary>
public class ProjectDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string TargetFramework { get; set; } = string.Empty;
    public string Template { get; set; } = string.Empty;
    public ProjectStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? LocalPath { get; set; }
    public bool IsSavedToLocal { get; set; }
}

/// <summary>
/// 创建项目请求 DTO
/// </summary>
public class CreateProjectDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string TargetFramework { get; set; } = "net10.0";
    public string Template { get; set; } = "standard";
    public bool SaveToLocal { get; set; } = true;
    public string? LocalPath { get; set; }
}

/// <summary>
/// 更新项目请求 DTO
/// </summary>
public class UpdateProjectDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public ProjectStatus? Status { get; set; }
}

/// <summary>
/// 项目文件 DTO
/// </summary>
public class ProjectFileDto
{
    public string Path { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsFolder { get; set; }
    public int LineCount { get; set; }
    public long Size { get; set; }
    public DateTime LastModified { get; set; }
    public List<ProjectFileDto> SubFiles { get; set; } = new();
}

/// <summary>
/// 项目统计 DTO
/// </summary>
public class ProjectStatisticsDto
{
    public int TotalFiles { get; set; }
    public int TotalLines { get; set; }
    public long TotalSize { get; set; }
    public int ClassCount { get; set; }
    public int InterfaceCount { get; set; }
    public int MethodCount { get; set; }
    public Dictionary<string, int> FileTypeDistribution { get; set; } = new();
}