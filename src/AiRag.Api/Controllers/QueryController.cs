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
    private readonly ILLMAdapter _llmAdapter;
    private readonly PromptBuilder _promptBuilder;
    private readonly EmbeddingOptions _embeddingOptions;
    private readonly LLMOptions _llmOptions;

    public class QueryRequest
    {
        public string Text { get; set; } = string.Empty;
        public int TopK { get; set; } = 5;
        public bool? UseLlm { get; set; }
    }

    public QueryController(IVectorStore vectorStore, IEmbeddingProvider embeddingProvider, ILLMAdapter llmAdapter, PromptBuilder promptBuilder, IOptions<EmbeddingOptions> embeddingOptions, IOptions<LLMOptions> llmOptions)
    {
        _vectorStore = vectorStore;
        _embeddingProvider = embeddingProvider;
        _llmAdapter = llmAdapter;
        _promptBuilder = promptBuilder;
        _embeddingOptions = embeddingOptions.Value;
        _llmOptions = llmOptions.Value;
    }

    [HttpPost]
    public async Task<IActionResult> Post(QueryRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Text)) return BadRequest(new { error = "text required" });

        var topK = req.TopK > 0 ? req.TopK : _embeddingOptions.QueryTopK;
        var useLlm = req.UseLlm ?? _llmOptions.Enabled;

        var embedding = await _embeddingProvider.GetEmbeddingAsync(req.Text);
        var results = await _vectorStore.QueryAsync(embedding, topK);
        var chunkIds = results.Select(r => r.chunkId);
        var chunks = await _vectorStore.GetChunksAsync(chunkIds);

        string response;
        bool llmUsed = false;

        if (useLlm && _llmOptions.Enabled)
        {
            try
            {
                var context = _promptBuilder.AssemblePassages(results, chunks, req.Text);
                response = await _llmAdapter.GenerateAsync(context);
                // Format as markdown for LLM responses
                response = $"**LLM Response:**\n\n{response}\n\n**Sources:**\n" + string.Join("\n", results.Select(r => $"- Chunk {r.chunkId} (score: {r.score:F2})"));
                llmUsed = true;
            }
            catch
            {
                // Fallback to retrieval-only
                response = _promptBuilder.AssemblePassages(results, chunks, req.Text);
            }
        }
        else
        {
            response = _promptBuilder.AssemblePassages(results, chunks, req.Text);
        }

        var outList = results.Select(r => new { chunkId = r.chunkId, score = r.score, metadata = new { } }).ToList();
        return Ok(new { results = outList, response, llmUsed });
    }

    [HttpPost("stream")]
    public async IAsyncEnumerable<string> PostStream(QueryRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Text)) yield break;

        var topK = req.TopK > 0 ? req.TopK : _embeddingOptions.QueryTopK;
        var useLlm = req.UseLlm ?? _llmOptions.Enabled;

        var embedding = await _embeddingProvider.GetEmbeddingAsync(req.Text);
        var results = await _vectorStore.QueryAsync(embedding, topK);
        var chunkIds = results.Select(r => r.chunkId);
        var chunks = await _vectorStore.GetChunksAsync(chunkIds);

        if (useLlm && _llmOptions.Enabled && _llmAdapter.IsStreamingSupported)
        {
            var tokens = await GetLlmTokensAsync(results, chunks, req.Text);
            foreach (var token in tokens)
            {
                yield return token;
            }
        }
        else
        {
            var assembled = _promptBuilder.AssemblePassages(results, chunks, req.Text);
            yield return assembled;
        }
    }

    private async Task<List<string>> GetLlmTokensAsync(IEnumerable<(string chunkId, double score)> results, IEnumerable<Chunk> chunks, string query)
    {
        var tokens = new List<string>();
        var context = _promptBuilder.AssemblePassages(results, chunks, query);
        var stream = _llmAdapter.GenerateStreamingAsync(context);

        try
        {
            await foreach (var token in stream)
            {
                tokens.Add(token);
            }

            // Send sources at the end
            tokens.Add("\n\n**Sources:**\n");
            foreach (var r in results)
            {
                tokens.Add($"- Chunk {r.chunkId} (score: {r.score:F2})\n");
            }
        }
        catch
        {
            // Fallback to retrieval-only
            var assembled = _promptBuilder.AssemblePassages(results, chunks, query);
            tokens.Add(assembled);
        }

        return tokens;
    }
}
