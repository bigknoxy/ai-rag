using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Tests.Integration;

public class TestIngestQuery : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TestIngestQuery(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task IngestThenQuery_ReturnsInsertedDocument()
    {
        var ingestJson = "{ \"documents\": [ { \"id\": \"doc-1\", \"text\": \"The quick brown fox\", \"metadata\": {} } ] }";
        var ingestContent = new StringContent(ingestJson, Encoding.UTF8, "application/json");
        var ingestResp = await _client.PostAsync("/api/ingest", ingestContent);
        Assert.Equal(System.Net.HttpStatusCode.Accepted, ingestResp.StatusCode);

        var queryJson = "{ \"text\": \"quick brown\", \"topK\": 1 }";
        var queryContent = new StringContent(queryJson, Encoding.UTF8, "application/json");
        var queryResp = await _client.PostAsync("/api/query", queryContent);
        Assert.Equal(System.Net.HttpStatusCode.OK, queryResp.StatusCode);

        var body = await queryResp.Content.ReadAsStringAsync();
        Assert.Contains("doc-1", body);
        // Ensure results include documentId in the returned JSON
        Assert.Contains("documentId", body);
    }

    [Fact]
    public async Task IngestThenQuery_ReturnsAssembledPassages()
    {
        var ingestJson = "{ \"documents\": [ { \"id\": \"doc-1\", \"text\": \"The quick brown fox\", \"metadata\": {} } ] }";
        var ingestContent = new StringContent(ingestJson, Encoding.UTF8, "application/json");
        var ingestResp = await _client.PostAsync("/api/ingest", ingestContent);
        Assert.Equal(System.Net.HttpStatusCode.Accepted, ingestResp.StatusCode);

        var queryJson = "{ \"text\": \"quick brown\", \"topK\": 1 }";
        var queryContent = new StringContent(queryJson, Encoding.UTF8, "application/json");
        var queryResp = await _client.PostAsync("/api/query", queryContent);
        Assert.Equal(System.Net.HttpStatusCode.OK, queryResp.StatusCode);

        var body = await queryResp.Content.ReadAsStringAsync();
Assert.Contains("assembled", body, System.StringComparison.OrdinalIgnoreCase); // PromptBuilder should include 'assembled'
         Assert.Contains("quick brown fox", body, System.StringComparison.OrdinalIgnoreCase); // Check that passage text appears
    }

    [Fact]
    public async Task QueryStream_ReturnsStreamingResponse()
    {
        var queryJson = "{ \"text\": \"quick brown\", \"topK\": 1 }";
        var queryContent = new StringContent(queryJson, Encoding.UTF8, "application/json");
        var queryResp = await _client.PostAsync("/api/query/stream", queryContent);
        Assert.Equal(System.Net.HttpStatusCode.OK, queryResp.StatusCode);

        var body = await queryResp.Content.ReadAsStringAsync();
        Assert.NotEmpty(body);
    }
}
