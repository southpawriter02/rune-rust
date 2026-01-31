// ═══════════════════════════════════════════════════════════════════════════════
// ICharacterCreationController.cs
// Interface defining the contract for the character creation workflow controller.
// Orchestrates step transitions, validates selections against providers, manages
// navigation (forward, back, cancel), and coordinates with the character factory
// for final creation. Each step method returns a StepResult containing the
// outcome, validation errors, and an updated ViewModel for TUI rendering.
// Version: 0.17.5d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Orchestrates the character creation workflow, managing state transitions,
/// validating selections, and coordinating with providers and the character factory.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ICharacterCreationController"/> is the central orchestrator for the
/// 6-step character creation workflow. It sits between the TUI screens (v0.17.5f)
/// and the domain providers, translating user selections into validated state
/// transitions with ViewModel updates.
/// </para>
/// <para>
/// <strong>Workflow Steps:</strong>
/// </para>
/// <list type="number">
///   <item><description>Lineage — <see cref="SelectLineageAsync"/> (Clan-Born requires flexible bonus)</description></item>
///   <item><description>Background — <see cref="SelectBackgroundAsync"/></description></item>
///   <item><description>Attributes — <see cref="ConfirmAttributesAsync"/> (validates allocation completeness)</description></item>
///   <item><description>Archetype — <see cref="SelectArchetypeAsync"/> (PERMANENT choice)</description></item>
///   <item><description>Specialization — <see cref="SelectSpecializationAsync"/> (validates archetype compatibility)</description></item>
///   <item><description>Summary — <see cref="ConfirmCharacterAsync"/> (validates name, creates character)</description></item>
/// </list>
/// <para>
/// <strong>Navigation:</strong> <see cref="GoBack"/> returns to the previous step
/// while preserving all selections. <see cref="Cancel"/> clears all state and ends
/// the session.
/// </para>
/// <para>
/// <strong>State Management:</strong> The controller maintains an internal
/// <see cref="CharacterCreationState"/> instance. Use <see cref="IsSessionActive"/>
/// to check if a session exists and <see cref="GetCurrentState"/> to retrieve the
/// current state and ViewModel tuple.
/// </para>
/// </remarks>
/// <seealso cref="StepResult"/>
/// <seealso cref="CharacterCreationResult"/>
/// <seealso cref="CharacterCreationState"/>
/// <seealso cref="CharacterCreationViewModel"/>
public interface ICharacterCreationController
{
    /// <summary>
    /// Initializes a new character creation session.
    /// Creates fresh state starting at Step 1 (Lineage).
    /// </summary>
    /// <returns>
    /// The initialized <see cref="CharacterCreationState"/> at the Lineage step
    /// with a unique <c>SessionId</c>.
    /// </returns>
    /// <remarks>
    /// Calling <see cref="Initialize"/> while a session is active replaces the
    /// existing session. The previous session's state is discarded.
    /// </remarks>
    CharacterCreationState Initialize();

    /// <summary>
    /// Processes lineage selection (Step 1).
    /// </summary>
    /// <param name="lineage">The selected lineage.</param>
    /// <param name="flexibleBonus">
    /// Required for <see cref="Lineage.ClanBorn"/> lineage. The attribute to receive
    /// the +1 flexible bonus. Ignored for other lineages.
    /// </param>
    /// <returns>
    /// A <see cref="StepResult"/> containing the success status, next step
    /// (<see cref="CharacterCreationStep.Background"/>), and updated ViewModel.
    /// Fails if <see cref="Lineage.ClanBorn"/> is selected without a flexible bonus.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Clan-Born is the only lineage that requires a <paramref name="flexibleBonus"/>
    /// parameter. All other lineages have fixed attribute modifiers and ignore
    /// this parameter.
    /// </para>
    /// </remarks>
    Task<StepResult> SelectLineageAsync(Lineage lineage, CoreAttribute? flexibleBonus = null);

    /// <summary>
    /// Processes background selection (Step 2).
    /// </summary>
    /// <param name="background">The selected background.</param>
    /// <returns>
    /// A <see cref="StepResult"/> containing the success status, next step
    /// (<see cref="CharacterCreationStep.Attributes"/>), and updated ViewModel.
    /// </returns>
    Task<StepResult> SelectBackgroundAsync(Background background);

    /// <summary>
    /// Processes attribute allocation confirmation (Step 3).
    /// </summary>
    /// <param name="attributes">The confirmed attribute allocation state.</param>
    /// <returns>
    /// A <see cref="StepResult"/> containing the success status, next step
    /// (<see cref="CharacterCreationStep.Archetype"/>), and updated ViewModel.
    /// Fails if <paramref name="attributes"/> is not complete (all points not spent).
    /// </returns>
    /// <remarks>
    /// The <paramref name="attributes"/> must have <c>IsComplete</c> equal to
    /// <c>true</c> (all allocation points spent) before confirmation is accepted.
    /// </remarks>
    Task<StepResult> ConfirmAttributesAsync(AttributeAllocationState attributes);

    /// <summary>
    /// Processes archetype selection (Step 4 — PERMANENT CHOICE).
    /// </summary>
    /// <param name="archetype">The selected archetype.</param>
    /// <returns>
    /// A <see cref="StepResult"/> containing the success status, next step
    /// (<see cref="CharacterCreationStep.Specialization"/>), and updated ViewModel.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>PERMANENT:</strong> Archetype is a permanent choice that cannot be
    /// changed after character creation. The TUI should warn the player before
    /// confirming this selection.
    /// </para>
    /// <para>
    /// Changing archetype clears the current specialization selection, since
    /// specializations are archetype-specific.
    /// </para>
    /// </remarks>
    Task<StepResult> SelectArchetypeAsync(Archetype archetype);

    /// <summary>
    /// Processes specialization selection (Step 5).
    /// </summary>
    /// <param name="specialization">The selected specialization ID.</param>
    /// <returns>
    /// A <see cref="StepResult"/> containing the success status, next step
    /// (<see cref="CharacterCreationStep.Summary"/>), and updated ViewModel.
    /// Fails if the specialization is not valid for the currently selected archetype.
    /// </returns>
    /// <remarks>
    /// The specialization must belong to the archetype selected in Step 4.
    /// The controller validates this by checking <c>ISpecializationProvider.GetByArchetype()</c>
    /// and matching on <c>SpecializationDefinition.SpecializationId</c>.
    /// </remarks>
    Task<StepResult> SelectSpecializationAsync(SpecializationId specialization);

    /// <summary>
    /// Processes final character confirmation with name (Step 6).
    /// Validates name, creates character via factory, and prepares for persistence.
    /// </summary>
    /// <param name="name">The character name to validate and assign.</param>
    /// <returns>
    /// A <see cref="CharacterCreationResult"/> containing the created
    /// <see cref="Player"/> entity or validation errors.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method performs three sequential validations:
    /// </para>
    /// <list type="number">
    ///   <item><description>Name validation via <c>INameValidator.Validate()</c></description></item>
    ///   <item><description>State completeness check (<c>CharacterCreationState.IsComplete</c>)</description></item>
    ///   <item><description>Character creation via <c>ICharacterFactory.CreateCharacterAsync()</c></description></item>
    /// </list>
    /// <para>
    /// If the character factory is not available (v0.17.5e pending), the method
    /// returns a success result with a null character and a "pending" message.
    /// </para>
    /// </remarks>
    Task<CharacterCreationResult> ConfirmCharacterAsync(string name);

    /// <summary>
    /// Navigates back to the previous step, preserving current selections.
    /// </summary>
    /// <returns>
    /// A <see cref="StepResult"/> with the previous step and updated ViewModel.
    /// Fails if already at the first step (Lineage).
    /// </returns>
    /// <remarks>
    /// <see cref="GoBack"/> never clears selections. The player's previous choices
    /// remain in the state and are displayed when returning to earlier steps.
    /// </remarks>
    StepResult GoBack();

    /// <summary>
    /// Cancels character creation and clears all state.
    /// </summary>
    /// <remarks>
    /// After <see cref="Cancel"/>, <see cref="IsSessionActive"/> returns <c>false</c>
    /// and all step selections are discarded. A new session must be started via
    /// <see cref="Initialize"/> to resume character creation.
    /// </remarks>
    void Cancel();

    /// <summary>
    /// Gets the current state and corresponding ViewModel.
    /// </summary>
    /// <returns>
    /// A tuple of the current <see cref="CharacterCreationState"/> and a freshly
    /// built <see cref="CharacterCreationViewModel"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no active session exists. Call <see cref="Initialize"/> first.
    /// </exception>
    (CharacterCreationState State, CharacterCreationViewModel ViewModel) GetCurrentState();

    /// <summary>
    /// Gets whether a creation session is currently active.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="Initialize"/> has been called and the session
    /// has not been cancelled or completed; <c>false</c> otherwise.
    /// </value>
    bool IsSessionActive { get; }
}
