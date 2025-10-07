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
}
