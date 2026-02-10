// ═══════════════════════════════════════════════════════════════════════════════
// IncorporealRestriction.cs
// Enumerates restrictions that apply when a character is in incorporeal form
// via the Myrk-gengr Merge with Darkness ability.
// Version: 0.20.4c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the restrictions applied to a character while in incorporeal form
/// from the <see cref="MyrkgengrAbilityId.MergeWithDarkness"/> ability.
/// </summary>
/// <remarks>
/// <para>
/// While incorporeal, the character can phase through solid objects and is
/// immune to physical attacks, but suffers significant restrictions on
/// interaction with the physical world.
/// </para>
/// </remarks>
/// <seealso cref="MyrkgengrAbilityId"/>
public enum IncorporealRestriction
{
    /// <summary>
    /// Cannot make physical (melee or ranged) attacks.
    /// Shadow abilities remain available.
    /// </summary>
    CannotAttackPhysically = 1,

    /// <summary>
    /// Cannot interact with physical objects (doors, levers, chests, etc.).
    /// </summary>
    CannotInteractWithObjects = 2,

    /// <summary>
    /// Cannot pick up, drop, or use physical items.
    /// </summary>
    CannotPickUpItems = 3,

    /// <summary>
    /// Takes 2d6 damage from magical light sources.
    /// This is the primary vulnerability of the incorporeal form.
    /// </summary>
    VulnerableToMagicalLight = 4
}
