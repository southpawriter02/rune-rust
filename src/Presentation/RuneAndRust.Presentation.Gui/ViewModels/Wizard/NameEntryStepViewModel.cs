namespace RuneAndRust.Presentation.Gui.ViewModels.Wizard;

using CommunityToolkit.Mvvm.ComponentModel;
using RuneAndRust.Presentation.Gui.Models;

/// <summary>
/// ViewModel for the name entry step.
/// </summary>
public partial class NameEntryStepViewModel : WizardStepViewModelBase
{
    /// <inheritdoc />
    public override string StepTitle => "Choose Your Name";

    /// <summary>
    /// Gets or sets the character name.
    /// </summary>
    [ObservableProperty]
    private string _characterName = string.Empty;

    /// <summary>
    /// Creates a new name entry step ViewModel.
    /// </summary>
    /// <param name="data">The character creation data.</param>
    public NameEntryStepViewModel(CharacterCreationData data) : base(data) { }

    /// <inheritdoc />
    public override bool Validate()
    {
        if (string.IsNullOrWhiteSpace(CharacterName) || CharacterName.Length < 2)
        {
            ValidationMessage = "Name must be at least 2 characters";
            return IsValid = false;
        }

        if (CharacterName.Length > 20)
        {
            ValidationMessage = "Name must be 20 characters or less";
            return IsValid = false;
        }

        Data.Name = CharacterName.Trim();
        ValidationMessage = null;
        return IsValid = true;
    }

    partial void OnCharacterNameChanged(string value) => Validate();
}
