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

    public ProjectService(
        HttpClient httpClient,
        ILogger<ProjectService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        InitializeSampleData();
    }

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

        _logger.LogInformation("更新项目 {ProjectId}", projectId);
        return project;
    }

    public async Task DeleteProjectAsync(string projectId)
    {
        await Task.Delay(100);
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
            // 按路径组织文件结构
            foreach (var kvp in projectFiles.OrderBy(x => x.Key))
            {
                var file = CreateProjectFile(kvp.Key, kvp.Value);
                AddFileToStructure(fileStructure, file);
            }
        }

        _fileStructures[projectId] = fileStructure;
        return fileStructure;
    }

    public async Task<string> GetFileContentAsync(string projectId, string filePath)
    {
        await Task.Delay(50);

        if (_fileContents.TryGetValue(projectId, out var files))
        {
            if (files.TryGetValue(filePath, out var content))
            {
                return content;
            }
        }

        // 如果文件不存在，返回空内容而不是抛出异常
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
        }

        // 清除文件结构缓存，下次获取时重新构建
        _fileStructures.Remove(projectId);

        _logger.LogInformation("保存文件 {FilePath} 到项目 {ProjectId}", filePath, projectId);
    }

    public async Task<ProjectFile> CreateFileAsync(string projectId, string filePath, string content = "")
    {
        await SaveFileAsync(projectId, filePath, content);

        var file = CreateProjectFile(filePath, content);
        _logger.LogInformation("创建文件 {FilePath} 在项目 {ProjectId}", filePath, projectId);

        return file;
    }

    public async Task DeleteFileAsync(string projectId, string filePath)
    {
        await Task.Delay(50);

        if (_fileContents.TryGetValue(projectId, out var files))
        {
            files.Remove(filePath);

            // 清除文件结构缓存
            _fileStructures.Remove(projectId);
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

                // 清除文件结构缓存
                _fileStructures.Remove(projectId);
            }
        }

        _logger.LogInformation("重命名文件 {OldPath} -> {NewPath} 在项目 {ProjectId}",
            oldPath, newPath, projectId);
    }

    #endregion

    #region 项目统计

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

        // 分析文件类型
        foreach (var file in allFiles)
        {
            var ext = Path.GetExtension(file.Path);
            if (!stats.FileTypeDistribution.ContainsKey(ext))
            {
                stats.FileTypeDistribution[ext] = 0;
            }
            stats.FileTypeDistribution[ext]++;
        }

        // 模拟代码统计
        stats.ClassCount = new Random().Next(10, 50);
        stats.InterfaceCount = new Random().Next(5, 20);
        stats.MethodCount = new Random().Next(50, 200);

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
                CreatedBy = "Admin User"
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
                CreatedBy = "Admin User"
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
                CreatedBy = "Admin User"
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

    private ProjectFile CreateProjectFile(string filePath, string content)
    {
        var fileName = Path.GetFileName(filePath);
        var isFolder = string.IsNullOrEmpty(fileName);

        return new ProjectFile
        {
            Path = filePath,
            Name = isFolder ? filePath.TrimEnd('/') : fileName,
            IsFolder = isFolder,
            IsDirty = false,
            LineCount = content?.Count(c => c == '\n') + 1 ?? 0,
            Size = content != null ? Encoding.UTF8.GetByteCount(content) : 0,
            LastModified = DateTime.Now,
            SubFiles = new List<ProjectFile>()
        };
    }

    private void AddFileToStructure(List<ProjectFile> structure, ProjectFile file)
    {
        var parts = file.Path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 1)
        {
            // 根级文件
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

    #region 代码生成

    private string GenerateProjectFile(string projectId)
    {
        return $@"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Microsoft.Extensions.DependencyInjection"" Version=""8.0.0"" />
    <PackageReference Include=""Microsoft.Extensions.Logging"" Version=""8.0.0"" />
    <PackageReference Include=""Serilog"" Version=""3.1.1"" />
  </ItemGroup>

</Project>";
    }

    private string GenerateProgramFile()
    {
        return @"using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FlowForge.Generated;

public class Program
{
    public static async Task Main(string[] args)
    {
        var services = new ServiceCollection();

        // 配置服务
        services.AddLogging(builder => 
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        services.AddSingleton<IWorkflowEngine, WorkflowEngine>();

        var serviceProvider = services.BuildServiceProvider();

        // 运行工作流
        var engine = serviceProvider.GetRequiredService<IWorkflowEngine>();
        var context = new WorkflowContext();

        await engine.ExecuteAsync(context);

        Console.WriteLine(""工作流执行完成"");
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

            return endResult;
        }}
        catch (Exception ex)
        {{
            _logger.LogError(ex, ""工作流执行异常"");
            return ActivityResult.Failed($""工作流异常: {{ex.Message}}"");
        }}
    }}

    private async Task<ActivityResult> ExecuteStartActivityAsync(WorkflowContext context)
    {{
        _logger.LogDebug(""执行开始活动"");
        await Task.Delay(100);
        return ActivityResult.Success(""开始活动完成"");
    }}

    private async Task<ActivityResult> ExecuteProcessActivityAsync(WorkflowContext context)
    {{
        _logger.LogDebug(""执行处理活动"");
        await Task.Delay(200);
        return ActivityResult.Success(""处理活动完成"");
    }}

    private async Task<ActivityResult> ExecuteEndActivityAsync(WorkflowContext context)
    {{
        _logger.LogDebug(""执行结束活动"");
        await Task.Delay(100);
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

        try
        {{
            // 活动逻辑
            await Task.Delay(100);

            _logger.LogInformation(""活动执行成功: {{ActivityName}}"", ""{name}"");
            return ActivityResult.Success(""活动执行成功"");
        }}
        catch (Exception ex)
        {{
            _logger.LogError(ex, ""活动执行异常: {{ActivityName}}"", ""{name}"");
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
/// 工作流上下文
/// </summary>
public class WorkflowContext
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime StartTime { get; set; } = DateTime.Now;
    public Dictionary<string, object> Variables { get; set; } = new();
    public List<string> ExecutionLog { get; set; } = new();

    public void AddLog(string message)
    {
        ExecutionLog.Add($""[{DateTime.Now:HH:mm:ss.fff}] {message}"");
    }

    public T? GetVariable<T>(string key)
    {
        if (Variables.TryGetValue(key, out var value))
        {
            return (T?)value;
        }
        return default;
    }

    public void SetVariable(string key, object value)
    {
        Variables[key] = value;
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
    public bool Success { get; set; }
    public string Message { get; set; } = """";
    public Dictionary<string, object> Data { get; set; } = new();

    public static ActivityResult Success(string message)
    {
        return new ActivityResult
        {
            Success = true,
            Message = message
        };
    }

    public static ActivityResult Failed(string message)
    {
        return new ActivityResult
        {
            Success = false,
            Message = message
        };
    }
}";
        }

        return "";
    }

    private string GenerateServiceInterfaceFile(string name)
    {
        return $@"namespace FlowForge.Generated.Services;

/// <summary>
/// {name} - 服务接口
/// </summary>
public interface {name}
{{
    /// <summary>
    /// 执行工作流
    /// </summary>
    Task<ActivityResult> ExecuteAsync(WorkflowContext context);

    /// <summary>
    /// 获取工作流状态
    /// </summary>
    Task<WorkflowStatus> GetStatusAsync(string workflowId);
}}";
    }

    private string GenerateServiceImplementationFile(string name)
    {
        return $@"using Microsoft.Extensions.Logging;

namespace FlowForge.Generated.Services;

/// <summary>
/// {name} - 服务实现
/// </summary>
public class {name} : IWorkflowEngine
{{
    private readonly ILogger<{name}> _logger;

    public {name}(ILogger<{name}> logger)
    {{
        _logger = logger;
    }}

    public async Task<ActivityResult> ExecuteAsync(WorkflowContext context)
    {{
        _logger.LogInformation(""开始执行工作流 {{WorkflowId}}"", context.Id);

        try
        {{
            var workflow = new MainWorkflow(_logger);
            var result = await workflow.ExecuteAsync(context);

            _logger.LogInformation(""工作流执行完成 {{WorkflowId}}"", context.Id);

            return result;
        }}
        catch (Exception ex)
        {{
            _logger.LogError(ex, ""工作流执行异常 {{WorkflowId}}"", context.Id);
            return ActivityResult.Failed($""执行异常: {{ex.Message}}"");
        }}
    }}

    public async Task<WorkflowStatus> GetStatusAsync(string workflowId)
    {{
        await Task.CompletedTask;

        return new WorkflowStatus
        {{
            Id = workflowId,
            Status = ""Running"",
            Progress = 50
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
      ""Microsoft.Hosting.Lifetime"": ""Information""
    }
  },
  ""Workflow"": {
    ""MaxParallelTasks"": 10,
    ""TimeoutSeconds"": 300
  }
}";
    }

    private string GenerateDevAppSettingsFile()
    {
        return @"{
  ""Logging"": {
    ""LogLevel"": {
      ""Default"": ""Debug"",
      ""Microsoft"": ""Information""
    }
  }
}";
    }

    private string GenerateReadmeFile(string projectId)
    {
        return $@"# FlowForge Generated Project

项目 ID: {projectId}
生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}

## 项目结构
```
├── Program.cs                      # 程序入口
├── Project.csproj                  # 项目文件
├── Workflows/                      # 工作流定义
│   └── MainWorkflow.cs
├── Activities/                     # 活动实现
│   ├── StartActivity.cs
│   ├── ProcessActivity.cs
│   └── EndActivity.cs
├── Models/                         # 数据模型
│   ├── WorkflowContext.cs
│   └── ActivityResult.cs
├── Services/                       # 服务层
│   ├── IWorkflowEngine.cs
│   └── WorkflowEngine.cs
└── appsettings.json               # 配置文件
```

## 运行项目
```bash
dotnet run
```

## 开发指南

1. 修改工作流逻辑请编辑 `Workflows/MainWorkflow.cs`
2. 添加新活动请在 `Activities/` 目录下创建新类
3. 修改配置请编辑 `appsettings.json`

---
由 FlowForge 自动生成";
    }

    #endregion
}
