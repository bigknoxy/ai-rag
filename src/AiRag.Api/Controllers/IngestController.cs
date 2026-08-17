using Microsoft.AspNetCore.Mvc;
using AiRag.Api.Models;
using AiRag.Api.Adapters;
using AiRag.Api.Services;

namespace AiRag.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IngestController : ControllerBase
{
    private readonly IEmbeddingProvider _embeddingProvider;
    private readonly IVectorStore _vectorStore;
    private readonly DocumentProcessor _documentProcessor;
    private readonly ILogger<IngestController> _logger;

    public IngestController(
        IEmbeddingProvider embeddingProvider,
        IVectorStore vectorStore,
        DocumentProcessor documentProcessor,
        ILogger<IngestController> logger)
    {
        _embeddingProvider = embeddingProvider;
        _vectorStore = vectorStore;
        _documentProcessor = documentProcessor;
        _logger = logger;
    }

    public class IngestRequest
    {
        public List<Document> Documents { get; set; } = new List<Document>();
    }

    public class TextInputRequest
    {
        public string Text { get; set; } = string.Empty;
        public string FileName { get; set; } = "text-input";
    }

    // Support file upload as shown in quickstart
    [HttpPost]
    public async Task<IActionResult> Post([FromForm] IFormFile? file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "file required" });

            var documents = await _documentProcessor.ProcessFileAsync(file);
            _logger.LogInformation("Processing file: {FileName}, generated {ChunkCount} chunks", file.FileName, documents.Count);
            return await IngestAsync(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during ingestion");
            return StatusCode(500, new { error = "Internal server error during ingestion", details = ex.Message });
        }
    }

    // Support direct text input
    [HttpPost("text")]
    public async Task<IActionResult> PostText([FromBody] TextInputRequest req)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(req.Text))
                return BadRequest(new { error = "text required" });

            var documents = await _documentProcessor.ProcessTextAsync(req.Text, req.FileName);
            return await IngestAsync(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during text ingestion");
            return StatusCode(500, new { error = "Internal server error during text ingestion", details = ex.Message });
        }
    }

    // Support JSON ingestion for backward compatibility
    [HttpPost("json")]
    public async Task<IActionResult> PostJson([FromBody] IngestRequest req)
    {
        try
        {
            if (req.Documents == null || req.Documents.Count == 0)
                return BadRequest(new { error = "documents required" });

            return await IngestAsync(req.Documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during JSON ingestion");
            return StatusCode(500, new { error = "Internal server error during JSON ingestion", details = ex.Message });
        }
    }

    // Get ingestion status
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        // This would typically check a background job queue
        // For now, return a simple status
        return Ok(new
        {
            status = "ready",
            mode = "direct",
            timestamp = DateTime.UtcNow
        });
    }

    private async Task<IActionResult> IngestAsync(List<Document> documents)
    {
        int ingested = 0;
        foreach (var doc in documents)
        {
            if (string.IsNullOrWhiteSpace(doc.Text)) continue;

            var chunkId = doc.Id ?? Guid.NewGuid().ToString();
            var documentId = ResolveDocumentId(doc);
            var record = new EmbeddingRecord
            {
                ChunkId = chunkId,
                Vector = await _embeddingProvider.GetEmbeddingAsync(doc.Text),
                Chunk = new Chunk
                {
                    Id = chunkId,
                    DocumentId = documentId,
                    Text = doc.Text,
                    StartOffset = 0,
                    EndOffset = doc.Text.Length
                },
                Source = doc.Metadata ?? new { documentId = doc.Id, chunkId = doc.Id }
            };

            await _vectorStore.SaveAsync(record);
            ingested++;
        }

        _logger.LogInformation("Successfully ingested {Ingested} of {Total} chunks", ingested, documents.Count);
        return Accepted(new { ingested, documents = documents.Count });
    }

    private static string ResolveDocumentId(Document doc)
    {
        var metadata = doc.Metadata;
        var value = metadata?.GetType()
            .GetProperty("DocumentId")?
            .GetValue(metadata)?.ToString();
        return string.IsNullOrWhiteSpace(value) ? (doc.Id ?? Guid.NewGuid().ToString()) : value;
    }
}
