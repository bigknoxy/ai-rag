using System.Threading.Tasks;
using Xunit;

namespace Tests.Unit;

public class TestFileVectorStore
{
    [Fact]
    public async Task SaveAsync_WritesFile()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), "ai-rag-test-" + System.Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmpDir);
        try
        {
            var filePath = Path.Combine(tmpDir, "samples", "vectors.json");
            // Create a FileVectorStore that points to the temp directory by overriding current dir
            var orig = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(tmpDir);
            try
            {
                var store = new AiRag.Api.Services.FileVectorStore();
                var rec = new AiRag.Api.Models.EmbeddingRecord { ChunkId = "c1", Vector = new float[] { 0.1f, 0.2f } };
                await store.SaveAsync(rec);
                Assert.True(File.Exists(filePath));
                var txt = File.ReadAllText(filePath);
                Assert.Contains("c1", txt);
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

        await Task.CompletedTask;
    }
}
