namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Identifies the activation type of a specialization ability.
/// </summary>
/// <remarks>
/// <para>
/// Specialization abilities fall into categories based on their activation:
/// <list type="bullet">
///   <item><description>Passive: Always active when character has the ability</description></item>
///   <item><description>Triggered: Activates automatically under specific conditions</description></item>
///   <item><description>Reactive: Character chooses to activate in response to event</description></item>
///   <item><description>Active: Requires deliberate activation and may cost resources</description></item>
/// </list>
/// </para>
/// <para>
/// <b>v0.15.2g:</b> Initial implementation of ability activation types.
/// </para>
/// </remarks>
public enum SpecializationAbilityType
{
    /// <summary>
    /// Always active when the character possesses this ability.
    /// No activation required.
    /// </summary>
    /// <remarks>
    /// Examples: [Roof-Runner], [Death-Defying Leap], [Featherfall], [One with the Static]
    /// </remarks>
    Passive = 0,

    /// <summary>
    /// Activates automatically when specific conditions are met.
    /// No character choice required.
    /// </summary>
    /// <remarks>
    /// Example: [Slip into Shadow] when entering shadows
    /// </remarks>
    Triggered = 1,

    /// <summary>
    /// Character can choose to activate in response to an event.
    /// May have limited uses.
    /// </summary>
    /// <remarks>
    /// Examples: [Double Jump] after failed leap, [Ghostly Form] after attack
    /// </remarks>
    Reactive = 2,

    /// <summary>
    /// Requires deliberate activation, typically costs an action.
    /// May have duration or concentration requirements.
    /// </summary>
    /// <remarks>
    /// Examples: [Wall-Run], [Cloak the Party]
    /// </remarks>
    Active = 3
}
