namespace AiRag.Api.Services;

/// <summary>
/// Deterministic, dependency-free embedding generator.
/// Uses a SHA-256 counter-based stretch of (salt, input) into a unit-normalized
/// float vector of the requested dimension. Identical inputs always produce
/// identical vectors, which makes it safe as a fallback when no live embedding
/// service is available.
/// </summary>
public static class DeterministicEmbedding
{
    public static float[] Generate(string input, int dimension, string salt = "")
    {
        if (dimension <= 0) throw new ArgumentOutOfRangeException(nameof(dimension));
        var data = System.Text.Encoding.UTF8.GetBytes(input ?? string.Empty);
        var saltBytes = System.Text.Encoding.UTF8.GetBytes(salt ?? string.Empty);

        int bytesNeeded = dimension * 4;
        var buffer = new byte[bytesNeeded];
        byte[] counterBytes = new byte[4];
        int filled = 0;
        uint counter = 0;
        using var sha = System.Security.Cryptography.SHA256.Create();
        while (filled < bytesNeeded)
        {
            counterBytes[0] = (byte)(counter & 0xff);
            counterBytes[1] = (byte)((counter >> 8) & 0xff);
            counterBytes[2] = (byte)((counter >> 16) & 0xff);
            counterBytes[3] = (byte)((counter >> 24) & 0xff);

            sha.Initialize();
            sha.TransformBlock(saltBytes, 0, saltBytes.Length, null, 0);
            sha.TransformBlock(counterBytes, 0, counterBytes.Length, null, 0);
            sha.TransformFinalBlock(data, 0, data.Length);
            var hash = sha.Hash ?? Array.Empty<byte>();
            int toCopy = Math.Min(hash.Length, bytesNeeded - filled);
            Buffer.BlockCopy(hash, 0, buffer, filled, toCopy);
            filled += toCopy;
            counter++;
        }

        var vector = new double[dimension];
        for (int i = 0; i < dimension; i++)
        {
            int offset = i * 4;
            uint u = (uint)(buffer[offset] | (buffer[offset + 1] << 8) | (buffer[offset + 2] << 16) | (buffer[offset + 3] << 24));
            double v01 = u / (double)uint.MaxValue;
            double v = (v01 * 2.0) - 1.0;
            vector[i] = v;
        }

        double sumSq = 0.0;
        for (int i = 0; i < dimension; i++) sumSq += vector[i] * vector[i];
        double norm = Math.Sqrt(sumSq);
        if (norm < 1e-12)
        {
            var fallback = new float[dimension];
            fallback[0] = 1f;
            return fallback;
        }

        var result = new float[dimension];
        for (int i = 0; i < dimension; i++) result[i] = (float)(vector[i] / norm);
        return result;
    }
}
