using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Context for performing a scout action.
/// </summary>
/// <remarks>
/// <para>
/// ScoutContext encapsulates all factors that affect a scout attempt:
/// <list type="bullet">
///   <item><description>Current position and terrain determine base DC</description></item>
///   <item><description>Adjacent rooms are candidates for revelation</description></item>
///   <item><description>Equipment provides dice pool bonuses</description></item>
///   <item><description>Visibility conditions modify the DC</description></item>
/// </list>
/// </para>
/// <para>
/// DC Calculation:
/// <code>Final DC = Base DC (terrain) + Visibility Modifier</code>
/// </para>
/// <para>
/// Base DCs by terrain:
/// <list type="bullet">
///   <item><description>OpenWasteland: DC 8 (clear sightlines)</description></item>
///   <item><description>ModerateRuins: DC 12 (partial cover)</description></item>
///   <item><description>DenseRuins: DC 16 (many hiding spots)</description></item>
///   <item><description>Labyrinthine: DC 20 (twisting passages)</description></item>
///   <item><description>GlitchedLabyrinth: DC 24 (reality distortions)</description></item>
/// </list>
/// </para>
/// <para>
/// Visibility modifiers (added to DC):
/// <list type="bullet">
///   <item><description>Excellent visibility: -2</description></item>
///   <item><description>Good visibility: -1</description></item>
///   <item><description>Normal visibility: 0</description></item>
///   <item><description>Poor visibility: +2</description></item>
///   <item><description>Terrible visibility: +4</description></item>
///   <item><description>Static storm: +6</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="PlayerId">The player performing the scout.</param>
/// <param name="CurrentRoomId">The room the player is scouting from.</param>
/// <param name="TerrainType">The terrain type affecting base DC.</param>
/// <param name="AdjacentRoomIds">IDs of rooms that can potentially be scouted.</param>
/// <param name="EquipmentBonus">Bonus dice from survival equipment (positive values help).</param>
/// <param name="VisibilityModifier">Modifier to DC from lighting/weather (positive = harder).</param>
public readonly record struct ScoutContext(
    string PlayerId,
    string CurrentRoomId,
    NavigationTerrainType TerrainType,
    IReadOnlyList<string> AdjacentRoomIds,
    int EquipmentBonus,
    int VisibilityModifier)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the number of adjacent rooms available to scout.
    /// </summary>
    public int AdjacentRoomCount => AdjacentRoomIds?.Count ?? 0;

    /// <summary>
    /// Gets the base DC for scouting based on terrain type.
    /// </summary>
    public int BaseDc => TerrainType.GetBaseDc();

    /// <summary>
    /// Gets the final DC including visibility modifier.
    /// </summary>
    /// <remarks>
    /// Final DC is clamped to a minimum of 1.
    /// </remarks>
    public int FinalDc => Math.Max(1, BaseDc + VisibilityModifier);

    /// <summary>
    /// Gets whether any adjacent rooms are available to scout.
    /// </summary>
    public bool HasAdjacentRooms => AdjacentRoomCount > 0;

    /// <summary>
    /// Gets whether visibility conditions are favorable.
    /// </summary>
    public bool HasGoodVisibility => VisibilityModifier < 0;

    /// <summary>
    /// Gets whether visibility conditions are unfavorable.
    /// </summary>
    public bool HasPoorVisibility => VisibilityModifier > 0;

    /// <summary>
    /// Gets the terrain display name.
    /// </summary>
    public string TerrainDisplayName => TerrainType.GetDisplayName();

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a basic scout context with default modifiers.
    /// </summary>
    /// <param name="playerId">The player performing the scout.</param>
    /// <param name="currentRoomId">The room being scouted from.</param>
    /// <param name="terrainType">The terrain type.</param>
    /// <param name="adjacentRoomIds">IDs of adjacent rooms.</param>
    /// <returns>A new ScoutContext with zero modifiers.</returns>
    public static ScoutContext Create(
        string playerId,
        string currentRoomId,
        NavigationTerrainType terrainType,
        IReadOnlyList<string> adjacentRoomIds)
    {
        return new ScoutContext(
            PlayerId: playerId,
            CurrentRoomId: currentRoomId,
            TerrainType: terrainType,
            AdjacentRoomIds: adjacentRoomIds,
            EquipmentBonus: 0,
            VisibilityModifier: 0);
    }

    /// <summary>
    /// Creates a scout context with equipment and visibility modifiers.
    /// </summary>
    /// <param name="playerId">The player performing the scout.</param>
    /// <param name="currentRoomId">The room being scouted from.</param>
    /// <param name="terrainType">The terrain type.</param>
    /// <param name="adjacentRoomIds">IDs of adjacent rooms.</param>
    /// <param name="equipmentBonus">Bonus from survival equipment.</param>
    /// <param name="visibilityModifier">DC modifier from visibility conditions.</param>
    /// <returns>A new ScoutContext with specified modifiers.</returns>
    public static ScoutContext CreateWithModifiers(
        string playerId,
        string currentRoomId,
        NavigationTerrainType terrainType,
        IReadOnlyList<string> adjacentRoomIds,
        int equipmentBonus,
        int visibilityModifier)
    {
        return new ScoutContext(
            PlayerId: playerId,
            CurrentRoomId: currentRoomId,
            TerrainType: terrainType,
            AdjacentRoomIds: adjacentRoomIds,
            EquipmentBonus: equipmentBonus,
            VisibilityModifier: visibilityModifier);
    }

    /// <summary>
    /// Creates an empty context for testing purposes.
    /// </summary>
    /// <param name="playerId">The player ID.</param>
    /// <returns>A ScoutContext with no adjacent rooms.</returns>
    public static ScoutContext Empty(string playerId)
    {
        return Create(
            playerId: playerId,
            currentRoomId: "test-room",
            terrainType: NavigationTerrainType.ModerateRuins,
            adjacentRoomIds: Array.Empty<string>());
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STRING FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a display string for this scout context.
    /// </summary>
    /// <returns>A formatted string suitable for display.</returns>
    public string ToDisplayString()
    {
        var visibilityStr = VisibilityModifier switch
        {
            < -1 => "excellent",
            -1 => "good",
            0 => "normal",
            <= 2 => "poor",
            <= 4 => "terrible",
            _ => "obscured"
        };

        return $"Scouting {TerrainDisplayName} (DC {FinalDc})\n" +
               $"  Visibility: {visibilityStr}\n" +
               $"  Adjacent rooms: {AdjacentRoomCount}\n" +
               $"  Equipment bonus: {EquipmentBonus:+#;-#;0}";
    }

    /// <summary>
    /// Creates a detailed string for logging purposes.
    /// </summary>
    /// <returns>A multi-line detailed string.</returns>
    public string ToDetailedString()
    {
        return $"ScoutContext\n" +
               $"  Player: {PlayerId}\n" +
               $"  Current Room: {CurrentRoomId}\n" +
               $"  Terrain: {TerrainType} (Base DC {BaseDc})\n" +
               $"  Visibility Modifier: {VisibilityModifier:+#;-#;0}\n" +
               $"  Final DC: {FinalDc}\n" +
               $"  Equipment Bonus: {EquipmentBonus:+#;-#;0}\n" +
               $"  Adjacent Rooms: {AdjacentRoomCount} ({string.Join(", ", AdjacentRoomIds ?? Array.Empty<string>())})";
    }

    /// <summary>
    /// Returns a human-readable summary of the scout context.
    /// </summary>
    /// <returns>A formatted string describing the context.</returns>
    public override string ToString() =>
        $"Scout from {CurrentRoomId} ({TerrainDisplayName}, DC {FinalDc}, {AdjacentRoomCount} adjacent)";
}
