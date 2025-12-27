using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Reasons why auto-travel may be interrupted before reaching destination.
/// </summary>
public enum TravelInterruptReason
{
    /// <summary>No interruption occurred.</summary>
    None = 0,

    /// <summary>User pressed ESC to cancel travel.</summary>
    UserCancelled = 1,

    /// <summary>A hazard was encountered along the path.</summary>
    HazardEncountered = 2,

    /// <summary>Combat was triggered (ambush or enemy encounter).</summary>
    CombatTriggered = 3,

    /// <summary>Character ran out of critical resources.</summary>
    ResourceDepleted = 4
}

/// <summary>
/// Result of an auto-travel operation.
/// </summary>
/// <param name="Success">Whether travel completed to destination.</param>
/// <param name="RoomsTraveled">Number of rooms traversed before stopping.</param>
/// <param name="TurnsElapsed">Number of game turns consumed during travel.</param>
/// <param name="FinalRoom">The room where travel ended (may not be destination if interrupted).</param>
/// <param name="Message">User-facing message describing the travel outcome.</param>
/// <param name="InterruptReason">Reason for interruption if travel did not complete.</param>
public record AutoTravelResult(
    bool Success,
    int RoomsTraveled,
    int TurnsElapsed,
    Room? FinalRoom,
    string Message,
    TravelInterruptReason? InterruptReason)
{
    /// <summary>
    /// Creates a successful travel result.
    /// </summary>
    public static AutoTravelResult Succeeded(int roomsTraveled, int turnsElapsed, Room finalRoom)
        => new(true, roomsTraveled, turnsElapsed, finalRoom,
            $"Arrived at {finalRoom.Name} after {roomsTraveled} rooms ({turnsElapsed} turns).",
            TravelInterruptReason.None);

    /// <summary>
    /// Creates an interrupted travel result.
    /// </summary>
    public static AutoTravelResult Interrupted(
        int roomsTraveled,
        int turnsElapsed,
        Room? finalRoom,
        TravelInterruptReason reason,
        string message)
        => new(false, roomsTraveled, turnsElapsed, finalRoom, message, reason);

    /// <summary>
    /// Creates a failed travel result (could not start).
    /// </summary>
    public static AutoTravelResult Failed(string message)
        => new(false, 0, 0, null, message, null);
}

/// <summary>
/// Service for executing auto-travel along a calculated path.
/// Handles precondition validation, path safety checks, and movement execution.
/// </summary>
/// <remarks>See: SPEC-NAV-002 for Pathfinder (Fast Travel) design (v0.3.20c).</remarks>
public interface IAutoTravelService
{
    /// <summary>
    /// Validates that the character can begin fast travel.
    /// Checks for exhaustion, encumbrance, and other blocking conditions.
    /// </summary>
    /// <param name="character">The character attempting to travel.</param>
    /// <returns>Error message if preconditions not met; null if travel allowed.</returns>
    Task<string?> ValidateTravelPreconditionsAsync(Character character);

    /// <summary>
    /// Validates that the path is safe to traverse.
    /// Checks for lethal rooms and dormant movement hazards.
    /// </summary>
    /// <param name="path">Room IDs along the proposed path.</param>
    /// <returns>Error message if path is unsafe; null if path is clear.</returns>
    Task<string?> ValidatePathSafetyAsync(IEnumerable<Guid> path);

    /// <summary>
    /// Executes travel along a series of directions.
    /// Calls NavigationService.MoveAsync for each step with visual feedback.
    /// </summary>
    /// <param name="directions">The movement directions to execute in order.</param>
    /// <param name="cancellationToken">Token to cancel travel (ESC key).</param>
    /// <returns>Result describing the travel outcome.</returns>
    Task<AutoTravelResult> ExecuteTravelAsync(
        IEnumerable<Direction> directions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// High-level travel command that resolves targets and executes the full journey.
    /// Handles keywords like "home", "anchor", "surface" and partial room names.
    /// </summary>
    /// <param name="target">The travel target (room name, keyword, or alias).</param>
    /// <param name="cancellationToken">Token to cancel travel (ESC key).</param>
    /// <returns>Result describing the travel outcome.</returns>
    Task<AutoTravelResult> TravelToAsync(
        string target,
        CancellationToken cancellationToken = default);
}
