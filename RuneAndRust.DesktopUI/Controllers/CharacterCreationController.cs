using RuneAndRust.Core;
using RuneAndRust.DesktopUI.Services;
using RuneAndRust.DesktopUI.ViewModels;
using RuneAndRust.Engine;
using Serilog;

namespace RuneAndRust.DesktopUI.Controllers;

/// <summary>
/// v0.44.2: Controller for character creation workflow.
/// Manages the step-based flow from class selection to dungeon entry.
/// </summary>
public class CharacterCreationController
{
    private readonly ILogger _logger;
    private readonly GameStateController _gameStateController;
    private readonly INavigationService _navigationService;
    private readonly DungeonGenerator? _dungeonGenerator;
    private readonly TemplateLibrary? _templateLibrary;

    // Character in progress
    private CharacterClass _selectedClass = CharacterClass.Warrior;
    private Specialization _selectedSpecialization = Specialization.None;
    private string _characterName = "Survivor";
    private Attributes _customAttributes = new();
    private bool _useAdvancedMode = false;

    // Attribute allocation tracking
    private const int TotalAttributePoints = 14; // Base 3 in each (15) - need to allocate
    private const int BaseAttributeValue = 1;
    private const int MaxAttributeValue = 6;

    public CharacterCreationController(
        ILogger logger,
        GameStateController gameStateController,
        INavigationService navigationService,
        TemplateLibrary? templateLibrary = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gameStateController = gameStateController ?? throw new ArgumentNullException(nameof(gameStateController));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _templateLibrary = templateLibrary;

        // Create dungeon generator if template library available
        if (_templateLibrary != null)
        {
            _dungeonGenerator = new DungeonGenerator(_templateLibrary);
        }

        ResetToDefaults();
    }

    /// <summary>
    /// Gets the currently selected class.
    /// </summary>
    public CharacterClass SelectedClass => _selectedClass;

    /// <summary>
    /// Gets the currently selected specialization.
    /// </summary>
    public Specialization SelectedSpecialization => _selectedSpecialization;

    /// <summary>
    /// Gets whether advanced attribute mode is enabled.
    /// </summary>
    public bool UseAdvancedMode => _useAdvancedMode;

    /// <summary>
    /// Gets the custom attributes for advanced mode.
    /// </summary>
    public Attributes CustomAttributes => _customAttributes;

    /// <summary>
    /// Gets the remaining attribute points in advanced mode.
    /// </summary>
    public int RemainingAttributePoints
    {
        get
        {
            int used = _customAttributes.Might + _customAttributes.Finesse +
                       _customAttributes.Wits + _customAttributes.Will + _customAttributes.Sturdiness;
            return TotalAttributePoints - (used - 5); // 5 is base (1 in each)
        }
    }

    /// <summary>
    /// Resets the character creation state to defaults.
    /// </summary>
    public void ResetToDefaults()
    {
        _selectedClass = CharacterClass.Warrior;
        _selectedSpecialization = Specialization.None;
        _characterName = "Survivor";
        _useAdvancedMode = false;
        ResetAttributesToBase();
    }

    /// <summary>
    /// Selects a character class.
    /// </summary>
    public void SelectClass(CharacterClass characterClass)
    {
        _selectedClass = characterClass;
        _selectedSpecialization = Specialization.None; // Reset specialization when class changes

        // Update default attributes based on class
        if (!_useAdvancedMode)
        {
            ApplyRecommendedBuild();
        }

        _logger.Information("Class selected: {Class}", characterClass);
    }

    /// <summary>
    /// Selects a specialization for the current class.
    /// </summary>
    public void SelectSpecialization(Specialization specialization)
    {
        // Validate specialization is valid for class
        if (!IsSpecializationValidForClass(specialization, _selectedClass))
        {
            _logger.Warning("Invalid specialization {Spec} for class {Class}", specialization, _selectedClass);
            return;
        }

        _selectedSpecialization = specialization;
        _logger.Information("Specialization selected: {Spec}", specialization);
    }

    /// <summary>
    /// Sets the character name.
    /// </summary>
    public void SetCharacterName(string name)
    {
        _characterName = string.IsNullOrWhiteSpace(name) ? "Survivor" : name.Trim();
    }

    /// <summary>
    /// Toggles between simple and advanced attribute allocation modes.
    /// </summary>
    public void SetAdvancedMode(bool useAdvanced)
    {
        _useAdvancedMode = useAdvanced;

        if (!useAdvanced)
        {
            ApplyRecommendedBuild();
        }
        else
        {
            ResetAttributesToBase();
        }

        _logger.Information("Attribute mode changed: Advanced={Advanced}", useAdvanced);
    }

    /// <summary>
    /// Adjusts an attribute in advanced mode.
    /// </summary>
    public bool AdjustAttribute(string attributeName, int delta)
    {
        if (!_useAdvancedMode) return false;

        int currentValue = GetAttributeValue(attributeName);
        int newValue = currentValue + delta;

        // Validate bounds
        if (newValue < BaseAttributeValue || newValue > MaxAttributeValue)
        {
            return false;
        }

        // Check if we have points for increase
        if (delta > 0 && RemainingAttributePoints < delta)
        {
            return false;
        }

        SetAttributeValue(attributeName, newValue);
        _logger.Debug("Attribute adjusted: {Attr} = {Value} (Remaining: {Remaining})",
            attributeName, newValue, RemainingAttributePoints);

        return true;
    }

    /// <summary>
    /// Gets available specializations for the selected class.
    /// </summary>
    public IEnumerable<Specialization> GetAvailableSpecializations()
    {
        return _selectedClass switch
        {
            CharacterClass.Warrior => new[]
            {
                Specialization.SkarHordeAspirant,
                Specialization.IronBane,
                Specialization.AtgeirWielder
            },
            CharacterClass.Adept => new[]
            {
                Specialization.BoneSetter,
                Specialization.ScrapTinker,
                Specialization.JotunReader
            },
            CharacterClass.Mystic => new[]
            {
                Specialization.VardWarden,
                Specialization.RustWitch
            },
            // Other classes don't have specializations defined yet
            _ => Array.Empty<Specialization>()
        };
    }

    /// <summary>
    /// Gets a description for a specialization.
    /// </summary>
    public string GetSpecializationDescription(Specialization spec)
    {
        return spec switch
        {
            Specialization.SkarHordeAspirant => "Savage berserker fueled by Savagery. High damage, enters rage state.",
            Specialization.IronBane => "Zealot specialized against mechanical and undying foes. Uses Righteous Fervor.",
            Specialization.AtgeirWielder => "Master of the versatile atgeir polearm. Superior reach and tactics.",
            Specialization.BoneSetter => "Non-magical medic and healer. Supports allies in and out of combat.",
            Specialization.ScrapTinker => "Crafting specialist and gadgeteer. Creates potions and devices.",
            Specialization.JotunReader => "System diagnostician and analyst. Reveals enemy weaknesses.",
            Specialization.VardWarden => "Defensive caster using runic barriers and sanctified ground.",
            Specialization.RustWitch => "Heretical debuffer wielding corrosion and entropy magic.",
            _ => "Unlock specializations by spending Progression Points during gameplay."
        };
    }

    /// <summary>
    /// Validates the current character configuration.
    /// </summary>
    public (bool IsValid, List<string> Errors) ValidateCharacter()
    {
        var errors = new List<string>();

        // Name validation
        if (string.IsNullOrWhiteSpace(_characterName))
        {
            errors.Add("Character name is required.");
        }
        else if (_characterName.Length > 20)
        {
            errors.Add("Character name must be 20 characters or less.");
        }

        // Attribute validation in advanced mode
        if (_useAdvancedMode)
        {
            if (RemainingAttributePoints < 0)
            {
                errors.Add("You have allocated too many attribute points.");
            }
            else if (RemainingAttributePoints > 0)
            {
                errors.Add($"You have {RemainingAttributePoints} unspent attribute points.");
            }

            // Check minimum values
            if (_customAttributes.Might < BaseAttributeValue ||
                _customAttributes.Finesse < BaseAttributeValue ||
                _customAttributes.Wits < BaseAttributeValue ||
                _customAttributes.Will < BaseAttributeValue ||
                _customAttributes.Sturdiness < BaseAttributeValue)
            {
                errors.Add("All attributes must be at least 1.");
            }
        }

        return (errors.Count == 0, errors);
    }

    /// <summary>
    /// Creates the character and transitions to dungeon exploration.
    /// </summary>
    public async Task<bool> ConfirmCharacterAsync()
    {
        // Validate
        var (isValid, errors) = ValidateCharacter();
        if (!isValid)
        {
            _logger.Warning("Character validation failed: {Errors}", string.Join(", ", errors));
            return false;
        }

        try
        {
            // Create character using CharacterFactory
            var character = CharacterFactory.CreateCharacter(_selectedClass, _characterName);

            // Apply custom attributes if in advanced mode
            if (_useAdvancedMode)
            {
                character.Attributes = new Attributes(
                    might: _customAttributes.Might,
                    finesse: _customAttributes.Finesse,
                    wits: _customAttributes.Wits,
                    will: _customAttributes.Will,
                    sturdiness: _customAttributes.Sturdiness
                );

                // Recalculate derived stats based on new attributes
                var equipService = new EquipmentService();
                equipService.RecalculatePlayerStats(character);
            }

            // Set specialization (will unlock abilities when PP is spent in gameplay)
            character.Specialization = _selectedSpecialization;

            _logger.Information("Character created: {Name} ({Class}), Spec={Spec}, HP={HP}, Stamina={Stamina}",
                character.Name, character.Class, character.Specialization, character.HP, character.Stamina);

            // Set player in game state
            _gameStateController.SetPlayer(character);

            // Generate dungeon
            Room? startingRoom = null;
            DungeonGraph? dungeon = null;

            if (_dungeonGenerator != null)
            {
                var seed = DateTime.UtcNow.Millisecond + character.Name.GetHashCode();
                dungeon = _dungeonGenerator.Generate(seed, targetRoomCount: 7);
                startingRoom = FindStartingRoom(dungeon);

                if (dungeon != null && startingRoom != null)
                {
                    _gameStateController.SetDungeon(dungeon, startingRoom);
                    _logger.Information("Dungeon generated with {Count} nodes, starting in: {Room}",
                        dungeon.NodeCount, startingRoom.Name);
                }
            }

            // Transition to exploration
            await _gameStateController.UpdatePhaseAsync(GamePhase.DungeonExploration, "Character creation complete");

            // Navigate to exploration view
            _navigationService.NavigateTo<DungeonExplorationViewModel>();

            _logger.Information("Character confirmed, transitioning to dungeon exploration");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error confirming character");
            return false;
        }
    }

    /// <summary>
    /// Cancels character creation and returns to main menu.
    /// </summary>
    public void Cancel()
    {
        _logger.Information("Character creation cancelled");
        _gameStateController.Reset();
        _navigationService.NavigateTo<MenuViewModel>();
    }

    private bool IsSpecializationValidForClass(Specialization spec, CharacterClass charClass)
    {
        return charClass switch
        {
            CharacterClass.Warrior => spec is Specialization.SkarHordeAspirant or
                                              Specialization.IronBane or
                                              Specialization.AtgeirWielder or
                                              Specialization.None,
            CharacterClass.Adept => spec is Specialization.BoneSetter or
                                            Specialization.ScrapTinker or
                                            Specialization.JotunReader or
                                            Specialization.None,
            CharacterClass.Mystic => spec is Specialization.VardWarden or
                                             Specialization.RustWitch or
                                             Specialization.None,
            _ => spec == Specialization.None
        };
    }

    private void ApplyRecommendedBuild()
    {
        _customAttributes = _selectedClass switch
        {
            CharacterClass.Warrior => new Attributes(might: 4, finesse: 2, wits: 2, will: 2, sturdiness: 4),
            CharacterClass.Scavenger => new Attributes(might: 3, finesse: 3, wits: 3, will: 2, sturdiness: 3),
            CharacterClass.Mystic => new Attributes(might: 2, finesse: 2, wits: 3, will: 4, sturdiness: 2),
            CharacterClass.Adept => new Attributes(might: 2, finesse: 3, wits: 4, will: 3, sturdiness: 2),
            CharacterClass.Skirmisher => new Attributes(might: 2, finesse: 4, wits: 3, will: 2, sturdiness: 3),
            _ => new Attributes(might: 3, finesse: 3, wits: 3, will: 3, sturdiness: 3)
        };
    }

    private void ResetAttributesToBase()
    {
        _customAttributes = new Attributes(
            might: BaseAttributeValue,
            finesse: BaseAttributeValue,
            wits: BaseAttributeValue,
            will: BaseAttributeValue,
            sturdiness: BaseAttributeValue
        );
    }

    private int GetAttributeValue(string attributeName)
    {
        return attributeName.ToUpperInvariant() switch
        {
            "MIGHT" => _customAttributes.Might,
            "FINESSE" => _customAttributes.Finesse,
            "WITS" => _customAttributes.Wits,
            "WILL" => _customAttributes.Will,
            "STURDINESS" => _customAttributes.Sturdiness,
            _ => 0
        };
    }

    private void SetAttributeValue(string attributeName, int value)
    {
        _customAttributes = attributeName.ToUpperInvariant() switch
        {
            "MIGHT" => new Attributes(value, _customAttributes.Finesse, _customAttributes.Wits, _customAttributes.Will, _customAttributes.Sturdiness),
            "FINESSE" => new Attributes(_customAttributes.Might, value, _customAttributes.Wits, _customAttributes.Will, _customAttributes.Sturdiness),
            "WITS" => new Attributes(_customAttributes.Might, _customAttributes.Finesse, value, _customAttributes.Will, _customAttributes.Sturdiness),
            "WILL" => new Attributes(_customAttributes.Might, _customAttributes.Finesse, _customAttributes.Wits, value, _customAttributes.Sturdiness),
            "STURDINESS" => new Attributes(_customAttributes.Might, _customAttributes.Finesse, _customAttributes.Wits, _customAttributes.Will, value),
            _ => _customAttributes
        };
    }

    private Room? FindStartingRoom(DungeonGraph? dungeon)
    {
        if (dungeon == null) return null;

        // Get the start node and create a room from it
        var startNode = dungeon.StartNode;
        if (startNode == null) return null;

        return new Room
        {
            RoomId = startNode.Id.ToString(),
            Name = "Dungeon Entrance",
            Description = "The entrance to the ancient ruins. Dust motes drift in the pale light filtering from above.",
            IsStartRoom = true,
            HasBeenCleared = true // Start room is safe
        };
    }
}
