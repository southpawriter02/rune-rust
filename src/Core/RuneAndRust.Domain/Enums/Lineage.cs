// ═══════════════════════════════════════════════════════════════════════════════
// Lineage.cs
// Enum defining the bloodline heritage options for character creation.
// Version: 0.17.0a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the bloodline heritage of a character from before the Great Silence.
/// </summary>
/// <remarks>
/// <para>
/// Lineage represents how a character's ancestors were affected by the Runic Blight.
/// It is the first permanent choice in character creation and determines attribute
/// modifiers, passive bonuses, unique traits, and Trauma Economy baselines.
/// </para>
/// <para>
/// The four lineages each offer distinct gameplay advantages:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <see cref="ClanBorn"/>: Flexible and adaptable baseline humans with +1 to any attribute
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="RuneMarked"/>: Mystic affinity (+2 WILL, -1 STURDINESS) with permanent Corruption
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="IronBlooded"/>: Physical resilience (+2 STURDINESS, -1 WILL) with mental vulnerability
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="VargrKin"/>: Primal connection (+1 MIGHT, +1 FINESSE, -1 WILL) with balanced bonuses
///     </description>
///   </item>
/// </list>
/// <para>
/// Lineage values are explicitly assigned (0-3) to ensure stable serialization
/// and database storage. New lineages should be added at the end if needed.
/// </para>
/// </remarks>
/// <seealso cref="Entities.LineageDefinition"/>
/// <seealso cref="ValueObjects.LineageAttributeModifiers"/>
public enum Lineage
{
    /// <summary>
    /// The Stable Code - Descendants of survivors with untainted bloodlines.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Clan-Born represent baseline humanity. Their ancestors survived the
    /// Great Silence through luck, preparation, or isolation rather than
    /// mutation. They gain a flexible +1 bonus to any attribute of the
    /// player's choice, representing their adaptable nature.
    /// </para>
    /// <para>
    /// Attribute Modifiers: +1 to any attribute (player's choice)
    /// </para>
    /// <para>
    /// Passive Bonuses: +5 Max HP, +1 Social skill
    /// </para>
    /// <para>
    /// Unique Trait: [Survivor's Resolve] - +1d10 to Rhetoric checks with Clan-Born NPCs
    /// </para>
    /// <para>
    /// Trauma Baseline: No starting Corruption or Stress, no vulnerabilities
    /// </para>
    /// </remarks>
    ClanBorn = 0,

    /// <summary>
    /// The Tainted Aether - Those whose blood carries the All-Rune's echo.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Rune-Marked ancestors were exposed to concentrated Aether during
    /// the Blight. They gain +2 WILL and enhanced Aether Pool but suffer
    /// -1 STURDINESS and start with 5 permanent Corruption, reflecting
    /// their mystical taint.
    /// </para>
    /// <para>
    /// Attribute Modifiers: +2 WILL, -1 STURDINESS
    /// </para>
    /// <para>
    /// Passive Bonuses: +5 Max AP, +1 Lore skill
    /// </para>
    /// <para>
    /// Unique Trait: [Aether-Tainted] - +10% Maximum Aether Pool
    /// </para>
    /// <para>
    /// Trauma Baseline: 5 permanent Corruption, -1 to resist Corruption
    /// </para>
    /// </remarks>
    RuneMarked = 1,

    /// <summary>
    /// The Corrupted Earth - Bloodlines hardened by proximity to Blight-metal.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Iron-Blooded ancestors worked the corrupted mines and forges during
    /// the Blight era. They gain +2 STURDINESS and enhanced Soak but suffer
    /// -1 WILL and are more vulnerable to Psychic Stress, their minds less
    /// fortified than their bodies.
    /// </para>
    /// <para>
    /// Attribute Modifiers: +2 STURDINESS, -1 WILL
    /// </para>
    /// <para>
    /// Passive Bonuses: +2 Soak, +1 Craft skill
    /// </para>
    /// <para>
    /// Unique Trait: [Hazard Acclimation] - +1d10 to Sturdiness Resolve vs environmental hazards
    /// </para>
    /// <para>
    /// Trauma Baseline: No starting Corruption or Stress, -1 to resist Psychic Stress
    /// </para>
    /// </remarks>
    IronBlooded = 2,

    /// <summary>
    /// The Uncorrupted Echo - Those who carry the wolf-spirit's blessing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Vargr-Kin trace their lineage to the Varangian shamans who communed
    /// with primal spirits. They gain +1 MIGHT, +1 FINESSE, -1 WILL, and
    /// enhanced movement. Their Primal Clarity trait reduces Psychic Stress
    /// taken, compensating for their Will penalty.
    /// </para>
    /// <para>
    /// Attribute Modifiers: +1 MIGHT, +1 FINESSE, -1 WILL
    /// </para>
    /// <para>
    /// Passive Bonuses: +1 Movement, +1 Survival skill
    /// </para>
    /// <para>
    /// Unique Trait: [Primal Clarity] - -10% Psychic Stress from all sources
    /// </para>
    /// <para>
    /// Trauma Baseline: No starting Corruption or Stress, no vulnerabilities
    /// </para>
    /// </remarks>
    VargrKin = 3
}
