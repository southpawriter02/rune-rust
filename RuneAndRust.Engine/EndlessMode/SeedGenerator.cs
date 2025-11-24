using System.Security.Cryptography;
using System.Text;

namespace RuneAndRust.Engine.EndlessMode;

/// <summary>
/// v0.40.4: Seed Generator
/// Generates reproducible seeds for endless mode runs
/// </summary>
public static class SeedGenerator
{
    // ═══════════════════════════════════════════════════════════
    // DAILY SEEDS
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Generate today's daily seed
    /// </summary>
    public static string GenerateDailySeed()
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        return $"ENDLESS-{date}";
    }

    /// <summary>
    /// Generate daily seed for a specific date
    /// </summary>
    public static string GenerateDailySeed(DateTime date)
    {
        var dateStr = date.ToString("yyyyMMdd");
        return $"ENDLESS-{dateStr}";
    }

    // ═══════════════════════════════════════════════════════════
    // CUSTOM SEEDS
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Generate a random seed
    /// </summary>
    public static string GenerateRandomSeed()
    {
        var random = new Random();
        var value = random.Next(1000, 9999);
        return $"ENDLESS-RANDOM-{value}";
    }

    /// <summary>
    /// Generate a seed from a string input
    /// </summary>
    public static string GenerateFromString(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return GenerateRandomSeed();
        }

        // Sanitize input
        var sanitized = new string(input
            .Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_')
            .Take(32)
            .ToArray())
            .ToUpperInvariant();

        return $"ENDLESS-{sanitized}";
    }

    // ═══════════════════════════════════════════════════════════
    // SEED CONVERSION
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Convert seed to integer for RNG
    /// </summary>
    public static int SeedToInt(string seed)
    {
        if (string.IsNullOrEmpty(seed))
        {
            return 0;
        }

        // Use MD5 hash of seed to generate consistent integer
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(seed));
        return BitConverter.ToInt32(hash, 0);
    }

    /// <summary>
    /// Get wave-specific seed for deterministic wave generation
    /// </summary>
    public static int GetWaveSeed(string baseSeed, int waveNumber)
    {
        var combinedSeed = $"{baseSeed}-WAVE-{waveNumber}";
        return SeedToInt(combinedSeed);
    }

    // ═══════════════════════════════════════════════════════════
    // VALIDATION
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Validate if a seed is properly formatted
    /// </summary>
    public static bool IsValidSeed(string seed)
    {
        if (string.IsNullOrWhiteSpace(seed))
        {
            return false;
        }

        // Must start with ENDLESS-
        if (!seed.StartsWith("ENDLESS-", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Must be reasonable length (8-50 characters)
        if (seed.Length < 8 || seed.Length > 50)
        {
            return false;
        }

        // Must only contain valid characters
        return seed.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_');
    }
}
