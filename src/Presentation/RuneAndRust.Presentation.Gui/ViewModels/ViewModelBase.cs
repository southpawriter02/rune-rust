using CommunityToolkit.Mvvm.ComponentModel;

namespace RuneAndRust.Presentation.Gui.ViewModels;

/// <summary>
/// Base class for all view models in the application.
/// </summary>
/// <remarks>
/// Inherits from ObservableObject to provide INotifyPropertyChanged support
/// via the CommunityToolkit.Mvvm source generators.
/// </remarks>
public abstract partial class ViewModelBase : ObservableObject
{
}
