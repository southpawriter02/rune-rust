// ═══════════════════════════════════════════════════════════════════════════════
// ICreationScreen.cs
// Contract for character creation step screens in the six-step creation wizard.
// Each screen handles display and input for a single CharacterCreationStep,
// returning a ScreenResult that indicates the user's action (continue with
// selection, go back, or cancel). Screens use Spectre.Console for rendering
// and receive the current CharacterCreationViewModel for display data.
// Version: 0.17.5f
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.Screens;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using Spectre.Console;

/// <summary>
/// Contract for character creation step screens in the six-step creation wizard.
/// </summary>
/// <remarks>
/// <para>
/// Each implementation of <see cref="ICreationScreen"/> handles a single step in the
/// character creation workflow. The screen is responsible for rendering step-specific
/// content (options, descriptions, previews) and collecting the user's selection via
/// Spectre.Console prompts.
/// </para>
/// <para>
/// <strong>Lifecycle:</strong> The <see cref="DisplayAsync"/> method is called by
/// <c>CreationWizardView</c> each time the step needs to be shown. The method renders
/// context, presents interactive prompts, and returns a <see cref="ScreenResult"/>
/// indicating the user's action. The orchestrator then processes the result.
/// </para>
/// <para>
/// <strong>Navigation:</strong> Each screen includes "Back" and "Cancel" navigation
/// options alongside its step-specific selections. The screen returns
/// <see cref="ScreenResult.GoBack"/> or <see cref="ScreenResult.Cancel"/> for these
/// actions, allowing the orchestrator to handle navigation uniformly.
/// </para>
/// <para>
/// <strong>Implementations:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>Step 1: <c>LineageScreen</c> — 4 lineage options with Clan-Born flexible bonus sub-prompt</description></item>
///   <item><description>Step 2: <c>BackgroundScreen</c> — 6 background options with skills/equipment preview</description></item>
///   <item><description>Step 3: <c>AttributeScreen</c> — Point-buy allocation with derived stats preview</description></item>
///   <item><description>Step 4: <c>ArchetypeScreen</c> — 4 archetype options with PERMANENT warning</description></item>
///   <item><description>Step 5: <c>SpecializationScreen</c> — Archetype-filtered specialization options</description></item>
///   <item><description>Step 6: <c>SummaryScreen</c> — Full summary, name input, and confirmation</description></item>
/// </list>
/// </remarks>
/// <seealso cref="ScreenResult"/>
/// <seealso cref="ScreenAction"/>
/// <seealso cref="CharacterCreationViewModel"/>
/// <seealso cref="CharacterCreationStep"/>
public interface ICreationScreen
{
    /// <summary>
    /// Gets the character creation step this screen handles.
    /// </summary>
    /// <value>
    /// The <see cref="CharacterCreationStep"/> enum value identifying which step
    /// in the workflow this screen is responsible for rendering.
    /// </value>
    CharacterCreationStep Step { get; }

    /// <summary>
    /// Displays the step screen, collects user input, and returns the result.
    /// </summary>
    /// <param name="viewModel">
    /// The current <see cref="CharacterCreationViewModel"/> containing display data
    /// for the step: step information, navigation state, current selections, preview
    /// data, and validation status.
    /// </param>
    /// <param name="console">
    /// The <see cref="IAnsiConsole"/> instance for Spectre.Console rendering.
    /// Injected for testability.
    /// </param>
    /// <param name="ct">Cancellation token for async operations.</param>
    /// <returns>
    /// A <see cref="ScreenResult"/> containing the user's action:
    /// <list type="bullet">
    ///   <item><description><see cref="ScreenAction.Continue"/> — User made a selection (available in <see cref="ScreenResult.Selection"/>)</description></item>
    ///   <item><description><see cref="ScreenAction.GoBack"/> — User requested navigation to the previous step</description></item>
    ///   <item><description><see cref="ScreenAction.Cancel"/> — User requested cancellation of the creation workflow</description></item>
    /// </list>
    /// </returns>
    /// <example>
    /// <code>
    /// var result = await screen.DisplayAsync(viewModel, AnsiConsole.Console);
    /// if (result.Action == ScreenAction.Continue)
    /// {
    ///     var selection = result.Selection; // Type varies by screen
    /// }
    /// </code>
    /// </example>
    Task<ScreenResult> DisplayAsync(
        CharacterCreationViewModel viewModel,
        IAnsiConsole console,
        CancellationToken ct = default);
}

// ═══════════════════════════════════════════════════════════════════════════════
// SCREEN RESULT
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Represents the outcome of a screen interaction in the creation wizard.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ScreenResult"/> encapsulates the user's action and optional selection
/// data from a creation screen. The <see cref="Action"/> property indicates what the
/// user chose to do (continue, go back, or cancel), and <see cref="Selection"/>
/// contains the typed selection data when the action is <see cref="ScreenAction.Continue"/>.
/// </para>
/// <para>
/// <strong>Selection types by step:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>Lineage: <c>(Lineage, CoreAttribute?)</c> tuple</description></item>
///   <item><description>Background: <see cref="RuneAndRust.Domain.Enums.Background"/> enum</description></item>
///   <item><description>Attributes: <see cref="AttributeAllocationState"/> value object</description></item>
///   <item><description>Archetype: <see cref="RuneAndRust.Domain.Enums.Archetype"/> enum</description></item>
///   <item><description>Specialization: <see cref="SpecializationId"/> enum</description></item>
///   <item><description>Summary: <c>string</c> (character name)</description></item>
/// </list>
/// </remarks>
/// <seealso cref="ScreenAction"/>
/// <seealso cref="ICreationScreen"/>
public readonly record struct ScreenResult
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the action the user performed on the screen.
    /// </summary>
    /// <value>
    /// A <see cref="ScreenAction"/> indicating whether the user continued with a
    /// selection, navigated back, or cancelled the workflow.
    /// </value>
    public ScreenAction Action { get; init; }

    /// <summary>
    /// Gets the user's selection data, if any.
    /// </summary>
    /// <value>
    /// The selection object when <see cref="Action"/> is <see cref="ScreenAction.Continue"/>;
    /// <c>null</c> when the action is <see cref="ScreenAction.GoBack"/> or
    /// <see cref="ScreenAction.Cancel"/>.
    /// </value>
    /// <remarks>
    /// The concrete type depends on the step screen that produced this result.
    /// The orchestrator casts this value to the expected type based on the current step.
    /// </remarks>
    public object? Selection { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a result indicating the user made a selection and wants to continue.
    /// </summary>
    /// <param name="selection">The user's selection data for this step.</param>
    /// <returns>
    /// A <see cref="ScreenResult"/> with <see cref="Action"/> set to
    /// <see cref="ScreenAction.Continue"/> and the provided selection.
    /// </returns>
    public static ScreenResult Selected(object selection) => new()
    {
        Action = ScreenAction.Continue,
        Selection = selection
    };

    /// <summary>
    /// Creates a result indicating the user wants to go back to the previous step.
    /// </summary>
    /// <returns>
    /// A <see cref="ScreenResult"/> with <see cref="Action"/> set to
    /// <see cref="ScreenAction.GoBack"/> and <c>null</c> selection.
    /// </returns>
    public static ScreenResult GoBack() => new()
    {
        Action = ScreenAction.GoBack,
        Selection = null
    };

    /// <summary>
    /// Creates a result indicating the user wants to cancel character creation.
    /// </summary>
    /// <returns>
    /// A <see cref="ScreenResult"/> with <see cref="Action"/> set to
    /// <see cref="ScreenAction.Cancel"/> and <c>null</c> selection.
    /// </returns>
    public static ScreenResult Cancel() => new()
    {
        Action = ScreenAction.Cancel,
        Selection = null
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation for debugging.
    /// </summary>
    /// <returns>
    /// A string showing the action and selection type, e.g.,
    /// "Continue: Archetype" or "GoBack" or "Cancel".
    /// </returns>
    public override string ToString() =>
        Action == ScreenAction.Continue
            ? $"Continue: {Selection?.GetType().Name ?? "null"}"
            : Action.ToString();
}

// ═══════════════════════════════════════════════════════════════════════════════
// SCREEN ACTION ENUM
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Actions a user can take on a creation screen.
/// </summary>
/// <remarks>
/// Used by <see cref="ScreenResult"/> to indicate what the user chose to do
/// on a creation screen. The <c>CreationWizardView</c> orchestrator uses this
/// to route to the appropriate controller method or navigation action.
/// </remarks>
/// <seealso cref="ScreenResult"/>
/// <seealso cref="ICreationScreen"/>
public enum ScreenAction
{
    /// <summary>
    /// The user made a selection and wants to continue to the next step.
    /// The selection data is available in <see cref="ScreenResult.Selection"/>.
    /// </summary>
    Continue = 0,

    /// <summary>
    /// The user wants to navigate back to the previous step.
    /// All current selections are preserved.
    /// </summary>
    GoBack = 1,

    /// <summary>
    /// The user wants to cancel the character creation workflow.
    /// All state will be discarded.
    /// </summary>
    Cancel = 2
}
