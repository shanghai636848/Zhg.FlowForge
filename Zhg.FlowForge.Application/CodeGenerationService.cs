using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Zhg.FlowForge.Application.Contract;
using Zhg.FlowForge.Domain;

namespace Zhg.FlowForge.Application;

/// <summary>
/// 代码生成应用服务实现
/// </summary>
public class CodeGenerationService : ICodeGenerationService
{
    private readonly IBpmnProcessRepository _bpmnRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<CodeGenerationService> _logger;

    public CodeGenerationService(
        IBpmnProcessRepository bpmnRepository,
        IProjectRepository projectRepository,
        ILogger<CodeGenerationService> logger)
    {
        _bpmnRepository = bpmnRepository;
        _projectRepository = projectRepository;
        _logger = logger;
    }

    public async Task<GenerationResultDto> GenerateAsync(
        GenerationRequestDto request,
        IProgress<GenerationProgressDto>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new GenerationResultDto();

        try
        {
            _logger.LogInformation("开始生成代码，项目: {ProjectName}", request.Config.ProjectName);

            // 1. 验证配置
            progress?.Report(new GenerationProgressDto
            {
                Percentage = 5,
                Message = "验证配置..."
            });

            var validation = await ValidateConfigurationAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                result.Success = false;
                result.Error = string.Join(", ", validation.Errors.Select(e => e.Message));
                return result;
            }

            // 2. 加载 BPMN 模型
            progress?.Report(new GenerationProgressDto
            {
                Percentage = 10,
                Message = "加载 BPMN 模型..."
            });

            var bpmnModel = await _bpmnRepository.GetByIdAsync(request.ProcessId, cancellationToken);
            if (bpmnModel == null)
            {
                result.Success = false;
                result.Error = "BPMN 流程不存在";
                return result;
            }

            // 3. 生成项目文件
            progress?.Report(new GenerationProgressDto
            {
                Percentage = 20,
                Message = "生成项目文件...",
                CurrentFile = "*.csproj"
            });
            await Task.Delay(300, cancellationToken);

            var projectFile = GenerateProjectFile(request, bpmnModel);
            result.Files.Add(projectFile);

            // 4. 生成主程序
            progress?.Report(new GenerationProgressDto
            {
                Percentage = 30,
                Message = "生成主程序...",
                CurrentFile = "Program.cs"
            });
            await Task.Delay(300, cancellationToken);

            var programFile = GenerateProgramFile(request, bpmnModel);
            result.Files.Add(programFile);

            // 5. 生成工作流类
            progress?.Report(new GenerationProgressDto
            {
                Percentage = 50,
                Message = "生成工作流定义...",
                CurrentFile = "Workflows/"
            });
            await Task.Delay(400, cancellationToken);

            var workflowFile = GenerateWorkflowFile(request, bpmnModel);
            result.Files.Add(workflowFile);

            // 6. 生成活动类
            progress?.Report(new GenerationProgressDto
            {
                Percentage = 70,
                Message = "生成活动类...",
                CurrentFile = "Activities/"
            });
            await Task.Delay(400, cancellationToken);

            var activityFiles = GenerateActivityFiles(request, bpmnModel);
            result.Files.AddRange(activityFiles);

            // 7. 生成模型类
            progress?.Report(new GenerationProgressDto
            {
                Percentage = 85,
                Message = "生成数据模型...",
                CurrentFile = "Models/"
            });
            await Task.Delay(300, cancellationToken);

            var modelFiles = GenerateModelFiles(request);
            result.Files.AddRange(modelFiles);

            // 8. 生成配置文件
            progress?.Report(new GenerationProgressDto
            {
                Percentage = 95,
                Message = "生成配置文件...",
                CurrentFile = "appsettings.json"
            });
            await Task.Delay(200, cancellationToken);

            var configFiles = GenerateConfigurationFiles();
            result.Files.AddRange(configFiles);

            // 完成
            progress?.Report(new GenerationProgressDto
            {
                Percentage = 100,
                Message = "生成完成!"
            });

            result.Success = true;
            result.TotalLines = result.Files.Sum(f => f.LineCount);
            result.ProjectPath = $"/projects/{request.Config.ProjectName}";

            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;

            _logger.LogInformation(
                "代码生成成功: {FileCount} 个文件, {LineCount} 行代码, 耗时 {Duration}ms",
                result.Files.Count,
                result.TotalLines,
                result.Duration.TotalMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "代码生成失败");

            result.Success = false;
            result.Error = $"生成异常: {ex.Message}";

            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;

            return result;
        }
    }

    public async Task<List<GeneratedFileDto>> PreviewAsync(
        GenerationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var files = new List<GeneratedFileDto>();

        try
        {
            var bpmnModel = await _bpmnRepository.GetByIdAsync(request.ProcessId, cancellationToken);
            if (bpmnModel == null) return files;

            // 生成预览文件（只生成主要文件）
            files.Add(GenerateProjectFile(request, bpmnModel));
            files.Add(GenerateProgramFile(request, bpmnModel));
            files.Add(GenerateWorkflowFile(request, bpmnModel));

            return files;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "预览生成失败");
            return files;
        }
    }

    public async Task<ValidationResultDto> ValidateConfigurationAsync(
        GenerationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        var result = new ValidationResultDto { IsValid = true };

        // 验证项目名称
        if (string.IsNullOrWhiteSpace(request.Config.ProjectName))
        {
            result.IsValid = false;
            result.Errors.Add(new ValidationErrorDto
            {
                ElementId = "ProjectName",
                Message = "项目名称不能为空"
            });
        }

        // 验证命名空间
        if (string.IsNullOrWhiteSpace(request.Config.Namespace))
        {
            result.IsValid = false;
            result.Errors.Add(new ValidationErrorDto
            {
                ElementId = "Namespace",
                Message = "命名空间不能为空"
            });
        }

        // 验证流程 ID
        if (string.IsNullOrWhiteSpace(request.ProcessId))
        {
            result.IsValid = false;
            result.Errors.Add(new ValidationErrorDto
            {
                ElementId = "ProcessId",
                Message = "流程 ID 不能为空"
            });
        }

        // 警告：AOT 优化
        if (request.Options.EnableAotOptimizations)
        {
            result.Warnings.Add(new ValidationWarningDto
            {
                ElementId = "EnableAotOptimizations",
                Message = "AOT 优化可能会限制某些反射功能"
            });
        }

        return result;
    }

    public async Task<List<CodeTemplateDto>> GetTemplatesAsync(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        return new List<CodeTemplateDto>
        {
            new CodeTemplateDto
            {
                Id = "standard",
                Name = "标准模板",
                Description = "适用于大多数业务流程，包含完整的工作流引擎",
                Category = "通用",
                Features = new List<string>
                {
                    "完整工作流引擎",
                    "依赖注入",
                    "异步支持",
                    "日志记录",
                    "异常处理"
                }
            },
            new CodeTemplateDto
            {
                Id = "minimal",
                Name = "最小模板",
                Description = "轻量级实现，适合简单流程",
                Category = "轻量",
                Features = new List<string>
                {
                    "无外部依赖",
                    "快速启动",
                    "易于理解"
                }
            },
            new CodeTemplateDto
            {
                Id = "microservice",
                Name = "微服务模板",
                Description = "容器化微服务架构，支持 Kubernetes 部署",
                Category = "云原生",
                Features = new List<string>
                {
                    "Docker 支持",
                    "健康检查",
                    "指标监控"
                }
            }
        };
    }

    #region Code Generation

    private GeneratedFileDto GenerateProjectFile(GenerationRequestDto request, BpmnProcess bpmnModel)
    {
        var config = request.Config;
        var content = new StringBuilder();

        content.AppendLine("<Project Sdk=\"Microsoft.NET.Sdk\">");
        content.AppendLine();
        content.AppendLine("  <PropertyGroup>");
        content.AppendLine($"    <OutputType>Exe</OutputType>");
        content.AppendLine($"    <TargetFramework>{config.TargetFramework}</TargetFramework>");
        content.AppendLine($"    <Nullable>{(config.EnableNullable ? "enable" : "disable")}</Nullable>");
        content.AppendLine($"    <ImplicitUsings>{(config.ImplicitUsings ? "enable" : "disable")}</ImplicitUsings>");
        content.AppendLine($"    <Version>{config.Version}</Version>");

        if (request.Options.EnableAotOptimizations)
        {
            content.AppendLine("    <PublishAot>true</PublishAot>");
        }

        content.AppendLine("  </PropertyGroup>");
        content.AppendLine();

        if (request.Dependencies.Any())
        {
            content.AppendLine("  <ItemGroup>");
            foreach (var dep in request.Dependencies)
            {
                content.AppendLine($"    <PackageReference Include=\"{dep.PackageId}\" Version=\"{dep.Version}\" />");
            }
            content.AppendLine("  </ItemGroup>");
            content.AppendLine();
        }

        content.AppendLine("</Project>");

        return new GeneratedFileDto
        {
            Path = $"{config.ProjectName}.csproj",
            Content = content.ToString()
        };
    }

    private GeneratedFileDto GenerateProgramFile(GenerationRequestDto request, BpmnProcess bpmnModel)
    {
        var config = request.Config;
        var options = request.Options;

        var content = new StringBuilder();

        if (!config.ImplicitUsings)
        {
            content.AppendLine("using System;");
            content.AppendLine("using System.Threading.Tasks;");
            content.AppendLine("using Microsoft.Extensions.DependencyInjection;");
            if (options.GenerateLogging)
            {
                content.AppendLine("using Microsoft.Extensions.Logging;");
            }
            content.AppendLine();
        }

        if (options.UseFileScoped)
        {
            content.AppendLine($"namespace {config.Namespace};");
            content.AppendLine();
        }

        if (options.GenerateXmlComments)
        {
            content.AppendLine("/// <summary>");
            content.AppendLine($"/// {config.ProjectName} - 主程序入口");
            content.AppendLine("/// </summary>");
        }

        content.AppendLine("public class Program");
        content.AppendLine("{");
        content.AppendLine($"    public static {(options.GenerateAsyncMethods ? "async Task" : "void")} Main(string[] args)");
        content.AppendLine("    {");
        content.AppendLine("        Console.WriteLine(\"=== FlowForge 工作流引擎 ===\"");
        content.AppendLine("            + Environment.NewLine);");
        content.AppendLine();
        content.AppendLine("        var services = new ServiceCollection();");
        content.AppendLine();

        if (options.GenerateLogging)
        {
            content.AppendLine("        services.AddLogging(builder =>");
            content.AppendLine("        {");
            content.AppendLine("            builder.AddConsole();");
            content.AppendLine("            builder.SetMinimumLevel(LogLevel.Information);");
            content.AppendLine("        });");
            content.AppendLine();
        }

        content.AppendLine($"        services.AddSingleton<{bpmnModel.Name}Workflow>();");
        content.AppendLine();
        content.AppendLine("        var serviceProvider = services.BuildServiceProvider();");
        content.AppendLine($"        var workflow = serviceProvider.GetRequiredService<{bpmnModel.Name}Workflow>();");
        content.AppendLine("        var context = new WorkflowContext();");
        content.AppendLine();

        if (options.GenerateAsyncMethods)
        {
            content.AppendLine("        var result = await workflow.ExecuteAsync(context);");
        }
        else
        {
            content.AppendLine("        var result = workflow.Execute(context);");
        }

        content.AppendLine();
        content.AppendLine("        Console.WriteLine($\"执行结果: {(result.Success ? \\\"成功\\\" : \\\"失败\\\")}\");");
        content.AppendLine("    }");
        content.AppendLine("}");

        if (!options.UseFileScoped)
        {
            content.AppendLine("}");
        }

        return new GeneratedFileDto
        {
            Path = "Program.cs",
            Content = content.ToString()
        };
    }

    private GeneratedFileDto GenerateWorkflowFile(GenerationRequestDto request, BpmnProcess bpmnModel)
    {
        var config = request.Config;
        var options = request.Options;
        var content = new StringBuilder();

        if (!config.ImplicitUsings)
        {
            content.AppendLine("using System;");
            content.AppendLine("using System.Threading.Tasks;");
            if (options.GenerateLogging)
            {
                content.AppendLine("using Microsoft.Extensions.Logging;");
            }
            content.AppendLine();
        }

        if (options.UseFileScoped)
        {
            content.AppendLine($"namespace {config.Namespace}.Workflows;");
            content.AppendLine();
        }

        if (options.GenerateXmlComments)
        {
            content.AppendLine("/// <summary>");
            content.AppendLine($"/// {bpmnModel.Name} - 工作流定义");
            content.AppendLine("/// </summary>");
        }

        content.AppendLine($"public class {bpmnModel.Name}Workflow");
        content.AppendLine("{");

        if (options.GenerateLogging)
        {
            content.AppendLine($"    private readonly ILogger<{bpmnModel.Name}Workflow> _logger;");
            content.AppendLine();
            content.AppendLine($"    public {bpmnModel.Name}Workflow(ILogger<{bpmnModel.Name}Workflow> logger)");
            content.AppendLine("    {");
            content.AppendLine("        _logger = logger;");
            content.AppendLine("    }");
            content.AppendLine();
        }

        var returnType = options.GenerateAsyncMethods ? "Task<ActivityResult>" : "ActivityResult";
        var asyncKeyword = options.GenerateAsyncMethods ? "async " : "";

        content.AppendLine($"    public {asyncKeyword}{returnType} ExecuteAsync(WorkflowContext context)");
        content.AppendLine("    {");

        if (options.GenerateLogging)
        {
            content.AppendLine("        _logger.LogInformation(\"开始执行工作流\");");
        }

        if (options.GenerateAsyncMethods)
        {
            content.AppendLine("        await Task.CompletedTask;");
        }

        content.AppendLine("        // TODO: 实现工作流逻辑");
        content.AppendLine("        return ActivityResult.Success(\"工作流执行成功\");");
        content.AppendLine("    }");
        content.AppendLine("}");

        if (!options.UseFileScoped)
        {
            content.AppendLine("}");
        }

        return new GeneratedFileDto
        {
            Path = $"Workflows/{bpmnModel.Name}Workflow.cs",
            Content = content.ToString()
        };
    }
    private List<GeneratedFileDto> GenerateActivityFiles(GenerationRequestDto request, BpmnProcess bpmnModel)
    {
        var files = new List<GeneratedFileDto>();
        var config = request.Config;
        var options = request.Options;
        foreach (var activity in bpmnModel.Activities)
        {
            var content = new StringBuilder(); if (!config.ImplicitUsings)
            {
                content.AppendLine("using System;");
                content.AppendLine("using System.Threading.Tasks;");
                if (options.GenerateLogging)
                {
                    content.AppendLine("using Microsoft.Extensions.Logging;");
                }
                content.AppendLine();
            }
            if (options.UseFileScoped)
            {
                content.AppendLine($"namespace {config.Namespace}.Activities;");
                content.AppendLine();
            }
            if (options.GenerateXmlComments)
            {
                content.AppendLine("/// <summary>");
                content.AppendLine($"/// {activity.Name} - 活动实现");
                content.AppendLine("/// </summary>");
            }
            var sanitizedName = SanitizeName(activity.Name);
            content.AppendLine($"public class {sanitizedName}Activity");
            content.AppendLine("{"); if (options.GenerateLogging)
            {
                content.AppendLine($"    private readonly ILogger<{sanitizedName}Activity> _logger;");
                content.AppendLine();
                content.AppendLine($"    public {sanitizedName}Activity(ILogger<{sanitizedName}Activity> logger)");
                content.AppendLine("    {");
                content.AppendLine("        _logger = logger;");
                content.AppendLine("    }");
                content.AppendLine();
            }
            var returnType = options.GenerateAsyncMethods ? "Task<ActivityResult>" : "ActivityResult";
            var asyncKeyword = options.GenerateAsyncMethods ? "async " : ""; content.AppendLine($"    public {asyncKeyword}{returnType} ExecuteAsync(WorkflowContext context)");
            content.AppendLine("    {");
            if (options.GenerateExceptionHandling)
            {
                content.AppendLine("        try");
                content.AppendLine("        {"); if (options.GenerateLogging)
                {
                    content.AppendLine($"            _logger.LogInformation(\"执行活动: {activity.Name}\");");
                }
                if (options.GenerateAsyncMethods)
                {
                    content.AppendLine("            await Task.Delay(100);");
                }
                content.AppendLine($"            // TODO: 实现 {activity.Name} 的业务逻辑");
                content.AppendLine("            return ActivityResult.Success(\"活动执行成功\");");
                content.AppendLine("        }");
                content.AppendLine("        catch (Exception ex)");
                content.AppendLine("        {"); if (options.GenerateLogging)
                {
                    content.AppendLine("            _logger.LogError(ex, \"活动执行异常\");");
                }
                content.AppendLine("            return ActivityResult.Failed($\"活动异常: {ex.Message}\");");
                content.AppendLine("        }");
            }
            else
            {
                if (options.GenerateLogging)
                {
                    content.AppendLine($"        _logger.LogInformation(\"执行活动: {activity.Name}\");");
                }
                if (options.GenerateAsyncMethods)
                {
                    content.AppendLine("        await Task.Delay(100);");
                }
                content.AppendLine($"        // TODO: 实现 {activity.Name} 的业务逻辑");
                content.AppendLine("        return ActivityResult.Success(\"活动执行成功\");");
            }
            content.AppendLine("    }");
            content.AppendLine("}"); if (!options.UseFileScoped)
            {
                content.AppendLine("}");
            }
            files.Add(new GeneratedFileDto
            {
                Path = $"Activities/{sanitizedName}Activity.cs",
                Content = content.ToString()
            });
        }
        return files;
    }
    private List<GeneratedFileDto> GenerateModelFiles(GenerationRequestDto request)
    {
        var files = new List<GeneratedFileDto>();
        var config = request.Config;
        var options = request.Options;    // WorkflowContext
        var contextContent = new StringBuilder();
        if (!config.ImplicitUsings)
        {
            contextContent.AppendLine("using System;");
            contextContent.AppendLine("using System.Collections.Generic;");
            contextContent.AppendLine();
        }
        if (options.UseFileScoped)
        {
            contextContent.AppendLine($"namespace {config.Namespace}.Models;");
            contextContent.AppendLine();
        }
        if (options.GenerateXmlComments)
        {
            contextContent.AppendLine("/// <summary>");
            contextContent.AppendLine("/// 工作流上下文");
            contextContent.AppendLine("/// </summary>");
        }
        contextContent.AppendLine("public class WorkflowContext");
        contextContent.AppendLine("{");
        contextContent.AppendLine("    public string Id { get; set; } = Guid.NewGuid().ToString();");
        contextContent.AppendLine("    public DateTime StartTime { get; set; } = DateTime.Now;");
        contextContent.AppendLine("    public Dictionary<string, object> Variables { get; set; } = new();");
        contextContent.AppendLine("    public List<string> ExecutionLog { get; set; } = new();");
        contextContent.AppendLine("}"); if (!options.UseFileScoped)
        {
            contextContent.AppendLine("}");
        }
        files.Add(new GeneratedFileDto
        {
            Path = "Models/WorkflowContext.cs",
            Content = contextContent.ToString()
        });
        // ActivityResult
        var resultContent = new StringBuilder();
        if (!config.ImplicitUsings)
        {
            resultContent.AppendLine("using System.Collections.Generic;");
            resultContent.AppendLine();
        }
        if (options.UseFileScoped)
        {
            resultContent.AppendLine($"namespace {config.Namespace}.Models;");
            resultContent.AppendLine();
        }
        if (options.GenerateXmlComments)
        {
            resultContent.AppendLine("/// <summary>");
            resultContent.AppendLine("/// 活动执行结果");
            resultContent.AppendLine("/// </summary>");
        }
        resultContent.AppendLine("public class ActivityResult");
        resultContent.AppendLine("{");
        resultContent.AppendLine("    public bool Success { get; set; }");
        resultContent.AppendLine("    public string Message { get; set; } = \"\";");
        resultContent.AppendLine("    public Dictionary<string, object> Data { get; set; } = new();");
        resultContent.AppendLine();
        resultContent.AppendLine("    public static ActivityResult Success(string message)"); if (options.UseExpressionBodies)
        {
            resultContent.AppendLine("        => new() { Success = true, Message = message };");
        }
        else
        {
            resultContent.AppendLine("    {");
            resultContent.AppendLine("        return new ActivityResult { Success = true, Message = message };");
            resultContent.AppendLine("    }");
        }
        resultContent.AppendLine();
        resultContent.AppendLine("    public static ActivityResult Failed(string message)"); if (options.UseExpressionBodies)
        {
            resultContent.AppendLine("        => new() { Success = false, Message = message };");
        }
        else
        {
            resultContent.AppendLine("    {");
            resultContent.AppendLine("        return new ActivityResult { Success = false, Message = message };");
            resultContent.AppendLine("    }");
        }
        resultContent.AppendLine("}"); if (!options.UseFileScoped)
        {
            resultContent.AppendLine("}");
        }
        files.Add(new GeneratedFileDto
        {
            Path = "Models/ActivityResult.cs",
            Content = resultContent.ToString()
        }); return files;
    }
    private List<GeneratedFileDto> GenerateConfigurationFiles()
    {
        var files = new List<GeneratedFileDto>();    // appsettings.json
        files.Add(new GeneratedFileDto
        {
            Path = "appsettings.json",
            Content = @"{
""Logging"": {
""LogLevel"": {
""Default"": ""Information"",
""Microsoft"": ""Warning""
}
},
""Workflow"": {
""MaxParallelTasks"": 10,
""TimeoutSeconds"": 300
}
}"
        });    // appsettings.Development.json
        files.Add(new GeneratedFileDto
        {
            Path = "appsettings.Development.json",
            Content = @"{
""Logging"": {
""LogLevel"": {
""Default"": ""Debug""
}
}
}"
        }); return files;
    }
    private string SanitizeName(string name)
    {
        // 移除特殊字符，只保留字母、数字和下划线
        var sanitized = new string(name.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());    // 确保以字母开头
        if (sanitized.Length > 0 && !char.IsLetter(sanitized[0]))
        {
            sanitized = "Activity_" + sanitized;
        }
        return string.IsNullOrEmpty(sanitized) ? "Activity" : sanitized;
    }
    #endregion
}