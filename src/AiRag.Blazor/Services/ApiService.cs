using AiRag.Blazor.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace AiRag.Blazor.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            // Resolve API base URL from environment (docker-compose sets `Api__BaseUrl`)
            var envBase = Environment.GetEnvironmentVariable("Api__BaseUrl") ?? "http://localhost:8000";
            if (!envBase.EndsWith("/")) envBase += "/";

            // Ensure the client points to the API's /api/ path
            var apiBase = new Uri(new Uri(envBase), "api/");
            _httpClient.BaseAddress = apiBase;
            Console.WriteLine($"[ApiService] BaseAddress set to: {_httpClient.BaseAddress}");
        }

    public async Task<IngestResponse> IngestAsync(IngestRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("ingest", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IngestResponse>() ?? new IngestResponse();
    }

    public async Task<QueryResponse> QueryAsync(QueryRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("query", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<QueryResponse>() ?? new QueryResponse();
    }

    public async IAsyncEnumerable<string> QueryStreamAsync(QueryRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("query/stream", request);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (!string.IsNullOrEmpty(line))
            {
                yield return line;
            }
        }
    }
}

public class IngestResponse
{
    public int Ingested { get; set; }
}

public class QueryResponse
{
    public List<QueryResult> Results { get; set; } = new();
    public string Response { get; set; } = string.Empty;
    public bool LlmUsed { get; set; }
}

public class QueryResult
{
    public string ChunkId { get; set; } = string.Empty;
    public string DocumentId { get; set; } = string.Empty;
    public double Score { get; set; }
    public object Metadata { get; set; } = new();
}