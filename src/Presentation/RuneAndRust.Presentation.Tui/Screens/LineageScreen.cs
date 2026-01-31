// ═══════════════════════════════════════════════════════════════════════════════
// LineageScreen.cs
// Step 1 screen: Lineage selection in the character creation wizard. Displays
// the four lineage options (Clan-Born, Rune-Marked, Iron-Blooded, Vargr-Kin)
// with attribute modifiers, passive bonuses, unique traits, and trauma baseline
// information. Clan-Born selection triggers a sub-prompt for the flexible
// attribute bonus (+1 to any core attribute).
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
/// Step 1: Lineage selection screen for the character creation wizard.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="LineageScreen"/> presents the four available lineages with detailed
/// information about each: attribute modifiers, passive bonuses, unique traits,
/// and trauma economy starting values. The player selects one lineage to define
/// their character's bloodline heritage.
/// </para>
/// <para>
/// <strong>Clan-Born Special Case:</strong> When the Clan-Born lineage is selected,
/// a sub-prompt appears allowing the player to choose which core attribute receives
/// the +1 flexible bonus. All other lineages have fixed attribute modifiers.
/// </para>
/// <para>
/// <strong>Return Value:</strong> Returns a <c>(Lineage, CoreAttribute?)</c> tuple
/// in <see cref="ScreenResult.Selection"/>. The <c>CoreAttribute?</c> is non-null
/// only when Clan-Born is selected.
/// </para>
/// </remarks>
/// <seealso cref="ICreationScreen"/>
/// <seealso cref="ILineageProvider"/>
public class LineageScreen : ICreationScreen
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Provider for lineage definitions and related data.
    /// </summary>
    private readonly ILineageProvider _lineageProvider;

    /// <summary>
    /// Logger for screen operations and diagnostics.
    /// </summary>
    private readonly ILogger<LineageScreen> _logger;

    /// <summary>
    /// Ordered array of lineage enum values for consistent display ordering.
    /// </summary>
    private static readonly Lineage[] LineageValues =
    {
        Lineage.ClanBorn, Lineage.RuneMarked,
        Lineage.IronBlooded, Lineage.VargrKin
    };

    /// <summary>
    /// Ordered array of core attribute enum values for the flexible bonus prompt.
    /// </summary>
    private static readonly CoreAttribute[] CoreAttributeValues =
    {
        CoreAttribute.Might, CoreAttribute.Finesse,
        CoreAttribute.Wits, CoreAttribute.Will, CoreAttribute.Sturdiness
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new lineage selection screen.
    /// </summary>
    /// <param name="lineageProvider">Provider for lineage definitions and data.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="lineageProvider"/> is <c>null</c>.
    /// </exception>
    public LineageScreen(
        ILineageProvider lineageProvider,
        ILogger<LineageScreen>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(lineageProvider);
        _lineageProvider = lineageProvider;
        _logger = logger ?? NullLogger<LineageScreen>.Instance;
        _logger.LogDebug("LineageScreen initialized");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ICreationScreen IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public CharacterCreationStep Step => CharacterCreationStep.Lineage;

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// Renders a selection prompt with four lineage options. Each option shows:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Display name and thematic tagline</description></item>
    ///   <item><description>Attribute modifiers (e.g., +2 WILL, -1 STURDINESS)</description></item>
    ///   <item><description>Passive bonuses (e.g., +5 Max HP, +1 Social)</description></item>
    ///   <item><description>Unique trait name</description></item>
    /// </list>
    /// <para>
    /// If Clan-Born is selected, a follow-up prompt asks which attribute receives
    /// the +1 flexible bonus.
    /// </para>
    /// </remarks>
    public Task<ScreenResult> DisplayAsync(
        CharacterCreationViewModel viewModel,
        IAnsiConsole console,
        CancellationToken ct = default)
    {
        _logger.LogDebug("Displaying lineage selection screen");

        // Render step header
        StepHeaderRenderer.Render(console, viewModel);
        StepHeaderRenderer.RenderProgress(console, viewModel);

        // Show validation errors from previous attempt
        OptionListRenderer.RenderValidationErrors(console, viewModel.ValidationErrors);

        // Build lineage detail panels
        RenderLineageDetails(console);

        // Build selection choices
        var choices = new List<string>();
        foreach (var lineage in LineageValues)
        {
            var def = _lineageProvider.GetLineage(lineage);
            var displayName = def?.DisplayName ?? lineage.ToString();
            choices.Add(displayName);
        }

        // Create and show prompt (Step 1 = canGoBack is false)
        var prompt = OptionListRenderer.CreatePrompt(
            "Choose your lineage:",
            choices,
            canGoBack: viewModel.CanGoBack);

        var selection = console.Prompt(prompt);
        _logger.LogDebug("Lineage selection made: {Selection}", selection);

        // Check for navigation actions
        var navResult = OptionListRenderer.TryParseNavigation(selection);
        if (navResult.HasValue)
            return Task.FromResult(navResult.Value);

        // Map selection back to Lineage enum
        var selectedIndex = choices.IndexOf(selection);
        if (selectedIndex < 0 || selectedIndex >= LineageValues.Length)
        {
            _logger.LogWarning("Invalid lineage selection index: {Index}", selectedIndex);
            return Task.FromResult(ScreenResult.Cancel());
        }

        var selectedLineage = LineageValues[selectedIndex];
        _logger.LogInformation("Lineage selected: {Lineage}", selectedLineage);

        // Clan-Born requires flexible attribute bonus selection
        CoreAttribute? flexibleBonus = null;
        if (selectedLineage == Lineage.ClanBorn)
        {
            flexibleBonus = PromptFlexibleAttributeBonus(console);
            if (flexibleBonus == null)
            {
                // User cancelled the sub-prompt; re-display this screen
                _logger.LogDebug("Clan-Born flexible bonus selection cancelled");
                return Task.FromResult(ScreenResult.GoBack());
            }

            _logger.LogInformation(
                "Clan-Born flexible bonus selected: {Attribute}", flexibleBonus);
        }

        return Task.FromResult(ScreenResult.Selected((selectedLineage, flexibleBonus)));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Renders detail panels for each lineage showing modifiers and traits.
    /// </summary>
    private void RenderLineageDetails(IAnsiConsole console)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn(new TableColumn("[bold]Lineage[/]").Centered())
            .AddColumn(new TableColumn("[bold]Attribute Modifiers[/]"))
            .AddColumn(new TableColumn("[bold]Passive Bonuses[/]"))
            .AddColumn(new TableColumn("[bold]Unique Trait[/]"))
            .AddColumn(new TableColumn("[bold]Trauma[/]"));

        foreach (var lineage in LineageValues)
        {
            var def = _lineageProvider.GetLineage(lineage);
            if (def == null) continue;

            var modifiers = _lineageProvider.GetAttributeModifiers(lineage);
            var passives = _lineageProvider.GetPassiveBonuses(lineage);
            var trait = _lineageProvider.GetUniqueTrait(lineage);
            var trauma = _lineageProvider.GetTraumaBaseline(lineage);

            var modText = FormatAttributeModifiers(modifiers, lineage);
            var passiveText = FormatPassiveBonuses(passives);
            var traitText = !string.IsNullOrEmpty(trait.TraitName) ? trait.TraitName : "None";
            var traumaText = FormatTraumaBaseline(trauma);

            table.AddRow(
                $"[bold cyan]{Markup.Escape(def.DisplayName)}[/]",
                modText,
                passiveText,
                $"[italic]{Markup.Escape(traitText)}[/]",
                traumaText);
        }

        console.Write(table);
        console.WriteLine();
    }

    /// <summary>
    /// Formats attribute modifiers for display, including Clan-Born's flexible bonus.
    /// </summary>
    private static string FormatAttributeModifiers(
        LineageAttributeModifiers? modifiers,
        Lineage lineage)
    {
        if (modifiers == null) return "None";

        var parts = new List<string>();

        if (modifiers.Value.MightModifier != 0)
            parts.Add(FormatModifier("MIGHT", modifiers.Value.MightModifier));
        if (modifiers.Value.FinesseModifier != 0)
            parts.Add(FormatModifier("FINESSE", modifiers.Value.FinesseModifier));
        if (modifiers.Value.WitsModifier != 0)
            parts.Add(FormatModifier("WITS", modifiers.Value.WitsModifier));
        if (modifiers.Value.WillModifier != 0)
            parts.Add(FormatModifier("WILL", modifiers.Value.WillModifier));
        if (modifiers.Value.SturdinessModifier != 0)
            parts.Add(FormatModifier("STURDINESS", modifiers.Value.SturdinessModifier));

        // Clan-Born gets +1 to any attribute (player choice)
        if (lineage == Lineage.ClanBorn)
            parts.Add("[yellow]+1 Any (choice)[/]");

        return parts.Count > 0 ? string.Join(", ", parts) : "None";
    }

    /// <summary>
    /// Formats a single attribute modifier with color coding.
    /// </summary>
    private static string FormatModifier(string name, int value)
    {
        var color = value > 0 ? "green" : "red";
        var sign = value > 0 ? "+" : "";
        return $"[{color}]{sign}{value} {name}[/]";
    }

    /// <summary>
    /// Formats passive bonuses for display.
    /// </summary>
    private static string FormatPassiveBonuses(LineagePassiveBonuses? passives)
    {
        if (passives == null) return "None";

        var parts = new List<string>();
        if (passives.Value.MaxHpBonus > 0)
            parts.Add($"+{passives.Value.MaxHpBonus} Max HP");
        if (passives.Value.MaxApBonus > 0)
            parts.Add($"+{passives.Value.MaxApBonus} Max AP");
        if (passives.Value.SoakBonus > 0)
            parts.Add($"+{passives.Value.SoakBonus} Soak");
        if (passives.Value.MovementBonus > 0)
            parts.Add($"+{passives.Value.MovementBonus} Movement");

        return parts.Count > 0 ? string.Join(", ", parts) : "None";
    }

    /// <summary>
    /// Formats trauma baseline for display.
    /// </summary>
    private static string FormatTraumaBaseline(LineageTraumaBaseline? trauma)
    {
        if (trauma == null) return "None";

        var parts = new List<string>();
        if (trauma.Value.StartingCorruption > 0)
            parts.Add($"[red]{trauma.Value.StartingCorruption} Corruption[/]");
        if (trauma.Value.CorruptionResistanceModifier != 0)
            parts.Add($"[red]{trauma.Value.CorruptionResistanceModifier} Corruption Resist[/]");
        if (trauma.Value.StressResistanceModifier != 0)
            parts.Add($"[red]{trauma.Value.StressResistanceModifier} Stress Resist[/]");

        return parts.Count > 0 ? string.Join(", ", parts) : "[green]None[/]";
    }

    /// <summary>
    /// Prompts for the Clan-Born flexible attribute bonus selection.
    /// </summary>
    /// <param name="console">The console to render the prompt on.</param>
    /// <returns>
    /// The selected <see cref="CoreAttribute"/>, or <c>null</c> if the user
    /// cancelled (selected "Back").
    /// </returns>
    private CoreAttribute? PromptFlexibleAttributeBonus(IAnsiConsole console)
    {
        _logger.LogDebug("Prompting for Clan-Born flexible attribute bonus");

        console.WriteLine();
        console.MarkupLine("[yellow]Clan-Born grants +1 to any attribute of your choice.[/]");
        console.WriteLine();

        var choices = CoreAttributeValues
            .Select(a => a.ToString())
            .ToList();

        var prompt = new SelectionPrompt<string>()
            .Title("[gold1]Choose the attribute for your +1 bonus:[/]")
            .HighlightStyle(new Style(Color.Cyan1, decoration: Decoration.Bold));
        prompt.AddChoices(choices);
        prompt.AddChoice(OptionListRenderer.BackChoice);

        var selection = console.Prompt(prompt);

        if (OptionListRenderer.IsBack(selection))
            return null;

        // Map back to CoreAttribute
        var index = choices.IndexOf(selection);
        if (index >= 0 && index < CoreAttributeValues.Length)
            return CoreAttributeValues[index];

        return null;
    }
}
