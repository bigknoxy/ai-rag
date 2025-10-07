using System.Threading.Tasks;
using AiRag.Api.Models;

namespace AiRag.Api.Adapters;

public interface IVectorStore
{
    Task SaveAsync(EmbeddingRecord record);
}
