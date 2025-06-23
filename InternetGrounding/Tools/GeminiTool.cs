using InternetGrounding.Tools.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text;
using System.Text.Json;

namespace InternetGrounding.Tools;

[McpServerToolType]
public class GeminiTool(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<GeminiTool> logger)
{
    private readonly IConfiguration _configuration = configuration;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILogger<GeminiTool> _logger = logger;

    [McpServerTool, Description("Gemini will be grounded with Google Search to provide more accurate answers.")]
    public async Task<string> AskGemini(string prompt)
    {
        string? apiKey = _configuration["GeminiApi:GEMINI_API_KEY"];
        string? modelId = _configuration["GeminiApi:MODEL_ID"];

        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(modelId))
        {
            _logger.LogError("Gemini API configuration is missing. Ensure GeminiApi:GEMINI_API_KEY and GeminiApi:MODEL_ID are provided via configuration sources (e.g., command line arguments or environment variables).");
            return "Gemini API configuration is missing.";
        }

        string requestUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{modelId}:generateContent?key={apiKey}";

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
            tools = new object[]
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

            if (response.IsSuccessStatusCode)
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

                    _logger.LogError("Gemini API returned success but response body was not in expected format or empty. Response Body: {ResponseBody}", responseBody);
                    return "Gemini API returned an unexpected successful response format.";
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "Failed to parse successful Gemini API response body. Response Body: {ResponseBody}", responseBody);
                    return $"Failed to parse successful Gemini API response: {jsonEx.Message}";
                }
            }
            else
            {
                _logger.LogError("Gemini API HTTP Error: {StatusCode}. Response Body: {ResponseBody}", response.StatusCode, responseBody);
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
                    _logger.LogError(jsonEx, "Failed to parse Gemini API error response body. Response Body: {ResponseBody}", responseBody);
                    return $"Gemini API HTTP Error ({response.StatusCode}): Failed to parse error details from response body.";
                }

                return $"Gemini API HTTP Error: {response.StatusCode}";
            }
        }
        catch (HttpRequestException e)
        {
            _logger.LogError(e, "Error calling Gemini API.");
            return $"Error calling Gemini API: {e.Message}";
        }
    }
}
