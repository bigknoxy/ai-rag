using AiRag.Blazor.Services;
using Moq;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace UnitTests;

public class TestApiService
{
    [Fact]
    public async Task IngestAsync_Success_ReturnsResponse()
    {
        // Arrange
        var mockHttpClient = new Mock<HttpClient>();
        var apiService = new ApiService(mockHttpClient.Object);

        var request = new AiRag.Blazor.Models.IngestRequest
        {
            Documents = { new AiRag.Blazor.Models.Document { Text = "Test document" } }
        };

        var expectedResponse = new AiRag.Blazor.Services.IngestResponse { Ingested = 1 };

        mockHttpClient.Setup(c => c.PostAsJsonAsync("ingest", request, default))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(expectedResponse)
            });

        // Act
        var result = await apiService.IngestAsync(request);

        // Assert
        Assert.Equal(1, result.Ingested);
    }

    [Fact]
    public async Task QueryAsync_Success_ReturnsResponse()
    {
        // Arrange
        var mockHttpClient = new Mock<HttpClient>();
        var apiService = new ApiService(mockHttpClient.Object);

        var request = new AiRag.Blazor.Models.QueryRequest { Text = "Test query" };

        var expectedResponse = new AiRag.Blazor.Services.QueryResponse
        {
            Response = "Test response",
            LlmUsed = false,
            Results = { new AiRag.Blazor.Services.QueryResult { ChunkId = "1", Score = 0.9 } }
        };

        mockHttpClient.Setup(c => c.PostAsJsonAsync("query", request, default))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(expectedResponse)
            });

        // Act
        var result = await apiService.QueryAsync(request);

        // Assert
        Assert.Equal("Test response", result.Response);
        Assert.Single(result.Results);
        Assert.Equal("1", result.Results.First().ChunkId);
    }
}