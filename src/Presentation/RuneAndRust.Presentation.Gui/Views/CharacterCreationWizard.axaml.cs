namespace RuneAndRust.Presentation.Gui.Views;

using Avalonia.Controls;
using RuneAndRust.Presentation.Gui.ViewModels.Wizard;

/// <summary>
/// Character creation wizard window.
/// </summary>
public partial class CharacterCreationWizard : Window
{
    /// <summary>
    /// Initializes a new instance of the CharacterCreationWizard.
    /// </summary>
    public CharacterCreationWizard()
    {
        InitializeComponent();

        DataContext = new WizardViewModel();
    }

    /// <summary>
    /// Initializes with a custom ViewModel.
    /// </summary>
    /// <param name="viewModel">The wizard ViewModel.</param>
    public CharacterCreationWizard(WizardViewModel viewModel) : this()
    {
        DataContext = viewModel;

        viewModel.OnCancel += () => Close();
        viewModel.OnCharacterCreated += _ => Close();
    }
}
