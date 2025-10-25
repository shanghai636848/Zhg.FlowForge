using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zhg.FlowForge.App.Shared.Services;


public class CodeGenerationService : ICodeGenerationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CodeGenerationService> _logger;
    private readonly IBpmnService _bpmnService;
    private readonly IProjectService _projectService;
    private readonly Dictionary<string, List<GenerationHistory>> _generationHistory = new();

    public CodeGenerationService(
        HttpClient httpClient,
        ILogger<CodeGenerationService> logger,
        IBpmnService bpmnService,
        IProjectService projectService)
    {
        _httpClient = httpClient;
        _logger = logger;
        _bpmnService = bpmnService;
        _projectService = projectService;
    }

    public async Task<GenerationResult> GenerateAsync(
        GenerationRequest request,
        IProgress<GenerationProgress>? progress = null)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new GenerationResult();

        try
        {
            _logger.LogInformation("开始生成代码，项目: {ProjectName}", request.Config.ProjectName);

            // 1. 验证配置
            progress?.Report(new GenerationProgress { Percentage = 5, Message = "验证配置..." });
            var validation = await ValidateConfigurationAsync(request);
            if (!validation.IsValid)
            {
                result.Success = false;
                result.Error = string.Join(", ", validation.Errors.Select(e => e.Message));
                return result;
            }

            // 2. 加载 BPMN 模型
            progress?.Report(new GenerationProgress { Percentage = 10, Message = "加载 BPMN 模型..." });
            var bpmnModel = await _bpmnService.GetProcessAsync(request.ProcessId);
            if (bpmnModel == null)
            {
                result.Success = false;
                result.Error = "BPMN 流程不存在";
                return result;
            }

            // 3. 加载模板
            progress?.Report(new GenerationProgress { Percentage = 15, Message = "加载代码模板..." });
            var template = await GetTemplateAsync(request.Template);
            if (template == null)
            {
                result.Success = false;
                result.Error = "模板不存在";
                return result;
            }

            // 4. 生成项目文件
            progress?.Report(new GenerationProgress { Percentage = 20, Message = "生成项目文件..." });
            await Task.Delay(300);
            var projectFile = GenerateProjectFile(request, bpmnModel);
            result.Files.Add(projectFile);

            // 5. 生成主程序
            progress?.Report(new GenerationProgress { Percentage = 30, Message = "生成主程序...", CurrentFile = "Program.cs" });
            await Task.Delay(300);
            var programFile = GenerateProgramFile(request, bpmnModel);
            result.Files.Add(programFile);

            // 6. 生成工作流类
            progress?.Report(new GenerationProgress { Percentage = 40, Message = "生成工作流定义...", CurrentFile = "Workflows/" });
            await Task.Delay(400);
            var workflowFiles = await GenerateWorkflowFilesAsync(request, bpmnModel);
            result.Files.AddRange(workflowFiles);

            // 7. 生成活动类
            progress?.Report(new GenerationProgress { Percentage = 55, Message = "生成活动类...", CurrentFile = "Activities/" });
            await Task.Delay(400);
            var activityFiles = await GenerateActivityFilesAsync(request, bpmnModel);
            result.Files.AddRange(activityFiles);

            // 8. 生成模型类
            progress?.Report(new GenerationProgress { Percentage = 70, Message = "生成数据模型...", CurrentFile = "Models/" });
            await Task.Delay(300);
            var modelFiles = GenerateModelFiles(request, bpmnModel);
            result.Files.AddRange(modelFiles);

            // 9. 生成服务类
            progress?.Report(new GenerationProgress { Percentage = 80, Message = "生成服务类...", CurrentFile = "Services/" });
            await Task.Delay(300);
            var serviceFiles = GenerateServiceFiles(request, bpmnModel);
            result.Files.AddRange(serviceFiles);

            // 10. 生成配置文件
            progress?.Report(new GenerationProgress { Percentage = 90, Message = "生成配置文件...", CurrentFile = "appsettings.json" });
            await Task.Delay(200);
            var configFiles = GenerateConfigurationFiles(request);
            result.Files.AddRange(configFiles);

            // 11. 保存到项目
            progress?.Report(new GenerationProgress { Percentage = 95, Message = "保存文件..." });
            await SaveToProjectAsync(request, result.Files);

            // 完成
            progress?.Report(new GenerationProgress { Percentage = 100, Message = "生成完成!" });

            result.Success = true;
            result.TotalLines = result.Files.Sum(f => f.LineCount);
            result.ProjectPath = $"/projects/{request.Config.ProjectName}";

            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;

            // 记录历史
            AddGenerationHistory(request, result);

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

    public async Task<List<GeneratedFile>> PreviewAsync(GenerationRequest request)
    {
        var files = new List<GeneratedFile>();

        try
        {
            var bpmnModel = await _bpmnService.GetProcessAsync(request.ProcessId);
            if (bpmnModel == null) return files;

            // 生成预览文件（只生成主要文件）
            files.Add(GenerateProjectFile(request, bpmnModel));
            files.Add(GenerateProgramFile(request, bpmnModel));

            var workflowFiles = await GenerateWorkflowFilesAsync(request, bpmnModel);
            files.Add(workflowFiles.FirstOrDefault() ?? new GeneratedFile());

            return files;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "预览生成失败");
            return files;
        }
    }

    public async Task<ValidationResult> ValidateConfigurationAsync(GenerationRequest request)
    {
        await Task.Delay(50);

        var result = new ValidationResult { IsValid = true };

        // 验证项目名称
        if (string.IsNullOrWhiteSpace(request.Config.ProjectName))
        {
            result.IsValid = false;
            result.Errors.Add(new ValidationError
            {
                Field = "ProjectName",
                Message = "项目名称不能为空"
            });
        }

        // 验证命名空间
        if (string.IsNullOrWhiteSpace(request.Config.Namespace))
        {
            result.IsValid = false;
            result.Errors.Add(new ValidationError
            {
                Field = "Namespace",
                Message = "命名空间不能为空"
            });
        }

        // 验证流程 ID
        if (string.IsNullOrWhiteSpace(request.ProcessId))
        {
            result.IsValid = false;
            result.Errors.Add(new ValidationError
            {
                Field = "ProcessId",
                Message = "流程 ID 不能为空"
            });
        }

        // 警告：AOT 优化
        if (request.Options.EnableAotOptimizations)
        {
            result.Warnings.Add(new ValidationWarning
            {
                Field = "EnableAotOptimizations",
                Message = "AOT 优化可能会限制某些反射功能"
            });
        }

        return result;
    }

    public async Task<List<CodeTemplate>> GetTemplatesAsync()
    {
        await Task.Delay(100);

        return new List<CodeTemplate>
        {
            new CodeTemplate
            {
                Id = "standard",
                Name = "标准模板",
                Description = "适用于大多数业务流程，包含完整的工作流引擎",
                Category = "通用",
                Author = "FlowForge Team",
                Version = "1.0.0",
                Tags = new List<string> { "推荐", "通用", "完整" },
                Features = new List<string>
                {
                    "完整工作流引擎",
                    "依赖注入",
                    "异步支持",
                    "日志记录",
                    "异常处理"
                },
                IconClass = "fas fa-file-code"
            },
            new CodeTemplate
            {
                Id = "minimal",
                Name = "最小模板",
                Description = "轻量级实现，适合简单流程",
                Category = "轻量",
                Author = "FlowForge Team",
                Version = "1.0.0",
                Tags = new List<string> { "轻量", "快速", "简单" },
                Features = new List<string>
                {
                    "无外部依赖",
                    "快速启动",
                    "易于理解",
                    "最小化代码"
                },
                IconClass = "fas fa-feather"
            },
            new CodeTemplate
            {
                Id = "microservice",
                Name = "微服务模板",
                Description = "容器化微服务架构，支持 Kubernetes 部署",
                Category = "云原生",
                Author = "FlowForge Team",
                Version = "1.0.0",
                Tags = new List<string> { "云原生", "K8s", "Docker" },
                Features = new List<string>
                {
                    "Docker 支持",
                    "健康检查",
                    "指标监控",
                    "配置中心",
                    "服务发现"
                },
                IconClass = "fas fa-cubes"
            },
            new CodeTemplate
            {
                Id = "serverless",
                Name = "无服务器模板",
                Description = "AWS Lambda / Azure Functions 部署",
                Category = "Serverless",
                Author = "FlowForge Team",
                Version = "1.0.0",
                Tags = new List<string> { "Serverless", "云函数" },
                Features = new List<string>
                {
                    "按需扩展",
                    "低成本",
                    "事件驱动",
                    "自动部署"
                },
                IconClass = "fas fa-cloud"
            },
            new CodeTemplate
            {
                Id = "enterprise",
                Name = "企业级模板",
                Description = "包含完整的企业级特性和最佳实践",
                Category = "企业",
                Author = "FlowForge Team",
                Version = "1.0.0",
                Tags = new List<string> { "企业", "完整", "安全" },
                Features = new List<string>
                {
                    "权限控制",
                    "审计日志",
                    "多租户",
                    "高可用",
                    "数据加密"
                },
                IconClass = "fas fa-building"
            },
            new CodeTemplate
            {
                Id = "api",
                Name = "REST API 模板",
                Description = "生成 RESTful API 服务",
                Category = "Web",
                Author = "FlowForge Team",
                Version = "1.0.0",
                Tags = new List<string> { "API", "Web", "REST" },
                Features = new List<string>
                {
                    "Swagger 文档",
                    "版本控制",
                    "限流",
                    "CORS",
                    "身份验证"
                },
                IconClass = "fas fa-server"
            }
        };
    }

    public async Task<CodeTemplate?> GetTemplateAsync(string templateId)
    {
        var templates = await GetTemplatesAsync();
        return templates.FirstOrDefault(t => t.Id == templateId);
    }

    public async Task<CodeTemplate> CreateCustomTemplateAsync(CreateTemplateRequest request)
    {
        await Task.Delay(200);

        var template = new CodeTemplate
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            Description = request.Description,
            Category = "自定义",
            Author = "Custom User",
            Version = "1.0.0",
            Tags = new List<string> { "自定义" },
            Features = new List<string>(),
            IconClass = "fas fa-magic",
            IsCustom = true
        };

        _logger.LogInformation("创建自定义模板: {TemplateName}", template.Name);

        return template;
    }

    public async Task<List<GenerationHistory>> GetGenerationHistoryAsync(string projectId)
    {
        await Task.CompletedTask;

        if (_generationHistory.TryGetValue(projectId, out var history))
        {
            return history.OrderByDescending(h => h.Timestamp).ToList();
        }

        return new List<GenerationHistory>();
    }

    public async Task<GenerationResult> RegenerateAsync(string historyId)
    {
        // 从历史记录中恢复配置并重新生成
        await Task.Delay(100);

        throw new NotImplementedException("重新生成功能待实现");
    }

    // 私有代码生成方法

    private GeneratedFile GenerateProjectFile(GenerationRequest request, BpmnProcess bpmnModel)
    {
        var config = request.Config;
        var options = request.Options;

        var content = new StringBuilder();
        content.AppendLine("<Project Sdk=\"Microsoft.NET.Sdk\">");
        content.AppendLine();
        content.AppendLine("  <PropertyGroup>");
        content.AppendLine($"    <TargetFramework>{config.TargetFramework}</TargetFramework>");
        content.AppendLine($"    <Nullable>{(config.EnableNullable ? "enable" : "disable")}</Nullable>");
        content.AppendLine($"    <ImplicitUsings>{(config.ImplicitUsings ? "enable" : "disable")}</ImplicitUsings>");
        content.AppendLine($"    <Version>{config.Version}</Version>");

        if (!string.IsNullOrEmpty(config.Author))
            content.AppendLine($"    <Authors>{config.Author}</Authors>");

        if (!string.IsNullOrEmpty(config.Company))
            content.AppendLine($"    <Company>{config.Company}</Company>");

        if (!string.IsNullOrEmpty(config.Copyright))
            content.AppendLine($"    <Copyright>{config.Copyright}</Copyright>");

        if (options.EnableAotOptimizations)
        {
            content.AppendLine("    <PublishAot>true</PublishAot>");
            content.AppendLine("    <InvariantGlobalization>true</InvariantGlobalization>");
        }

        if (options.EnableTrimming)
        {
            content.AppendLine("    <PublishTrimmed>true</PublishTrimmed>");
            content.AppendLine("    <TrimMode>full</TrimMode>");
        }

        content.AppendLine("  </PropertyGroup>");
        content.AppendLine();

        // 添加依赖包
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

        return new GeneratedFile
        {
            Path = $"{config.ProjectName}.csproj",
            Content = content.ToString()
        };
    }

    private GeneratedFile GenerateProgramFile(GenerationRequest request, BpmnProcess bpmnModel)
    {
        var config = request.Config;
        var options = request.Options;
        var useFileScoped = options.UseFileScoped;

        var content = new StringBuilder();

        // Using 语句
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

        // 命名空间
        if (useFileScoped)
        {
            content.AppendLine($"namespace {config.Namespace};");
            content.AppendLine();
        }
        else
        {
            content.AppendLine($"namespace {config.Namespace}");
            content.AppendLine("{");
        }

        // 类定义
        var indent = useFileScoped ? "" : "    ";

        if (options.GenerateXmlComments)
        {
            content.AppendLine($"{indent}/// <summary>");
            content.AppendLine($"{indent}/// {config.ProjectName} - 主程序入口");
            content.AppendLine($"{indent}/// </summary>");
        }

        content.AppendLine($"{indent}public class Program");
        content.AppendLine($"{indent}{{");

        // Main 方法
        if (options.GenerateXmlComments)
        {
            content.AppendLine($"{indent}    /// <summary>");
            content.AppendLine($"{indent}    /// 程序入口点");
            content.AppendLine($"{indent}    /// </summary>");
        }

        var asyncKeyword = options.GenerateAsyncMethods ? "async " : "";
        var returnType = options.GenerateAsyncMethods ? "Task" : "void";

        content.AppendLine($"{indent}    public static {asyncKeyword}{returnType} Main(string[] args)");
        content.AppendLine($"{indent}    {{");

        // 服务配置
        content.AppendLine($"{indent}        var services = new ServiceCollection();");
        content.AppendLine();

        if (options.GenerateLogging)
        {
            content.AppendLine($"{indent}        // 配置日志");
            content.AppendLine($"{indent}        services.AddLogging(builder =>");
            content.AppendLine($"{indent}        {{");
            content.AppendLine($"{indent}            builder.AddConsole();");
            content.AppendLine($"{indent}            builder.SetMinimumLevel(LogLevel.Information);");
            content.AppendLine($"{indent}        }});");
            content.AppendLine();
        }

        content.AppendLine($"{indent}        // 注册服务");
        content.AppendLine($"{indent}        services.AddSingleton<IWorkflowEngine, WorkflowEngine>();");
        content.AppendLine($"{indent}        services.AddSingleton<{bpmnModel.Name}Workflow>();");
        content.AppendLine();

        content.AppendLine($"{indent}        var serviceProvider = services.BuildServiceProvider();");
        content.AppendLine();

        // 执行工作流
        content.AppendLine($"{indent}        // 执行工作流");
        content.AppendLine($"{indent}        var engine = serviceProvider.GetRequiredService<IWorkflowEngine>();");
        content.AppendLine($"{indent}        var context = new WorkflowContext();");
        content.AppendLine();

        if (options.GenerateAsyncMethods)
        {
            var awaitConfig = options.UseConfigureAwait ? ".ConfigureAwait(false)" : "";
            content.AppendLine($"{indent}        await engine.ExecuteAsync(context){awaitConfig};");
        }
        else
        {
            content.AppendLine($"{indent}        engine.Execute(context);");
        }
        content.AppendLine();

        content.AppendLine($"{indent}        Console.WriteLine(\"工作流执行完成\");");

        content.AppendLine($"{indent}    }}");
        content.AppendLine($"{indent}}}");

        if (!useFileScoped)
        {
            content.AppendLine("}");
        }

        return new GeneratedFile
        {
            Path = "Program.cs",
            Content = content.ToString()
        };
    }

    private async Task<List<GeneratedFile>> GenerateWorkflowFilesAsync(GenerationRequest request, BpmnProcess bpmnModel)
    {
        await Task.Delay(200);

        var files = new List<GeneratedFile>();
        var config = request.Config;
        var options = request.Options;

        var workflowFile = new GeneratedFile
        {
            Path = $"Workflows/{bpmnModel.Name}Workflow.cs",
            Content = GenerateWorkflowClass(config, options, bpmnModel)
        };

        files.Add(workflowFile);

        return files;
    }

    private string GenerateWorkflowClass(ProjectConfig config, CodeGenerationOptions options, BpmnProcess bpmnModel)
    {
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
        else
        {
            content.AppendLine($"namespace {config.Namespace}.Workflows");
            content.AppendLine("{");
        }

        var indent = options.UseFileScoped ? "" : "    ";

        if (options.GenerateXmlComments)
        {
            content.AppendLine($"{indent}/// <summary>");
            content.AppendLine($"{indent}/// {bpmnModel.Name} - 工作流定义");
            content.AppendLine($"{indent}/// {config.Description}");
            content.AppendLine($"{indent}/// </summary>");
        }
        content.AppendLine($"{indent}public class {bpmnModel.Name}Workflow");
        content.AppendLine($"{indent}{{");

        // 字段
        if (options.GenerateLogging)
        {
            content.AppendLine($"{indent}    private readonly ILogger<{bpmnModel.Name}Workflow> _logger;");
            content.AppendLine();
        }

        // 构造函数
        if (options.GenerateXmlComments)
        {
            content.AppendLine($"{indent}    /// <summary>");
            content.AppendLine($"{indent}    /// 初始化工作流");
            content.AppendLine($"{indent}    /// </summary>");
        }

        content.Append($"{indent}    public {bpmnModel.Name}Workflow(");
        if (options.GenerateLogging)
        {
            content.Append($"ILogger<{bpmnModel.Name}Workflow> logger");
        }
        content.AppendLine(")");
        content.AppendLine($"{indent}    {{");
        if (options.GenerateLogging)
        {
            content.AppendLine($"{indent}        _logger = logger;");
        }
        content.AppendLine($"{indent}    }}");
        content.AppendLine();

        // Execute 方法
        if (options.GenerateXmlComments)
        {
            content.AppendLine($"{indent}    /// <summary>");
            content.AppendLine($"{indent}    /// 执行工作流");
            content.AppendLine($"{indent}    /// </summary>");
            content.AppendLine($"{indent}    /// <param name=\"context\">工作流上下文</param>");
            content.AppendLine($"{indent}    /// <returns>执行结果</returns>");
        }

        var asyncKeyword = options.GenerateAsyncMethods ? "async " : "";
        var returnType = options.GenerateAsyncMethods
            ? (options.UseValueTask ? "ValueTask<ActivityResult>" : "Task<ActivityResult>")
            : "ActivityResult";

        content.AppendLine($"{indent}    public {asyncKeyword}{returnType} ExecuteAsync(WorkflowContext context)");
        content.AppendLine($"{indent}    {{");

        if (options.GenerateLogging)
        {
            content.AppendLine($"{indent}        _logger.LogInformation(\"开始执行工作流: {{WorkflowName}}\", \"{bpmnModel.Name}\");");
            content.AppendLine();
        }

        if (options.GenerateExceptionHandling)
        {
            content.AppendLine($"{indent}        try");
            content.AppendLine($"{indent}        {{");
        }

        var execIndent = options.GenerateExceptionHandling ? $"{indent}            " : $"{indent}        ";

        // 按顺序执行活动
        foreach (var activity in bpmnModel.Activities)
        {
            content.AppendLine($"{execIndent}// {activity.Name}");

            var activityVar = $"{ToCamelCase(activity.Name)}Result";
            var awaitKeyword = options.GenerateAsyncMethods ? "await " : "";
            var awaitConfig = options.UseConfigureAwait && options.GenerateAsyncMethods ? ".ConfigureAwait(false)" : "";

            content.AppendLine($"{execIndent}var {activityVar} = {awaitKeyword}Execute{activity.Name}Async(context){awaitConfig};");

            content.AppendLine($"{execIndent}if (!{activityVar}.Success)");
            content.AppendLine($"{execIndent}{{");

            if (options.GenerateLogging)
            {
                content.AppendLine($"{execIndent}    _logger.LogError(\"{activity.Name} 执行失败\");");
            }

            content.AppendLine($"{execIndent}    return ActivityResult.Failed(\"{activity.Name} 执行失败\");");
            content.AppendLine($"{execIndent}}}");
            content.AppendLine();
        }

        if (options.GenerateLogging)
        {
            content.AppendLine($"{execIndent}_logger.LogInformation(\"工作流执行完成: {{WorkflowName}}\", \"{bpmnModel.Name}\");");
        }

        content.AppendLine($"{execIndent}return ActivityResult.Success(\"工作流执行成功\");");

        if (options.GenerateExceptionHandling)
        {
            content.AppendLine($"{indent}        }}");
            content.AppendLine($"{indent}        catch (Exception ex)");
            content.AppendLine($"{indent}        {{");

            if (options.GenerateLogging)
            {
                content.AppendLine($"{indent}            _logger.LogError(ex, \"工作流执行异常\");");
            }

            content.AppendLine($"{indent}            return ActivityResult.Failed($\"工作流异常: {{ex.Message}}\");");
            content.AppendLine($"{indent}        }}");
        }

        content.AppendLine($"{indent}    }}");
        content.AppendLine();

        // 生成各个活动的执行方法
        foreach (var activity in bpmnModel.Activities)
        {
            content.AppendLine(GenerateActivityMethod(indent, options, activity));
        }

        content.AppendLine($"{indent}}}");

        if (!options.UseFileScoped)
        {
            content.AppendLine("}");
        }

        return content.ToString();
    }

    private string GenerateActivityMethod(string indent, CodeGenerationOptions options, BpmnActivity activity)
    {
        var sb = new StringBuilder();

        if (options.GenerateXmlComments)
        {
            sb.AppendLine($"{indent}    /// <summary>");
            sb.AppendLine($"{indent}    /// 执行 {activity.Name}");
            sb.AppendLine($"{indent}    /// </summary>");
        }

        var asyncKeyword = options.GenerateAsyncMethods ? "async " : "";
        var returnType = options.GenerateAsyncMethods
            ? (options.UseValueTask ? "ValueTask<ActivityResult>" : "Task<ActivityResult>")
            : "ActivityResult";

        sb.AppendLine($"{indent}    private {asyncKeyword}{returnType} Execute{activity.Name}Async(WorkflowContext context)");
        sb.AppendLine($"{indent}    {{");

        if (options.GenerateLogging)
        {
            sb.AppendLine($"{indent}        _logger.LogDebug(\"执行活动: {activity.Name}\");");
        }

        if (options.GenerateAsyncMethods)
        {
            sb.AppendLine($"{indent}        await Task.Delay(100);");
        }

        sb.AppendLine($"{indent}        // TODO: 实现 {activity.Name} 的业务逻辑");
        sb.AppendLine($"{indent}        return ActivityResult.Success(\"{activity.Name} 完成\");");
        sb.AppendLine($"{indent}    }}");
        sb.AppendLine();

        return sb.ToString();
    }

    private async Task<List<GeneratedFile>> GenerateActivityFilesAsync(GenerationRequest request, BpmnProcess bpmnModel)
    {
        await Task.Delay(200);

        var files = new List<GeneratedFile>();
        var config = request.Config;
        var options = request.Options;

        foreach (var activity in bpmnModel.Activities)
        {
            var activityFile = new GeneratedFile
            {
                Path = $"Activities/{activity.Name}Activity.cs",
                Content = GenerateActivityClass(config, options, activity)
            };
            files.Add(activityFile);
        }

        return files;
    }

    private string GenerateActivityClass(ProjectConfig config, CodeGenerationOptions options, BpmnActivity activity)
    {
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
            content.AppendLine($"namespace {config.Namespace}.Activities;");
            content.AppendLine();
        }
        else
        {
            content.AppendLine($"namespace {config.Namespace}.Activities");
            content.AppendLine("{");
        }

        var indent = options.UseFileScoped ? "" : "    ";

        if (options.GenerateXmlComments)
        {
            content.AppendLine($"{indent}/// <summary>");
            content.AppendLine($"{indent}/// {activity.Name} - 活动实现");
            content.AppendLine($"{indent}/// </summary>");
        }

        content.AppendLine($"{indent}public class {activity.Name}Activity");
        content.AppendLine($"{indent}{{");

        if (options.GenerateLogging)
        {
            content.AppendLine($"{indent}    private readonly ILogger<{activity.Name}Activity> _logger;");
            content.AppendLine();
            content.AppendLine($"{indent}    public {activity.Name}Activity(ILogger<{activity.Name}Activity> logger)");
            content.AppendLine($"{indent}    {{");
            content.AppendLine($"{indent}        _logger = logger;");
            content.AppendLine($"{indent}    }}");
            content.AppendLine();
        }

        if (options.GenerateXmlComments)
        {
            content.AppendLine($"{indent}    /// <summary>");
            content.AppendLine($"{indent}    /// 执行活动");
            content.AppendLine($"{indent}    /// </summary>");
        }

        var asyncKeyword = options.GenerateAsyncMethods ? "async " : "";
        var returnType = options.GenerateAsyncMethods ? "Task<ActivityResult>" : "ActivityResult";

        content.AppendLine($"{indent}    public {asyncKeyword}{returnType} ExecuteAsync(WorkflowContext context)");
        content.AppendLine($"{indent}    {{");

        if (options.GenerateLogging)
        {
            content.AppendLine($"{indent}        _logger.LogInformation(\"执行活动: {{ActivityName}}\", \"{activity.Name}\");");
            content.AppendLine();
        }

        if (options.GenerateExceptionHandling)
        {
            content.AppendLine($"{indent}        try");
            content.AppendLine($"{indent}        {{");
            content.AppendLine($"{indent}            // TODO: 实现活动逻辑");

            if (options.GenerateAsyncMethods)
            {
                content.AppendLine($"{indent}            await Task.Delay(100);");
            }

            content.AppendLine();
            content.AppendLine($"{indent}            return ActivityResult.Success(\"活动执行成功\");");
            content.AppendLine($"{indent}        }}");
            content.AppendLine($"{indent}        catch (Exception ex)");
            content.AppendLine($"{indent}        {{");

            if (options.GenerateLogging)
            {
                content.AppendLine($"{indent}            _logger.LogError(ex, \"活动执行异常\");");
            }

            content.AppendLine($"{indent}            return ActivityResult.Failed($\"活动异常: {{ex.Message}}\");");
            content.AppendLine($"{indent}        }}");
        }
        else
        {
            content.AppendLine($"{indent}        // TODO: 实现活动逻辑");

            if (options.GenerateAsyncMethods)
            {
                content.AppendLine($"{indent}        await Task.Delay(100);");
            }

            content.AppendLine();
            content.AppendLine($"{indent}        return ActivityResult.Success(\"活动执行成功\");");
        }

        content.AppendLine($"{indent}    }}");
        content.AppendLine($"{indent}}}");

        if (!options.UseFileScoped)
        {
            content.AppendLine("}");
        }

        return content.ToString();
    }

    private List<GeneratedFile> GenerateModelFiles(GenerationRequest request, BpmnProcess bpmnModel)
    {
        var files = new List<GeneratedFile>();
        var config = request.Config;
        var options = request.Options;

        // WorkflowContext
        files.Add(new GeneratedFile
        {
            Path = "Models/WorkflowContext.cs",
            Content = GenerateModelClass(config, options, "WorkflowContext", new[]
            {
            ("string", "Id", "Guid.NewGuid().ToString()"),
            ("DateTime", "StartTime", "DateTime.Now"),
            ("Dictionary<string, object>", "Variables", "new()"),
            ("List<string>", "ExecutionLog", "new()")
        })
        });

        // ActivityResult
        files.Add(new GeneratedFile
        {
            Path = "Models/ActivityResult.cs",
            Content = GenerateResultClass(config, options)
        });

        return files;
    }

    private string GenerateModelClass(ProjectConfig config, CodeGenerationOptions options, string className, (string Type, string Name, string DefaultValue)[] properties)
    {
        var content = new StringBuilder();

        if (!config.ImplicitUsings)
        {
            content.AppendLine("using System;");
            content.AppendLine("using System.Collections.Generic;");
            content.AppendLine();
        }

        if (options.UseFileScoped)
        {
            content.AppendLine($"namespace {config.Namespace}.Models;");
            content.AppendLine();
        }
        else
        {
            content.AppendLine($"namespace {config.Namespace}.Models");
            content.AppendLine("{");
        }

        var indent = options.UseFileScoped ? "" : "    ";

        if (options.GenerateXmlComments)
        {
            content.AppendLine($"{indent}/// <summary>");
            content.AppendLine($"{indent}/// {className} - 数据模型");
            content.AppendLine($"{indent}/// </summary>");
        }

        content.AppendLine($"{indent}public class {className}");
        content.AppendLine($"{indent}{{");

        foreach (var (type, name, defaultValue) in properties)
        {
            if (options.GenerateXmlComments)
            {
                content.AppendLine($"{indent}    /// <summary>");
                content.AppendLine($"{indent}    /// {name}");
                content.AppendLine($"{indent}    /// </summary>");
            }

            content.AppendLine($"{indent}    public {type} {name} {{ get; set; }} = {defaultValue};");
            content.AppendLine();
        }

        content.AppendLine($"{indent}}}");

        if (!options.UseFileScoped)
        {
            content.AppendLine("}");
        }

        return content.ToString();
    }

    private string GenerateResultClass(ProjectConfig config, CodeGenerationOptions options)
    {
        var content = new StringBuilder();

        if (!config.ImplicitUsings)
        {
            content.AppendLine("using System.Collections.Generic;");
            content.AppendLine();
        }

        if (options.UseFileScoped)
        {
            content.AppendLine($"namespace {config.Namespace}.Models;");
            content.AppendLine();
        }
        else
        {
            content.AppendLine($"namespace {config.Namespace}.Models");
            content.AppendLine("{");
        }

        var indent = options.UseFileScoped ? "" : "    ";

        if (options.GenerateXmlComments)
        {
            content.AppendLine($"{indent}/// <summary>");
            content.AppendLine($"{indent}/// ActivityResult - 活动执行结果");
            content.AppendLine($"{indent}/// </summary>");
        }

        content.AppendLine($"{indent}public class ActivityResult");
        content.AppendLine($"{indent}{{");
        content.AppendLine($"{indent}    public bool Success {{ get; set; }}");
        content.AppendLine($"{indent}    public string Message {{ get; set; }} = \"\";");
        content.AppendLine($"{indent}    public Dictionary<string, object> Data {{ get; set; }} = new();");
        content.AppendLine();
        content.AppendLine($"{indent}    public static ActivityResult Success(string message)");

        if (options.UseExpressionBodies)
        {
            content.AppendLine($"{indent}        => new() {{ Success = true, Message = message }};");
        }
        else
        {
            content.AppendLine($"{indent}    {{");
            content.AppendLine($"{indent}        return new ActivityResult");
            content.AppendLine($"{indent}        {{");
            content.AppendLine($"{indent}            Success = true,");
            content.AppendLine($"{indent}            Message = message");
            content.AppendLine($"{indent}        }};");
            content.AppendLine($"{indent}    }}");
        }

        content.AppendLine();
        content.AppendLine($"{indent}    public static ActivityResult Failed(string message)");

        if (options.UseExpressionBodies)
        {
            content.AppendLine($"{indent}        => new() {{ Success = false, Message = message }};");
        }
        else
        {
            content.AppendLine($"{indent}    {{");
            content.AppendLine($"{indent}        return new ActivityResult");
            content.AppendLine($"{indent}        {{");
            content.AppendLine($"{indent}            Success = false,");
            content.AppendLine($"{indent}            Message = message");
            content.AppendLine($"{indent}        }};");
            content.AppendLine($"{indent}    }}");
        }

        content.AppendLine($"{indent}}}");

        if (!options.UseFileScoped)
        {
            content.AppendLine("}");
        }

        return content.ToString();
    }

    private List<GeneratedFile> GenerateServiceFiles(GenerationRequest request, BpmnProcess bpmnModel)
    {
        var files = new List<GeneratedFile>();
        var config = request.Config;
        var options = request.Options;

        // Interface
        files.Add(new GeneratedFile
        {
            Path = "Services/IWorkflowEngine.cs",
            Content = GenerateServiceInterface(config, options)
        });

        // Implementation
        files.Add(new GeneratedFile
        {
            Path = "Services/WorkflowEngine.cs",
            Content = GenerateServiceImplementation(config, options, bpmnModel)
        });

        return files;
    }

    private string GenerateServiceInterface(ProjectConfig config, CodeGenerationOptions options)
    {
        var content = new StringBuilder();

        if (!config.ImplicitUsings)
        {
            content.AppendLine("using System.Threading.Tasks;");
            content.AppendLine();
        }

        if (options.UseFileScoped)
        {
            content.AppendLine($"namespace {config.Namespace}.Services;");
            content.AppendLine();
        }
        else
        {
            content.AppendLine($"namespace {config.Namespace}.Services");
            content.AppendLine("{");
        }

        var indent = options.UseFileScoped ? "" : "    ";

        if (options.GenerateXmlComments)
        {
            content.AppendLine($"{indent}/// <summary>");
            content.AppendLine($"{indent}/// IWorkflowEngine - 工作流引擎接口");
            content.AppendLine($"{indent}/// </summary>");
        }

        content.AppendLine($"{indent}public interface IWorkflowEngine");
        content.AppendLine($"{indent}{{");

        if (options.GenerateXmlComments)
        {
            content.AppendLine($"{indent}    /// <summary>");
            content.AppendLine($"{indent}    /// 执行工作流");
            content.AppendLine($"{indent}    /// </summary>");
        }

        var returnType = options.GenerateAsyncMethods ? "Task<ActivityResult>" : "ActivityResult";
        content.AppendLine($"{indent}    {returnType} ExecuteAsync(WorkflowContext context);");

        content.AppendLine($"{indent}}}");

        if (!options.UseFileScoped)
        {
            content.AppendLine("}");
        }

        return content.ToString();
    }

    private string GenerateServiceImplementation(ProjectConfig config, CodeGenerationOptions options, BpmnProcess bpmnModel)
    {
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
            content.AppendLine($"namespace {config.Namespace}.Services;");
            content.AppendLine();
        }
        else
        {
            content.AppendLine($"namespace {config.Namespace}.Services");
            content.AppendLine("{");
        }

        var indent = options.UseFileScoped ? "" : "    ";

        if (options.GenerateXmlComments)
        {
            content.AppendLine($"{indent}/// <summary>");
            content.AppendLine($"{indent}/// WorkflowEngine - 工作流引擎实现");
            content.AppendLine($"{indent}/// </summary>");
        }

        content.AppendLine($"{indent}public class WorkflowEngine : IWorkflowEngine");
        content.AppendLine($"{indent}{{");

        if (options.GenerateLogging)
        {
            content.AppendLine($"{indent}    private readonly ILogger<WorkflowEngine> _logger;");
            content.AppendLine();
            content.AppendLine($"{indent}    public WorkflowEngine(ILogger<WorkflowEngine> logger)");
            content.AppendLine($"{indent}    {{");
            content.AppendLine($"{indent}        _logger = logger;");
            content.AppendLine($"{indent}    }}");
            content.AppendLine();
        }

        var asyncKeyword = options.GenerateAsyncMethods ? "async " : "";
        var returnType = options.GenerateAsyncMethods ? "Task<ActivityResult>" : "ActivityResult";

        content.AppendLine($"{indent}    public {asyncKeyword}{returnType} ExecuteAsync(WorkflowContext context)");
        content.AppendLine($"{indent}    {{");

        if (options.GenerateLogging)
        {
            content.AppendLine($"{indent}        _logger.LogInformation(\"开始执行工作流\");");
        }

        content.AppendLine($"{indent}        // TODO: 实现工作流引擎逻辑");

        if (options.GenerateAsyncMethods)
        {
            content.AppendLine($"{indent}        await Task.CompletedTask;");
        }

        content.AppendLine($"{indent}        return ActivityResult.Success(\"执行完成\");");
        content.AppendLine($"{indent}    }}");
        content.AppendLine($"{indent}}}");

        if (!options.UseFileScoped)
        {
            content.AppendLine("}");
        }

        return content.ToString();
    }

    private List<GeneratedFile> GenerateConfigurationFiles(GenerationRequest request)
    {
        var files = new List<GeneratedFile>();

        files.Add(new GeneratedFile
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
        });
        files.Add(new GeneratedFile
        {
            Path = "appsettings.Development.json",
            Content = @"{
""Logging"": {
""LogLevel"": {
""Default"": ""Debug""
}
}
}"
        });
        return files;
    }

    private async Task SaveToProjectAsync(GenerationRequest request, List<GeneratedFile> files)
    {
        // 创建项目
        var project = await _projectService.CreateProjectAsync(new CreateProjectRequest
        {
            Name = request.Config.ProjectName,
            Description = request.Config.Description,
            Namespace = request.Config.Namespace,
            TargetFramework = request.Config.TargetFramework,
            Template = request.Template
        });

        // 保存文件
        foreach (var file in files)
        {
            await _projectService.SaveFileAsync(project.Id, file.Path, file.Content);
        }
    }

    private void AddGenerationHistory(GenerationRequest request, GenerationResult result)
    {
        if (!_generationHistory.ContainsKey(request.ProcessId))
        {
            _generationHistory[request.ProcessId] = new List<GenerationHistory>();
        }

        _generationHistory[request.ProcessId].Add(new GenerationHistory
        {
            ProjectId = request.ProcessId,
            Template = request.Template,
            Timestamp = DateTime.Now,
            Success = result.Success,
            FilesGenerated = result.Files.Count,
            LinesGenerated = result.TotalLines
        });
    }

    private string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
            return str;

        return char.ToLowerInvariant(str[0]) + str.Substring(1);
    }
}
