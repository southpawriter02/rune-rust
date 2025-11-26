using RuneAndRust.Core;
using RuneAndRust.DesktopUI.Services;
using RuneAndRust.DesktopUI.ViewModels;
using RuneAndRust.Engine;
using Serilog;

namespace RuneAndRust.DesktopUI.Controllers;

/// <summary>
/// v0.44.2: Controller for character creation workflow.
/// Orchestrates the canonical v2.0 character creation sequence:
/// Lineage → Background → Attributes → Archetype → Specialization → Summary
/// </summary>
public class CharacterCreationController
{
    private readonly ILogger _logger;
    private readonly GameStateController _gameStateController;
    private readonly INavigationService _navigationService;
    private readonly DungeonGenerator? _dungeonGenerator;
    private readonly TemplateLibrary? _templateLibrary;

    // ViewModel reference for updating UI state
    private CharacterCreationViewModel? _viewModel;

    // Character creation state
    private Lineage _selectedLineage = Lineage.None;
    private Background _selectedBackground = Background.None;
    private CharacterClass _selectedArchetype = CharacterClass.Warrior;
    private Specialization _selectedSpecialization = Specialization.None;
    private string _characterName = "Survivor";
    private Attributes _customAttributes = new();
    private bool _useAdvancedMode = false;

    // Attribute allocation constants (v2.0 point-buy system)
    private const int TotalAttributePoints = 15;
    private const int BaseAttributeValue = 5;
    private const int MaxAttributeValue = 10;
    private const int MinAttributeValue = 3;

    // Adjusted base values per attribute (modified by lineage and background)
    private int _baseMight = BaseAttributeValue;
    private int _baseFinesse = BaseAttributeValue;
    private int _baseWits = BaseAttributeValue;
    private int _baseWill = BaseAttributeValue;
    private int _baseSturdiness = BaseAttributeValue;

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

    #region Public Properties

    /// <summary>Gets the selected lineage.</summary>
    public Lineage SelectedLineage => _selectedLineage;

    /// <summary>Gets the selected background.</summary>
    public Background SelectedBackground => _selectedBackground;

    /// <summary>Gets the selected archetype (class).</summary>
    public CharacterClass SelectedArchetype => _selectedArchetype;

    /// <summary>Gets the selected specialization.</summary>
    public Specialization SelectedSpecialization => _selectedSpecialization;

    /// <summary>Gets whether advanced attribute mode is enabled.</summary>
    public bool UseAdvancedMode => _useAdvancedMode;

    /// <summary>Gets the custom attributes for advanced mode.</summary>
    public Attributes CustomAttributes => _customAttributes;

    /// <summary>Gets the remaining attribute points in advanced mode.</summary>
    public int RemainingAttributePoints
    {
        get
        {
            int used = CalculateTotalPointsUsed();
            return TotalAttributePoints - used;
        }
    }

    /// <summary>Gets the adjusted base value for Might (influenced by lineage/background).</summary>
    public int BaseMight => _baseMight;

    /// <summary>Gets the adjusted base value for Finesse (influenced by lineage/background).</summary>
    public int BaseFinesse => _baseFinesse;

    /// <summary>Gets the adjusted base value for Wits (influenced by lineage/background).</summary>
    public int BaseWits => _baseWits;

    /// <summary>Gets the adjusted base value for Will (influenced by lineage/background).</summary>
    public int BaseWill => _baseWill;

    /// <summary>Gets the adjusted base value for Sturdiness (influenced by lineage/background).</summary>
    public int BaseSturdiness => _baseSturdiness;

    #endregion

    #region Workflow Methods

    /// <summary>
    /// Initializes the character creation workflow with the given ViewModel.
    /// Starts at Step 1: Lineage selection.
    /// </summary>
    public void Initialize(CharacterCreationViewModel viewModel)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        ResetToDefaults();

        _viewModel.CurrentStep = CharacterCreationStep.Lineage;
        LoadAvailableLineages();

        _logger.Information("[CHAR_CREATE] Workflow initialized, starting at Lineage selection");
    }

    /// <summary>
    /// Step 1: Handles lineage selection and proceeds to Background.
    /// </summary>
    public async Task OnLineageSelectedAsync(string lineageId)
    {
        if (!Enum.TryParse<Lineage>(lineageId, true, out var lineage) || lineage == Lineage.None)
        {
            _logger.Warning("[CHAR_CREATE] Invalid lineage: {Lineage}", lineageId);
            return;
        }

        _selectedLineage = lineage;
        _logger.Information("[CHAR_CREATE] Lineage selected: {Lineage}", lineage.GetDisplayName());

        // Update ViewModel and proceed to Background
        if (_viewModel != null)
        {
            _viewModel.SelectedLineage = lineage;
            _viewModel.CurrentStep = CharacterCreationStep.Background;
            LoadAvailableBackgrounds();
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Step 2: Handles background selection and proceeds to Attributes.
    /// </summary>
    public async Task OnBackgroundSelectedAsync(string backgroundId)
    {
        if (!Enum.TryParse<Background>(backgroundId, true, out var background) || background == Background.None)
        {
            _logger.Warning("[CHAR_CREATE] Invalid background: {Background}", backgroundId);
            return;
        }

        _selectedBackground = background;
        _logger.Information("[CHAR_CREATE] Background selected: {Background}", background.GetDisplayName());

        // Update ViewModel and proceed to Attributes
        if (_viewModel != null)
        {
            _viewModel.SelectedBackground = background;
            _viewModel.CurrentStep = CharacterCreationStep.Attributes;
            InitializeAttributeAllocation();
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Step 3: Toggles between simple (recommended) and advanced (custom) attribute allocation.
    /// </summary>
    public void OnAttributeAllocationModeChanged(bool useAdvanced)
    {
        _useAdvancedMode = useAdvanced;

        if (!useAdvanced)
        {
            // Simple mode: Will apply recommended build when archetype is selected
            ResetAttributesToBase();
        }
        else
        {
            // Advanced mode: Start with base values for manual allocation
            ResetAttributesToBase();
        }

        if (_viewModel != null)
        {
            _viewModel.UseAdvancedMode = useAdvanced;
            _viewModel.RemainingAttributePoints = RemainingAttributePoints;
        }

        _logger.Information("[CHAR_CREATE] Attribute mode: {Mode}", useAdvanced ? "Advanced" : "Simple");
    }

    /// <summary>
    /// Step 3 (Advanced): Adjusts an individual attribute value.
    /// Uses point-buy: 1pt for 5→8, 2pt for 8→10
    /// </summary>
    public bool OnAttributeChanged(string attributeName, int targetValue)
    {
        if (!_useAdvancedMode) return false;

        int currentValue = GetAttributeValue(attributeName);
        if (targetValue < MinAttributeValue || targetValue > MaxAttributeValue)
        {
            return false;
        }

        // Calculate cost difference
        int currentCost = CalculateAttributeCost(BaseAttributeValue, currentValue);
        int newCost = CalculateAttributeCost(BaseAttributeValue, targetValue);
        int costDiff = newCost - currentCost;

        // Check if we can afford the increase
        if (costDiff > 0 && costDiff > RemainingAttributePoints)
        {
            return false;
        }

        SetAttributeValue(attributeName, targetValue);

        if (_viewModel != null)
        {
            _viewModel.RemainingAttributePoints = RemainingAttributePoints;
            UpdateViewModelAttributes();
        }

        _logger.Debug("[CHAR_CREATE] Attribute {Attr} set to {Value}, remaining points: {Remaining}",
            attributeName, targetValue, RemainingAttributePoints);

        return true;
    }

    /// <summary>
    /// Calculates the point cost to raise an attribute from base to target.
    /// Uses v2.0 point-buy: 1pt per point from 5→8, 2pt per point from 8→10
    /// </summary>
    public int CalculateAttributeCost(int baseValue, int targetValue)
    {
        if (targetValue <= baseValue) return 0;

        int cost = 0;
        for (int v = baseValue + 1; v <= targetValue; v++)
        {
            // 1 point per level up to 8, 2 points per level from 9-10
            cost += (v <= 8) ? 1 : 2;
        }
        return cost;
    }

    /// <summary>
    /// Step 3: Confirms attribute allocation and proceeds to Archetype.
    /// </summary>
    public async Task OnAttributesConfirmedAsync()
    {
        // In simple mode, we proceed directly; attributes will be set based on archetype
        // In advanced mode, validate that all points are spent
        if (_useAdvancedMode && RemainingAttributePoints != 0)
        {
            _logger.Warning("[CHAR_CREATE] Cannot confirm - {Remaining} points remaining",
                RemainingAttributePoints);
            return;
        }

        if (_viewModel != null)
        {
            _viewModel.CurrentStep = CharacterCreationStep.Archetype;
            LoadAvailableArchetypes();
        }

        _logger.Information("[CHAR_CREATE] Attributes confirmed, proceeding to Archetype selection");
        await Task.CompletedTask;
    }

    /// <summary>
    /// Step 4: Handles archetype selection and proceeds to Specialization.
    /// In simple mode, also applies recommended attribute build.
    /// </summary>
    public async Task OnArchetypeSelectedAsync(string archetypeId)
    {
        if (!Enum.TryParse<CharacterClass>(archetypeId, true, out var archetype))
        {
            _logger.Warning("[CHAR_CREATE] Invalid archetype: {Archetype}", archetypeId);
            return;
        }

        _selectedArchetype = archetype;
        _selectedSpecialization = Specialization.None; // Reset specialization

        // Apply recommended build if in simple mode
        if (!_useAdvancedMode)
        {
            ApplyRecommendedBuild(archetype);
        }

        _logger.Information("[CHAR_CREATE] Archetype selected: {Archetype}", archetype);

        if (_viewModel != null)
        {
            _viewModel.SelectedArchetype = archetype;
            _viewModel.CurrentStep = CharacterCreationStep.Specialization;
            LoadAvailableSpecializations();
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Applies recommended attribute build for an archetype (simple mode).
    /// Distributes 15 points optimally for the archetype.
    /// </summary>
    public void ApplyRecommendedBuild(CharacterClass archetype)
    {
        _customAttributes = archetype switch
        {
            // Warrior: MIGHT/STURDINESS focused
            CharacterClass.Warrior => new Attributes(might: 8, finesse: 5, wits: 5, will: 5, sturdiness: 8),
            // Mystic: WILL/WITS focused
            CharacterClass.Mystic => new Attributes(might: 5, finesse: 5, wits: 7, will: 8, sturdiness: 5),
            // Adept: WITS focused, balanced support
            CharacterClass.Adept => new Attributes(might: 5, finesse: 7, wits: 8, will: 6, sturdiness: 6),
            // Skirmisher: FINESSE focused, high mobility
            CharacterClass.Skirmisher => new Attributes(might: 6, finesse: 8, wits: 6, will: 5, sturdiness: 7),
            // Default balanced
            _ => new Attributes(might: 7, finesse: 6, wits: 6, will: 6, sturdiness: 6)
        };

        _logger.Information("[CHAR_CREATE] Applied recommended build for {Archetype}", archetype);

        if (_viewModel != null)
        {
            UpdateViewModelAttributes();
        }
    }

    /// <summary>
    /// Step 5: Handles specialization selection and proceeds to Summary.
    /// </summary>
    public async Task OnSpecializationSelectedAsync(string specializationId)
    {
        if (!Enum.TryParse<Specialization>(specializationId, true, out var spec))
        {
            _logger.Warning("[CHAR_CREATE] Invalid specialization: {Spec}", specializationId);
            return;
        }

        // Validate specialization is valid for archetype
        if (!IsSpecializationValidForArchetype(spec, _selectedArchetype))
        {
            _logger.Warning("[CHAR_CREATE] Specialization {Spec} invalid for archetype {Archetype}",
                spec, _selectedArchetype);
            return;
        }

        _selectedSpecialization = spec;
        _logger.Information("[CHAR_CREATE] Specialization selected: {Spec}", spec);

        if (_viewModel != null)
        {
            _viewModel.SelectedSpecialization = spec;
            ShowSummary();
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Step 6: Shows the character summary for final review.
    /// </summary>
    public void ShowSummary()
    {
        if (_viewModel == null) return;

        _viewModel.CurrentStep = CharacterCreationStep.Summary;

        // Populate summary properties
        _viewModel.SummaryLineage = _selectedLineage.GetDisplayName();
        _viewModel.SummaryBackground = _selectedBackground.GetDisplayName();
        _viewModel.SummaryArchetype = _selectedArchetype.ToString();
        _viewModel.SummarySpecialization = _selectedSpecialization.ToString();
        _viewModel.SummaryAttributes = FormatAttributeSummary();

        _logger.Information("[CHAR_CREATE] Showing character summary");
    }

    /// <summary>
    /// Step 6: Confirms character creation and transitions to exploration.
    /// Initializes Saga System fields.
    /// </summary>
    public async Task<bool> OnConfirmCharacterAsync(string characterName)
    {
        // Validate name
        if (string.IsNullOrWhiteSpace(characterName))
        {
            _logger.Warning("[CHAR_CREATE] Character name is required");
            return false;
        }

        characterName = characterName.Trim();

        // v5.0 mandatory: ASCII-only names
        if (!IsASCIIOnly(characterName))
        {
            _logger.Warning("[CHAR_CREATE] Character name must be ASCII-only: {Name}", characterName);
            return false;
        }

        if (characterName.Length > 20)
        {
            _logger.Warning("[CHAR_CREATE] Character name too long: {Length}", characterName.Length);
            return false;
        }

        _characterName = characterName;

        // Validate full character configuration
        var (isValid, errors) = ValidateCharacter();
        if (!isValid)
        {
            _logger.Warning("[CHAR_CREATE] Validation failed: {Errors}", string.Join(", ", errors));
            _viewModel?.ShowValidationErrors(errors);
            return false;
        }

        try
        {
            // Create character using CharacterFactory
            var survivor = CharacterFactory.CreateCharacter(_selectedArchetype, _characterName);

            // Apply custom attributes
            survivor.Attributes = new Attributes(
                might: _customAttributes.Might,
                finesse: _customAttributes.Finesse,
                wits: _customAttributes.Wits,
                will: _customAttributes.Will,
                sturdiness: _customAttributes.Sturdiness
            );

            // Recalculate derived stats
            var equipService = new EquipmentService();
            equipService.RecalculatePlayerStats(survivor);

            // Set specialization
            survivor.Specialization = _selectedSpecialization;

            // MANDATORY: Initialize Saga System progression fields (v0.44.2)
            survivor.CurrentLegend = 0;           // No Legend earned yet
            survivor.ProgressionPoints = 0;        // No PP available to spend
            survivor.CurrentMilestone = 0;         // Starting at Milestone 0
            survivor.LegendToNextMilestone = 500;  // First milestone requires 500 Legend

            _logger.Information(
                "[CHAR_CREATE] Survivor initialized: {Name} ({Lineage} {Archetype}/{Spec}), " +
                "Legend={Legend}, PP={PP}, Milestone={Milestone}, NextMilestone={Next}",
                survivor.Name, _selectedLineage.GetDisplayName(), _selectedArchetype, _selectedSpecialization,
                survivor.CurrentLegend, survivor.ProgressionPoints, survivor.CurrentMilestone, survivor.LegendToNextMilestone);

            // Set player in game state
            _gameStateController.SetPlayer(survivor);

            // Generate starting Sector (dungeon)
            Room? startingRoom = null;
            DungeonGraph? dungeon = null;

            if (_dungeonGenerator != null)
            {
                var seed = DateTime.UtcNow.Millisecond + survivor.Name.GetHashCode();
                dungeon = _dungeonGenerator.Generate(seed, targetRoomCount: 7);
                startingRoom = FindStartingRoom(dungeon);

                if (dungeon != null && startingRoom != null)
                {
                    _gameStateController.SetDungeon(dungeon, startingRoom);
                    _logger.Information("[CHAR_CREATE] Starting Sector generated: {RoomCount} rooms, starting in {Room}",
                        dungeon.NodeCount, startingRoom.Name);
                }
            }

            // If no dungeon generator, create placeholder
            if (dungeon == null || startingRoom == null)
            {
                dungeon = new DungeonGraph();
                startingRoom = new Room
                {
                    RoomId = "1",
                    Name = "Sector Entrance",
                    Description = "The entrance to the ancient Dvergr ruins. Dust motes drift in the pale light filtering from above.",
                    IsStartRoom = true,
                    HasBeenCleared = true
                };
                _gameStateController.SetDungeon(dungeon, startingRoom);
            }

            // Transition to exploration
            await _gameStateController.UpdatePhaseAsync(Core.GamePhase.DungeonExploration, "Survivor creation complete");

            // Navigate to exploration view
            _navigationService.NavigateTo<DungeonExplorationViewModel>();

            _logger.Information("[CHAR_CREATE] Survivor {Name}'s saga begins in Sector {SectorId}",
                survivor.Name, startingRoom.RoomId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "[CHAR_CREATE] Error confirming Survivor");
            return false;
        }
    }

    /// <summary>
    /// Validates that a string contains only ASCII characters.
    /// v5.0 mandatory for character names.
    /// </summary>
    public bool IsASCIIOnly(string input)
    {
        return input.All(c => c <= 127);
    }

    /// <summary>
    /// Cancels character creation and returns to main menu.
    /// </summary>
    public async Task OnCancelAsync()
    {
        _logger.Information("[CHAR_CREATE] Character creation cancelled");
        _gameStateController.Reset();
        _navigationService.NavigateTo<MenuViewModel>();
        await Task.CompletedTask;
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Resets the character creation state to defaults.
    /// </summary>
    public void ResetToDefaults()
    {
        _selectedLineage = Lineage.None;
        _selectedBackground = Background.None;
        _selectedArchetype = CharacterClass.Warrior;
        _selectedSpecialization = Specialization.None;
        _characterName = "Survivor";
        _useAdvancedMode = false;
        ResetAttributesToBase();
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
            errors.Add("Survivor name is required.");
        }
        else if (_characterName.Length > 20)
        {
            errors.Add("Survivor name must be 20 characters or less.");
        }
        else if (!IsASCIIOnly(_characterName))
        {
            errors.Add("Survivor name must contain only ASCII characters.");
        }

        // Lineage validation
        if (_selectedLineage == Lineage.None)
        {
            errors.Add("You must select a lineage.");
        }

        // Background validation
        if (_selectedBackground == Background.None)
        {
            errors.Add("You must select a background.");
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
            if (_customAttributes.Might < MinAttributeValue ||
                _customAttributes.Finesse < MinAttributeValue ||
                _customAttributes.Wits < MinAttributeValue ||
                _customAttributes.Will < MinAttributeValue ||
                _customAttributes.Sturdiness < MinAttributeValue)
            {
                errors.Add($"All attributes must be at least {MinAttributeValue}.");
            }
        }

        return (errors.Count == 0, errors);
    }

    /// <summary>
    /// Gets available specializations for the selected archetype.
    /// </summary>
    public IEnumerable<Specialization> GetAvailableSpecializations()
    {
        return _selectedArchetype switch
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

    private bool IsSpecializationValidForArchetype(Specialization spec, CharacterClass archetype)
    {
        return archetype switch
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

    private void ResetAttributesToBase()
    {
        // Use adjusted base values influenced by lineage and background
        _customAttributes = new Attributes(
            might: _baseMight,
            finesse: _baseFinesse,
            wits: _baseWits,
            will: _baseWill,
            sturdiness: _baseSturdiness
        );
    }

    /// <summary>
    /// Calculates adjusted base values for each attribute based on selected lineage and background.
    /// Lineage provides +1/-1 modifiers, Background provides +1 to its primary attribute.
    /// </summary>
    private void CalculateAdjustedBaseValues()
    {
        // Start with standard base values
        _baseMight = BaseAttributeValue;
        _baseFinesse = BaseAttributeValue;
        _baseWits = BaseAttributeValue;
        _baseWill = BaseAttributeValue;
        _baseSturdiness = BaseAttributeValue;

        // Apply lineage modifiers
        if (_selectedLineage != Lineage.None)
        {
            var lineageMods = _selectedLineage.GetAttributeModifiers();
            foreach (var mod in lineageMods)
            {
                ApplyModifier(mod.Key, mod.Value);
            }
        }

        // Apply background bonus (+1 to primary attribute)
        if (_selectedBackground != Background.None)
        {
            string primaryAttr = _selectedBackground.GetPrimaryAttributeBonus();
            if (!string.IsNullOrEmpty(primaryAttr))
            {
                ApplyModifier(primaryAttr, 1);
            }
        }

        // Clamp all values to valid range
        _baseMight = Math.Clamp(_baseMight, MinAttributeValue, MaxAttributeValue);
        _baseFinesse = Math.Clamp(_baseFinesse, MinAttributeValue, MaxAttributeValue);
        _baseWits = Math.Clamp(_baseWits, MinAttributeValue, MaxAttributeValue);
        _baseWill = Math.Clamp(_baseWill, MinAttributeValue, MaxAttributeValue);
        _baseSturdiness = Math.Clamp(_baseSturdiness, MinAttributeValue, MaxAttributeValue);

        _logger.Debug("[CHAR_CREATE] Adjusted bases: MIGHT={Might}, FINESSE={Finesse}, WITS={Wits}, WILL={Will}, STURDINESS={Sturdiness}",
            _baseMight, _baseFinesse, _baseWits, _baseWill, _baseSturdiness);
    }

    /// <summary>Applies a modifier to the specified attribute's base value.</summary>
    private void ApplyModifier(string attributeName, int modifier)
    {
        switch (attributeName.ToUpperInvariant())
        {
            case "MIGHT": _baseMight += modifier; break;
            case "FINESSE": _baseFinesse += modifier; break;
            case "WITS": _baseWits += modifier; break;
            case "WILL": _baseWill += modifier; break;
            case "STURDINESS": _baseSturdiness += modifier; break;
        }
    }

    /// <summary>Gets the adjusted base value for a specific attribute.</summary>
    public int GetAdjustedBase(string attributeName)
    {
        return attributeName.ToUpperInvariant() switch
        {
            "MIGHT" => _baseMight,
            "FINESSE" => _baseFinesse,
            "WITS" => _baseWits,
            "WILL" => _baseWill,
            "STURDINESS" => _baseSturdiness,
            _ => BaseAttributeValue
        };
    }

    private int CalculateTotalPointsUsed()
    {
        int cost = 0;
        // Use adjusted bases for each attribute
        cost += CalculateAttributeCost(_baseMight, _customAttributes.Might);
        cost += CalculateAttributeCost(_baseFinesse, _customAttributes.Finesse);
        cost += CalculateAttributeCost(_baseWits, _customAttributes.Wits);
        cost += CalculateAttributeCost(_baseWill, _customAttributes.Will);
        cost += CalculateAttributeCost(_baseSturdiness, _customAttributes.Sturdiness);
        return cost;
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

    private string FormatAttributeSummary()
    {
        return $"MIGHT: {_customAttributes.Might}, FINESSE: {_customAttributes.Finesse}, " +
               $"WITS: {_customAttributes.Wits}, WILL: {_customAttributes.Will}, STURDINESS: {_customAttributes.Sturdiness}";
    }

    private Room? FindStartingRoom(DungeonGraph? dungeon)
    {
        if (dungeon == null) return null;

        var startNode = dungeon.StartNode;
        if (startNode == null) return null;

        return new Room
        {
            RoomId = startNode.Id.ToString(),
            Name = "Sector Entrance",
            Description = "The entrance to the ancient Dvergr ruins. Dust motes drift in the pale light filtering from above.",
            IsStartRoom = true,
            HasBeenCleared = true
        };
    }

    #region ViewModel Loaders

    private void LoadAvailableLineages()
    {
        if (_viewModel == null) return;

        _viewModel.AvailableLineages.Clear();
        foreach (var lineage in Enum.GetValues<Lineage>().Where(l => l != Lineage.None))
        {
            _viewModel.AvailableLineages.Add(new LineageInfo
            {
                Lineage = lineage,
                Name = lineage.GetDisplayName(),
                Description = lineage.GetDescription()
            });
        }
    }

    private void LoadAvailableBackgrounds()
    {
        if (_viewModel == null) return;

        _viewModel.AvailableBackgrounds.Clear();
        foreach (var bg in Enum.GetValues<Background>().Where(b => b != Background.None))
        {
            _viewModel.AvailableBackgrounds.Add(new BackgroundInfo
            {
                Background = bg,
                Name = bg.GetDisplayName(),
                Description = bg.GetDescription(),
                PrimaryAttribute = bg.GetPrimaryAttributeBonus()
            });
        }
    }

    private void InitializeAttributeAllocation()
    {
        // Calculate adjusted base values based on lineage and background
        CalculateAdjustedBaseValues();

        // Reset attributes to the adjusted bases
        ResetAttributesToBase();

        if (_viewModel != null)
        {
            // Update ViewModel with adjusted bases
            _viewModel.BaseMight = _baseMight;
            _viewModel.BaseFinesse = _baseFinesse;
            _viewModel.BaseWits = _baseWits;
            _viewModel.BaseWill = _baseWill;
            _viewModel.BaseSturdiness = _baseSturdiness;

            _viewModel.RemainingAttributePoints = TotalAttributePoints;
            UpdateViewModelAttributes();
        }
    }

    private void LoadAvailableArchetypes()
    {
        if (_viewModel == null) return;

        _viewModel.AvailableArchetypes.Clear();
        _viewModel.AvailableArchetypes.Add(CharacterClass.Warrior);
        _viewModel.AvailableArchetypes.Add(CharacterClass.Mystic);
        _viewModel.AvailableArchetypes.Add(CharacterClass.Adept);
        _viewModel.AvailableArchetypes.Add(CharacterClass.Skirmisher);
    }

    private void LoadAvailableSpecializations()
    {
        if (_viewModel == null) return;

        _viewModel.AvailableSpecializations.Clear();
        foreach (var spec in GetAvailableSpecializations())
        {
            _viewModel.AvailableSpecializations.Add(new SpecializationInfo
            {
                Specialization = spec,
                Name = spec.ToString(),
                Description = GetSpecializationDescription(spec)
            });
        }
    }

    private void UpdateViewModelAttributes()
    {
        if (_viewModel == null) return;

        _viewModel.AttributeMight = _customAttributes.Might;
        _viewModel.AttributeFinesse = _customAttributes.Finesse;
        _viewModel.AttributeWits = _customAttributes.Wits;
        _viewModel.AttributeWill = _customAttributes.Will;
        _viewModel.AttributeSturdiness = _customAttributes.Sturdiness;
        _viewModel.RemainingAttributePoints = RemainingAttributePoints;
    }

    #endregion

    #endregion
}
