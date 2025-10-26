using Microsoft.AspNetCore.Http.HttpResults;
using System.Text.Json.Serialization;
using Zhg.FlowForge.Api;
using Zhg.FlowForge.Application;
using Zhg.FlowForge.Application.Contract;
using Zhg.FlowForge.Domain;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Windows 服务支持
builder.Host.UseWindowsService();

// 日志
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// 健康检查
builder.Services.AddHealthChecks();


// 注册服务
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IBpmnProcessService, BpmnProcessService>();
builder.Services.AddScoped<ICodeGenerationService, CodeGenerationService>();
builder.Services.AddScoped<ICompilationService, CompilationService>();
// 注册仓储（使用内存实现，生产环境可替换为 EF Core 等）
builder.Services.AddSingleton<IProjectRepository, InMemoryProjectRepository>();
builder.Services.AddSingleton<IBpmnProcessRepository, InMemoryBpmnProcessRepository>();
// 注册领域服务
builder.Services.AddScoped<IProjectDomainService, ProjectDomainService>();
builder.Services.AddScoped<IFileSystemService, FileSystemService>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// 异常处理
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Unhandled exception");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("Internal Server Error");
    }
});

app.UseCors("AllowAll");
app.MapHealthChecks("/health");
// 注册端点
app.MapProjectEndpoints();
app.MapBpmnProcessEndpoints();
app.MapCodeGenerationEndpoints();
app.MapCompilationEndpoints();

app.Run();
