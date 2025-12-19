using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Engine.Factories;
using Spectre.Console;

namespace RuneAndRust.Terminal.Services;

/// <summary>
/// Controller for the character creation wizard.
/// Uses Spectre.Console for rich terminal UI during character creation.
/// </summary>
public class CharacterCreationController
{
    private readonly ILogger<CharacterCreationController> _logger;
    private readonly CharacterFactory _characterFactory;
    private readonly ICharacterRepository _characterRepository;
    private readonly IStatCalculationService _statService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CharacterCreationController"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    /// <param name="characterFactory">The character factory.</param>
    /// <param name="characterRepository">The character repository.</param>
    /// <param name="statService">The stat calculation service.</param>
    public CharacterCreationController(
        ILogger<CharacterCreationController> logger,
        CharacterFactory characterFactory,
        ICharacterRepository characterRepository,
        IStatCalculationService statService)
    {
        _logger = logger;
        _characterFactory = characterFactory;
        _characterRepository = characterRepository;
        _statService = statService;
    }

    /// <summary>
    /// Runs the character creation wizard and returns the created character.
    /// </summary>
    /// <returns>The created character, or null if creation was cancelled.</returns>
    public async Task<Character?> RunCreationWizardAsync()
    {
        _logger.LogInformation("Starting character creation wizard");

        AnsiConsole.Clear();
        DisplayWelcomeBanner();

        // Step 1: Name
        var name = PromptForName();
        if (string.IsNullOrEmpty(name))
        {
            _logger.LogInformation("Character creation cancelled during name entry");
            return null;
        }

        // Check for duplicate name
        if (await _characterRepository.NameExistsAsync(name))
        {
            AnsiConsole.MarkupLine("[red]A character with that name already exists.[/]");
            _logger.LogWarning("Duplicate character name attempted: {Name}", name);
            return null;
        }

        // Step 2: Lineage
        var lineage = PromptForLineage();

        // Step 3: Archetype
        var archetype = PromptForArchetype();

        // Step 4: Preview and Confirm
        var character = _characterFactory.CreateSimple(name, lineage, archetype);

        if (!ConfirmCharacter(character))
        {
            _logger.LogInformation("Character creation cancelled during confirmation");
            return null;
        }

        // Save to database
        await _characterRepository.AddAsync(character);
        await _characterRepository.SaveChangesAsync();

        _logger.LogInformation("Character created and saved: {CharacterName} ({CharacterId})", character.Name, character.Id);

        DisplaySuccessMessage(character);

        return character;
    }

    /// <summary>
    /// Displays the welcome banner for character creation.
    /// </summary>
    private void DisplayWelcomeBanner()
    {
        var rule = new Rule("[yellow]CHARACTER CREATION[/]")
            .Centered();
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[grey]Forge your survivor in the ashes of the old world.[/]");
        AnsiConsole.MarkupLine("[grey]Choose wisely, for these choices shape your fate.[/]");
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Prompts the user for a character name.
    /// </summary>
    /// <returns>The entered name, or empty string if cancelled.</returns>
    private string PromptForName()
    {
        AnsiConsole.MarkupLine("[cyan]Step 1: Name Your Survivor[/]");
        AnsiConsole.WriteLine();

        var name = AnsiConsole.Prompt(
            new TextPrompt<string>("[green]Enter character name[/] [grey](or 'cancel' to abort)[/]:")
                .Validate(n =>
                {
                    if (n.Equals("cancel", StringComparison.OrdinalIgnoreCase))
                        return ValidationResult.Success();

                    if (string.IsNullOrWhiteSpace(n))
                        return ValidationResult.Error("[red]Name cannot be empty[/]");

                    if (n.Length < 2)
                        return ValidationResult.Error("[red]Name must be at least 2 characters[/]");

                    if (n.Length > 50)
                        return ValidationResult.Error("[red]Name cannot exceed 50 characters[/]");

                    return ValidationResult.Success();
                }));

        if (name.Equals("cancel", StringComparison.OrdinalIgnoreCase))
            return string.Empty;

        AnsiConsole.WriteLine();
        return name;
    }

    /// <summary>
    /// Prompts the user to select a lineage.
    /// </summary>
    /// <returns>The selected lineage.</returns>
    private LineageType PromptForLineage()
    {
        AnsiConsole.MarkupLine("[cyan]Step 2: Choose Your Lineage[/]");
        AnsiConsole.WriteLine();

        var lineageDescriptions = new Dictionary<LineageType, (string Name, string Description, string Bonuses)>
        {
            { LineageType.Human, ("Human", "Adaptable survivors who endure through sheer determination.", "+1 to all attributes") },
            { LineageType.RuneMarked, ("Rune-Marked", "Descendants bearing ancient runic inscriptions in their skin.", "+2 Wits, +2 Will, -1 Sturdiness") },
            { LineageType.IronBlooded, ("Iron-Blooded", "Those with machine-integrated bloodlines from the old world.", "+2 Sturdiness, +2 Might, -1 Wits") },
            { LineageType.VargrKin, ("Vargr-Kin", "Wolf-kin bearing the curse of the northern wastes.", "+2 Finesse, +2 Wits, -1 Will") }
        };

        // Build selection choices with descriptions
        var choices = lineageDescriptions.Select(kvp =>
            $"[yellow]{kvp.Value.Name}[/] - {kvp.Value.Description} [green]({kvp.Value.Bonuses})[/]"
        ).ToList();

        var lineageNames = lineageDescriptions.Values.Select(v => v.Name).ToList();

        var selectedIndex = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[green]Select your lineage:[/]")
                .PageSize(10)
                .AddChoices(choices));

        var lineage = lineageDescriptions.First(kvp =>
            selectedIndex.Contains(kvp.Value.Name)).Key;

        AnsiConsole.MarkupLine($"[grey]Selected:[/] [yellow]{lineageDescriptions[lineage].Name}[/]");
        AnsiConsole.WriteLine();

        return lineage;
    }

    /// <summary>
    /// Prompts the user to select an archetype.
    /// </summary>
    /// <returns>The selected archetype.</returns>
    private ArchetypeType PromptForArchetype()
    {
        AnsiConsole.MarkupLine("[cyan]Step 3: Choose Your Archetype[/]");
        AnsiConsole.WriteLine();

        var archetypeDescriptions = new Dictionary<ArchetypeType, (string Name, string Description, string Bonuses)>
        {
            { ArchetypeType.Warrior, ("Warrior", "Frontline combatant specializing in durability and melee damage.", "+2 Sturdiness, +1 Might") },
            { ArchetypeType.Skirmisher, ("Skirmisher", "Cunning fighter favoring speed and precision over raw power.", "+2 Finesse, +1 Wits") },
            { ArchetypeType.Adept, ("Adept", "Runic practitioner channeling ancient power through inscribed formulas.", "+2 Wits, +1 Will") },
            { ArchetypeType.Mystic, ("Mystic", "Wielder of primal forces, drawing power from the world's corruption.", "+2 Will, +1 Sturdiness") }
        };

        var choices = archetypeDescriptions.Select(kvp =>
            $"[yellow]{kvp.Value.Name}[/] - {kvp.Value.Description} [green]({kvp.Value.Bonuses})[/]"
        ).ToList();

        var selectedIndex = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[green]Select your archetype:[/]")
                .PageSize(10)
                .AddChoices(choices));

        var archetype = archetypeDescriptions.First(kvp =>
            selectedIndex.Contains(kvp.Value.Name)).Key;

        AnsiConsole.MarkupLine($"[grey]Selected:[/] [yellow]{archetypeDescriptions[archetype].Name}[/]");
        AnsiConsole.WriteLine();

        return archetype;
    }

    /// <summary>
    /// Displays the character preview and asks for confirmation.
    /// </summary>
    /// <param name="character">The character to preview.</param>
    /// <returns>True if the user confirms, false otherwise.</returns>
    private bool ConfirmCharacter(Character character)
    {
        AnsiConsole.MarkupLine("[cyan]Step 4: Review Your Character[/]");
        AnsiConsole.WriteLine();

        // Create character summary table
        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.AddColumn(new TableColumn("[yellow]Attribute[/]").Centered());
        table.AddColumn(new TableColumn("[yellow]Value[/]").Centered());

        table.AddRow("[white]Name[/]", $"[green]{character.Name}[/]");
        table.AddRow("[white]Lineage[/]", $"[cyan]{character.Lineage}[/]");
        table.AddRow("[white]Archetype[/]", $"[cyan]{character.Archetype}[/]");
        table.AddEmptyRow();

        // Core attributes
        table.AddRow("[grey]--- ATTRIBUTES ---[/]", "");
        table.AddRow("Sturdiness", FormatStat(character.Sturdiness));
        table.AddRow("Might", FormatStat(character.Might));
        table.AddRow("Wits", FormatStat(character.Wits));
        table.AddRow("Will", FormatStat(character.Will));
        table.AddRow("Finesse", FormatStat(character.Finesse));
        table.AddEmptyRow();

        // Derived stats
        table.AddRow("[grey]--- DERIVED STATS ---[/]", "");
        table.AddRow("Max HP", $"[red]{character.MaxHP}[/]");
        table.AddRow("Max Stamina", $"[yellow]{character.MaxStamina}[/]");
        table.AddRow("Action Points", $"[blue]{character.ActionPoints}[/]");

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        return AnsiConsole.Confirm("[green]Create this character?[/]", true);
    }

    /// <summary>
    /// Formats a stat value with color coding.
    /// </summary>
    private static string FormatStat(int value)
    {
        var color = value switch
        {
            <= 3 => "red",
            <= 5 => "yellow",
            <= 7 => "green",
            _ => "cyan"
        };
        return $"[{color}]{value}[/]";
    }

    /// <summary>
    /// Displays the success message after character creation.
    /// </summary>
    /// <param name="character">The created character.</param>
    private void DisplaySuccessMessage(Character character)
    {
        AnsiConsole.WriteLine();

        var panel = new Panel(new Markup(
            $"[green]Your character, [yellow]{character.Name}[/], has been forged in the ashes of the old world.[/]\n" +
            $"[grey]May they survive the trials ahead.[/]"))
        {
            Border = BoxBorder.Double,
            Padding = new Padding(2, 1)
        };

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }
}
