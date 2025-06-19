using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text;
using System.Text.Json;

// Overview:
// This application sets up a Model Context Protocol (MCP) server to facilitate interaction
// with the Google Gemini API. It provides tools for sending prompts to the Gemini API
// and receiving responses, enabling AI-powered functionalities within the MCP framework.

internal class Program
{
    /// <summary>
    /// Main entry point for the application. Sets up and runs the MCP server host.
    /// </summary>
    /// <param name="args">Command-line arguments for configuration and runtime options.</param>
    /// <returns>A task representing the asynchronous operation of the host.</returns>
    private static async Task Main(string[] args)
    {
        // Initialize the host application builder with command-line arguments.
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

        // Set the content root path to the base directory of the application.
        builder.Environment.ContentRootPath = AppContext.BaseDirectory;

        // Configure logging to output to the console with a low threshold for detailed logs.
        _ = builder.Logging.AddConsole(consoleLogOptions =>
        {
            consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
        });

        // Add HTTP client services for making API requests.
        _ = builder.Services.AddHttpClient();

        // Configure the MCP server with standard I/O transport and register tools from the assembly.
        _ = builder.Services
            .AddMcpServer()
            .WithStdioServerTransport()
            .WithToolsFromAssembly();

        // Build and run the host asynchronously.
        await builder.Build().RunAsync();
    }
}

[McpServerToolType]
public class GeminiTool
{
    // Dependencies for configuration, HTTP client creation, and logging.
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GeminiTool> _logger;

    /// <summary>
    /// Initializes a new instance of the GeminiTool class with necessary dependencies.
    /// </summary>
    /// <param name="configuration">Configuration provider for API keys and settings.</param>
    /// <param name="httpClientFactory">Factory for creating HTTP clients.</param>
    /// <param name="logger">Logger for diagnostic and error information.</param>
    public GeminiTool(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<GeminiTool> logger)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Sends a prompt to the Google Gemini API and returns the response.
    /// This method constructs an API request with the provided prompt and handles the response or errors.
    /// </summary>
    /// <param name="prompt">The text prompt to send to the Gemini API.</param>
    /// <returns>The response from the Gemini API as a string, or an error message if the request fails.</returns>
    [McpServerTool, Description("Interacts with the Google Gemini API.")]
    public async Task<string> AskGemini(string prompt)
    {
        // Retrieve configuration values for the Gemini API.
        string? apiKey = _configuration["GeminiApi:GEMINI_API_KEY"];
        string? modelId = _configuration["GeminiApi:MODEL_ID"];
        string? generateContentApi = _configuration["GeminiApi:GENERATE_CONTENT_API"];

        // Check if required configuration values are missing and log an error if they are.
        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(modelId) || string.IsNullOrEmpty(generateContentApi))
        {
            _logger.LogError("Gemini API configuration is missing. Ensure GeminiApi:GEMINI_API_KEY, GeminiApi:MODEL_ID, and GeminiApi:GENERATE_CONTENT_API are provided via configuration sources (e.g., command line arguments, environment variables, or appsettings.json).");
            return "Gemini API configuration is missing.";
        }

        // Construct the API request URL using configuration values.
        string requestUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{modelId}:{generateContentApi}?key={apiKey}";

        // Define the request body with the prompt, generation configuration, and tools.
        var requestBody = new
        {
            contents = new[]
            {
                new { role = "user", parts = new[] { new { text = prompt } } }
            },
            generationConfig = new
            {
                temperature = 0, // Set temperature to 0 for deterministic responses.
                responseMimeType = "text/plain"
            },
            tools = new object[]
            {
                new { url_context = new { } }, // Placeholder for URL context tool.
                new { google_search = new { } } // Placeholder for Google Search tool.
            }
        };

        // Serialize the request body to JSON and prepare HTTP content.
        string jsonBody = JsonSerializer.Serialize(requestBody);
        StringContent content = new(jsonBody, Encoding.UTF8, "application/json");

        // Create an HTTP client for the API request.
        using HttpClient httpClient = _httpClientFactory.CreateClient();

        try
        {
            // Send the POST request to the Gemini API.
            HttpResponseMessage response = await httpClient.PostAsync(requestUrl, content);
            string responseBody = await response.Content.ReadAsStringAsync();

            // Check if the response indicates an error and handle it.
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Gemini API HTTP Error: {StatusCode}. Response Body: {ResponseBody}", response.StatusCode, responseBody);
                try
                {
                    // Attempt to parse error details from the response body.
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
                }

                // Return a generic error message if parsing fails.
                return $"Gemini API HTTP Error: {response.StatusCode}";
            }

            // Return the successful response body.
            return responseBody;
        }
        catch (HttpRequestException e)
        {
            // Log and return any network or HTTP request errors.
            _logger.LogError(e, "Error calling Gemini API.");
            return $"Error calling Gemini API: {e.Message}";
        }
    }
}
