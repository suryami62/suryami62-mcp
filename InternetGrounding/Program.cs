using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text;
using System.Text.Json;

/// <summary>
/// Entry point for the Model Context Protocol (MCP) server application.
/// Configures the host, services, logging, and runs the server.
/// </summary>
internal class Program
{
    private static async Task Main(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

        // Set the content root to the directory of the executable
        builder.Environment.ContentRootPath = AppContext.BaseDirectory;

        _ = builder.Logging.AddConsole(consoleLogOptions =>
        {
            // Configure all logs to go to stderr
            consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
        });

        _ = builder.Services.AddHttpClient(); // Register HttpClient for dependency injection

        _ = builder.Services
            .AddMcpServer()
            .WithStdioServerTransport()
            .WithToolsFromAssembly();

        // Explicitly add appsettings.json to configuration using the base directory
        _ = builder.Configuration.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"), optional: false, reloadOnChange: true);

        await builder.Build().RunAsync();
    }
}

/// <summary>
/// Represents an MCP Server Tool that interacts with the Google Gemini API.
/// </summary>
[McpServerToolType]
public class GeminiTool
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GeminiTool> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GeminiTool"/> class.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="httpClientFactory">The HTTP client factory for creating HttpClient instances.</param>
    /// <param name="logger">The logger for logging information and errors.</param>
    public GeminiTool(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<GeminiTool> logger)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Interacts with the Google Gemini API to generate content based on a prompt.
    /// </summary>
    /// <param name="prompt">The text prompt to send to the Gemini API.</param>
    /// <returns>A string containing the response from the Gemini API, or an error message.</returns>
    [McpServerTool, Description("Interacts with the Google Gemini API.")]
    public async Task<string> AskGemini(string prompt)
    {
        string? apiKey = _configuration["GeminiApi:GEMINI_API_KEY"];
        string? modelId = _configuration["GeminiApi:MODEL_ID"];
        string? generateContentApi = _configuration["GeminiApi:GENERATE_CONTENT_API"];

        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(modelId) || string.IsNullOrEmpty(generateContentApi))
        {
            _logger.LogError("Gemini API configuration is missing. Check appsettings.json for GeminiApi:GEMINI_API_KEY, GeminiApi:MODEL_ID, and GeminiApi:GENERATE_CONTENT_API.");
            return "Gemini API configuration is missing.";
        }

        string requestUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{modelId}:{generateContentApi}?key={apiKey}";

        var requestBody = new
        {
            contents = new[]
            {
                new { role = "user", parts = new[] { new { text = prompt } } }
            },
            generationConfig = new
            {
                temperature = 0,
                responseMimeType = "text/plain"
            },
            tools = new object[] // Explicitly type the array elements as object
            {
                new { url_context = new { } },
                new { google_search = new { } }
            }
        };

        string jsonBody = JsonSerializer.Serialize(requestBody);
        StringContent content = new(jsonBody, Encoding.UTF8, "application/json");

        using HttpClient httpClient = _httpClientFactory.CreateClient();

        try
        {
            HttpResponseMessage response = await httpClient.PostAsync(requestUrl, content);
            string responseBody = await response.Content.ReadAsStringAsync();

            // Check for success status code
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Gemini API HTTP Error: {StatusCode}. Response Body: {ResponseBody}", response.StatusCode, responseBody);
                // Attempt to parse the error from the response body
                try
                {
                    using JsonDocument doc = JsonDocument.Parse(responseBody);
                    if (doc.RootElement.TryGetProperty("error", out JsonElement errorElement) &&
                        errorElement.TryGetProperty("message", out JsonElement messageElement))
                    {
                        string errorMessage = messageElement.ToString();
                        _logger.LogError("Gemini API Error Message: {ErrorMessage}", errorMessage);
                        return $"Gemini API Error ({response.StatusCode}): {errorMessage}";
                    }
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "Failed to parse Gemini API error response body.");
                    // Ignore JSON parsing errors, return generic HTTP error
                }

                // If parsing failed or no specific error message found in body
                return $"Gemini API HTTP Error: {response.StatusCode}";
            }

            // If success status code, return the body
            return responseBody;
        }
        catch (HttpRequestException e)
        {
            _logger.LogError(e, "Error calling Gemini API.");
            return $"Error calling Gemini API: {e.Message}";
        }
    }
}
