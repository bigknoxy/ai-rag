using System.Text.Json.Serialization;

namespace AiRag.Blazor.Models;

public class IngestRequest
{
    [JsonPropertyName("documents")]
    public List<Document> Documents { get; set; } = new();
}

public class Document
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}