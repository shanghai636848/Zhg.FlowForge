using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zhg.FlowForge.App.Shared.Services;


public class ProjectService : IProjectService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProjectService> _logger;
    private readonly Dictionary<string, Project> _projects = new();
    private readonly Dictionary<string, Dictionary<string, string>> _fileContents = new();
    private readonly Dictionary<string, List<ProjectFile>> _fileStructures = new();

    // 本地文件系统根路径
    private readonly string _localRootPath;

    public ProjectService(
        HttpClient httpClient,
        ILogger<ProjectService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        // 设置本地项目根目录
        _localRootPath = Path.Combine("C:", "FlowForge", "Projects");
        EnsureLocalRootExists();

        InitializeSampleData();
    }

    #region 本地文件系统支持

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

    public async Task<string> GetProjectRootPathAsync(string projectId)
    {
        await Task.CompletedTask;

        if (_projects.TryGetValue(projectId, out var project))
        {
            return project.LocalPath ?? Path.Combine(_localRootPath, project.Name);
        }

        return Path.Combine(_localRootPath, projectId);
    }

    public async Task<bool> SaveProjectToLocalAsync(string projectId, string? customPath = null)
    {
        try
        {
            if (!_projects.TryGetValue(projectId, out var project))
            {
                _logger.LogWarning("项目 {ProjectId} 不存在", projectId);
                return false;
            }

            var projectPath = customPath ?? Path.Combine(_localRootPath, project.Name);

            // 确保项目目录存在
            if (!Directory.Exists(projectPath))
            {
                Directory.CreateDirectory(projectPath);
            }

            // 保存所有文件
            if (_fileContents.TryGetValue(projectId, out var files))
            {
                foreach (var file in files)
                {
                    var filePath = Path.Combine(projectPath, file.Key.Replace('/', Path.DirectorySeparatorChar));
                    var fileDir = Path.GetDirectoryName(filePath);

                    // 确保文件所在目录存在
                    if (!string.IsNullOrEmpty(fileDir) && !Directory.Exists(fileDir))
                    {
                        Directory.CreateDirectory(fileDir);
                    }

                    // 写入文件内容
                    await File.WriteAllTextAsync(filePath, file.Value, Encoding.UTF8);
                }
            }

            // 更新项目信息
            project.LocalPath = projectPath;
            project.IsSavedToLocal = true;
            project.UpdatedAt = DateTime.Now;

            _logger.LogInformation("项目 {ProjectName} 已保存到本地: {Path}", project.Name, projectPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存项目到本地失败: {ProjectId}", projectId);
            return false;
        }
    }

    public async Task<Project> LoadProjectFromLocalAsync(string localPath)
    {
        try
        {
            if (!Directory.Exists(localPath))
            {
                throw new DirectoryNotFoundException($"目录不存在: {localPath}");
            }

            var projectName = Path.GetFileName(localPath);
            var projectId = Guid.NewGuid().ToString();

            var project = new Project
            {
                Id = projectId,
                Name = projectName,
                Description = $"从本地加载: {localPath}",
                Namespace = projectName.Replace(" ", "").Replace("-", ""),
                LocalPath = localPath,
                IsSavedToLocal = true,
                CreatedAt = Directory.GetCreationTime(localPath),
                UpdatedAt = Directory.GetLastWriteTime(localPath)
            };

            _projects[projectId] = project;

            // 加载所有文件
            var files = new Dictionary<string, string>();
            await LoadDirectoryRecursive(localPath, localPath, files);
            _fileContents[projectId] = files;

            _logger.LogInformation("从本地加载项目: {ProjectName} ({FileCount} 个文件)",
                projectName, files.Count);

            return project;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "从本地加载项目失败: {Path}", localPath);
            throw;
        }
    }

    private async Task LoadDirectoryRecursive(string rootPath, string currentPath, Dictionary<string, string> files)
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
                    var content = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
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
                await LoadDirectoryRecursive(rootPath, dirPath, files);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载目录失败: {Path}", currentPath);
        }
    }

    public async Task<bool> ExistsInLocalAsync(string projectId)
    {
        await Task.CompletedTask;

        if (_projects.TryGetValue(projectId, out var project) &&
            !string.IsNullOrEmpty(project.LocalPath))
        {
            return Directory.Exists(project.LocalPath);
        }

        return false;
    }

    #endregion

    #region 项目管理

    public async Task<List<Project>> GetProjectsAsync()
    {
        await Task.Delay(100);
        return _projects.Values.OrderByDescending(p => p.UpdatedAt).ToList();
    }

    public async Task<Project?> GetProjectAsync(string projectId)
    {
        await Task.Delay(50);
        return _projects.TryGetValue(projectId, out var project) ? project : null;
    }

    public async Task<Project> CreateProjectAsync(CreateProjectRequest request)
    {
        await Task.Delay(200);

        var project = new Project
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            Description = request.Description,
            Namespace = request.Namespace,
            TargetFramework = request.TargetFramework,
            Template = request.Template,
            Status = ProjectStatus.Developing,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            CreatedBy = "Current User"
        };

        _projects[project.Id] = project;
        await InitializeProjectFilesAsync(project.Id, request.Template);

        // 如果需要保存到本地
        if (request.SaveToLocal)
        {
            var localPath = request.LocalPath ?? Path.Combine(_localRootPath, project.Name);
            await SaveProjectToLocalAsync(project.Id, localPath);
        }

        _logger.LogInformation("创建项目 {ProjectName} ({ProjectId})", project.Name, project.Id);
        return project;
    }

    public async Task<Project> UpdateProjectAsync(string projectId, UpdateProjectRequest request)
    {
        await Task.Delay(100);

        if (!_projects.TryGetValue(projectId, out var project))
        {
            throw new Exception($"项目 {projectId} 不存在");
        }

        if (request.Name != null) project.Name = request.Name;
        if (request.Description != null) project.Description = request.Description;
        if (request.Status.HasValue) project.Status = request.Status.Value;
        project.UpdatedAt = DateTime.Now;

        // 同步到本地文件系统
        if (project.IsSavedToLocal)
        {
            await SaveProjectToLocalAsync(projectId);
        }

        _logger.LogInformation("更新项目 {ProjectId}", projectId);
        return project;
    }

    public async Task DeleteProjectAsync(string projectId)
    {
        await Task.Delay(100);

        if (_projects.TryGetValue(projectId, out var project) &&
            project.IsSavedToLocal &&
            !string.IsNullOrEmpty(project.LocalPath))
        {
            try
            {
                if (Directory.Exists(project.LocalPath))
                {
                    Directory.Delete(project.LocalPath, true);
                    _logger.LogInformation("删除本地项目文件: {Path}", project.LocalPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除本地项目文件失败");
            }
        }

        _projects.Remove(projectId);
        _fileContents.Remove(projectId);
        _fileStructures.Remove(projectId);

        _logger.LogInformation("删除项目 {ProjectId}", projectId);
    }

    #endregion

    #region 文件管理

    public async Task<List<ProjectFile>> GetProjectFilesAsync(string projectId)
    {
        await Task.Delay(100);

        // 如果已有文件结构缓存，直接返回
        if (_fileStructures.TryGetValue(projectId, out var cachedStructure))
        {
            return cachedStructure;
        }

        // 否则从文件内容构建文件结构
        var fileStructure = new List<ProjectFile>();

        if (_fileContents.TryGetValue(projectId, out var projectFiles))
        {
            foreach (var kvp in projectFiles.OrderBy(x => x.Key))
            {
                var file = CreateProjectFile(projectId, kvp.Key, kvp.Value);
                AddFileToStructure(fileStructure, file);
            }
        }

        _fileStructures[projectId] = fileStructure;
        return fileStructure;
    }

    public async Task<string> GetFileContentAsync(string projectId, string filePath)
    {
        await Task.Delay(50);

        // 优先从内存缓存读取
        if (_fileContents.TryGetValue(projectId, out var files))
        {
            if (files.TryGetValue(filePath, out var content))
            {
                return content;
            }
        }

        // 尝试从本地文件系统读取
        if (_projects.TryGetValue(projectId, out var project) &&
            project.IsSavedToLocal &&
            !string.IsNullOrEmpty(project.LocalPath))
        {
            try
            {
                var localFilePath = Path.Combine(project.LocalPath,
                    filePath.Replace('/', Path.DirectorySeparatorChar));

                if (File.Exists(localFilePath))
                {
                    var content = await File.ReadAllTextAsync(localFilePath, Encoding.UTF8);

                    // 更新缓存
                    if (!_fileContents.ContainsKey(projectId))
                    {
                        _fileContents[projectId] = new Dictionary<string, string>();
                    }
                    _fileContents[projectId][filePath] = content;

                    return content;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "从本地读取文件失败: {FilePath}", filePath);
            }
        }

        _logger.LogWarning("文件 {FilePath} 不存在，返回空内容", filePath);
        return string.Empty;
    }

    public async Task SaveFileAsync(string projectId, string filePath, string content)
    {
        await Task.Delay(100);

        if (!_fileContents.ContainsKey(projectId))
        {
            _fileContents[projectId] = new Dictionary<string, string>();
        }

        _fileContents[projectId][filePath] = content;

        // 更新项目修改时间
        if (_projects.TryGetValue(projectId, out var project))
        {
            project.UpdatedAt = DateTime.Now;

            // 同步到本地文件系统
            if (project.IsSavedToLocal && !string.IsNullOrEmpty(project.LocalPath))
            {
                try
                {
                    var localFilePath = Path.Combine(project.LocalPath,
                        filePath.Replace('/', Path.DirectorySeparatorChar));
                    var fileDir = Path.GetDirectoryName(localFilePath);
                    if (!string.IsNullOrEmpty(fileDir) && !Directory.Exists(fileDir))
                    {
                        Directory.CreateDirectory(fileDir);
                    }

                    await File.WriteAllTextAsync(localFilePath, content, Encoding.UTF8);
                    _logger.LogDebug("文件已同步到本地: {FilePath}", localFilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "同步文件到本地失败: {FilePath}", filePath);
                }
            }
        }

        // 清除文件结构缓存
        _fileStructures.Remove(projectId);

        _logger.LogInformation("保存文件 {FilePath} 到项目 {ProjectId}", filePath, projectId);
    }

    public async Task<ProjectFile> CreateFileAsync(string projectId, string filePath, string content = "")
    {
        await SaveFileAsync(projectId, filePath, content);

        var file = CreateProjectFile(projectId, filePath, content);
        _logger.LogInformation("创建文件 {FilePath} 在项目 {ProjectId}", filePath, projectId);

        return file;
    }

    public async Task DeleteFileAsync(string projectId, string filePath)
    {
        await Task.Delay(50);

        if (_fileContents.TryGetValue(projectId, out var files))
        {
            files.Remove(filePath);
            _fileStructures.Remove(projectId);
        }

        // 从本地文件系统删除
        if (_projects.TryGetValue(projectId, out var project) &&
            project.IsSavedToLocal &&
            !string.IsNullOrEmpty(project.LocalPath))
        {
            try
            {
                var localFilePath = Path.Combine(project.LocalPath,
                    filePath.Replace('/', Path.DirectorySeparatorChar));

                if (File.Exists(localFilePath))
                {
                    File.Delete(localFilePath);
                    _logger.LogDebug("从本地删除文件: {FilePath}", localFilePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "从本地删除文件失败: {FilePath}", filePath);
            }
        }

        _logger.LogInformation("删除文件 {FilePath} 从项目 {ProjectId}", filePath, projectId);
    }

    public async Task RenameFileAsync(string projectId, string oldPath, string newPath)
    {
        await Task.Delay(50);

        if (_fileContents.TryGetValue(projectId, out var files))
        {
            if (files.TryGetValue(oldPath, out var content))
            {
                files.Remove(oldPath);
                files[newPath] = content;
                _fileStructures.Remove(projectId);
            }
        }

        // 在本地文件系统重命名
        if (_projects.TryGetValue(projectId, out var project) &&
            project.IsSavedToLocal &&
            !string.IsNullOrEmpty(project.LocalPath))
        {
            try
            {
                var localOldPath = Path.Combine(project.LocalPath,
                    oldPath.Replace('/', Path.DirectorySeparatorChar));
                var localNewPath = Path.Combine(project.LocalPath,
                    newPath.Replace('/', Path.DirectorySeparatorChar));

                if (File.Exists(localOldPath))
                {
                    var newDir = Path.GetDirectoryName(localNewPath);
                    if (!string.IsNullOrEmpty(newDir) && !Directory.Exists(newDir))
                    {
                        Directory.CreateDirectory(newDir);
                    }

                    File.Move(localOldPath, localNewPath);
                    _logger.LogDebug("在本地重命名文件: {OldPath} -> {NewPath}", localOldPath, localNewPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "在本地重命名文件失败");
            }
        }

        _logger.LogInformation("重命名文件 {OldPath} -> {NewPath} 在项目 {ProjectId}",
            oldPath, newPath, projectId);
    }

    #endregion

    #region 统计信息

    public async Task<ProjectStatistics> GetProjectStatisticsAsync(string projectId)
    {
        await Task.Delay(100);

        var files = await GetProjectFilesAsync(projectId);
        var allFiles = GetAllFiles(files);

        var stats = new ProjectStatistics
        {
            TotalFiles = allFiles.Count,
            TotalLines = allFiles.Sum(f => f.LineCount),
            TotalSize = allFiles.Sum(f => f.Size)
        };

        foreach (var file in allFiles)
        {
            var ext = Path.GetExtension(file.Path);
            if (!stats.FileTypeDistribution.ContainsKey(ext))
            {
                stats.FileTypeDistribution[ext] = 0;
            }
            stats.FileTypeDistribution[ext]++;
        }

        // 简单的代码统计
        var csFiles = allFiles.Where(f => f.Extension == ".cs").ToList();
        stats.ClassCount = csFiles.Count * 2; // 简化估算
        stats.InterfaceCount = csFiles.Count / 2;
        stats.MethodCount = csFiles.Count * 5;

        return stats;
    }

    #endregion

    #region 导入导出

    public async Task<Project> ImportProjectAsync(string zipFilePath)
    {
        await Task.Delay(500);

        var project = new Project
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Imported Project",
            Description = "从文件导入的项目",
            Namespace = "Imported.Project",
            TargetFramework = "net10.0",
            Template = "standard",
            Status = ProjectStatus.Developing,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            CreatedBy = "Current User"
        };

        _projects[project.Id] = project;
        await InitializeProjectFilesAsync(project.Id, "standard");

        _logger.LogInformation("导入项目 {ProjectName} ({ProjectId})", project.Name, project.Id);
        return project;
    }

    public async Task<string> ExportProjectAsync(string projectId)
    {
        await Task.Delay(500);

        if (!_projects.ContainsKey(projectId))
        {
            throw new Exception($"项目 {projectId} 不存在");
        }

        var exportPath = $"/downloads/{projectId}.zip";
        _logger.LogInformation("导出项目 {ProjectId} 到 {ExportPath}", projectId, exportPath);
        return exportPath;
    }

    #endregion

    #region 搜索

    public async Task<List<Project>> SearchProjectsAsync(string query)
    {
        await Task.Delay(100);

        if (string.IsNullOrWhiteSpace(query))
        {
            return await GetProjectsAsync();
        }

        return _projects.Values
            .Where(p =>
                p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(p => p.UpdatedAt)
            .ToList();
    }

    #endregion

    #region 私有辅助方法

    private void InitializeSampleData()
    {
        var sampleProjects = new[]
        {
            new Project
            {
                Id = "proj-001",
                Name = "订单处理流程",
                Description = "电商订单自动化处理系统",
                Namespace = "ECommerce.OrderWorkflow",
                TargetFramework = "net10.0",
                Template = "standard",
                Status = ProjectStatus.Developing,
                CreatedAt = DateTime.Now.AddDays(-10),
                UpdatedAt = DateTime.Now.AddHours(-2),
                CreatedBy = "Admin User",
                IsSavedToLocal = false
            },
            new Project
            {
                Id = "proj-002",
                Name = "用户审批流程",
                Description = "多级审批工作流系统",
                Namespace = "Enterprise.ApprovalWorkflow",
                TargetFramework = "net10.0",
                Template = "enterprise",
                Status = ProjectStatus.Completed,
                CreatedAt = DateTime.Now.AddDays(-30),
                UpdatedAt = DateTime.Now.AddDays(-1),
                CreatedBy = "Admin User",
                IsSavedToLocal = false
            },
            new Project
            {
                Id = "proj-003",
                Name = "库存管理系统",
                Description = "智能库存调度和补货流程",
                Namespace = "Warehouse.InventoryWorkflow",
                TargetFramework = "net10.0",
                Template = "microservice",
                Status = ProjectStatus.Deployed,
                CreatedAt = DateTime.Now.AddDays(-45),
                UpdatedAt = DateTime.Now.AddDays(-3),
                CreatedBy = "Admin User",
                IsSavedToLocal = false
            }
        };

        foreach (var project in sampleProjects)
        {
            _projects[project.Id] = project;
            InitializeProjectFilesAsync(project.Id, project.Template).Wait();
        }
    }

    private async Task InitializeProjectFilesAsync(string projectId, string template)
    {
        await Task.CompletedTask;

        if (!_fileContents.ContainsKey(projectId))
        {
            _fileContents[projectId] = new Dictionary<string, string>();
        }

        var files = _fileContents[projectId];

        // 项目文件
        files["Project.csproj"] = GenerateProjectFile(projectId);
        files["Program.cs"] = GenerateProgramFile();

        // 工作流
        files["Workflows/MainWorkflow.cs"] = GenerateWorkflowFile("MainWorkflow");

        // 活动
        files["Activities/StartActivity.cs"] = GenerateActivityFile("StartActivity");
        files["Activities/ProcessActivity.cs"] = GenerateActivityFile("ProcessActivity");
        files["Activities/EndActivity.cs"] = GenerateActivityFile("EndActivity");

        // 模型
        files["Models/WorkflowContext.cs"] = GenerateModelFile("WorkflowContext");
        files["Models/ActivityResult.cs"] = GenerateModelFile("ActivityResult");

        // 服务
        files["Services/IWorkflowEngine.cs"] = GenerateServiceInterfaceFile("IWorkflowEngine");
        files["Services/WorkflowEngine.cs"] = GenerateServiceImplementationFile("WorkflowEngine");

        // 配置
        files["appsettings.json"] = GenerateAppSettingsFile();
        files["appsettings.Development.json"] = GenerateDevAppSettingsFile();

        // README
        files["README.md"] = GenerateReadmeFile(projectId);
    }

    private ProjectFile CreateProjectFile(string projectId, string filePath, string content)
    {
        var fileName = Path.GetFileName(filePath);
        var isFolder = string.IsNullOrEmpty(fileName);

        string? localPath = null;
        if (_projects.TryGetValue(projectId, out var project) &&
            project.IsSavedToLocal &&
            !string.IsNullOrEmpty(project.LocalPath))
        {
            localPath = Path.Combine(project.LocalPath,
                filePath.Replace('/', Path.DirectorySeparatorChar));
        }

        return new ProjectFile
        {
            Path = filePath,
            Name = isFolder ? filePath.TrimEnd('/') : fileName,
            IsFolder = isFolder,
            IsDirty = false,
            LineCount = content?.Count(c => c == '\n') + 1 ?? 0,
            Size = content != null ? Encoding.UTF8.GetByteCount(content) : 0,
            LastModified = DateTime.Now,
            SubFiles = new List<ProjectFile>(),
            LocalPath = localPath
        };
    }

    private void AddFileToStructure(List<ProjectFile> structure, ProjectFile file)
    {
        var parts = file.Path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 1)
        {// 根级文件
            structure.Add(file);
        }
        else
        {
            // 需要创建或找到文件夹
            var currentLevel = structure;
            for (int i = 0; i < parts.Length - 1; i++)
            {
                var folderName = parts[i];
                var folderPath = string.Join("/", parts.Take(i + 1));

                var folder = currentLevel.FirstOrDefault(f =>
                    f.IsFolder && f.Name == folderName);

                if (folder == null)
                {
                    folder = new ProjectFile
                    {
                        Path = folderPath,
                        Name = folderName,
                        IsFolder = true,
                        SubFiles = new List<ProjectFile>(),
                        LastModified = DateTime.Now
                    };
                    currentLevel.Add(folder);
                }

                currentLevel = folder.SubFiles;
            }

            currentLevel.Add(file);
        }
    }

    private List<ProjectFile> GetAllFiles(List<ProjectFile> files)
    {
        var result = new List<ProjectFile>();

        foreach (var file in files)
        {
            if (!file.IsFolder)
            {
                result.Add(file);
            }

            if (file.SubFiles != null && file.SubFiles.Any())
            {
                result.AddRange(GetAllFiles(file.SubFiles));
            }
        }

        return result;
    }

    #endregion

    #region 代码生成（保持原有的生成方法）

    private string GenerateProjectFile(string projectId)
    {
        var projectName = _projects.TryGetValue(projectId, out var proj) ? proj.Name : "GeneratedProject";

        return $@"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>{projectName.Replace(" ", "").Replace("-", "")}</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include=""Microsoft.Extensions.DependencyInjection"" Version=""8.0.0"" />
    <PackageReference Include=""Microsoft.Extensions.Logging"" Version=""8.0.0"" />
    <PackageReference Include=""Microsoft.Extensions.Logging.Console"" Version=""8.0.0"" />
    <PackageReference Include=""Serilog"" Version=""3.1.1"" />
  </ItemGroup>
</Project>";
    }
    private string GenerateProgramFile()
    {
        return @"using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
namespace FlowForge.Generated;
/// <summary>
/// 程序主入口
/// </summary>
public class Program
{
public static async Task Main(string[] args)
{
Console.WriteLine(""=== FlowForge 工作流引擎 ==="" + Environment.NewLine);
    // 配置服务容器
    var services = new ServiceCollection();

    // 添加日志服务
    services.AddLogging(builder => 
    {
        builder.AddConsole();
        builder.SetMinimumLevel(LogLevel.Information);
    });

    // 注册工作流服务
    services.AddSingleton<IWorkflowEngine, WorkflowEngine>();
    services.AddTransient<MainWorkflow>();

    var serviceProvider = services.BuildServiceProvider();

    try
    {
        // 创建工作流引擎
        var engine = serviceProvider.GetRequiredService<IWorkflowEngine>();
        
        // 创建工作流上下文
        var context = new WorkflowContext();
        context.SetVariable(""StartTime"", DateTime.Now);

        Console.WriteLine($""工作流 ID: {context.Id}"");
        Console.WriteLine($""开始时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}"" + Environment.NewLine);

        // 执行工作流
        var result = await engine.ExecuteAsync(context);

        Console.WriteLine(Environment.NewLine + ""=== 执行结果 ===="");
        Console.WriteLine($""状态: {(result.Success ? ""成功"" : ""失败"")}"");
        Console.WriteLine($""消息: {result.Message}"");
        Console.WriteLine($""执行时长: {(DateTime.Now - context.StartTime).TotalMilliseconds:F2} ms"");

        if (context.ExecutionLog.Any())
        {
            Console.WriteLine(Environment.NewLine + ""=== 执行日志 ===="");
            foreach (var log in context.ExecutionLog)
            {
                Console.WriteLine(log);
            }
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($""错误: {ex.Message}"");
        Console.ResetColor();
    }

    Console.WriteLine(Environment.NewLine + ""按任意键退出..."");
    Console.ReadKey();
}
}";
    }
    private string GenerateWorkflowFile(string name)
    {
        return $@"using Microsoft.Extensions.Logging;
namespace FlowForge.Generated.Workflows;
/// <summary>
/// {name} - 主工作流定义
/// </summary>
public class {name}
{{
private readonly ILogger<{name}> _logger;
public {name}(ILogger<{name}> logger)
{{
    _logger = logger;
}}

/// <summary>
/// 执行工作流
/// </summary>
public async Task<ActivityResult> ExecuteAsync(WorkflowContext context)
{{
    _logger.LogInformation(""开始执行工作流: {{WorkflowName}}"", ""{name}"");
    context.AddLog(""工作流开始执行"");

    try
    {{
        // 开始活动
        var startResult = await ExecuteStartActivityAsync(context);
        if (!startResult.Success)
        {{
            _logger.LogError(""开始活动执行失败"");
            return ActivityResult.Failed(""开始活动执行失败"");
        }}

        // 处理活动
        var processResult = await ExecuteProcessActivityAsync(context);
        if (!processResult.Success)
        {{
            _logger.LogError(""处理活动执行失败"");
            return ActivityResult.Failed(""处理活动执行失败"");
        }}

        // 结束活动
        var endResult = await ExecuteEndActivityAsync(context);

        _logger.LogInformation(""工作流执行完成: {{WorkflowName}}"", ""{name}"");
        context.AddLog(""工作流执行完成"");

        return endResult;
    }}
    catch (Exception ex)
    {{
        _logger.LogError(ex, ""工作流执行异常"");
        context.AddLog($""工作流异常: {{ex.Message}}"");
        return ActivityResult.Failed($""工作流异常: {{ex.Message}}"");
    }}
}}

private async Task<ActivityResult> ExecuteStartActivityAsync(WorkflowContext context)
{{
    _logger.LogDebug(""执行开始活动"");
    context.AddLog(""执行: 开始活动"");
    await Task.Delay(100);
    return ActivityResult.Success(""开始活动完成"");
}}

private async Task<ActivityResult> ExecuteProcessActivityAsync(WorkflowContext context)
{{
    _logger.LogDebug(""执行处理活动"");
    context.AddLog(""执行: 处理活动"");
    await Task.Delay(200);
    
    // 模拟业务处理
    context.SetVariable(""ProcessedCount"", 100);
    
    return ActivityResult.Success(""处理活动完成"");
}}

private async Task<ActivityResult> ExecuteEndActivityAsync(WorkflowContext context)
{{
    _logger.LogDebug(""执行结束活动"");
    context.AddLog(""执行: 结束活动"");
    await Task.Delay(100);
    
    var processedCount = context.GetVariable<int>(""ProcessedCount"");
    context.AddLog($""处理数量: {{processedCount}}"");
    
    return ActivityResult.Success(""结束活动完成"");
}}
}}";
    }
    private string GenerateActivityFile(string name)
    {
        return $@"using Microsoft.Extensions.Logging;
namespace FlowForge.Generated.Activities;
/// <summary>
/// {name} - 活动实现
/// </summary>
public class {name}
{{
private readonly ILogger<{name}> _logger;
public {name}(ILogger<{name}> logger)
{{
    _logger = logger;
}}

/// <summary>
/// 执行活动
/// </summary>
public async Task<ActivityResult> ExecuteAsync(WorkflowContext context)
{{
    _logger.LogInformation(""执行活动: {{ActivityName}}"", ""{name}"");
    context.AddLog($""开始执行活动: {name}"");

    try
    {{
        // TODO: 实现具体的业务逻辑
        await Task.Delay(100);

        _logger.LogInformation(""活动执行成功: {{ActivityName}}"", ""{name}"");
        context.AddLog($""活动执行成功: {name}"");
        
        return ActivityResult.Success(""活动执行成功"");
    }}
    catch (Exception ex)
    {{
        _logger.LogError(ex, ""活动执行异常: {{ActivityName}}"", ""{name}"");
        context.AddLog($""活动执行异常: {name} - {{ex.Message}}"");
        return ActivityResult.Failed($""活动异常: {{ex.Message}}"");
    }}
}}
}}";
    }
    private string GenerateModelFile(string name)
    {
        if (name == "WorkflowContext")
        {
            return @"namespace FlowForge.Generated.Models;
/// <summary>
/// 工作流上下文 - 存储工作流执行过程中的状态和数据
/// </summary>
public class WorkflowContext
{
/// <summary>
/// 工作流实例 ID
/// </summary>
public string Id { get; set; } = Guid.NewGuid().ToString();
/// <summary>
/// 工作流开始时间
/// </summary>
public DateTime StartTime { get; set; } = DateTime.Now;

/// <summary>
/// 工作流变量集合
/// </summary>
public Dictionary<string, object> Variables { get; set; } = new();

/// <summary>
/// 执行日志
/// </summary>
public List<string> ExecutionLog { get; set; } = new();

/// <summary>
/// 添加日志
/// </summary>
public void AddLog(string message)
{
    ExecutionLog.Add($""[{DateTime.Now:HH:mm:ss.fff}] {message}"");
}

/// <summary>
/// 获取变量值
/// </summary>
public T? GetVariable<T>(string key)
{
    if (Variables.TryGetValue(key, out var value))
    {
        if (value is T typedValue)
        {
            return typedValue;
        }
        
        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return default;
        }
    }
    return default;
}

/// <summary>
/// 设置变量值
/// </summary>
public void SetVariable(string key, object value)
{
    Variables[key] = value;
}

/// <summary>
/// 检查变量是否存在
/// </summary>
public bool HasVariable(string key)
{
    return Variables.ContainsKey(key);
}

/// <summary>
/// 移除变量
/// </summary>
public bool RemoveVariable(string key)
{
    return Variables.Remove(key);
}
}";
        }
        else if (name == "ActivityResult")
        {
            return @"namespace FlowForge.Generated.Models;
/// <summary>
/// 活动执行结果
/// </summary>
public class ActivityResult
{
/// <summary>
/// 是否成功
/// </summary>
public bool Success { get; set; }
/// <summary>
/// 结果消息
/// </summary>
public string Message { get; set; } = """";

/// <summary>
/// 附加数据
/// </summary>
public Dictionary<string, object> Data { get; set; } = new();

/// <summary>
/// 创建成功结果
/// </summary>
public static ActivityResult Success(string message)
{
    return new ActivityResult
    {
        Success = true,
        Message = message
    };
}

/// <summary>
/// 创建失败结果
/// </summary>
public static ActivityResult Failed(string message)
{
    return new ActivityResult
    {
        Success = false,
        Message = message
    };
}

/// <summary>
/// 添加附加数据
/// </summary>
public ActivityResult WithData(string key, object value)
{
    Data[key] = value;
    return this;
}
}";
        }
        return "";
    }

    private string GenerateServiceInterfaceFile(string name)
    {
        return $@"namespace FlowForge.Generated.Services;
/// <summary>
/// {name} - 工作流引擎接口
/// </summary>
public interface {name}
{{
/// <summary>
/// 执行工作流
/// </summary>
/// <param name=""context"">工作流上下文</param>
/// <returns>执行结果</returns>
Task<ActivityResult> ExecuteAsync(WorkflowContext context);
/// <summary>
/// 获取工作流状态
/// </summary>
/// <param name=""workflowId"">工作流 ID</param>
/// <returns>工作流状态</returns>
Task<WorkflowStatus> GetStatusAsync(string workflowId);
}}
/// <summary>
/// 工作流状态
/// </summary>
public class WorkflowStatus
{{
public string Id {{ get; set; }} = """";
public string Status {{ get; set; }} = """"; // Running, Completed, Failed
public int Progress {{ get; set; }}
public DateTime? StartTime {{ get; set; }}
public DateTime? EndTime {{ get; set; }}
}}";
    }
    private string GenerateServiceImplementationFile(string name)
    {
        return $@"using Microsoft.Extensions.Logging;
namespace FlowForge.Generated.Services;
/// <summary>
/// {name} - 工作流引擎实现
/// </summary>
public class {name} : IWorkflowEngine
{{
private readonly ILogger<{name}> _logger;
private readonly IServiceProvider _serviceProvider;
public {name}(
    ILogger<{name}> logger,
    IServiceProvider serviceProvider)
{{
    _logger = logger;
    _serviceProvider = serviceProvider;
}}

/// <summary>
/// 执行工作流
/// </summary>
public async Task<ActivityResult> ExecuteAsync(WorkflowContext context)
{{
    _logger.LogInformation(""工作流引擎开始执行 {{WorkflowId}}"", context.Id);

    try
    {{
        // 创建工作流实例
        var workflow = _serviceProvider.GetService(typeof(MainWorkflow)) as MainWorkflow;
        if (workflow == null)
        {{
            throw new InvalidOperationException(""无法创建工作流实例"");
        }}

        // 执行工作流
        var result = await workflow.ExecuteAsync(context);

        _logger.LogInformation(
            ""工作流执行完成 {{WorkflowId}}, 状态: {{Status}}"", 
            context.Id, 
            result.Success ? ""成功"" : ""失败"");

        return result;
    }}
    catch (Exception ex)
    {{
        _logger.LogError(ex, ""工作流执行异常 {{WorkflowId}}"", context.Id);
        return ActivityResult.Failed($""执行异常: {{ex.Message}}"");
    }}
}}

/// <summary>
/// 获取工作流状态
/// </summary>
public async Task<WorkflowStatus> GetStatusAsync(string workflowId)
{{
    await Task.CompletedTask;

    // TODO: 从存储中获取实际状态
    return new WorkflowStatus
    {{
        Id = workflowId,
        Status = ""Running"",
        Progress = 50,
        StartTime = DateTime.Now.AddMinutes(-5)
    }};
}}
}}";
    }
    private string GenerateAppSettingsFile()
    {
        return @"{
""Logging"": {
""LogLevel"": {
""Default"": ""Information"",
""Microsoft"": ""Warning"",
""Microsoft.Hosting.Lifetime"": ""Information"",
""System"": ""Warning""
},
""Console"": {
""FormatterName"": ""simple"",
""FormatterOptions"": {
""SingleLine"": true,
""IncludeScopes"": true,
""TimestampFormat"": ""yyyy-MM-dd HH:mm:ss ""
}
}
},
""Workflow"": {
""MaxParallelTasks"": 10,
""TimeoutSeconds"": 300,
""RetryCount"": 3,
""RetryDelaySeconds"": 5
},
""Database"": {
""ConnectionString"": ""Data Source=workflow.db"",
""Provider"": ""SQLite""
}
}";
    }
    private string GenerateDevAppSettingsFile()
    {
        return @"{
""Logging"": {
""LogLevel"": {
""Default"": ""Debug"",
""Microsoft"": ""Information"",
""System"": ""Information""
}
},
""Workflow"": {
""TimeoutSeconds"": 600
}
}";
    }
    private string GenerateReadmeFile(string projectId)
    {
        var projectName = _projects.TryGetValue(projectId, out var proj) ? proj.Name : "Generated Project";

        return $@"# {projectName}

由 FlowForge 自动生成的工作流项目

📋 项目信息

项目 ID: {projectId}
生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
目标框架: .NET 10.0

📁 项目结构
{projectName}/
├── Program.cs                      # 程序入口点
├── Project.csproj                  # 项目配置文件
├── Workflows/                      # 工作流定义
│   └── MainWorkflow.cs            # 主工作流
├── Activities/                     # 活动实现
│   ├── StartActivity.cs           # 开始活动
│   ├── ProcessActivity.cs         # 处理活动
│   └── EndActivity.cs             # 结束活动
├── Models/                         # 数据模型
│   ├── WorkflowContext.cs         # 工作流上下文
│   └── ActivityResult.cs          # 活动结果
├── Services/                       # 服务层
│   ├── IWorkflowEngine.cs         # 工作流引擎接口
│   └── WorkflowEngine.cs          # 工作流引擎实现
├── appsettings.json               # 应用配置
├── appsettings.Development.json   # 开发环境配置
└── README.md                       # 项目说明文档
🚀 快速开始
1. 恢复依赖包
bashdotnet restore
2. 构建项目
bashdotnet build
3. 运行项目
bashdotnet run
🔧 开发指南
修改工作流逻辑
编辑 Workflows/MainWorkflow.cs 文件来修改工作流的执行逻辑：
csharppublic async Task<ActivityResult> ExecuteAsync(WorkflowContext context)
{{
    // 在这里添加你的工作流逻辑
}}
添加新活动

在 Activities/ 目录下创建新的活动类
继承并实现活动接口
在工作流中调用新活动

csharppublic class MyNewActivity
{{
    public async Task<ActivityResult> ExecuteAsync(WorkflowContext context)
    {{
        // 实现活动逻辑
        return ActivityResult.Success(""活动完成"");
    }}
}}
配置修改
编辑 appsettings.json 来修改应用配置：
json{{
  ""Workflow"": {{
    ""MaxParallelTasks"": 10,
    ""TimeoutSeconds"": 300
  }}
}}
📦 依赖包

Microsoft.Extensions.DependencyInjection - 依赖注入容器
Microsoft.Extensions.Logging - 日志框架
Serilog - 结构化日志

🐛 调试技巧
启用详细日志
修改 appsettings.Development.json:
json{{
  ""Logging"": {{
    ""LogLevel"": {{
      ""Default"": ""Debug""
    }}
  }}
}}
```

### 使用断点调试

在 Visual Studio 或 VS Code 中设置断点，然后按 F5 开始调试。

## 📝 代码规范

- 使用 `async/await` 进行异步编程
- 所有公共方法都应添加 XML 文档注释
- 使用依赖注入管理对象生命周期
- 异常应被正确捕获和记录

## 🔗 相关链接

- [FlowForge 官方文档](https://flowforge.io/docs)
- [.NET 10 文档](https://docs.microsoft.com/dotnet)
- [工作流最佳实践](https://flowforge.io/best-practices)

## 📄 许可证

Copyright © {DateTime.Now.Year} FlowForge

---

**由 FlowForge 自动生成** | 生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
";
    }

    #endregion
}