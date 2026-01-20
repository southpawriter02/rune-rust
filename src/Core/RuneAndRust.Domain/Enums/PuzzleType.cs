namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Categories of puzzle mechanics.
/// </summary>
/// <remarks>
/// <para>
/// PuzzleType defines the interaction pattern for each puzzle category:
/// </para>
/// <list type="bullet">
///   <item><description><see cref="Sequence"/> - Activate objects in order via lever/button interactions</description></item>
///   <item><description><see cref="Combination"/> - Enter numeric or symbolic codes</description></item>
///   <item><description><see cref="Pattern"/> - Match or reproduce visual/spatial patterns</description></item>
///   <item><description><see cref="Riddle"/> - Answer questions posed by NPCs</description></item>
///   <item><description><see cref="Logic"/> - General logic puzzles with custom validation</description></item>
/// </list>
/// <para>
/// Type-specific validation is implemented in v0.4.2b. This enum provides categorization
/// for future puzzle type handlers.
/// </para>
/// </remarks>
/// <seealso cref="Entities.Puzzle"/>
public enum PuzzleType
{
    /// <summary>
    /// Activate objects in a specific order.
    /// </summary>
    /// <remarks>
    /// Sequence puzzles require the player to interact with objects (levers, buttons, statues)
    /// in a specific order. Wrong order resets the sequence.
    /// </remarks>
    Sequence = 0,

    /// <summary>
    /// Enter a correct combination or code.
    /// </summary>
    /// <remarks>
    /// Combination puzzles require entering a numeric or symbolic code (e.g., "4-7-2-1").
    /// The code is validated against a stored solution.
    /// </remarks>
    Combination = 1,

    /// <summary>
    /// Match or reproduce a pattern.
    /// </summary>
    /// <remarks>
    /// Pattern puzzles require the player to match visual, spatial, or sequential patterns.
    /// Examples include Simon-says style memory games or tile matching.
    /// </remarks>
    Pattern = 2,

    /// <summary>
    /// Answer a riddle correctly.
    /// </summary>
    /// <remarks>
    /// Riddle puzzles present a question with one or more valid answers.
    /// Answer matching is typically case-insensitive and may accept synonyms.
    /// </remarks>
    Riddle = 3,

    /// <summary>
    /// General logic puzzle with custom validation.
    /// </summary>
    /// <remarks>
    /// Logic puzzles have custom validation logic that doesn't fit other categories.
    /// Examples include mathematical puzzles, state-based puzzles, or abstract challenges.
    /// </remarks>
    Logic = 4
}
