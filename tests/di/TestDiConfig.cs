using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using AiRag.Api.Services;
using AiRag.Api.Adapters;
using Xunit;

namespace Tests.Di;

public class TestDiConfig
{
    [Fact]
    public void PrecomputedMode_RegistersPrecomputedProvider()
    {
        var dict = new System.Collections.Generic.Dictionary<string, string?>
        {
            { "Embedding:Mode", "Precomputed" },
            { "Embedding:PrecomputedPath", System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "samples", "sample1.embeddings.json") }
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
        var services = new ServiceCollection();
        services.AddAiRagPhase0(config);
        var sp = services.BuildServiceProvider();
        var provider = sp.GetService<IEmbeddingProvider>();
        Assert.NotNull(provider);
        Assert.IsType<PrecomputedEmbeddingProvider>(provider);
    }

    [Fact]
    public void LiveMode_RegistersLiveProvider()
    {
        var dict = new System.Collections.Generic.Dictionary<string, string?>
        {
            { "Embedding:Mode", "Live" },
            { "Embedding:LiveHost", "http://127.0.0.1:8001" }
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
        var services = new ServiceCollection();
        services.AddAiRagPhase0(config);
        var sp = services.BuildServiceProvider();
        var provider = sp.GetService<IEmbeddingProvider>();
        Assert.NotNull(provider);
        Assert.IsType<LiveEmbeddingProvider>(provider);
    }
}
