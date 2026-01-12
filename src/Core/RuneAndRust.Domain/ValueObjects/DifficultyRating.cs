using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the difficulty rating of a room or area.
/// </summary>
/// <remarks>
/// Difficulty is calculated from multiple factors:
/// - Depth (Z-level): Deeper = harder
/// - Distance from start: Farther = harder
/// - Room type: Special rooms modify difficulty
///
/// The effective level determines monster tiers, level bonuses, and loot quality.
/// </remarks>
public readonly record struct DifficultyRating
{
    /// <summary>
    /// Gets the base difficulty level (1-100).
    /// </summary>
    /// <remarks>
    /// Calculated from depth and distance contributions before room type modifier.
    /// </remarks>
    public int Level { get; init; }

    /// <summary>
    /// Gets the depth contribution (Z-level).
    /// </summary>
    public int DepthFactor { get; init; }

    /// <summary>
    /// Gets the distance contribution (Manhattan distance from start).
    /// </summary>
    public int DistanceFactor { get; init; }

    /// <summary>
    /// Gets the room type modifier.
    /// </summary>
    /// <remarks>
    /// Multiplier applied to base level:
    /// - Treasure: 1.5x (harder guardians)
    /// - Boss: 2.0x (challenging encounters)
    /// - Trap: 1.2x (dangerous)
    /// - Safe: 0.0x (no combat)
    /// - Shrine: 1.1x (slightly harder)
    /// - Standard: 1.0x (no modifier)
    /// </remarks>
    public float RoomTypeModifier { get; init; }

    /// <summary>
    /// Gets the effective difficulty after all modifiers.
    /// </summary>
    public int EffectiveLevel => RoomTypeModifier > 0
        ? Math.Clamp((int)(Level * RoomTypeModifier), 1, 100)
        : 0;

    /// <summary>
    /// Gets the suggested monster tier based on effective difficulty.
    /// </summary>
    /// <remarks>
    /// Tier thresholds:
    /// - Level 1-14: common
    /// - Level 15-34: named
    /// - Level 35-59: elite
    /// - Level 60+: boss
    /// </remarks>
    public string SuggestedMonsterTier => EffectiveLevel switch
    {
        0 => "none",
        < 15 => "common",
        < 35 => "named",
        < 60 => "elite",
        _ => "boss"
    };

    /// <summary>
    /// Gets the loot quality multiplier.
    /// </summary>
    /// <remarks>
    /// Formula: 1.0 + (EffectiveLevel × 0.02)
    /// - Level 1: 1.02x
    /// - Level 50: 2.0x
    /// - Level 100: 3.0x
    /// </remarks>
    public float LootQualityMultiplier => 1.0f + (EffectiveLevel * 0.02f);

    /// <summary>
    /// Gets the monster level bonus added to base monster level.
    /// </summary>
    /// <remarks>
    /// Formula: EffectiveLevel / 10
    /// - Level 10-19: +1
    /// - Level 20-29: +2
    /// - Level 50-59: +5
    /// </remarks>
    public int MonsterLevelBonus => EffectiveLevel / 10;

    /// <summary>
    /// Gets whether this is a combat-enabled room.
    /// </summary>
    public bool HasCombat => RoomTypeModifier > 0;

    /// <summary>
    /// Creates a default difficulty rating for the starting area.
    /// </summary>
    public static DifficultyRating Starting => new()
    {
        Level = 1,
        DepthFactor = 0,
        DistanceFactor = 0,
        RoomTypeModifier = 1.0f
    };

    /// <summary>
    /// Calculates difficulty from position and room type.
    /// </summary>
    /// <param name="position">The room's 3D position.</param>
    /// <param name="startPosition">The dungeon starting position.</param>
    /// <param name="roomType">The type of room.</param>
    /// <param name="rules">The scaling rules configuration.</param>
    /// <returns>A calculated DifficultyRating.</returns>
    public static DifficultyRating Calculate(
        Position3D position,
        Position3D startPosition,
        RoomType roomType,
        ScalingRules? rules = null)
    {
        rules ??= ScalingRules.Default;

        var depth = Math.Abs(position.Z);
        var distance = ManhattanDistance(position, startPosition);

        var depthContribution = (int)(depth * rules.DepthMultiplier);
        var distanceContribution = (int)(distance * rules.DistanceMultiplier);
        var baseLevel = rules.BaseDifficulty + depthContribution + distanceContribution;

        var typeModifier = GetRoomTypeModifier(roomType, rules);

        return new DifficultyRating
        {
            Level = Math.Clamp(baseLevel, 1, 100),
            DepthFactor = depth,
            DistanceFactor = distance,
            RoomTypeModifier = typeModifier
        };
    }

    /// <summary>
    /// Gets the room type modifier from rules.
    /// </summary>
    private static float GetRoomTypeModifier(RoomType roomType, ScalingRules rules) =>
        roomType switch
        {
            RoomType.Treasure => rules.TreasureRoomModifier,
            RoomType.Boss => rules.BossRoomModifier,
            RoomType.Trap => rules.TrapRoomModifier,
            RoomType.Safe => 0.0f, // No combat in safe rooms
            RoomType.Shrine => rules.ShrineRoomModifier,
            _ => 1.0f
        };

    /// <summary>
    /// Calculates Manhattan distance between two 3D positions.
    /// </summary>
    private static int ManhattanDistance(Position3D a, Position3D b) =>
        Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) + Math.Abs(a.Z - b.Z);

    /// <inheritdoc/>
    public override string ToString() =>
        $"Difficulty[Level={Level}, Effective={EffectiveLevel}, Tier={SuggestedMonsterTier}]";
}
