// ═══════════════════════════════════════════════════════════════════════════════
// ResourceType.cs
// Enum defining the primary resource types used by archetypes for abilities.
// Version: 0.17.3a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the primary resource types used by archetypes for abilities.
/// </summary>
/// <remarks>
/// <para>
/// Resource types determine which pool characters draw from when using
/// abilities. The primary resource affects regeneration rates, ability
/// costs, and synergies with lineage traits.
/// </para>
/// <para>
/// Currently, <see cref="Archetype.Mystic"/> is the only archetype using
/// <see cref="AetherPool"/>. <see cref="Archetype.Warrior"/>,
/// <see cref="Archetype.Skirmisher"/>, and <see cref="Archetype.Adept"/>
/// all use <see cref="Stamina"/>.
/// </para>
/// <para>
/// Resource type values are explicitly assigned (0-1) to ensure stable
/// serialization and database storage.
/// </para>
/// </remarks>
/// <seealso cref="Archetype"/>
/// <seealso cref="Entities.ArchetypeDefinition"/>
public enum ResourceType
{
    /// <summary>
    /// Stamina - Physical resource for non-magical abilities.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used by Warrior, Skirmisher, and Adept archetypes. Stamina powers
    /// physical combat abilities, stances, and utility skills.
    /// </para>
    /// <para>
    /// Regenerates based on Finesse and Might attributes.
    /// </para>
    /// <para>
    /// Formula: Max Stamina = (FINESSE × 5) + (MIGHT × 5) + 20 + Archetype Bonus
    /// </para>
    /// </remarks>
    Stamina = 0,

    /// <summary>
    /// Aether Pool - Magical resource for Aetheric abilities.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used exclusively by the Mystic archetype. Aether Pool powers ranged
    /// magical attacks, crowd control, and Aetheric channeling abilities.
    /// </para>
    /// <para>
    /// Regenerates based on Will and Wits attributes.
    /// </para>
    /// <para>
    /// Formula: Max Aether Pool = (WILL × 10) + (WITS × 5) + Archetype Bonus + Lineage Bonus
    /// </para>
    /// <para>
    /// The Rune-Marked lineage provides an additional ×1.10 multiplier
    /// via the Aether-Tainted trait.
    /// </para>
    /// </remarks>
    AetherPool = 1
}
