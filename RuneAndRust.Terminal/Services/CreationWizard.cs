using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Constants;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Core.Models.Input;
using RuneAndRust.Engine.Factories;
using RuneAndRust.Terminal.Helpers;
using RuneAndRust.Terminal.Rendering;
using Spectre.Console;
using Spectre.Console.Rendering;
using Character = RuneAndRust.Core.Entities.Character;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Terminal.Services;

/// <summary>
/// Interactive character creation wizard with split-screen UI (v0.3.15a - The Lexicon).
/// Uses Spectre.Console Layout for real-time stat preview during selection.
/// Uses localized strings via ILocalizationService.
/// </summary>
/// <remarks>
/// See: SPEC-LOC-001 for Localization System design.
/// v0.3.23a: Refactored to use IInputService for standardized input handling.
/// </remarks>
public class CreationWizard
{
    private readonly IWizardService _wizardService;
    private readonly INarrativeService _narrativeService;
    private readonly CharacterFactory _characterFactory;
    private readonly ICharacterRepository _characterRepository;
    private readonly IInputService _inputService;
    private readonly ILocalizationService _loc;
    private readonly ILogger<CreationWizard> _logger;

    private WizardContext _context = new();
    private int _selectedIndex = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreationWizard"/> class.
    /// </summary>
    /// <param name="wizardService">The wizard service for stat calculations.</param>
    /// <param name="narrativeService">The narrative service for background descriptions.</param>
    /// <param name="characterFactory">The character factory for creating characters.</param>
    /// <param name="characterRepository">The character repository for persistence.</param>
    /// <param name="inputService">The input service for standardized input handling.</param>
    /// <param name="localizationService">The localization service for string lookup.</param>
    /// <param name="logger">The logger for traceability.</param>
    public CreationWizard(
        IWizardService wizardService,
        INarrativeService narrativeService,
        CharacterFactory characterFactory,
        ICharacterRepository characterRepository,
        IInputService inputService,
        ILocalizationService localizationService,
        ILogger<CreationWizard> logger)
    {
        _wizardService = wizardService;
        _narrativeService = narrativeService;
        _characterFactory = characterFactory;
        _characterRepository = characterRepository;
        _inputService = inputService;
        _loc = localizationService;
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

        // Step 0: Name entry with duplicate check loop
        string? name = null;
        while (name == null)
        {
            var enteredName = PromptForName();
            if (string.IsNullOrEmpty(enteredName) || enteredName.Equals("cancel", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("[Wizard] Character creation cancelled at name entry");
                return null;
            }

            if (await _characterRepository.NameExistsAsync(enteredName))
            {
                AnsiConsole.MarkupLine($"[red]{_loc.Get(LocKeys.UI_Creation_DuplicateName)}[/]");
                _logger.LogWarning("[Wizard] Duplicate character name attempted: {Name}", enteredName);
                AnsiConsole.WriteLine();
                // Loop continues, prompting for a new name
            }
            else
            {
                name = enteredName;
            }
        }
        _context.Name = name;
        _context.CurrentStep = 1;

        // Step 1: Lineage selection (split-screen)
        Console.Clear();
        var lineage = await RunSelectionStepAsync(
            _loc.Get(LocKeys.UI_Creation_Step_Lineage),
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
            _loc.Get(LocKeys.UI_Creation_Step_Archetype),
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

        // Step 3: Background selection
        Console.Clear();
        var background = await RunSimpleSelectionStepAsync(
            _loc.Get(LocKeys.UI_Creation_Step_Background),
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
                    layout["Right"].Update(new Panel(statsPanel).Header($"[yellow]{_loc.Get(LocKeys.UI_Creation_Preview)}[/]").Border(BoxBorder.Rounded));

                    ctx.UpdateTarget(layout);

                    // Non-blocking input check via IInputService (v0.3.23a)
                    if (_inputService.TryReadNext(out var inputEvent))
                    {
                        switch (inputEvent)
                        {
                            // Handle via GameAction
                            case ActionEvent { Action: GameAction.MoveNorth }:
                                _selectedIndex = Math.Max(0, _selectedIndex - 1);
                                break;
                            case ActionEvent { Action: GameAction.MoveSouth }:
                                _selectedIndex = Math.Min(options.Length - 1, _selectedIndex + 1);
                                break;
                            case ActionEvent { Action: GameAction.Confirm }:
                                result = options[_selectedIndex];
                                break;
                            case ActionEvent { Action: GameAction.Cancel }:
                                cancelled = true;
                                break;

                            // Fallback for raw keys
                            case RawKeyEvent { KeyInfo.Key: ConsoleKey.UpArrow or ConsoleKey.K }:
                                _selectedIndex = Math.Max(0, _selectedIndex - 1);
                                break;
                            case RawKeyEvent { KeyInfo.Key: ConsoleKey.DownArrow or ConsoleKey.J }:
                                _selectedIndex = Math.Min(options.Length - 1, _selectedIndex + 1);
                                break;
                            case RawKeyEvent { KeyInfo.Key: ConsoleKey.Enter or ConsoleKey.Spacebar }:
                                result = options[_selectedIndex];
                                break;
                            case RawKeyEvent { KeyInfo.Key: ConsoleKey.Backspace or ConsoleKey.Escape or ConsoleKey.Q }:
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
    /// <remarks>v0.3.23a: Refactored to use IInputService.</remarks>
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

                    // Non-blocking input check via IInputService (v0.3.23a)
                    if (_inputService.TryReadNext(out var inputEvent))
                    {
                        switch (inputEvent)
                        {
                            // Handle via GameAction
                            case ActionEvent { Action: GameAction.MoveNorth }:
                                _selectedIndex = Math.Max(0, _selectedIndex - 1);
                                break;
                            case ActionEvent { Action: GameAction.MoveSouth }:
                                _selectedIndex = Math.Min(options.Length - 1, _selectedIndex + 1);
                                break;
                            case ActionEvent { Action: GameAction.Confirm }:
                                result = options[_selectedIndex];
                                break;
                            case ActionEvent { Action: GameAction.Cancel }:
                                cancelled = true;
                                break;

                            // Fallback for raw keys
                            case RawKeyEvent { KeyInfo.Key: ConsoleKey.UpArrow or ConsoleKey.K }:
                                _selectedIndex = Math.Max(0, _selectedIndex - 1);
                                break;
                            case RawKeyEvent { KeyInfo.Key: ConsoleKey.DownArrow or ConsoleKey.J }:
                                _selectedIndex = Math.Min(options.Length - 1, _selectedIndex + 1);
                                break;
                            case RawKeyEvent { KeyInfo.Key: ConsoleKey.Enter or ConsoleKey.Spacebar }:
                                result = options[_selectedIndex];
                                break;
                            case RawKeyEvent { KeyInfo.Key: ConsoleKey.Backspace or ConsoleKey.Escape or ConsoleKey.Q }:
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
    /// Builds a simple menu panel for selections without bonuses.
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
        rows.Add(new Markup($"[grey]{_loc.Get(LocKeys.UI_Creation_NavigationHint)}[/]"));

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
        rows.Add(new Markup($"[grey]{_loc.Get(LocKeys.UI_Creation_NavigationHint)}[/]"));

        return new Panel(new Rows(rows)).Border(BoxBorder.Rounded);
    }

    /// <summary>
    /// Prompts for character name using blocking input.
    /// </summary>
    private string PromptForName()
    {
        AnsiConsole.Write(new Rule($"[yellow]{_loc.Get(LocKeys.UI_Creation_Title)}[/]").Centered());
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[grey]{_loc.Get(LocKeys.UI_Creation_Subtitle)}[/]");
        AnsiConsole.MarkupLine($"[grey]{_loc.Get(LocKeys.UI_Creation_Instruction)}[/]");
        AnsiConsole.WriteLine();

        return AnsiConsole.Prompt(
            new TextPrompt<string>($"[green]{_loc.Get(LocKeys.UI_Creation_NamePrompt)}[/] [grey]{_loc.Get(LocKeys.UI_Creation_NameCancelHint)}[/]:")
                .Validate(n =>
                {
                    if (n.Equals("cancel", StringComparison.OrdinalIgnoreCase))
                        return ValidationResult.Success();
                    if (string.IsNullOrWhiteSpace(n))
                        return ValidationResult.Error($"[red]{_loc.Get(LocKeys.UI_Creation_NameEmpty)}[/]");
                    if (n.Length < 2)
                        return ValidationResult.Error($"[red]{_loc.Get(LocKeys.UI_Creation_NameTooShort)}[/]");
                    if (n.Length > 50)
                        return ValidationResult.Error($"[red]{_loc.Get(LocKeys.UI_Creation_NameTooLong)}[/]");
                    return ValidationResult.Success();
                }));
    }

    /// <summary>
    /// Displays the success message after character creation.
    /// </summary>
    private void DisplaySuccess(Character character)
    {
        Console.Clear();

        var successMessage = _loc.Get(LocKeys.UI_Creation_Success_Message, Markup.Escape(character.Name));
        var lineageLabel = _loc.Get(LocKeys.UI_Creation_Success_LineageLabel);
        var archetypeLabel = _loc.Get(LocKeys.UI_Creation_Success_ArchetypeLabel);
        var backgroundLabel = _loc.Get(LocKeys.UI_Creation_Success_BackgroundLabel);
        var closingText = _loc.Get(LocKeys.UI_Creation_Success_Closing);

        var panel = new Panel(new Markup(
            $"[green]{successMessage}[/]\n\n" +
            $"[grey]{lineageLabel}[/] [cyan]{character.Lineage}[/]\n" +
            $"[grey]{archetypeLabel}[/] [cyan]{character.Archetype}[/]\n" +
            $"[grey]{backgroundLabel}[/] [cyan]{character.Background}[/]\n\n" +
            $"[grey]{closingText}[/]"))
        {
            Border = BoxBorder.Double,
            Padding = new Padding(2, 1),
            Header = new PanelHeader($"[yellow]{_loc.Get(LocKeys.UI_Creation_Success_Title)}[/]")
        };

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[grey]{_loc.Get(LocKeys.UI_Creation_ContinuePrompt)}[/]");
        ConsoleInputHelper.WaitForKeyPress();
    }
}
