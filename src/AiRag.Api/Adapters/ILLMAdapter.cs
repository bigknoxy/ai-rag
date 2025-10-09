using System.Collections.Generic;
using System.Threading.Tasks;

namespace AiRag.Api.Adapters;

public interface ILLMAdapter
{
    string ProviderName { get; }
    string ModelName { get; }
    bool IsStreamingSupported { get; }

    Task<string> GenerateAsync(string prompt, int maxTokens = 512);
    IAsyncEnumerable<string> GenerateStreamingAsync(string prompt, int maxTokens = 512);
}
