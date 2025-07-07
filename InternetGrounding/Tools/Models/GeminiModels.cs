using System.Text.Json.Serialization;

namespace InternetGrounding.Tools.Models;

/// <summary>
/// Represents a response candidate from the Gemini API, containing content and metadata.
/// </summary>
public class Candidate
{
    [JsonPropertyName("content")]
    public Content? Content { get; set; }

    [JsonPropertyName("groundingMetadata")]
    public GroundingMetadata? GroundingMetadata { get; set; }

    [JsonPropertyName("urlContextMetadata")]
    public UrlContextMetadata? UrlContextMetadata { get; set; }
}

/// <summary>
/// Contains parts of content returned by the Gemini API.
/// </summary>
public class Content
{
    [JsonPropertyName("parts")]
    public List<Part>? Parts { get; set; }
}

/// <summary>
/// Represents a chunk of grounding information, often linked to web resources.
/// </summary>
public class GroundingChunk
{
    [JsonPropertyName("web")]
    public Web? Web { get; set; }
}

/// <summary>
/// Metadata related to grounding information for API responses.
/// </summary>
public class GroundingMetadata
{
    [JsonPropertyName("groundingChunks")]
    public List<GroundingChunk>? GroundingChunks { get; set; }

    [JsonPropertyName("groundingSupports")]
    public List<GroundingSupport>? GroundingSupports { get; set; }

    [JsonPropertyName("webSearchQueries")]
    public List<string>? WebSearchQueries { get; set; }
}

/// <summary>
/// Support information for grounding, often containing text segments.
/// </summary>
public class GroundingSupport
{
    [JsonPropertyName("segment")]
    public Segment? Segment { get; set; }
}

/// <summary>
/// A part of content, typically containing text data.
/// </summary>
public class Part
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

/// <summary>
/// The root response object from the Gemini API, containing a list of candidates.
/// </summary>
public class GeminiResponse
{
    [JsonPropertyName("candidates")]
    public List<Candidate>? Candidates { get; set; }
}

/// <summary>
/// A segment of text used in grounding support.
/// </summary>
public class Segment
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

/// <summary>
/// Metadata related to URLs used in the context of the API response.
/// </summary>
public class UrlContextMetadata
{
    [JsonPropertyName("urlMetadata")]
    public List<UrlMetadatum>? UrlMetadata { get; set; }
}

/// <summary>
/// Information about a retrieved URL in the API response context.
/// </summary>
public class UrlMetadatum
{
    [JsonPropertyName("retrievedUrl")]
    public string? RetrievedUrl { get; set; }
}

/// <summary>
/// Web resource information used for grounding, including URI and title.
/// </summary>
public class Web
{
    [JsonPropertyName("uri")]
    public string? Uri { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }
}
