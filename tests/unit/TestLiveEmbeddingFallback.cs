using AiRag.Api.Models;
using AiRag.Api.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Xunit;

namespace Tests.Unit;

public class TestLiveEmbeddingFallback
{
    // A port on localhost that nothing listens on; connections are refused immediately.
    private const string UnreachableHost = "http://127.0.0.1:59999";

    private static LiveEmbeddingProvider CreateProvider(string fallbackMode, int deterministicDim = 16)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Embedding:LiveHost"] = UnreachableHost
            })
            .Build();
        var options = Options.Create(new EmbeddingOptions
        {
            Mode = "Live",
            FallbackMode = fallbackMode,
            Deterministic = new EmbeddingOptions.DeterministicOptions
            {
                Dimension = deterministicDim,
                Salt = "test"
            }
        });
        return new LiveEmbeddingProvider(config, options);
    }

    [Fact]
    public async Task Live_Unavailable_ReturnsDeterministicFallback()
    {
        var provider = CreateProvider("Deterministic");

        var a = await provider.GetEmbeddingAsync("hello world");
        var b = await provider.GetEmbeddingAsync("hello world");

        Assert.Equal(16, a.Length);
        Assert.Equal(a, b);
        var norm = Math.Sqrt(a.Sum(x => (double)x * x));
        Assert.InRange(norm, 1e-6, 2.0);
    }

    [Fact]
    public async Task Live_DifferentTexts_ReturnDifferentFallbacks()
    {
        var provider = CreateProvider("Deterministic");

        var a = await provider.GetEmbeddingAsync("alpha");
        var b = await provider.GetEmbeddingAsync("beta");

        Assert.NotEqual(a, b);
    }

    [Fact]
    public async Task Live_Strict_Unavailable_Throws()
    {
        var provider = CreateProvider("Strict");

        await Assert.ThrowsAsync<InvalidOperationException>(() => provider.GetEmbeddingAsync("hello"));
    }

    [Fact]
    public void Fallback_IsDeterministicAndNormalized()
    {
        var provider = CreateProvider("Deterministic");

        var a = provider.FallbackEmbedding("stable input");
        var b = provider.FallbackEmbedding("stable input");

        Assert.Equal(a, b);
        Assert.Equal(16, a.Length);
    }
}
