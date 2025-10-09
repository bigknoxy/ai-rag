using AiRag.Api.Adapters;

namespace AiRag.Api.Services;

public class LiveEmbeddingProvider : IEmbeddingProvider
{
    private readonly string _host;

    public LiveEmbeddingProvider(Microsoft.Extensions.Configuration.IConfiguration cfg)
    {
        _host = cfg["Embedding:LiveHost"] ?? "http://127.0.0.1:8001";
    }

    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        // Minimal implementation that calls local embedding service - note: tests should not use this provider
        using var http = new System.Net.Http.HttpClient();
        var resp = await http.PostAsJsonAsync(new System.Uri(new Uri(_host), "/embed"), new { text });
        resp.EnsureSuccessStatusCode();
        var obj = await resp.Content.ReadFromJsonAsync<float[]>();
        return obj ?? Array.Empty<float>();
    }
}
