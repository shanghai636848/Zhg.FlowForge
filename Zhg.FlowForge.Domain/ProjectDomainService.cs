using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Zhg.FlowForge.Domain;

/// <summary>
/// 项目领域服务实现
/// </summary>
public class ProjectDomainService : IProjectDomainService
{
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<ProjectDomainService> _logger;

    public ProjectDomainService(
        IProjectRepository projectRepository,
        ILogger<ProjectDomainService> logger)
    {
        _projectRepository = projectRepository;
        _logger = logger;
    }

    public async Task<bool> CanDeleteProjectAsync(string projectId, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);
        if (project == null)
        {
            return false;
        }

        // 业务规则：已部署的项目不能删除
        if (project.Status == Shared.ProjectStatus.Deployed)
        {
            _logger.LogWarning("无法删除已部署的项目: {ProjectId}", projectId);
            return false;
        }

        return true;
    }

    public async Task<string> GenerateProjectNamespaceAsync(
        string projectName,
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        // 移除特殊字符，替换空格为点号
        var sanitized = new string(projectName
            .Where(c => char.IsLetterOrDigit(c) || c == ' ' || c == '.')
            .ToArray());

        sanitized = sanitized.Replace(' ', '.');

        // 确保每个部分以大写字母开头
        var parts = sanitized.Split('.');
        var capitalizedParts = parts.Select(p =>
        {
            if (string.IsNullOrEmpty(p)) return p;
            return char.ToUpper(p[0]) + p.Substring(1);
        });

        var namespaceStr = string.Join(".", capitalizedParts.Where(p => !string.IsNullOrEmpty(p)));

        // 确保命名空间有效
        if (string.IsNullOrEmpty(namespaceStr) || !char.IsLetter(namespaceStr[0]))
        {
            namespaceStr = "FlowForge." + namespaceStr;
        }

        return namespaceStr;
    }
}