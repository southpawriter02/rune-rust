using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a room revealed through scouting operations.
/// </summary>
/// <remarks>
/// <para>
/// ScoutedRoom captures all information discovered about an adjacent room
/// during reconnaissance. This comprehensive snapshot allows players to
/// assess threats, opportunities, and potential routes before committing.
/// </para>
/// <para>
/// Information gathered per scouted room:
/// <list type="bullet">
///   <item><description>Basic Info: Room name, description, terrain type</description></item>
///   <item><description>Enemies: Type, count, threat level, position</description></item>
///   <item><description>Hazards: Type, severity, avoidance hints</description></item>
///   <item><description>Points of Interest: Containers, signs, mechanisms, etc.</description></item>
///   <item><description>Exits: Available directions for further movement</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="RoomId">Unique identifier for the scouted room.</param>
/// <param name="RoomName">Display name of the room.</param>
/// <param name="Description">Brief narrative description of the room.</param>
/// <param name="TerrainType">The navigation terrain type of this room.</param>
/// <param name="Enemies">List of enemies detected in this room.</param>
/// <param name="Hazards">List of hazards detected in this room.</param>
/// <param name="PointsOfInterest">List of points of interest in this room.</param>
/// <param name="Exits">List of exit directions from this room.</param>
public readonly record struct ScoutedRoom(
    string RoomId,
    string RoomName,
    string Description,
    NavigationTerrainType TerrainType,
    IReadOnlyList<DetectedEnemy> Enemies,
    IReadOnlyList<DetectedHazard> Hazards,
    IReadOnlyList<PointOfInterest> PointsOfInterest,
    IReadOnlyList<string> Exits)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this room contains enemies.
    /// </summary>
    public bool HasEnemies => Enemies?.Count > 0;

    /// <summary>
    /// Gets whether this room contains hazards.
    /// </summary>
    public bool HasHazards => Hazards?.Count > 0;

    /// <summary>
    /// Gets whether this room has points of interest.
    /// </summary>
    public bool HasPointsOfInterest => PointsOfInterest?.Count > 0;

    /// <summary>
    /// Gets whether this room contains any threats (enemies or hazards).
    /// </summary>
    public bool HasThreats => HasEnemies || HasHazards;

    /// <summary>
    /// Gets the total number of enemies in this room.
    /// </summary>
    public int TotalEnemyCount => Enemies?.Sum(e => e.Count) ?? 0;

    /// <summary>
    /// Gets the highest threat level among enemies in this room.
    /// </summary>
    public ThreatLevel? HighestThreatLevel =>
        HasEnemies ? Enemies.Max(e => e.ThreatLevel) : null;

    /// <summary>
    /// Gets the highest hazard severity in this room.
    /// </summary>
    public HazardSeverity? HighestHazardSeverity =>
        HasHazards ? Hazards.Max(h => h.Severity) : null;

    /// <summary>
    /// Gets whether this room is considered dangerous (high threats or severe hazards).
    /// </summary>
    public bool IsDangerous =>
        (HighestThreatLevel >= ThreatLevel.High) ||
        (HighestHazardSeverity >= HazardSeverity.Severe);

    /// <summary>
    /// Gets the number of exits from this room.
    /// </summary>
    public int ExitCount => Exits?.Count ?? 0;

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a ScoutedRoom with the specified information.
    /// </summary>
    /// <param name="roomId">The room's unique identifier.</param>
    /// <param name="roomName">The room's display name.</param>
    /// <param name="description">Brief description.</param>
    /// <param name="terrainType">The terrain type.</param>
    /// <param name="enemies">Detected enemies.</param>
    /// <param name="hazards">Detected hazards.</param>
    /// <param name="pointsOfInterest">Detected points of interest.</param>
    /// <param name="exits">Available exit directions.</param>
    /// <returns>A new ScoutedRoom instance.</returns>
    public static ScoutedRoom Create(
        string roomId,
        string roomName,
        string description,
        NavigationTerrainType terrainType,
        IReadOnlyList<DetectedEnemy>? enemies = null,
        IReadOnlyList<DetectedHazard>? hazards = null,
        IReadOnlyList<PointOfInterest>? pointsOfInterest = null,
        IReadOnlyList<string>? exits = null)
    {
        return new ScoutedRoom(
            RoomId: roomId,
            RoomName: roomName,
            Description: description,
            TerrainType: terrainType,
            Enemies: enemies ?? Array.Empty<DetectedEnemy>(),
            Hazards: hazards ?? Array.Empty<DetectedHazard>(),
            PointsOfInterest: pointsOfInterest ?? Array.Empty<PointOfInterest>(),
            Exits: exits ?? Array.Empty<string>());
    }

    /// <summary>
    /// Creates an empty ScoutedRoom (for testing or placeholders).
    /// </summary>
    /// <param name="roomId">The room's unique identifier.</param>
    /// <param name="roomName">The room's display name.</param>
    /// <returns>A ScoutedRoom with no enemies, hazards, or POIs.</returns>
    public static ScoutedRoom Empty(string roomId, string roomName)
    {
        return Create(
            roomId: roomId,
            roomName: roomName,
            description: "An unremarkable room.",
            terrainType: NavigationTerrainType.ModerateRuins);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STRING FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a display string for this scouted room.
    /// </summary>
    /// <returns>A formatted string suitable for player display.</returns>
    public string ToDisplayString()
    {
        var result = $"[{RoomName}]\n{Description}\n";

        if (HasEnemies)
        {
            result += $"\n  Enemies: {string.Join(", ", Enemies.Select(e => e.ToDisplayString()))}";
        }

        if (HasHazards)
        {
            result += $"\n  Hazards: {string.Join(", ", Hazards.Select(h => h.ToDisplayString()))}";
        }

        if (HasPointsOfInterest)
        {
            result += $"\n  Notable: {string.Join(", ", PointsOfInterest.Select(p => p.ToDisplayString()))}";
        }

        if (ExitCount > 0)
        {
            result += $"\n  Exits: {string.Join(", ", Exits)}";
        }

        return result;
    }

    /// <summary>
    /// Creates a compact summary of this scouted room.
    /// </summary>
    /// <returns>A single-line summary.</returns>
    public string ToSummaryString()
    {
        var parts = new List<string> { RoomName };

        if (HasEnemies)
        {
            var threatStr = HighestThreatLevel?.GetShortDescriptor() ?? "unknown";
            parts.Add($"{TotalEnemyCount} enemy ({threatStr})");
        }

        if (HasHazards)
        {
            var severityStr = HighestHazardSeverity?.GetShortDescriptor() ?? "unknown";
            parts.Add($"{Hazards.Count} hazard ({severityStr})");
        }

        if (HasPointsOfInterest)
        {
            parts.Add($"{PointsOfInterest.Count} POI");
        }

        return string.Join(" | ", parts);
    }

    /// <summary>
    /// Creates a detailed string for logging purposes.
    /// </summary>
    /// <returns>A multi-line detailed string.</returns>
    public string ToDetailedString()
    {
        var result = $"ScoutedRoom: {RoomName} ({RoomId})\n" +
                     $"  Terrain: {TerrainType.GetDisplayName()}\n" +
                     $"  Description: {Description}\n" +
                     $"  Enemies: {Enemies?.Count ?? 0} (Total: {TotalEnemyCount})\n" +
                     $"  Hazards: {Hazards?.Count ?? 0}\n" +
                     $"  Points of Interest: {PointsOfInterest?.Count ?? 0}\n" +
                     $"  Exits: {ExitCount} ({string.Join(", ", Exits ?? Array.Empty<string>())})\n" +
                     $"  Is Dangerous: {IsDangerous}";

        return result;
    }

    /// <summary>
    /// Returns a human-readable summary of the scouted room.
    /// </summary>
    /// <returns>A formatted string describing the room.</returns>
    public override string ToString() => ToSummaryString();
}
