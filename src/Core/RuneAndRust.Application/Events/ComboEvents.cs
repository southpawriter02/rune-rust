namespace RuneAndRust.Application.Events;

/// <summary>
/// Published when a combatant starts tracking a new combo.
/// </summary>
/// <remarks>
/// <para>A combo starts when the combatant uses the first ability in a combo sequence.</para>
/// <para>Multiple combos can be started by the same ability if it's the first step of multiple combos.</para>
/// </remarks>
/// <param name="CombatantId">The unique identifier of the combatant who started the combo.</param>
/// <param name="ComboId">The unique identifier of the combo that was started.</param>
/// <param name="ComboName">The display name of the combo.</param>
/// <param name="TotalSteps">The total number of steps in the combo.</param>
/// <param name="WindowTurns">The number of turns allowed to complete the combo.</param>
public record ComboStartedEvent(
    Guid CombatantId,
    string ComboId,
    string ComboName,
    int TotalSteps,
    int WindowTurns);

/// <summary>
/// Published when a combatant advances an in-progress combo to the next step.
/// </summary>
/// <remarks>
/// <para>This event fires for each intermediate step (steps 2 through N-1).</para>
/// <para>The final step triggers <see cref="ComboCompletedEvent"/> instead.</para>
/// </remarks>
/// <param name="CombatantId">The unique identifier of the combatant progressing the combo.</param>
/// <param name="ComboId">The unique identifier of the combo.</param>
/// <param name="ComboName">The display name of the combo.</param>
/// <param name="CurrentStep">The step just completed (1-indexed).</param>
/// <param name="TotalSteps">The total number of steps in the combo.</param>
/// <param name="WindowRemaining">The number of turns remaining to complete the combo.</param>
public record ComboProgressedEvent(
    Guid CombatantId,
    string ComboId,
    string ComboName,
    int CurrentStep,
    int TotalSteps,
    int WindowRemaining);

/// <summary>
/// Published when a combatant successfully completes a combo.
/// </summary>
/// <remarks>
/// <para>A combo completes when the combatant executes the final ability within the time window.</para>
/// <para>Bonus effects are applied after this event is published.</para>
/// </remarks>
/// <param name="CombatantId">The unique identifier of the combatant who completed the combo.</param>
/// <param name="ComboId">The unique identifier of the completed combo.</param>
/// <param name="ComboName">The display name of the combo.</param>
/// <param name="BonusEffectsApplied">The number of bonus effects that were applied.</param>
public record ComboCompletedEvent(
    Guid CombatantId,
    string ComboId,
    string ComboName,
    int BonusEffectsApplied);

/// <summary>
/// Published when a combo fails to complete.
/// </summary>
/// <remarks>
/// <para>A combo can fail due to:</para>
/// <list type="bullet">
///   <item><description>Window expiration - the time window closed before completion</description></item>
///   <item><description>Wrong ability - an ability was used that doesn't match the next step</description></item>
///   <item><description>Target mismatch - the target didn't satisfy the step's target requirement</description></item>
/// </list>
/// </remarks>
/// <param name="CombatantId">The unique identifier of the combatant whose combo failed.</param>
/// <param name="ComboId">The unique identifier of the failed combo.</param>
/// <param name="ComboName">The display name of the combo.</param>
/// <param name="Reason">The reason the combo failed.</param>
/// <param name="StepReached">The last step completed before failure (1-indexed).</param>
public record ComboFailedEvent(
    Guid CombatantId,
    string ComboId,
    string ComboName,
    string Reason,
    int StepReached);

/// <summary>
/// Published when a combo bonus effect is applied to a target.
/// </summary>
/// <remarks>
/// <para>Multiple events may be published for a single combo completion if the combo has multiple bonus effects.</para>
/// </remarks>
/// <param name="CombatantId">The unique identifier of the combatant who triggered the bonus.</param>
/// <param name="ComboId">The unique identifier of the completed combo.</param>
/// <param name="ComboName">The display name of the combo.</param>
/// <param name="EffectType">The type of bonus effect applied (e.g., "ExtraDamage", "ApplyStatus").</param>
/// <param name="EffectDescription">A human-readable description of the effect.</param>
/// <param name="TargetId">The unique identifier of the target receiving the effect (null for self).</param>
public record ComboBonusAppliedEvent(
    Guid CombatantId,
    string ComboId,
    string ComboName,
    string EffectType,
    string EffectDescription,
    Guid? TargetId);

/// <summary>
/// Published when a combatant's combo progress is reset (e.g., at combat end).
/// </summary>
/// <remarks>
/// <para>This event is published when all in-progress combos for a combatant are cleared.</para>
/// </remarks>
/// <param name="CombatantId">The unique identifier of the combatant whose combos were reset.</param>
/// <param name="CombosCleared">The number of in-progress combos that were cleared.</param>
public record ComboProgressResetEvent(
    Guid CombatantId,
    int CombosCleared);
