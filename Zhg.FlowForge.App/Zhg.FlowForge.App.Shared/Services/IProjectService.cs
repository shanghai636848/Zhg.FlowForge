using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zhg.FlowForge.App.Shared.Services;

/// <summary>
/// 项目管理服务接口
/// </summary>
public interface IProjectService
{
    /// <summary>
    /// 获取所有项目
    /// </summary>
    Task<List<Project>> GetProjectsAsync();

    /// <summary>
    /// 获取项目详情
    /// </summary>
    Task<Project?> GetProjectAsync(string projectId);

    /// <summary>
    /// 创建新项目
    /// </summary>
    Task<Project> CreateProjectAsync(CreateProjectRequest request);

    /// <summary>
    /// 更新项目
    /// </summary>
    Task<Project> UpdateProjectAsync(string projectId, UpdateProjectRequest request);

    /// <summary>
    /// 删除项目
    /// </summary>
    Task DeleteProjectAsync(string projectId);

    /// <summary>
    /// 获取项目文件列表
    /// </summary>
    Task<List<ProjectFile>> GetProjectFilesAsync(string projectId);

    /// <summary>
    /// 获取文件内容
    /// </summary>
    Task<string> GetFileContentAsync(string projectId, string filePath);

    /// <summary>
    /// 保存文件
    /// </summary>
    Task SaveFileAsync(string projectId, string filePath, string content);

    /// <summary>
    /// 创建文件
    /// </summary>
    Task<ProjectFile> CreateFileAsync(string projectId, string filePath, string content = "");

    /// <summary>
    /// 删除文件
    /// </summary>
    Task DeleteFileAsync(string projectId, string filePath);

    /// <summary>
    /// 重命名文件
    /// </summary>
    Task RenameFileAsync(string projectId, string oldPath, string newPath);

    /// <summary>
    /// 导入项目
    /// </summary>
    Task<Project> ImportProjectAsync(string zipFilePath);

    /// <summary>
    /// 导出项目
    /// </summary>
    Task<string> ExportProjectAsync(string projectId);

    /// <summary>
    /// 获取项目统计信息
    /// </summary>
    Task<ProjectStatistics> GetProjectStatisticsAsync(string projectId);

    /// <summary>
    /// 搜索项目
    /// </summary>
    Task<List<Project>> SearchProjectsAsync(string query);
}

/// <summary>
/// 项目实体
/// </summary>
public class Project
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Namespace { get; set; } = "";
    public string TargetFramework { get; set; } = "net10.0";
    public string Template { get; set; } = "standard";
    public ProjectStatus Status { get; set; } = ProjectStatus.Developing;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public string CreatedBy { get; set; } = "";
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// 项目文件
/// </summary>
public class ProjectFile
{
    public string Path { get; set; } = "";
    public string Name => System.IO.Path.GetFileName(Path);
    public bool IsFolder { get; set; }
    public bool IsDirty { get; set; }
    public int LineCount { get; set; }
    public long Size { get; set; }
    public DateTime LastModified { get; set; } = DateTime.Now;
    public List<ProjectFile> SubFiles { get; set; }
}

/// <summary>
/// 项目状态
/// </summary>
public enum ProjectStatus
{
    Developing,
    Completed,
    Deployed,
    Archived
}

/// <summary>
/// 创建项目请求
/// </summary>
public class CreateProjectRequest
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Namespace { get; set; } = "";
    public string TargetFramework { get; set; } = "net10.0";
    public string Template { get; set; } = "standard";
}

/// <summary>
/// 更新项目请求
/// </summary>
public class UpdateProjectRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public ProjectStatus? Status { get; set; }
}

/// <summary>
/// 项目统计信息
/// </summary>
public class ProjectStatistics
{
    public int TotalFiles { get; set; }
    public int TotalLines { get; set; }
    public long TotalSize { get; set; }
    public int ClassCount { get; set; }
    public int InterfaceCount { get; set; }
    public int MethodCount { get; set; }
    public Dictionary<string, int> FileTypeDistribution { get; set; } = new();

}
