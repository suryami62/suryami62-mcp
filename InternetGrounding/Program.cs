using InternetGrounding.Resources;
using InternetGrounding.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// Entry point for the InternetGrounding MCP server application.
/// Configures logging, HTTP clients, and MCP server services.
/// </summary>
HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

// Configure logging to output all levels to console for debugging.
_ = builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

// Add HTTP client services for API interactions.
_ = builder.Services.AddHttpClient();

// Configure MCP server with necessary tools and resources.
_ = builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<GeminiTool>() // Registers Gemini API interaction tool.
    .WithResources<ResourceType>() // Registers custom resources for the MCP server.
    .WithToolsFromAssembly(); // Automatically registers additional tools from the assembly.

await builder.Build().RunAsync();
