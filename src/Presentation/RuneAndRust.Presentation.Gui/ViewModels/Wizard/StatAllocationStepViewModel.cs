namespace RuneAndRust.Presentation.Gui.ViewModels.Wizard;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RuneAndRust.Presentation.Gui.Models;
using System.Collections.ObjectModel;

/// <summary>
/// ViewModel for the stat allocation step.
/// </summary>
public partial class StatAllocationStepViewModel : WizardStepViewModelBase
{
    private const int TotalPoints = 27;
    private static readonly int[] PointCosts = [0, 1, 2, 3, 4, 5, 7, 9]; // For values 8-15

    /// <inheritdoc />
    public override string StepTitle => "Allocate Stats";

    /// <summary>
    /// Gets or sets the remaining points.
    /// </summary>
    [ObservableProperty]
    private int _pointsRemaining = TotalPoints;

    /// <summary>
    /// Gets the stat allocations.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<StatAllocationViewModel> _stats = [];

    /// <summary>
    /// Creates a new stat allocation step ViewModel.
    /// </summary>
    /// <param name="data">The character creation data.</param>
    public StatAllocationStepViewModel(CharacterCreationData data) : base(data)
    {
        Stats.Add(new StatAllocationViewModel("Strength", 10, this));
        Stats.Add(new StatAllocationViewModel("Dexterity", 10, this));
        Stats.Add(new StatAllocationViewModel("Constitution", 10, this));
        Stats.Add(new StatAllocationViewModel("Intelligence", 10, this));
        Stats.Add(new StatAllocationViewModel("Wisdom", 10, this));
        Stats.Add(new StatAllocationViewModel("Charisma", 10, this));

        RecalculatePoints();
    }

    /// <summary>
    /// Recalculates remaining points.
    /// </summary>
    public void RecalculatePoints()
    {
        var spent = Stats.Sum(s => GetPointCost(s.BaseValue));
        PointsRemaining = TotalPoints - spent;
        Validate();
    }

    /// <inheritdoc />
    public override bool Validate()
    {
        if (PointsRemaining < 0)
        {
            ValidationMessage = "You've spent too many points";
            return IsValid = false;
        }

        foreach (var stat in Stats)
        {
            Data.Stats[stat.StatName] = stat.BaseValue;
        }

        ValidationMessage = null;
        return IsValid = true;
    }

    private static int GetPointCost(int value) =>
        value >= 8 && value <= 15 ? PointCosts[value - 8] : 0;
}

/// <summary>
/// ViewModel for a single stat allocation.
/// </summary>
public partial class StatAllocationViewModel : ObservableObject
{
    private readonly StatAllocationStepViewModel _parent;

    /// <summary>
    /// Gets the stat name.
    /// </summary>
    public string StatName { get; }

    /// <summary>
    /// Gets or sets the base value.
    /// </summary>
    [ObservableProperty]
    private int _baseValue;

    /// <summary>
    /// Gets or sets the racial bonus.
    /// </summary>
    public int RacialBonus { get; set; }

    /// <summary>
    /// Gets the total value.
    /// </summary>
    public int Total => BaseValue + RacialBonus;

    /// <summary>
    /// Gets the modifier.
    /// </summary>
    public int Modifier => (Total - 10) / 2;

    /// <summary>
    /// Gets the modifier string.
    /// </summary>
    public string ModifierText => Modifier >= 0 ? $"+{Modifier}" : Modifier.ToString();

    /// <summary>
    /// Creates a new stat allocation ViewModel.
    /// </summary>
    public StatAllocationViewModel(string name, int baseVal, StatAllocationStepViewModel parent)
    {
        StatName = name;
        _baseValue = baseVal;
        _parent = parent;
    }

    /// <summary>
    /// Increment the stat.
    /// </summary>
    [RelayCommand]
    private void Increment()
    {
        if (BaseValue < 15)
        {
            BaseValue++;
            _parent.RecalculatePoints();
            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(Modifier));
            OnPropertyChanged(nameof(ModifierText));
        }
    }

    /// <summary>
    /// Decrement the stat.
    /// </summary>
    [RelayCommand]
    private void Decrement()
    {
        if (BaseValue > 8)
        {
            BaseValue--;
            _parent.RecalculatePoints();
            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(Modifier));
            OnPropertyChanged(nameof(ModifierText));
        }
    }
}
