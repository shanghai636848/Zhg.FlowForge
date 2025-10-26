using System;
using System.Collections.Generic;
using System.Text;

namespace Zhg.FlowForge.Application.Contract;

/// <summary>
/// 文件系统服务接口
/// </summary>
public interface IFileSystemService
{
    Task SaveProjectToLocalAsync(string projectId, string localPath, CancellationToken cancellationToken = default);
    Task<ProjectDto> LoadProjectFromLocalAsync(string localPath, CancellationToken cancellationToken = default);
    Task DeleteLocalProjectAsync(string localPath, CancellationToken cancellationToken = default);
}