namespace RuneAndRust.Presentation.Gui.ViewModels.Wizard;

using CommunityToolkit.Mvvm.ComponentModel;
using RuneAndRust.Presentation.Gui.Models;

/// <summary>
/// Base class for character creation wizard steps.
/// </summary>
public abstract partial class WizardStepViewModelBase : ViewModelBase
{
    /// <summary>
    /// Gets the character creation data being built.
    /// </summary>
    protected CharacterCreationData Data { get; }

    /// <summary>
    /// Gets the step title.
    /// </summary>
    public abstract string StepTitle { get; }

    /// <summary>
    /// Gets or sets whether this step is valid.
    /// </summary>
    [ObservableProperty]
    private bool _isValid;

    /// <summary>
    /// Gets or sets the validation message.
    /// </summary>
    [ObservableProperty]
    private string? _validationMessage;

    /// <summary>
    /// Creates a new wizard step ViewModel.
    /// </summary>
    /// <param name="data">The character creation data.</param>
    protected WizardStepViewModelBase(CharacterCreationData data)
    {
        Data = data;
    }

    /// <summary>
    /// Validates this step.
    /// </summary>
    /// <returns>True if valid, false otherwise.</returns>
    public abstract bool Validate();
}
