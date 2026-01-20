namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Configuration for a combination/code-based puzzle.
/// </summary>
/// <remarks>
/// <para>
/// Combination puzzles require entering a correct code or combination.
/// Features include:
/// </para>
/// <list type="bullet">
///   <item><description>Configurable valid characters (digits, letters, symbols)</description></item>
///   <item><description>Case-sensitive or case-insensitive matching</description></item>
///   <item><description>Optional partial feedback (correct positions/characters)</description></item>
///   <item><description>Support for alternate valid solutions</description></item>
/// </list>
/// </remarks>
public class CombinationPuzzle
{
    // ===== Core Properties =====

    /// <summary>
    /// Gets the correct combination.
    /// </summary>
    public string Solution { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the length of the combination.
    /// </summary>
    public int Length { get; private set; }

    /// <summary>
    /// Gets the valid characters/digits for input.
    /// </summary>
    public string ValidCharacters { get; private set; } = "0123456789";

    /// <summary>
    /// Gets whether input is case-sensitive.
    /// </summary>
    public bool CaseSensitive { get; private set; }

    /// <summary>
    /// Gets whether to show partial feedback (correct digits).
    /// </summary>
    public bool ShowPartialFeedback { get; private set; }

    /// <summary>
    /// Gets the separator between input segments (null = none).
    /// </summary>
    public string? Separator { get; private set; }

    /// <summary>
    /// Gets alternate accepted solutions (if any).
    /// </summary>
    public IReadOnlyList<string> AlternateSolutions { get; private set; } = Array.Empty<string>();

    // ===== Constructors =====

    /// <summary>
    /// Private parameterless constructor for EF Core.
    /// </summary>
    private CombinationPuzzle() { }

    // ===== Factory Methods =====

    /// <summary>
    /// Creates a combination puzzle configuration.
    /// </summary>
    /// <param name="solution">The correct solution.</param>
    /// <param name="validChars">Valid input characters.</param>
    /// <param name="caseSensitive">Case-sensitive matching.</param>
    /// <param name="showPartialFeedback">Show correct position hints.</param>
    /// <param name="separator">Separator for display.</param>
    /// <param name="alternateSolutions">Additional valid solutions.</param>
    /// <returns>A new CombinationPuzzle instance.</returns>
    public static CombinationPuzzle Create(
        string solution,
        string? validChars = null,
        bool caseSensitive = false,
        bool showPartialFeedback = false,
        string? separator = null,
        IEnumerable<string>? alternateSolutions = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(solution);

        return new CombinationPuzzle
        {
            Solution = solution,
            Length = solution.Length,
            ValidCharacters = validChars ?? "0123456789",
            CaseSensitive = caseSensitive,
            ShowPartialFeedback = showPartialFeedback,
            Separator = separator,
            AlternateSolutions = (IReadOnlyList<string>?)alternateSolutions?.ToList() ?? Array.Empty<string>()
        };
    }

    // ===== Validation Methods =====

    /// <summary>
    /// Validates if the input matches the solution.
    /// </summary>
    /// <param name="input">The player's input.</param>
    /// <returns>True if the input is correct.</returns>
    public bool Validate(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        var comparison = CaseSensitive
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;

        // Normalize input by removing separators
        var normalizedInput = NormalizeInput(input);

        if (Solution.Equals(normalizedInput, comparison))
            return true;

        return AlternateSolutions.Any(alt => alt.Equals(normalizedInput, comparison));
    }

    /// <summary>
    /// Validates that input contains only valid characters.
    /// </summary>
    /// <param name="input">The input to validate.</param>
    /// <returns>True if all characters are valid.</returns>
    public bool IsValidInput(string input)
    {
        if (string.IsNullOrEmpty(input))
            return true;

        var normalizedInput = NormalizeInput(input);
        var validSet = CaseSensitive
            ? ValidCharacters.ToHashSet()
            : ValidCharacters.ToLowerInvariant().ToHashSet();

        var compareInput = CaseSensitive ? normalizedInput : normalizedInput.ToLowerInvariant();

        return compareInput.All(c => validSet.Contains(c));
    }

    // ===== Feedback Methods =====

    /// <summary>
    /// Gets partial feedback showing correct positions.
    /// </summary>
    /// <param name="input">The player's input.</param>
    /// <returns>Feedback about correct positions and characters.</returns>
    public CombinationFeedback GetFeedback(string input)
    {
        if (!ShowPartialFeedback || string.IsNullOrEmpty(input))
            return new CombinationFeedback { CorrectPositions = 0, CorrectCharacters = 0 };

        var normalizedInput = NormalizeInput(input);
        var normalizedSolution = CaseSensitive ? Solution : Solution.ToLowerInvariant();
        var compareInput = CaseSensitive ? normalizedInput : normalizedInput.ToLowerInvariant();

        var correctPositions = 0;
        var solutionCharCounts = new Dictionary<char, int>();
        var inputCharCounts = new Dictionary<char, int>();

        // Count exact position matches and character frequencies
        for (int i = 0; i < Math.Min(compareInput.Length, normalizedSolution.Length); i++)
        {
            if (compareInput[i] == normalizedSolution[i])
            {
                correctPositions++;
            }
        }

        // Count character frequencies in solution
        foreach (var c in normalizedSolution)
        {
            solutionCharCounts.TryGetValue(c, out var count);
            solutionCharCounts[c] = count + 1;
        }

        // Count character frequencies in input
        foreach (var c in compareInput)
        {
            inputCharCounts.TryGetValue(c, out var count);
            inputCharCounts[c] = count + 1;
        }

        // Calculate correct characters (in wrong positions)
        var totalCorrectChars = 0;
        foreach (var kvp in inputCharCounts)
        {
            if (solutionCharCounts.TryGetValue(kvp.Key, out var solutionCount))
            {
                totalCorrectChars += Math.Min(kvp.Value, solutionCount);
            }
        }

        // Correct characters in wrong position = total correct - exact matches
        var correctCharacters = totalCorrectChars - correctPositions;

        return new CombinationFeedback
        {
            CorrectPositions = correctPositions,
            CorrectCharacters = Math.Max(0, correctCharacters)
        };
    }

    // ===== Helper Methods =====

    /// <summary>
    /// Formats input with separators for display.
    /// </summary>
    /// <param name="input">The raw input.</param>
    /// <returns>Formatted input string.</returns>
    public string FormatForDisplay(string input)
    {
        if (string.IsNullOrEmpty(Separator) || string.IsNullOrEmpty(input))
            return input ?? string.Empty;

        return string.Join(Separator, input.ToCharArray());
    }

    private string NormalizeInput(string input)
    {
        if (string.IsNullOrEmpty(Separator) || string.IsNullOrEmpty(input))
            return input ?? string.Empty;

        return input.Replace(Separator, "", StringComparison.Ordinal);
    }

    /// <summary>
    /// Returns a string representation of this combination puzzle.
    /// </summary>
    public override string ToString() =>
        $"CombinationPuzzle(Length={Length}, CaseSensitive={CaseSensitive}, PartialFeedback={ShowPartialFeedback})";
}

/// <summary>
/// Feedback for a combination attempt.
/// </summary>
/// <remarks>
/// Provides information about how close an attempt was to the correct solution,
/// similar to Mastermind-style feedback.
/// </remarks>
public readonly record struct CombinationFeedback
{
    /// <summary>
    /// Gets the number of characters in the correct position.
    /// </summary>
    public int CorrectPositions { get; init; }

    /// <summary>
    /// Gets the number of correct characters in wrong positions.
    /// </summary>
    public int CorrectCharacters { get; init; }

    /// <summary>
    /// Gets whether any characters were correct.
    /// </summary>
    public bool HasCorrectChars => CorrectPositions > 0 || CorrectCharacters > 0;

    /// <summary>
    /// Gets the total number of correct characters (any position).
    /// </summary>
    public int TotalCorrect => CorrectPositions + CorrectCharacters;
}
