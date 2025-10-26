using System.Collections.Concurrent;
using Zhg.FlowForge.Domain;

namespace Zhg.FlowForge.Api;

/// <summary>
/// 内存项目仓储实现（演示用）
/// </summary>
public class InMemoryProjectRepository : IProjectRepository
{
    private readonly ConcurrentDictionary<string, Project> _projects = new();

    public Task<Project?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _projects.TryGetValue(id, out var project);
        return Task.FromResult(project);
    }

    public Task<List<Project>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_projects.Values.OrderByDescending(p => p.UpdatedAt).ToList());
    }

    public Task<Project> AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        _projects[project.Id] = project;
        return Task.FromResult(project);
    }

    public Task UpdateAsync(Project project, CancellationToken cancellationToken = default)
    {
        _projects[project.Id] = project;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        _projects.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public Task<List<Project>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        var results = _projects.Values
            .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                       p.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(p => p.UpdatedAt)
            .ToList();

        return Task.FromResult(results);
    }
}