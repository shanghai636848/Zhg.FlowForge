using System;
using System.Collections.Generic;
using System.Text;

namespace Zhg.FlowForge.Domain;

/// <summary>
/// 项目领域服务接口
/// </summary>
public interface IProjectDomainService
{
    Task<bool> CanDeleteProjectAsync(string projectId, CancellationToken cancellationToken = default);
    Task<string> GenerateProjectNamespaceAsync(string projectName, CancellationToken cancellationToken = default);
}