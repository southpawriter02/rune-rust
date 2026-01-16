namespace RuneAndRust.Presentation.Gui.ViewModels.Wizard;

using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RuneAndRust.Presentation.Gui.Models;
using Serilog;
using System.Collections.ObjectModel;

/// <summary>
/// Main ViewModel for the character creation wizard.
/// </summary>
public partial class WizardViewModel : ViewModelBase
{
    private readonly List<WizardStepViewModelBase> _steps;

    /// <summary>
    /// Gets or sets the current step number (1-based).
    /// </summary>
    [ObservableProperty]
    private int _currentStep = 1;

    /// <summary>
    /// Gets the total number of steps.
    /// </summary>
    public int TotalSteps => 6;

    /// <summary>
    /// Gets or sets the current step ViewModel.
    /// </summary>
    [ObservableProperty]
    private WizardStepViewModelBase _currentStepViewModel;

    /// <summary>
    /// Gets the character creation data.
    /// </summary>
    [ObservableProperty]
    private CharacterCreationData _creationData = new();

    /// <summary>
    /// Gets or sets whether the back button is visible.
    /// </summary>
    [ObservableProperty]
    private bool _canGoBack;

    /// <summary>
    /// Gets or sets whether this is the last step.
    /// </summary>
    [ObservableProperty]
    private bool _isLastStep;

    /// <summary>
    /// Gets the current step title.
    /// </summary>
    public string CurrentStepTitle => CurrentStepViewModel?.StepTitle ?? string.Empty;

    /// <summary>
    /// Gets the next button text.
    /// </summary>
    public string NextButtonText => IsLastStep ? "Create Character" : "Next ›";

    /// <summary>
    /// Gets the step indicators.
    /// </summary>
    public ObservableCollection<StepIndicator> StepIndicators { get; } = [];

    /// <summary>
    /// Event raised when character creation is complete.
    /// </summary>
    public event Action<CharacterCreationData>? OnCharacterCreated;

    /// <summary>
    /// Event raised when the wizard should close.
    /// </summary>
    public event Action? OnCancel;

    /// <summary>
    /// Creates a new WizardViewModel.
    /// </summary>
    public WizardViewModel()
    {
        _steps =
        [
            new NameEntryStepViewModel(CreationData),
            new RaceSelectionStepViewModel(CreationData),
            new ClassSelectionStepViewModel(CreationData),
            new StatAllocationStepViewModel(CreationData),
            new AppearanceStepViewModel(CreationData),
            new ReviewStepViewModel(CreationData)
        ];

        for (int i = 0; i < TotalSteps; i++)
        {
            StepIndicators.Add(new StepIndicator(i + 1, i == 0));
        }

        _currentStepViewModel = _steps[0];
        UpdateNavigationState();
    }

    /// <summary>
    /// Go to the previous step.
    /// </summary>
    [RelayCommand]
    private void GoBack()
    {
        if (CurrentStep > 1)
        {
            CurrentStep--;
            CurrentStepViewModel = _steps[CurrentStep - 1];
            UpdateNavigationState();
            Log.Debug("Wizard navigated back to step {Step}", CurrentStep);
        }
    }

    /// <summary>
    /// Go to the next step or create character.
    /// </summary>
    [RelayCommand]
    private void GoNext()
    {
        if (!CurrentStepViewModel.Validate())
        {
            Log.Debug("Step {Step} validation failed: {Message}",
                CurrentStep, CurrentStepViewModel.ValidationMessage);
            return;
        }

        if (IsLastStep)
        {
            CreateCharacter();
        }
        else
        {
            CurrentStep++;
            CurrentStepViewModel = _steps[CurrentStep - 1];

            // Refresh review step if entering it
            if (CurrentStepViewModel is ReviewStepViewModel reviewVm)
            {
                reviewVm.Refresh();
            }

            UpdateNavigationState();
            Log.Debug("Wizard advanced to step {Step}", CurrentStep);
        }
    }

    /// <summary>
    /// Cancel character creation.
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        Log.Information("Character creation cancelled");
        OnCancel?.Invoke();
    }

    private void CreateCharacter()
    {
        Log.Information("Character created: {Name} ({Race} {Class})",
            CreationData.Name, CreationData.RaceId, CreationData.ClassId);
        OnCharacterCreated?.Invoke(CreationData);
    }

    private void UpdateNavigationState()
    {
        CanGoBack = CurrentStep > 1;
        IsLastStep = CurrentStep == TotalSteps;

        for (int i = 0; i < StepIndicators.Count; i++)
        {
            StepIndicators[i].IsActive = i < CurrentStep;
        }

        OnPropertyChanged(nameof(CurrentStepTitle));
        OnPropertyChanged(nameof(NextButtonText));
    }
}

/// <summary>
/// Represents a step indicator dot.
/// </summary>
public partial class StepIndicator : ObservableObject
{
    /// <summary>
    /// Gets the step number.
    /// </summary>
    public int StepNumber { get; }

    /// <summary>
    /// Gets or sets whether this step is active.
    /// </summary>
    [ObservableProperty]
    private bool _isActive;

    /// <summary>
    /// Gets the fill brush.
    /// </summary>
    public IBrush Fill => IsActive ? Brushes.Gold : Brushes.Transparent;

    /// <summary>
    /// Creates a new step indicator.
    /// </summary>
    public StepIndicator(int step, bool active)
    {
        StepNumber = step;
        IsActive = active;
    }

    partial void OnIsActiveChanged(bool value)
    {
        OnPropertyChanged(nameof(Fill));
    }
}
