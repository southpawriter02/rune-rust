// ═══════════════════════════════════════════════════════════════════════════════
// RuneType.cs
// Categorizes the different types of runes that a Rúnasmiðr can inscribe.
// Each type determines the rune's effect and valid targets.
// Version: 0.20.2a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Categorizes the different types of runes available to the Rúnasmiðr specialization.
/// </summary>
/// <remarks>
/// <para>
/// Each rune type determines the effect applied when inscribed on an item or surface:
/// </para>
/// <list type="bullet">
///   <item><description><b>Enhancement (1):</b> Weapon damage bonus (+2 damage). Tier 1.</description></item>
///   <item><description><b>Protection (2):</b> Armor defense bonus (+1 Defense). Tier 1.</description></item>
///   <item><description><b>Elemental (3):</b> Adds elemental damage to attacks. Tier 2 (v0.20.2b).</description></item>
///   <item><description><b>Ward (4):</b> Creates a damage absorption barrier. Tier 1.</description></item>
///   <item><description><b>Trap (5):</b> Triggered effect on enemy contact. Tier 2 (v0.20.2b).</description></item>
/// </list>
/// <para>
/// Enum values are explicitly assigned to ensure stable serialization. Types 3 and 5
/// are defined here for forward compatibility but are not implemented until v0.20.2b.
/// </para>
/// </remarks>
/// <seealso cref="RunasmidrAbilityId"/>
public enum RuneType
{
    /// <summary>
    /// Weapon damage bonus rune. Grants +2 damage when inscribed on a weapon.
    /// </summary>
    Enhancement = 1,

    /// <summary>
    /// Armor defense bonus rune. Grants +1 Defense when inscribed on armor.
    /// </summary>
    Protection = 2,

    /// <summary>
    /// Elemental damage rune. Adds elemental damage to attacks.
    /// Implemented in v0.20.2b (Tier 2).
    /// </summary>
    Elemental = 3,

    /// <summary>
    /// Damage absorption ward rune. Creates a barrier that absorbs incoming damage.
    /// </summary>
    Ward = 4,

    /// <summary>
    /// Triggered trap rune. Activates an effect when an enemy enters the area.
    /// Implemented in v0.20.2b (Tier 2).
    /// </summary>
    Trap = 5
}
