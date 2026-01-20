namespace RuneAndRust.Presentation.Gui.ViewModels.Wizard;

using RuneAndRust.Presentation.Gui.Models;

/// <summary>
/// ViewModel for the review step.
/// </summary>
public partial class ReviewStepViewModel : WizardStepViewModelBase
{
    /// <inheritdoc />
    public override string StepTitle => "Review Character";

    /// <summary>
    /// Gets the character name.
    /// </summary>
    public string CharacterName => Data.Name;

    /// <summary>
    /// Gets the race ID.
    /// </summary>
    public string RaceId => Data.RaceId;

    /// <summary>
    /// Gets the class ID.
    /// </summary>
    public string ClassId => Data.ClassId;

    /// <summary>
    /// Gets the stats summary.
    /// </summary>
    public IReadOnlyDictionary<string, int> Stats => Data.Stats;

    /// <summary>
    /// Gets the portrait ID.
    /// </summary>
    public string PortraitId => Data.PortraitId;

    /// <summary>
    /// Creates a new review step ViewModel.
    /// </summary>
    /// <param name="data">The character creation data.</param>
    public ReviewStepViewModel(CharacterCreationData data) : base(data)
    {
        IsValid = true;
    }

    /// <inheritdoc />
    public override bool Validate() => IsValid = true;

    /// <summary>
    /// Refreshes the display data.
    /// </summary>
    public void Refresh()
    {
        OnPropertyChanged(nameof(CharacterName));
        OnPropertyChanged(nameof(RaceId));
        OnPropertyChanged(nameof(ClassId));
        OnPropertyChanged(nameof(Stats));
        OnPropertyChanged(nameof(PortraitId));
    }
}
