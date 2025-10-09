namespace AiRag.Api.Models;

public class Chunk
{
    public string Id { get; set; } = System.Guid.NewGuid().ToString();
    public string DocumentId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public int StartOffset { get; set; }
    public int EndOffset { get; set; }
    public object? Metadata { get; set; }
    public System.DateTime CreatedAt { get; set; } = System.DateTime.UtcNow;
}
