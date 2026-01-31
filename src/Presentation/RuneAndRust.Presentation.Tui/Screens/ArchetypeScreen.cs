// ═══════════════════════════════════════════════════════════════════════════════
// ArchetypeScreen.cs
// Step 4 screen: Archetype selection in the character creation wizard. Displays
// the four archetype options (Warrior, Skirmisher, Mystic, Adept) with role,
// resource type, HP bonus, key strengths, and starting abilities. This is a
// PERMANENT choice — a confirmation sub-prompt is displayed after selection.
// Version: 0.17.5f
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.Screens;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Presentation.Tui.Components;
using Spectre.Console;

/// <summary>
/// Step 4: Archetype selection screen for the character creation wizard (PERMANENT choice).
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ArchetypeScreen"/> presents the four available archetypes with detailed
/// role descriptions, resource bonuses, starting abilities, and key strengths.
/// This is the only permanent choice in the creation workflow — once confirmed,
/// the archetype cannot be changed.
/// </para>
/// <para>
/// <strong>Confirmation:</strong> After the player selects an archetype, a
/// confirmation prompt is displayed warning that this choice is permanent.
/// The player must explicitly confirm before the selection is finalized.
/// </para>
/// <para>
/// <strong>Return Value:</strong> Returns an <see cref="Archetype"/> enum value
/// in <see cref="ScreenResult.Selection"/>.
/// </para>
/// </remarks>
/// <seealso cref="ICreationScreen"/>
/// <seealso cref="IArchetypeProvider"/>
public class ArchetypeScreen : ICreationScreen
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly IArchetypeProvider _archetypeProvider;
    private readonly ILogger<ArchetypeScreen> _logger;

    private static readonly Archetype[] ArchetypeValues =
    {
        Archetype.Warrior, Archetype.Skirmisher,
        Archetype.Mystic, Archetype.Adept
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new archetype selection screen.
    /// </summary>
    /// <param name="archetypeProvider">Provider for archetype definitions and data.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public ArchetypeScreen(
        IArchetypeProvider archetypeProvider,
        ILogger<ArchetypeScreen>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(archetypeProvider);
        _archetypeProvider = archetypeProvider;
        _logger = logger ?? NullLogger<ArchetypeScreen>.Instance;
        _logger.LogDebug("ArchetypeScreen initialized");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ICreationScreen IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public CharacterCreationStep Step => CharacterCreationStep.Archetype;

    /// <inheritdoc />
    public Task<ScreenResult> DisplayAsync(
        CharacterCreationViewModel viewModel,
        IAnsiConsole console,
        CancellationToken ct = default)
    {
        _logger.LogDebug("Displaying archetype selection screen");

        // Render step header (includes permanent warning)
        StepHeaderRenderer.Render(console, viewModel);
        StepHeaderRenderer.RenderProgress(console, viewModel);

        // Show validation errors from previous attempt
        OptionListRenderer.RenderValidationErrors(console, viewModel.ValidationErrors);

        // Render archetype details
        RenderArchetypeDetails(console);

        // Build selection choices
        var choices = new List<string>();
        foreach (var archetype in ArchetypeValues)
        {
            var def = _archetypeProvider.GetArchetype(archetype);
            var displayName = def?.DisplayName ?? archetype.ToString();
            var bonuses = _archetypeProvider.GetResourceBonuses(archetype);
            choices.Add($"{displayName} — HP+{bonuses.MaxHpBonus} | ST+{bonuses.MaxStaminaBonus} | AP+{bonuses.MaxAetherPoolBonus}");
        }

        var prompt = OptionListRenderer.CreatePrompt(
            "Choose your archetype:",
            choices,
            canGoBack: viewModel.CanGoBack);

        var selection = console.Prompt(prompt);
        _logger.LogDebug("Archetype selection made: {Selection}", selection);

        // Check for navigation actions
        var navResult = OptionListRenderer.TryParseNavigation(selection);
        if (navResult.HasValue)
            return Task.FromResult(navResult.Value);

        // Map selection back to Archetype enum
        var selectedIndex = choices.IndexOf(selection);
        if (selectedIndex < 0 || selectedIndex >= ArchetypeValues.Length)
        {
            _logger.LogWarning("Invalid archetype selection index: {Index}", selectedIndex);
            return Task.FromResult(ScreenResult.Cancel());
        }

        var selectedArchetype = ArchetypeValues[selectedIndex];

        // Confirm permanent choice
        var def2 = _archetypeProvider.GetArchetype(selectedArchetype);
        var confirmName = def2?.DisplayName ?? selectedArchetype.ToString();

        console.WriteLine();
        var confirmed = console.Confirm(
            $"[yellow]This choice is PERMANENT.[/] Confirm [bold cyan]{Markup.Escape(confirmName)}[/]?",
            defaultValue: false);

        if (!confirmed)
        {
            _logger.LogDebug("Archetype selection not confirmed, re-displaying");
            // Return GoBack to re-display this screen
            return DisplayAsync(viewModel, console, ct);
        }

        _logger.LogInformation("Archetype selected (PERMANENT): {Archetype}", selectedArchetype);

        return Task.FromResult(ScreenResult.Selected(selectedArchetype));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Renders detail panels for each archetype showing role, stats, and abilities.
    /// </summary>
    private void RenderArchetypeDetails(IAnsiConsole console)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn(new TableColumn("[bold]Archetype[/]").Centered())
            .AddColumn(new TableColumn("[bold]Role[/]"))
            .AddColumn(new TableColumn("[bold]Resource Bonuses[/]"))
            .AddColumn(new TableColumn("[bold]Starting Abilities[/]"));

        foreach (var archetype in ArchetypeValues)
        {
            var def = _archetypeProvider.GetArchetype(archetype);
            if (def == null) continue;

            var bonuses = _archetypeProvider.GetResourceBonuses(archetype);
            var abilities = _archetypeProvider.GetStartingAbilities(archetype);

            var bonusText = FormatResourceBonuses(bonuses);
            var abilityText = abilities.Count > 0
                ? string.Join(", ", abilities.Select(a => Markup.Escape(a.AbilityName)))
                : "None";

            table.AddRow(
                $"[bold cyan]{Markup.Escape(def.DisplayName)}[/]",
                Markup.Escape(def.Description ?? ""),
                bonusText,
                abilityText);
        }

        console.Write(table);
        console.WriteLine();
    }

    /// <summary>
    /// Formats resource bonuses for display with color coding.
    /// </summary>
    private static string FormatResourceBonuses(ArchetypeResourceBonuses bonuses)
    {
        var parts = new List<string>();
        if (bonuses.MaxHpBonus > 0)
            parts.Add($"[green]+{bonuses.MaxHpBonus} HP[/]");
        if (bonuses.MaxStaminaBonus > 0)
            parts.Add($"[green]+{bonuses.MaxStaminaBonus} Stamina[/]");
        if (bonuses.MaxAetherPoolBonus > 0)
            parts.Add($"[blue]+{bonuses.MaxAetherPoolBonus} AP[/]");
        return parts.Count > 0 ? string.Join(", ", parts) : "None";
    }
}
