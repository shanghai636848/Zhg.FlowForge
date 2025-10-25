using System;
using System.Collections.Generic;
using System.Text;

namespace Zhg.FlowForge.App.Shared.Services;

/// <summary>
/// BPMN 流程服务接口
/// </summary>
public interface IBpmnService
{
    /// <summary>
    /// 获取所有流程
    /// </summary>
    Task<List<BpmnProcess>> GetProcessesAsync();

    /// <summary>
    /// 获取流程详情
    /// </summary>
    Task<BpmnProcess?> GetProcessAsync(string processId);

    /// <summary>
    /// 创建流程
    /// </summary>
    Task<BpmnProcess> CreateProcessAsync(CreateProcessRequest request);

    /// <summary>
    /// 更新流程
    /// </summary>
    Task<BpmnProcess> UpdateProcessAsync(string processId, UpdateProcessRequest request);

    /// <summary>
    /// 删除流程
    /// </summary>
    Task DeleteProcessAsync(string processId);

    /// <summary>
    /// 验证 BPMN 模型
    /// </summary>
    Task<BpmnValidationResult> ValidateAsync(string processId);

    /// <summary>
    /// 从 XML 导入
    /// </summary>
    Task<BpmnProcess> ImportFromXmlAsync(string xml);

    /// <summary>
    /// 导出为 XML
    /// </summary>
    Task<string> ExportToXmlAsync(string processId);

    /// <summary>
    /// 获取流程图（SVG）
    /// </summary>
    Task<string> GetProcessDiagramAsync(string processId);

    /// <summary>
    /// 分析流程复杂度
    /// </summary>
    Task<ProcessComplexity> AnalyzeComplexityAsync(string processId);
}

/// <summary>
/// BPMN 流程
/// </summary>
public class BpmnProcess
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public List<BpmnActivity> Activities { get; set; } = new();
    public List<BpmnSequenceFlow> SequenceFlows { get; set; } = new();
    public List<BpmnGateway> Gateways { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public string Version { get; set; } = "1.0";
}

/// <summary>
/// BPMN 活动
/// </summary>
public class BpmnActivity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public string Type { get; set; } = "Task"; // Task, ServiceTask, UserTask, etc.
    public Dictionary<string, string> Properties { get; set; } = new();
}

/// <summary>
/// BPMN 顺序流
/// </summary>
public class BpmnSequenceFlow
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SourceRef { get; set; } = "";
    public string TargetRef { get; set; } = "";
    public string? ConditionExpression { get; set; }
}

/// <summary>
/// BPMN 网关
/// </summary>
public class BpmnGateway
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public string Type { get; set; } = "Exclusive"; // Exclusive, Parallel, Inclusive
}

/// <summary>
/// 创建流程请求
/// </summary>
public class CreateProcessRequest
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
}

/// <summary>
/// 更新流程请求
/// </summary>
public class UpdateProcessRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// BPMN 验证结果
/// </summary>
public class BpmnValidationResult
{
    public bool IsValid { get; set; }
    public List<BpmnValidationError> Errors { get; set; } = new();
    public List<BpmnValidationWarning> Warnings { get; set; } = new();
}

/// <summary>
/// BPMN 验证错误
/// </summary>
public class BpmnValidationError
{
    public string Code { get; set; } = "";
    public string Message { get; set; } = "";
    public string ElementId { get; set; } = "";
}

/// <summary>
/// BPMN 验证警告
/// </summary>
public class BpmnValidationWarning
{
    public string Code { get; set; } = "";
    public string Message { get; set; } = "";
    public string ElementId { get; set; } = "";
}

/// <summary>
/// 流程复杂度
/// </summary>
public class ProcessComplexity
{
    public int ActivityCount { get; set; }
    public int GatewayCount { get; set; }
    public int PathCount { get; set; }
    public int CyclomaticComplexity { get; set; }
    public string ComplexityLevel { get; set; } = ""; // Low, Medium, High
    public Dictionary<string, int> ActivityTypeDistribution { get; set; } = new();
}