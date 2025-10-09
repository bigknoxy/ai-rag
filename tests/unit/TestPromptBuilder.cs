using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Unit;

public class TestPromptBuilder
{
    [Fact]
    public void AssemblePassages_FormatsMultipleChunks()
    {
        var builder = new AiRag.Api.Services.PromptBuilder();
        var results = new[]
        {
            (chunkId: "c1", score: 0.9),
            (chunkId: "c2", score: 0.8)
        };
        var chunks = new[]
        {
            new AiRag.Api.Models.Chunk { Id = "c1", Text = "First chunk" },
            new AiRag.Api.Models.Chunk { Id = "c2", Text = "Second chunk" }
        };
        var assembled = builder.AssemblePassages(results, chunks, "test query");
        Assert.Contains("First chunk", assembled);
        Assert.Contains("Second chunk", assembled);
        // This will fail until PromptBuilder is implemented
    }

    [Fact]
    public void AssemblePassages_HandlesEmptyResults()
    {
        var builder = new AiRag.Api.Services.PromptBuilder();
        var results = new (string, double)[0];
        var chunks = new AiRag.Api.Models.Chunk[0];
        var assembled = builder.AssemblePassages(results, chunks, "test query");
        Assert.Equal("No relevant passages found.", assembled);
        // This will fail until PromptBuilder is implemented
    }
}