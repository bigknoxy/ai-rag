using Microsoft.AspNetCore.Mvc;
using AiRag.Api.Models;
using AiRag.Api.Adapters;

namespace AiRag.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IngestController : ControllerBase
{
    private readonly IEmbeddingProvider _embeddingProvider;
    private readonly IVectorStore _vectorStore;

    public IngestController(IEmbeddingProvider embeddingProvider, IVectorStore vectorStore)
    {
        _embeddingProvider = embeddingProvider;
        _vectorStore = vectorStore;
    }

    public class IngestRequest
    {
        public List<Document> Documents { get; set; } = new List<Document>();
    }

    [HttpPost]
    public async Task<IActionResult> Post(IngestRequest req)
    {
        if (req.Documents == null || req.Documents.Count == 0)
            return BadRequest(new { error = "documents required" });

        int ingested = 0;
        foreach (var doc in req.Documents)
        {
            if (string.IsNullOrWhiteSpace(doc.Text)) continue;
            var chunk = new Chunk { DocumentId = doc.Id ?? System.Guid.NewGuid().ToString(), Text = doc.Text, StartOffset = 0, EndOffset = doc.Text.Length };
            var vec = await _embeddingProvider.GetEmbeddingAsync(chunk.Text);
            var record = new EmbeddingRecord { ChunkId = chunk.Id, Vector = vec, Source = new { documentId = chunk.DocumentId, chunkId = chunk.Id } };
            await _vectorStore.SaveAsync(record);
            ingested++;
        }

        return Accepted(new { ingested });
    }
}
