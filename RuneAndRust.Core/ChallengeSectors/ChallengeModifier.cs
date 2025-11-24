using System.Text.Json;

namespace RuneAndRust.Core.ChallengeSectors;

/// <summary>
/// v0.40.2: Challenge modifier definition
/// Represents a single gameplay modifier that can be applied to Challenge Sectors
/// </summary>
public class ChallengeModifier
{
    /// <summary>Unique modifier identifier (e.g., "no_healing", "lava_floors")</summary>
    public string ModifierId { get; set; } = string.Empty;

    /// <summary>Display name (e.g., "No Healing", "Lava Floors")</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Modifier category</summary>
    public ChallengeModifierCategory Category { get; set; }

    /// <summary>Player-facing description of the modifier's effects</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Difficulty multiplier (1.0 = baseline, higher = harder)</summary>
    public float DifficultyMultiplier { get; set; } = 1.0f;

    /// <summary>Modifier-specific parameters (JSON)</summary>
    public Dictionary<string, object> Parameters { get; set; } = new();

    /// <summary>Service method or implementation class reference</summary>
    public string? ApplicationLogic { get; set; }

    /// <summary>Is this modifier active and available?</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Sort order for display</summary>
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// Get a parameter value as a specific type
    /// </summary>
    public T? GetParameter<T>(string key, T? defaultValue = default)
    {
        if (!Parameters.TryGetValue(key, out var value))
        {
            return defaultValue;
        }

        try
        {
            if (value is JsonElement jsonElement)
            {
                return JsonSerializer.Deserialize<T>(jsonElement.GetRawText());
            }

            if (value is T typedValue)
            {
                return typedValue;
            }

            return (T?)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Set a parameter value
    /// </summary>
    public void SetParameter<T>(string key, T value)
    {
        if (value != null)
        {
            Parameters[key] = value;
        }
    }
}
