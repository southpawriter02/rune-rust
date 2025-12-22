using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Engine.Factories;
using RuneAndRust.Terminal.Rendering;
using Spectre.Console;
using Spectre.Console.Rendering;
using Character = RuneAndRust.Core.Entities.Character;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Terminal.Services;

/// <summary>
/// Interactive character creation wizard with split-screen UI (v0.3.4b).
/// Updated in v0.3.4c to add Background selection step.
/// Uses Spectre.Console Layout for real-time stat preview during selection.
/// </summary>
public class CreationWizard
{
    private readonly IWizardService _wizardService;
    private readonly INarrativeService _narrativeService;
    private readonly CharacterFactory _characterFactory;
    private readonly ICharacterRepository _characterRepository;
    private readonly ILogger<CreationWizard> _logger;

    private WizardContext _context = new();
    private int _selectedIndex = 0;

    public CreationWizard(
        IWizardService wizardService,
        INarrativeService narrativeService,
        CharacterFactory characterFactory,
        ICharacterRepository characterRepository,
        ILogger<CreationWizard> logger)
    {
        _wizardService = wizardService;
        _narrativeService = narrativeService;
        _characterFactory = characterFactory;
        _characterRepository = characterRepository;
        _logger = logger;
    }

    /// <summary>
    /// Runs the character creation wizard.
    /// </summary>
    /// <returns>The created character, or null if cancelled.</returns>
    public async Task<Character?> RunAsync()
    {
        _logger.LogInformation("[Wizard] Starting character creation wizard");
        _context.Reset();
        Console.Clear();

        // Step 0: Name entry (blocking prompt - no preview needed)
        var name = PromptForName();
        if (string.IsNullOrEmpty(name) || name.Equals("cancel", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("[Wizard] Character creation cancelled at name entry");
            return null;
        }

        if (await _characterRepository.NameExistsAsync(name))
        {
            AnsiConsole.MarkupLine("[red]A character with that name already exists.[/]");
            _logger.LogWarning("[Wizard] Duplicate character name attempted: {Name}", name);
            return null;
        }
        _context.Name = name;
        _context.CurrentStep = 1;

        // Step 1: Lineage selection (split-screen)
        Console.Clear();
        var lineage = await RunSelectionStepAsync(
            "Choose Your Lineage",
            Enum.GetValues<LineageType>(),
            l => _wizardService.GetLineageDisplayName(l),
            l => _wizardService.GetLineageDescription(l),
            l => _wizardService.GetLineageBonusSummary(l),
            l => _wizardService.GetPreviewStats(_context, previewLineage: l),
            l => l);

        if (!lineage.HasValue)
        {
            _logger.LogInformation("[Wizard] Character creation cancelled at lineage selection");
            return null;
        }
        _context.Lineage = lineage.Value;
        _context.CurrentStep = 2;
        _logger.LogDebug("[Wizard] Step 1 - Lineage selected: {Lineage}", lineage.Value);

        // Step 2: Archetype selection (split-screen)
        Console.Clear();
        var archetype = await RunSelectionStepAsync(
            "Choose Your Archetype",
            Enum.GetValues<ArchetypeType>(),
            a => _wizardService.GetArchetypeDisplayName(a),
            a => _wizardService.GetArchetypeDescription(a),
            a => _wizardService.GetArchetypeBonusSummary(a),
            a => _wizardService.GetPreviewStats(_context, previewArchetype: a),
            a => a);

        if (!archetype.HasValue)
        {
            _logger.LogInformation("[Wizard] Character creation cancelled at archetype selection");
            return null;
        }
        _context.Archetype = archetype.Value;
        _context.CurrentStep = 3;
        _logger.LogDebug("[Wizard] Step 2 - Archetype selected: {Archetype}", archetype.Value);

        // Step 3: Background selection (v0.3.4c)
        Console.Clear();
        var background = await RunSimpleSelectionStepAsync(
            "Choose Your Background",
            Enum.GetValues<BackgroundType>(),
            b => _narrativeService.GetBackgroundDisplayName(b),
            b => _narrativeService.GetBackgroundDescription(b));

        if (!background.HasValue)
        {
            _logger.LogInformation("[Wizard] Character creation cancelled at background selection");
            return null;
        }
        _context.Background = background.Value;
        _logger.LogDebug("[Wizard] Step 3 - Background selected: {Background}", background.Value);

        // Create and save character
        var character = _characterFactory.CreateSimple(
            _context.Name,
            _context.Lineage.Value,
            _context.Archetype.Value,
            _context.Background.Value);

        await _characterRepository.AddAsync(character);
        await _characterRepository.SaveChangesAsync();

        _logger.LogInformation("[Wizard] Character created: {Name} (ID: {Id})", character.Name, character.Id);
        DisplaySuccess(character);

        return character;
    }

    /// <summary>
    /// Runs a selection step with split-screen layout and real-time stat preview.
    /// </summary>
    private async Task<T?> RunSelectionStepAsync<T>(
        string title,
        T[] options,
        Func<T, string> getName,
        Func<T, string> getDescription,
        Func<T, string> getBonuses,
        Func<T, Dictionary<CharacterAttribute, int>> getPreviewStats,
        Func<T, object> getArchetypeForDerived)
        where T : struct
    {
        _selectedIndex = 0;
        T? result = null;
        var cancelled = false;

        await AnsiConsole.Live(new Text(""))
            .AutoClear(false)
            .StartAsync(async ctx =>
            {
                while (result == null && !cancelled)
                {
                    // Build layout
                    var layout = new Layout("Root")
                        .SplitColumns(
                            new Layout("Left").Size(50),
                            new Layout("Right"));

                    // Left panel: Menu
                    var menuPanel = BuildMenuPanel(title, options, getName, getDescription, getBonuses);
                    layout["Left"].Update(menuPanel);

                    // Right panel: Stats preview
                    var hoveredOption = options[_selectedIndex];
                    var previewStats = getPreviewStats(hoveredOption);

                    // Determine archetype for derived stats calculation
                    ArchetypeType? archetypeForDerived = _context.Archetype;
                    if (!archetypeForDerived.HasValue && hoveredOption is ArchetypeType at)
                    {
                        archetypeForDerived = at;
                    }

                    var derived = _wizardService.GetDerivedStats(previewStats, archetypeForDerived);

                    // Determine display names for preview
                    string? lineageDisplay = null;
                    string? archetypeDisplay = null;

                    if (_context.Lineage.HasValue)
                    {
                        lineageDisplay = _wizardService.GetLineageDisplayName(_context.Lineage.Value);
                    }
                    else if (hoveredOption is LineageType lt)
                    {
                        lineageDisplay = _wizardService.GetLineageDisplayName(lt);
                    }

                    if (_context.Archetype.HasValue)
                    {
                        archetypeDisplay = _wizardService.GetArchetypeDisplayName(_context.Archetype.Value);
                    }
                    else if (hoveredOption is ArchetypeType at2)
                    {
                        archetypeDisplay = _wizardService.GetArchetypeDisplayName(at2);
                    }

                    var statsPanel = StatPreviewRenderer.Render(
                        previewStats,
                        derived,
                        lineageDisplay,
                        archetypeDisplay);
                    layout["Right"].Update(new Panel(statsPanel).Header("[yellow]Preview[/]").Border(BoxBorder.Rounded));

                    ctx.UpdateTarget(layout);

                    // Non-blocking input check
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(intercept: true);
                        switch (key.Key)
                        {
                            case ConsoleKey.UpArrow:
                            case ConsoleKey.K:
                                _selectedIndex = Math.Max(0, _selectedIndex - 1);
                                break;
                            case ConsoleKey.DownArrow:
                            case ConsoleKey.J:
                                _selectedIndex = Math.Min(options.Length - 1, _selectedIndex + 1);
                                break;
                            case ConsoleKey.Enter:
                            case ConsoleKey.Spacebar:
                                result = options[_selectedIndex];
                                break;
                            case ConsoleKey.Backspace:
                            case ConsoleKey.Escape:
                            case ConsoleKey.Q:
                                cancelled = true;
                                break;
                        }
                    }

                    await Task.Delay(16); // ~60fps refresh
                }
            });

        return cancelled ? null : result;
    }

    /// <summary>
    /// Runs a simple selection step without stat preview (v0.3.4c).
    /// Used for Background selection which doesn't affect stats.
    /// </summary>
    private async Task<T?> RunSimpleSelectionStepAsync<T>(
        string title,
        T[] options,
        Func<T, string> getName,
        Func<T, string> getDescription)
        where T : struct
    {
        _selectedIndex = 0;
        T? result = null;
        var cancelled = false;

        await AnsiConsole.Live(new Text(""))
            .AutoClear(false)
            .StartAsync(async ctx =>
            {
                while (result == null && !cancelled)
                {
                    // Build simple menu panel
                    var menuPanel = BuildSimpleMenuPanel(title, options, getName, getDescription);
                    ctx.UpdateTarget(menuPanel);

                    // Non-blocking input check
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(intercept: true);
                        switch (key.Key)
                        {
                            case ConsoleKey.UpArrow:
                            case ConsoleKey.K:
                                _selectedIndex = Math.Max(0, _selectedIndex - 1);
                                break;
                            case ConsoleKey.DownArrow:
                            case ConsoleKey.J:
                                _selectedIndex = Math.Min(options.Length - 1, _selectedIndex + 1);
                                break;
                            case ConsoleKey.Enter:
                            case ConsoleKey.Spacebar:
                                result = options[_selectedIndex];
                                break;
                            case ConsoleKey.Backspace:
                            case ConsoleKey.Escape:
                            case ConsoleKey.Q:
                                cancelled = true;
                                break;
                        }
                    }

                    await Task.Delay(16); // ~60fps refresh
                }
            });

        return cancelled ? null : result;
    }

    /// <summary>
    /// Builds a simple menu panel for selections without bonuses (v0.3.4c).
    /// </summary>
    private Panel BuildSimpleMenuPanel<T>(
        string title,
        T[] options,
        Func<T, string> getName,
        Func<T, string> getDescription)
    {
        var rows = new List<IRenderable>
        {
            new Rule($"[cyan]{Markup.Escape(title)}[/]").LeftJustified(),
            new Text("")
        };

        for (int i = 0; i < options.Length; i++)
        {
            var option = options[i];
            var prefix = i == _selectedIndex ? "[yellow]►[/] " : "  ";
            var nameColor = i == _selectedIndex ? "yellow" : "white";

            rows.Add(new Markup($"{prefix}[{nameColor}]{Markup.Escape(getName(option))}[/]"));
            rows.Add(new Markup($"   [grey]{Markup.Escape(getDescription(option))}[/]"));
            rows.Add(new Text(""));
        }

        rows.Add(new Rule().RuleStyle(Style.Parse("grey")));
        rows.Add(new Markup("[grey]↑↓/JK Navigate | Enter/Space Select | Esc/Q Back[/]"));

        return new Panel(new Rows(rows)).Border(BoxBorder.Rounded);
    }

    /// <summary>
    /// Builds the menu panel showing options with descriptions and bonuses.
    /// </summary>
    private Panel BuildMenuPanel<T>(
        string title,
        T[] options,
        Func<T, string> getName,
        Func<T, string> getDescription,
        Func<T, string> getBonuses)
    {
        var rows = new List<IRenderable>
        {
            new Rule($"[cyan]{Markup.Escape(title)}[/]").LeftJustified(),
            new Text("")
        };

        for (int i = 0; i < options.Length; i++)
        {
            var option = options[i];
            var prefix = i == _selectedIndex ? "[yellow]►[/] " : "  ";
            var nameColor = i == _selectedIndex ? "yellow" : "white";
            var bonusColor = i == _selectedIndex ? "green" : "grey";

            rows.Add(new Markup($"{prefix}[{nameColor}]{Markup.Escape(getName(option))}[/]"));
            rows.Add(new Markup($"   [grey]{Markup.Escape(getDescription(option))}[/]"));
            rows.Add(new Markup($"   [{bonusColor}]{Markup.Escape(getBonuses(option))}[/]"));
            rows.Add(new Text(""));
        }

        rows.Add(new Rule().RuleStyle(Style.Parse("grey")));
        rows.Add(new Markup("[grey]↑↓/JK Navigate | Enter/Space Select | Esc/Q Back[/]"));

        return new Panel(new Rows(rows)).Border(BoxBorder.Rounded);
    }

    /// <summary>
    /// Prompts for character name using blocking input.
    /// </summary>
    private string PromptForName()
    {
        AnsiConsole.Write(new Rule("[yellow]CHARACTER CREATION[/]").Centered());
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Forge your survivor in the ashes of the old world.[/]");
        AnsiConsole.MarkupLine("[grey]Choose wisely, for these choices shape your fate.[/]");
        AnsiConsole.WriteLine();

        return AnsiConsole.Prompt(
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
    }

    /// <summary>
    /// Displays the success message after character creation.
    /// </summary>
    private void DisplaySuccess(Character character)
    {
        Console.Clear();

        var panel = new Panel(new Markup(
            $"[green]Your character, [yellow]{Markup.Escape(character.Name)}[/], has been forged in the ashes of the old world.[/]\n\n" +
            $"[grey]Lineage:[/] [cyan]{character.Lineage}[/]\n" +
            $"[grey]Archetype:[/] [cyan]{character.Archetype}[/]\n" +
            $"[grey]Background:[/] [cyan]{character.Background}[/]\n\n" +
            $"[grey]May they survive the trials ahead.[/]"))
        {
            Border = BoxBorder.Double,
            Padding = new Padding(2, 1),
            Header = new PanelHeader("[yellow]Character Created[/]")
        };

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
        Console.ReadKey(intercept: true);
    }
}
