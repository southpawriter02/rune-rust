using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Events;

/// <summary>
/// Published when a combatant changes their combat stance.
/// </summary>
/// <remarks>
/// <para>This event is triggered whenever a combatant successfully switches
/// from one stance to another during combat.</para>
/// <para>Use this event to:</para>
/// <list type="bullet">
///   <item><description>Update UI elements showing the current stance</description></item>
///   <item><description>Log combat events for replay or analysis</description></item>
///   <item><description>Trigger animations or visual effects</description></item>
/// </list>
/// </remarks>
/// <param name="CombatantId">The unique identifier of the combatant who changed stance.</param>
/// <param name="CombatantName">The display name of the combatant for logging purposes.</param>
/// <param name="OldStance">The stance the combatant was in before the change.</param>
/// <param name="NewStance">The stance the combatant changed to.</param>
public record StanceChangedEvent(
    Guid CombatantId,
    string CombatantName,
    CombatStance OldStance,
    CombatStance NewStance);

/// <summary>
/// Published when a combatant's ability to change stance is reset for a new round.
/// </summary>
/// <remarks>
/// <para>Combatants can only change stance once per round. This event indicates
/// that the per-round limit has been reset, allowing the combatant to change
/// stance again.</para>
/// <para>Typically fired at the start of a new combat round or the combatant's turn.</para>
/// </remarks>
/// <param name="CombatantId">The unique identifier of the combatant whose stance change was reset.</param>
/// <param name="CombatantName">The display name of the combatant for logging purposes.</param>
public record StanceChangeResetEvent(
    Guid CombatantId,
    string CombatantName);

/// <summary>
/// Published when stance modifiers are applied to a combatant.
/// </summary>
/// <remarks>
/// <para>This event provides detailed information about which stat modifiers
/// were applied when entering a new stance.</para>
/// <para>Useful for debugging and verifying correct modifier application.</para>
/// </remarks>
/// <param name="CombatantId">The unique identifier of the combatant receiving modifiers.</param>
/// <param name="StanceId">The ID of the stance being applied (e.g., "aggressive").</param>
/// <param name="AttackModifier">The attack bonus/penalty applied.</param>
/// <param name="DefenseModifier">The defense bonus/penalty applied.</param>
/// <param name="SaveModifier">The save bonus/penalty applied to all saves.</param>
public record StanceModifiersAppliedEvent(
    Guid CombatantId,
    string StanceId,
    int AttackModifier,
    int DefenseModifier,
    int SaveModifier);

/// <summary>
/// Published when stance modifiers are removed from a combatant.
/// </summary>
/// <remarks>
/// <para>This event is triggered before applying new stance modifiers when
/// switching stances, ensuring the old modifiers are properly removed.</para>
/// </remarks>
/// <param name="CombatantId">The unique identifier of the combatant losing modifiers.</param>
/// <param name="StanceId">The ID of the stance being removed (e.g., "aggressive").</param>
public record StanceModifiersRemovedEvent(
    Guid CombatantId,
    string StanceId);
