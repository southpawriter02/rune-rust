namespace RuneAndRust.Presentation.Gui.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

/// <summary>
/// ViewModel for a puzzle element.
/// </summary>
public partial class PuzzleElementViewModel : ViewModelBase
{
    /// <summary>Gets the element ID.</summary>
    public string Id { get; }

    /// <summary>Gets the element symbol.</summary>
    public string Symbol { get; }

    /// <summary>Whether the element is toggled (for lever puzzles).</summary>
    [ObservableProperty] private bool _isToggled;

    /// <summary>Selection order (for sequence puzzles).</summary>
    [ObservableProperty] private int _selectionOrder = -1;

    /// <summary>Whether the element is revealed (for matching puzzles).</summary>
    [ObservableProperty] private bool _isRevealed;

    /// <summary>Whether the element is matched (for matching puzzles).</summary>
    [ObservableProperty] private bool _isMatched;

    /// <summary>Whether this element has been selected.</summary>
    public bool IsSelected => SelectionOrder >= 0;

    /// <summary>Gets the display symbol based on state.</summary>
    public string DisplaySymbol => IsToggled ? "↓" : "↑";

    /// <summary>Creates a puzzle element ViewModel.</summary>
    public PuzzleElementViewModel(string id, string symbol)
    {
        Id = id;
        Symbol = symbol;
    }

    /// <summary>Resets the element state.</summary>
    public void Reset()
    {
        IsToggled = false;
        SelectionOrder = -1;
        IsRevealed = false;
        IsMatched = false;
    }
}
