namespace AiRag.Api.Models;

public class LLMOptions
{
    public bool Enabled { get; set; } = false;
    public string Provider { get; set; } = "Mock";
    public string Model { get; set; } = "MockModel";
    public string BaseUrl { get; set; } = "http://localhost:11434";
}