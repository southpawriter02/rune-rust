// ═══════════════════════════════════════════════════════════════════════════════
// TrapTriggerType.cs
// Describes how a runic trap is triggered when activated. Each type determines
// the detection and activation conditions for the trap.
// Version: 0.20.2b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Describes how a runic trap is triggered when activated.
/// </summary>
/// <remarks>
/// <para>
/// Trap trigger types determine the conditions under which a placed runic trap
/// activates and deals damage. The default type for Tier 2 Runic Trap ability
/// is <see cref="Movement"/>.
/// </para>
/// <list type="bullet">
///   <item><description><b>Movement (1):</b> Triggers when a creature enters the trap's space.</description></item>
///   <item><description><b>Proximity (2):</b> Triggers when a creature moves within adjacent space. Future enhancement.</description></item>
///   <item><description><b>Timed (3):</b> Triggers after a set delay. Future enhancement.</description></item>
/// </list>
/// <para>
/// Enum values are explicitly assigned to ensure stable serialization.
/// <see cref="Proximity"/> and <see cref="Timed"/> are defined for forward
/// compatibility but are not fully implemented until future versions.
/// </para>
/// </remarks>
/// <seealso cref="RunasmidrAbilityId"/>
public enum TrapTriggerType
{
    /// <summary>
    /// Triggers when a creature enters the space containing the trap.
    /// This is the default trigger type for the Tier 2 Runic Trap ability.
    /// </summary>
    Movement = 1,

    /// <summary>
    /// Triggers when a creature moves within an adjacent space.
    /// Future enhancement — not fully implemented in v0.20.2b.
    /// </summary>
    Proximity = 2,

    /// <summary>
    /// Triggers after a set time delay from placement.
    /// Future enhancement — not fully implemented in v0.20.2b.
    /// </summary>
    Timed = 3
}
