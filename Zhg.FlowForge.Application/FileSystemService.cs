using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Zhg.FlowForge.Application.Contract;
using Zhg.FlowForge.Domain;
using Zhg.FlowForge.Domain.Shared;

namespace Zhg.FlowForge.Application;

/// <summary>
/// 文件系统服务实现
/// </summary>
public class FileSystemService : IFileSystemService
{
    private readonly ILogger<FileSystemService> _logger;
    private readonly string _localRootPath;

    public FileSystemService(ILogger<FileSystemService> logger)
    {
        _logger = logger;
        _localRootPath = Path.Combine("C:", "FlowForge", "Projects");
        EnsureLocalRootExists();
    }

    public async Task SaveProjectToLocalAsync(
        string projectId,
        string localPath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 确保项目目录存在
            if (!Directory.Exists(localPath))
            {
                Directory.CreateDirectory(localPath);
            }

            // TODO: 从项目仓储获取文件内容并保存
            _logger.LogInformation("项目 {ProjectId} 已保存到本地: {Path}", projectId, localPath);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存项目到本地失败: {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<ProjectDto> LoadProjectFromLocalAsync(
        string localPath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Directory.Exists(localPath))
            {
                throw new DirectoryNotFoundException($"目录不存在: {localPath}");
            }

            var projectName = Path.GetFileName(localPath);
            var project = Project.Create(
                projectName,
                $"从本地加载: {localPath}",
                projectName.Replace(" ", "").Replace("-", ""),
                FlowForgeConstants.DefaultTargetFramework,
                FlowForgeConstants.Templates.Standard,
                "System"
            );

            project.SaveToLocal(localPath);

            // 加载所有文件
            var files = new Dictionary<string, string>();
            await LoadDirectoryRecursiveAsync(localPath, localPath, files, cancellationToken);

            _logger.LogInformation(
                "从本地加载项目: {ProjectName} ({FileCount} 个文件)",
                projectName,
                files.Count);

            //return project;
            return new ProjectDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "从本地加载项目失败: {Path}", localPath);
            throw;
        }
    }

    public async Task DeleteLocalProjectAsync(
        string localPath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (Directory.Exists(localPath))
            {
                Directory.Delete(localPath, true);
                _logger.LogInformation("删除本地项目: {Path}", localPath);
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除本地项目失败: {Path}", localPath);
            throw;
        }
    }

    #region Private Methods

    private void EnsureLocalRootExists()
    {
        try
        {
            if (!Directory.Exists(_localRootPath))
            {
                Directory.CreateDirectory(_localRootPath);
                _logger.LogInformation("创建本地项目根目录: {Path}", _localRootPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建本地项目根目录失败");
        }
    }

    private async Task LoadDirectoryRecursiveAsync(
        string rootPath,
        string currentPath,
        Dictionary<string, string> files,
        CancellationToken cancellationToken)
    {
        try
        {
            // 加载文件
            foreach (var filePath in Directory.GetFiles(currentPath))
            {
                try
                {
                    var relativePath = Path.GetRelativePath(rootPath, filePath)
                        .Replace(Path.DirectorySeparatorChar, '/');
                    var content = await File.ReadAllTextAsync(filePath, Encoding.UTF8, cancellationToken);
                    files[relativePath] = content;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "读取文件失败: {FilePath}", filePath);
                }
            }
            // 递归加载子目录
            foreach (var dirPath in Directory.GetDirectories(currentPath))
            {
                await LoadDirectoryRecursiveAsync(rootPath, dirPath, files, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载目录失败: {Path}", currentPath);
        }
    }

    #endregion
}