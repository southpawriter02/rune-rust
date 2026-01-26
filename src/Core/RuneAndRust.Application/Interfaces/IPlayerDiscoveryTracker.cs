namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Tracks what flora and fauna species a player has discovered in each room.
/// </summary>
/// <remarks>
/// <para>
/// Discovery tracking is essential for the harvest system - players can only
/// harvest flora they have noticed through ambient observation or examination.
/// </para>
/// <para>
/// Discoveries are persistent across sessions, allowing players to return to
/// previously explored rooms and harvest resources they identified earlier.
/// </para>
/// </remarks>
public interface IPlayerDiscoveryTracker
{
    /// <summary>
    /// Marks a species as discovered in a specific room.
    /// </summary>
    /// <param name="roomId">The room where discovery occurred.</param>
    /// <param name="speciesId">The species descriptor ID.</param>
    /// <exception cref="ArgumentException">Thrown when roomId or speciesId is null/whitespace.</exception>
    void MarkSpeciesDiscovered(string roomId, string speciesId);

    /// <summary>
    /// Gets all species discovered in a specific room.
    /// </summary>
    /// <param name="roomId">The room to query.</param>
    /// <returns>List of discovered species IDs.</returns>
    /// <exception cref="ArgumentException">Thrown when roomId is null or whitespace.</exception>
    IReadOnlyList<string> GetDiscoveredSpecies(string roomId);

    /// <summary>
    /// Checks if a specific species has been discovered in a room.
    /// </summary>
    /// <param name="roomId">The room to check.</param>
    /// <param name="speciesId">The species to check for.</param>
    /// <returns>True if discovered, false otherwise.</returns>
    bool IsSpeciesDiscovered(string roomId, string speciesId);

    /// <summary>
    /// Gets the total count of species discovered across all rooms.
    /// </summary>
    /// <returns>The total discovery count.</returns>
    int GetTotalDiscoveryCount();

    /// <summary>
    /// Clears all discoveries for a specific room.
    /// </summary>
    /// <param name="roomId">The room to clear.</param>
    void ClearRoomDiscoveries(string roomId);
}
