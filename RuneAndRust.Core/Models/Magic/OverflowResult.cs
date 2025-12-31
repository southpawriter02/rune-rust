namespace RuneAndRust.Core.Models.Magic;

/// <summary>
/// Result of an Aetheric Overflow event (resonance reaching 100).
/// </summary>
public record OverflowResult(
    decimal PotencyBonus,
    int DurationTurns,
    int DischargeAmount,
    bool SoulFractureRisk,
    int TotalOverflowCount)
{
    /// <summary>
    /// True if this overflow carries risk of permanent Soul Fracture.
    /// </summary>
    public bool IsHighRisk => SoulFractureRisk;

    /// <summary>
    /// Factory for creating a "no overflow" result.
    /// </summary>
    public static OverflowResult None => new(
        PotencyBonus: 1.0m,
        DurationTurns: 0,
        DischargeAmount: 0,
        SoulFractureRisk: false,
        TotalOverflowCount: 0);
}
