namespace InternetGrounding.Tools.Models;

/// <summary>
/// Represents the configuration options for the Gemini API.
/// </summary>
public class GeminiApiOptions
{
    public const string GeminiApi = "GeminiApi";

    /// <summary>
    /// Gets or sets the API key for accessing the Gemini API.
    /// </summary>
    public string? GEMINI_API_KEY { get; set; }

    /// <summary>
    /// Gets or sets the model ID to be used for Gemini API requests.
    /// </summary>
    public string? MODEL_ID { get; set; }
}