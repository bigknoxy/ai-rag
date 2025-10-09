using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using AiRag.Api.Adapters;

namespace AiRag.Api.Services;

public class LlamaCppAdapter : ILLMAdapter
{
    public string ProviderName => "LlamaCpp";
    public string ModelName { get; private set; }
    public bool IsStreamingSupported => true;

    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public LlamaCppAdapter(string modelName, string baseUrl = "http://localhost:8080")
    {
        ModelName = modelName;
        _baseUrl = baseUrl;
        _httpClient = new HttpClient { BaseAddress = new Uri(_baseUrl) };
    }

    public async Task<string> GenerateAsync(string prompt, int maxTokens = 512)
    {
        var request = new
        {
            prompt = prompt,
            max_tokens = maxTokens,
            stream = false
        };

        var response = await _httpClient.PostAsJsonAsync("/completion", request);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
        return jsonResponse.GetProperty("content").GetString() ?? string.Empty;
    }

    public async IAsyncEnumerable<string> GenerateStreamingAsync(string prompt, int maxTokens = 512)
    {
        var request = new
        {
            prompt = prompt,
            max_tokens = maxTokens,
            stream = true
        };

        var response = await _httpClient.PostAsync("/completion", JsonContent.Create(request));
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrEmpty(line) || !line.StartsWith("data: ")) continue;

            var data = line.Substring(6);
            if (data == "[DONE]") break;

            var json = JsonDocument.Parse(data);
            if (json.RootElement.TryGetProperty("content", out var contentProp))
            {
                var token = contentProp.GetString();
                if (!string.IsNullOrEmpty(token))
                {
                    yield return token;
                }
            }
        }
    }
}