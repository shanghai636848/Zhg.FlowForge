using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// 配置 HttpClient
builder.Services.AddScoped<HttpClient>(sp =>
{
    var navigationManager = sp.GetRequiredService<NavigationManager>();
    return new HttpClient
    {
        BaseAddress = new Uri(navigationManager.BaseUri)
    };
});

// 注册全局服务
builder.Services.AddSingleton<Zhg.FlowForge.App.Shared.Services.IThemeService, Zhg.FlowForge.App.Shared.Services.ThemeService>();
builder.Services.AddSingleton<Zhg.FlowForge.App.Shared.Services.IToastService, Zhg.FlowForge.App.Shared.Services.ToastService>();
builder.Services.AddSingleton<Zhg.FlowForge.App.Shared.Services.ILocalizationService, Zhg.FlowForge.App.Shared.Services.LocalizationService>();

// 注册业务服务
builder.Services.AddScoped<Zhg.FlowForge.App.Shared.Services.IProjectService, Zhg.FlowForge.App.Shared.Services.ProjectService>();
//builder.Services.AddScoped<IBpmnService, BpmnService>();
//builder.Services.AddScoped<ICodeGenerationService, CodeGenerationService>();
builder.Services.AddScoped<Zhg.FlowForge.App.Shared.Services.ICompilationService, Zhg.FlowForge.App.Shared.Services.CompilationService>();
builder.Services.AddScoped<Zhg.FlowForge.App.Shared.Services.ICodeAnalysisService, Zhg.FlowForge.App.Shared.Services.CodeAnalysisService>();

await builder.Build().RunAsync();
