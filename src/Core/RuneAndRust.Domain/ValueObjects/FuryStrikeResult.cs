// ═══════════════════════════════════════════════════════════════════════════════
// FuryStrikeResult.cs
// Value object encapsulating the outcome of a Fury Strike ability activation,
// including damage breakdown, Rage spent, and Corruption trigger status.
// Version: 0.20.5a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Encapsulates the outcome of a Fury Strike ability activation.
/// </summary>
/// <remarks>
/// <para>
/// Fury Strike deals weapon damage + 3d6 fury damage to melee range target.
/// On a critical hit (natural 20), an additional 1d6 bonus fury damage is added.
/// The ability costs 20 Rage and 2 AP. If used while Enraged (80+ Rage),
/// +1 Corruption is triggered.
/// </para>
/// <para>
/// <strong>Damage formula:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>Base damage = weapon damage</description></item>
///   <item><description>Fury damage = 3d6</description></item>
///   <item><description>Critical bonus = 1d6 (on natural 20 only)</description></item>
///   <item><description>Final = BaseDamage + FuryDamage + CriticalBonusDamage</description></item>
/// </list>
/// <example>
/// <code>
/// var result = FuryStrikeResult.Create(
///     attackRoll: 18, baseDamage: 8, furyDamage: 11,
///     criticalBonusDamage: 0, rageSpent: 20,
///     wasCritical: false, corruptionTriggered: false);
/// // result.FinalDamage = 19 (8 + 11 + 0)
/// </code>
/// </example>
/// </remarks>
/// <seealso cref="BerserkrAbilityId"/>
/// <seealso cref="RageResource"/>
public sealed record FuryStrikeResult
{
    // ─────────────────────────────────────────────────────────────────────────
    // Properties
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// The attack roll result (d20 + modifiers).
    /// </summary>
    public int AttackRoll { get; init; }

    /// <summary>
    /// Base weapon damage dealt.
    /// </summary>
    public int BaseDamage { get; init; }

    /// <summary>
    /// Fury damage dealt (3d6).
    /// </summary>
    public int FuryDamage { get; init; }

    /// <summary>
    /// Critical hit bonus damage (1d6 on natural 20, otherwise 0).
    /// </summary>
    public int CriticalBonusDamage { get; init; }

    /// <summary>
    /// Total damage before critical bonus.
    /// </summary>
    public int TotalDamage => BaseDamage + FuryDamage;

    /// <summary>
    /// Final damage including all components.
    /// </summary>
    public int FinalDamage => TotalDamage + CriticalBonusDamage;

    /// <summary>
    /// Rage points spent on this attack (always <see cref="RageResource.FuryStrikeCost"/>).
    /// </summary>
    public int RageSpent { get; init; } = RageResource.FuryStrikeCost;

    /// <summary>
    /// Whether the attack was a critical hit (natural 20).
    /// </summary>
    public bool WasCritical { get; init; }

    /// <summary>
    /// Whether Corruption was triggered by this ability usage.
    /// True when Fury Strike is used at 80+ Rage.
    /// </summary>
    public bool CorruptionTriggered { get; init; }

    /// <summary>
    /// Description of why Corruption was triggered, if applicable.
    /// </summary>
    public string? CorruptionReason { get; init; }

    // ─────────────────────────────────────────────────────────────────────────
    // Factory Methods
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a FuryStrikeResult with all damage components.
    /// </summary>
    /// <param name="attackRoll">Attack roll result.</param>
    /// <param name="baseDamage">Base weapon damage.</param>
    /// <param name="furyDamage">Fury damage (3d6).</param>
    /// <param name="criticalBonusDamage">Critical bonus (1d6 on nat 20, else 0).</param>
    /// <param name="rageSpent">Rage spent.</param>
    /// <param name="wasCritical">Whether this was a critical hit.</param>
    /// <param name="corruptionTriggered">Whether Corruption was triggered.</param>
    /// <param name="corruptionReason">Reason for Corruption, if triggered.</param>
    /// <returns>A new FuryStrikeResult.</returns>
    public static FuryStrikeResult Create(
        int attackRoll,
        int baseDamage,
        int furyDamage,
        int criticalBonusDamage = 0,
        int rageSpent = RageResource.FuryStrikeCost,
        bool wasCritical = false,
        bool corruptionTriggered = false,
        string? corruptionReason = null) =>
        new()
        {
            AttackRoll = attackRoll,
            BaseDamage = baseDamage,
            FuryDamage = furyDamage,
            CriticalBonusDamage = criticalBonusDamage,
            RageSpent = rageSpent,
            WasCritical = wasCritical,
            CorruptionTriggered = corruptionTriggered,
            CorruptionReason = corruptionReason
        };

    // ─────────────────────────────────────────────────────────────────────────
    // Display
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Formats a detailed damage breakdown string.
    /// </summary>
    /// <returns>Formatted breakdown (e.g., "8 base + 11 fury + 4 critical = 23 total").</returns>
    public string GetDamageBreakdown()
    {
        var parts = $"{BaseDamage} base + {FuryDamage} fury";

        if (CriticalBonusDamage > 0)
            parts += $" + {CriticalBonusDamage} critical";

        return $"{parts} = {FinalDamage} total";
    }

    /// <summary>
    /// Formats a result string for game display.
    /// </summary>
    /// <param name="targetName">Name of the target struck.</param>
    /// <returns>Formatted result string.</returns>
    public string GetResultString(string targetName)
    {
        var critText = WasCritical ? " CRITICAL" : "";
        var corruptionText = CorruptionTriggered
            ? $" [Corruption: {CorruptionReason}]"
            : "";

        return $"Fury Strike{critText} hits {targetName} for {FinalDamage} damage " +
               $"({GetDamageBreakdown()}) [{RageSpent} Rage spent]{corruptionText}";
    }

    /// <summary>
    /// Returns a diagnostic representation of this result.
    /// </summary>
    public override string ToString() =>
        $"FuryStrike(Roll={AttackRoll}, Damage={FinalDamage}, " +
        $"Critical={WasCritical}, Corruption={CorruptionTriggered})";
}
