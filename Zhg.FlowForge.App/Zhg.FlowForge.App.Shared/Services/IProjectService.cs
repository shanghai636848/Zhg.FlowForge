using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zhg.FlowForge.App.Shared.Services;

public interface IProjectService
{
    // 项目管理
    Task<List<Project>> GetProjectsAsync();
    Task<Project?> GetProjectAsync(string projectId);
    Task<Project> CreateProjectAsync(CreateProjectRequest request);
    Task<Project> UpdateProjectAsync(string projectId, UpdateProjectRequest request);
    Task DeleteProjectAsync(string projectId);

    // 文件管理
    Task<List<ProjectFile>> GetProjectFilesAsync(string projectId);
    Task<string> GetFileContentAsync(string projectId, string filePath);
    Task SaveFileAsync(string projectId, string filePath, string content);
    Task<ProjectFile> CreateFileAsync(string projectId, string filePath, string content = "");
    Task DeleteFileAsync(string projectId, string filePath);
    Task RenameFileAsync(string projectId, string oldPath, string newPath);

    // 统计信息
    Task<ProjectStatistics> GetProjectStatisticsAsync(string projectId);

    // 导入导出
    Task<Project> ImportProjectAsync(string zipFilePath);
    Task<string> ExportProjectAsync(string projectId);

    // 搜索
    Task<List<Project>> SearchProjectsAsync(string query);
}

public class ProjectFile
{
    public string Path { get; set; } = "";
    public string Name { get; set; } = "";
    public bool IsFolder { get; set; }
    public bool IsDirty { get; set; }
    public int LineCount { get; set; }
    public long Size { get; set; }
    public DateTime LastModified { get; set; }
    public List<ProjectFile> SubFiles { get; set; } = new();

    // 便捷属性
    public string Extension => IsFolder ? "" : System.IO.Path.GetExtension(Path);
    public string SizeFormatted => FormatFileSize(Size);

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}

// 请求模型
public class CreateProjectRequest
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Namespace { get; set; } = "";
    public string TargetFramework { get; set; } = "net10.0";
    public string Template { get; set; } = "standard";
}

public class UpdateProjectRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public ProjectStatus? Status { get; set; }
}

// 统计模型
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


public class ProjectStats
{
    public int FileCount { get; set; }
    public int TotalLines { get; set; }
    public long TotalSize { get; set; }
    public DateTime LastModified { get; set; }
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
/// 项目状态
/// </summary>
public enum ProjectStatus
{
    Developing,
    Completed,
    Deployed,
    Archived
}

