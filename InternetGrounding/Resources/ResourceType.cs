using ModelContextProtocol.Server;
using System.ComponentModel;

namespace InternetGrounding.Resources;

/// <summary>
/// Provides various resources for the MCP server, including text templates and configuration data
/// related to Gemini API interactions and general purposes.
/// </summary>
[McpServerResourceType]
public class ResourceType
{
    // General purpose resource
    [McpServerResource, Description("A direct text resource")]
    public static string DirectTextResource()
    {
        return "This is a direct resource";
    }

    // Gemini API related resources
    /// <summary>
    /// Provides a template string for crafting prompts to be sent to the Gemini API.
    /// </summary>
    [McpServerResource, Description("A template for Gemini API prompts")]
    public static string GeminiPromptTemplate()
    {
        return "Generate content about {topic} with detailed information and references.";
    }

    /// <summary>
    /// Explains the purpose and usage of the 'url_context' tool in Gemini API requests for grounding.
    /// </summary>
    [McpServerResource, Description("Explanation of the 'url_context' tool for Gemini API grounding")]
    public static string GeminiUrlContextToolExplanation()
    {
        return "The 'url_context' tool allows the Gemini API to ground responses based on content from specified URLs.";
    }

    /// <summary>
    /// Explains the purpose and usage of the 'google_search' tool in Gemini API requests for grounding.
    /// </summary>
    [McpServerResource, Description("Explanation of the 'google_search' tool for Gemini API grounding")]
    public static string GeminiGoogleSearchToolExplanation()
    {
        return "The 'google_search' tool enables the Gemini API to perform Google searches for grounding information.";
    }

    /// <summary>
    /// Returns a JSON string representing default configuration settings for the Gemini API.
    /// </summary>
    [McpServerResource, Description("Default configuration settings for Gemini API")]
    public static string GeminiDefaultConfig()
    {
        return "{\"temperature\": 0, \"responseMimeType\": \"text/plain\"}";
    }

    /// <summary>
    /// Explains the structure and components of grounding information returned by the Gemini API.
    /// </summary>
    [McpServerResource, Description("Explanation of grounding information format from Gemini API")]
    public static string GroundingInfoExplanation()
    {
        return "Grounding information includes web search queries, grounding chunks (URIs and titles), and supporting text segments.";
    }
}
