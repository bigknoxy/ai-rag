namespace AiRag.Api.Models;

public class EmbeddingVector
{
    public string Id { get; set; } = string.Empty; // maps to chunkId
    public float[] Vector { get; set; } = System.Array.Empty<float>();
    public int Dimension => Vector?.Length ?? 0;
    public string Source { get; set; } = "Precomputed"; // or Live

    public void Validate(int expectedDimension)
    {
        if (Dimension != expectedDimension)
            throw new System.InvalidOperationException($"Embedding vector dimension {Dimension} does not match expected {expectedDimension}");
    }
}
