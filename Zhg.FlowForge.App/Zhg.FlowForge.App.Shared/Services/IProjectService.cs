using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zhg.FlowForge.App.Shared.Services;

// IProjectService.cs
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

    // 本地文件系统操作
    Task<string> GetProjectRootPathAsync(string projectId);
    Task<bool> SaveProjectToLocalAsync(string projectId, string localPath);
    Task<Project> LoadProjectFromLocalAsync(string localPath);
    Task<bool> ExistsInLocalAsync(string projectId);
}

// 扩展模型
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

    // 新增：本地文件路径
    public string? LocalPath { get; set; }

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

public class CreateProjectRequest
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Namespace { get; set; } = "";
    public string TargetFramework { get; set; } = "net10.0";
    public string Template { get; set; } = "standard";
    public bool SaveToLocal { get; set; } = true; // 新增：是否保存到本地
    public string? LocalPath { get; set; } // 新增：本地路径
}

public class UpdateProjectRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public ProjectStatus? Status { get; set; }
}

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

    // 新增：本地存储信息
    public string? LocalPath { get; set; }
    public bool IsSavedToLocal { get; set; }
}

public enum ProjectStatus
{
    Developing,
    Completed,
    Deployed,
    Archived
}


public class ProjectStats
{
    public int FileCount { get; set; }
    public int TotalLines { get; set; }
    public long TotalSize { get; set; }
    public DateTime LastModified { get; set; }
}

