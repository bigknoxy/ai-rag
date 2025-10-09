using System.Text.Json;
using AiRag.Api.Adapters;
using AiRag.Api.Models;

namespace AiRag.Api.Services;

public class PrecomputedEmbeddingProvider : IEmbeddingProvider
{
    private readonly Dictionary<string, float[]> _map = new();
    private readonly int _dimension;

    public PrecomputedEmbeddingProvider(Microsoft.Extensions.Configuration.IConfiguration cfg)
    {
        var configured = cfg["Embedding:PrecomputedPath"];
        var defaultCandidate = Path.Combine(Directory.GetCurrentDirectory(), "samples", "sample1.embeddings.json");
        var candidates = new List<string>();

        if (!string.IsNullOrWhiteSpace(configured)) candidates.Add(configured);
        candidates.Add(defaultCandidate);

        // also try app base directory (test host) and walk up parents to find samples/
        var appBase = AppContext.BaseDirectory ?? Directory.GetCurrentDirectory();
        candidates.Add(Path.Combine(appBase, "samples", "sample1.embeddings.json"));

        var dir = new DirectoryInfo(appBase);
        for (int i = 0; i < 6 && dir != null; i++)
        {
            var p = Path.Combine(dir.FullName, "samples", "sample1.embeddings.json");
            candidates.Add(p);
            dir = dir.Parent;
        }

        // pick first existing
        var path = candidates.FirstOrDefault(File.Exists);

        var configuredDim = int.TryParse(cfg["Embedding:Dimension"], out var d) ? d : -1;

        if (path == null)
            throw new FileNotFoundException($"Precomputed embeddings not found. Tried: {string.Join(';', candidates.Distinct())}");

        var txt = File.ReadAllText(path);
        var doc = JsonSerializer.Deserialize<Dictionary<string, float[]>>(txt);
        if (doc == null)
            throw new System.InvalidOperationException("Precomputed embeddings file could not be parsed");

        // If dimension is not configured, infer from first vector
        if (configuredDim > 0)
        {
            _dimension = configuredDim;
        }
        else
        {
            var first = doc.Values.FirstOrDefault(v => v != null && v.Length > 0);
            if (first == null)
                throw new System.InvalidOperationException("Precomputed embeddings file contains no vectors to infer dimension from");
            _dimension = first.Length;
        }

        foreach (var kv in doc)
        {
            if (kv.Value == null || kv.Value.Length != _dimension)
                throw new System.InvalidOperationException($"Embedding vector for {kv.Key} does not match expected dimension {_dimension}");
            _map[kv.Key] = kv.Value;
        }
    }

    public Task<float[]> GetEmbeddingAsync(string text)
    {
        // Deterministic mapping: if text contains known id return that vector, else return zero vector
        foreach (var k in _map.Keys)
        {
            if (text != null && text.Contains(k, System.StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(_map[k]);
        }
        return Task.FromResult(Enumerable.Repeat(0.0f, _dimension).ToArray());
    }
}
