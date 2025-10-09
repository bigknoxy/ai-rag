using System.Threading.Tasks;
using AiRag.Api.Models;

namespace AiRag.Api.Adapters;

public interface IVectorStore
{
    Task SaveAsync(EmbeddingRecord record);
    Task<System.Collections.Generic.List<(string chunkId, double score)>> QueryAsync(float[] vector, int topK);
    Task RemoveAsync(string id);
    Task ClearAsync();
}
