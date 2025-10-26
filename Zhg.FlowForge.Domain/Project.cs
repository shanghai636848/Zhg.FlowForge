using System;
using System.Collections.Generic;
using System.Text;
using Zhg.FlowForge.Domain.Shared;

namespace Zhg.FlowForge.Domain;

/// <summary>
/// 项目聚合根
/// </summary>
public class Project
{
    public string Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Namespace { get; private set; }
    public string TargetFramework { get; private set; }
    public string Template { get; private set; }
    public ProjectStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public string CreatedBy { get; private set; }
    public string? LocalPath { get; private set; }
    public bool IsSavedToLocal { get; private set; }
    public Dictionary<string, string> Metadata { get; private set; }

    private Project()
    {
        // EF Core
        Id = string.Empty;
        Name = string.Empty;
        Description = string.Empty;
        Namespace = string.Empty;
        TargetFramework = string.Empty;
        Template = string.Empty;
        CreatedBy = string.Empty;
        Metadata = new();
    }

    public static Project Create(
        string name,
        string description,
        string namespaceName,
        string targetFramework,
        string template,
        string createdBy)
    {
        var project = new Project
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Description = description,
            Namespace = namespaceName,
            TargetFramework = targetFramework,
            Template = template,
            Status = ProjectStatus.Developing,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = createdBy,
            Metadata = new Dictionary<string, string>()
        };

        return project;
    }

    public void Update(string? name = null, string? description = null, ProjectStatus? status = null)
    {
        if (name != null) Name = name;
        if (description != null) Description = description;
        if (status.HasValue) Status = status.Value;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SaveToLocal(string localPath)
    {
        LocalPath = localPath;
        IsSavedToLocal = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsModified()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
