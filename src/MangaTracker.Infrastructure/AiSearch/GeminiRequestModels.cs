using System.Text.Json.Serialization;

namespace MangaTracker.Infrastructure.AiSearch;

// --- Lo que MANDAMOS ---
public class GeminiGenerateRequest
{
    [JsonPropertyName("contents")]
    public List<GeminiContent> Contents { get; set; } = [];

    [JsonPropertyName("generationConfig")]
    public GeminiGenerationConfig GenerationConfig { get; set; } = new();
}

public class GeminiContent
{
    [JsonPropertyName("parts")]
    public List<GeminiPart> Parts { get; set; } = [];
}

public class GeminiPart
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

public class GeminiGenerationConfig
{
    [JsonPropertyName("temperature")]
    public double Temperature { get; set; } = 0.1; // baja: queremos consistencia, no creatividad

    [JsonPropertyName("responseMimeType")]
    public string ResponseMimeType { get; set; } = "application/json"; // exigimos JSON

    [JsonPropertyName("responseSchema")]
    public object ResponseSchema { get; set; } = new(); // el "molde" exacto que tiene que cumplir
}

// --- Lo que RECIBIMOS ---
public class GeminiGenerateResponse
{
    [JsonPropertyName("candidates")]
    public List<GeminiCandidate> Candidates { get; set; } = [];
}

public class GeminiCandidate
{
    [JsonPropertyName("content")]
    public GeminiContent Content { get; set; } = new();
}