using Microsoft.AspNetCore.Components;
using Zhg.FlowForge.App.Shared.Services;
using Zhg.FlowForge.App.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// 配置 HttpClient
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
builder.Services.AddSingleton<IThemeService, ThemeService>();
builder.Services.AddSingleton<IToastService, ToastService>();
builder.Services.AddSingleton<ILocalizationService, LocalizationService>();

// 注册业务服务
builder.Services.AddScoped<IProjectService, ProjectService>();
//builder.Services.AddScoped<IBpmnService, BpmnService>();
//builder.Services.AddScoped<ICodeGenerationService, CodeGenerationService>();
builder.Services.AddScoped<ICompilationService, CompilationService>();
builder.Services.AddScoped<ICodeAnalysisService, CodeAnalysisService>();

// 注册日志
builder.Logging.SetMinimumLevel(LogLevel.Information);
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
        typeof(Zhg.FlowForge.App.Shared._Imports).Assembly,
        typeof(Zhg.FlowForge.App.Web.Client._Imports).Assembly);

app.Run();
