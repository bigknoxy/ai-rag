using System.Text.Json.Serialization;

namespace AiRag.Blazor.Models;

public class QueryRequest
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("topK")]
    public int TopK { get; set; } = 5;

    [JsonPropertyName("useLlm")]
    public bool? UseLlm { get; set; }
}