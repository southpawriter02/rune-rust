using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents an exit from a room with visibility and discovery state.
/// </summary>
/// <remarks>
/// Exits can be hidden and require discovery via the search command.
/// Hidden exits are not shown in room descriptions until discovered.
/// The Exit record replaces the previous Guid-based exit system.
/// </remarks>
public readonly record struct Exit
{
    /// <summary>
    /// Gets the ID of the room this exit leads to.
    /// </summary>
    public Guid TargetRoomId { get; init; }

    /// <summary>
    /// Gets whether this exit is hidden and requires discovery.
    /// </summary>
    /// <remarks>
    /// Hidden exits are not shown in room descriptions or the exits list
    /// until they have been discovered via the search command.
    /// </remarks>
    public bool IsHidden { get; init; }

    /// <summary>
    /// Gets whether this exit has been discovered by the player.
    /// </summary>
    /// <remarks>
    /// For non-hidden exits, this is always true.
    /// For hidden exits, this becomes true after successful discovery.
    /// </remarks>
    public bool IsDiscovered { get; init; }

    /// <summary>
    /// Gets the subtle hint that may be noticed before full discovery.
    /// </summary>
    /// <remarks>
    /// Examples: "a slight draft", "faint scratching sounds", "discolored stones"
    /// This hint may be shown on failed search attempts or high perception.
    /// </remarks>
    public string? HiddenHint { get; init; }

    /// <summary>
    /// Gets the difficulty class for discovering this exit.
    /// </summary>
    /// <remarks>
    /// Standard DC values: Easy (8), Moderate (12), Challenging (15), Hard (18).
    /// </remarks>
    public int DiscoveryDC { get; init; }

    /// <summary>
    /// Gets the type of vertical connection, if applicable.
    /// </summary>
    /// <remarks>
    /// Null for horizontal exits (N, S, E, W).
    /// Set for vertical exits (Up, Down) to describe the connection type.
    /// </remarks>
    public StairType? StairType { get; init; }

    /// <summary>
    /// Gets whether this exit is visible to the player.
    /// </summary>
    /// <remarks>
    /// An exit is visible if it's not hidden, or if it has been discovered.
    /// </remarks>
    public bool IsVisible => !IsHidden || IsDiscovered;

    /// <summary>
    /// Creates a standard (non-hidden) exit to the target room.
    /// </summary>
    /// <param name="targetId">The ID of the target room.</param>
    /// <returns>A visible, non-hidden exit.</returns>
    public static Exit Standard(Guid targetId) => new()
    {
        TargetRoomId = targetId,
        IsHidden = false,
        IsDiscovered = true,
        DiscoveryDC = 0,
        StairType = null
    };

    /// <summary>
    /// Creates a standard vertical exit with stair type.
    /// </summary>
    /// <param name="targetId">The ID of the target room.</param>
    /// <param name="stairType">The type of vertical connection.</param>
    /// <returns>A visible, non-hidden vertical exit.</returns>
    public static Exit Vertical(Guid targetId, StairType stairType) => new()
    {
        TargetRoomId = targetId,
        IsHidden = false,
        IsDiscovered = true,
        DiscoveryDC = 0,
        StairType = stairType
    };

    /// <summary>
    /// Creates a hidden exit that requires discovery.
    /// </summary>
    /// <param name="targetId">The ID of the target room.</param>
    /// <param name="discoveryDC">The difficulty class for discovery.</param>
    /// <param name="hint">Optional hint for near-misses.</param>
    /// <returns>A hidden exit awaiting discovery.</returns>
    public static Exit Hidden(Guid targetId, int discoveryDC, string? hint = null) => new()
    {
        TargetRoomId = targetId,
        IsHidden = true,
        IsDiscovered = false,
        DiscoveryDC = discoveryDC,
        HiddenHint = hint,
        StairType = null
    };

    /// <summary>
    /// Creates a hidden vertical exit that requires discovery.
    /// </summary>
    /// <param name="targetId">The ID of the target room.</param>
    /// <param name="stairType">The type of vertical connection.</param>
    /// <param name="discoveryDC">The difficulty class for discovery.</param>
    /// <param name="hint">Optional hint for near-misses.</param>
    /// <returns>A hidden vertical exit awaiting discovery.</returns>
    public static Exit HiddenVertical(
        Guid targetId,
        StairType stairType,
        int discoveryDC,
        string? hint = null) => new()
    {
        TargetRoomId = targetId,
        IsHidden = true,
        IsDiscovered = false,
        DiscoveryDC = discoveryDC,
        HiddenHint = hint,
        StairType = stairType
    };

    /// <summary>
    /// Creates a new Exit with the discovered state set to true.
    /// </summary>
    /// <returns>A copy of this exit with IsDiscovered = true.</returns>
    public Exit AsDiscovered() => this with { IsDiscovered = true };
}
