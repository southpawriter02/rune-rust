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
        set => this.RaiseAndSetIfChanged(ref _remainingAttributePoints, value);
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
        set => this.RaiseAndSetIfChanged(ref _attributeMight, value);
    }

    public int AttributeFinesse
    {
        get => _attributeFinesse;
        set => this.RaiseAndSetIfChanged(ref _attributeFinesse, value);
    }

    public int AttributeWits
    {
        get => _attributeWits;
        set => this.RaiseAndSetIfChanged(ref _attributeWits, value);
    }

    public int AttributeWill
    {
        get => _attributeWill;
        set => this.RaiseAndSetIfChanged(ref _attributeWill, value);
    }

    public int AttributeSturdiness
    {
        get => _attributeSturdiness;
        set => this.RaiseAndSetIfChanged(ref _attributeSturdiness, value);
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
        SelectLineageCommand = ReactiveCommand.CreateFromTask<string>(OnSelectLineageAsync);
        SelectBackgroundCommand = ReactiveCommand.CreateFromTask<string>(OnSelectBackgroundAsync);
        ToggleAdvancedModeCommand = ReactiveCommand.Create<bool>(OnToggleAdvancedMode);
        AdjustAttributeCommand = ReactiveCommand.Create<(string, int)>(OnAdjustAttribute);
        ConfirmAttributesCommand = ReactiveCommand.CreateFromTask(OnConfirmAttributesAsync);
        SelectArchetypeCommand = ReactiveCommand.CreateFromTask<string>(OnSelectArchetypeAsync);
        SelectSpecializationCommand = ReactiveCommand.CreateFromTask<string>(OnSelectSpecializationAsync);
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

    /// <summary>
    /// v0.44.1 constructor with GameStateController (backwards compatible).
    /// </summary>
    public CharacterCreationViewModel(
        INavigationService navigationService,
        GameStateController gameStateController,
        ILogger logger) : this()
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _gameStateController = gameStateController ?? throw new ArgumentNullException(nameof(gameStateController));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Legacy constructor without controller (backwards compatibility).
    /// </summary>
    public CharacterCreationViewModel(
        INavigationService navigationService,
        ISaveGameService? saveGameService = null) : this()
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
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

    private async Task OnSelectLineageAsync(string lineageId)
    {
        if (_controller != null)
        {
            await _controller.OnLineageSelectedAsync(lineageId);
        }
        else if (Enum.TryParse<Lineage>(lineageId, true, out var lineage))
        {
            SelectedLineage = lineage;
            CurrentStep = CharacterCreationStep.Background;
        }
    }

    private async Task OnSelectBackgroundAsync(string backgroundId)
    {
        if (_controller != null)
        {
            await _controller.OnBackgroundSelectedAsync(backgroundId);
        }
        else if (Enum.TryParse<Background>(backgroundId, true, out var bg))
        {
            SelectedBackground = bg;
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
        if (_controller != null && UseAdvancedMode)
        {
            int currentValue = args.attributeName.ToUpperInvariant() switch
            {
                "MIGHT" => AttributeMight,
                "FINESSE" => AttributeFinesse,
                "WITS" => AttributeWits,
                "WILL" => AttributeWill,
                "STURDINESS" => AttributeSturdiness,
                _ => 5
            };

            _controller.OnAttributeChanged(args.attributeName, currentValue + args.delta);
        }
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

    private async Task OnSelectArchetypeAsync(string archetypeId)
    {
        if (_controller != null)
        {
            await _controller.OnArchetypeSelectedAsync(archetypeId);
        }
        else if (Enum.TryParse<CharacterClass>(archetypeId, true, out var archetype))
        {
            SelectedArchetype = archetype;
            CurrentStep = CharacterCreationStep.Specialization;
        }
    }

    private async Task OnSelectSpecializationAsync(string specializationId)
    {
        if (_controller != null)
        {
            await _controller.OnSpecializationSelectedAsync(specializationId);
        }
        else if (Enum.TryParse<Specialization>(specializationId, true, out var spec))
        {
            SelectedSpecialization = spec;
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
