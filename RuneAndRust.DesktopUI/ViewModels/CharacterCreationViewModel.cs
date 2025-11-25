using ReactiveUI;
using RuneAndRust.Core;
using RuneAndRust.DesktopUI.Controllers;
using RuneAndRust.DesktopUI.Services;
using RuneAndRust.Engine;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RuneAndRust.DesktopUI.ViewModels;

/// <summary>
/// v0.44.1: View model for character creation screen.
/// Allows players to choose name, class, and start a new game.
/// Integrates with GameStateController for proper game state management.
/// </summary>
public class CharacterCreationViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly GameStateController? _gameStateController;
    private readonly ILogger? _logger;
    private string _characterName = "Survivor";
    private CharacterClass _selectedClass = CharacterClass.Warrior;
    private string _classDescription = string.Empty;

    /// <summary>
    /// Available character classes.
    /// </summary>
    public ObservableCollection<CharacterClassInfo> AvailableClasses { get; } = new();

    /// <summary>
    /// Character name input.
    /// </summary>
    public string CharacterName
    {
        get => _characterName;
        set
        {
            this.RaiseAndSetIfChanged(ref _characterName, value);
            this.RaisePropertyChanged(nameof(CanStartGame));
        }
    }

    /// <summary>
    /// Currently selected character class.
    /// </summary>
    public CharacterClass SelectedClass
    {
        get => _selectedClass;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedClass, value);
            UpdateClassDescription();
        }
    }

    /// <summary>
    /// Description of the currently selected class.
    /// </summary>
    public string ClassDescription
    {
        get => _classDescription;
        private set => this.RaiseAndSetIfChanged(ref _classDescription, value);
    }

    /// <summary>
    /// Whether the game can be started (valid name entered).
    /// </summary>
    public bool CanStartGame => !string.IsNullOrWhiteSpace(CharacterName);

    /// <summary>
    /// Page title.
    /// </summary>
    public string Title => "Create Your Character";

    #region Commands

    /// <summary>
    /// Command to go back to main menu.
    /// </summary>
    public ICommand BackCommand { get; }

    /// <summary>
    /// Command to start the game with the created character.
    /// </summary>
    public ICommand StartGameCommand { get; }

    /// <summary>
    /// Command to select a character class.
    /// </summary>
    public ICommand SelectClassCommand { get; }

    #endregion

    /// <summary>
    /// Creates a new instance for design-time support.
    /// </summary>
    public CharacterCreationViewModel()
    {
        _navigationService = null!;

        BackCommand = ReactiveCommand.CreateFromTask(OnBackAsync);
        StartGameCommand = ReactiveCommand.CreateFromTask(OnStartGameAsync);
        SelectClassCommand = ReactiveCommand.Create<CharacterClass>(OnSelectClass);

        LoadAvailableClasses();
        UpdateClassDescription();
    }

    /// <summary>
    /// Creates a new instance with GameStateController (v0.44.1+).
    /// </summary>
    public CharacterCreationViewModel(
        INavigationService navigationService,
        GameStateController gameStateController,
        ILogger logger)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _gameStateController = gameStateController ?? throw new ArgumentNullException(nameof(gameStateController));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        BackCommand = ReactiveCommand.CreateFromTask(OnBackAsync);
        StartGameCommand = ReactiveCommand.CreateFromTask(OnStartGameAsync);
        SelectClassCommand = ReactiveCommand.Create<CharacterClass>(OnSelectClass);

        LoadAvailableClasses();
        UpdateClassDescription();
    }

    /// <summary>
    /// Legacy constructor without GameStateController (backwards compatibility).
    /// </summary>
    public CharacterCreationViewModel(
        INavigationService navigationService,
        ISaveGameService? saveGameService = null)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

        BackCommand = ReactiveCommand.CreateFromTask(OnBackAsync);
        StartGameCommand = ReactiveCommand.CreateFromTask(OnStartGameAsync);
        SelectClassCommand = ReactiveCommand.Create<CharacterClass>(OnSelectClass);

        LoadAvailableClasses();
        UpdateClassDescription();
    }

    /// <summary>
    /// Loads all available character classes.
    /// </summary>
    private void LoadAvailableClasses()
    {
        AvailableClasses.Clear();

        AvailableClasses.Add(new CharacterClassInfo
        {
            Class = CharacterClass.Warrior,
            Name = "Warrior",
            ShortDescription = "Hardy fighter with high HP and defensive capabilities",
            PrimaryAttribute = "MIGHT",
            HP = 50,
            Stamina = 30,
            IconText = "\u2694" // Crossed swords
        });

        AvailableClasses.Add(new CharacterClassInfo
        {
            Class = CharacterClass.Scavenger,
            Name = "Scavenger",
            ShortDescription = "Balanced survivor using cunning and agility",
            PrimaryAttribute = "FINESSE",
            HP = 40,
            Stamina = 40,
            IconText = "\u2692" // Hammer and pick
        });

        AvailableClasses.Add(new CharacterClassInfo
        {
            Class = CharacterClass.Mystic,
            Name = "Mystic",
            ShortDescription = "Wielder of corrupted aetheric energy",
            PrimaryAttribute = "WILL",
            HP = 30,
            Stamina = 50,
            IconText = "\u2728" // Sparkles
        });

        AvailableClasses.Add(new CharacterClassInfo
        {
            Class = CharacterClass.Adept,
            Name = "Adept",
            ShortDescription = "Skill-based specialist excelling in support",
            PrimaryAttribute = "WITS",
            HP = 35,
            Stamina = 40,
            IconText = "\u2699" // Gear
        });

        AvailableClasses.Add(new CharacterClassInfo
        {
            Class = CharacterClass.Skirmisher,
            Name = "Skirmisher",
            ShortDescription = "Agility-based combatant with high evasion",
            PrimaryAttribute = "FINESSE",
            HP = 40,
            Stamina = 35,
            IconText = "\u26A1" // Lightning bolt
        });
    }

    /// <summary>
    /// Updates the class description when selection changes.
    /// </summary>
    private void UpdateClassDescription()
    {
        ClassDescription = CharacterFactory.GetClassDescription(SelectedClass);
    }

    private async Task OnBackAsync()
    {
        _logger?.Information("Character creation cancelled, returning to menu");

        // If we have a game state controller, reset the game state
        if (_gameStateController != null && _gameStateController.HasActiveGame)
        {
            await _gameStateController.UpdatePhaseAsync(GamePhase.MainMenu, "Character creation cancelled");
            _gameStateController.Reset();
        }

        _navigationService.NavigateBack();
    }

    private async Task OnStartGameAsync()
    {
        if (!CanStartGame) return;

        // Create the character using CharacterFactory
        var character = CharacterFactory.CreateCharacter(SelectedClass, CharacterName);

        _logger?.Information("Created character: {Name} ({Class}) - HP: {HP}, Stamina: {Stamina}, Abilities: {AbilityCount}",
            character.Name, character.Class, character.HP, character.Stamina, character.Abilities.Count);

        if (_gameStateController != null && _gameStateController.HasActiveGame)
        {
            // Set the player in game state
            _gameStateController.SetPlayer(character);

            _logger?.Information("Character set in GameState, SessionId: {SessionId}",
                _gameStateController.CurrentGameState.SessionId);

            // TODO (v0.44.3): Generate dungeon and transition to exploration
            // For now, show a message that dungeon generation isn't implemented yet
            _logger?.Warning("Dungeon generation not yet implemented (v0.44.3). " +
                "Character created successfully. Returning to menu.");

            // Navigate back to menu for now (until v0.44.3 implements dungeon generation)
            _gameStateController.Reset();
            _navigationService.NavigateTo<MenuViewModel>();
        }
        else
        {
            // Legacy path without GameStateController
            _logger?.Information("Character created (legacy path). Returning to menu.");
            _navigationService.NavigateBack();
        }
    }

    private void OnSelectClass(CharacterClass characterClass)
    {
        SelectedClass = characterClass;
    }
}

/// <summary>
/// Display information for a character class option.
/// </summary>
public class CharacterClassInfo : ReactiveObject
{
    /// <summary>
    /// The character class enum value.
    /// </summary>
    public CharacterClass Class { get; set; }

    /// <summary>
    /// Display name for the class.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Short description of the class.
    /// </summary>
    public string ShortDescription { get; set; } = string.Empty;

    /// <summary>
    /// Primary attribute for the class.
    /// </summary>
    public string PrimaryAttribute { get; set; } = string.Empty;

    /// <summary>
    /// Starting HP value.
    /// </summary>
    public int HP { get; set; }

    /// <summary>
    /// Starting stamina value.
    /// </summary>
    public int Stamina { get; set; }

    /// <summary>
    /// Icon text (Unicode character) for display.
    /// </summary>
    public string IconText { get; set; } = string.Empty;

    /// <summary>
    /// Color based on primary attribute.
    /// </summary>
    public string AttributeColor => PrimaryAttribute switch
    {
        "MIGHT" => "#DC143C",     // Red
        "FINESSE" => "#4CAF50",   // Green
        "WITS" => "#4A90E2",      // Blue
        "WILL" => "#9400D3",      // Purple
        "STURDINESS" => "#FFA500", // Orange
        _ => "#CCCCCC"
    };
}
