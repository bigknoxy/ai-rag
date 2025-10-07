using System;
using System.Text.Json.Serialization;

namespace AiRag.Api.Models;

public class EmbeddingRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ChunkId { get; set; } = string.Empty;
    public float[] Vector { get; set; } = Array.Empty<float>();
    public object? Source { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
