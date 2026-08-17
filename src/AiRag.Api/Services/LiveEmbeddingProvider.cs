using AiRag.Api.Adapters;
using AiRag.Api.Models;

namespace AiRag.Api.Services;

public class LiveEmbeddingProvider : IEmbeddingProvider
{
    private readonly string _host;
    private readonly EmbeddingOptions _options;
    private readonly Microsoft.Extensions.Logging.ILogger<LiveEmbeddingProvider> _logger;
    // The provider is registered as a DI singleton, so one HttpClient per instance is safe
    // and avoids socket exhaustion from per-request clients.
    private readonly System.Net.Http.HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(5) };

    public LiveEmbeddingProvider(
        Microsoft.Extensions.Configuration.IConfiguration cfg,
        Microsoft.Extensions.Options.IOptions<EmbeddingOptions> options,
        Microsoft.Extensions.Logging.ILogger<LiveEmbeddingProvider>? logger = null)
    {
        _host = cfg["Embedding:LiveHost"] ?? "http://127.0.0.1:8001";
        _options = options.Value;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<LiveEmbeddingProvider>.Instance;
    }

    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        try
        {
            // Call the local embedding service and be tolerant of different response shapes.
            var resp = await _http.PostAsJsonAsync(new System.Uri(new Uri(_host), "/embed"), new { text });
            resp.EnsureSuccessStatusCode();
            var content = await resp.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(content))
            {
                var emb = TryParseResponse(content);
                if (emb != null && emb.Length > 0)
                    return emb;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Live embedding service at {Host} unavailable", _host);
        }

        // Service unavailable or returned no usable data.
        if (IsStrict())
        {
            throw new InvalidOperationException($"Live embedding service at {_host} is unavailable and FallbackMode is Strict");
        }

        _logger.LogInformation("Falling back to deterministic embedding for missing live embedding");
        return FallbackEmbedding(text);
    }

    public float[] FallbackEmbedding(string text)
    {
        var dimension = _options.Deterministic?.Dimension > 0
            ? _options.Deterministic.Dimension
            : (_options.Dimension > 0 ? _options.Dimension : 384);
        var salt = _options.Deterministic?.Salt ?? string.Empty;
        return DeterministicEmbedding.Generate(text ?? string.Empty, dimension, salt);
    }

    private bool IsStrict()
        => string.Equals((_options.FallbackMode ?? string.Empty).Trim(), "Strict", System.StringComparison.OrdinalIgnoreCase);

    private static float[]? TryParseResponse(string content)
    {
        // Try deserializing into the expected object shape first, then try an array of floats.
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        try
        {
            var obj = System.Text.Json.JsonSerializer.Deserialize<EmbeddingResponse>(content, options);
            if (obj?.embedding != null && obj.embedding.Length > 0)
                return obj.embedding;
        }
        catch { /* fall through and try other shapes */ }

        try
        {
            var arr = System.Text.Json.JsonSerializer.Deserialize<float[]>(content, options);
            if (arr != null && arr.Length > 0)
                return arr;
        }
        catch { }

        // Last resort: attempt to parse a top-level property named 'vector' or 'embeddings'
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("vector", out var vecProp) || doc.RootElement.TryGetProperty("embeddings", out vecProp))
            {
                if (vecProp.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    var list = new List<float>();
                    foreach (var el in vecProp.EnumerateArray())
                    {
                        if (el.TryGetSingle(out var v)) list.Add(v);
                        else if (el.TryGetDouble(out var d)) list.Add((float)d);
                    }
                    if (list.Count > 0) return list.ToArray();
                }
            }
        }
        catch { }

        return null;
    }

    private class EmbeddingResponse
    {
        public float[] embedding { get; set; } = Array.Empty<float>();
        public int dimension { get; set; }
    }
}
