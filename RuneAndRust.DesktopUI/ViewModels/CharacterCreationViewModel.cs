using ReactiveUI;
using RuneAndRust.Core;
using RuneAndRust.DesktopUI.Services;
using RuneAndRust.Engine;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace RuneAndRust.DesktopUI.ViewModels;

/// <summary>
/// View model for character creation screen.
/// Allows players to choose name, class, and start a new game.
/// </summary>
public class CharacterCreationViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly ISaveGameService? _saveGameService;
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

        BackCommand = ReactiveCommand.Create(() => { });
        StartGameCommand = ReactiveCommand.Create(() => { });
        SelectClassCommand = ReactiveCommand.Create<CharacterClass>(_ => { });

        LoadAvailableClasses();
        UpdateClassDescription();
    }

    /// <summary>
    /// Creates a new instance with dependency injection.
    /// </summary>
    public CharacterCreationViewModel(
        INavigationService navigationService,
        ISaveGameService? saveGameService = null)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _saveGameService = saveGameService;

        BackCommand = ReactiveCommand.Create(OnBack);
        StartGameCommand = ReactiveCommand.Create(OnStartGame);
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

    private void OnBack()
    {
        _navigationService.NavigateBack();
    }

    private void OnStartGame()
    {
        if (!CanStartGame) return;

        // Create the character
        var character = CharacterFactory.CreateCharacter(SelectedClass, CharacterName);

        Console.WriteLine($"[CHARACTER CREATION] Created character: {character.Name} ({character.Class})");
        Console.WriteLine($"[CHARACTER CREATION] Stats - HP: {character.HP}, Stamina: {character.Stamina}");
        Console.WriteLine($"[CHARACTER CREATION] Abilities: {character.Abilities.Count}");

        // TODO: In a full implementation, this would:
        // 1. Store the character in a game state service
        // 2. Navigate to the dungeon exploration view
        // For now, we'll just go back to the menu with a success message

        // Navigate back to menu (placeholder until game view is implemented)
        _navigationService.NavigateBack();
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
