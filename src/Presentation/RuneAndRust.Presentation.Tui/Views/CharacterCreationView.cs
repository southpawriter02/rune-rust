using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;
using Spectre.Console;

namespace RuneAndRust.Presentation.Tui.Views;

/// <summary>
/// Character creation wizard view for new game setup.
/// </summary>
public class CharacterCreationView
{
    private readonly PlayerCreationService _creationService;
    private readonly IAnsiConsole _console;
    private readonly ILogger<CharacterCreationView> _logger;

    public CharacterCreationView(
        PlayerCreationService creationService,
        IAnsiConsole console,
        ILogger<CharacterCreationView> logger)
    {
        _creationService = creationService ?? throw new ArgumentNullException(nameof(creationService));
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Runs the character creation wizard and returns the created player.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created player entity, or null if cancelled.</returns>
    public async Task<Player?> RunAsync(CancellationToken ct = default)
    {
        _console.Clear();
        RenderTitle();

        // Step 1: Name
        var name = await GetPlayerNameAsync(ct);
        if (string.IsNullOrEmpty(name)) return null;

        // Step 2: Race
        var races = _creationService.GetAvailableRaces();
        var race = await SelectRaceAsync(races, ct);
        if (race == null) return null;

        // Step 3: Background
        var backgrounds = _creationService.GetAvailableBackgrounds();
        var background = await SelectBackgroundAsync(backgrounds, ct);
        if (background == null) return null;

        // Step 4: Attributes
        var attributes = await AllocateAttributesAsync(race, ct);
        if (attributes == null) return null;

        // Step 5: Description (optional)
        var description = await GetDescriptionAsync(ct);

        // Step 6: Summary and confirm
        var confirmed = await ShowSummaryAndConfirmAsync(
            name, race, background, attributes.Value, description, ct);

        if (!confirmed) return null;

        // Create the character
        try
        {
            var player = _creationService.CreateCharacter(
                name, race.Id, background.Id, attributes.Value, description);

            _logger.LogInformation("Character created: {Name}", player.Name);
            return player;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create character");
            _console.MarkupLine("[red]Failed to create character. Please try again.[/]");
            return null;
        }
    }

    private void RenderTitle()
    {
        _console.Write(new FigletText("New Hero").Centered().Color(Color.Gold1));
        _console.WriteLine();
    }

    private Task<string?> GetPlayerNameAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var name = _console.Ask<string>("Enter your character's [gold1]name[/]:");

            var validation = _creationService.ValidateName(name);
            if (validation.IsValid)
            {
                return Task.FromResult<string?>(_creationService.NormalizeName(name));
            }

            _console.MarkupLine($"[red]{validation.ErrorMessage}[/]");
        }
        return Task.FromResult<string?>(null);
    }

    private Task<RaceDefinition?> SelectRaceAsync(
        IReadOnlyList<RaceDefinition> races,
        CancellationToken ct)
    {
        _console.WriteLine();
        _console.MarkupLine("[bold]Choose your race:[/]");
        _console.WriteLine();

        var selection = _console.Prompt(
            new SelectionPrompt<RaceDefinition>()
                .Title("Select a [gold1]race[/]")
                .PageSize(10)
                .AddChoices(races)
                .UseConverter(r => $"{r.Name} - {r.Description}")
                .HighlightStyle(new Style(Color.Gold1)));

        // Show racial details
        _console.WriteLine();
        var panel = new Panel(
            $"[bold]{selection.Name}[/]\n\n" +
            $"{selection.Lore}\n\n" +
            $"[dim]Attribute Modifiers:[/]\n" +
            string.Join("\n", selection.AttributeModifiers.Select(m =>
                $"  {m.Key.ToUpperInvariant()}: {(m.Value >= 0 ? "+" : "")}{m.Value}")) +
            (selection.TraitName != null
                ? $"\n\n[dim]Racial Trait: {selection.TraitName}[/]\n{selection.TraitDescription}"
                : ""))
        {
            Header = new PanelHeader($"[gold1]{selection.Name}[/]"),
            Border = BoxBorder.Rounded,
            Padding = new Padding(2, 1)
        };
        _console.Write(panel);

        var confirm = _console.Confirm($"Confirm selection: [gold1]{selection.Name}[/]?");
        return Task.FromResult<RaceDefinition?>(confirm ? selection : null);
    }

    private Task<BackgroundDefinition?> SelectBackgroundAsync(
        IReadOnlyList<BackgroundDefinition> backgrounds,
        CancellationToken ct)
    {
        _console.WriteLine();
        _console.MarkupLine("[bold]Choose your background:[/]");
        _console.WriteLine();

        // Group by category
        var grouped = backgrounds.GroupBy(b => b.Category).ToList();

        var selection = _console.Prompt(
            new SelectionPrompt<BackgroundDefinition>()
                .Title("Select a [gold1]background[/]")
                .PageSize(10)
                .AddChoiceGroup(
                    new BackgroundDefinition { Name = "Professions", Category = "Header" },
                    grouped.FirstOrDefault(g => g.Key == "Profession")?.ToArray() ?? [])
                .AddChoiceGroup(
                    new BackgroundDefinition { Name = "Lineages", Category = "Header" },
                    grouped.FirstOrDefault(g => g.Key == "Lineage")?.ToArray() ?? [])
                .UseConverter(b => b.Category == "Header" ? $"[dim]{b.Name}[/]" : $"{b.Name} - {b.Description}")
                .HighlightStyle(new Style(Color.Gold1)));

        // Show background details
        _console.WriteLine();
        var panel = new Panel(
            $"[bold]{selection.Name}[/] ({selection.Category})\n\n" +
            $"{selection.Lore}\n\n" +
            $"[dim]Attribute Bonuses:[/]\n" +
            string.Join("\n", selection.AttributeBonuses.Select(m =>
                $"  {m.Key.ToUpperInvariant()}: +{m.Value}")) +
            (selection.StarterAbilityName != null
                ? $"\n\n[dim]Starting Ability: {selection.StarterAbilityName}[/]\n{selection.StarterAbilityDescription}"
                : ""))
        {
            Header = new PanelHeader($"[gold1]{selection.Name}[/]"),
            Border = BoxBorder.Rounded,
            Padding = new Padding(2, 1)
        };
        _console.Write(panel);

        var confirm = _console.Confirm($"Confirm selection: [gold1]{selection.Name}[/]?");
        return Task.FromResult<BackgroundDefinition?>(confirm ? selection : null);
    }

    private Task<PlayerAttributes?> AllocateAttributesAsync(
        RaceDefinition race,
        CancellationToken ct)
    {
        var rules = _creationService.GetPointBuyRules();
        var attributes = _creationService.GetAttributeDefinitions();

        // Start with base values
        var values = new Dictionary<string, int>
        {
            ["might"] = rules.BaseAttributeValue,
            ["fortitude"] = rules.BaseAttributeValue,
            ["will"] = rules.BaseAttributeValue,
            ["wits"] = rules.BaseAttributeValue,
            ["finesse"] = rules.BaseAttributeValue
        };

        while (!ct.IsCancellationRequested)
        {
            _console.Clear();
            _console.MarkupLine($"[bold]Allocate Attributes[/] - Points: [gold1]{rules.StartingPoints}[/]");
            _console.WriteLine();

            var current = new PlayerAttributes(
                values["might"], values["fortitude"], values["will"],
                values["wits"], values["finesse"]);

            var pointsUsed = current.CalculatePointCost();
            var pointsRemaining = rules.StartingPoints - pointsUsed;

            // Display current allocation
            var table = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn("Attribute")
                .AddColumn("Base")
                .AddColumn("Racial")
                .AddColumn("Total")
                .AddColumn("Cost");

            foreach (var attr in attributes)
            {
                var baseVal = values[attr.Id];
                var raceMod = race.AttributeModifiers.GetValueOrDefault(attr.Id, 0);
                var total = baseVal + raceMod;

                table.AddRow(
                    $"[bold]{attr.Name}[/]",
                    baseVal.ToString(),
                    raceMod >= 0 ? $"[green]+{raceMod}[/]" : $"[red]{raceMod}[/]",
                    total.ToString(),
                    CalculateAttributeCost(baseVal, rules.BaseAttributeValue).ToString());
            }

            _console.Write(table);
            _console.MarkupLine($"\n[bold]Points Used:[/] {pointsUsed}/{rules.StartingPoints} ([gold1]{pointsRemaining} remaining[/])");
            _console.WriteLine();

            // Select attribute to modify
            var choices = new List<string>(attributes.Select(a => a.Name))
            {
                "[green]Confirm[/]",
                "[red]Cancel[/]"
            };

            var choice = _console.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select attribute to modify or confirm:")
                    .AddChoices(choices));

            if (choice.Contains("Confirm"))
            {
                if (pointsUsed <= rules.StartingPoints)
                {
                    return Task.FromResult<PlayerAttributes?>(current);
                }
                _console.MarkupLine("[red]Over budget! Reduce some attributes.[/]");
                continue;
            }

            if (choice.Contains("Cancel"))
            {
                return Task.FromResult<PlayerAttributes?>(null);
            }

            // Find the attribute
            var selectedAttr = attributes.First(a => a.Name == choice);
            var currentVal = values[selectedAttr.Id];

            // Get new value
            var newValue = _console.Prompt(
                new TextPrompt<int>($"New value for {selectedAttr.Name} (current: {currentVal}, range: {rules.MinimumAttribute}-{rules.MaximumAttribute}):")
                    .DefaultValue(currentVal)
                    .Validate(v =>
                        v >= rules.MinimumAttribute && v <= rules.MaximumAttribute
                            ? ValidationResult.Success()
                            : ValidationResult.Error($"Must be between {rules.MinimumAttribute} and {rules.MaximumAttribute}")));

            values[selectedAttr.Id] = newValue;
        }

        return Task.FromResult<PlayerAttributes?>(null);
    }

    private static int CalculateAttributeCost(int value, int baseValue)
    {
        if (value <= baseValue) return 0;
        if (value <= 14) return value - baseValue;
        return (14 - baseValue) + ((value - 14) * 2);
    }

    private Task<string> GetDescriptionAsync(CancellationToken ct)
    {
        _console.WriteLine();
        _console.MarkupLine("[bold]Character Backstory (Optional)[/]");
        _console.MarkupLine("[dim]Enter a brief backstory for your character (up to 500 characters), or press Enter to skip.[/]");
        _console.WriteLine();

        var description = _console.Prompt(
            new TextPrompt<string>("Backstory:")
                .AllowEmpty()
                .Validate(d => d.Length <= 500
                    ? ValidationResult.Success()
                    : ValidationResult.Error("Backstory cannot exceed 500 characters")));

        return Task.FromResult(description);
    }

    private Task<bool> ShowSummaryAndConfirmAsync(
        string name,
        RaceDefinition race,
        BackgroundDefinition background,
        PlayerAttributes attributes,
        string description,
        CancellationToken ct)
    {
        _console.Clear();

        var finalAttrs = _creationService.CalculateFinalAttributes(
            attributes, race.Id, background.Id);

        var panel = new Panel(
            $"[bold]Name:[/] {name}\n" +
            $"[bold]Race:[/] {race.Name}\n" +
            $"[bold]Background:[/] {background.Name}\n\n" +
            $"[bold]Attributes:[/]\n" +
            $"  Might: {finalAttrs.Might}\n" +
            $"  Fortitude: {finalAttrs.Fortitude}\n" +
            $"  Will: {finalAttrs.Will}\n" +
            $"  Wits: {finalAttrs.Wits}\n" +
            $"  Finesse: {finalAttrs.Finesse}\n\n" +
            (race.TraitName != null ? $"[bold]Racial Trait:[/] {race.TraitName}\n" : "") +
            (background.StarterAbilityName != null ? $"[bold]Ability:[/] {background.StarterAbilityName}\n" : "") +
            (!string.IsNullOrWhiteSpace(description) ? $"\n[bold]Backstory:[/]\n{description}" : ""))
        {
            Header = new PanelHeader("[gold1]CHARACTER SUMMARY[/]"),
            Border = BoxBorder.Double,
            Padding = new Padding(2, 1)
        };

        _console.Write(panel);
        _console.WriteLine();

        return Task.FromResult(_console.Confirm("[bold]Create this character?[/]"));
    }
}
