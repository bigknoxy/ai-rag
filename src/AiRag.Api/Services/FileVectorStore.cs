using System.Text.Json;
using AiRag.Api.Adapters;
using AiRag.Api.Models;

namespace AiRag.Api.Services;

public class FileVectorStore : IVectorStore
{
    private readonly string _path = Path.Combine(Directory.GetCurrentDirectory(), "samples", "vectors.json");
    private readonly object _lock = new();

    public async Task SaveAsync(EmbeddingRecord record)
    {
        lock (_lock)
        {
            List<EmbeddingRecord> list;
            if (File.Exists(_path))
            {
                var txt = File.ReadAllText(_path);
                list = string.IsNullOrWhiteSpace(txt) ? new List<EmbeddingRecord>() : JsonSerializer.Deserialize<List<EmbeddingRecord>>(txt) ?? new List<EmbeddingRecord>();
            }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_path) ?? ".");
                list = new List<EmbeddingRecord>();
            }

            list.Add(record);
            File.WriteAllText(_path, JsonSerializer.Serialize(list));
        }

        await Task.CompletedTask;
    }

    public Task<System.Collections.Generic.List<(string chunkId, double score)>> QueryAsync(float[] vector, int topK)
    {
        List<EmbeddingRecord> list;
        lock (_lock)
        {
            if (!File.Exists(_path))
            {
                list = new List<EmbeddingRecord>();
            }
            else
            {
                var txt = File.ReadAllText(_path);
                list = string.IsNullOrWhiteSpace(txt) ? new List<EmbeddingRecord>() : JsonSerializer.Deserialize<List<EmbeddingRecord>>(txt) ?? new List<EmbeddingRecord>();
            }
        }

        var results = new System.Collections.Generic.List<(string, double)>();
        foreach (var rec in list)
        {
            var score = CosineSimilarity(vector, rec.Vector);
            results.Add((rec.ChunkId, score));
        }

        var ordered = results
            .OrderByDescending(r => r.Item2)
            .ThenBy(r => r.Item1)
            .Take(topK)
            .ToList();

        return Task.FromResult(ordered);
    }

    public Task RemoveAsync(string id)
    {
        lock (_lock)
        {
            if (!File.Exists(_path)) return Task.CompletedTask;
            var txt = File.ReadAllText(_path);
            var list = string.IsNullOrWhiteSpace(txt) ? new List<EmbeddingRecord>() : JsonSerializer.Deserialize<List<EmbeddingRecord>>(txt) ?? new List<EmbeddingRecord>();
            var newList = list.Where(r => r.ChunkId != id).ToList();
            File.WriteAllText(_path, JsonSerializer.Serialize(newList));
        }

        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        lock (_lock)
        {
            File.WriteAllText(_path, JsonSerializer.Serialize(new List<EmbeddingRecord>()));
        }

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
