using System;
using System.Collections.Generic;
using System.Text;

namespace Zhg.FlowForge.App.Shared.Services;

public interface ITemplateService
{
    Task<List<WorkflowTemplate>> GetTemplatesAsync();
    Task<WorkflowTemplate?> GetTemplateAsync(string templateId);
    Task<WorkflowTemplate> CreateTemplateAsync(CreateTemplateRequest1 request);
    Task ToggleFavoriteAsync(string templateId);
    Task<List<WorkflowTemplate>> SearchTemplatesAsync(string query);
}

public class WorkflowTemplate
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Category { get; set; } = "";
    public string Author { get; set; } = "";
    public bool IsOfficial { get; set; }
    public bool IsCustom { get; set; }
    public bool IsFavorite { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<string> Features { get; set; } = new();
    public List<string> TechStack { get; set; } = new();
    public string Icon { get; set; } = "fas fa-project-diagram";
    public string GradientClass { get; set; } = "from-blue-500 to-indigo-600";
    public string? ThumbnailUrl { get; set; }
    public int Downloads { get; set; }
    public double Rating { get; set; } = 5.0;
    public int ReviewCount { get; set; }
    public int ActivityCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string UseCases { get; set; } = "";
}

public class CreateTemplateRequest1
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Category { get; set; } = "";
}