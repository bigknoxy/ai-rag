using Microsoft.Extensions.DependencyInjection;
using AiRag.Api.Adapters;

namespace AiRag.Api.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAiRagPhase0(this IServiceCollection services, Microsoft.Extensions.Configuration.IConfiguration? cfg = null)
    {
        var config = cfg ?? new Microsoft.Extensions.Configuration.ConfigurationBuilder().AddEnvironmentVariables().Build();
        services.Configure<AiRag.Api.Models.EmbeddingOptions>(config.GetSection("Embedding"));
        services.Configure<AiRag.Api.Models.LLMOptions>(config.GetSection("LLM"));
        // Make IConfiguration available to services
        services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(config);
        var mode = config["Embedding:Mode"] ?? "Precomputed";

        // Always register the concrete LiveEmbeddingProvider so Precomputed provider can optionally call it as a fallback
        services.AddSingleton<LiveEmbeddingProvider>();

        if (string.Equals(mode, "Live", System.StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<AiRag.Api.Adapters.IEmbeddingProvider>(sp => sp.GetRequiredService<LiveEmbeddingProvider>());
        }
        else
        {
            // When using Precomputed provider, if a LiveEmbeddingProvider is registered we pass it as an optional fallback
            services.AddSingleton<AiRag.Api.Adapters.IEmbeddingProvider>(sp =>
            {
                var cfg = sp.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
                var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AiRag.Api.Models.EmbeddingOptions>>();
                var live = sp.GetService<LiveEmbeddingProvider>();
                // Logging is optional here so callers can use minimal DI containers (e.g. unit tests).
                var logger = sp.GetService<Microsoft.Extensions.Logging.ILogger<PrecomputedEmbeddingProvider>>();
                return new PrecomputedEmbeddingProvider(cfg, opts, live, logger);
            });
        }

        // Register LLM adapter based on config
        var llmOptions = config.GetSection("LLM").Get<AiRag.Api.Models.LLMOptions>() ?? new AiRag.Api.Models.LLMOptions();
        switch (llmOptions.Provider.ToLower())
        {
            case "ollama":
                services.AddSingleton<AiRag.Api.Adapters.ILLMAdapter>(sp => new OllamaAdapter(llmOptions.Model, llmOptions.BaseUrl));
                break;
            case "llamacpp":
                services.AddSingleton<AiRag.Api.Adapters.ILLMAdapter>(sp => new LlamaCppAdapter(llmOptions.Model, llmOptions.BaseUrl));
                break;
            default:
                services.AddSingleton<AiRag.Api.Adapters.ILLMAdapter, MockLLMAdapter>();
                break;
        }

        // For now register in-memory vector store; file store kept as fallback
        services.AddSingleton<AiRag.Api.Adapters.IVectorStore, AiRag.Api.Services.InMemoryVectorStore>();
        services.AddSingleton<PromptBuilder>();
        services.AddSingleton<DocumentProcessor>();
        return services;
    }
}
