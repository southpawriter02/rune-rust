using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Models.Combat;

/// <summary>
/// Represents the result of adding or purging corruption on a character.
/// Unlike Stress, corruption is NOT mitigated by WILL - it accumulates directly.
/// </summary>
/// <param name="RawCorruption">The initial corruption amount (positive for add, negative for purge).</param>
/// <param name="NetCorruptionApplied">Actual corruption change (clamped to 0-100 range).</param>
/// <param name="CurrentTotal">Character's new corruption total after application.</param>
/// <param name="PreviousTier">Corruption tier before this change.</param>
/// <param name="NewTier">Corruption tier after this change.</param>
/// <param name="TierChanged">True if a tier threshold was crossed.</param>
/// <param name="IsTerminal">True if corruption reached 100, triggering Terminal Error.</param>
/// <param name="Source">Description of what caused the corruption (for logging and UI).</param>
public record CorruptionResult(
    int RawCorruption,
    int NetCorruptionApplied,
    int CurrentTotal,
    CorruptionTier PreviousTier,
    CorruptionTier NewTier,
    bool TierChanged,
    bool IsTerminal,
    string Source
);
