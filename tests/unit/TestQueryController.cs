using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Tests.Unit;

public class TestQueryController
{
    [Fact]
    public async Task Post_UsesEmbeddingProviderForQuery()
    {
        var mockVectorStore = new Mock<AiRag.Api.Adapters.IVectorStore>();
        var mockEmbeddingProvider = new Mock<AiRag.Api.Adapters.IEmbeddingProvider>();
        var mockLlmAdapter = new Mock<AiRag.Api.Adapters.ILLMAdapter>();
        var mockPromptBuilder = new Mock<AiRag.Api.Services.PromptBuilder>();
        var embeddingOptions = Options.Create(new AiRag.Api.Models.EmbeddingOptions { QueryTopK = 3 });
        var llmOptions = Options.Create(new AiRag.Api.Models.LLMOptions { Enabled = false });

        mockEmbeddingProvider.Setup(p => p.GetEmbeddingAsync("test query")).ReturnsAsync(new float[] { 0.1f, 0.2f });
        mockVectorStore.Setup(v => v.QueryAsync(It.IsAny<float[]>(), 3)).ReturnsAsync(new System.Collections.Generic.List<(string, double)> { ("c1", 0.9) });
        mockVectorStore.Setup(v => v.GetChunksAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(new System.Collections.Generic.List<AiRag.Api.Models.Chunk>());
        mockPromptBuilder.Setup(p => p.AssemblePassages(It.IsAny<IEnumerable<(string, double)>>(), It.IsAny<IEnumerable<AiRag.Api.Models.Chunk>>(), "test query")).Returns("assembled");

        var controller = new AiRag.Api.Controllers.QueryController(mockVectorStore.Object, mockEmbeddingProvider.Object, mockLlmAdapter.Object, mockPromptBuilder.Object, embeddingOptions, llmOptions);

        var result = await controller.Post(new AiRag.Api.Controllers.QueryController.QueryRequest { Text = "test query", TopK = 3 });

        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = okResult.Value;
        Assert.NotNull(value?.GetType().GetProperty("results")?.GetValue(value));
        mockEmbeddingProvider.Verify(p => p.GetEmbeddingAsync("test query"), Times.Once);
    }

    [Fact]
    public async Task Post_UsesDefaultTopKFromOptions()
    {
        var mockVectorStore = new Mock<AiRag.Api.Adapters.IVectorStore>();
        var mockEmbeddingProvider = new Mock<AiRag.Api.Adapters.IEmbeddingProvider>();
        var mockLlmAdapter = new Mock<AiRag.Api.Adapters.ILLMAdapter>();
        var mockPromptBuilder = new Mock<AiRag.Api.Services.PromptBuilder>();
        var embeddingOptions = Options.Create(new AiRag.Api.Models.EmbeddingOptions { QueryTopK = 5 });
        var llmOptions = Options.Create(new AiRag.Api.Models.LLMOptions { Enabled = false });

        mockEmbeddingProvider.Setup(p => p.GetEmbeddingAsync("test")).ReturnsAsync(new float[] { 0.1f });
        mockVectorStore.Setup(v => v.QueryAsync(It.IsAny<float[]>(), 5)).ReturnsAsync(new System.Collections.Generic.List<(string, double)>());
        mockVectorStore.Setup(v => v.GetChunksAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(new System.Collections.Generic.List<AiRag.Api.Models.Chunk>());
        mockPromptBuilder.Setup(p => p.AssemblePassages(It.IsAny<IEnumerable<(string, double)>>(), It.IsAny<IEnumerable<AiRag.Api.Models.Chunk>>(), "test")).Returns("assembled");

        var controller = new AiRag.Api.Controllers.QueryController(mockVectorStore.Object, mockEmbeddingProvider.Object, mockLlmAdapter.Object, mockPromptBuilder.Object, embeddingOptions, llmOptions);

        var result = await controller.Post(new AiRag.Api.Controllers.QueryController.QueryRequest { Text = "test" }); // No TopK

        mockVectorStore.Verify(v => v.QueryAsync(It.IsAny<float[]>(), 5), Times.Once);
    }
}