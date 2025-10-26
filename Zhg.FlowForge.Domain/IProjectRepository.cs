using System;
using System.Collections.Generic;
using System.Text;

namespace Zhg.FlowForge.Domain;

/// <summary>
/// 项目仓储接口
/// </summary>
public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<List<Project>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Project> AddAsync(Project project, CancellationToken cancellationToken = default);
    Task UpdateAsync(Project project, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task<List<Project>> SearchAsync(string query, CancellationToken cancellationToken = default);
}