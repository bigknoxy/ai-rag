using AiRag.Api.Models;

namespace AiRag.Api.Services;

public class PromptBuilder
{
    public virtual string AssemblePassages(IEnumerable<(string chunkId, double score)> results, IEnumerable<Chunk> chunks, string query)
    {
        var relevantChunks = results
            .Join(chunks, r => r.chunkId, c => c.Id, (r, c) => c)
            .OrderByDescending(c => results.First(res => res.chunkId == c.Id).score)
            .ToList();

        if (!relevantChunks.Any())
            return "No relevant passages found.";

        var assembled = $"Query: {query}\n\nPassages:\n" + string.Join("\n\n", relevantChunks.Select(c => c.Text));
        return assembled;
    }
}