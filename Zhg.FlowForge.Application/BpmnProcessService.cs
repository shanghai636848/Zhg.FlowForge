using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Zhg.FlowForge.Application.Contract;
using Zhg.FlowForge.Domain;

namespace Zhg.FlowForge.Application;

/// <summary>
/// BPMN 流程应用服务实现
/// </summary>
public class BpmnProcessService : IBpmnProcessService
{
    private readonly IBpmnProcessRepository _bpmnRepository;
    private readonly ILogger<BpmnProcessService> _logger;

    public BpmnProcessService(
        IBpmnProcessRepository bpmnRepository,
        ILogger<BpmnProcessService> logger)
    {
        _bpmnRepository = bpmnRepository;
        _logger = logger;
    }

    public async Task<List<BpmnProcessDto>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var processes = await _bpmnRepository.GetAllAsync(cancellationToken);
        return processes.Select(MapToDto).ToList();
    }

    public async Task<BpmnProcessDto?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        var process = await _bpmnRepository.GetByIdAsync(id, cancellationToken);
        return process != null ? MapToDto(process) : null;
    }

    public async Task<BpmnProcessDto> CreateAsync(
        CreateBpmnProcessDto input,
        CancellationToken cancellationToken = default)
    {
        var process = BpmnProcess.Create(input.Name, input.Description);
        await _bpmnRepository.AddAsync(process, cancellationToken);

        _logger.LogInformation("Created BPMN process: {ProcessName} ({ProcessId})",
            process.Name, process.Id);

        return MapToDto(process);
    }

    public async Task<BpmnProcessDto> UpdateAsync(
        string id,
        UpdateBpmnProcessDto input,
        CancellationToken cancellationToken = default)
    {
        var process = await _bpmnRepository.GetByIdAsync(id, cancellationToken);
        if (process == null)
        {
            throw new KeyNotFoundException($"BPMN process with id {id} not found");
        }

        process.Update(input.Name, input.Description);
        await _bpmnRepository.UpdateAsync(process, cancellationToken);

        _logger.LogInformation("Updated BPMN process: {ProcessId}", id);

        return MapToDto(process);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await _bpmnRepository.DeleteAsync(id, cancellationToken);
        _logger.LogInformation("Deleted BPMN process: {ProcessId}", id);
    }

    public async Task<BpmnValidationResultDto> ValidateAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var process = await _bpmnRepository.GetByIdAsync(id, cancellationToken);
        if (process == null)
        {
            throw new KeyNotFoundException($"BPMN process with id {id} not found");
        }

        var result = new BpmnValidationResultDto { IsValid = true };

        // 验证开始事件
        var startEvents = process.Activities.Where(a => a.Type == "StartEvent").ToList();
        if (startEvents.Count == 0)
        {
            result.IsValid = false;
            result.Errors.Add(new ValidationErrorDto
            {
                Code = "NO_START_EVENT",
                Message = "流程必须有至少一个开始事件"
            });
        }
        else if (startEvents.Count > 1)
        {
            result.Warnings.Add(new ValidationWarningDto
            {
                Code = "MULTIPLE_START_EVENTS",
                Message = "流程有多个开始事件"
            });
        }

        // 验证结束事件
        var endEvents = process.Activities.Where(a => a.Type == "EndEvent").ToList();
        if (endEvents.Count == 0)
        {
            result.IsValid = false;
            result.Errors.Add(new ValidationErrorDto
            {
                Code = "NO_END_EVENT",
                Message = "流程必须有至少一个结束事件"
            });
        }

        // 验证顺序流连接
        foreach (var activity in process.Activities)
        {
            if (activity.Type != "StartEvent" && activity.Type != "EndEvent")
            {
                var hasIncoming = process.SequenceFlows.Any(f => f.TargetRef == activity.Id);
                var hasOutgoing = process.SequenceFlows.Any(f => f.SourceRef == activity.Id);

                if (!hasIncoming)
                {
                    result.Warnings.Add(new ValidationWarningDto
                    {
                        Code = "NO_INCOMING_FLOW",
                        Message = $"活动 '{activity.Name}' 没有输入流",
                        ElementId = activity.Id
                    });
                }

                if (!hasOutgoing)
                {
                    result.Warnings.Add(new ValidationWarningDto
                    {
                        Code = "NO_OUTGOING_FLOW",
                        Message = $"活动 '{activity.Name}' 没有输出流",
                        ElementId = activity.Id
                    });
                }
            }
        }

        // 验证死锁
        var deadlocks = DetectDeadlocks(process);
        if (deadlocks.Any())
        {
            result.IsValid = false;
            foreach (var deadlock in deadlocks)
            {
                result.Errors.Add(new ValidationErrorDto
                {
                    Code = "DEADLOCK_DETECTED",
                    Message = $"检测到死锁",
                    ElementId = deadlock
                });
            }
        }

        return result;
    }

    public async Task<string> ExportToXmlAsync(string id, CancellationToken cancellationToken = default)
    {
        var process = await _bpmnRepository.GetByIdAsync(id, cancellationToken);
        if (process == null)
        {
            throw new KeyNotFoundException($"BPMN process with id {id} not found");
        }

        var xml = GenerateBpmnXml(process);
        _logger.LogInformation("Exported BPMN process to XML: {ProcessId}", id);

        return xml;
    }

    public async Task<BpmnProcessDto> ImportFromXmlAsync(
        string xml,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var doc = XDocument.Parse(xml);
            var process = ParseBpmnXml(doc);

            await _bpmnRepository.AddAsync(process, cancellationToken);

            _logger.LogInformation("Imported BPMN process from XML: {ProcessName}", process.Name);

            return MapToDto(process);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import BPMN XML");
            throw new InvalidOperationException($"导入失败: {ex.Message}", ex);
        }
    }

    #region Private Methods

    private static BpmnProcessDto MapToDto(BpmnProcess process)
    {
        return new BpmnProcessDto
        {
            Id = process.Id,
            Name = process.Name,
            Description = process.Description,
            Version = process.Version,
            CreatedAt = process.CreatedAt,
            UpdatedAt = process.UpdatedAt,
            Activities = process.Activities.Select(a => new BpmnActivityDto
            {
                Id = a.Id,
                Name = a.Name,
                Type = a.Type,
                Properties = a.Properties
            }).ToList(),
            SequenceFlows = process.SequenceFlows.Select(f => new BpmnSequenceFlowDto
            {
                Id = f.Id,
                SourceRef = f.SourceRef,
                TargetRef = f.TargetRef,
                ConditionExpression = f.ConditionExpression
            }).ToList(),
            Gateways = process.Gateways.Select(g => new BpmnGatewayDto
            {
                Id = g.Id,
                Name = g.Name,
                Type = g.Type
            }).ToList()
        };
    }

    private List<string> DetectDeadlocks(BpmnProcess process)
    {
        var deadlocks = new List<string>();

        // 简化的死锁检测：查找没有输出流的非结束活动
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
        XNamespace bpmn = "http://www.omg.org/spec/BPMN/20100524/MODEL";

        var processElement = doc.Descendants(bpmn + "process").FirstOrDefault();
        if (processElement == null)
        {
            throw new InvalidOperationException("无效的 BPMN XML：未找到 process 元素");
        }

        var processId = processElement.Attribute("id")?.Value ?? Guid.NewGuid().ToString();
        var processName = processElement.Attribute("name")?.Value ?? "未命名流程";

        var process = BpmnProcess.Create(processName, "");

        // 解析活动
        foreach (var taskElement in processElement.Elements()
            .Where(e => e.Name.LocalName.Contains("Task") || e.Name.LocalName.Contains("Event")))
        {
            var activity = BpmnActivity.Create(
                taskElement.Attribute("name")?.Value ?? "",
                taskElement.Name.LocalName
            );
            process.AddActivity(activity);
        }

        // 解析顺序流
        foreach (var flowElement in processElement.Elements(bpmn + "sequenceFlow"))
        {
            var flow = BpmnSequenceFlow.Create(
                flowElement.Attribute("sourceRef")?.Value ?? "",
                flowElement.Attribute("targetRef")?.Value ?? ""
            );
            process.AddSequenceFlow(flow);
        }

        // 解析网关
        foreach (var gatewayElement in processElement.Elements()
            .Where(e => e.Name.LocalName.Contains("Gateway")))
        {
            var gateway = BpmnGateway.Create(
                gatewayElement.Attribute("name")?.Value ?? "",
                gatewayElement.Name.LocalName.Replace("Gateway", "")
            );
            process.AddGateway(gateway);
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
        sb.AppendLine($"  <process id=\"{process.Id}\" name=\"{process.Name}\" isExecutable=\"true\">");

        // 活动
        foreach (var activity in process.Activities)
        {
            var elementName = activity.Type.ToLowerInvariant();
            sb.AppendLine($"    <{elementName} id=\"{activity.Id}\" name=\"{activity.Name}\" />");
        }

        // 网关
        foreach (var gateway in process.Gateways)
        {
            var elementName = gateway.Type.ToLowerInvariant() + "Gateway";
            sb.AppendLine($"    <{elementName} id=\"{gateway.Id}\" name=\"{gateway.Name}\" />");
        }

        // 顺序流
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
        sb.AppendLine("</definitions>");

        return sb.ToString();
    }

    #endregion
}