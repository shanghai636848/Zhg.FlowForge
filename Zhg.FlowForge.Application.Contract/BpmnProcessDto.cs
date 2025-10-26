using System;
using System.Collections.Generic;
using System.Text;

namespace Zhg.FlowForge.Application.Contract;

/// <summary>
/// BPMN 流程 DTO
/// </summary>
public class BpmnProcessDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<BpmnActivityDto> Activities { get; set; } = new();
    public List<BpmnSequenceFlowDto> SequenceFlows { get; set; } = new();
    public List<BpmnGatewayDto> Gateways { get; set; } = new();
}

public class BpmnActivityDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, string> Properties { get; set; } = new();
}

public class BpmnSequenceFlowDto
{
    public string Id { get; set; } = string.Empty;
    public string SourceRef { get; set; } = string.Empty;
    public string TargetRef { get; set; } = string.Empty;
    public string? ConditionExpression { get; set; }
}

public class BpmnGatewayDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

/// <summary>
/// 创建 BPMN 流程请求 DTO
/// </summary>
public class CreateBpmnProcessDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// 更新 BPMN 流程请求 DTO
/// </summary>
public class UpdateBpmnProcessDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// BPMN 验证结果 DTO
/// </summary>
public class BpmnValidationResultDto
{
    public bool IsValid { get; set; }
    public List<ValidationErrorDto> Errors { get; set; } = new();
    public List<ValidationWarningDto> Warnings { get; set; } = new();
}

public class ValidationErrorDto
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string ElementId { get; set; } = string.Empty;
}

public class ValidationWarningDto
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string ElementId { get; set; } = string.Empty;
}