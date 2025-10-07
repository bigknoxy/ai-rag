using System.Text.Json;
using AiRag.Api.Adapters;

namespace AiRag.Api.Services;

public class PrecomputedEmbeddingProvider : IEmbeddingProvider
{
    private readonly string _path = Path.Combine(Directory.GetCurrentDirectory(), "samples", "sample1.embeddings.json");

    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        if (!File.Exists(_path))
        {
            // Fallback: return zero vector
            return Enumerable.Repeat(0.0f, 384).ToArray();
        }

        var txt = await File.ReadAllTextAsync(_path);
        var doc = JsonSerializer.Deserialize<Dictionary<string, float[]>>(txt);
        if (doc != null && doc.TryGetValue("sample-1", out var vec))
        {
            return vec;
        }

        return Enumerable.Repeat(0.0f, 384).ToArray();
    }
}
