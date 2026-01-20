using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Configuration for a riddle with question and acceptable answers.
/// </summary>
/// <remarks>
/// <para>
/// RiddleDefinition provides the configuration for riddles posed by NPCs.
/// Features include:
/// </para>
/// <list type="bullet">
///   <item><description>Case-insensitive answer matching</description></item>
///   <item><description>Multiple accepted answers</description></item>
///   <item><description>Optional hints with reveal costs</description></item>
///   <item><description>Custom correct/wrong messages</description></item>
/// </list>
/// </remarks>
public class RiddleDefinition
{
    // ===== Core Properties =====

    /// <summary>
    /// Gets the unique identifier for this riddle.
    /// </summary>
    public string Id { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the riddle question text.
    /// </summary>
    public string Question { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the accepted answers (case-insensitive).
    /// </summary>
    public IReadOnlyList<string> AcceptedAnswers { get; private set; } = Array.Empty<string>();

    /// <summary>
    /// Gets optional hints for this riddle.
    /// </summary>
    public IReadOnlyList<PuzzleHint> Hints { get; private set; } = Array.Empty<PuzzleHint>();

    /// <summary>
    /// Gets the difficulty rating (1-5).
    /// </summary>
    public int Difficulty { get; private set; } = 1;

    /// <summary>
    /// Gets the category/theme of this riddle.
    /// </summary>
    public string? Category { get; private set; }

    /// <summary>
    /// Gets the message shown on correct answer.
    /// </summary>
    public string CorrectMessage { get; private set; } = "Correct!";

    /// <summary>
    /// Gets the message shown on wrong answer.
    /// </summary>
    public string WrongMessage { get; private set; } = "That is not correct.";

    // ===== Computed Properties =====

    /// <summary>
    /// Gets whether this riddle has hints.
    /// </summary>
    public bool HasHints => Hints.Count > 0;

    /// <summary>
    /// Gets the number of hints available.
    /// </summary>
    public int HintCount => Hints.Count;

    // ===== Constructors =====

    /// <summary>
    /// Private parameterless constructor for EF Core.
    /// </summary>
    private RiddleDefinition() { }

    // ===== Factory Methods =====

    /// <summary>
    /// Creates a riddle definition.
    /// </summary>
    /// <param name="id">Unique identifier.</param>
    /// <param name="question">The riddle question text.</param>
    /// <param name="acceptedAnswers">Valid answers (case-insensitive).</param>
    /// <param name="hints">Optional hints.</param>
    /// <param name="difficulty">Difficulty rating (1-5).</param>
    /// <param name="category">Category/theme.</param>
    /// <param name="correctMessage">Message on correct answer.</param>
    /// <param name="wrongMessage">Message on wrong answer.</param>
    /// <returns>A new RiddleDefinition instance.</returns>
    public static RiddleDefinition Create(
        string id,
        string question,
        IEnumerable<string> acceptedAnswers,
        IEnumerable<PuzzleHint>? hints = null,
        int difficulty = 1,
        string? category = null,
        string? correctMessage = null,
        string? wrongMessage = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(question);

        var answerList = acceptedAnswers.ToList();
        if (answerList.Count == 0)
            throw new ArgumentException("At least one accepted answer is required.", nameof(acceptedAnswers));

        return new RiddleDefinition
        {
            Id = id.ToLowerInvariant(),
            Question = question,
            AcceptedAnswers = answerList,
            Hints = (IReadOnlyList<PuzzleHint>?)hints?.OrderBy(h => h.Order).ToList() ?? Array.Empty<PuzzleHint>(),
            Difficulty = Math.Clamp(difficulty, 1, 5),
            Category = category,
            CorrectMessage = correctMessage ?? "Correct!",
            WrongMessage = wrongMessage ?? "That is not correct."
        };
    }

    // ===== Validation Methods =====

    /// <summary>
    /// Validates if an answer is correct.
    /// </summary>
    /// <param name="answer">The answer to validate.</param>
    /// <returns>True if the answer is accepted.</returns>
    public bool ValidateAnswer(string answer)
    {
        if (string.IsNullOrWhiteSpace(answer))
            return false;

        return AcceptedAnswers.Any(a =>
            a.Equals(answer.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets the hint at the specified order.
    /// </summary>
    /// <param name="order">The hint order (1-indexed).</param>
    /// <returns>The hint, or null if not found.</returns>
    public PuzzleHint? GetHint(int order)
    {
        return Hints.FirstOrDefault(h => h.Order == order);
    }

    /// <summary>
    /// Gets hints up to the specified revealed count.
    /// </summary>
    /// <param name="revealedCount">Number of hints already revealed.</param>
    /// <returns>The next hint to reveal, or null if all revealed.</returns>
    public PuzzleHint? GetNextHint(int revealedCount)
    {
        return Hints.FirstOrDefault(h => h.Order == revealedCount + 1);
    }

    /// <summary>
    /// Returns a string representation of this riddle definition.
    /// </summary>
    public override string ToString() =>
        $"RiddleDefinition({Id}, Difficulty={Difficulty}, Hints={HintCount})";
}
