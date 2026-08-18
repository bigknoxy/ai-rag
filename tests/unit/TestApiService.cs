using AiRag.Blazor.Services;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests;

public class TestApiService
{
    [Fact]
    public async Task IngestAsync_Success_ReturnsResponse()
    {
        // Arrange
        var request = new AiRag.Blazor.Models.IngestRequest
        {
            Documents = { new AiRag.Blazor.Models.Document { Text = "Test document" } }
        };

        var expectedResponse = new AiRag.Blazor.Services.IngestResponse { Ingested = 1 };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedResponse)
        };

        var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage)
            .Verifiable();

        var client = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var apiService = new ApiService(client);

        // Act
        var result = await apiService.IngestAsync(request);

        // Assert
        Assert.Equal(1, result.Ingested);

        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post && req.RequestUri != null),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task QueryAsync_Success_ReturnsResponse()
    {
        // Arrange
        var request = new AiRag.Blazor.Models.QueryRequest { Text = "Test query" };

        var expectedResponse = new AiRag.Blazor.Services.QueryResponse
        {
            Response = "Test response",
            LlmUsed = false,
            Results = { new AiRag.Blazor.Services.QueryResult { ChunkId = "1", Score = 0.9 } }
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedResponse)
        };

        var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage)
            .Verifiable();

        var client = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var apiService = new ApiService(client);

        // Act
        var result = await apiService.QueryAsync(request);

        // Assert
        Assert.Equal("Test response", result.Response);
        Assert.Single(result.Results);
        Assert.Equal("1", result.Results.First().ChunkId);

        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post && req.RequestUri != null),
            ItExpr.IsAny<CancellationToken>());
    }
}
