using System.Text.Json.Serialization;

namespace InternetGrounding.Tools.Models;

public class Candidate
{
    [JsonPropertyName("content")]
    public Content? Content { get; set; }

    [JsonPropertyName("groundingMetadata")]
    public GroundingMetadata? GroundingMetadata { get; set; }

    [JsonPropertyName("urlContextMetadata")]
    public UrlContextMetadata? UrlContextMetadata { get; set; }
}

public class Content
{
    [JsonPropertyName("parts")]
    public List<Part>? Parts { get; set; }
}

public class GroundingChunk
{
    [JsonPropertyName("web")]
    public Web? Web { get; set; }
}

public class GroundingMetadata
{
    [JsonPropertyName("groundingChunks")]
    public List<GroundingChunk>? GroundingChunks { get; set; }

    [JsonPropertyName("groundingSupports")]
    public List<GroundingSupport>? GroundingSupports { get; set; }

    [JsonPropertyName("webSearchQueries")]
    public List<string>? WebSearchQueries { get; set; }
}

public class GroundingSupport
{
    [JsonPropertyName("segment")]
    public Segment? Segment { get; set; }
}

public class Part
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

public class GeminiResponse
{
    [JsonPropertyName("candidates")]
    public List<Candidate>? Candidates { get; set; }
}

public class Segment
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

public class UrlContextMetadata
{
    [JsonPropertyName("urlMetadata")]
    public List<UrlMetadatum>? UrlMetadata { get; set; }
}

public class UrlMetadatum
{
    [JsonPropertyName("retrievedUrl")]
    public string? RetrievedUrl { get; set; }
}

public class Web
{
    [JsonPropertyName("uri")]
    public string? Uri { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }
}
