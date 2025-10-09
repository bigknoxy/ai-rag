using AiRag.Api.Adapters;
using AiRag.Api.Models;
using System.Collections.Concurrent;

namespace AiRag.Api.Services;

public class InMemoryVectorStore : IVectorStore
{
    private readonly ConcurrentDictionary<string, EmbeddingRecord> _store = new();

    public Task SaveAsync(EmbeddingRecord record)
    {
        _store[record.ChunkId] = record;
        return Task.CompletedTask;
    }

    public Task<System.Collections.Generic.List<(string chunkId, double score)>> QueryAsync(float[] vector, int topK)
    {
        var results = new System.Collections.Generic.List<(string, double)>();
        foreach (var kv in _store)
        {
            var rec = kv.Value;
            var score = CosineSimilarity(vector, rec.Vector);
            results.Add((rec.ChunkId, score));
        }

        var ordered = results
            .OrderByDescending(r => r.Item2)
            .ThenBy(r => r.Item1) // stable tie-breaker by id
            .Take(topK)
            .ToList();

        return Task.FromResult(ordered);
    }

    public Task RemoveAsync(string id)
    {
        _store.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        _store.Clear();
        return Task.CompletedTask;
    }

    private static double CosineSimilarity(float[] a, float[] b)
    {
        if (a == null || b == null || a.Length == 0 || b.Length == 0) return 0.0;
        var min = Math.Min(a.Length, b.Length);
        double dot = 0, na = 0, nb = 0;
        for (int i = 0; i < min; i++)
        {
            dot += a[i] * b[i];
            na += a[i] * a[i];
            nb += b[i] * b[i];
        }
        if (na == 0 || nb == 0) return 0.0;
        return dot / (Math.Sqrt(na) * Math.Sqrt(nb));
    }
}
