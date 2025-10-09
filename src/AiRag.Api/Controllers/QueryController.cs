using Microsoft.AspNetCore.Mvc;
using AiRag.Api.Adapters;
using AiRag.Api.Models;

namespace AiRag.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QueryController : ControllerBase
{
    private readonly IVectorStore _vectorStore;

    public class QueryRequest
    {
        public string Text { get; set; } = string.Empty;
        public int TopK { get; set; } = 5;
    }

    public QueryController(IVectorStore vectorStore)
    {
        _vectorStore = vectorStore;
    }

    [HttpPost]
    public async Task<IActionResult> Post(QueryRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Text)) return BadRequest(new { error = "text required" });

        // For now, use a zero-vector embedding for the query; in tests PrecomputedEmbeddingProvider may be used via direct vector entries
        var embedding = Enumerable.Repeat(0.0f, 384).ToArray();
        var results = await _vectorStore.QueryAsync(embedding, req.TopK);

        var outList = results.Select(r => new { chunkId = r.chunkId, score = r.score, metadata = new { } }).ToList();
        return Ok(new { results = outList });
    }
}
