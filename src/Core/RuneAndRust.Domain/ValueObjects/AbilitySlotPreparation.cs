// ═══════════════════════════════════════════════════════════════════════════════
// AbilitySlotPreparation.cs
// Value object defining the tiered ability slot structure for a character's
// specialization, including slot counts, PP costs, and unlock thresholds.
// Version: 0.20.0a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the tiered ability slot structure prepared for a character when
/// they select a specialization.
/// </summary>
/// <remarks>
/// <para>
/// The ability slot structure uses a fixed 4-tier layout:
/// </para>
/// <list type="bullet">
///   <item><description><b>Tier 1:</b> 3 slots — Free (0 PP each), unlocked immediately</description></item>
///   <item><description><b>Tier 2:</b> 3 slots — 4 PP each, requires 8 PP invested in archetype tree</description></item>
///   <item><description><b>Tier 3:</b> 2 slots — 5 PP each, requires 16 PP invested in archetype tree</description></item>
///   <item><description><b>Capstone:</b> 1 slot — 6 PP, requires 24 PP invested in archetype tree</description></item>
/// </list>
/// <para>
/// Total: 9 ability slots per specialization, 35 PP maximum to complete the
/// full ability tree. The <see cref="TotalSlots"/> computed property always
/// returns the sum of all tier slot counts.
/// </para>
/// </remarks>
/// <seealso cref="SpecializationId"/>
public sealed record AbilitySlotPreparation
{
    /// <summary>
    /// The character this ability slot preparation belongs to.
    /// </summary>
    public required Guid CharacterId { get; init; }

    /// <summary>
    /// The specialization these ability slots are prepared for.
    /// </summary>
    public required SpecializationId SpecializationId { get; init; }

    /// <summary>
    /// Number of Tier 1 ability slots. Standard value: 3.
    /// Tier 1 slots are free (0 PP) and unlocked immediately.
    /// </summary>
    public required int Tier1Slots { get; init; }

    /// <summary>
    /// Number of Tier 2 ability slots. Standard value: 3.
    /// Each costs 4 PP and requires 8 PP invested in the archetype tree.
    /// </summary>
    public required int Tier2Slots { get; init; }

    /// <summary>
    /// Number of Tier 3 ability slots. Standard value: 2.
    /// Each costs 5 PP and requires 16 PP invested in the archetype tree.
    /// </summary>
    public required int Tier3Slots { get; init; }

    /// <summary>
    /// Number of Capstone ability slots. Standard value: 1.
    /// Costs 6 PP and requires 24 PP invested in the archetype tree.
    /// </summary>
    public required int CapstoneSlots { get; init; }

    /// <summary>
    /// Total number of ability slots across all tiers.
    /// Standard value: 9 (3 + 3 + 2 + 1).
    /// </summary>
    public int TotalSlots => Tier1Slots + Tier2Slots + Tier3Slots + CapstoneSlots;
}
