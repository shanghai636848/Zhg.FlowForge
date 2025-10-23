using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Zhg.FlowForge.App.Shared.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped<ScrollService>();

await builder.Build().RunAsync();
