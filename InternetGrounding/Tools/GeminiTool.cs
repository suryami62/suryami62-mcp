using InternetGrounding.Tools.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net;
using System.Text;
using System.Text.Json;

namespace InternetGrounding.Tools;

/// <summary>
/// Provides functionality to interact with the Gemini API for generating content with internet grounding.
/// </summary>
[McpServerToolType]
public class GeminiTool
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GeminiTool> _logger;
    private readonly GeminiApiOptions _geminiApiOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="GeminiTool"/> class.
    /// </summary>
    /// <param name="geminiApiOptions">The options for configuring the Gemini API, including API key and model ID.</param>
    /// <param name="httpClientFactory">The factory used to create <see cref="HttpClient"/> instances.</param>
    /// <param name="logger">The logger for recording diagnostic information.</param>
    /// <exception cref="InvalidOperationException">Thrown if the Gemini API key or model ID is missing from the configuration.</exception>
    public GeminiTool(IOptions<GeminiApiOptions> geminiApiOptions, IHttpClientFactory httpClientFactory, ILogger<GeminiTool> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _geminiApiOptions = geminiApiOptions.Value;

        if (string.IsNullOrEmpty(_geminiApiOptions.GEMINI_API_KEY) || string.IsNullOrEmpty(_geminiApiOptions.MODEL_ID))
        {
            _logger.LogError("Gemini API configuration is missing. Please ensure 'GeminiApi:GEMINI_API_KEY' and 'GeminiApi:MODEL_ID' are provided in the application's configuration.");
            throw new InvalidOperationException("Gemini API configuration is missing.");
        }
    }

    /// <summary>
    /// Sends a prompt to the Gemini API and returns the generated content with grounding information.
    /// </summary>
    /// <param name="prompt">The user prompt to send to the Gemini API.</param>
    /// <returns>The response content formatted as a string, including grounding metadata if available.</returns>
    [McpServerTool, Description("Gemini will be grounded with Google Search to provide more accurate answers.")]
    public async Task<string> AskGemini(string prompt)
    {
        string requestUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{_geminiApiOptions.MODEL_ID}:generateContent?key={_geminiApiOptions.GEMINI_API_KEY}";
        object requestBody = CreateRequestBody(prompt);
        StringContent content = new(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        using HttpClient httpClient = _httpClientFactory.CreateClient();
        _logger.LogInformation("Sending request to Gemini API for prompt: {Prompt}", prompt);

        string responseBody = string.Empty;

        try
        {
            HttpResponseMessage response = await httpClient.PostAsync(requestUrl, content);
            responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Received response from Gemini API with status code: {StatusCode}", response.StatusCode);

            return !response.IsSuccessStatusCode
                ? throw ParseErrorResponse(response.StatusCode, responseBody)
                : ParseSuccessResponse(responseBody);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError(e, "Network error occurred while calling Gemini API for prompt: {Prompt}", prompt);
            throw new GeminiApiException($"Network error calling Gemini API: {e.Message}", e);
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "Failed to parse Gemini API response body. Response Body: {ResponseBody}", responseBody);
            throw new GeminiApiException($"Failed to parse Gemini API response: {jsonEx.Message}", jsonEx);
        }
        catch (GeminiApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while calling Gemini API for prompt: {Prompt}", prompt);
            throw new GeminiApiException($"An unexpected error occurred: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Creates the request body for the Gemini API call, including the user prompt and grounding tools.
    /// </summary>
    /// <param name="prompt">The user prompt to be included in the request.</param>
    /// <returns>An anonymous object representing the JSON request body for the Gemini API.</returns>
    private static object CreateRequestBody(string prompt)
    {
        return new
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

            tools = new object[]
            {
                new { url_context = new { } },
                new { google_search = new { } }
            }
        };
    }

    /// <summary>
    /// Parses a successful JSON response from the Gemini API and extracts the generated content and grounding information.
    /// </summary>
    /// <param name="responseBody">The raw JSON response body from the Gemini API.</param>
    /// <returns>A formatted string containing the main generated text and any available grounding metadata (web search queries, chunks, and URL context).</returns>
    /// <exception cref="GeminiApiException">Thrown if the response body cannot be deserialized or if the expected content/grounding information is missing.</exception>
    private string ParseSuccessResponse(string responseBody)
    {
        try
        {
            GeminiResponse? geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseBody);

            if (geminiResponse?.Candidates != null && geminiResponse.Candidates.Count > 0)
            {
                Candidate firstCandidate = geminiResponse.Candidates[0];
                StringBuilder resultBuilder = new();

                if (firstCandidate.Content?.Parts != null && firstCandidate.Content.Parts.Count > 0)
                {
                    Part firstPart = firstCandidate.Content.Parts[0];

                    if (firstPart != null && !string.IsNullOrEmpty(firstPart.Text))
                    {
                        _ = resultBuilder.AppendLine("Main Text:");
                        _ = resultBuilder.AppendLine(firstPart.Text);
                    }
                }

                if (firstCandidate.GroundingMetadata != null)
                {
                    _ = resultBuilder.AppendLine("\nGrounding Information:");

                    if (firstCandidate.GroundingMetadata.WebSearchQueries != null && firstCandidate.GroundingMetadata.WebSearchQueries.Count > 0)
                    {
                        _ = resultBuilder.AppendLine("Web Search Queries:");

                        foreach (string query in firstCandidate.GroundingMetadata.WebSearchQueries)
                        {
                            _ = resultBuilder.AppendLine($"- {query}");
                        }
                    }

                    if (firstCandidate.GroundingMetadata.GroundingChunks != null && firstCandidate.GroundingMetadata.GroundingChunks.Count > 0)
                    {
                        _ = resultBuilder.AppendLine("Grounding Chunks:");

                        foreach (GroundingChunk chunk in firstCandidate.GroundingMetadata.GroundingChunks)
                        {
                            if (chunk.Web != null)
                            {
                                _ = resultBuilder.AppendLine($"- URI: {chunk.Web.Uri}, Title: {chunk.Web.Title}");
                            }
                        }
                    }

                    if (firstCandidate.GroundingMetadata.GroundingSupports != null && firstCandidate.GroundingMetadata.GroundingSupports.Count > 0)
                    {
                        _ = resultBuilder.AppendLine("Grounding Supports:");

                        foreach (GroundingSupport support in firstCandidate.GroundingMetadata.GroundingSupports)
                        {
                            if (support.Segment != null)
                            {
                                _ = resultBuilder.AppendLine($"- Text: {support.Segment.Text}");
                            }
                        }
                    }
                }

                if (firstCandidate.UrlContextMetadata != null && firstCandidate.UrlContextMetadata.UrlMetadata != null && firstCandidate.UrlContextMetadata.UrlMetadata.Count > 0)
                {
                    _ = resultBuilder.AppendLine("\nURL Context Metadata:");
                    _ = resultBuilder.AppendLine("Retrieved URLs:");

                    foreach (UrlMetadatum urlMeta in firstCandidate.UrlContextMetadata.UrlMetadata)
                    {
                        if (!string.IsNullOrEmpty(urlMeta.RetrievedUrl))
                        {
                            _ = resultBuilder.AppendLine($"- {urlMeta.RetrievedUrl}");
                        }
                    }
                }

                if (resultBuilder.Length > 0)
                {
                    return resultBuilder.ToString();
                }
            }

            _logger.LogError("Gemini API returned a successful status code, but the response body was empty or not in the expected format. Response Body: {ResponseBody}", responseBody);

            return "Gemini API returned an unexpected successful response format.";
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "Failed to deserialize successful Gemini API response body. Response Body: {ResponseBody}", responseBody);
            throw new GeminiApiException($"Failed to parse successful Gemini API response: {jsonEx.Message}", jsonEx);
        }
    }

    /// <summary>
    /// Parses an error JSON response from the Gemini API and constructs a <see cref="GeminiApiException"/>.
    /// </summary>
    /// <param name="statusCode">The HTTP status code of the error response.</param>
    /// <param name="responseBody">The raw JSON error response body from the Gemini API.</param>
    /// <returns>A <see cref="GeminiApiException"/> containing details about the error.</returns>
    private GeminiApiException ParseErrorResponse(HttpStatusCode statusCode, string responseBody)
    {
        _logger.LogError("Gemini API returned an HTTP error. Status Code: {StatusCode}, Response Body: {ResponseBody}", statusCode, responseBody);

        try
        {
            using JsonDocument doc = JsonDocument.Parse(responseBody);

            if (doc.RootElement.TryGetProperty("error", out JsonElement errorElement) &&
                errorElement.TryGetProperty("message", out JsonElement messageElement))
            {
                string errorMessage = messageElement.ToString();
                _logger.LogError("Gemini API Error Details: {ErrorMessage}", errorMessage);

                return new GeminiApiException($"Gemini API Error ({statusCode}): {errorMessage}", statusCode);
            }
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "Failed to parse Gemini API error response body. Response Body: {ResponseBody}", responseBody);

            return new GeminiApiException($"Gemini API HTTP Error ({statusCode}): Failed to parse error details from response body.", statusCode, jsonEx);
        }

        return new GeminiApiException($"Gemini API HTTP Error: {statusCode}. No specific error message found in response.", statusCode);
    }
}
