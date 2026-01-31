// ═══════════════════════════════════════════════════════════════════════════════
// StepResult.cs
// Represents the result of a step selection or navigation action in the character
// creation workflow. Contains success status, current and next step information,
// validation errors (if any), and the updated ViewModel reflecting the new state.
// Returned by all ICharacterCreationController step methods and navigation actions.
// Version: 0.17.5d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the result of a step selection or navigation action
/// in the character creation workflow.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="StepResult"/> is an immutable value object returned by all step
/// selection methods and navigation actions on
/// <c>ICharacterCreationController</c>. It encapsulates the outcome of each
/// workflow operation, providing enough context for the TUI to update its display.
/// </para>
/// <para>
/// <strong>Properties:</strong>
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <strong>Success:</strong> Whether the action completed without validation errors.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>CurrentStep:</strong> The step the workflow is currently at after the action.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>NextStep:</strong> The next step to advance to after a successful action.
///       Null if the action failed or if at the final step (Summary).
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>ValidationErrors:</strong> A list of validation error messages when
///       the action failed. Empty when successful.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>UpdatedViewModel:</strong> The refreshed ViewModel reflecting the
///       current state after the action, suitable for rendering by the TUI.
///     </description>
///   </item>
/// </list>
/// <para>
/// <strong>Factory Methods:</strong> Use <see cref="Succeeded"/> for successful outcomes
/// and <see cref="Failed(CharacterCreationStep, CharacterCreationViewModel, string[])"/>
/// for validation failures. Direct construction via init properties is supported but
/// factory methods are preferred for consistency.
/// </para>
/// </remarks>
/// <seealso cref="CharacterCreationViewModel"/>
/// <seealso cref="CharacterCreationStep"/>
public readonly record struct StepResult
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether the action succeeded without validation errors.
    /// </summary>
    /// <value>
    /// <c>true</c> if the step selection or navigation completed successfully;
    /// <c>false</c> if validation errors were encountered.
    /// </value>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the next step after a successful action.
    /// Null if the action failed or if at the final step.
    /// </summary>
    /// <value>
    /// The <see cref="CharacterCreationStep"/> to navigate to next, or <c>null</c>
    /// when the action failed or the workflow is at the Summary step.
    /// </value>
    public CharacterCreationStep? NextStep { get; init; }

    /// <summary>
    /// Gets the current step after the action has been processed.
    /// </summary>
    /// <value>
    /// The <see cref="CharacterCreationStep"/> that the workflow is currently at.
    /// On success, this is the step that was just completed or navigated to.
    /// On failure, this remains the step where the error occurred.
    /// </value>
    public CharacterCreationStep CurrentStep { get; init; }

    /// <summary>
    /// Gets validation errors if the action failed. Empty if successful.
    /// </summary>
    /// <value>
    /// A read-only list of user-friendly error messages describing why the action
    /// failed. Empty (not null) when <see cref="Success"/> is <c>true</c>.
    /// </value>
    public IReadOnlyList<string> ValidationErrors { get; init; }

    /// <summary>
    /// Gets the updated ViewModel reflecting the new state after the action.
    /// </summary>
    /// <value>
    /// A <see cref="CharacterCreationViewModel"/> rebuilt from the current
    /// <c>CharacterCreationState</c> via <c>IViewModelBuilder</c>.
    /// </value>
    public CharacterCreationViewModel UpdatedViewModel { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a successful step result.
    /// </summary>
    /// <param name="currentStep">The step that the workflow is currently at.</param>
    /// <param name="nextStep">
    /// The next step to advance to. Null if at the final step.
    /// </param>
    /// <param name="viewModel">
    /// The updated ViewModel reflecting the new state.
    /// </param>
    /// <returns>
    /// A <see cref="StepResult"/> with <see cref="Success"/> set to <c>true</c>,
    /// the provided step information, and an empty validation errors list.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = StepResult.Succeeded(
    ///     CharacterCreationStep.Background,
    ///     CharacterCreationStep.Attributes,
    ///     viewModel);
    /// // result.Success == true
    /// // result.CurrentStep == Background
    /// // result.NextStep == Attributes
    /// </code>
    /// </example>
    public static StepResult Succeeded(
        CharacterCreationStep currentStep,
        CharacterCreationStep? nextStep,
        CharacterCreationViewModel viewModel) => new()
    {
        Success = true,
        CurrentStep = currentStep,
        NextStep = nextStep,
        ValidationErrors = Array.Empty<string>(),
        UpdatedViewModel = viewModel
    };

    /// <summary>
    /// Creates a failed step result with validation errors.
    /// </summary>
    /// <param name="currentStep">The step where the error occurred.</param>
    /// <param name="viewModel">
    /// The current ViewModel (unchanged from before the failed action).
    /// </param>
    /// <param name="errors">One or more validation error messages.</param>
    /// <returns>
    /// A <see cref="StepResult"/> with <see cref="Success"/> set to <c>false</c>,
    /// <see cref="NextStep"/> set to <c>null</c>, and the provided error messages.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = StepResult.Failed(
    ///     CharacterCreationStep.Lineage,
    ///     viewModel,
    ///     "Clan-Born lineage requires selecting a flexible attribute bonus.");
    /// // result.Success == false
    /// // result.ValidationErrors.Count == 1
    /// </code>
    /// </example>
    public static StepResult Failed(
        CharacterCreationStep currentStep,
        CharacterCreationViewModel viewModel,
        params string[] errors) => new()
    {
        Success = false,
        CurrentStep = currentStep,
        NextStep = null,
        ValidationErrors = errors,
        UpdatedViewModel = viewModel
    };

    /// <summary>
    /// Creates a failed step result from a list of errors.
    /// </summary>
    /// <param name="currentStep">The step where the error occurred.</param>
    /// <param name="viewModel">
    /// The current ViewModel (unchanged from before the failed action).
    /// </param>
    /// <param name="errors">The list of validation error messages.</param>
    /// <returns>
    /// A <see cref="StepResult"/> with <see cref="Success"/> set to <c>false</c>,
    /// <see cref="NextStep"/> set to <c>null</c>, and the provided error messages.
    /// </returns>
    public static StepResult Failed(
        CharacterCreationStep currentStep,
        CharacterCreationViewModel viewModel,
        IReadOnlyList<string> errors) => new()
    {
        Success = false,
        CurrentStep = currentStep,
        NextStep = null,
        ValidationErrors = errors,
        UpdatedViewModel = viewModel
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation for debugging.
    /// </summary>
    /// <returns>
    /// A human-readable string showing the success status, current step, and
    /// next step (if successful) or error count (if failed).
    /// </returns>
    public override string ToString() =>
        Success
            ? $"Success: {CurrentStep} → {NextStep}"
            : $"Failed at {CurrentStep}: {ValidationErrors.Count} error(s)";
}
