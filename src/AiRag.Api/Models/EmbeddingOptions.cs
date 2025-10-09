namespace AiRag.Api.Models;

public class EmbeddingOptions
{
    public string Mode { get; set; } = "Precomputed"; // Precomputed | Live
    public string PrecomputedPath { get; set; } = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "samples", "sample1.embeddings.json");
    public int Dimension { get; set; } = 384;
    public string LiveHost { get; set; } = "http://127.0.0.1:8001";
    public int QueryTopK { get; set; } = 5;
}
