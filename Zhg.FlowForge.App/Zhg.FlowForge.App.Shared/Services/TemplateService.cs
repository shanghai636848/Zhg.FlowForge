using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Zhg.FlowForge.App.Shared.Services;

public class TemplateService : ITemplateService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TemplateService> _logger;
    private readonly List<WorkflowTemplate> _templates = new();

    public TemplateService(HttpClient httpClient, ILogger<TemplateService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        InitializeSampleTemplates();
    }

    public async Task<List<WorkflowTemplate>> GetTemplatesAsync()
    {
        await Task.Delay(300); // 模拟网络延迟
        return _templates;
    }

    public async Task<WorkflowTemplate?> GetTemplateAsync(string templateId)
    {
        await Task.Delay(100);
        return _templates.FirstOrDefault(t => t.Id == templateId);
    }

    public async Task<WorkflowTemplate> CreateTemplateAsync(CreateTemplateRequest1 request)
    {
        await Task.Delay(200);

        var template = new WorkflowTemplate
        {
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            Author = "Current User",
            IsCustom = true,
            IsOfficial = false
        };

        _templates.Add(template);
        _logger.LogInformation("创建模板: {TemplateName}", template.Name);

        return template;
    }

    public async Task ToggleFavoriteAsync(string templateId)
    {
        await Task.Delay(50);

        var template = _templates.FirstOrDefault(t => t.Id == templateId);
        if (template != null)
        {
            template.IsFavorite = !template.IsFavorite;
        }
    }

    public async Task<List<WorkflowTemplate>> SearchTemplatesAsync(string query)
    {
        await Task.Delay(200);

        return _templates
            .Where(t => t.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                       t.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private void InitializeSampleTemplates()
    {
        _templates.AddRange(new[]
        {
            new WorkflowTemplate
            {
                Name = "电商订单处理流程",
                Description = "从接单到发货的完整订单处理流程，支持库存检查、支付处理和物流跟踪",
                Category = "business",
                Author = "FlowForge Team",
                IsOfficial = true,
                Tags = new List<string> { "电商", "订单", "支付", "库存" },
                Features = new List<string>
                {
                    "自动库存检查", "多支付方式", "实时物流跟踪", "订单状态通知"
                },
                TechStack = new List<string> { "C#", "ASP.NET Core", "Redis", "RabbitMQ" },
                GradientClass = "from-blue-500 to-cyan-600",
                Downloads = 1234,
                Rating = 4.8,
                ReviewCount = 89,
                ActivityCount = 12,
                UseCases = "适用于电商平台、零售系统等需要处理订单全流程的场景"
            },
            new WorkflowTemplate
            {
                Name = "多级审批流程",
                Description = "灵活的多级审批工作流，支持并行审批、条件分支和自动提醒",
                Category = "business",
                Author = "FlowForge Team",
                IsOfficial = true,
                Tags = new List<string> { "审批", "OA", "工作流" },
                Features = new List<string>
                {
                    "多级审批", "并行审批", "条件路由", "超时提醒"
                },
                TechStack = new List<string> { "C#", "ASP.NET Core", "SignalR" },
                GradientClass = "from-purple-500 to-pink-600",
                Downloads = 892,
                Rating = 4.9,
                ReviewCount = 67,
                ActivityCount = 8,
                UseCases = "适用于企业 OA 系统、费用报销、请假申请等审批场景"
            },
            new WorkflowTemplate
            {
                Name = "数据 ETL 管道",
                Description = "自动化的数据提取、转换和加载流程，支持多数据源和实时同步",
                Category = "automation",
                Author = "DataTeam",
                IsOfficial = false,
                Tags = new List<string> { "ETL", "数据处理", "自动化" },
                Features = new List<string>
                {
                    "多数据源支持", "数据转换", "增量同步", "错误重试"
                },
                TechStack = new List<string> { "C#", "Dapper", "Quartz.NET" },
                GradientClass = "from-emerald-500 to-teal-600",
                Downloads = 567,
                Rating = 4.6,
                ReviewCount = 34,
                ActivityCount = 15,
                UseCases = "适用于数据仓库、BI 系统、数据分析平台"
            },
            new WorkflowTemplate
            {
                Name = "用户注册激活流程",
                Description = "完整的用户注册、邮箱验证、账号激活流程",
                Category = "business",
                Author = "AuthTeam",
                IsOfficial = false,
                Tags = new List<string> { "用户", "注册", "认证" },
                Features = new List<string>
                {
                    "邮箱验证", "手机验证", "实名认证", "欢迎邮件"
                },
                TechStack = new List<string> { "C#", "ASP.NET Identity", "SendGrid" },
                GradientClass = "from-rose-500 to-orange-600",
                Downloads = 445,
                Rating = 4.7,
                ReviewCount = 28,
                ActivityCount = 6,
                UseCases = "适用于需要用户注册和身份验证的各类系统"
            },
            new WorkflowTemplate
            {
                Name = "定时任务调度",
                Description = "灵活的定时任务调度系统，支持 Cron 表达式和依赖管理",
                Category = "automation",
                Author = "FlowForge Team",
                IsOfficial = true,
                Tags = new List<string> { "定时任务", "调度", "自动化" },
                Features = new List<string>
                {
                    "Cron 表达式", "任务依赖", "失败重试", "执行历史"
                },
                TechStack = new List<string> { "C#", "Quartz.NET", "Hangfire" },
                GradientClass = "from-indigo-500 to-purple-600",
                Downloads = 723,
                Rating = 4.8,
                ReviewCount = 52,
                ActivityCount = 10,
                UseCases = "适用于需要定时执行的数据处理、报表生成、数据清理等场景"
            },
            new WorkflowTemplate
            {
                Name = "API 网关集成",
                Description = "微服务 API 网关集成流程，支持认证、限流和熔断",
                Category = "integration",
                Author = "CloudTeam",
                IsOfficial = false,
                Tags = new List<string> { "API", "网关", "微服务" },
                Features = new List<string>
                {
                    "统一认证", "请求限流", "熔断降级", "链路追踪"
                },
                TechStack = new List<string> { "C#", "Ocelot", "Polly" },
                GradientClass = "from-cyan-500 to-blue-600",
                Downloads = 389,
                Rating = 4.5,
                ReviewCount = 21,
                ActivityCount = 14,
                UseCases = "适用于微服务架构、API 网关、服务治理场景"
            },
            new WorkflowTemplate
            {
                Name = "消息队列处理",
                Description = "基于消息队列的异步处理流程，支持消息重试和死信队列",
                Category = "integration",
                Author = "MessagingTeam",
                IsOfficial = false,
                Tags = new List<string> { "消息队列", "异步", "RabbitMQ" },
                Features = new List<string>
                {
                    "消息确认", "重试机制", "死信队列", "优先级队列"
                },
                TechStack = new List<string> { "C#", "RabbitMQ", "MassTransit" },
                GradientClass = "from-amber-500 to-orange-600",
                Downloads = 512,
                Rating = 4.7,
                ReviewCount = 38,
                ActivityCount = 9,
                UseCases = "适用于高并发、解耦、异步处理场景"
            },
            new WorkflowTemplate
            {
                Name = "文档审批归档",
                Description = "文档从提交、审批到归档的完整生命周期管理",
                Category = "business",
                Author = "DocTeam",
                IsOfficial = false,
                Tags = new List<string> { "文档", "审批", "归档" },
                Features = new List<string>
                {
                    "版本控制", "审批流", "电子签名", "自动归档"
                },
                TechStack = new List<string> { "C#", "Azure Blob", "SignalR" },
                GradientClass = "from-violet-500 to-purple-600",
                Downloads = 298,
                Rating = 4.6,
                ReviewCount = 19,
                ActivityCount = 11,
                UseCases = "适用于文档管理系统、合同管理、档案管理"
            },
            new WorkflowTemplate
            {
                Name = "CI/CD 部署流水线",
                Description = "自动化的持续集成和持续部署流程",
                Category = "automation",
                Author = "DevOps Team",
                IsOfficial = true,
                Tags = new List<string> { "CI/CD", "DevOps", "自动化" },
                Features = new List<string>
                {
                    "自动构建", "单元测试", "自动部署", "回滚机制"
                },
                TechStack = new List<string> { "C#", "Docker", "Kubernetes" },
                GradientClass = "from-green-500 to-emerald-600",
                Downloads = 876,
                Rating = 4.9,
                ReviewCount = 71,
                ActivityCount = 18,
                UseCases = "适用于 DevOps 团队、自动化部署、持续交付"
            },
            new WorkflowTemplate
            {
                Name = "客户服务工单系统",
                Description = "从工单创建到解决的完整客服流程",
                Category = "business",
                Author = "ServiceTeam",
                IsOfficial = false,
                Tags = new List<string> { "客服", "工单", "CRM" },
                Features = new List<string>
                {
                    "工单分配", "优先级管理", "SLA 监控", "客户通知"
                },
                TechStack = new List<string> { "C#", "ASP.NET Core", "SendGrid" },
                GradientClass = "from-pink-500 to-rose-600",
                Downloads = 634,
                Rating = 4.7,
                ReviewCount = 45,
                ActivityCount = 13,
                UseCases = "适用于客服系统、IT 运维、售后服务"
            },
            new WorkflowTemplate
            {
                Name = "库存补货流程",
                Description = "自动化的库存监控和补货申请流程",
                Category = "automation",
                Author = "InventoryTeam",
                IsOfficial = false,
                Tags = new List<string> { "库存", "补货", "供应链" },
                Features = new List<string>
                {
                    "库存监控", "自动预警", "补货申请", "供应商对接"
                },
                TechStack = new List<string> { "C#", "SQL Server", "Redis" },
                GradientClass = "from-yellow-500 to-amber-600",
                Downloads = 421,
                Rating = 4.5,
                ReviewCount = 27,
                ActivityCount = 10,
                UseCases = "适用于零售、仓储、供应链管理系统"
            },
            new WorkflowTemplate
            {
                Name = "员工入职流程",
                Description = "新员工从 Offer 到入职的完整 HR 流程",
                Category = "business",
                Author = "HRTeam",
                IsOfficial = false,
                Tags = new List<string> { "HR", "入职", "人力资源" },
                Features = new List<string>
                {
                    "资料收集", "账号开通", "培训安排", "资产分配"
                },
                TechStack = new List<string> { "C#", "ASP.NET Core" },
                GradientClass = "from-blue-500 to-indigo-600",
                Downloads = 345,
                Rating = 4.6,
                ReviewCount = 22,
                ActivityCount = 7,
                UseCases = "适用于 HR 系统、人力资源管理、员工管理"
            }
        });
    }
}
