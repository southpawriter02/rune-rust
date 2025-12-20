using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Models.Combat;

/// <summary>
/// Encapsulates corruption tier logic and penalty calculations.
/// Corruption is a "High Water Mark" mechanic that scales up easily
/// but requires significant, rare resources to lower.
/// </summary>
/// <param name="Value">The current corruption value (0-100).</param>
public record CorruptionState(int Value)
{
    /// <summary>
    /// Gets the corruption tier based on the current value.
    /// </summary>
    public CorruptionTier Tier => Value switch
    {
        >= 100 => CorruptionTier.Terminal,
        >= 81 => CorruptionTier.Fractured,
        >= 61 => CorruptionTier.Blighted,
        >= 41 => CorruptionTier.Corrupted,
        >= 21 => CorruptionTier.Tainted,
        _ => CorruptionTier.Pristine
    };

    /// <summary>
    /// Gets the Max AP multiplier based on corruption tier.
    /// Pristine/Tainted: 100%, Corrupted: 90%, Blighted: 80%, Fractured: 60%, Terminal: 0%
    /// </summary>
    public double MaxApMultiplier => Tier switch
    {
        CorruptionTier.Terminal => 0.0,
        CorruptionTier.Fractured => 0.60,
        CorruptionTier.Blighted => 0.80,
        CorruptionTier.Corrupted => 0.90,
        _ => 1.0
    };

    /// <summary>
    /// Gets the Max AP percentage penalty for display purposes.
    /// </summary>
    public int MaxApPenaltyPercent => Tier switch
    {
        CorruptionTier.Terminal => 100,
        CorruptionTier.Fractured => 40,
        CorruptionTier.Blighted => 20,
        CorruptionTier.Corrupted => 10,
        _ => 0
    };

    /// <summary>
    /// Gets the WILL attribute penalty based on corruption tier.
    /// Blighted: -1, Fractured/Terminal: -2
    /// </summary>
    public int WillPenalty => Tier switch
    {
        CorruptionTier.Terminal => 2,
        CorruptionTier.Fractured => 2,
        CorruptionTier.Blighted => 1,
        _ => 0
    };

    /// <summary>
    /// Gets the WITS attribute penalty based on corruption tier.
    /// Fractured/Terminal: -1
    /// </summary>
    public int WitsPenalty => Tier switch
    {
        CorruptionTier.Terminal => 1,
        CorruptionTier.Fractured => 1,
        _ => 0
    };

    /// <summary>
    /// Gets whether the character has reached terminal corruption (100+).
    /// Terminal corruption results in becoming a Forlorn (unplayable).
    /// </summary>
    public bool IsTerminal => Tier == CorruptionTier.Terminal;

    /// <summary>
    /// Gets whether the character has any corruption penalties.
    /// </summary>
    public bool HasPenalties => Tier >= CorruptionTier.Corrupted;

    /// <summary>
    /// Gets the display name for the current corruption tier.
    /// </summary>
    public string TierDisplayName => Tier switch
    {
        CorruptionTier.Terminal => "TERMINAL",
        CorruptionTier.Fractured => "Fractured",
        CorruptionTier.Blighted => "Blighted",
        CorruptionTier.Corrupted => "Corrupted",
        CorruptionTier.Tainted => "Tainted",
        _ => "Pristine"
    };
}
