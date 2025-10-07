using Microsoft.Extensions.DependencyInjection;
using AiRag.Api.Adapters;

namespace AiRag.Api.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAiRagPhase0(this IServiceCollection services)
    {
        services.AddSingleton<IEmbeddingProvider, PrecomputedEmbeddingProvider>();
        services.AddSingleton<IVectorStore, FileVectorStore>();
        return services;
    }
}
