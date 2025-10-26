using System;
using System.Collections.Generic;
using System.Text;

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