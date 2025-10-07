using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Tests.Contract;

public class TestIngest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TestIngest(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostIngest_Returns200()
    {
        var json = "{ \"documents\": [ { \"id\": \"doc-1\", \"text\": \"Hello world\", \"metadata\": {} } ] }";
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var resp = await _client.PostAsync("/api/ingest", content);
        Assert.True(resp.IsSuccessStatusCode, "Expected successful status code");
        var body = await resp.Content.ReadAsStringAsync();
        Assert.Contains("ingested", body);
    }
}
