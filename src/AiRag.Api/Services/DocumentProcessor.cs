using AiRag.Api.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace AiRag.Api.Services;

public class DocumentProcessor
{
    private readonly int _maxChunkSize;
    private readonly int _chunkOverlap;

    public DocumentProcessor(int maxChunkSize = 500, int chunkOverlap = 50)
    {
        _maxChunkSize = maxChunkSize;
        _chunkOverlap = chunkOverlap;
    }

    public async Task<List<Document>> ProcessFileAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is required");

        var content = await ReadFileContentAsync(file);
        var chunks = ChunkText(content);

        var documentId = Guid.NewGuid().ToString();
        var documents = new List<Document>();

        for (int i = 0; i < chunks.Count; i++)
        {
            documents.Add(new Document
            {
                Id = $"{documentId}_chunk_{i}",
                Text = chunks[i],
                Metadata = new
                {
                    FileName = file.FileName,
                    ChunkIndex = i,
                    TotalChunks = chunks.Count,
                    DocumentId = documentId,
                    FileSize = file.Length,
                    ContentType = file.ContentType
                }
            });
        }

        return documents;
    }

    public Task<List<Document>> ProcessTextAsync(string text, string fileName = "text-input")
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text is required");

        var chunks = ChunkText(text);
        var documentId = Guid.NewGuid().ToString();
        var documents = new List<Document>();

        for (int i = 0; i < chunks.Count; i++)
        {
            documents.Add(new Document
            {
                Id = $"{documentId}_chunk_{i}",
                Text = chunks[i],
                Metadata = new
                {
                    FileName = fileName,
                    ChunkIndex = i,
                    TotalChunks = chunks.Count,
                    DocumentId = documentId
                }
            });
        }

        return Task.FromResult(documents);
    }

    private async Task<string> ReadFileContentAsync(IFormFile file)
    {
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;

        using var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync();

        // Handle different file types
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return extension switch
        {
            ".md" or ".markdown" => ProcessMarkdown(content),
            ".txt" => content,
            _ => content // Default to plain text
        };
    }

    private string ProcessMarkdown(string content)
    {
        // Remove markdown syntax but keep the text structure
        var lines = content.Split('\n');
        var processedLines = new List<string>();

        foreach (var line in lines)
        {
            var processed = line;

            // Remove headers (# ## ###)
            processed = Regex.Replace(processed, @"^#{1,6}\s*", "");

            // Remove bold/italic **text** *text*
            processed = Regex.Replace(processed, @"\*\*(.*?)\*\*", "$1");
            processed = Regex.Replace(processed, @"\*(.*?)\*", "$1");

            // Remove links [text](url)
            processed = Regex.Replace(processed, @"\[(.*?)\]\(.*?\)", "$1");

            // Remove code blocks `code`
            processed = Regex.Replace(processed, @"`(.*?)`", "$1");

            // Remove list markers
            processed = Regex.Replace(processed, @"^[\s]*[-*+]\s*", "");
            processed = Regex.Replace(processed, @"^[\s]*\d+\.\s*", "");

            processedLines.Add(processed);
        }

        return string.Join("\n", processedLines);
    }

    private List<string> ChunkText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new List<string> { string.Empty };

        // Split by paragraphs first
        var paragraphs = text.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        var chunks = new List<string>();
        var currentChunk = new StringBuilder();

        foreach (var paragraph in paragraphs)
        {
            var trimmedParagraph = paragraph.Trim();
            if (string.IsNullOrEmpty(trimmedParagraph))
                continue;

            // If adding this paragraph exceeds the chunk size, start a new chunk
            if (currentChunk.Length + trimmedParagraph.Length > _maxChunkSize && currentChunk.Length > 0)
            {
                chunks.Add(currentChunk.ToString().Trim());
                currentChunk.Clear();

                // Add overlap from the end of the previous chunk
                var words = trimmedParagraph.Split(' ');
                var overlapWords = words.TakeLast(_chunkOverlap / 5); // Rough estimate of words
                currentChunk.Append(string.Join(" ", overlapWords)).Append(" ");

                // Add the rest of the paragraph
                var remainingWords = words.Skip(overlapWords.Count());
                trimmedParagraph = string.Join(" ", remainingWords);
            }

            currentChunk.AppendLine(trimmedParagraph);
        }

        // Add the last chunk if it has content
        if (currentChunk.Length > 0)
        {
            chunks.Add(currentChunk.ToString().Trim());
        }

        // If no chunks were created, create one with the original text
        if (chunks.Count == 0)
        {
            chunks.Add(text);
        }

        return chunks;
    }
}