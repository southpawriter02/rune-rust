// ═══════════════════════════════════════════════════════════════════════════════
// CreationWizardView.cs
// Orchestrator for the six-step character creation wizard. Routes the workflow
// through step screens (LineageScreen, BackgroundScreen, AttributeScreen,
// ArchetypeScreen, SpecializationScreen, SummaryScreen) based on controller
// state, handles navigation (back/cancel), processes screen results through the
// ICharacterCreationController, and manages validation error display. Returns
// the created Player entity upon successful completion, or null if cancelled.
// Version: 0.17.5f
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.Views;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Presentation.Tui.Screens;
using Spectre.Console;

/// <summary>
/// Orchestrator for the six-step character creation wizard.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="CreationWizardView"/> is the top-level view that drives the character
/// creation workflow. It coordinates between the <see cref="ICharacterCreationController"/>
/// (which manages state and validation) and the individual <see cref="ICreationScreen"/>
/// implementations (which handle rendering and user interaction).
/// </para>
/// <para>
/// <strong>Workflow Loop:</strong>
/// </para>
/// <list type="number">
///   <item><description>Initialize the controller to get the starting state</description></item>
///   <item><description>Get current state and ViewModel from the controller</description></item>
///   <item><description>Route to the appropriate screen based on the current step</description></item>
///   <item><description>Process the screen result (selection, back, cancel)</description></item>
///   <item><description>Submit selections to the controller and handle validation</description></item>
///   <item><description>Repeat until the workflow completes or is cancelled</description></item>
/// </list>
/// <para>
/// <strong>Navigation:</strong>
/// </para>
/// <list type="bullet">
///   <item><description><strong>GoBack:</strong> Calls <c>controller.GoBack()</c> to move to the previous step</description></item>
///   <item><description><strong>Cancel:</strong> Calls <c>controller.Cancel()</c> and returns <c>null</c></description></item>
///   <item><description><strong>Continue:</strong> Submits the selection to the appropriate controller method</description></item>
/// </list>
/// <para>
/// <strong>Summary Step:</strong> The final step calls
/// <c>controller.ConfirmCharacterAsync(name)</c> which returns a
/// <see cref="CharacterCreationResult"/> containing the created <see cref="Player"/>.
/// </para>
/// </remarks>
/// <seealso cref="ICharacterCreationController"/>
/// <seealso cref="ICreationScreen"/>
/// <seealso cref="CharacterCreationState"/>
/// <seealso cref="CharacterCreationViewModel"/>
public class CreationWizardView
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Controller managing character creation state and validation.
    /// </summary>
    private readonly ICharacterCreationController _controller;

    /// <summary>
    /// Screen implementations indexed by their step for routing.
    /// </summary>
    private readonly Dictionary<CharacterCreationStep, ICreationScreen> _screens;

    /// <summary>
    /// Spectre.Console instance for rendering.
    /// </summary>
    private readonly IAnsiConsole _console;

    /// <summary>
    /// Logger for wizard operations and diagnostics.
    /// </summary>
    private readonly ILogger<CreationWizardView> _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new character creation wizard orchestrator.
    /// </summary>
    /// <param name="controller">Controller managing creation state and validation.</param>
    /// <param name="screens">Collection of step screen implementations.</param>
    /// <param name="console">Spectre.Console instance for rendering. Injected for testability.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="controller"/>, <paramref name="screens"/>,
    /// or <paramref name="console"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="screens"/> is empty.
    /// </exception>
    public CreationWizardView(
        ICharacterCreationController controller,
        IEnumerable<ICreationScreen> screens,
        IAnsiConsole console,
        ILogger<CreationWizardView>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(screens);
        ArgumentNullException.ThrowIfNull(console);

        _controller = controller;
        _console = console;
        _logger = logger ?? NullLogger<CreationWizardView>.Instance;

        // Index screens by step for O(1) routing
        _screens = new Dictionary<CharacterCreationStep, ICreationScreen>();
        foreach (var screen in screens)
        {
            _screens[screen.Step] = screen;
            _logger.LogDebug("Registered screen for step {Step}: {Type}", screen.Step, screen.GetType().Name);
        }

        if (_screens.Count == 0)
        {
            throw new ArgumentException("At least one creation screen must be provided.", nameof(screens));
        }

        _logger.LogDebug(
            "CreationWizardView initialized with {ScreenCount} screens",
            _screens.Count);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Runs the character creation wizard to completion.
    /// </summary>
    /// <param name="ct">Cancellation token for async operations.</param>
    /// <returns>
    /// The created <see cref="Player"/> entity if the wizard completes successfully,
    /// or <c>null</c> if the player cancels the creation workflow.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method drives the main wizard loop:
    /// </para>
    /// <list type="number">
    ///   <item><description>Initializes the controller session</description></item>
    ///   <item><description>Loops: get state → find screen → display → process result</description></item>
    ///   <item><description>Returns created Player on success, null on cancellation</description></item>
    /// </list>
    /// <para>
    /// The loop continues until the player either confirms character creation
    /// (returning the Player) or cancels (returning null). Navigation actions
    /// (GoBack) cycle within the loop without exiting.
    /// </para>
    /// </remarks>
    public async Task<Player?> RunAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Starting character creation wizard");

        // Initialize controller session
        var initialState = _controller.Initialize();
        _logger.LogDebug(
            "Controller initialized. Starting step: {Step}, SessionId: {SessionId}",
            initialState.CurrentStep,
            initialState.SessionId);

        // Main wizard loop
        while (_controller.IsSessionActive && !ct.IsCancellationRequested)
        {
            // Get current state and ViewModel
            var (state, viewModel) = _controller.GetCurrentState();
            var currentStep = state.CurrentStep;

            _logger.LogDebug("Wizard loop: current step = {Step}", currentStep);

            // Find the screen for this step
            if (!_screens.TryGetValue(currentStep, out var screen))
            {
                _logger.LogError(
                    "No screen registered for step {Step}. Available: {Available}",
                    currentStep,
                    string.Join(", ", _screens.Keys));
                _console.MarkupLine($"[red]Error: No screen available for step {currentStep}.[/]");
                return null;
            }

            // Clear the console for a fresh screen
            _console.Clear();

            // Display the screen and get the result
            var result = await screen.DisplayAsync(viewModel, _console, ct);
            _logger.LogDebug(
                "Screen result for {Step}: Action={Action}, HasSelection={HasSelection}",
                currentStep,
                result.Action,
                result.Selection != null);

            // Process the result
            switch (result.Action)
            {
                case ScreenAction.Cancel:
                    _logger.LogInformation("Player cancelled character creation at step {Step}", currentStep);
                    _controller.Cancel();
                    _console.MarkupLine("[yellow]Character creation cancelled.[/]");
                    return null;

                case ScreenAction.GoBack:
                    _logger.LogDebug("Player navigated back from step {Step}", currentStep);
                    var backResult = _controller.GoBack();
                    _logger.LogDebug(
                        "GoBack result: Success={Success}, CurrentStep={Step}",
                        backResult.Success,
                        backResult.CurrentStep);
                    continue;

                case ScreenAction.Continue:
                    // Process the selection through the controller
                    var processResult = await ProcessSelectionAsync(currentStep, result.Selection, ct);

                    if (processResult is Player player)
                    {
                        // Character creation completed successfully
                        _logger.LogInformation(
                            "Character creation completed successfully: {PlayerName}",
                            player.Name);
                        return player;
                    }

                    // If processResult is null, there was a validation error
                    // The loop will re-display the current step with errors
                    if (processResult == null)
                    {
                        _logger.LogDebug(
                            "Selection processing had validation errors, re-displaying step {Step}",
                            currentStep);
                    }

                    continue;

                default:
                    _logger.LogWarning("Unknown ScreenAction: {Action}", result.Action);
                    continue;
            }
        }

        if (ct.IsCancellationRequested)
        {
            _logger.LogInformation("Character creation cancelled via cancellation token");
            _controller.Cancel();
        }

        return null;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SELECTION PROCESSING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Routes a screen selection to the appropriate controller method based on the current step.
    /// </summary>
    /// <param name="step">The current character creation step.</param>
    /// <param name="selection">The selection data from the screen (type varies by step).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A <see cref="Player"/> if character creation is complete (Summary step),
    /// a non-null object for successful step advancement, or <c>null</c> if
    /// validation failed.
    /// </returns>
    private async Task<object?> ProcessSelectionAsync(
        CharacterCreationStep step,
        object? selection,
        CancellationToken ct)
    {
        _logger.LogDebug(
            "Processing selection for step {Step}: {SelectionType}",
            step,
            selection?.GetType().Name ?? "null");

        switch (step)
        {
            case CharacterCreationStep.Lineage:
                return await ProcessLineageSelectionAsync(selection);

            case CharacterCreationStep.Background:
                return await ProcessBackgroundSelectionAsync(selection);

            case CharacterCreationStep.Attributes:
                return await ProcessAttributeSelectionAsync(selection);

            case CharacterCreationStep.Archetype:
                return await ProcessArchetypeSelectionAsync(selection);

            case CharacterCreationStep.Specialization:
                return await ProcessSpecializationSelectionAsync(selection);

            case CharacterCreationStep.Summary:
                return await ProcessSummaryConfirmationAsync(selection);

            default:
                _logger.LogError("Unknown step: {Step}", step);
                return null;
        }
    }

    /// <summary>
    /// Processes a lineage selection through the controller.
    /// </summary>
    /// <param name="selection">Expected: <c>(Lineage, CoreAttribute?)</c> tuple.</param>
    /// <returns>A non-null object on success, null on validation failure.</returns>
    private async Task<object?> ProcessLineageSelectionAsync(object? selection)
    {
        if (selection is not ValueTuple<Lineage, CoreAttribute?> tuple)
        {
            _logger.LogWarning("Invalid lineage selection type: {Type}", selection?.GetType().Name);
            return null;
        }

        var (lineage, flexibleBonus) = tuple;
        _logger.LogDebug(
            "Submitting lineage selection: {Lineage}, FlexibleBonus={Bonus}",
            lineage, flexibleBonus);

        var result = await _controller.SelectLineageAsync(lineage, flexibleBonus);
        return HandleStepResult(result);
    }

    /// <summary>
    /// Processes a background selection through the controller.
    /// </summary>
    /// <param name="selection">Expected: <see cref="Background"/> enum.</param>
    /// <returns>A non-null object on success, null on validation failure.</returns>
    private async Task<object?> ProcessBackgroundSelectionAsync(object? selection)
    {
        if (selection is not Background background)
        {
            _logger.LogWarning("Invalid background selection type: {Type}", selection?.GetType().Name);
            return null;
        }

        _logger.LogDebug("Submitting background selection: {Background}", background);
        var result = await _controller.SelectBackgroundAsync(background);
        return HandleStepResult(result);
    }

    /// <summary>
    /// Processes an attribute allocation through the controller.
    /// </summary>
    /// <param name="selection">Expected: <see cref="AttributeAllocationState"/>.</param>
    /// <returns>A non-null object on success, null on validation failure.</returns>
    private async Task<object?> ProcessAttributeSelectionAsync(object? selection)
    {
        if (selection is not AttributeAllocationState attributes)
        {
            _logger.LogWarning("Invalid attribute selection type: {Type}", selection?.GetType().Name);
            return null;
        }

        _logger.LogDebug("Submitting attribute allocation: {State}", attributes);
        var result = await _controller.ConfirmAttributesAsync(attributes);
        return HandleStepResult(result);
    }

    /// <summary>
    /// Processes an archetype selection through the controller.
    /// </summary>
    /// <param name="selection">Expected: <see cref="Archetype"/> enum.</param>
    /// <returns>A non-null object on success, null on validation failure.</returns>
    private async Task<object?> ProcessArchetypeSelectionAsync(object? selection)
    {
        if (selection is not Archetype archetype)
        {
            _logger.LogWarning("Invalid archetype selection type: {Type}", selection?.GetType().Name);
            return null;
        }

        _logger.LogDebug("Submitting archetype selection: {Archetype}", archetype);
        var result = await _controller.SelectArchetypeAsync(archetype);
        return HandleStepResult(result);
    }

    /// <summary>
    /// Processes a specialization selection through the controller.
    /// </summary>
    /// <param name="selection">Expected: <see cref="SpecializationId"/> enum.</param>
    /// <returns>A non-null object on success, null on validation failure.</returns>
    private async Task<object?> ProcessSpecializationSelectionAsync(object? selection)
    {
        if (selection is not SpecializationId specializationId)
        {
            _logger.LogWarning("Invalid specialization selection type: {Type}", selection?.GetType().Name);
            return null;
        }

        _logger.LogDebug("Submitting specialization selection: {SpecializationId}", specializationId);
        var result = await _controller.SelectSpecializationAsync(specializationId);
        return HandleStepResult(result);
    }

    /// <summary>
    /// Processes the summary confirmation through the controller, creating the character.
    /// </summary>
    /// <param name="selection">Expected: <c>string</c> (character name).</param>
    /// <returns>
    /// The created <see cref="Player"/> on success, or null on validation failure.
    /// </returns>
    private async Task<object?> ProcessSummaryConfirmationAsync(object? selection)
    {
        if (selection is not string name)
        {
            _logger.LogWarning("Invalid summary selection type: {Type}", selection?.GetType().Name);
            return null;
        }

        _logger.LogDebug("Confirming character creation with name: {Name}", name);
        var result = await _controller.ConfirmCharacterAsync(name);

        if (result.Success)
        {
            _logger.LogInformation(
                "Character creation succeeded: {Message}",
                result.Message);

            // Display success message
            _console.WriteLine();
            var successPanel = new Panel($"[bold green]{Markup.Escape(result.Message)}[/]")
            {
                Border = BoxBorder.Double,
                BorderStyle = new Style(Color.Green),
                Header = new PanelHeader("[bold green]SUCCESS[/]"),
                Padding = new Padding(1, 0)
            };
            _console.Write(successPanel);
            _console.WriteLine();

            return result.Character;
        }

        // Validation errors
        _logger.LogWarning(
            "Character creation failed: {Message}, Errors={Errors}",
            result.Message,
            string.Join("; ", result.ValidationErrors));

        _console.MarkupLine($"[red]{Markup.Escape(result.Message)}[/]");
        foreach (var error in result.ValidationErrors)
        {
            _console.MarkupLine($"  [red]• {Markup.Escape(error)}[/]");
        }

        return null;
    }

    /// <summary>
    /// Handles a <see cref="StepResult"/> from the controller, logging outcomes
    /// and returning a non-null object on success or null on failure.
    /// </summary>
    /// <param name="result">The step result from the controller.</param>
    /// <returns>A non-null object on success (the result itself), null on failure.</returns>
    private object? HandleStepResult(StepResult result)
    {
        if (result.Success)
        {
            _logger.LogDebug(
                "Step succeeded. Current={Current}, Next={Next}",
                result.CurrentStep,
                result.NextStep);
            return result;
        }

        _logger.LogWarning(
            "Step failed at {Step} with {ErrorCount} error(s): {Errors}",
            result.CurrentStep,
            result.ValidationErrors.Count,
            string.Join("; ", result.ValidationErrors));

        return null;
    }
}
