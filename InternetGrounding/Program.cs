using InternetGrounding.Resources;
using InternetGrounding.Tools;
using InternetGrounding.Tools.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// Entry point for the InternetGrounding MCP server application.
/// Configures logging, HTTP clients, and MCP server services.
/// </summary>
HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

_ = builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

_ = builder.Services.AddHttpClient();

_ = builder.Services.Configure<GeminiApiOptions>(
    builder.Configuration.GetSection(GeminiApiOptions.GeminiApi));

_ = builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<GeminiTool>()
    .WithResources<ResourceType>()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
