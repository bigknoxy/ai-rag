using System.Threading.Tasks;
using Xunit;

namespace Tests.Unit;

public class TestFileVectorStore_GetChunks
{
    [Fact]
    public async Task GetChunksAsync_ReturnsChunksForExistingRecords()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), "ai-rag-test-" + System.Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmpDir);
        try
        {
            var filePathDir = Path.Combine(tmpDir, "samples");
            Directory.CreateDirectory(filePathDir);
            var filePath = Path.Combine(filePathDir, "vectors.json");
            var recs = new System.Collections.Generic.List<AiRag.Api.Models.EmbeddingRecord>
            {
                new AiRag.Api.Models.EmbeddingRecord { ChunkId = "c1", Vector = new float[] { 0.1f }, Chunk = new AiRag.Api.Models.Chunk { Id = "c1", DocumentId = "doc-1", Text = "Text1" } },
                new AiRag.Api.Models.EmbeddingRecord { ChunkId = "c2", Vector = new float[] { 0.2f }, Chunk = new AiRag.Api.Models.Chunk { Id = "c2", DocumentId = "doc-2", Text = "Text2" } }
            };
            File.WriteAllText(filePath, System.Text.Json.JsonSerializer.Serialize(recs));

            var orig = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(tmpDir);
            try
            {
                var store = new AiRag.Api.Services.FileVectorStore();
                var chunks = await store.GetChunksAsync(new[] { "c1" });
                Assert.Single(chunks);
                Assert.Equal("c1", chunks[0].Id);
                Assert.Equal("doc-1", chunks[0].DocumentId);
            }
            finally
            {
                Directory.SetCurrentDirectory(orig);
            }
        }
        finally
        {
            Directory.Delete(tmpDir, true);
        }
    }
}
