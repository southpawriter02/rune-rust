namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the complete outcome of a scout action.
/// </summary>
/// <remarks>
/// <para>
/// ScoutResult captures all information discovered during reconnaissance:
/// <list type="bullet">
///   <item><description>RoomsRevealed: List of adjacent rooms successfully scouted</description></item>
///   <item><description>Each room includes enemies, hazards, and points of interest</description></item>
///   <item><description>NetSuccesses determines how many rooms were revealed</description></item>
/// </list>
/// </para>
/// <para>
/// Room Revelation Formula:
/// <code>
/// if (netSuccesses >= 0):
///     roomsRevealed = 1 + (netSuccesses / 2)
/// else:
///     roomsRevealed = 0
///
/// roomsRevealed = min(roomsRevealed, adjacentRoomCount)
/// </code>
/// </para>
/// <para>
/// This rewards investment in Wasteland Survival with more tactical information:
/// <list type="bullet">
///   <item><description>Net 0-1: 1 room revealed (base success)</description></item>
///   <item><description>Net 2-3: 2 rooms revealed</description></item>
///   <item><description>Net 4-5: 3 rooms revealed</description></item>
///   <item><description>Net 6+: 4+ rooms revealed (capped at adjacent count)</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="RoomsRevealed">List of scouted rooms with their contents.</param>
/// <param name="NetSuccesses">Number of net successes on the scout check.</param>
/// <param name="ScoutSucceeded">Whether the scout action succeeded at all.</param>
/// <param name="TargetDc">The DC that was attempted.</param>
/// <param name="RollDetails">Optional details about the dice roll for logging/display.</param>
public readonly record struct ScoutResult(
    IReadOnlyList<ScoutedRoom> RoomsRevealed,
    int NetSuccesses,
    bool ScoutSucceeded,
    int TargetDc = 0,
    string? RollDetails = null)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the total number of rooms revealed.
    /// </summary>
    public int RoomCount => RoomsRevealed?.Count ?? 0;

    /// <summary>
    /// Gets the total number of enemies detected across all scouted rooms.
    /// </summary>
    public int TotalEnemiesDetected =>
        RoomsRevealed?.Sum(r => r.Enemies?.Sum(e => e.Count) ?? 0) ?? 0;

    /// <summary>
    /// Gets the total number of hazards detected across all scouted rooms.
    /// </summary>
    public int TotalHazardsDetected =>
        RoomsRevealed?.Sum(r => r.Hazards?.Count ?? 0) ?? 0;

    /// <summary>
    /// Gets the total number of points of interest detected across all scouted rooms.
    /// </summary>
    public int TotalPointsOfInterest =>
        RoomsRevealed?.Sum(r => r.PointsOfInterest?.Count ?? 0) ?? 0;

    /// <summary>
    /// Gets whether any enemies were detected.
    /// </summary>
    public bool EnemiesPresent => TotalEnemiesDetected > 0;

    /// <summary>
    /// Gets whether any hazards were detected.
    /// </summary>
    public bool HazardsPresent => TotalHazardsDetected > 0;

    /// <summary>
    /// Gets whether any points of interest were detected.
    /// </summary>
    public bool PointsOfInterestPresent => TotalPointsOfInterest > 0;

    /// <summary>
    /// Gets whether any threats (enemies or hazards) were detected.
    /// </summary>
    public bool ThreatsDetected => EnemiesPresent || HazardsPresent;

    /// <summary>
    /// Gets whether any dangerous rooms were detected.
    /// </summary>
    public bool DangerousRoomsDetected =>
        RoomsRevealed?.Any(r => r.IsDangerous) ?? false;

    /// <summary>
    /// Gets the margin by which the scout succeeded or failed.
    /// </summary>
    public int Margin => NetSuccesses;

    /// <summary>
    /// Gets whether this was a critical success (revealed many rooms).
    /// </summary>
    /// <remarks>
    /// A critical success occurs when net successes >= 4, revealing 3+ rooms.
    /// </remarks>
    public bool IsCriticalSuccess => ScoutSucceeded && NetSuccesses >= 4;

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a successful scout result.
    /// </summary>
    /// <param name="rooms">List of rooms revealed.</param>
    /// <param name="netSuccesses">Net successes from the skill check.</param>
    /// <param name="targetDc">The DC that was attempted.</param>
    /// <param name="rollDetails">Optional roll details for logging.</param>
    /// <returns>A successful scout result.</returns>
    public static ScoutResult Success(
        IReadOnlyList<ScoutedRoom> rooms,
        int netSuccesses,
        int targetDc = 0,
        string? rollDetails = null)
    {
        return new ScoutResult(
            RoomsRevealed: rooms,
            NetSuccesses: netSuccesses,
            ScoutSucceeded: true,
            TargetDc: targetDc,
            RollDetails: rollDetails);
    }

    /// <summary>
    /// Creates a failed scout result.
    /// </summary>
    /// <param name="netSuccesses">Net successes from the failed check.</param>
    /// <param name="targetDc">The DC that was not met.</param>
    /// <param name="rollDetails">Optional roll details for logging.</param>
    /// <returns>A failed scout result.</returns>
    public static ScoutResult Failure(
        int netSuccesses,
        int targetDc = 0,
        string? rollDetails = null)
    {
        return new ScoutResult(
            RoomsRevealed: Array.Empty<ScoutedRoom>(),
            NetSuccesses: netSuccesses,
            ScoutSucceeded: false,
            TargetDc: targetDc,
            RollDetails: rollDetails);
    }

    /// <summary>
    /// Creates an empty result for when scouting cannot be attempted.
    /// </summary>
    /// <param name="reason">Optional reason why scouting couldn't be attempted.</param>
    /// <returns>An empty scout result.</returns>
    public static ScoutResult Empty(string? reason = null)
    {
        return new ScoutResult(
            RoomsRevealed: Array.Empty<ScoutedRoom>(),
            NetSuccesses: 0,
            ScoutSucceeded: false,
            TargetDc: 0,
            RollDetails: reason ?? "Scouting not attempted");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CALCULATION METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculates the number of rooms that should be revealed based on net successes.
    /// </summary>
    /// <param name="netSuccesses">Number of successes above DC.</param>
    /// <param name="adjacentRoomCount">Maximum available adjacent rooms.</param>
    /// <returns>Number of rooms to reveal.</returns>
    /// <remarks>
    /// Formula: 1 + (netSuccesses / 2), capped at adjacent room count.
    /// Failure (net &lt; 0) reveals 0 rooms.
    /// </remarks>
    public static int CalculateRoomsRevealed(int netSuccesses, int adjacentRoomCount)
    {
        if (netSuccesses < 0)
        {
            return 0;
        }

        // Base 1 room + 1 per 2 net successes
        int roomsFromSuccesses = 1 + (netSuccesses / 2);

        // Cannot exceed available adjacent rooms
        return Math.Min(roomsFromSuccesses, adjacentRoomCount);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STRING FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a display string summarizing the scout results for the player.
    /// </summary>
    /// <returns>A formatted string suitable for player display.</returns>
    public string ToDisplayString()
    {
        if (!ScoutSucceeded)
        {
            return "You strain to observe the surroundings but learn nothing useful. " +
                   "The terrain obscures your view.";
        }

        var result = $"You carefully scout the area and discover information about " +
                     $"{RoomCount} adjacent {(RoomCount == 1 ? "room" : "rooms")}.\n";

        if (EnemiesPresent)
        {
            result += $"\n[WARNING] Enemies detected: {TotalEnemiesDetected} hostile " +
                      $"{(TotalEnemiesDetected == 1 ? "presence" : "presences")} spotted.";
        }

        if (HazardsPresent)
        {
            result += $"\n[CAUTION] Hazards detected: {TotalHazardsDetected} environmental " +
                      $"{(TotalHazardsDetected == 1 ? "danger" : "dangers")} identified.";
        }

        if (PointsOfInterestPresent)
        {
            result += $"\n[INTEREST] Points of interest: {TotalPointsOfInterest} notable " +
                      $"{(TotalPointsOfInterest == 1 ? "feature" : "features")} observed.";
        }

        return result;
    }

    /// <summary>
    /// Creates a detailed string including all scouted room information.
    /// </summary>
    /// <returns>A multi-line string with complete scout details.</returns>
    public string ToDetailedString()
    {
        var result = $"ScoutResult\n" +
                     $"  Succeeded: {ScoutSucceeded}\n" +
                     $"  Net Successes: {NetSuccesses}\n" +
                     $"  Target DC: {TargetDc}\n" +
                     $"  Rooms Revealed: {RoomCount}\n" +
                     $"  Total Enemies: {TotalEnemiesDetected}\n" +
                     $"  Total Hazards: {TotalHazardsDetected}\n" +
                     $"  Total POIs: {TotalPointsOfInterest}\n";

        if (!string.IsNullOrEmpty(RollDetails))
        {
            result += $"  Roll Details: {RollDetails}\n";
        }

        if (RoomCount > 0)
        {
            result += "\n  Scouted Rooms:\n";
            foreach (var room in RoomsRevealed)
            {
                result += $"    - {room.ToSummaryString()}\n";
            }
        }

        return result;
    }

    /// <summary>
    /// Returns a human-readable summary of the scout result.
    /// </summary>
    /// <returns>A formatted string describing the outcome.</returns>
    public override string ToString()
    {
        if (!ScoutSucceeded)
        {
            return $"Scout failed ({NetSuccesses} vs DC {TargetDc})";
        }

        var criticalStr = IsCriticalSuccess ? " [CRITICAL]" : "";
        return $"Scouted {RoomCount} room(s) ({NetSuccesses} net successes){criticalStr}";
    }
}
