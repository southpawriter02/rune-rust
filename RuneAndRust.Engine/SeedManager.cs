using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// Manages dungeon seed generation, parsing, and persistence (v0.10)
/// </summary>
public class SeedManager
{
    private static readonly ILogger _log = Log.ForContext<SeedManager>();
    private static readonly Random _seedGenerator = new Random();

    /// <summary>
    /// Generates a new random seed
    /// </summary>
    public int GenerateSeed()
    {
        // Use a combination of timestamp and random value for uniqueness
        var timestamp = (int)(DateTime.UtcNow.Ticks % int.MaxValue);
        var random = _seedGenerator.Next();
        var seed = timestamp ^ random;

        _log.Debug("Generated new seed: {Seed}", seed);
        return seed;
    }

    /// <summary>
    /// Parses a seed from a string (allows players to input seeds as text)
    /// </summary>
    public int ParseSeed(string seedString)
    {
        if (string.IsNullOrWhiteSpace(seedString))
        {
            throw new ArgumentException("Seed string cannot be empty", nameof(seedString));
        }

        // Try to parse as integer first
        if (int.TryParse(seedString, out var intSeed))
        {
            _log.Debug("Parsed seed as integer: {Seed}", intSeed);
            return intSeed;
        }

        // Otherwise, use string hash code
        var hashSeed = seedString.GetHashCode();
        _log.Debug("Parsed seed from string '{SeedString}': {Seed}", seedString, hashSeed);
        return hashSeed;
    }

    /// <summary>
    /// Converts a seed to a shareable string format
    /// </summary>
    public string SeedToString(int seed)
    {
        return seed.ToString();
    }

    /// <summary>
    /// Validates that a seed is within acceptable range
    /// </summary>
    public bool IsValidSeed(int seed)
    {
        // All int values are technically valid seeds
        return true;
    }

    /// <summary>
    /// Generates a display-friendly seed name
    /// </summary>
    public string GetSeedDisplayName(int seed)
    {
        return $"Seed: {seed}";
    }

    /// <summary>
    /// Gets statistics about a seed (for debugging)
    /// </summary>
    public Dictionary<string, object> GetSeedInfo(int seed)
    {
        return new Dictionary<string, object>
        {
            ["Seed"] = seed,
            ["SeedString"] = SeedToString(seed),
            ["DisplayName"] = GetSeedDisplayName(seed),
            ["IsValid"] = IsValidSeed(seed)
        };
    }
}
