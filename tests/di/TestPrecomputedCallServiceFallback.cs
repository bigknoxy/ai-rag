using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using AiRag.Api.Services;
using AiRag.Api.Adapters;
using AiRag.Api.Models;
using Xunit;

namespace Tests.Di
{
    public class DummyLiveProvider : IEmbeddingProvider
    {
        public Task<float[]> GetEmbeddingAsync(string text)
        {
            return Task.FromResult(new float[] { 0.1f, 0.2f, 0.3f });
        }
    }

    public class TestPrecomputedCallServiceFallback
    {
        [Fact]
        public async Task Precomputed_UsesLiveProvider_When_CallServiceEnabled()
        {
            // create a small temporary precomputed embeddings file with dimension 3
            var tmp = System.IO.Path.GetTempFileName();
            System.IO.File.WriteAllText(tmp, "{ \"dummy\": [0.0, 0.0, 0.0] }\n");
            var dict = new System.Collections.Generic.Dictionary<string, string?>
            {
                { "Embedding:Mode", "Precomputed" },
                { "Embedding:FallbackMode", "CallService" },
                { "Embedding:PrecomputedPath", tmp },
                { "Embedding:Dimension", "3" }
            };
            var config = new Microsoft.Extensions.Configuration.ConfigurationManager();
            foreach (var kv in dict) if (kv.Value != null) config[kv.Key] = kv.Value;
            var services = new ServiceCollection();
            services.Configure<EmbeddingOptions>(config.GetSection("Embedding"));
            services.AddSingleton<LiveEmbeddingProvider>();
            services.AddSingleton<IEmbeddingProvider>(sp =>
            {
                var cfg = sp.GetRequiredService<IConfiguration>();
                var opts = sp.GetRequiredService<IOptions<EmbeddingOptions>>();
                var live = new DummyLiveProvider();
                return new PrecomputedEmbeddingProvider(cfg, opts, live);
            });
            services.AddSingleton<IConfiguration>(config);

            var sp = services.BuildServiceProvider();
            var provider = sp.GetRequiredService<IEmbeddingProvider>();

            var emb = await provider.GetEmbeddingAsync("some text that will not match precomputed key");
            Assert.NotNull(emb);
            Assert.Equal(3, emb.Length);
            Assert.Equal(0.1f, emb[0]);
        }
    }
}
