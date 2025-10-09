using Microsoft.AspNetCore.Mvc;
using AiRag.Api.Adapters;
using AiRag.Api.Models;
using AiRag.Api.Services;
using Microsoft.Extensions.Options;

namespace AiRag.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QueryController : ControllerBase
{
    private readonly IVectorStore _vectorStore;
    private readonly IEmbeddingProvider _embeddingProvider;
    private readonly PromptBuilder _promptBuilder;
    private readonly EmbeddingOptions _options;

    public class QueryRequest
    {
        public string Text { get; set; } = string.Empty;
        public int TopK { get; set; } = 5;
    }

    public QueryController(IVectorStore vectorStore, IEmbeddingProvider embeddingProvider, PromptBuilder promptBuilder, IOptions<EmbeddingOptions> options)
    {
        _vectorStore = vectorStore;
        _embeddingProvider = embeddingProvider;
        _promptBuilder = promptBuilder;
        _options = options.Value;
    }

    [HttpPost]
    public async Task<IActionResult> Post(QueryRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Text)) return BadRequest(new { error = "text required" });

        var topK = req.TopK > 0 ? req.TopK : _options.QueryTopK;
        var embedding = await _embeddingProvider.GetEmbeddingAsync(req.Text);
        var results = await _vectorStore.QueryAsync(embedding, topK);
        var chunkIds = results.Select(r => r.chunkId);
        var chunks = await _vectorStore.GetChunksAsync(chunkIds);
        var assembled = _promptBuilder.AssemblePassages(results, chunks, req.Text);

        var outList = results.Select(r => new { chunkId = r.chunkId, score = r.score, metadata = new { } }).ToList();
        return Ok(new { results = outList, assembled });
    }
}
