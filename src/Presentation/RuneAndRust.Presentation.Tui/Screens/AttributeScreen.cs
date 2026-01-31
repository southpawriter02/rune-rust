// ═══════════════════════════════════════════════════════════════════════════════
// AttributeScreen.cs
// Step 3 screen: Attribute allocation in the character creation wizard. Supports
// two modes — Simple (archetype recommended build via SelectionPrompt) and
// Advanced (manual point-buy with per-attribute increase/decrease actions).
// Displays a live attribute table with derived stats preview, point cost
// information, and mode toggle. Point-buy costs: values 2-8 cost 1pt each,
// values 9-10 cost 2pt each.
// Version: 0.17.5f
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.Screens;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Presentation.Tui.Components;
using Spectre.Console;

/// <summary>
/// Step 3: Attribute allocation screen for the character creation wizard.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="AttributeScreen"/> provides two allocation modes:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <strong>Simple Mode:</strong> The player selects an archetype's recommended
///       build via a <see cref="SelectionPrompt{T}"/>. Attributes are auto-set and
///       cannot be manually adjusted. Recommended for new players.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Advanced Mode:</strong> The player manually allocates points across
///       five core attributes (Might, Finesse, Wits, Will, Sturdiness) using a
///       point-buy system. Each attribute starts at 1 with a pool of 15 points
///       (14 for Adept). Values 2-8 cost 1 point each; values 9-10 cost 2 points each.
///     </description>
///   </item>
/// </list>
/// <para>
/// <strong>Derived Stats Preview:</strong> A live preview shows how the current
/// attribute allocation affects HP, Stamina, and Aether Pool, including bonuses
/// from lineage and archetype if selected.
/// </para>
/// <para>
/// <strong>Return Value:</strong> Returns an <see cref="AttributeAllocationState"/>
/// in <see cref="ScreenResult.Selection"/>.
/// </para>
/// </remarks>
/// <seealso cref="ICreationScreen"/>
/// <seealso cref="IArchetypeProvider"/>
/// <seealso cref="IViewModelBuilder"/>
/// <seealso cref="AttributeAllocationState"/>
public class AttributeScreen : ICreationScreen
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Minimum allowed attribute value during point-buy allocation.
    /// </summary>
    private const int MinAttributeValue = 1;

    /// <summary>
    /// Maximum allowed attribute value during point-buy allocation.
    /// </summary>
    private const int MaxAttributeValue = 10;

    /// <summary>
    /// Threshold above which attribute increases cost 2 points instead of 1.
    /// Values 2-8 cost 1 point each; values 9-10 cost 2 points each.
    /// </summary>
    private const int ExpensiveThreshold = 8;

    /// <summary>
    /// Standard point pool for most archetypes.
    /// </summary>
    private const int StandardPointPool = 15;

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Provider for archetype definitions and recommended builds.
    /// </summary>
    private readonly IArchetypeProvider _archetypeProvider;

    /// <summary>
    /// Builder for derived stats preview during allocation.
    /// </summary>
    private readonly IViewModelBuilder _viewModelBuilder;

    /// <summary>
    /// Logger for screen operations and diagnostics.
    /// </summary>
    private readonly ILogger<AttributeScreen> _logger;

    /// <summary>
    /// Ordered array of core attribute enum values for consistent display.
    /// </summary>
    private static readonly CoreAttribute[] CoreAttributes =
    {
        CoreAttribute.Might, CoreAttribute.Finesse,
        CoreAttribute.Wits, CoreAttribute.Will, CoreAttribute.Sturdiness
    };

    /// <summary>
    /// Ordered array of archetype enum values for recommended build selection.
    /// </summary>
    private static readonly Archetype[] ArchetypeValues =
    {
        Archetype.Warrior, Archetype.Skirmisher,
        Archetype.Mystic, Archetype.Adept
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new attribute allocation screen.
    /// </summary>
    /// <param name="archetypeProvider">Provider for archetype definitions and recommended builds.</param>
    /// <param name="viewModelBuilder">Builder for derived stats preview during allocation.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="archetypeProvider"/> or <paramref name="viewModelBuilder"/> is <c>null</c>.
    /// </exception>
    public AttributeScreen(
        IArchetypeProvider archetypeProvider,
        IViewModelBuilder viewModelBuilder,
        ILogger<AttributeScreen>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(archetypeProvider);
        ArgumentNullException.ThrowIfNull(viewModelBuilder);
        _archetypeProvider = archetypeProvider;
        _viewModelBuilder = viewModelBuilder;
        _logger = logger ?? NullLogger<AttributeScreen>.Instance;
        _logger.LogDebug("AttributeScreen initialized");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ICreationScreen IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public CharacterCreationStep Step => CharacterCreationStep.Attributes;

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// The attribute screen operates as an internal loop. In Simple mode, it
    /// presents a single <see cref="SelectionPrompt{T}"/> with recommended builds.
    /// In Advanced mode, it loops through an action menu (increase/decrease
    /// attributes, toggle mode, reset, confirm) until the player confirms or
    /// navigates away.
    /// </para>
    /// </remarks>
    public Task<ScreenResult> DisplayAsync(
        CharacterCreationViewModel viewModel,
        IAnsiConsole console,
        CancellationToken ct = default)
    {
        _logger.LogDebug("Displaying attribute allocation screen");

        // Render step header
        StepHeaderRenderer.Render(console, viewModel);
        StepHeaderRenderer.RenderProgress(console, viewModel);

        // Show validation errors from previous attempt
        OptionListRenderer.RenderValidationErrors(console, viewModel.ValidationErrors);

        // Start in Simple mode by default — prompt for mode choice first
        var modeChoice = PromptModeSelection(console);

        if (modeChoice == "simple")
        {
            _logger.LogDebug("Player chose Simple mode for attribute allocation");
            return HandleSimpleMode(viewModel, console, ct);
        }

        if (modeChoice == "advanced")
        {
            _logger.LogDebug("Player chose Advanced mode for attribute allocation");
            return HandleAdvancedMode(viewModel, console, ct);
        }

        // Navigation action from mode prompt
        if (modeChoice == "back")
            return Task.FromResult(ScreenResult.GoBack());

        return Task.FromResult(ScreenResult.Cancel());
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // MODE SELECTION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Prompts the player to choose between Simple and Advanced allocation modes.
    /// </summary>
    /// <param name="console">The Spectre.Console instance for rendering.</param>
    /// <returns>
    /// <c>"simple"</c>, <c>"advanced"</c>, <c>"back"</c>, or <c>"cancel"</c>.
    /// </returns>
    private string PromptModeSelection(IAnsiConsole console)
    {
        _logger.LogDebug("Prompting for allocation mode selection");

        console.MarkupLine("[grey italic]Choose how you'd like to allocate your attributes.[/]");
        console.WriteLine();

        var choices = new List<string>
        {
            "Simple — Use a recommended build",
            "Advanced — Manual point-buy allocation"
        };

        var prompt = new SelectionPrompt<string>()
            .Title("[gold1]Allocation Mode:[/]")
            .HighlightStyle(new Style(Color.Cyan1, decoration: Decoration.Bold));
        prompt.AddChoices(choices);
        prompt.AddChoice(OptionListRenderer.BackChoice);
        prompt.AddChoice(OptionListRenderer.CancelChoice);

        var selection = console.Prompt(prompt);
        _logger.LogDebug("Mode selection: {Selection}", selection);

        if (OptionListRenderer.IsBack(selection)) return "back";
        if (OptionListRenderer.IsCancel(selection)) return "cancel";

        return selection.StartsWith("Simple", StringComparison.OrdinalIgnoreCase)
            ? "simple"
            : "advanced";
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SIMPLE MODE
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Handles Simple mode — player selects a recommended build from a list.
    /// </summary>
    /// <param name="viewModel">The current ViewModel with step display data.</param>
    /// <param name="console">The Spectre.Console instance for rendering.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="ScreenResult"/> with the selected <see cref="AttributeAllocationState"/>.</returns>
    private Task<ScreenResult> HandleSimpleMode(
        CharacterCreationViewModel viewModel,
        IAnsiConsole console,
        CancellationToken ct)
    {
        _logger.LogDebug("Entering Simple mode — displaying recommended builds");

        // Gather recommended builds from all archetypes
        var builds = new List<(string Display, RecommendedBuild Build, Archetype Archetype)>();

        foreach (var archetype in ArchetypeValues)
        {
            var build = _archetypeProvider.GetRecommendedBuild(archetype);
            if (build == null) continue;

            var def = _archetypeProvider.GetArchetype(archetype);
            var archetypeName = def?.DisplayName ?? archetype.ToString();
            var display = $"{archetypeName} — {build.Value.GetDisplaySummary()}";
            builds.Add((display, build.Value, archetype));
        }

        if (builds.Count == 0)
        {
            _logger.LogWarning("No recommended builds found from archetype provider");
            console.MarkupLine("[red]No recommended builds available.[/]");
            return Task.FromResult(ScreenResult.GoBack());
        }

        // Render builds table
        RenderRecommendedBuildsTable(console, builds);

        // Build selection prompt
        var choices = builds.Select(b => b.Display).ToList();
        var prompt = OptionListRenderer.CreatePrompt(
            "Choose a recommended build:",
            choices,
            canGoBack: true);

        var selection = console.Prompt(prompt);
        _logger.LogDebug("Simple mode selection: {Selection}", selection);

        // Check navigation
        var navResult = OptionListRenderer.TryParseNavigation(selection);
        if (navResult.HasValue)
        {
            // If back, re-display to pick mode again
            return navResult.Value.Action == ScreenAction.GoBack
                ? DisplayAsync(viewModel, console, ct)
                : Task.FromResult(navResult.Value);
        }

        // Map selection to build
        var selectedIndex = choices.IndexOf(selection);
        if (selectedIndex < 0 || selectedIndex >= builds.Count)
        {
            _logger.LogWarning("Invalid recommended build selection index: {Index}", selectedIndex);
            return Task.FromResult(ScreenResult.Cancel());
        }

        var (_, selectedBuild, selectedArchetype) = builds[selectedIndex];
        var archetypeId = selectedArchetype.ToString().ToLowerInvariant();
        var totalPoints = selectedBuild.TotalAttributePoints;

        var state = AttributeAllocationState.CreateFromRecommendedBuild(
            archetypeId,
            selectedBuild.Might,
            selectedBuild.Finesse,
            selectedBuild.Wits,
            selectedBuild.Will,
            selectedBuild.Sturdiness,
            totalPoints);

        _logger.LogInformation(
            "Simple mode allocation confirmed: {Build}, State={State}",
            selectedBuild.GetDisplaySummary(),
            state);

        return Task.FromResult(ScreenResult.Selected(state));
    }

    /// <summary>
    /// Renders a table of recommended builds for Simple mode selection.
    /// </summary>
    /// <param name="console">The Spectre.Console instance for rendering.</param>
    /// <param name="builds">List of recommended builds with display text and data.</param>
    private void RenderRecommendedBuildsTable(
        IAnsiConsole console,
        List<(string Display, RecommendedBuild Build, Archetype Archetype)> builds)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn(new TableColumn("[bold]Build[/]").Centered())
            .AddColumn(new TableColumn("[bold]MIGHT[/]").Centered())
            .AddColumn(new TableColumn("[bold]FIN[/]").Centered())
            .AddColumn(new TableColumn("[bold]WITS[/]").Centered())
            .AddColumn(new TableColumn("[bold]WILL[/]").Centered())
            .AddColumn(new TableColumn("[bold]STUR[/]").Centered())
            .AddColumn(new TableColumn("[bold]Total[/]").Centered());

        foreach (var (_, build, archetype) in builds)
        {
            var def = _archetypeProvider.GetArchetype(archetype);
            var name = def?.DisplayName ?? archetype.ToString();

            table.AddRow(
                $"[bold cyan]{Markup.Escape(name)}[/]",
                build.Might.ToString(),
                build.Finesse.ToString(),
                build.Wits.ToString(),
                build.Will.ToString(),
                build.Sturdiness.ToString(),
                $"[yellow]{build.TotalAttributePoints}[/]");
        }

        console.Write(table);
        console.WriteLine();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ADVANCED MODE
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Handles Advanced mode — interactive point-buy attribute allocation loop.
    /// </summary>
    /// <param name="viewModel">The current ViewModel with step display data.</param>
    /// <param name="console">The Spectre.Console instance for rendering.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="ScreenResult"/> with the confirmed <see cref="AttributeAllocationState"/>.</returns>
    private Task<ScreenResult> HandleAdvancedMode(
        CharacterCreationViewModel viewModel,
        IAnsiConsole console,
        CancellationToken ct)
    {
        var state = AttributeAllocationState.CreateAdvancedDefault(StandardPointPool);
        _logger.LogDebug("Starting Advanced mode with {Points} points", StandardPointPool);

        while (true)
        {
            // Render current allocation
            console.WriteLine();
            RenderAttributeTable(console, state);
            RenderPointCostLegend(console);

            // Build action choices
            var actions = BuildAdvancedActionChoices(state);

            var prompt = new SelectionPrompt<string>()
                .Title($"[gold1]Points Remaining: [bold cyan]{state.PointsRemaining}[/] / {state.TotalPoints}[/]")
                .HighlightStyle(new Style(Color.Cyan1, decoration: Decoration.Bold))
                .PageSize(18)
                .AddChoices(actions);

            var selection = console.Prompt(prompt);
            _logger.LogDebug("Advanced mode action: {Action}", selection);

            // Parse and handle the action
            if (selection == "Confirm Allocation" && state.IsComplete)
            {
                _logger.LogInformation("Advanced mode allocation confirmed: {State}", state);
                return Task.FromResult(ScreenResult.Selected(state));
            }

            if (selection == "Reset to Default")
            {
                _logger.LogDebug("Resetting Advanced mode allocation to default");
                state = AttributeAllocationState.CreateAdvancedDefault(StandardPointPool);
                continue;
            }

            if (selection == "Switch to Simple Mode")
            {
                _logger.LogDebug("Switching from Advanced to Simple mode");
                return HandleSimpleMode(viewModel, console, ct);
            }

            if (OptionListRenderer.IsBack(selection))
            {
                // Go back to mode selection
                return DisplayAsync(viewModel, console, ct);
            }

            if (OptionListRenderer.IsCancel(selection))
            {
                return Task.FromResult(ScreenResult.Cancel());
            }

            // Parse attribute adjustment action (e.g., "+ Might", "- Finesse")
            var adjustResult = TryParseAttributeAdjustment(selection, state);
            if (adjustResult.HasValue)
            {
                state = adjustResult.Value;
                _logger.LogDebug("Attribute adjusted. New state: {State}", state);
            }
        }
    }

    /// <summary>
    /// Builds the list of action choices for the Advanced mode prompt.
    /// </summary>
    /// <param name="state">The current allocation state.</param>
    /// <returns>Ordered list of available action strings.</returns>
    private List<string> BuildAdvancedActionChoices(AttributeAllocationState state)
    {
        var actions = new List<string>();

        // Increase/decrease for each attribute
        foreach (var attr in CoreAttributes)
        {
            var current = state.GetAttributeValue(attr);
            var name = attr.ToString();

            // Can increase if below max and have enough points
            if (current < MaxAttributeValue && state.PointsRemaining > 0)
            {
                var cost = GetIncreaseCost(current);
                if (state.PointsRemaining >= cost)
                    actions.Add($"+ {name} ({current} -> {current + 1}, cost {cost}pt)");
            }

            // Can decrease if above min
            if (current > MinAttributeValue)
            {
                var refund = GetDecreasRefund(current);
                actions.Add($"- {name} ({current} -> {current - 1}, refund {refund}pt)");
            }
        }

        // Control actions
        actions.Add("Reset to Default");
        actions.Add("Switch to Simple Mode");

        if (state.IsComplete)
            actions.Add("Confirm Allocation");

        actions.Add(OptionListRenderer.BackChoice);
        actions.Add(OptionListRenderer.CancelChoice);

        return actions;
    }

    /// <summary>
    /// Attempts to parse an attribute adjustment action string and apply it.
    /// </summary>
    /// <param name="selection">The user's selection string (e.g., "+ Might (1 -> 2, cost 1pt)").</param>
    /// <param name="state">The current allocation state.</param>
    /// <returns>The updated state, or <c>null</c> if the selection is not an attribute action.</returns>
    private AttributeAllocationState? TryParseAttributeAdjustment(
        string selection,
        AttributeAllocationState state)
    {
        if (string.IsNullOrEmpty(selection) || selection.Length < 3)
            return null;

        var isIncrease = selection.StartsWith("+ ", StringComparison.Ordinal);
        var isDecrease = selection.StartsWith("- ", StringComparison.Ordinal);
        if (!isIncrease && !isDecrease)
            return null;

        // Extract attribute name (between "+ " and " (")
        var parenIndex = selection.IndexOf(" (", StringComparison.Ordinal);
        if (parenIndex < 0) return null;

        var attrName = selection[2..parenIndex].Trim();

        // Find the matching CoreAttribute
        CoreAttribute? targetAttr = null;
        foreach (var attr in CoreAttributes)
        {
            if (string.Equals(attr.ToString(), attrName, StringComparison.OrdinalIgnoreCase))
            {
                targetAttr = attr;
                break;
            }
        }

        if (targetAttr == null)
        {
            _logger.LogWarning("Could not parse attribute from selection: {Selection}", selection);
            return null;
        }

        var current = state.GetAttributeValue(targetAttr.Value);

        if (isIncrease)
        {
            var newValue = current + 1;
            var cost = GetIncreaseCost(current);
            _logger.LogDebug(
                "Increasing {Attribute}: {Current} -> {New}, cost {Cost}",
                targetAttr.Value, current, newValue, cost);
            return state.WithAttributeValue(targetAttr.Value, newValue, cost);
        }
        else
        {
            var newValue = current - 1;
            var refund = GetDecreasRefund(current);
            _logger.LogDebug(
                "Decreasing {Attribute}: {Current} -> {New}, refund {Refund}",
                targetAttr.Value, current, newValue, refund);
            return state.WithAttributeValue(targetAttr.Value, newValue, -refund);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RENDERING HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Renders the current attribute allocation as a formatted table.
    /// </summary>
    /// <param name="console">The Spectre.Console instance for rendering.</param>
    /// <param name="state">The current allocation state to display.</param>
    private static void RenderAttributeTable(IAnsiConsole console, AttributeAllocationState state)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn(new TableColumn("[bold]Attribute[/]"))
            .AddColumn(new TableColumn("[bold]Value[/]").Centered())
            .AddColumn(new TableColumn("[bold]Bar[/]"))
            .AddColumn(new TableColumn("[bold]Cost to +1[/]").Centered());

        foreach (var attr in CoreAttributes)
        {
            var value = state.GetAttributeValue(attr);
            var bar = new string('█', value) + new string('░', MaxAttributeValue - value);
            var costText = value < MaxAttributeValue
                ? $"{GetIncreaseCost(value)}pt"
                : "[grey]MAX[/]";

            var valueColor = value >= 8 ? "green" : value >= 5 ? "cyan" : "white";

            table.AddRow(
                $"[bold]{attr}[/]",
                $"[{valueColor}]{value}[/]",
                $"[cyan]{bar}[/]",
                costText);
        }

        console.Write(table);
    }

    /// <summary>
    /// Renders the point cost legend below the attribute table.
    /// </summary>
    /// <param name="console">The Spectre.Console instance for rendering.</param>
    private static void RenderPointCostLegend(IAnsiConsole console)
    {
        console.MarkupLine("  [grey]Point costs: values 2-8 = 1pt each, values 9-10 = 2pt each[/]");
        console.WriteLine();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // POINT COST CALCULATIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculates the point cost to increase an attribute from its current value by 1.
    /// </summary>
    /// <param name="currentValue">The current attribute value (1-9).</param>
    /// <returns>1 for values 1-7 (result 2-8), 2 for values 8-9 (result 9-10).</returns>
    private static int GetIncreaseCost(int currentValue) =>
        currentValue >= ExpensiveThreshold ? 2 : 1;

    /// <summary>
    /// Calculates the point refund for decreasing an attribute from its current value by 1.
    /// </summary>
    /// <param name="currentValue">The current attribute value (2-10).</param>
    /// <returns>2 for values 9-10, 1 for values 2-8.</returns>
    private static int GetDecreasRefund(int currentValue) =>
        currentValue > ExpensiveThreshold ? 2 : 1;
}
