using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using AiRag.Api.Adapters;

namespace AiRag.Api.Services;

public class OllamaAdapter : ILLMAdapter
{
    public string ProviderName => "Ollama";
    public string ModelName { get; private set; }
    public bool IsStreamingSupported => true;

    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public OllamaAdapter(string modelName, string baseUrl = "http://localhost:11434")
    {
        ModelName = modelName;
        _baseUrl = baseUrl;
        _httpClient = new HttpClient { BaseAddress = new Uri(_baseUrl) };
    }

    public async Task<string> GenerateAsync(string prompt, int maxTokens = 512)
    {
        var request = new
        {
            model = ModelName,
            prompt = prompt,
            max_tokens = maxTokens,
            stream = false
        };

        var response = await _httpClient.PostAsJsonAsync("/api/generate", request);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
        return jsonResponse.GetProperty("response").GetString() ?? string.Empty;
    }

    public async IAsyncEnumerable<string> GenerateStreamingAsync(string prompt, int maxTokens = 512)
    {
        var request = new
        {
            model = ModelName,
            prompt = prompt,
            max_tokens = maxTokens,
            stream = true
        };

        var response = await _httpClient.PostAsync("/api/generate", JsonContent.Create(request));
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrEmpty(line)) continue;

            var json = JsonDocument.Parse(line);
            if (json.RootElement.TryGetProperty("response", out var responseProp))
            {
                var token = responseProp.GetString();
                if (!string.IsNullOrEmpty(token))
                {
                    yield return token;
                }
            }

            if (json.RootElement.GetProperty("done").GetBoolean())
            {
                break;
            }
        }
    }
}