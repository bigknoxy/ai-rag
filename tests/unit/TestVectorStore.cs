using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Unit;

public class TestVectorStore
{
    [Fact]
    public async Task InMemory_Query_ReturnsDeterministicTopK()
    {
        var store = new AiRag.Api.Services.InMemoryVectorStore();
        await store.ClearAsync();

        // Insert three vectors
        await store.SaveAsync(new AiRag.Api.Models.EmbeddingRecord { ChunkId = "a", Vector = new float[] { 1f, 0f } });
        await store.SaveAsync(new AiRag.Api.Models.EmbeddingRecord { ChunkId = "b", Vector = new float[] { 0f, 1f } });
        await store.SaveAsync(new AiRag.Api.Models.EmbeddingRecord { ChunkId = "c", Vector = new float[] { 0.7071f, 0.7071f } });

        var query = new float[] { 1f, 0f };
        var res = await store.QueryAsync(query, 3);
        Assert.Equal(3, res.Count);
        // top should be 'a' then 'c' then 'b'
        Assert.Equal("a", res[0].chunkId);
        Assert.Equal("c", res[1].chunkId);
        Assert.Equal("b", res[2].chunkId);
    }

    [Fact]
    public async Task CosineSimilarity_TieBreaksById()
    {
        var store = new AiRag.Api.Services.InMemoryVectorStore();
        await store.ClearAsync();
        // two identical vectors but ids ensure deterministic ordering
        await store.SaveAsync(new AiRag.Api.Models.EmbeddingRecord { ChunkId = "aa", Vector = new float[] { 1f, 0f } });
        await store.SaveAsync(new AiRag.Api.Models.EmbeddingRecord { ChunkId = "ab", Vector = new float[] { 1f, 0f } });

        var res = await store.QueryAsync(new float[] { 1f, 0f }, 2);
        Assert.Equal(2, res.Count);
        Assert.Equal("aa", res[0].chunkId);
        Assert.Equal("ab", res[1].chunkId);
    }
}
