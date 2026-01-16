namespace RuneAndRust.Presentation.Gui.Models;

/// <summary>
/// Types of interactive puzzles.
/// </summary>
public enum PuzzleType
{
    /// <summary>Toggle levers up/down.</summary>
    Lever,
    /// <summary>Select elements in order.</summary>
    Sequence,
    /// <summary>Match pairs of tiles.</summary>
    Matching
}

/// <summary>
/// A puzzle element.
/// </summary>
/// <param name="Id">Element identifier.</param>
/// <param name="Symbol">Display symbol.</param>
public record PuzzleElement(string Id, string Symbol);

/// <summary>
/// A puzzle definition.
/// </summary>
/// <param name="Name">Puzzle name.</param>
/// <param name="Instructions">Player instructions.</param>
/// <param name="Type">Puzzle type.</param>
/// <param name="Elements">Interactive elements.</param>
/// <param name="Solution">Expected solution.</param>
/// <param name="MaxAttempts">Maximum attempts allowed.</param>
/// <param name="Hints">Available hints.</param>
/// <param name="SuccessMessage">Message on success.</param>
/// <param name="FailureMessage">Message on failure.</param>
public record Puzzle(
    string Name,
    string Instructions,
    PuzzleType Type,
    IReadOnlyList<PuzzleElement> Elements,
    string Solution,
    int MaxAttempts = 3,
    IReadOnlyList<string>? Hints = null,
    string SuccessMessage = "Well done!",
    string FailureMessage = "The puzzle remains unsolved.")
{
    /// <summary>Gets the hints list.</summary>
    public IReadOnlyList<string> Hints { get; } = Hints ?? [];
}
