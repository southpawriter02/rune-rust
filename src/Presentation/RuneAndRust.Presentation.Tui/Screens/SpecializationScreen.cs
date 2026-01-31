// ═══════════════════════════════════════════════════════════════════════════════
// SpecializationScreen.cs
// Step 5 screen: Specialization selection in the character creation wizard.
// Displays specializations filtered by the player's chosen archetype. Shows
// path type (Coherent/Heretical), tagline, description, special resource info,
// and Corruption warnings for Heretical paths. The first specialization is
// free during character creation.
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
/// Step 5: Specialization selection screen for the character creation wizard.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="SpecializationScreen"/> presents the available specializations filtered
/// by the player's chosen archetype. Each specialization is displayed with its path
/// type classification (Coherent or Heretical), tagline, description, and special
/// resource information when applicable.
/// </para>
/// <para>
/// <strong>Heretical Warning:</strong> When the player selects a Heretical
/// specialization, a Corruption risk warning is displayed before confirmation.
/// Heretical specializations interface with corrupted Aether and some abilities
/// may trigger Corruption gain.
/// </para>
/// <para>
/// <strong>Specialization Counts by Archetype:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>Warrior: 6 (Berserkr, IronBane, Skjaldmaer, SkarHorde, AtgeirWielder, GorgeMaw)</description></item>
///   <item><description>Skirmisher: 4 (Veidimadr, MyrkGengr, Strandhogg, HlekkrMaster)</description></item>
///   <item><description>Mystic: 2 (Seidkona, EchoCaller)</description></item>
///   <item><description>Adept: 5 (BoneSetter, JotunReader, Skald, ScrapTinker, Einbui)</description></item>
/// </list>
/// <para>
/// <strong>Return Value:</strong> Returns a <see cref="SpecializationId"/> enum value
/// in <see cref="ScreenResult.Selection"/>.
/// </para>
/// </remarks>
/// <seealso cref="ICreationScreen"/>
/// <seealso cref="ISpecializationProvider"/>
/// <seealso cref="ICharacterCreationController"/>
public class SpecializationScreen : ICreationScreen
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Provider for specialization definitions and data.
    /// </summary>
    private readonly ISpecializationProvider _specializationProvider;

    /// <summary>
    /// Controller for accessing current creation state (archetype selection).
    /// </summary>
    private readonly ICharacterCreationController _controller;

    /// <summary>
    /// Logger for screen operations and diagnostics.
    /// </summary>
    private readonly ILogger<SpecializationScreen> _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new specialization selection screen.
    /// </summary>
    /// <param name="specializationProvider">Provider for specialization definitions and data.</param>
    /// <param name="controller">Controller for accessing current creation state.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="specializationProvider"/> or <paramref name="controller"/> is <c>null</c>.
    /// </exception>
    public SpecializationScreen(
        ISpecializationProvider specializationProvider,
        ICharacterCreationController controller,
        ILogger<SpecializationScreen>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(specializationProvider);
        ArgumentNullException.ThrowIfNull(controller);
        _specializationProvider = specializationProvider;
        _controller = controller;
        _logger = logger ?? NullLogger<SpecializationScreen>.Instance;
        _logger.LogDebug("SpecializationScreen initialized");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ICreationScreen IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public CharacterCreationStep Step => CharacterCreationStep.Specialization;

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// Queries available specializations based on the player's chosen archetype
    /// from the controller state. Renders a detail table and selection prompt.
    /// Heretical specializations display a Corruption warning upon selection.
    /// </para>
    /// </remarks>
    public Task<ScreenResult> DisplayAsync(
        CharacterCreationViewModel viewModel,
        IAnsiConsole console,
        CancellationToken ct = default)
    {
        _logger.LogDebug("Displaying specialization selection screen");

        // Render step header
        StepHeaderRenderer.Render(console, viewModel);
        StepHeaderRenderer.RenderProgress(console, viewModel);

        // Show validation errors from previous attempt
        OptionListRenderer.RenderValidationErrors(console, viewModel.ValidationErrors);

        // Get current archetype from controller state
        var (state, _) = _controller.GetCurrentState();
        var archetype = state.SelectedArchetype;

        if (archetype == null)
        {
            _logger.LogWarning("No archetype selected — cannot show specializations");
            console.MarkupLine("[red]No archetype selected. Please go back and select an archetype first.[/]");
            return Task.FromResult(ScreenResult.GoBack());
        }

        _logger.LogDebug("Querying specializations for archetype: {Archetype}", archetype.Value);

        // Query specializations for this archetype
        var specializations = _specializationProvider.GetByArchetype(archetype.Value);

        if (specializations.Count == 0)
        {
            _logger.LogWarning("No specializations found for archetype: {Archetype}", archetype.Value);
            console.MarkupLine($"[red]No specializations available for {archetype.Value}.[/]");
            return Task.FromResult(ScreenResult.GoBack());
        }

        _logger.LogDebug(
            "Found {Count} specializations for {Archetype}",
            specializations.Count,
            archetype.Value);

        // Render specialization details table
        RenderSpecializationDetails(console, specializations);

        // Build selection choices
        var choices = new List<string>();
        foreach (var spec in specializations)
        {
            var pathTag = spec.IsHeretical ? "[red]Heretical[/]" : "[green]Coherent[/]";
            var resourceTag = spec.HasSpecialResource
                ? $" | {Markup.Escape(spec.SpecialResource.DisplayName)}"
                : "";
            choices.Add($"{Markup.Escape(spec.DisplayName)} — {pathTag}{resourceTag}");
        }

        // Create prompt with plain-text versions (Spectre strips markup for matching)
        var plainChoices = new List<string>();
        foreach (var spec in specializations)
        {
            var pathTag = spec.IsHeretical ? "Heretical" : "Coherent";
            var resourceTag = spec.HasSpecialResource
                ? $" | {spec.SpecialResource.DisplayName}"
                : "";
            plainChoices.Add($"{spec.DisplayName} — {pathTag}{resourceTag}");
        }

        var prompt = OptionListRenderer.CreatePrompt(
            "Choose your specialization:",
            plainChoices,
            canGoBack: viewModel.CanGoBack);

        var selection = console.Prompt(prompt);
        _logger.LogDebug("Specialization selection made: {Selection}", selection);

        // Check for navigation actions
        var navResult = OptionListRenderer.TryParseNavigation(selection);
        if (navResult.HasValue)
            return Task.FromResult(navResult.Value);

        // Map selection back to SpecializationId
        var selectedIndex = plainChoices.IndexOf(selection);
        if (selectedIndex < 0 || selectedIndex >= specializations.Count)
        {
            _logger.LogWarning("Invalid specialization selection index: {Index}", selectedIndex);
            return Task.FromResult(ScreenResult.Cancel());
        }

        var selectedSpec = specializations[selectedIndex];
        _logger.LogDebug(
            "Selected specialization: {Name} ({Id}), PathType={PathType}",
            selectedSpec.DisplayName,
            selectedSpec.SpecializationId,
            selectedSpec.PathType);

        // Show Corruption warning for Heretical specializations
        if (selectedSpec.IsHeretical)
        {
            var warning = selectedSpec.GetCorruptionWarning();
            if (!string.IsNullOrEmpty(warning))
            {
                console.WriteLine();
                var warningPanel = new Panel($"[bold red]{Markup.Escape(warning)}[/]")
                {
                    Border = BoxBorder.Heavy,
                    BorderStyle = new Style(Color.Red),
                    Header = new PanelHeader("[bold red]CORRUPTION RISK[/]"),
                    Padding = new Padding(1, 0)
                };
                console.Write(warningPanel);
                console.WriteLine();

                var confirmed = console.Confirm(
                    $"[yellow]Proceed with [bold red]{Markup.Escape(selectedSpec.DisplayName)}[/] (Heretical)?[/]",
                    defaultValue: false);

                if (!confirmed)
                {
                    _logger.LogDebug("Heretical specialization selection not confirmed, re-displaying");
                    return DisplayAsync(viewModel, console, ct);
                }
            }
        }

        _logger.LogInformation(
            "Specialization selected: {Name} ({Id}), PathType={PathType}, " +
            "HasSpecialResource={HasResource}",
            selectedSpec.DisplayName,
            selectedSpec.SpecializationId,
            selectedSpec.PathType,
            selectedSpec.HasSpecialResource);

        return Task.FromResult(ScreenResult.Selected(selectedSpec.SpecializationId));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Renders a detail table showing all available specializations for the archetype.
    /// </summary>
    /// <param name="console">The Spectre.Console instance to render to.</param>
    /// <param name="specializations">The list of specialization definitions to display.</param>
    private void RenderSpecializationDetails(
        IAnsiConsole console,
        IReadOnlyList<Domain.Entities.SpecializationDefinition> specializations)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn(new TableColumn("[bold]Specialization[/]").Centered())
            .AddColumn(new TableColumn("[bold]Path[/]").Centered())
            .AddColumn(new TableColumn("[bold]Tagline[/]"))
            .AddColumn(new TableColumn("[bold]Special Resource[/]"));

        foreach (var spec in specializations)
        {
            var pathColor = spec.IsHeretical ? "red" : "green";
            var pathText = $"[{pathColor}]{spec.PathType}[/]";

            var resourceText = spec.HasSpecialResource
                ? $"[yellow]{Markup.Escape(spec.SpecialResource.DisplayName)}[/]"
                : "[grey]None[/]";

            table.AddRow(
                $"[bold cyan]{Markup.Escape(spec.DisplayName)}[/]",
                pathText,
                $"[italic]{Markup.Escape(spec.Tagline)}[/]",
                resourceText);
        }

        console.Write(table);
        console.WriteLine();

        // Show brief description for each specialization
        foreach (var spec in specializations)
        {
            console.MarkupLine(
                $"  [bold cyan]{Markup.Escape(spec.DisplayName)}[/]: " +
                $"[grey]{Markup.Escape(spec.SelectionText)}[/]");
        }

        console.WriteLine();
    }
}
