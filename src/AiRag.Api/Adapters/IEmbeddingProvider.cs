using System.Threading.Tasks;

namespace AiRag.Api.Adapters;

public interface IEmbeddingProvider
{
    Task<float[]> GetEmbeddingAsync(string text);
}
