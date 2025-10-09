using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AiRag.Api.Adapters;

namespace AiRag.Api.Services;

public class MockLLMAdapter : ILLMAdapter
{
    public string ProviderName => "Mock";
    public string ModelName => "MockModel";
    public bool IsStreamingSupported => true;

    private readonly List<string> _mockResponses = new()
    {
        "This is a mock response from the LLM adapter for testing purposes.",
        "Mock response: Retrieval-augmented generation combines retrieval with generation for better answers.",
        "Simulated LLM output: The system uses precomputed embeddings in CI mode."
    };

    private int _responseIndex = 0;

    public Task<string> GenerateAsync(string prompt, int maxTokens = 512)
    {
        var response = _mockResponses[_responseIndex % _mockResponses.Count];
        _responseIndex++;
        return Task.FromResult(response);
    }

    public async IAsyncEnumerable<string> GenerateStreamingAsync(string prompt, int maxTokens = 512)
    {
        var response = await GenerateAsync(prompt, maxTokens);
        var words = response.Split(' ');

        foreach (var word in words)
        {
            yield return word + " ";
            await Task.Delay(50); // Simulate streaming delay
        }
    }
}