using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Models.Magic;

/// <summary>
/// Penalties applied based on a character's corruption level.
/// Corruption accumulates from Catastrophic backlash and imposes increasingly severe penalties.
/// </summary>
/// <remarks>
/// See: v0.4.3d (The Backlash) for implementation details.
/// Penalties scale with corruption level from minor inconveniences to complete loss of spellcasting.
/// </remarks>
public sealed record CorruptionPenalties
{
    /// <summary>
    /// The corruption level tier.
    /// </summary>
    public CorruptionLevel Level { get; init; }

    /// <summary>
    /// Raw corruption value (0-100).
    /// </summary>
    public int CorruptionValue { get; init; }

    /// <summary>
    /// Multiplier for MaxAP (1.0 = no penalty, 0.0 = cannot use AP).
    /// Untouched: 1.0, Tainted: 0.9, Afflicted: 0.8, Blighted: 0.7, Lost: 0.0.
    /// </summary>
    public double MaxApMultiplier { get; init; } = 1.0;

    /// <summary>
    /// Penalty to Will attribute (negative value).
    /// Afflicted: -1, Blighted: -2, Lost: -2.
    /// </summary>
    public int WillPenalty { get; init; }

    /// <summary>
    /// Penalty to Wits attribute (negative value).
    /// Blighted: -1, Lost: -1.
    /// </summary>
    public int WitsPenalty { get; init; }

    /// <summary>
    /// Whether the character can still cast spells.
    /// False only at Lost level (75+ corruption).
    /// </summary>
    public bool CanCastSpells { get; init; } = true;

    /// <summary>
    /// Domain 4 compliant description of the corruption state.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Computed: Effective MaxAP reduction for display (assumes base 10 AP).
    /// </summary>
    public int EffectiveApPenalty => (int)((1.0 - MaxApMultiplier) * 10);

    /// <summary>
    /// Computed: Whether any penalties are active.
    /// </summary>
    public bool HasPenalties => Level != CorruptionLevel.Untouched;

    /// <summary>
    /// Factory: Creates penalties for Untouched level (0-9 corruption).
    /// </summary>
    public static CorruptionPenalties Untouched(int corruption) => new()
    {
        Level = CorruptionLevel.Untouched,
        CorruptionValue = corruption,
        MaxApMultiplier = 1.0,
        WillPenalty = 0,
        WitsPenalty = 0,
        CanCastSpells = true,
        Description = "Your soul remains untouched by the weave's corruption."
    };

    /// <summary>
    /// Factory: Creates penalties for Tainted level (10-24 corruption).
    /// </summary>
    public static CorruptionPenalties Tainted(int corruption) => new()
    {
        Level = CorruptionLevel.Tainted,
        CorruptionValue = corruption,
        MaxApMultiplier = 0.9,
        WillPenalty = 0,
        WitsPenalty = 0,
        CanCastSpells = true,
        Description = "A faint darkness lingers at the edge of your vision."
    };

    /// <summary>
    /// Factory: Creates penalties for Afflicted level (25-49 corruption).
    /// </summary>
    public static CorruptionPenalties Afflicted(int corruption) => new()
    {
        Level = CorruptionLevel.Afflicted,
        CorruptionValue = corruption,
        MaxApMultiplier = 0.8,
        WillPenalty = -1,
        WitsPenalty = 0,
        CanCastSpells = true,
        Description = "The corruption whispers in quiet moments."
    };

    /// <summary>
    /// Factory: Creates penalties for Blighted level (50-74 corruption).
    /// </summary>
    public static CorruptionPenalties Blighted(int corruption) => new()
    {
        Level = CorruptionLevel.Blighted,
        CorruptionValue = corruption,
        MaxApMultiplier = 0.7,
        WillPenalty = -2,
        WitsPenalty = -1,
        CanCastSpells = true,
        Description = "Your flesh bears the marks of arcane scarring."
    };

    /// <summary>
    /// Factory: Creates penalties for Lost level (75-100 corruption).
    /// </summary>
    public static CorruptionPenalties Lost(int corruption) => new()
    {
        Level = CorruptionLevel.Lost,
        CorruptionValue = corruption,
        MaxApMultiplier = 0.0,
        WillPenalty = -2,
        WitsPenalty = -1,
        CanCastSpells = false,
        Description = "Your soul has been consumed. Magic no longer answers."
    };
}
