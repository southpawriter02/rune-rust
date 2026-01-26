namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service interface for puzzle hint discovery, tracking, and interaction
/// unlocking within the examination system.
/// </summary>
/// <remarks>
/// <para>
/// This service manages the relationship between examination results and
/// puzzle hints, checking for applicable hints when expert examination
/// succeeds and tracking what the player has discovered.
/// </para>
/// <para>
/// The service also manages unlocked interactions, which are commands
/// that become available only after specific hints are discovered.
/// </para>
/// </remarks>
public interface IExaminationPuzzleHintService
{
    /// <summary>
    /// Checks for puzzle hints that can be revealed from an examination result.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method is called after an examination check succeeds at Layer 3
    /// (Expert). It looks up any ExaminationHintLinks for the object and
    /// checks if their conditions are met.
    /// </para>
    /// <para>
    /// Hints are only revealed once per player; subsequent examinations of
    /// the same object will not re-reveal already-discovered hints.
    /// </para>
    /// </remarks>
    /// <param name="objectId">The examined object ID.</param>
    /// <param name="achievedLayer">The examination layer achieved (1, 2, or 3).</param>
    /// <param name="characterId">The examining character's ID.</param>
    /// <param name="roomId">The room where examination occurred.</param>
    /// <returns>A result containing any revealed hints and unlocked interactions.</returns>
    HintRevealResult CheckForHints(string objectId, int achievedLayer, string characterId, string roomId);

    /// <summary>
    /// Gets all hints the player has discovered for a specific puzzle.
    /// </summary>
    /// <param name="puzzleId">The puzzle ID to check.</param>
    /// <param name="characterId">The character ID.</param>
    /// <returns>List of discovered hints for this puzzle.</returns>
    IReadOnlyList<DiscoveredHint> GetDiscoveredHintsForPuzzle(string puzzleId, string characterId);

    /// <summary>
    /// Gets all hints the player has discovered across all puzzles.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <returns>List of all discovered hints.</returns>
    IReadOnlyList<DiscoveredHint> GetAllDiscoveredHints(string characterId);

    /// <summary>
    /// Checks if a specific hint has been discovered.
    /// </summary>
    /// <param name="hintId">The hint ID to check.</param>
    /// <param name="characterId">The character ID.</param>
    /// <returns>True if the hint has been discovered.</returns>
    bool IsHintDiscovered(string hintId, string characterId);

    /// <summary>
    /// Gets all interactions the player has unlocked through hint discovery.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <returns>List of unlocked interactions.</returns>
    IReadOnlyList<UnlockedInteraction> GetUnlockedInteractions(string characterId);

    /// <summary>
    /// Checks if a specific interaction has been unlocked.
    /// </summary>
    /// <param name="commandText">The command text to check.</param>
    /// <param name="characterId">The character ID.</param>
    /// <returns>True if the interaction is available.</returns>
    bool IsInteractionUnlocked(string commandText, string characterId);

    /// <summary>
    /// Gets the puzzle hint by ID.
    /// </summary>
    /// <param name="hintId">The hint ID.</param>
    /// <returns>The puzzle hint, or null if not found.</returns>
    ExaminationPuzzleHint? GetHintById(string hintId);

    /// <summary>
    /// Gets all hints for a specific puzzle (regardless of discovery state).
    /// </summary>
    /// <param name="puzzleId">The puzzle ID.</param>
    /// <returns>All hints associated with this puzzle.</returns>
    IReadOnlyList<ExaminationPuzzleHint> GetHintsForPuzzle(string puzzleId);

    /// <summary>
    /// Gets the percentage of hints discovered for a puzzle.
    /// </summary>
    /// <param name="puzzleId">The puzzle ID.</param>
    /// <param name="characterId">The character ID.</param>
    /// <returns>Percentage (0-100) of hints discovered.</returns>
    int GetPuzzleHintProgress(string puzzleId, string characterId);
}
