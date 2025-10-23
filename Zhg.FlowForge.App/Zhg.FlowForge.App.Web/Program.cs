using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Zhg.FlowForge.App.Shared.Interfaces;
using Zhg.FlowForge.App.Shared.Models;
using Zhg.FlowForge.App.Shared.Services;
using Zhg.FlowForge.App.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddScoped<ScrollService>();

// ����Ӧ������
builder.Services.Configure<AppSettings>(options =>
{
    options.DefaultCulture = "zh-CN";
    options.ResourceSource = "JSON";
    options.JsonResourcesPath = "localization";
    options.UseBuiltInResources = true;
});

builder.Services.AddScoped<IFileSystemService, FileSystemService>();
builder.Services.AddScoped<IJsonResourceService, JsonResourceService>();
builder.Services.AddScoped<BlazorPreferenceService>();
builder.Services.AddScoped<IPreferenceService>(provider =>
    provider.GetRequiredService<BlazorPreferenceService>());
builder.Services.AddScoped<ILocalizationService, LocalizationService>();


var app = builder.Build();

// ��ʼ�����ػ�����
await using (var scope = app.Services.CreateAsyncScope())
{
    var localizationService = scope.ServiceProvider.GetRequiredService<ILocalizationService>();
    await localizationService.InitializeAsync();
}

// ʹ�ñ��ػ��м��
app.UseRequestLocalization();

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
