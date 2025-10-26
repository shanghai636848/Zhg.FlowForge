using System;
using System.Collections.Generic;
using System.Text;

namespace Zhg.FlowForge.Domain;

/// <summary>
/// BPMN 流程聚合根
/// </summary>
public class BpmnProcess
{
    public string Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Version { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private readonly List<BpmnActivity> _activities = new();
    private readonly List<BpmnSequenceFlow> _sequenceFlows = new();
    private readonly List<BpmnGateway> _gateways = new();

    public IReadOnlyList<BpmnActivity> Activities => _activities.AsReadOnly();
    public IReadOnlyList<BpmnSequenceFlow> SequenceFlows => _sequenceFlows.AsReadOnly();
    public IReadOnlyList<BpmnGateway> Gateways => _gateways.AsReadOnly();

    private BpmnProcess()
    {
        Id = string.Empty;
        Name = string.Empty;
        Description = string.Empty;
        Version = string.Empty;
    }

    public static BpmnProcess Create(string name, string description)
    {
        var process = new BpmnProcess
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Description = description,
            Version = "1.0",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // 添加默认的开始和结束事件
        process.AddActivity(BpmnActivity.CreateStartEvent());
        process.AddActivity(BpmnActivity.CreateEndEvent());

        return process;
    }

    public void AddActivity(BpmnActivity activity)
    {
        _activities.Add(activity);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddSequenceFlow(BpmnSequenceFlow flow)
    {
        _sequenceFlows.Add(flow);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddGateway(BpmnGateway gateway)
    {
        _gateways.Add(gateway);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string? name = null, string? description = null)
    {
        if (name != null) Name = name;
        if (description != null) Description = description;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// BPMN 活动实体
/// </summary>
public class BpmnActivity
{
    public string Id { get; private set; }
    public string Name { get; private set; }
    public string Type { get; private set; }
    public Dictionary<string, string> Properties { get; private set; }

    private BpmnActivity()
    {
        Id = string.Empty;
        Name = string.Empty;
        Type = string.Empty;
        Properties = new();
    }

    public static BpmnActivity Create(string name, string type)
    {
        return new BpmnActivity
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Type = type,
            Properties = new Dictionary<string, string>()
        };
    }

    public static BpmnActivity CreateStartEvent()
    {
        return Create("开始", "StartEvent");
    }

    public static BpmnActivity CreateEndEvent()
    {
        return Create("结束", "EndEvent");
    }
}

/// <summary>
/// BPMN 顺序流实体
/// </summary>
public class BpmnSequenceFlow
{
    public string Id { get; private set; }
    public string SourceRef { get; private set; }
    public string TargetRef { get; private set; }
    public string? ConditionExpression { get; private set; }

    private BpmnSequenceFlow()
    {
        Id = string.Empty;
        SourceRef = string.Empty;
        TargetRef = string.Empty;
    }

    public static BpmnSequenceFlow Create(string sourceRef, string targetRef, string? conditionExpression = null)
    {
        return new BpmnSequenceFlow
        {
            Id = Guid.NewGuid().ToString(),
            SourceRef = sourceRef,
            TargetRef = targetRef,
            ConditionExpression = conditionExpression
        };
    }
}

/// <summary>
/// BPMN 网关实体
/// </summary>
public class BpmnGateway
{
    public string Id { get; private set; }
    public string Name { get; private set; }
    public string Type { get; private set; }

    private BpmnGateway()
    {
        Id = string.Empty;
        Name = string.Empty;
        Type = string.Empty;
    }

    public static BpmnGateway Create(string name, string type)
    {
        return new BpmnGateway
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Type = type
        };
    }
}
