using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using RuneAndRust.Core;
using RuneAndRust.DesktopUI.Controllers;
using RuneAndRust.DesktopUI.Services;
using RuneAndRust.Engine;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RuneAndRust.DesktopUI.ViewModels;

/// <summary>
/// v0.44.2: Character creation workflow steps.
/// Follows the canonical v2.0 sequence: Lineage → Background → Attributes → Archetype → Specialization → Summary
/// </summary>
public enum CharacterCreationStep
{
    /// <summary>Step 1: Select bloodline/heritage traits</summary>
    Lineage,
    /// <summary>Step 2: Select pre-crash profession</summary>
    Background,
    /// <summary>Step 3: Allocate attribute points</summary>
    Attributes,
    /// <summary>Step 4: Select archetype (class)</summary>
    Archetype,
    /// <summary>Step 5: Select specialization</summary>
    Specialization,
    /// <summary>Step 6: Review and confirm</summary>
    Summary
}

/// <summary>
/// v0.44.2: View model for character creation screen.
/// Supports the complete 6-step character creation workflow.
/// Integrates with CharacterCreationController for game state management.
/// </summary>
public class CharacterCreationViewModel : ViewModelBase
{
    private readonly INavigationService? _navigationService;
    private readonly CharacterCreationController? _controller;
    private readonly GameStateController? _gameStateController;
    private readonly ILogger? _logger;

    // Step tracking
    private CharacterCreationStep _currentStep = CharacterCreationStep.Lineage;
    private bool _useAdvancedMode = false;
    private int _remainingAttributePoints = 15;

    // Selection state
    private Lineage _selectedLineage = Lineage.None;
    private Background _selectedBackground = Background.None;
    private CharacterClass _selectedArchetype = CharacterClass.Warrior;
    private Specialization _selectedSpecialization = Specialization.None;
    private string _characterName = "Survivor";

    // Attribute values
    private int _attributeMight = 5;
    private int _attributeFinesse = 5;
    private int _attributeWits = 5;
    private int _attributeWill = 5;
    private int _attributeSturdiness = 5;

    // Base attribute values (adjusted by lineage/background)
    private int _baseMight = 5;
    private int _baseFinesse = 5;
    private int _baseWits = 5;
    private int _baseWill = 5;
    private int _baseSturdiness = 5;

    // Summary properties
    private string _summaryLineage = string.Empty;
    private string _summaryBackground = string.Empty;
    private string _summaryArchetype = string.Empty;
    private string _summarySpecialization = string.Empty;
    private string _summaryAttributes = string.Empty;

    // Validation
    private List<string> _validationErrors = new();

    #region Properties

    /// <summary>
    /// Current step in the character creation workflow.
    /// </summary>
    public CharacterCreationStep CurrentStep
    {
        get => _currentStep;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentStep, value);
            this.RaisePropertyChanged(nameof(StepTitle));
            this.RaisePropertyChanged(nameof(CanGoBack));
            this.RaisePropertyChanged(nameof(CanGoForward));
        }
    }

    /// <summary>
    /// Display title for the current step.
    /// </summary>
    public string StepTitle => CurrentStep switch
    {
        CharacterCreationStep.Lineage => "Step 1: Choose Your Lineage",
        CharacterCreationStep.Background => "Step 2: Choose Your Background",
        CharacterCreationStep.Attributes => "Step 3: Allocate Attributes",
        CharacterCreationStep.Archetype => "Step 4: Choose Your Archetype",
        CharacterCreationStep.Specialization => "Step 5: Choose Your Specialization",
        CharacterCreationStep.Summary => "Step 6: Confirm Your Survivor",
        _ => "Create Your Survivor"
    };

    /// <summary>
    /// Whether advanced attribute allocation mode is enabled.
    /// </summary>
    public bool UseAdvancedMode
    {
        get => _useAdvancedMode;
        set => this.RaiseAndSetIfChanged(ref _useAdvancedMode, value);
    }

    /// <summary>
    /// Remaining attribute points to allocate.
    /// </summary>
    public int RemainingAttributePoints
    {
        get => _remainingAttributePoints;
        set
        {
            if (this.RaiseAndSetIfChanged(ref _remainingAttributePoints, value) != value) return;
            // When points change, all increase costs may become available/unavailable
            NotifyAttributeCostChanges();
        }
    }

    /// <summary>
    /// Character name input.
    /// </summary>
    public string CharacterName
    {
        get => _characterName;
        set
        {
            this.RaiseAndSetIfChanged(ref _characterName, value);
            this.RaisePropertyChanged(nameof(CanConfirm));
        }
    }

    /// <summary>
    /// Whether the character can be confirmed (valid name entered).
    /// </summary>
    public bool CanConfirm => !string.IsNullOrWhiteSpace(CharacterName) && CharacterName.Length <= 20;

    /// <summary>
    /// Whether back navigation is available.
    /// </summary>
    public bool CanGoBack => CurrentStep != CharacterCreationStep.Lineage;

    /// <summary>
    /// Whether forward navigation is available.
    /// </summary>
    public bool CanGoForward => CurrentStep != CharacterCreationStep.Summary;

    /// <summary>
    /// Page title.
    /// </summary>
    public string Title => "Create Your Survivor";

    #endregion

    #region Selection Properties

    /// <summary>Selected lineage.</summary>
    public Lineage SelectedLineage
    {
        get => _selectedLineage;
        set => this.RaiseAndSetIfChanged(ref _selectedLineage, value);
    }

    /// <summary>Selected background.</summary>
    public Background SelectedBackground
    {
        get => _selectedBackground;
        set => this.RaiseAndSetIfChanged(ref _selectedBackground, value);
    }

    /// <summary>Selected archetype (class).</summary>
    public CharacterClass SelectedArchetype
    {
        get => _selectedArchetype;
        set => this.RaiseAndSetIfChanged(ref _selectedArchetype, value);
    }

    /// <summary>Selected specialization.</summary>
    public Specialization SelectedSpecialization
    {
        get => _selectedSpecialization;
        set => this.RaiseAndSetIfChanged(ref _selectedSpecialization, value);
    }

    #endregion

    #region Attribute Properties

    public int AttributeMight
    {
        get => _attributeMight;
        set
        {
            if (this.RaiseAndSetIfChanged(ref _attributeMight, value) != value) return;
            NotifyAttributeCostChanges();
        }
    }

    public int AttributeFinesse
    {
        get => _attributeFinesse;
        set
        {
            if (this.RaiseAndSetIfChanged(ref _attributeFinesse, value) != value) return;
            NotifyAttributeCostChanges();
        }
    }

    public int AttributeWits
    {
        get => _attributeWits;
        set
        {
            if (this.RaiseAndSetIfChanged(ref _attributeWits, value) != value) return;
            NotifyAttributeCostChanges();
        }
    }

    public int AttributeWill
    {
        get => _attributeWill;
        set
        {
            if (this.RaiseAndSetIfChanged(ref _attributeWill, value) != value) return;
            NotifyAttributeCostChanges();
        }
    }

    public int AttributeSturdiness
    {
        get => _attributeSturdiness;
        set
        {
            if (this.RaiseAndSetIfChanged(ref _attributeSturdiness, value) != value) return;
            NotifyAttributeCostChanges();
        }
    }

    #endregion

    #region Base Attribute Properties (Adjusted by Lineage/Background)

    /// <summary>Base Might value adjusted by lineage/background.</summary>
    public int BaseMight
    {
        get => _baseMight;
        set
        {
            if (this.RaiseAndSetIfChanged(ref _baseMight, value) != value) return;
            NotifyAttributeCostChanges();
        }
    }

    /// <summary>Base Finesse value adjusted by lineage/background.</summary>
    public int BaseFinesse
    {
        get => _baseFinesse;
        set
        {
            if (this.RaiseAndSetIfChanged(ref _baseFinesse, value) != value) return;
            NotifyAttributeCostChanges();
        }
    }

    /// <summary>Base Wits value adjusted by lineage/background.</summary>
    public int BaseWits
    {
        get => _baseWits;
        set
        {
            if (this.RaiseAndSetIfChanged(ref _baseWits, value) != value) return;
            NotifyAttributeCostChanges();
        }
    }

    /// <summary>Base Will value adjusted by lineage/background.</summary>
    public int BaseWill
    {
        get => _baseWill;
        set
        {
            if (this.RaiseAndSetIfChanged(ref _baseWill, value) != value) return;
            NotifyAttributeCostChanges();
        }
    }

    /// <summary>Base Sturdiness value adjusted by lineage/background.</summary>
    public int BaseSturdiness
    {
        get => _baseSturdiness;
        set
        {
            if (this.RaiseAndSetIfChanged(ref _baseSturdiness, value) != value) return;
            NotifyAttributeCostChanges();
        }
    }

    #endregion

    #region Attribute Cost Properties

    // Constants for cost calculations
    private const int DefaultBaseAttributeValue = 5;
    private const int MaxAttributeValue = 10;
    private const int MinAttributeValue = 3;

    /// <summary>Gets cost to increase attribute by 1 (0 if at max or not enough points).</summary>
    private int GetIncreaseCost(int currentValue)
    {
        if (currentValue >= MaxAttributeValue) return 0;
        int nextValue = currentValue + 1;
        int cost = nextValue <= 8 ? 1 : 2;
        return cost <= RemainingAttributePoints ? cost : 0;
    }

    /// <summary>Gets points refunded by decreasing attribute by 1 (0 if at min or at/below base).</summary>
    private int GetDecreaseCost(int currentValue, int baseValue)
    {
        if (currentValue <= MinAttributeValue) return 0; // Can't go lower than min
        if (currentValue <= baseValue) return 0; // At or below base = no refund
        // Refund is the cost paid to reach current level from previous
        return currentValue <= 8 ? 1 : 2;
    }

    // Cost to increase each attribute by 1
    public int MightIncreaseCost => GetIncreaseCost(AttributeMight);
    public int FinesseIncreaseCost => GetIncreaseCost(AttributeFinesse);
    public int WitsIncreaseCost => GetIncreaseCost(AttributeWits);
    public int WillIncreaseCost => GetIncreaseCost(AttributeWill);
    public int SturdinessIncreaseCost => GetIncreaseCost(AttributeSturdiness);

    // Points refunded by decreasing each attribute by 1 (uses per-attribute base)
    public int MightDecreaseCost => GetDecreaseCost(AttributeMight, BaseMight);
    public int FinesseDecreaseCost => GetDecreaseCost(AttributeFinesse, BaseFinesse);
    public int WitsDecreaseCost => GetDecreaseCost(AttributeWits, BaseWits);
    public int WillDecreaseCost => GetDecreaseCost(AttributeWill, BaseWill);
    public int SturdinessDecreaseCost => GetDecreaseCost(AttributeSturdiness, BaseSturdiness);

    // Can increase/decrease checks (can't go below base value)
    public bool CanIncreaseMight => AttributeMight < MaxAttributeValue && MightIncreaseCost > 0;
    public bool CanDecreaseMight => AttributeMight > BaseMight;
    public bool CanIncreaseFinesse => AttributeFinesse < MaxAttributeValue && FinesseIncreaseCost > 0;
    public bool CanDecreaseFinesse => AttributeFinesse > BaseFinesse;
    public bool CanIncreaseWits => AttributeWits < MaxAttributeValue && WitsIncreaseCost > 0;
    public bool CanDecreaseWits => AttributeWits > BaseWits;
    public bool CanIncreaseWill => AttributeWill < MaxAttributeValue && WillIncreaseCost > 0;
    public bool CanDecreaseWill => AttributeWill > BaseWill;
    public bool CanIncreaseSturdiness => AttributeSturdiness < MaxAttributeValue && SturdinessIncreaseCost > 0;
    public bool CanDecreaseSturdiness => AttributeSturdiness > BaseSturdiness;

    /// <summary>Notifies all cost-related properties when any attribute changes.</summary>
    private void NotifyAttributeCostChanges()
    {
        // Notify all cost and can-change properties
        this.RaisePropertyChanged(nameof(MightIncreaseCost));
        this.RaisePropertyChanged(nameof(MightDecreaseCost));
        this.RaisePropertyChanged(nameof(CanIncreaseMight));
        this.RaisePropertyChanged(nameof(CanDecreaseMight));

        this.RaisePropertyChanged(nameof(FinesseIncreaseCost));
        this.RaisePropertyChanged(nameof(FinesseDecreaseCost));
        this.RaisePropertyChanged(nameof(CanIncreaseFinesse));
        this.RaisePropertyChanged(nameof(CanDecreaseFinesse));

        this.RaisePropertyChanged(nameof(WitsIncreaseCost));
        this.RaisePropertyChanged(nameof(WitsDecreaseCost));
        this.RaisePropertyChanged(nameof(CanIncreaseWits));
        this.RaisePropertyChanged(nameof(CanDecreaseWits));

        this.RaisePropertyChanged(nameof(WillIncreaseCost));
        this.RaisePropertyChanged(nameof(WillDecreaseCost));
        this.RaisePropertyChanged(nameof(CanIncreaseWill));
        this.RaisePropertyChanged(nameof(CanDecreaseWill));

        this.RaisePropertyChanged(nameof(SturdinessIncreaseCost));
        this.RaisePropertyChanged(nameof(SturdinessDecreaseCost));
        this.RaisePropertyChanged(nameof(CanIncreaseSturdiness));
        this.RaisePropertyChanged(nameof(CanDecreaseSturdiness));
    }

    #endregion

    #region Summary Properties

    public string SummaryLineage
    {
        get => _summaryLineage;
        set => this.RaiseAndSetIfChanged(ref _summaryLineage, value);
    }

    public string SummaryBackground
    {
        get => _summaryBackground;
        set => this.RaiseAndSetIfChanged(ref _summaryBackground, value);
    }

    public string SummaryArchetype
    {
        get => _summaryArchetype;
        set => this.RaiseAndSetIfChanged(ref _summaryArchetype, value);
    }

    public string SummarySpecialization
    {
        get => _summarySpecialization;
        set => this.RaiseAndSetIfChanged(ref _summarySpecialization, value);
    }

    public string SummaryAttributes
    {
        get => _summaryAttributes;
        set => this.RaiseAndSetIfChanged(ref _summaryAttributes, value);
    }

    #endregion

    #region Collections

    /// <summary>Available lineages for selection.</summary>
    public ObservableCollection<LineageInfo> AvailableLineages { get; } = new();

    /// <summary>Available backgrounds for selection.</summary>
    public ObservableCollection<BackgroundInfo> AvailableBackgrounds { get; } = new();

    /// <summary>Available archetypes for selection.</summary>
    public ObservableCollection<CharacterClass> AvailableArchetypes { get; } = new();

    /// <summary>Available specializations for selection.</summary>
    public ObservableCollection<SpecializationInfo> AvailableSpecializations { get; } = new();

    /// <summary>Validation errors to display.</summary>
    public List<string> ValidationErrors
    {
        get => _validationErrors;
        set => this.RaiseAndSetIfChanged(ref _validationErrors, value);
    }

    #endregion

    #region Commands

    /// <summary>Command to go back to previous step or main menu.</summary>
    public ICommand BackCommand { get; }

    /// <summary>Command to select a lineage.</summary>
    public ICommand SelectLineageCommand { get; }

    /// <summary>Command to select a background.</summary>
    public ICommand SelectBackgroundCommand { get; }

    /// <summary>Command to toggle advanced attribute mode.</summary>
    public ICommand ToggleAdvancedModeCommand { get; }

    /// <summary>Command to adjust an attribute value.</summary>
    public ICommand AdjustAttributeCommand { get; }

    /// <summary>Commands to increase individual attributes.</summary>
    public ICommand IncreaseMightCommand { get; }
    public ICommand DecreaseMightCommand { get; }
    public ICommand IncreaseFinesseCommand { get; }
    public ICommand DecreaseFinesseCommand { get; }
    public ICommand IncreaseWitsCommand { get; }
    public ICommand DecreaseWitsCommand { get; }
    public ICommand IncreaseWillCommand { get; }
    public ICommand DecreaseWillCommand { get; }
    public ICommand IncreaseSturdinessCommand { get; }
    public ICommand DecreaseSturdinessCommand { get; }

    /// <summary>Command to confirm attributes and proceed.</summary>
    public ICommand ConfirmAttributesCommand { get; }

    /// <summary>Command to select an archetype.</summary>
    public ICommand SelectArchetypeCommand { get; }

    /// <summary>Command to select a specialization.</summary>
    public ICommand SelectSpecializationCommand { get; }

    /// <summary>Command to confirm and create the character.</summary>
    public ICommand ConfirmCharacterCommand { get; }

    /// <summary>Command to cancel character creation.</summary>
    public ICommand CancelCommand { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Design-time constructor.
    /// </summary>
    public CharacterCreationViewModel()
    {
        BackCommand = ReactiveCommand.CreateFromTask(OnBackAsync);
        SelectLineageCommand = ReactiveCommand.CreateFromTask<Lineage>(OnSelectLineageAsync);
        SelectBackgroundCommand = ReactiveCommand.CreateFromTask<Background>(OnSelectBackgroundAsync);
        ToggleAdvancedModeCommand = ReactiveCommand.Create<bool>(OnToggleAdvancedMode);
        AdjustAttributeCommand = ReactiveCommand.Create<(string, int)>(OnAdjustAttribute);

        // Individual attribute adjustment commands
        IncreaseMightCommand = ReactiveCommand.Create(() => OnAdjustAttribute(("MIGHT", 1)));
        DecreaseMightCommand = ReactiveCommand.Create(() => OnAdjustAttribute(("MIGHT", -1)));
        IncreaseFinesseCommand = ReactiveCommand.Create(() => OnAdjustAttribute(("FINESSE", 1)));
        DecreaseFinesseCommand = ReactiveCommand.Create(() => OnAdjustAttribute(("FINESSE", -1)));
        IncreaseWitsCommand = ReactiveCommand.Create(() => OnAdjustAttribute(("WITS", 1)));
        DecreaseWitsCommand = ReactiveCommand.Create(() => OnAdjustAttribute(("WITS", -1)));
        IncreaseWillCommand = ReactiveCommand.Create(() => OnAdjustAttribute(("WILL", 1)));
        DecreaseWillCommand = ReactiveCommand.Create(() => OnAdjustAttribute(("WILL", -1)));
        IncreaseSturdinessCommand = ReactiveCommand.Create(() => OnAdjustAttribute(("STURDINESS", 1)));
        DecreaseSturdinessCommand = ReactiveCommand.Create(() => OnAdjustAttribute(("STURDINESS", -1)));

        ConfirmAttributesCommand = ReactiveCommand.CreateFromTask(OnConfirmAttributesAsync);
        SelectArchetypeCommand = ReactiveCommand.CreateFromTask<CharacterClass>(OnSelectArchetypeAsync);
        SelectSpecializationCommand = ReactiveCommand.CreateFromTask<Specialization>(OnSelectSpecializationAsync);
        ConfirmCharacterCommand = ReactiveCommand.CreateFromTask(OnConfirmCharacterAsync);
        CancelCommand = ReactiveCommand.CreateFromTask(OnCancelAsync);

        LoadDesignTimeData();
    }

    /// <summary>
    /// v0.44.2: Primary constructor with CharacterCreationController.
    /// </summary>
    [ActivatorUtilitiesConstructor]
    public CharacterCreationViewModel(
        INavigationService navigationService,
        CharacterCreationController controller,
        GameStateController gameStateController,
        ILogger logger) : this()
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _gameStateController = gameStateController ?? throw new ArgumentNullException(nameof(gameStateController));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Initialize the controller with this ViewModel
        _controller.Initialize(this);
    }

    #endregion

    #region Command Handlers

    private async Task OnBackAsync()
    {
        if (_controller != null && CurrentStep == CharacterCreationStep.Lineage)
        {
            // At first step, cancel and go to menu
            await _controller.OnCancelAsync();
        }
        else if (_navigationService != null && CurrentStep == CharacterCreationStep.Lineage)
        {
            // Legacy: go back to menu
            _navigationService.NavigateBack();
        }
        else
        {
            // Go to previous step
            CurrentStep = CurrentStep switch
            {
                CharacterCreationStep.Background => CharacterCreationStep.Lineage,
                CharacterCreationStep.Attributes => CharacterCreationStep.Background,
                CharacterCreationStep.Archetype => CharacterCreationStep.Attributes,
                CharacterCreationStep.Specialization => CharacterCreationStep.Archetype,
                CharacterCreationStep.Summary => CharacterCreationStep.Specialization,
                _ => CurrentStep
            };
        }
    }

    private async Task OnSelectLineageAsync(Lineage lineage)
    {
        if (_controller != null)
        {
            await _controller.OnLineageSelectedAsync(lineage.ToString());
        }
        else
        {
            SelectedLineage = lineage;
            CurrentStep = CharacterCreationStep.Background;
        }
    }

    private async Task OnSelectBackgroundAsync(Background background)
    {
        if (_controller != null)
        {
            await _controller.OnBackgroundSelectedAsync(background.ToString());
        }
        else
        {
            SelectedBackground = background;
            CurrentStep = CharacterCreationStep.Attributes;
        }
    }

    private void OnToggleAdvancedMode(bool useAdvanced)
    {
        if (_controller != null)
        {
            _controller.OnAttributeAllocationModeChanged(useAdvanced);
        }
        else
        {
            UseAdvancedMode = useAdvanced;
        }
    }

    private void OnAdjustAttribute((string attributeName, int delta) args)
    {
        if (!UseAdvancedMode) return;

        int currentValue = args.attributeName.ToUpperInvariant() switch
        {
            "MIGHT" => AttributeMight,
            "FINESSE" => AttributeFinesse,
            "WITS" => AttributeWits,
            "WILL" => AttributeWill,
            "STURDINESS" => AttributeSturdiness,
            _ => 5
        };

        // Get the base value for this attribute (can't go below base)
        int baseValue = args.attributeName.ToUpperInvariant() switch
        {
            "MIGHT" => BaseMight,
            "FINESSE" => BaseFinesse,
            "WITS" => BaseWits,
            "WILL" => BaseWill,
            "STURDINESS" => BaseSturdiness,
            _ => DefaultBaseAttributeValue
        };

        int targetValue = currentValue + args.delta;

        // Enforce base/max bounds (can't go below base value)
        if (targetValue < baseValue || targetValue > MaxAttributeValue)
        {
            return;
        }

        // Check if we can afford the increase
        if (args.delta > 0)
        {
            int cost = targetValue <= 8 ? 1 : 2;
            if (cost > RemainingAttributePoints) return;
        }

        if (_controller != null)
        {
            _controller.OnAttributeChanged(args.attributeName, targetValue);
        }
        else
        {
            // Design-time/fallback: Update attribute directly
            switch (args.attributeName.ToUpperInvariant())
            {
                case "MIGHT": AttributeMight = targetValue; break;
                case "FINESSE": AttributeFinesse = targetValue; break;
                case "WITS": AttributeWits = targetValue; break;
                case "WILL": AttributeWill = targetValue; break;
                case "STURDINESS": AttributeSturdiness = targetValue; break;
            }

            // Recalculate remaining points (design-time)
            RecalculateRemainingPoints();
        }
    }

    /// <summary>
    /// Recalculates remaining attribute points based on current attribute values.
    /// Uses per-attribute base values influenced by lineage/background.
    /// Used for design-time/fallback when controller is not available.
    /// </summary>
    private void RecalculateRemainingPoints()
    {
        int CalculateCost(int baseValue, int currentValue)
        {
            if (currentValue <= baseValue) return 0;
            int cost = 0;
            for (int v = baseValue + 1; v <= currentValue; v++)
            {
                cost += (v <= 8) ? 1 : 2;
            }
            return cost;
        }

        int totalUsed = CalculateCost(BaseMight, AttributeMight)
                      + CalculateCost(BaseFinesse, AttributeFinesse)
                      + CalculateCost(BaseWits, AttributeWits)
                      + CalculateCost(BaseWill, AttributeWill)
                      + CalculateCost(BaseSturdiness, AttributeSturdiness);

        RemainingAttributePoints = 15 - totalUsed;
    }

    private async Task OnConfirmAttributesAsync()
    {
        if (_controller != null)
        {
            await _controller.OnAttributesConfirmedAsync();
        }
        else
        {
            CurrentStep = CharacterCreationStep.Archetype;
        }
    }

    private async Task OnSelectArchetypeAsync(CharacterClass archetype)
    {
        if (_controller != null)
        {
            await _controller.OnArchetypeSelectedAsync(archetype.ToString());
        }
        else
        {
            SelectedArchetype = archetype;
            CurrentStep = CharacterCreationStep.Specialization;
        }
    }

    private async Task OnSelectSpecializationAsync(Specialization specialization)
    {
        if (_controller != null)
        {
            await _controller.OnSpecializationSelectedAsync(specialization.ToString());
        }
        else
        {
            SelectedSpecialization = specialization;
            CurrentStep = CharacterCreationStep.Summary;
        }
    }

    private async Task OnConfirmCharacterAsync()
    {
        if (!CanConfirm) return;

        if (_controller != null)
        {
            var success = await _controller.OnConfirmCharacterAsync(CharacterName);
            if (!success)
            {
                _logger?.Warning("Character confirmation failed");
            }
        }
        else
        {
            // Legacy path
            _logger?.Information("Character created (legacy path): {Name}", CharacterName);
            _navigationService?.NavigateBack();
        }
    }

    private async Task OnCancelAsync()
    {
        if (_controller != null)
        {
            await _controller.OnCancelAsync();
        }
        else
        {
            _navigationService?.NavigateBack();
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Shows validation errors to the user.
    /// </summary>
    public void ShowValidationErrors(List<string> errors)
    {
        ValidationErrors = errors;
        _logger?.Warning("Validation errors: {Errors}", string.Join(", ", errors));
    }

    private void LoadDesignTimeData()
    {
        // Load sample data for design-time preview
        AvailableLineages.Add(new LineageInfo
        {
            Lineage = Lineage.ClanBorn,
            Name = "Clan-Born",
            Description = "Raised in the surviving settlements with strong community ties."
        });
        AvailableLineages.Add(new LineageInfo
        {
            Lineage = Lineage.RuneMarked,
            Name = "Rune-Marked",
            Description = "Born with runic inscriptions, touched by ancient magic."
        });

        AvailableBackgrounds.Add(new BackgroundInfo
        {
            Background = Background.VillageBlacksmith,
            Name = "Village Blacksmith",
            Description = "Skilled in metalwork and repair.",
            PrimaryAttribute = "MIGHT"
        });

        AvailableArchetypes.Add(CharacterClass.Warrior);
        AvailableArchetypes.Add(CharacterClass.Mystic);
        AvailableArchetypes.Add(CharacterClass.Adept);

        AvailableSpecializations.Add(new SpecializationInfo
        {
            Specialization = Specialization.SkarHordeAspirant,
            Name = "Skar-Horde Aspirant",
            Description = "Savage berserker fueled by Savagery."
        });
    }

    #endregion
}

#region Info Classes

/// <summary>
/// Display information for a lineage option.
/// </summary>
public class LineageInfo : ReactiveObject
{
    public Lineage Lineage { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Display information for a background option.
/// </summary>
public class BackgroundInfo : ReactiveObject
{
    public Background Background { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PrimaryAttribute { get; set; } = string.Empty;

    public string AttributeColor => PrimaryAttribute switch
    {
        "MIGHT" => "#DC143C",
        "FINESSE" => "#4CAF50",
        "WITS" => "#4A90E2",
        "WILL" => "#9400D3",
        "STURDINESS" => "#FFA500",
        _ => "#CCCCCC"
    };
}

/// <summary>
/// Display information for a specialization option.
/// </summary>
public class SpecializationInfo : ReactiveObject
{
    public Specialization Specialization { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Display information for a character class option.
/// </summary>
public class CharacterClassInfo : ReactiveObject
{
    public CharacterClass Class { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string PrimaryAttribute { get; set; } = string.Empty;
    public int HP { get; set; }
    public int Stamina { get; set; }
    public string IconText { get; set; } = string.Empty;

    public string AttributeColor => PrimaryAttribute switch
    {
        "MIGHT" => "#DC143C",
        "FINESSE" => "#4CAF50",
        "WITS" => "#4A90E2",
        "WILL" => "#9400D3",
        "STURDINESS" => "#FFA500",
        _ => "#CCCCCC"
    };
}

#endregion

#region Converters

/// <summary>
/// Converter for step visibility - shows panel only when current step matches.
/// </summary>
public class StepVisibilityConverter : Avalonia.Data.Converters.IValueConverter
{
    public static readonly StepVisibilityConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is CharacterCreationStep currentStep && parameter is string stepName)
        {
            if (Enum.TryParse<CharacterCreationStep>(stepName, true, out var targetStep))
            {
                return currentStep == targetStep;
            }
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter for bool to green/gray color (for refund/decrease indicators).
/// </summary>
public class BoolToGreenGrayConverter : Avalonia.Data.Converters.IValueConverter
{
    public static readonly BoolToGreenGrayConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is bool canDecrease && canDecrease)
        {
            return Avalonia.Media.Brush.Parse("#4CAF50"); // Green - points will be refunded
        }
        return Avalonia.Media.Brush.Parse("#555555"); // Gray - at minimum
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter for bool to red/gray color (for cost/increase indicators).
/// </summary>
public class BoolToRedGrayConverter : Avalonia.Data.Converters.IValueConverter
{
    public static readonly BoolToRedGrayConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is bool canIncrease && canIncrease)
        {
            return Avalonia.Media.Brush.Parse("#FF6B6B"); // Red - will cost points
        }
        return Avalonia.Media.Brush.Parse("#555555"); // Gray - at max or no points
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter for step indicator colors - highlights current/completed steps.
/// </summary>
public class StepToColorConverter : Avalonia.Data.Converters.IValueConverter
{
    public static readonly StepToColorConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is CharacterCreationStep currentStep && parameter is string stepName)
        {
            if (Enum.TryParse<CharacterCreationStep>(stepName, true, out var targetStep))
            {
                if (currentStep == targetStep)
                {
                    // Current step - bright blue
                    return Avalonia.Media.Brush.Parse("#4A90E2");
                }
                else if ((int)currentStep > (int)targetStep)
                {
                    // Completed step - green
                    return Avalonia.Media.Brush.Parse("#4CAF50");
                }
                else
                {
                    // Future step - dark gray
                    return Avalonia.Media.Brush.Parse("#444444");
                }
            }
        }
        return Avalonia.Media.Brush.Parse("#444444");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

#endregion
