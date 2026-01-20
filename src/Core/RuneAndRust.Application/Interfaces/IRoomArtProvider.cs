using RuneAndRust.Application.Configuration;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Provides room ASCII art definitions.
/// </summary>
public interface IRoomArtProvider
{
    /// <summary>
    /// Gets the art definition for a room type.
    /// </summary>
    /// <param name="roomTypeId">The room type ID.</param>
    /// <returns>Art definition, or null if not found.</returns>
    RoomArtDefinition? GetArtForRoom(string roomTypeId);
    
    /// <summary>
    /// Gets the default art settings.
    /// </summary>
    /// <returns>Default art settings.</returns>
    DefaultArtSettings GetDefaultArt();
    
    /// <summary>
    /// Checks if a room type has custom art.
    /// </summary>
    /// <param name="roomTypeId">The room type ID.</param>
    /// <returns>True if custom art exists.</returns>
    bool HasArt(string roomTypeId);
    
    /// <summary>
    /// Gets all available room art IDs.
    /// </summary>
    IReadOnlyList<string> AvailableArtIds { get; }
}
