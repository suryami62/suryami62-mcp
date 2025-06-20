using InternetGrounding.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

_ = builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

_ = builder.Services.AddHttpClient();

_ = builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<GeminiTool>()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
