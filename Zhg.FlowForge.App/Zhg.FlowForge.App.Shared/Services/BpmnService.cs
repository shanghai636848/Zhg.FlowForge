using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Zhg.FlowForge.App.Shared.Services;

namespace Zhg.FlowForge.App.Shared.Services;

public class BpmnService : IBpmnService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BpmnService> _logger;
    private readonly Dictionary<string, BpmnProcess> _processes = new();

    public BpmnService(
        HttpClient httpClient,
        ILogger<BpmnService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;    // 初始化示例数据
        InitializeSampleData();
    }

    public async Task<List<BpmnProcess>> GetProcessesAsync()
    {
        await Task.Delay(100);
        return _processes.Values.OrderByDescending(p => p.UpdatedAt).ToList();
    }
    public async Task<BpmnProcess?> GetProcessAsync(string processId)
    {
        await Task.Delay(50);
        return _processes.TryGetValue(processId, out var process) ? process : null;
    }
    public async Task<BpmnProcess> CreateProcessAsync(CreateProcessRequest request)
    {
        await Task.Delay(100); var process = new BpmnProcess
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            Description = request.Description,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };    // 添加默认的开始和结束事件
        process.Activities.Add(new BpmnActivity
        {
            Id = "start",
            Name = "开始",
            Type = "StartEvent"
        }); process.Activities.Add(new BpmnActivity
        {
            Id = "end",
            Name = "结束",
            Type = "EndEvent"
        }); _processes[process.Id] = process; 
        _logger.LogInformation("创建 BPMN 流程: {ProcessName} ({ProcessId})", process.Name, process.Id); return process;
    }
    public async Task<BpmnProcess> UpdateProcessAsync(string processId, UpdateProcessRequest request)
    {
        await Task.Delay(100); if (!_processes.TryGetValue(processId, out var process))
        {
            throw new Exception($"流程 {processId} 不存在");
        }
        if (request.Name != null) process.Name = request.Name;
        if (request.Description != null) process.Description = request.Description;
        process.UpdatedAt = DateTime.Now; _logger.LogInformation("更新 BPMN 流程: {ProcessId}", processId); return process;
    }
    public async Task DeleteProcessAsync(string processId)
    {
        await Task.Delay(100); _processes.Remove(processId); _logger.LogInformation("删除 BPMN 流程: {ProcessId}", processId);
    }
    public async Task<BpmnValidationResult> ValidateAsync(string processId)
    {
        await Task.Delay(200); var result = new BpmnValidationResult { IsValid = true }; if (!_processes.TryGetValue(processId, out var process))
        {
            result.IsValid = false;
            result.Errors.Add(new BpmnValidationError
            {
                Code = "PROCESS_NOT_FOUND",
                Message = "流程不存在"
            });
            return result;
        }    // 验证开始事件
        var startEvents = process.Activities.Where(a => a.Type == "StartEvent").ToList();
        if (startEvents.Count == 0)
        {
            result.IsValid = false;
            result.Errors.Add(new BpmnValidationError
            {
                Code = "NO_START_EVENT",
                Message = "流程必须有至少一个开始事件"
            });
        }
        else if (startEvents.Count > 1)
        {
            result.Warnings.Add(new BpmnValidationWarning
            {
                Code = "MULTIPLE_START_EVENTS",
                Message = "流程有多个开始事件"
            });
        }    // 验证结束事件
        var endEvents = process.Activities.Where(a => a.Type == "EndEvent").ToList();
        if (endEvents.Count == 0)
        {
            result.IsValid = false;
            result.Errors.Add(new BpmnValidationError
            {
                Code = "NO_END_EVENT",
                Message = "流程必须有至少一个结束事件"
            });
        }    // 验证顺序流连接
        foreach (var activity in process.Activities)
        {
            if (activity.Type != "StartEvent" && activity.Type != "EndEvent")
            {
                var hasIncoming = process.SequenceFlows.Any(f => f.TargetRef == activity.Id);
                var hasOutgoing = process.SequenceFlows.Any(f => f.SourceRef == activity.Id); if (!hasIncoming)
                {
                    result.Warnings.Add(new BpmnValidationWarning
                    {
                        Code = "NO_INCOMING_FLOW",
                        Message = $"活动 '{activity.Name}' 没有输入流",
                        ElementId = activity.Id
                    });
                }
                if (!hasOutgoing)
                {
                    result.Warnings.Add(new BpmnValidationWarning
                    {
                        Code = "NO_OUTGOING_FLOW",
                        Message = $"活动 '{activity.Name}' 没有输出流",
                        ElementId = activity.Id
                    });
                }
            }
        }    // 验证死锁
        var deadlocks = DetectDeadlocks(process);
        if (deadlocks.Any())
        {
            result.IsValid = false;
            foreach (var deadlock in deadlocks)
            {
                result.Errors.Add(new BpmnValidationError
                {
                    Code = "DEADLOCK_DETECTED",
                    Message = $"检测到死锁: {deadlock}",
                    ElementId = deadlock
                });
            }
        }
        return result;
    }
    public async Task<BpmnProcess> ImportFromXmlAsync(string xml)
    {
        await Task.Delay(300); try
        {
            var doc = XDocument.Parse(xml);
            var process = ParseBpmnXml(doc); _processes[process.Id] = process; _logger.LogInformation("从 XML 导入 BPMN 流程: {ProcessName}", process.Name); return process;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "导入 BPMN XML 失败");
            throw new Exception($"导入失败: {ex.Message}");
        }
    }
    public async Task<string> ExportToXmlAsync(string processId)
    {
        await Task.Delay(200); if (!_processes.TryGetValue(processId, out var process))
        {
            throw new Exception($"流程 {processId} 不存在");
        }
        var xml = GenerateBpmnXml(process);
        _logger.LogInformation("导出 BPMN 流程为 XML: {ProcessId}", processId); return xml;
    }
    public async Task<string> GetProcessDiagramAsync(string processId)
    {
        await Task.Delay(200); if (!_processes.TryGetValue(processId, out var process))
        {
            throw new Exception($"流程 {processId} 不存在");
        }    // 生成简单的 SVG 图表
        var svg = GenerateProcessDiagram(process);
        return svg;
    }
    public async Task<ProcessComplexity> AnalyzeComplexityAsync(string processId)
    {
        await Task.Delay(150); if (!_processes.TryGetValue(processId, out var process))
        {
            throw new Exception($"流程 {processId} 不存在");
        }
        var complexity = new ProcessComplexity
        {
            ActivityCount = process.Activities.Count,
            GatewayCount = process.Gateways.Count,
            PathCount = CalculatePathCount(process),
            CyclomaticComplexity = CalculateCyclomaticComplexity(process)
        };    // 活动类型分布
        foreach (var activity in process.Activities)
        {
            if (!complexity.ActivityTypeDistribution.ContainsKey(activity.Type))
            {
                complexity.ActivityTypeDistribution[activity.Type] = 0;
            }
            complexity.ActivityTypeDistribution[activity.Type]++;
        }    // 复杂度等级
        complexity.ComplexityLevel = complexity.CyclomaticComplexity switch
        {
            <= 10 => "Low",
            <= 20 => "Medium",
            _ => "High"
        }; return complexity;
    }
    // 私有辅助方法
    private void InitializeSampleData()
    {
        // 示例流程 1: 订单处理
        var orderProcess = new BpmnProcess
        {
            Id = "process-order",
            Name = "订单处理流程",
            Description = "电商订单自动化处理",
            Version = "1.0"
        };

        orderProcess.Activities.AddRange(new[]
        {
        new BpmnActivity { Id = "start", Name = "开始", Type = "StartEvent" },
        new BpmnActivity { Id = "receive-order", Name = "接收订单", Type = "UserTask" },
        new BpmnActivity { Id = "validate-order", Name = "验证订单", Type = "ServiceTask" },
        new BpmnActivity { Id = "check-inventory", Name = "检查库存", Type = "ServiceTask" },
        new BpmnActivity { Id = "process-payment", Name = "处理支付", Type = "ServiceTask" },
        new BpmnActivity { Id = "ship-order", Name = "发货", Type = "ServiceTask" },
        new BpmnActivity { Id = "notify-customer", Name = "通知客户", Type = "SendTask" },
        new BpmnActivity { Id = "end", Name = "结束", Type = "EndEvent" }
    });

        orderProcess.SequenceFlows.AddRange(new[]
            {
        new BpmnSequenceFlow { Id = "flow1", SourceRef = "start", TargetRef = "receive-order" },
        new BpmnSequenceFlow { Id = "flow2", SourceRef = "receive-order", TargetRef = "validate-order" },
        new BpmnSequenceFlow { Id = "flow3", SourceRef = "validate-order", TargetRef = "check-inventory" },
        new BpmnSequenceFlow { Id = "flow4", SourceRef = "check-inventory", TargetRef = "process-payment" },
        new BpmnSequenceFlow { Id = "flow5", SourceRef = "process-payment", TargetRef = "ship-order" },
        new BpmnSequenceFlow { Id = "flow6", SourceRef = "ship-order", TargetRef = "notify-customer" },
        new BpmnSequenceFlow { Id = "flow7", SourceRef = "notify-customer", TargetRef = "end" }
    }); _processes[orderProcess.Id] = orderProcess;    // 示例流程 2: 审批流程
        var approvalProcess = new BpmnProcess
        {
            Id = "process-approval",
            Name = "审批流程",
            Description = "多级审批工作流",
            Version = "1.0"
        }; approvalProcess.Activities.AddRange(new[]
        {
        new BpmnActivity { Id = "start", Name = "开始", Type = "StartEvent" },
        new BpmnActivity { Id = "submit-request", Name = "提交申请", Type = "UserTask" },
        new BpmnActivity { Id = "manager-review", Name = "经理审批", Type = "UserTask" },
        new BpmnActivity { Id = "director-review", Name = "总监审批", Type = "UserTask" },
        new BpmnActivity { Id = "approve", Name = "批准", Type = "ServiceTask" },
        new BpmnActivity { Id = "reject", Name = "拒绝", Type = "ServiceTask" },
        new BpmnActivity { Id = "end", Name = "结束", Type = "EndEvent" }
    }); approvalProcess.Gateways.Add(new BpmnGateway
    {
        Id = "gateway1",
        Name = "审批决策",
        Type = "Exclusive"
    }); approvalProcess.SequenceFlows.AddRange(new[]
    {
        new BpmnSequenceFlow { Id = "flow1", SourceRef = "start", TargetRef = "submit-request" },
        new BpmnSequenceFlow { Id = "flow2", SourceRef = "submit-request", TargetRef = "manager-review" },
        new BpmnSequenceFlow { Id = "flow3", SourceRef = "manager-review", TargetRef = "director-review" },
        new BpmnSequenceFlow { Id = "flow4", SourceRef = "director-review", TargetRef = "gateway1" },
        new BpmnSequenceFlow { Id = "flow5", SourceRef = "gateway1", TargetRef = "approve", ConditionExpression = "approved == true" },
        new BpmnSequenceFlow { Id = "flow6", SourceRef = "gateway1", TargetRef = "reject", ConditionExpression = "approved == false" },
        new BpmnSequenceFlow { Id = "flow7", SourceRef = "approve", TargetRef = "end" },
        new BpmnSequenceFlow { Id = "flow8", SourceRef = "reject", TargetRef = "end" }
    }); _processes[approvalProcess.Id] = approvalProcess;
    }
    private List<string> DetectDeadlocks(BpmnProcess process)
    {
        var deadlocks = new List<string>();    // 简化的死锁检测：查找没有输出流的非结束活动
        foreach (var activity in process.Activities)
        {
            if (activity.Type != "EndEvent")
            {
                var hasOutgoing = process.SequenceFlows.Any(f => f.SourceRef == activity.Id);
                if (!hasOutgoing)
                {
                    deadlocks.Add(activity.Id);
                }
            }
        }
        return deadlocks;
    }
    private BpmnProcess ParseBpmnXml(XDocument doc)
    {
        XNamespace bpmn = "http://www.omg.org/spec/BPMN/20100524/MODEL"; var processElement = doc.Descendants(bpmn + "process").FirstOrDefault();
        if (processElement == null)
        {
            throw new Exception("无效的 BPMN XML：未找到 process 元素");
        }
        var process = new BpmnProcess
        {
            Id = processElement.Attribute("id")?.Value ?? Guid.NewGuid().ToString(),
            Name = processElement.Attribute("name")?.Value ?? "未命名流程",
            Description = ""
        };    // 解析活动
        foreach (var taskElement in processElement.Elements()
            .Where(e => e.Name.LocalName.Contains("Task") || e.Name.LocalName.Contains("Event")))
        {
            process.Activities.Add(new BpmnActivity
            {
                Id = taskElement.Attribute("id")?.Value ?? Guid.NewGuid().ToString(),
                Name = taskElement.Attribute("name")?.Value ?? "",
                Type = taskElement.Name.LocalName
            });
        }    // 解析顺序流
        foreach (var flowElement in processElement.Elements(bpmn + "sequenceFlow"))
        {
            process.SequenceFlows.Add(new BpmnSequenceFlow
            {
                Id = flowElement.Attribute("id")?.Value ?? Guid.NewGuid().ToString(),
                SourceRef = flowElement.Attribute("sourceRef")?.Value ?? "",
                TargetRef = flowElement.Attribute("targetRef")?.Value ?? ""
            });
        }    // 解析网关
        foreach (var gatewayElement in processElement.Elements()
            .Where(e => e.Name.LocalName.Contains("Gateway")))
        {
            process.Gateways.Add(new BpmnGateway
            {
                Id = gatewayElement.Attribute("id")?.Value ?? Guid.NewGuid().ToString(),
                Name = gatewayElement.Attribute("name")?.Value ?? "",
                Type = gatewayElement.Name.LocalName.Replace("Gateway", "")
            });
        }
        return process;
    }
    private string GenerateBpmnXml(BpmnProcess process)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<definitions xmlns=\"http://www.omg.org/spec/BPMN/20100524/MODEL\"");
        sb.AppendLine("             xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"");
        sb.AppendLine("             targetNamespace=\"http://flowforge.io/bpmn\">");
        sb.AppendLine();
        sb.AppendLine($"  <process id=\"{process.Id}\" name=\"{process.Name}\" isExecutable=\"true\">");    // 活动
        foreach (var activity in process.Activities)
        {
            var elementName = activity.Type.ToLowerInvariant();
            sb.AppendLine($"    <{elementName} id=\"{activity.Id}\" name=\"{activity.Name}\" />");
        }    // 网关
        foreach (var gateway in process.Gateways)
        {
            var elementName = gateway.Type.ToLowerInvariant() + "Gateway";
            sb.AppendLine($"    <{elementName} id=\"{gateway.Id}\" name=\"{gateway.Name}\" />");
        }    // 顺序流
        foreach (var flow in process.SequenceFlows)
        {
            sb.Append($"    <sequenceFlow id=\"{flow.Id}\" sourceRef=\"{flow.SourceRef}\" targetRef=\"{flow.TargetRef}\"");
            if (!string.IsNullOrEmpty(flow.ConditionExpression))
            {
                sb.AppendLine(">");
                sb.AppendLine($"      <conditionExpression>{flow.ConditionExpression}</conditionExpression>");
                sb.AppendLine("    </sequenceFlow>");
            }
            else
            {
                sb.AppendLine(" />");
            }
        }
        sb.AppendLine("  </process>");
        sb.AppendLine("</definitions>"); return sb.ToString();
    }
    private string GenerateProcessDiagram(BpmnProcess process)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<svg width=\"800\" height=\"400\" xmlns=\"http://www.w3.org/2000/svg\">");    // 背景
        sb.AppendLine("  <rect width=\"100%\" height=\"100%\" fill=\"#f8fafc\"/>");    // 简单的横向布局
        int x = 50;
        int y = 150;
        int spacing = 120; for (int i = 0; i < process.Activities.Count; i++)
        {
            var activity = process.Activities[i];
            var currentX = x + (i * spacing);        // 活动框
            if (activity.Type == "StartEvent" || activity.Type == "EndEvent")
            {
                // 圆形事件
                sb.AppendLine($"  <circle cx=\"{currentX}\" cy=\"{y}\" r=\"20\" fill=\"#10b981\" stroke=\"#059669\" stroke-width=\"2\"/>");
            }
            else
            {
                // 矩形任务
                sb.AppendLine($"  <rect x=\"{currentX - 40}\" y=\"{y - 30}\" width=\"80\" height=\"60\" rx=\"5\" fill=\"#3b82f6\" stroke=\"#2563eb\" stroke-width=\"2\"/>");
            }        // 文本
            sb.AppendLine($"  <text x=\"{currentX}\" y=\"{y + 50}\" text-anchor=\"middle\" font-size=\"12\" fill=\"#1e293b\">{activity.Name}</text>");        // 连接线
            if (i < process.Activities.Count - 1)
            {
                var nextX = x + ((i + 1) * spacing);
                sb.AppendLine($"  <line x1=\"{currentX + 40}\" y1=\"{y}\" x2=\"{nextX - 40}\" y2=\"{y}\" stroke=\"#64748b\" stroke-width=\"2\" marker-end=\"url(#arrowhead)\"/>");
            }
        }    // 箭头标记
        sb.AppendLine("  <defs>");
        sb.AppendLine("    <marker id=\"arrowhead\" markerWidth=\"10\" markerHeight=\"10\" refX=\"9\" refY=\"3\" orient=\"auto\">");
        sb.AppendLine("      <polygon points=\"0 0, 10 3, 0 6\" fill=\"#64748b\"/>");
        sb.AppendLine("    </marker>");
        sb.AppendLine("  </defs>"); sb.AppendLine("</svg>"); return sb.ToString();
    }
    private int CalculatePathCount(BpmnProcess process)
    {
        // 简化的路径计数
        return (int)Math.Pow(2, process.Gateways.Count);
    }
    private int CalculateCyclomaticComplexity(BpmnProcess process)
    {
        // 圈复杂度 = E - N + 2P
        // E = 边数（顺序流）
        // N = 节点数（活动 + 网关）
        // P = 连通分量数（通常为 1）
        int edges = process.SequenceFlows.Count;
        int nodes = process.Activities.Count + process.Gateways.Count;
        int components = 1;
        return edges - nodes + (2 * components);
    }
}