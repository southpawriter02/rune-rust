// ═══════════════════════════════════════════════════════════════════════════════
// SummaryScreen.cs
// Step 6 screen: Character summary and name entry in the character creation
// wizard. Displays a complete summary of all creation choices (lineage,
// background, attributes, archetype, specialization) along with derived stats,
// abilities, and equipment previews. Collects the character name via a text
// prompt with real-time validation, then presents a final confirmation prompt.
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
/// Step 6: Character summary and name entry screen for the character creation wizard.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="SummaryScreen"/> is the final step in the character creation workflow.
/// It presents a comprehensive summary of all selections made in Steps 1-5, then
/// collects the character name and asks for final confirmation.
/// </para>
/// <para>
/// <strong>Summary Sections:</strong>
/// </para>
/// <list type="number">
///   <item><description>Lineage and background selections</description></item>
///   <item><description>Attribute allocation table with all five core attributes</description></item>
///   <item><description>Archetype and specialization selections</description></item>
///   <item><description>Derived stats preview (HP, Stamina, Aether Pool)</description></item>
///   <item><description>Abilities preview (archetype + specialization Tier 1)</description></item>
///   <item><description>Starting equipment preview</description></item>
/// </list>
/// <para>
/// <strong>Name Input:</strong> The character name is collected via a
/// <see cref="TextPrompt{T}"/> with validation using <see cref="INameValidator"/>.
/// Names must be 2-20 characters, contain only ASCII letters/spaces/hyphens,
/// and pass the profanity filter.
/// </para>
/// <para>
/// <strong>Final Confirmation:</strong> After name entry, a "Confirm &amp; Begin Saga"
/// prompt is shown. The player can confirm, go back, or cancel.
/// </para>
/// <para>
/// <strong>Return Value:</strong> Returns the character name <c>string</c>
/// in <see cref="ScreenResult.Selection"/>.
/// </para>
/// </remarks>
/// <seealso cref="ICreationScreen"/>
/// <seealso cref="INameValidator"/>
public class SummaryScreen : ICreationScreen
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Validator for character name input.
    /// </summary>
    private readonly INameValidator _nameValidator;

    /// <summary>
    /// Logger for screen operations and diagnostics.
    /// </summary>
    private readonly ILogger<SummaryScreen> _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new summary and name entry screen.
    /// </summary>
    /// <param name="nameValidator">Validator for character name input.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="nameValidator"/> is <c>null</c>.
    /// </exception>
    public SummaryScreen(
        INameValidator nameValidator,
        ILogger<SummaryScreen>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(nameValidator);
        _nameValidator = nameValidator;
        _logger = logger ?? NullLogger<SummaryScreen>.Instance;
        _logger.LogDebug("SummaryScreen initialized");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ICreationScreen IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public CharacterCreationStep Step => CharacterCreationStep.Summary;

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// Renders the full character summary, collects the character name, and
    /// presents a final confirmation prompt. The method does not return until
    /// the player confirms, goes back, or cancels.
    /// </para>
    /// </remarks>
    public Task<ScreenResult> DisplayAsync(
        CharacterCreationViewModel viewModel,
        IAnsiConsole console,
        CancellationToken ct = default)
    {
        _logger.LogDebug("Displaying summary and name entry screen");

        // Render step header
        StepHeaderRenderer.Render(console, viewModel);
        StepHeaderRenderer.RenderProgress(console, viewModel);

        // Show validation errors from previous attempt
        OptionListRenderer.RenderValidationErrors(console, viewModel.ValidationErrors);

        // Render full character summary
        RenderCharacterSummary(console, viewModel);

        // Collect character name
        var name = PromptCharacterName(console);
        if (name == null)
        {
            _logger.LogDebug("Name entry cancelled, going back");
            return Task.FromResult(ScreenResult.GoBack());
        }

        _logger.LogDebug("Character name entered: {Name}", name);

        // Final confirmation
        console.WriteLine();
        var choices = new List<string>
        {
            "Confirm & Begin Saga",
            OptionListRenderer.BackChoice,
            OptionListRenderer.CancelChoice
        };

        var prompt = new SelectionPrompt<string>()
            .Title($"[gold1]Finalize [bold cyan]{Markup.Escape(name)}[/]?[/]")
            .HighlightStyle(new Style(Color.Cyan1, decoration: Decoration.Bold))
            .AddChoices(choices);

        var selection = console.Prompt(prompt);
        _logger.LogDebug("Final confirmation selection: {Selection}", selection);

        if (OptionListRenderer.IsBack(selection))
        {
            _logger.LogDebug("User selected Back from final confirmation");
            return Task.FromResult(ScreenResult.GoBack());
        }

        if (OptionListRenderer.IsCancel(selection))
        {
            _logger.LogDebug("User selected Cancel from final confirmation");
            return Task.FromResult(ScreenResult.Cancel());
        }

        _logger.LogInformation("Character creation confirmed with name: {Name}", name);
        return Task.FromResult(ScreenResult.Selected(name));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CHARACTER NAME INPUT
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Prompts the player to enter a character name with validation.
    /// </summary>
    /// <param name="console">The Spectre.Console instance for rendering.</param>
    /// <returns>The validated character name, or <c>null</c> if the player cancels.</returns>
    private string? PromptCharacterName(IAnsiConsole console)
    {
        _logger.LogDebug("Prompting for character name input");

        console.MarkupLine("[grey italic]Enter your character's name (2-20 characters, letters/spaces/hyphens only).[/]");
        console.WriteLine();

        // Retry loop for name validation
        while (true)
        {
            var namePrompt = new TextPrompt<string>("[gold1]Character Name:[/]")
                .PromptStyle(new Style(Color.Cyan1))
                .AllowEmpty();

            var input = console.Prompt(namePrompt);

            // Empty input means go back
            if (string.IsNullOrWhiteSpace(input))
            {
                _logger.LogDebug("Empty name input — treating as back navigation");
                return null;
            }

            var trimmed = input.Trim();
            var result = _nameValidator.Validate(trimmed);

            if (result.IsValid)
            {
                _logger.LogDebug("Name validated successfully: {Name}", trimmed);
                return trimmed;
            }

            // Show validation error
            _logger.LogDebug(
                "Name validation failed: {Error}, SuggestedName={Suggestion}",
                result.ErrorMessage,
                result.SuggestedName);

            console.MarkupLine($"  [red]{Markup.Escape(result.ErrorMessage ?? "Invalid name.")}[/]");

            if (!string.IsNullOrEmpty(result.SuggestedName))
            {
                console.MarkupLine(
                    $"  [yellow]Suggestion: [bold]{Markup.Escape(result.SuggestedName)}[/][/]");
            }

            console.WriteLine();
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SUMMARY RENDERING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Renders the full character summary including all selections and previews.
    /// </summary>
    /// <param name="console">The Spectre.Console instance for rendering.</param>
    /// <param name="viewModel">The current ViewModel with all selection data.</param>
    private void RenderCharacterSummary(IAnsiConsole console, CharacterCreationViewModel viewModel)
    {
        _logger.LogDebug("Rendering character summary");

        // Selections summary table
        RenderSelectionsTable(console, viewModel);

        // Derived stats preview
        if (viewModel.DerivedStatsPreview.HasValue && viewModel.DerivedStatsPreview.Value.HasValues)
        {
            RenderDerivedStatsPreview(console, viewModel.DerivedStatsPreview.Value);
        }

        // Abilities preview
        if (viewModel.AbilitiesPreview.HasValue && viewModel.AbilitiesPreview.Value.HasAbilities)
        {
            RenderAbilitiesPreview(console, viewModel.AbilitiesPreview.Value);
        }

        // Equipment preview
        if (viewModel.EquipmentPreview.HasValue && viewModel.EquipmentPreview.Value.HasItems)
        {
            RenderEquipmentPreview(console, viewModel.EquipmentPreview.Value);
        }
    }

    /// <summary>
    /// Renders the selections summary as a two-column table.
    /// </summary>
    /// <param name="console">The Spectre.Console instance for rendering.</param>
    /// <param name="viewModel">The current ViewModel with selection display names.</param>
    private static void RenderSelectionsTable(IAnsiConsole console, CharacterCreationViewModel viewModel)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .Title("[bold yellow]Character Summary[/]")
            .AddColumn(new TableColumn("[bold]Choice[/]"))
            .AddColumn(new TableColumn("[bold]Selection[/]"));

        // Lineage
        table.AddRow(
            "[bold]Lineage[/]",
            FormatSelection(viewModel.SelectedLineageName));

        // Background
        table.AddRow(
            "[bold]Background[/]",
            FormatSelection(viewModel.SelectedBackgroundName));

        // Archetype
        table.AddRow(
            "[bold]Archetype[/]",
            FormatSelection(viewModel.SelectedArchetypeName));

        // Specialization
        table.AddRow(
            "[bold]Specialization[/]",
            FormatSelection(viewModel.SelectedSpecializationName));

        console.Write(table);
        console.WriteLine();
    }

    /// <summary>
    /// Renders the derived stats preview (HP, Stamina, AP).
    /// </summary>
    /// <param name="console">The Spectre.Console instance for rendering.</param>
    /// <param name="stats">The derived stats preview to display.</param>
    private static void RenderDerivedStatsPreview(IAnsiConsole console, DerivedStatsPreview stats)
    {
        var table = new Table()
            .Border(TableBorder.Simple)
            .BorderColor(Color.Grey)
            .Title("[bold green]Derived Stats[/]")
            .AddColumn(new TableColumn("[bold]Stat[/]"))
            .AddColumn(new TableColumn("[bold]Value[/]").Centered())
            .AddColumn(new TableColumn("[bold]Breakdown[/]"));

        // HP
        var hpBreakdown = new List<string>();
        if (stats.HpFromLineage > 0) hpBreakdown.Add($"Lineage +{stats.HpFromLineage}");
        if (stats.HpFromArchetype > 0) hpBreakdown.Add($"Archetype +{stats.HpFromArchetype}");

        table.AddRow(
            "[bold green]Max HP[/]",
            $"[green]{stats.MaxHp}[/]",
            hpBreakdown.Count > 0 ? string.Join(", ", hpBreakdown) : "Base");

        // Stamina
        var staminaBreakdown = new List<string>();
        if (stats.StaminaFromArchetype > 0) staminaBreakdown.Add($"Archetype +{stats.StaminaFromArchetype}");

        table.AddRow(
            "[bold yellow]Max Stamina[/]",
            $"[yellow]{stats.MaxStamina}[/]",
            staminaBreakdown.Count > 0 ? string.Join(", ", staminaBreakdown) : "Base");

        // Aether Pool
        var apBreakdown = new List<string>();
        if (stats.ApFromLineage > 0) apBreakdown.Add($"Lineage +{stats.ApFromLineage}");
        if (stats.ApFromArchetype > 0) apBreakdown.Add($"Archetype +{stats.ApFromArchetype}");

        table.AddRow(
            "[bold blue]Max AP[/]",
            $"[blue]{stats.MaxAp}[/]",
            apBreakdown.Count > 0 ? string.Join(", ", apBreakdown) : "Base");

        console.Write(table);
        console.WriteLine();
    }

    /// <summary>
    /// Renders the abilities preview (archetype starting + specialization Tier 1).
    /// </summary>
    /// <param name="console">The Spectre.Console instance for rendering.</param>
    /// <param name="abilities">The abilities preview to display.</param>
    private static void RenderAbilitiesPreview(IAnsiConsole console, AbilitiesPreview abilities)
    {
        console.MarkupLine("[bold cyan]Starting Abilities[/]");

        if (abilities.ArchetypeAbilities?.Count > 0)
        {
            console.MarkupLine("  [grey]Archetype:[/]");
            foreach (var ability in abilities.ArchetypeAbilities)
            {
                console.MarkupLine($"    [cyan]• {Markup.Escape(ability)}[/]");
            }
        }

        if (abilities.HasSpecializationAbilities)
        {
            console.MarkupLine("  [grey]Specialization (Tier 1):[/]");
            foreach (var ability in abilities.SpecializationAbilities)
            {
                console.MarkupLine($"    [cyan]• {Markup.Escape(ability)}[/]");
            }
        }

        console.MarkupLine($"  [grey]Total: {abilities.TotalCount} abilities[/]");
        console.WriteLine();
    }

    /// <summary>
    /// Renders the equipment preview from the background selection.
    /// </summary>
    /// <param name="console">The Spectre.Console instance for rendering.</param>
    /// <param name="equipment">The equipment preview to display.</param>
    private static void RenderEquipmentPreview(IAnsiConsole console, EquipmentPreview equipment)
    {
        console.MarkupLine(
            $"[bold cyan]Starting Equipment[/] [grey](from {Markup.Escape(equipment.FromBackground)})[/]");

        foreach (var item in equipment.Items)
        {
            var qtyText = item.Quantity > 1 ? $" x{item.Quantity}" : "";
            var typeText = !string.IsNullOrEmpty(item.ItemType) ? $" [grey]({item.ItemType})[/]" : "";
            console.MarkupLine($"    [cyan]• {Markup.Escape(item.Name)}{qtyText}[/]{typeText}");
        }

        console.WriteLine();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FORMATTING HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats a selection value for display, showing "Not selected" for null values.
    /// </summary>
    /// <param name="value">The selection display name, or <c>null</c> if not yet selected.</param>
    /// <returns>Formatted markup string for the selection cell.</returns>
    private static string FormatSelection(string? value) =>
        !string.IsNullOrEmpty(value)
            ? $"[bold cyan]{Markup.Escape(value)}[/]"
            : "[grey italic]Not selected[/]";
}
