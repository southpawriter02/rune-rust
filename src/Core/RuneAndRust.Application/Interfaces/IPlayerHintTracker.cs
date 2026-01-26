namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Tracks player hint discoveries and unlocked interactions for
/// puzzle progression and command availability.
/// </summary>
/// <remarks>
/// This tracker maintains the player's discovery state across the session,
/// enabling the puzzle hint system to avoid re-revealing hints and to
/// validate that unlocked interactions are available.
/// </remarks>
public interface IPlayerHintTracker
{
    /// <summary>
    /// Marks a hint as discovered by the player.
    /// </summary>
    /// <param name="hintId">The discovered hint ID.</param>
    /// <param name="puzzleId">The puzzle this hint relates to.</param>
    /// <param name="characterId">The discovering character.</param>
    /// <param name="roomId">The room where discovery occurred.</param>
    /// <param name="objectId">The object examined to reveal the hint.</param>
    void MarkHintDiscovered(string hintId, string puzzleId, string characterId, string roomId, string objectId);

    /// <summary>
    /// Checks if a hint has been discovered.
    /// </summary>
    /// <param name="hintId">The hint ID to check.</param>
    /// <param name="characterId">The character ID.</param>
    /// <returns>True if discovered.</returns>
    bool IsHintDiscovered(string hintId, string characterId);

    /// <summary>
    /// Gets all discovered hints for a puzzle.
    /// </summary>
    /// <param name="puzzleId">The puzzle ID.</param>
    /// <param name="characterId">The character ID.</param>
    /// <returns>List of discovered hints.</returns>
    IReadOnlyList<DiscoveredHint> GetDiscoveredHintsForPuzzle(string puzzleId, string characterId);

    /// <summary>
    /// Gets all discovered hints.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <returns>List of all discovered hints.</returns>
    IReadOnlyList<DiscoveredHint> GetAllDiscoveredHints(string characterId);

    /// <summary>
    /// Unlocks an interaction for the player.
    /// </summary>
    /// <param name="interaction">The interaction to unlock.</param>
    /// <param name="characterId">The character ID.</param>
    void UnlockInteraction(UnlockedInteraction interaction, string characterId);

    /// <summary>
    /// Gets all unlocked interactions.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <returns>List of unlocked interactions.</returns>
    IReadOnlyList<UnlockedInteraction> GetUnlockedInteractions(string characterId);

    /// <summary>
    /// Checks if a specific command is unlocked.
    /// </summary>
    /// <param name="commandText">The command to check.</param>
    /// <param name="characterId">The character ID.</param>
    /// <returns>True if the command is available.</returns>
    bool IsCommandUnlocked(string commandText, string characterId);

    /// <summary>
    /// Gets the total count of discovered hints for a character.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <returns>The count of discovered hints.</returns>
    int GetDiscoveredHintCount(string characterId);
}
