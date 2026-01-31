// ═══════════════════════════════════════════════════════════════════════════════
// BackgroundScreen.cs
// Step 2 screen: Background selection in the character creation wizard. Displays
// the six background options (Village Smith, Traveling Healer, Ruin Delver,
// Clan Guard, Wandering Skald, Outcast Scavenger) with starting skills and
// equipment previews. Backgrounds determine the character's pre-Silence profession
// and grant initial skill bonuses and starting equipment.
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
/// Step 2: Background selection screen for the character creation wizard.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="BackgroundScreen"/> presents the six available backgrounds with information
/// about each: starting skill bonuses, starting equipment, and a thematic selection text.
/// </para>
/// <para>
/// <strong>Return Value:</strong> Returns a <see cref="Background"/> enum value
/// in <see cref="ScreenResult.Selection"/>.
/// </para>
/// </remarks>
/// <seealso cref="ICreationScreen"/>
/// <seealso cref="IBackgroundProvider"/>
public class BackgroundScreen : ICreationScreen
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly IBackgroundProvider _backgroundProvider;
    private readonly ILogger<BackgroundScreen> _logger;

    private static readonly Background[] BackgroundValues =
    {
        Background.VillageSmith, Background.TravelingHealer,
        Background.RuinDelver, Background.ClanGuard,
        Background.WanderingSkald, Background.OutcastScavenger
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new background selection screen.
    /// </summary>
    /// <param name="backgroundProvider">Provider for background definitions and data.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public BackgroundScreen(
        IBackgroundProvider backgroundProvider,
        ILogger<BackgroundScreen>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(backgroundProvider);
        _backgroundProvider = backgroundProvider;
        _logger = logger ?? NullLogger<BackgroundScreen>.Instance;
        _logger.LogDebug("BackgroundScreen initialized");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ICreationScreen IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public CharacterCreationStep Step => CharacterCreationStep.Background;

    /// <inheritdoc />
    public Task<ScreenResult> DisplayAsync(
        CharacterCreationViewModel viewModel,
        IAnsiConsole console,
        CancellationToken ct = default)
    {
        _logger.LogDebug("Displaying background selection screen");

        // Render step header
        StepHeaderRenderer.Render(console, viewModel);
        StepHeaderRenderer.RenderProgress(console, viewModel);

        // Show validation errors from previous attempt
        OptionListRenderer.RenderValidationErrors(console, viewModel.ValidationErrors);

        // Render background details table
        RenderBackgroundDetails(console);

        // Build selection choices
        var choices = new List<string>();
        foreach (var bg in BackgroundValues)
        {
            var def = _backgroundProvider.GetBackground(bg);
            var displayName = def?.DisplayName ?? bg.ToString();
            var skills = _backgroundProvider.GetSkillGrants(bg);
            var skillText = skills.Count > 0
                ? string.Join(", ", skills.Select(s => $"{s.SkillId} +{s.BonusAmount}"))
                : "None";
            choices.Add($"{displayName} — Skills: {skillText}");
        }

        var prompt = OptionListRenderer.CreatePrompt(
            "Choose your background:",
            choices,
            canGoBack: viewModel.CanGoBack);

        var selection = console.Prompt(prompt);
        _logger.LogDebug("Background selection made: {Selection}", selection);

        // Check for navigation actions
        var navResult = OptionListRenderer.TryParseNavigation(selection);
        if (navResult.HasValue)
            return Task.FromResult(navResult.Value);

        // Map selection back to Background enum
        var selectedIndex = choices.IndexOf(selection);
        if (selectedIndex < 0 || selectedIndex >= BackgroundValues.Length)
        {
            _logger.LogWarning("Invalid background selection index: {Index}", selectedIndex);
            return Task.FromResult(ScreenResult.Cancel());
        }

        var selectedBg = BackgroundValues[selectedIndex];
        _logger.LogInformation("Background selected: {Background}", selectedBg);

        return Task.FromResult(ScreenResult.Selected(selectedBg));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Renders a detailed table showing all backgrounds with skills and equipment.
    /// </summary>
    private void RenderBackgroundDetails(IAnsiConsole console)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn(new TableColumn("[bold]Background[/]").Centered())
            .AddColumn(new TableColumn("[bold]Starting Skills[/]"))
            .AddColumn(new TableColumn("[bold]Starting Equipment[/]"));

        foreach (var bg in BackgroundValues)
        {
            var def = _backgroundProvider.GetBackground(bg);
            if (def == null) continue;

            var skills = _backgroundProvider.GetSkillGrants(bg);
            var equipment = _backgroundProvider.GetEquipmentGrants(bg);

            var skillText = skills.Count > 0
                ? string.Join("\n", skills.Select(s => $"+{s.BonusAmount} {Markup.Escape(s.SkillId)}"))
                : "None";

            var equipText = equipment.Count > 0
                ? string.Join("\n", equipment.Select(e =>
                    e.Quantity > 1
                        ? $"{Markup.Escape(e.ItemId)} x{e.Quantity}"
                        : Markup.Escape(e.ItemId)))
                : "None";

            table.AddRow(
                $"[bold cyan]{Markup.Escape(def.DisplayName)}[/]",
                skillText,
                equipText);
        }

        console.Write(table);
        console.WriteLine();
    }
}
