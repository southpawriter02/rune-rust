using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.31.2: Reality Tear Service
/// Handles Reality Tear hazards in Alfheim biome.
/// Processes positional warping, Corruption application, and Energy damage.
///
/// Responsibilities:
/// - Detect character entry into Reality Tear tiles
/// - Warp character to random valid tile (3-5 tiles away)
/// - Apply Energy damage (2d8)
/// - Apply Corruption (+5)
/// - Apply [Dazed] status (1 turn)
/// </summary>
public class RealityTearService
{
    private static readonly ILogger _log = Log.ForContext<RealityTearService>();
    private readonly DiceService _diceService;
    private readonly PositioningService? _positioningService;

    /// <summary>
    /// v0.31.2 canonical: Corruption applied per Reality Tear encounter
    /// </summary>
    private const int CORRUPTION_PER_TEAR = 5;

    /// <summary>
    /// v0.31.2 canonical: Energy damage dice on warp
    /// </summary>
    private const string DAMAGE_DICE = "2d8";

    /// <summary>
    /// v0.31.2 canonical: Minimum warp distance in tiles
    /// </summary>
    private const int WARP_DISTANCE_MIN = 3;

    /// <summary>
    /// v0.31.2 canonical: Maximum warp distance in tiles
    /// </summary>
    private const int WARP_DISTANCE_MAX = 5;

    /// <summary>
    /// v0.31.2 canonical: Dazed status duration after warp
    /// </summary>
    private const int DAZED_DURATION = 1;

    public RealityTearService(
        DiceService? diceService = null,
        PositioningService? positioningService = null)
    {
        _diceService = diceService ?? new DiceService();
        _positioningService = positioningService;
        _log.Information("RealityTearService initialized");
    }

    #region Reality Tear Processing

    /// <summary>
    /// Process character entering Reality Tear.
    /// Warps position, applies damage, Corruption, and Dazed status.
    /// </summary>
    public RealityTearResult ProcessRealityTearEncounter(
        PlayerCharacter character,
        GridPosition tearPosition,
        BattlefieldGrid grid)
    {
        _log.Information(
            "{Character} entered Reality Tear at ({Column}, {Row})",
            character.Name, tearPosition.Column, tearPosition.Row);

        var result = new RealityTearResult
        {
            CharacterName = character.Name,
            OriginalPosition = tearPosition
        };

        // Apply Energy damage
        var damageRoll = _diceService.RollDice(2, 8); // 2d8
        var actualDamage = ApplyEnergyDamage(character, damageRoll);

        result.EnergyDamage = actualDamage;
        result.RemainingHP = character.HP;

        // Apply Corruption
        ApplyCorruption(character, CORRUPTION_PER_TEAR);
        result.CorruptionApplied = CORRUPTION_PER_TEAR;
        result.TotalCorruption = character.Corruption;

        // Warp position
        var newPosition = SelectWarpDestination(tearPosition, grid);
        result.WarpedPosition = newPosition;

        // Update character position
        if (character.Position != null)
        {
            character.Position = newPosition;
        }

        // Apply Dazed status
        // TODO: Apply [Dazed] status when status effect system is implemented
        result.DazedApplied = true;
        result.DazedDuration = DAZED_DURATION;

        // Build narrative message
        result.Message = BuildTearMessage(character.Name, tearPosition, newPosition, actualDamage);

        _log.Information(
            "Reality Tear: {Character} warped from ({OldColumn}, {OldRow}) to ({NewColumn}, {NewRow}), " +
            "took {Damage} Energy damage, gained +{Corruption} Corruption (total: {TotalCorruption})",
            character.Name, tearPosition.Column, tearPosition.Row,
            newPosition.Column, newPosition.Row, actualDamage, CORRUPTION_PER_TEAR, character.Corruption);

        return result;
    }

    /// <summary>
    /// Process enemy entering Reality Tear.
    /// Warps position and applies damage (enemies don't have Corruption).
    /// </summary>
    public RealityTearResult ProcessRealityTearEncounter(
        Enemy enemy,
        GridPosition tearPosition,
        BattlefieldGrid grid)
    {
        _log.Information(
            "{Enemy} entered Reality Tear at ({Column}, {Row})",
            enemy.Name, tearPosition.Column, tearPosition.Row);

        var result = new RealityTearResult
        {
            CharacterName = enemy.Name,
            OriginalPosition = tearPosition
        };

        // Apply Energy damage
        var damageRoll = _diceService.RollDice(2, 8); // 2d8
        enemy.HP = Math.Max(0, enemy.HP - damageRoll);

        result.EnergyDamage = damageRoll;
        result.RemainingHP = enemy.HP;

        // Enemies don't have Corruption
        result.CorruptionApplied = 0;
        result.TotalCorruption = 0;

        // Warp position
        var newPosition = SelectWarpDestination(tearPosition, grid);
        result.WarpedPosition = newPosition;

        if (enemy.Position != null)
        {
            enemy.Position = newPosition;
        }

        // Apply Dazed status
        result.DazedApplied = true;
        result.DazedDuration = DAZED_DURATION;

        result.Message = BuildTearMessage(enemy.Name, tearPosition, newPosition, damageRoll);

        _log.Information(
            "Reality Tear: {Enemy} warped from ({OldColumn}, {OldRow}) to ({NewColumn}, {NewRow}), " +
            "took {Damage} Energy damage",
            enemy.Name, tearPosition.Column, tearPosition.Row,
            newPosition.Column, newPosition.Row, damageRoll);

        return result;
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Select random valid tile for warp destination (3-5 tiles away).
    /// Uses Manhattan distance.
    /// </summary>
    private GridPosition SelectWarpDestination(
        GridPosition originTile,
        BattlefieldGrid grid)
    {
        // Find all valid warp destinations
        var validTiles = grid.Tiles.Values
            .Where(t => t.IsPassable() && !t.IsOccupied)
            .Where(t =>
            {
                var distance = CalculateManhattanDistance(
                    originTile.Column, (int)originTile.Row, t.Position.Column, (int)t.Position.Row);
                return distance >= WARP_DISTANCE_MIN && distance <= WARP_DISTANCE_MAX;
            })
            .ToList();

        if (!validTiles.Any())
        {
            _log.Warning("No valid warp destinations found within range {Min}-{Max}, staying in place",
                WARP_DISTANCE_MIN, WARP_DISTANCE_MAX);
            return originTile;
        }

        // Randomly select from valid destinations
        var chosenIndex = _diceService.Roll(0, validTiles.Count - 1);
        var chosenTile = validTiles[chosenIndex];

        _log.Debug("Selected warp destination: ({Column}, {Row}) from {Count} valid options",
            chosenTile.Position.Column, chosenTile.Position.Row, validTiles.Count);

        return chosenTile.Position;
    }

    /// <summary>
    /// Calculate Manhattan distance between two grid positions.
    /// </summary>
    private int CalculateManhattanDistance(int x1, int y1, int x2, int y2)
    {
        return Math.Abs(x2 - x1) + Math.Abs(y2 - y1);
    }

    /// <summary>
    /// Apply Energy damage to character.
    /// Returns actual damage dealt after resistances.
    /// </summary>
    private int ApplyEnergyDamage(PlayerCharacter character, int baseDamage)
    {
        // TODO: Apply Energy resistance when resistance system is implemented
        // For now, apply full damage
        var actualDamage = baseDamage;

        character.HP = Math.Max(0, character.HP - actualDamage);

        _log.Debug("{Character} took {Damage} Energy damage from Reality Tear (HP: {HP}/{MaxHP})",
            character.Name, actualDamage, character.HP, character.MaxHP);

        return actualDamage;
    }

    /// <summary>
    /// Apply Corruption to character.
    /// </summary>
    private void ApplyCorruption(PlayerCharacter character, int corruption)
    {
        character.Corruption += corruption;

        _log.Debug("{Character} gained +{Corruption} Corruption from Reality Tear (total: {Total})",
            character.Name, corruption, character.Corruption);
    }

    /// <summary>
    /// Build narrative message for Reality Tear encounter.
    /// </summary>
    private string BuildTearMessage(
        string characterName,
        GridPosition oldPos,
        GridPosition newPos,
        int damage)
    {
        var distance = CalculateManhattanDistance(
            oldPos.Column, (int)oldPos.Row, newPos.Column, (int)newPos.Row);

        return $"🌀 Reality Tear!\n" +
               $"   {characterName} steps into a spacetime rupture\n" +
               $"   Physics failure: Warped {distance} tiles ({oldPos} → {newPos})\n" +
               $"   Quantum flux: {damage} Energy damage ({DAMAGE_DICE})\n" +
               $"   [Dazed] for {DAZED_DURATION} turn (reality reassertion trauma)\n" +
               $"   The Aetheric field inverts around you - when coherence returns, you are elsewhere.";
    }

    #endregion

    #region Public Helpers

    /// <summary>
    /// Check if a tile is a Reality Tear.
    /// </summary>
    public bool IsRealityTear(BattlefieldTile tile)
    {
        // Check if tile has Reality Tear environmental feature
        return tile.HasEnvironmentalFeature("Reality Tear");
    }

    /// <summary>
    /// Get Reality Tear damage roll (for preview/information).
    /// </summary>
    public string GetDamageDice()
    {
        return DAMAGE_DICE;
    }

    #endregion
}

#region Data Transfer Objects

/// <summary>
/// Result of a Reality Tear encounter
/// </summary>
public class RealityTearResult
{
    public string CharacterName { get; set; } = string.Empty;
    public GridPosition OriginalPosition { get; set; }
    public GridPosition WarpedPosition { get; set; }
    public int EnergyDamage { get; set; }
    public int RemainingHP { get; set; }
    public int CorruptionApplied { get; set; }
    public int TotalCorruption { get; set; }
    public bool DazedApplied { get; set; }
    public int DazedDuration { get; set; }
    public string Message { get; set; } = string.Empty;
}

#endregion
