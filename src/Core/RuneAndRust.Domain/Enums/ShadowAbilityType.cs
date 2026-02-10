// ═══════════════════════════════════════════════════════════════════════════════
// ShadowAbilityType.cs
// Enum classifying Myrk-gengr ability activation types (Active, Passive,
// Stance, Triggered, Toggle, Ultimate).
// Version: 0.20.4a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Classifies the activation type of Myrk-gengr abilities.
/// </summary>
/// <remarks>
/// <para>
/// Ability types determine how an ability is activated, maintained, and ended:
/// </para>
/// <list type="bullet">
///   <item><description>
///     <b>Active:</b> Requires AP and explicit activation. One-time effect.
///     Example: Shadow Step (2 AP, 10 Shadow Essence).
///   </description></item>
///   <item><description>
///     <b>Passive:</b> Always active, no AP cost, no activation required.
///     Example: Dark-Adapted (removes dim light penalties).
///   </description></item>
///   <item><description>
///     <b>Stance:</b> Requires AP to enter, ongoing cost to maintain,
///     persists until manually ended or resource depleted.
///     Example: Cloak of Night (1 AP to enter, 5 Shadow Essence/turn).
///   </description></item>
///   <item><description>
///     <b>Triggered:</b> Activates automatically when specific conditions
///     are met. No AP cost.
///   </description></item>
///   <item><description>
///     <b>Toggle:</b> Switch between on/off states. Initial AP cost but
///     no per-turn maintenance cost.
///   </description></item>
///   <item><description>
///     <b>Ultimate:</b> Capstone ability with high cost and long cooldown.
///     Typically once per rest.
///   </description></item>
/// </list>
/// </remarks>
/// <seealso cref="MyrkgengrAbilityId"/>
public enum ShadowAbilityType
{
    /// <summary>
    /// One-time activation requiring AP. Example: Shadow Step.
    /// </summary>
    Active = 0,

    /// <summary>
    /// Always active, no activation needed. Example: Dark-Adapted.
    /// </summary>
    Passive = 1,

    /// <summary>
    /// Persistent state with ongoing maintenance cost. Example: Cloak of Night.
    /// </summary>
    Stance = 2,

    /// <summary>
    /// Automatically activates when conditions are met.
    /// </summary>
    Triggered = 3,

    /// <summary>
    /// Toggle between on/off states with initial AP cost.
    /// </summary>
    Toggle = 4,

    /// <summary>
    /// Capstone ability with high cost and long cooldown.
    /// </summary>
    Ultimate = 5
}
