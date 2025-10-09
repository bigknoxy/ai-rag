using System.ComponentModel.DataAnnotations;

namespace AiRag.Api.Models;

public class Document
{
    public string? Id { get; set; }
    public string? Source { get; set; }

    [Required]
    public string Text { get; set; } = string.Empty;

    public object? Metadata { get; set; }

    public System.DateTime CreatedAt { get; set; } = System.DateTime.UtcNow;

    public System.Collections.Generic.List<Chunk> Chunks { get; set; } = new System.Collections.Generic.List<Chunk>();
}
