global using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AiRag.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddAiRagPhase0(builder.Configuration);

var app = builder.Build();

// Startup validations: fail-fast in CI if embedding mode is not Precomputed
var env = app.Environment.EnvironmentName ?? "";
var embeddingOptions = app.Services.GetService<Microsoft.Extensions.Options.IOptions<AiRag.Api.Models.EmbeddingOptions>>()?.Value ?? new AiRag.Api.Models.EmbeddingOptions();
if (string.Equals(env, "CI", System.StringComparison.OrdinalIgnoreCase) && !string.Equals(embeddingOptions.Mode, "Precomputed", System.StringComparison.OrdinalIgnoreCase))
{
    throw new System.InvalidOperationException("In CI environment Embedding:Mode must be Precomputed to avoid network calls.");
}

// If using Precomputed provider validate the configured file exists and vectors match dimension
if (string.Equals(embeddingOptions.Mode, "Precomputed", System.StringComparison.OrdinalIgnoreCase))
{
    try
    {
        // Construct provider to validate file/dimension — will throw if invalid
        var provider = app.Services.GetRequiredService<AiRag.Api.Adapters.IEmbeddingProvider>();
    }
    catch (System.Exception ex)
    {
        throw new System.InvalidOperationException("Precomputed embeddings provider failed validation at startup: " + ex.Message, ex);
    }
}

// Add request logging middleware
app.UseMiddleware<AiRag.Api.Services.RequestLoggingMiddleware>();

app.MapControllers();

app.MapGet("/", () => Results.Ok(new { status = "ok" }));

app.Run();

