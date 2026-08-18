using System.Text.Json;
using AiRag.Api.Adapters;
using AiRag.Api.Models;

namespace AiRag.Api.Services;

public class PrecomputedEmbeddingProvider : IEmbeddingProvider
{
    private readonly Dictionary<string, float[]> _map = new();
    private readonly int _dimension;
    private readonly EmbeddingOptions _options;
    private readonly IEmbeddingProvider? _liveProvider;
    private readonly Microsoft.Extensions.Logging.ILogger<PrecomputedEmbeddingProvider> _logger;
    private long _fallbackCount = 0;

    public PrecomputedEmbeddingProvider(
        Microsoft.Extensions.Configuration.IConfiguration cfg,
        Microsoft.Extensions.Options.IOptions<EmbeddingOptions> opts,
        IEmbeddingProvider? liveProvider = null,
        Microsoft.Extensions.Logging.ILogger<PrecomputedEmbeddingProvider>? logger = null)
    {
        _options = opts.Value;
        _liveProvider = liveProvider;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<PrecomputedEmbeddingProvider>.Instance;

        var configured = cfg["Embedding:PrecomputedPath"];
        var defaultCandidate = Path.Combine(Directory.GetCurrentDirectory(), "samples", "sample1.embeddings.json");
        var candidates = new List<string>();

        if (!string.IsNullOrWhiteSpace(configured)) candidates.Add(configured);
        candidates.Add(defaultCandidate);

        // also try app base directory (test host) and walk up parents to find samples/
        var appBase = AppContext.BaseDirectory ?? Directory.GetCurrentDirectory();
        candidates.Add(Path.Combine(appBase, "samples", "sample1.embeddings.json"));

        var dir = new DirectoryInfo(appBase);
        for (int i = 0; i < 6 && dir != null; i++)
        {
            var p = Path.Combine(dir.FullName, "samples", "sample1.embeddings.json");
            candidates.Add(p);
            dir = dir.Parent;
        }

        // pick first existing
        var path = candidates.FirstOrDefault(File.Exists);

        var configuredDim = _options.Dimension > 0 ? _options.Dimension : (int.TryParse(cfg["Embedding:Dimension"], out var d) ? d : -1);

        if (path == null)
            throw new FileNotFoundException($"Precomputed embeddings not found. Tried: {string.Join(';', candidates.Distinct())}");

        var txt = File.ReadAllText(path);
        var doc = JsonSerializer.Deserialize<Dictionary<string, float[]>>(txt);
        if (doc == null)
            throw new System.InvalidOperationException("Precomputed embeddings file could not be parsed");

        // If dimension is not configured, infer from first vector
        if (configuredDim > 0)
        {
            _dimension = configuredDim;
        }
        else
        {
            var first = doc.Values.FirstOrDefault(v => v != null && v.Length > 0);
            if (first == null)
                throw new System.InvalidOperationException("Precomputed embeddings file contains no vectors to infer dimension from");
            _dimension = first.Length;
        }

        foreach (var kv in doc)
        {
            if (kv.Value == null || kv.Value.Length != _dimension)
                throw new System.InvalidOperationException($"Embedding vector for {kv.Key} does not match expected dimension {_dimension}");
            _map[kv.Key] = kv.Value;
        }
    }

    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        // Normalize null inputs to empty string to avoid nullability warnings
        if (text == null) text = string.Empty;

        // If precomputed key contained in text, return it
        foreach (var k in _map.Keys)
        {
            if (text.Contains(k, System.StringComparison.OrdinalIgnoreCase))
                return _map[k];
        }

        // Missing precomputed embedding — decide fallback
        var mode = (_options.FallbackMode ?? "Deterministic").Trim();
        if (string.Equals(mode, "Strict", System.StringComparison.OrdinalIgnoreCase))
        {
            throw new PrecomputedEmbeddingMissingException("Precomputed embedding missing for input (strict mode)");
        }

        // Option: call service when enabled
        if (string.Equals(mode, "CallService", System.StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                // resolve live provider from DI
                var live = _liveProvider;
                if (live != null)
                {
                    var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(3));
                    var emb = await live.GetEmbeddingAsync(text).WaitAsync(cts.Token);
                    if (emb != null && emb.Length == _dimension && !IsAllZero(emb))
                    {
                        return emb;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Live provider fallback failed, using deterministic embedding: {Message}", ex.Message);
            }
        }

        // Deterministic fallback
        var dimension = _options.Deterministic?.Dimension > 0 ? _options.Deterministic.Dimension : _dimension;
        var salt = _options.Deterministic?.Salt ?? string.Empty;
        var det = GenerateDeterministicEmbedding(text, dimension, salt);
        var count = System.Threading.Interlocked.Increment(ref _fallbackCount);
        _logger.LogInformation("Precomputed embedding missing; deterministic fallback used. count={Count}", count);
        return det;
    }

    private static bool IsAllZero(float[] v)
    {
        if (v == null) return true;
        double sum = 0.0;
        foreach (var x in v) sum += x * x;
        return sum < 1e-12;
    }

    // Kept (private, static) for backward compatibility with reflection-based tests.
    private static float[] GenerateDeterministicEmbedding(string input, int dimension, string salt)
        => DeterministicEmbedding.Generate(input, dimension, salt);
}

