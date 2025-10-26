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

// Windows ����֧��
builder.Host.UseWindowsService();

// ��־
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

// �������
builder.Services.AddHealthChecks();


// ע�����
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IBpmnProcessService, BpmnProcessService>();
builder.Services.AddScoped<ICodeGenerationService, CodeGenerationService>();
builder.Services.AddScoped<ICompilationService, CompilationService>();
// ע��ִ���ʹ���ڴ�ʵ�֣������������滻Ϊ EF Core �ȣ�
builder.Services.AddSingleton<IProjectRepository, InMemoryProjectRepository>();
builder.Services.AddSingleton<IBpmnProcessRepository, InMemoryBpmnProcessRepository>();
// ע���������
builder.Services.AddScoped<IProjectDomainService, ProjectDomainService>();
builder.Services.AddScoped<IFileSystemService, FileSystemService>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// �쳣����
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
// ע��˵�
app.MapProjectEndpoints();
app.MapBpmnProcessEndpoints();
app.MapCodeGenerationEndpoints();
app.MapCompilationEndpoints();

app.Run();
