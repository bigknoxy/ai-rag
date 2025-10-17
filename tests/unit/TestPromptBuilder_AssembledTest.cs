using Xunit;

namespace Tests.Unit;

public class TestPromptBuilder_AssembledTest
{
    [Fact]
    public void AssemblePassages_IncludesAssembledPrefix()
    {
        var builder = new AiRag.Api.Services.PromptBuilder();
        var results = new[] { (chunkId: "c1", score: 1.0) };
        var chunks = new[] { new AiRag.Api.Models.Chunk { Id = "c1", Text = "Sample text" } };
        var assembled = builder.AssemblePassages(results, chunks, "q");
        Assert.Contains("Assembled passages", assembled, System.StringComparison.OrdinalIgnoreCase);
    }
}
