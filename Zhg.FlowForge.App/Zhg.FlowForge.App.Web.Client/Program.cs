using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// ���� HttpClient
builder.Services.AddScoped<HttpClient>(sp =>
{
    var navigationManager = sp.GetRequiredService<NavigationManager>();
    return new HttpClient
    {
        BaseAddress = new Uri(navigationManager.BaseUri)
    };
});

// ע��ȫ�ַ���
builder.Services.AddSingleton<Zhg.FlowForge.App.Shared.Services.IThemeService, Zhg.FlowForge.App.Shared.Services.ThemeService>();
builder.Services.AddSingleton<Zhg.FlowForge.App.Shared.Services.IToastService, Zhg.FlowForge.App.Shared.Services.ToastService>();
builder.Services.AddSingleton<Zhg.FlowForge.App.Shared.Services.ILocalizationService, Zhg.FlowForge.App.Shared.Services.LocalizationService>();

// ע��ҵ�����
builder.Services.AddScoped<Zhg.FlowForge.App.Shared.Services.IProjectService, Zhg.FlowForge.App.Shared.Services.ProjectService>();
builder.Services.AddScoped<Zhg.FlowForge.App.Shared.Services.IBpmnService, Zhg.FlowForge.App.Shared.Services.BpmnService>();
builder.Services.AddScoped<Zhg.FlowForge.App.Shared.Services.ICodeGenerationService, Zhg.FlowForge.App.Shared.Services.CodeGenerationService>();
builder.Services.AddScoped<Zhg.FlowForge.App.Shared.Services.ICompilationService, Zhg.FlowForge.App.Shared.Services.CompilationService>();
builder.Services.AddScoped<Zhg.FlowForge.App.Shared.Services.ICodeAnalysisService, Zhg.FlowForge.App.Shared.Services.CodeAnalysisService>();
builder.Services.AddScoped<Zhg.FlowForge.App.Shared.Services.IDeploymentService, Zhg.FlowForge.App.Shared.Services.DeploymentService>();
builder.Services.AddScoped<Zhg.FlowForge.App.Shared.Services.ITemplateService, Zhg.FlowForge.App.Shared.Services.TemplateService>();

// ע����־
builder.Logging.SetMinimumLevel(LogLevel.Information);
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);


await builder.Build().RunAsync();
