using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Tests.Contract;

public class TestQuery : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TestQuery(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostQuery_ReturnsResultsArray()
    {
        var json = "{ \"text\": \"Hello world\", \"topK\": 3 }";
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var resp = await _client.PostAsync("/api/query", content);
        Assert.Equal(System.Net.HttpStatusCode.OK, resp.StatusCode);
        var body = await resp.Content.ReadAsStringAsync();
        Assert.Contains("results", body);
    }

    [Fact]
    public async Task PostQuery_ReturnsResponse()
    {
        var json = "{ \"text\": \"Hello world\", \"topK\": 3 }";
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var resp = await _client.PostAsync("/api/query", content);
        Assert.Equal(System.Net.HttpStatusCode.OK, resp.StatusCode);
        var body = await resp.Content.ReadAsStringAsync();
        Assert.Contains("response", body);
    }
}
