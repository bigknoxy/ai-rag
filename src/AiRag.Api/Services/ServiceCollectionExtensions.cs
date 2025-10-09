using Microsoft.Extensions.DependencyInjection;
using AiRag.Api.Adapters;

namespace AiRag.Api.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAiRagPhase0(this IServiceCollection services, Microsoft.Extensions.Configuration.IConfiguration? cfg = null)
    {
        var config = cfg ?? new Microsoft.Extensions.Configuration.ConfigurationBuilder().AddEnvironmentVariables().Build();
        services.Configure<AiRag.Api.Models.EmbeddingOptions>(config.GetSection("Embedding"));
        var mode = config["Embedding:Mode"] ?? "Precomputed";

        if (string.Equals(mode, "Live", System.StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<AiRag.Api.Adapters.IEmbeddingProvider, LiveEmbeddingProvider>();
        }
        else
        {
            services.AddSingleton<AiRag.Api.Adapters.IEmbeddingProvider, PrecomputedEmbeddingProvider>();
        }

        // For now register in-memory vector store; file store kept as fallback
        services.AddSingleton<AiRag.Api.Adapters.IVectorStore, AiRag.Api.Services.InMemoryVectorStore>();
        services.AddSingleton<PromptBuilder>();
        return services;
    }
}
