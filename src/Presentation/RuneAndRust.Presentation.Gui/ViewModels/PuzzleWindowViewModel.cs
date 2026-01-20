namespace RuneAndRust.Presentation.Gui.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RuneAndRust.Presentation.Gui.Models;
using Serilog;
using System.Collections.ObjectModel;

/// <summary>
/// ViewModel for the Puzzle Window.
/// </summary>
public partial class PuzzleWindowViewModel : ViewModelBase
{
    private readonly Action? _closeAction;
    private Puzzle _puzzle = null!;
    private PuzzleElementViewModel? _firstMatchSelection;

    /// <summary>Gets the puzzle title.</summary>
    [ObservableProperty] private string _title = "Puzzle";

    /// <summary>Gets the puzzle instructions.</summary>
    [ObservableProperty] private string _instructions = "";

    /// <summary>Gets the puzzle type.</summary>
    [ObservableProperty] private PuzzleType _puzzleType;

    /// <summary>Gets the current input display.</summary>
    [ObservableProperty] private string _currentInput = "";

    /// <summary>Gets remaining attempts.</summary>
    [ObservableProperty] private int _attemptsRemaining = 3;

    /// <summary>Gets remaining hints.</summary>
    [ObservableProperty] private int _hintsRemaining = 0;

    /// <summary>Gets the current hint.</summary>
    [ObservableProperty] private string? _currentHint;

    /// <summary>Whether the puzzle is solved.</summary>
    [ObservableProperty] private bool _isSolved;

    /// <summary>Whether the puzzle has failed.</summary>
    [ObservableProperty] private bool _hasFailed;

    /// <summary>Result message.</summary>
    [ObservableProperty] private string? _resultMessage;

    /// <summary>Puzzle elements.</summary>
    public ObservableCollection<PuzzleElementViewModel> Elements { get; } = [];

    /// <summary>Whether hints can be used.</summary>
    public bool CanUseHint => HintsRemaining > 0 && !IsSolved && !HasFailed;

    /// <summary>Whether to show result overlay.</summary>
    public bool ShowResult => IsSolved || HasFailed;

    /// <summary>Attempts text.</summary>
    public string AttemptsText => $"Attempts: {AttemptsRemaining}";

    /// <summary>Hint button text.</summary>
    public string HintButtonText => $"Hint ({HintsRemaining})";

    /// <summary>Creates a puzzle window ViewModel.</summary>
    public PuzzleWindowViewModel(Action? closeAction = null)
    {
        _closeAction = closeAction;
        LoadSamplePuzzle();
    }

    /// <summary>Loads a puzzle.</summary>
    public void LoadPuzzle(Puzzle puzzle)
    {
        _puzzle = puzzle;
        Title = puzzle.Name;
        Instructions = puzzle.Instructions;
        PuzzleType = puzzle.Type;
        AttemptsRemaining = puzzle.MaxAttempts;
        HintsRemaining = puzzle.Hints.Count;
        IsSolved = false;
        HasFailed = false;
        ResultMessage = null;
        CurrentHint = null;
        CurrentInput = "";

        Elements.Clear();
        foreach (var el in puzzle.Elements)
            Elements.Add(new PuzzleElementViewModel(el.Id, el.Symbol));

        Log.Information("Loaded puzzle: {Title} ({Type})", Title, PuzzleType);
    }

    /// <summary>Toggles an element (for lever puzzles).</summary>
    [RelayCommand]
    private void ToggleElement(PuzzleElementViewModel element)
    {
        if (IsSolved || HasFailed) return;

        if (PuzzleType == PuzzleType.Lever)
        {
            element.IsToggled = !element.IsToggled;
            UpdateCurrentInput();
            Log.Debug("Toggled element {Id}: {State}", element.Id, element.IsToggled);
        }
    }

    /// <summary>Selects an element (for sequence/matching puzzles).</summary>
    [RelayCommand]
    private async Task SelectElement(PuzzleElementViewModel element)
    {
        if (IsSolved || HasFailed) return;

        if (PuzzleType == PuzzleType.Sequence && !element.IsSelected)
        {
            element.SelectionOrder = GetNextSelectionOrder();
            UpdateCurrentInput();
            Log.Debug("Selected element {Id} as #{Order}", element.Id, element.SelectionOrder);
        }
        else if (PuzzleType == PuzzleType.Matching && !element.IsMatched)
        {
            await HandleMatchingSelection(element);
        }
    }

    /// <summary>Attempts to solve the puzzle.</summary>
    [RelayCommand]
    private void Solve()
    {
        if (IsSolved || HasFailed) return;

        var solution = GetCurrentSolution();
        if (solution == _puzzle.Solution)
        {
            IsSolved = true;
            ResultMessage = "🎉 PUZZLE SOLVED! 🎉\n\n" + _puzzle.SuccessMessage;
            Log.Information("Puzzle solved: {Title}", Title);
        }
        else
        {
            AttemptsRemaining--;
            OnPropertyChanged(nameof(AttemptsText));

            if (AttemptsRemaining <= 0)
            {
                HasFailed = true;
                ResultMessage = "❌ PUZZLE FAILED ❌\n\n" + _puzzle.FailureMessage;
                Log.Information("Puzzle failed: {Title}", Title);
            }
            else
            {
                ResultMessage = $"Incorrect. {AttemptsRemaining} attempts remaining.";
                Log.Debug("Wrong solution. Attempts left: {Attempts}", AttemptsRemaining);
            }
        }

        OnPropertyChanged(nameof(ShowResult));
        OnPropertyChanged(nameof(CanUseHint));
    }

    /// <summary>Uses a hint.</summary>
    [RelayCommand]
    private void UseHint()
    {
        if (!CanUseHint) return;

        var hintIndex = _puzzle.Hints.Count - HintsRemaining;
        CurrentHint = _puzzle.Hints[hintIndex];
        HintsRemaining--;
        OnPropertyChanged(nameof(HintButtonText));
        OnPropertyChanged(nameof(CanUseHint));
        Log.Debug("Used hint: {Hint}", CurrentHint);
    }

    /// <summary>Resets the puzzle.</summary>
    [RelayCommand]
    private void Reset()
    {
        foreach (var el in Elements)
            el.Reset();

        CurrentInput = "";
        CurrentHint = null;
        ResultMessage = null;
        _firstMatchSelection = null;
        Log.Debug("Puzzle reset");
    }

    /// <summary>Gives up on the puzzle.</summary>
    [RelayCommand]
    private void GiveUp()
    {
        HasFailed = true;
        ResultMessage = "You gave up.\n\n" + _puzzle.FailureMessage;
        OnPropertyChanged(nameof(ShowResult));
        Log.Debug("Gave up on puzzle");
    }

    /// <summary>Closes the puzzle window.</summary>
    [RelayCommand]
    private void Close() => _closeAction?.Invoke();

    private int GetNextSelectionOrder() => Elements.Count(e => e.IsSelected) + 1;

    private void UpdateCurrentInput()
    {
        CurrentInput = PuzzleType switch
        {
            PuzzleType.Lever => string.Join(" ", Elements.Select(e => e.IsToggled ? "↓" : "↑")),
            PuzzleType.Sequence => string.Join(" → ", Elements.Where(e => e.IsSelected)
                .OrderBy(e => e.SelectionOrder).Select(e => e.Symbol)),
            _ => ""
        };
    }

    private string GetCurrentSolution() => PuzzleType switch
    {
        PuzzleType.Lever => string.Join("", Elements.Select(e => e.IsToggled ? "1" : "0")),
        PuzzleType.Sequence => string.Join(",", Elements.Where(e => e.IsSelected)
            .OrderBy(e => e.SelectionOrder).Select(e => e.Id)),
        PuzzleType.Matching => Elements.All(e => e.IsMatched) ? "complete" : "incomplete",
        _ => ""
    };

    private async Task HandleMatchingSelection(PuzzleElementViewModel element)
    {
        element.IsRevealed = true;

        if (_firstMatchSelection is null)
        {
            _firstMatchSelection = element;
        }
        else
        {
            if (_firstMatchSelection.Symbol == element.Symbol && _firstMatchSelection != element)
            {
                _firstMatchSelection.IsMatched = true;
                element.IsMatched = true;
                Log.Debug("Matched pair: {Symbol}", element.Symbol);
            }
            else
            {
                var first = _firstMatchSelection;
                await Task.Delay(500);
                first.IsRevealed = false;
                element.IsRevealed = false;
            }
            _firstMatchSelection = null;
        }
    }

    private void LoadSamplePuzzle()
    {
        var puzzle = new Puzzle(
            Name: "The Three Levers",
            Instructions: "Set the levers to the correct pattern to unlock the door.",
            Type: PuzzleType.Lever,
            Elements: [
                new PuzzleElement("lever1", "🔴"),
                new PuzzleElement("lever2", "🟢"),
                new PuzzleElement("lever3", "🔵")
            ],
            Solution: "101",
            MaxAttempts: 3,
            Hints: ["The first lever should be down.", "The middle lever stays up.", "Red and blue are down."],
            SuccessMessage: "The ancient door creaks open, revealing a hidden passage!",
            FailureMessage: "The mechanism locks. You'll need to find another way."
        );

        LoadPuzzle(puzzle);
    }
}
