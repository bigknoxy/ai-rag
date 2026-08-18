using System;
using System.Linq;
using System.Threading.Tasks;
using AiRag.Api.Services;
using AiRag.Api.Models;
using Microsoft.Extensions.Options;
using Xunit;

public class TestPrecomputedFallback
{
    [Fact]
    public void GenerateDeterministic_IsDeterministic_AndNormalized()
    {
        var method = typeof(PrecomputedEmbeddingProvider)
            .GetMethod("GenerateDeterministicEmbedding", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
        var a = method.Invoke(null, new object[] { "hello world", 16, "s" }) as float[];
        var b = method.Invoke(null, new object[] { "hello world", 16, "s" }) as float[];
        Assert.NotNull(a);
        Assert.Equal(16, a.Length);
        Assert.Equal(a, b);
        double sum = 0;
        foreach (var v in a) sum += v * v;
        Assert.InRange(Math.Sqrt(sum), 1e-6, 2.0);
    }

    [Fact]
    public async Task Precomputed_StrictMode_Throws_WhenMissing()
    {
        var cfg = new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build();
        var opts = Options.Create(new EmbeddingOptions { FallbackMode = "Strict", Dimension = 8 });
        // Create a minimal embeddings file in temp
        var tmp = System.IO.Path.GetTempFileName();
        System.IO.File.WriteAllText(tmp, "{}\n");
        Environment.SetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER", "true");
        try
        {
            var tmpDict = new System.Collections.Generic.Dictionary<string, string?> { { "Embedding:PrecomputedPath", tmp }, { "Embedding:Dimension", "8" } };
            var cfgMgr = new Microsoft.Extensions.Configuration.ConfigurationManager();
            foreach (var kv in tmpDict) if (kv.Value != null) cfgMgr[kv.Key] = kv.Value;
            cfg = cfgMgr;
            var provider = new PrecomputedEmbeddingProvider(cfg, opts);
            await Assert.ThrowsAsync<PrecomputedEmbeddingMissingException>(() => provider.GetEmbeddingAsync("missing-key"));
        }
        finally
        {
            try { System.IO.File.Delete(tmp); } catch { }
        }
    }

    [Fact]
    public async Task Precomputed_DeterministicFallback_WhenMissing()
    {
        var cfg = new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build();
        var opts = Options.Create(new EmbeddingOptions { FallbackMode = "Deterministic", Dimension = 8, Deterministic = new EmbeddingOptions.DeterministicOptions { Dimension = 8, Salt = "s" } });
        var tmp = System.IO.Path.GetTempFileName();
        System.IO.File.WriteAllText(tmp, "{}\n");
        try
        {
            var tmpDict = new System.Collections.Generic.Dictionary<string, string?> { { "Embedding:PrecomputedPath", tmp }, { "Embedding:Dimension", "8" } };
            var cfgMgr = new Microsoft.Extensions.Configuration.ConfigurationManager();
            foreach (var kv in tmpDict) if (kv.Value != null) cfgMgr[kv.Key] = kv.Value;
            cfg = cfgMgr;
            var provider = new PrecomputedEmbeddingProvider(cfg, opts);
            var emb = await provider.GetEmbeddingAsync("missing-key");
            Assert.NotNull(emb);
            Assert.Equal(8, emb.Length);
            double sum = 0; foreach (var v in emb) sum += v * v;
            Assert.InRange(Math.Sqrt(sum), 1e-6, 2.0);
        }
        finally
        {
            try { System.IO.File.Delete(tmp); } catch { }
        }
    }
}
