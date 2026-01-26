using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for calculating and applying passive perception checks.
/// </summary>
/// <remarks>
/// <para>
/// Passive perception is calculated from a character's WITS attribute
/// and modified by status effects, environmental factors, and equipment.
/// </para>
/// <para>
/// The service automatically checks hidden elements when characters enter
/// rooms, revealing those with DC at or below the effective passive value.
/// </para>
/// </remarks>
public interface IPassivePerceptionService
{
    /// <summary>
    /// Calculates passive perception for a character.
    /// </summary>
    /// <param name="characterId">The character's unique identifier.</param>
    /// <returns>The calculated passive perception with breakdown.</returns>
    /// <exception cref="ArgumentException">Thrown when characterId is invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when character not found.</exception>
    PassivePerception CalculatePassivePerception(string characterId);

    /// <summary>
    /// Checks passive perception against all hidden elements in a room.
    /// </summary>
    /// <param name="characterId">The character entering the room.</param>
    /// <param name="roomId">The room's database ID being entered.</param>
    /// <returns>Result containing revealed and missed element counts.</returns>
    /// <remarks>
    /// Called automatically on room entry. Elements with DC at or below
    /// the character's effective passive perception are revealed.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown when IDs are invalid.</exception>
    PassivePerceptionResult CheckRoomEntry(string characterId, Guid roomId);

    /// <summary>
    /// Reveals a specific hidden element (for active search or special abilities).
    /// </summary>
    /// <param name="elementId">The hidden element's definition ID.</param>
    /// <param name="characterId">The character revealing the element.</param>
    /// <exception cref="ArgumentException">Thrown when IDs are invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when element not found or already revealed.</exception>
    void RevealElement(string elementId, string characterId);
}
