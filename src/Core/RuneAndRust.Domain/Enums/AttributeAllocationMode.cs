// ═══════════════════════════════════════════════════════════════════════════════
// AttributeAllocationMode.cs
// Enum defining the two available modes for attribute allocation during
// character creation: Simple (archetype-recommended) and Advanced (point-buy).
// Version: 0.17.2b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the available modes for attribute allocation during character creation.
/// </summary>
/// <remarks>
/// <para>
/// The allocation mode determines how a player assigns attribute points during
/// Step 3 of character creation. Two modes are available to accommodate both
/// new and experienced players:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <see cref="Simple"/>: Archetype-recommended builds for quick character creation.
///       The player selects an archetype and attributes are automatically assigned
///       to recommended values. No manual adjustment is allowed.
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="Advanced"/>: Full point-buy customization for experienced players.
///       All attributes start at 1 and the player distributes points manually
///       with cost scaling at higher values (9-10 cost 2 points each).
///     </description>
///   </item>
/// </list>
/// <para>
/// Players can switch between modes during character creation. Switching from
/// Advanced to Simple will reset attributes to the archetype's recommended build.
/// Switching from Simple to Advanced preserves current values and enables manual editing.
/// </para>
/// <para>
/// Values are explicitly assigned (0-1) to ensure stable serialization
/// and database storage.
/// </para>
/// </remarks>
/// <seealso cref="ValueObjects.AttributeAllocationState"/>
/// <seealso cref="CoreAttribute"/>
public enum AttributeAllocationMode
{
    /// <summary>
    /// Simple mode uses archetype-recommended attribute builds.
    /// </summary>
    /// <remarks>
    /// <para>
    /// In Simple mode:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Player selects an archetype first</description></item>
    ///   <item><description>Attributes are automatically set to recommended values</description></item>
    ///   <item><description>Manual adjustment is disabled (+/- controls hidden)</description></item>
    ///   <item><description>Fastest path through character creation for new players</description></item>
    /// </list>
    /// <para>
    /// Recommended builds per archetype:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Warrior: M4, F3, Wi2, Wl2, S4 (15 pts)</description></item>
    ///   <item><description>Skirmisher: M3, F4, Wi3, Wl2, S3 (15 pts)</description></item>
    ///   <item><description>Mystic: M2, F3, Wi4, Wl4, S2 (15 pts)</description></item>
    ///   <item><description>Adept: M3, F3, Wi3, Wl2, S3 (14 pts)</description></item>
    /// </list>
    /// </remarks>
    Simple = 0,

    /// <summary>
    /// Advanced mode enables full point-buy customization.
    /// </summary>
    /// <remarks>
    /// <para>
    /// In Advanced mode:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>All attributes start at 1 (base minimum)</description></item>
    ///   <item><description>Player has 15 points to spend (14 for Adept archetype)</description></item>
    ///   <item><description>Cost scaling applies: values 2-8 cost 1 point, values 9-10 cost 2 points</description></item>
    ///   <item><description>Maximum attribute value is 10, minimum is 1</description></item>
    ///   <item><description>Full control over attribute distribution</description></item>
    /// </list>
    /// <para>
    /// Players who switch from Simple to Advanced mode retain their current
    /// attribute values but gain the ability to manually adjust them.
    /// </para>
    /// </remarks>
    Advanced = 1
}
