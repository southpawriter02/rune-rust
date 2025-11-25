using RuneAndRust.Core;
using RuneAndRust.Core.AI;
using RuneAndRust.Persistence;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// Damage types for combat
/// </summary>
public enum DamageType
{
    Physical,
    Fire,
    Poison,
    Energy,
    Psychic,
    Ice,
    Lightning
}

/// <summary>
/// Extension methods for BattlefieldTile to support environmental features and terrain
/// </summary>
public static class BattlefieldTileExtensions
{
    private static readonly Dictionary<GridPosition, List<string>> _environmentalFeatures = new();
    private static readonly Dictionary<GridPosition, List<string>> _terrainFeatures = new();

    public static bool HasEnvironmentalFeature(this BattlefieldTile tile, string featureName)
    {
        if (_environmentalFeatures.TryGetValue(tile.Position, out var features))
        {
            return features.Contains(featureName);
        }
        return false;
    }

    public static bool HasTerrain(this BattlefieldTile tile, string terrainName)
    {
        if (_terrainFeatures.TryGetValue(tile.Position, out var features))
        {
            return features.Contains(terrainName);
        }
        return false;
    }

    public static void AddEnvironmentalFeature(this BattlefieldTile tile, string featureName)
    {
        if (!_environmentalFeatures.ContainsKey(tile.Position))
        {
            _environmentalFeatures[tile.Position] = new List<string>();
        }
        if (!_environmentalFeatures[tile.Position].Contains(featureName))
        {
            _environmentalFeatures[tile.Position].Add(featureName);
        }
    }

    public static void RemoveEnvironmentalFeature(this BattlefieldTile tile, string featureName)
    {
        if (_environmentalFeatures.TryGetValue(tile.Position, out var features))
        {
            features.Remove(featureName);
        }
    }

    public static void AddTerrain(this BattlefieldTile tile, string terrainName)
    {
        if (!_terrainFeatures.ContainsKey(tile.Position))
        {
            _terrainFeatures[tile.Position] = new List<string>();
        }
        if (!_terrainFeatures[tile.Position].Contains(terrainName))
        {
            _terrainFeatures[tile.Position].Add(terrainName);
        }
    }

    public static void RemoveTerrain(this BattlefieldTile tile, string terrainName)
    {
        if (_terrainFeatures.TryGetValue(tile.Position, out var features))
        {
            features.Remove(terrainName);
        }
    }
}

/// <summary>
/// Extension methods for combat damage handling
/// </summary>
public static class CombatExtensions
{
    public static void TakeDamage(this object combatant, int damage, DamageType damageType)
    {
        switch (combatant)
        {
            case PlayerCharacter pc:
                pc.HP = Math.Max(0, pc.HP - damage);
                break;
            case Enemy e:
                e.HP = Math.Max(0, e.HP - damage);
                break;
            case Companion c:
                c.CurrentHitPoints = Math.Max(0, c.CurrentHitPoints - damage);
                break;
        }
    }

    public static string GetName(this object combatant)
    {
        return combatant switch
        {
            PlayerCharacter pc => pc.Name,
            Enemy e => e.Name,
            Companion c => c.DisplayName,
            _ => "Unknown"
        };
    }

    public static GridPosition? GetPosition(this object combatant)
    {
        return combatant switch
        {
            PlayerCharacter pc => null, // Position stored externally
            Enemy e => e.Position,
            Companion c => c.Position,
            _ => null
        };
    }

    public static void SetPosition(this object combatant, GridPosition? position)
    {
        switch (combatant)
        {
            case Enemy e:
                e.Position = position;
                break;
            case Companion c:
                c.Position = position;
                break;
        }
    }
}

/// <summary>
/// v0.32.2: Orchestration service for Jötunheim biome.
/// Coordinates industrial hazards, environmental terrain, and Jötun corpse mechanics.
/// NO AMBIENT CONDITION - threats are physical and technological.
/// </summary>
public class JotunheimBiomeService
{
    private static readonly ILogger _log = Log.ForContext<JotunheimBiomeService>();

    private readonly JotunheimDataRepository _dataRepository;
    private readonly PowerConduitService _powerConduitService;
    private readonly DiceService _diceService;
    private readonly EnvironmentalObjectService _environmentalObjectService;
    private readonly TraumaEconomyService _traumaEconomyService;

    public JotunheimBiomeService(
        JotunheimDataRepository dataRepository,
        PowerConduitService powerConduitService,
        DiceService diceService,
        EnvironmentalObjectService environmentalObjectService,
        TraumaEconomyService traumaEconomyService)
    {
        _dataRepository = dataRepository;
        _powerConduitService = powerConduitService;
        _diceService = diceService;
        _environmentalObjectService = environmentalObjectService;
        _traumaEconomyService = traumaEconomyService;

        _log.Information("JotunheimBiomeService initialized - industrial decay biome active");
    }

    #region Environmental Hazard Processing

    /// <summary>
    /// Process all Jötunheim environmental hazards for current turn.
    /// NO AMBIENT CONDITION - only direct hazard effects.
    /// </summary>
    public void ProcessEnvironmentalHazards(BattlefieldState battlefield, int currentTurn)
    {
        _log.Debug("Processing Jötunheim environmental hazards - turn {Turn}", currentTurn);

        // 1. Live Power Conduits (signature hazard)
        _powerConduitService.ProcessPowerConduitsForTurn(battlefield);

        // 2. High-Pressure Steam Vents (every 3 turns)
        ProcessSteamVents(battlefield, currentTurn);

        // 3. Jötun Corpse Terrain (Psychic Stress)
        ProcessJotunProximityStress(battlefield);

        // 4. Flooded Terrain (Poison damage)
        ProcessFloodedTerrain(battlefield);

        // 5. Toxic Haze (Poison damage + vision obscure)
        ProcessToxicHaze(battlefield);
    }

    #endregion

    #region High-Pressure Steam Vents

    private readonly Dictionary<GridPosition, int> _steamVentLastEruption = new();
    private const int STEAM_VENT_CYCLE = 3; // Erupts every 3 turns
    private const int STEAM_VENT_CONE_LENGTH = 3;
    private const string STEAM_VENT_DAMAGE = "2d6";
    private const int STEAM_VENT_KNOCKBACK = 1;

    /// <summary>
    /// Process steam vents. Erupts every 3 turns with warning.
    /// </summary>
    private void ProcessSteamVents(BattlefieldState battlefield, int currentTurn)
    {
        var steamVents = battlefield.Grid.Tiles.Values
            .Where(t => t.HasEnvironmentalFeature("High-Pressure Steam Vent"))
            .ToList();

        foreach (var vent in steamVents)
        {
            if (!_steamVentLastEruption.ContainsKey(vent.Position))
            {
                _steamVentLastEruption[vent.Position] = 0; // Initialize
            }

            var turnsSinceEruption = currentTurn - _steamVentLastEruption[vent.Position];

            // Warning on turn before eruption
            if (turnsSinceEruption == STEAM_VENT_CYCLE - 1)
            {
                _log.Information("⚠️ Steam vent at ({Column}, {Row}) HISSING - will erupt next turn",
                    vent.Position.Column, vent.Position.Row);

                battlefield.CombatLog.Add(
                    $"⚠️ The steam vent at ({vent.Position.Column}, {vent.Position.Row}) hisses loudly - pressure is building!");
            }

            // Eruption on cycle completion
            if (turnsSinceEruption >= STEAM_VENT_CYCLE)
            {
                ProcessSteamVentEruption(vent, battlefield);
                _steamVentLastEruption[vent.Position] = currentTurn;
            }
        }
    }

    private void ProcessSteamVentEruption(BattlefieldTile vent, BattlefieldState battlefield)
    {
        // Get vent facing direction (from special_rules or default)
        var direction = GetVentDirection(vent);

        var affectedTiles = battlefield.Grid.GetConeArea(vent.Position, direction, STEAM_VENT_CONE_LENGTH);
        var affectedCombatants = affectedTiles
            .SelectMany(t => battlefield.Grid.GetCombatantsAtPosition(t))
            .ToList();

        _log.Warning("💨 Steam vent at ({Column}, {Row}) ERUPTS - {Count} combatants affected",
            vent.Position.Column, vent.Position.Row, affectedCombatants.Count);

        battlefield.CombatLog.Add(
            $"💨 Superheated steam ERUPTS from the vent at ({vent.Position.Column}, {vent.Position.Row})!");

        foreach (var combatant in affectedCombatants)
        {
            // Fire damage (2d6)
            var damage = _diceService.RollDice(2, 6);
            combatant.TakeDamage(damage, DamageType.Fire);

            _log.Information("{Combatant} takes {Damage} Fire damage from steam vent eruption",
                combatant.GetName(), damage);

            // Knockback
            var combatantPos = combatant.GetPosition() ?? vent.Position;
            var knockbackDirection = battlefield.Grid.GetDirectionFrom(vent.Position, combatantPos);
            var knockbackDestination = battlefield.Grid.GetPositionInDirection(
                combatantPos, knockbackDirection, STEAM_VENT_KNOCKBACK);

            if (knockbackDestination != null && battlefield.Grid.IsValidPosition(knockbackDestination) &&
                !battlefield.Grid.IsBlocked(knockbackDestination))
            {
                combatant.SetPosition(knockbackDestination);
                _log.Debug("{Combatant} knocked back {Distance} tiles by steam eruption",
                    combatant.GetName(), STEAM_VENT_KNOCKBACK);

                battlefield.CombatLog.Add(
                    $"{combatant.GetName()} is knocked back by explosive force!");
            }

            battlefield.CombatLog.Add(
                $"{combatant.GetName()} takes {damage} Fire damage from superheated steam!");
        }
    }

    private Direction GetVentDirection(BattlefieldTile vent)
    {
        // TODO: Parse from special_rules JSON if needed
        // For now, default to North
        return Direction.North;
    }

    #endregion

    #region Unstable Ceiling/Wall

    private readonly HashSet<GridPosition> _collapsedCeilings = new();
    private const int COLLAPSE_TRIGGER_DAMAGE = 10;
    private const string COLLAPSE_DAMAGE = "3d8";
    private const int COLLAPSE_AREA_SIZE = 2; // 2x2

    /// <summary>
    /// Check if heavy damage triggers ceiling collapse.
    /// Called by combat system when damage exceeds threshold near unstable ceiling.
    /// </summary>
    public bool CheckCeilingCollapse(GridPosition impactPosition, int damageDealt, BattlefieldState battlefield)
    {
        if (damageDealt < COLLAPSE_TRIGGER_DAMAGE)
        {
            return false;
        }

        // Find unstable ceilings within 2 tiles of impact
        var nearbyCeilings = battlefield.Grid.GetTilesInRadius(impactPosition, 2)
            .Select(pos => battlefield.Grid.GetTile(pos))
            .Where(t => t != null && t.HasEnvironmentalFeature("Unstable Ceiling/Wall"))
            .Where(t => !_collapsedCeilings.Contains(t!.Position)) // Not already collapsed
            .ToList();

        if (!nearbyCeilings.Any())
        {
            return false;
        }

        // Trigger first nearby unstable ceiling
        var ceiling = nearbyCeilings.First()!;
        ProcessCeilingCollapse(ceiling, battlefield);

        return true;
    }

    private void ProcessCeilingCollapse(BattlefieldTile ceiling, BattlefieldState battlefield)
    {
        _log.Warning("🧱 Unstable ceiling at ({Column}, {Row}) COLLAPSES - heavy impact triggered structural failure",
            ceiling.Position.Column, ceiling.Position.Row);

        // Mark as collapsed (one-time only)
        _collapsedCeilings.Add(ceiling.Position);
        ceiling.RemoveEnvironmentalFeature("Unstable Ceiling/Wall");

        // Calculate 2x2 collapse area
        var affectedPositions = battlefield.Grid.GetAreaAround(ceiling.Position, COLLAPSE_AREA_SIZE);
        var affectedCombatants = affectedPositions
            .SelectMany(pos => battlefield.Grid.GetCombatantsAtPosition(pos))
            .ToList();

        battlefield.CombatLog.Add(
            $"🧱 The ceiling COLLAPSES in a shower of concrete and twisted metal!");

        foreach (var combatant in affectedCombatants)
        {
            var damage = _diceService.RollDice(3, 8); // 3d8
            combatant.TakeDamage(damage, DamageType.Physical);

            _log.Information("{Combatant} takes {Damage} Physical damage from ceiling collapse",
                combatant.GetName(), damage);

            battlefield.CombatLog.Add(
                $"{combatant.GetName()} is struck by falling debris - {damage} Physical damage!");
        }

        // Create [Debris Pile] terrain in affected area
        foreach (var pos in affectedPositions)
        {
            var tile = battlefield.Grid.GetTile(pos);
            if (tile != null)
            {
                tile.AddTerrain("Debris Pile");
                _log.Debug("Created [Debris Pile] at ({Column}, {Row})", pos.Column, pos.Row);
            }
        }

        battlefield.CombatLog.Add(
            "Piles of rubble create difficult terrain and partial cover.");
    }

    #endregion

    #region Jötun Corpse Terrain - Psychic Stress

    /// <summary>
    /// Apply +2 Psychic Stress per turn to characters on Jötun Corpse Terrain.
    /// Corrupted logic core broadcast creates oppressive psychic hum.
    /// </summary>
    private void ProcessJotunProximityStress(BattlefieldState battlefield)
    {
        var jotunCorpseTiles = battlefield.Grid.Tiles.Values
            .Where(t => t.HasTerrain("Jotun Corpse Terrain"))
            .ToList();

        if (!jotunCorpseTiles.Any())
        {
            return;
        }

        foreach (var tile in jotunCorpseTiles)
        {
            var combatants = battlefield.Grid.GetCombatantsAtPosition(tile.Position);

            foreach (var combatant in combatants)
            {
                if (combatant is PlayerCharacter character)
                {
                    _traumaEconomyService.AddStress(character, 2,
                        "Jötun proximity - corrupted logic core broadcast");

                    _log.Debug("{Character} on Jötun corpse terrain - applied +2 Psychic Stress",
                        character.Name);

                    // Only log every 3 turns to avoid spam
                    if (battlefield.CurrentTurn % 3 == 0)
                    {
                        battlefield.CombatLog.Add(
                            $"{character.Name} feels the oppressive hum from the dormant giant (+2 Stress)");
                    }
                }
            }
        }
    }

    #endregion

    #region Flooded Terrain - Poison Damage

    /// <summary>
    /// Apply 1 Poison damage to characters ending turn in flooded coolant.
    /// </summary>
    private void ProcessFloodedTerrain(BattlefieldState battlefield)
    {
        var floodedTiles = battlefield.Grid.Tiles.Values
            .Where(t => t.HasTerrain("Flooded (Coolant)") || t.HasTerrain("Flooded"))
            .ToList();

        foreach (var tile in floodedTiles)
        {
            var combatants = battlefield.Grid.GetCombatantsAtPosition(tile.Position);

            foreach (var combatant in combatants)
            {
                combatant.TakeDamage(1, DamageType.Poison);

                _log.Debug("{Combatant} ends turn in flooded coolant - 1 Poison damage",
                    combatant.GetName());

                if (battlefield.CurrentTurn % 2 == 0) // Every other turn to reduce spam
                {
                    battlefield.CombatLog.Add(
                        $"{combatant.GetName()} wades through toxic coolant (1 Poison damage)");
                }
            }
        }
    }

    #endregion

    #region Toxic Haze - Vision Obscure + Poison

    /// <summary>
    /// Apply 1d4 Poison damage per turn in toxic haze zones.
    /// Also applies Disadvantage to ranged attacks (handled by combat system).
    /// </summary>
    private void ProcessToxicHaze(BattlefieldState battlefield)
    {
        var hazeTiles = battlefield.Grid.Tiles.Values
            .Where(t => t.HasEnvironmentalFeature("Toxic Haze"))
            .ToList();

        foreach (var tile in hazeTiles)
        {
            // Get 4x4 zone
            var hazeZone = battlefield.Grid.GetAreaAround(tile.Position, 4);
            var affectedCombatants = hazeZone
                .SelectMany(pos => battlefield.Grid.GetCombatantsAtPosition(pos))
                .Distinct()
                .ToList();

            foreach (var combatant in affectedCombatants)
            {
                var damage = _diceService.RollDice(1, 4); // 1d4
                combatant.TakeDamage(damage, DamageType.Poison);

                _log.Debug("{Combatant} in toxic haze - {Damage} Poison damage",
                    combatant.GetName(), damage);
            }
        }
    }

    #endregion

    #region Assembly Line - Forced Movement

    private readonly Dictionary<GridPosition, GridDirection> _assemblyLineBelts = new();

    /// <summary>
    /// Process active assembly line conveyor belts.
    /// Moves characters 2 tiles per turn in belt direction.
    /// </summary>
    public void ProcessAssemblyLines(BattlefieldState battlefield)
    {
        var beltTiles = battlefield.Grid.Tiles.Values
            .Where(t => t.HasEnvironmentalFeature("Assembly Line (Active)"))
            .ToList();

        foreach (var belt in beltTiles)
        {
            var combatants = battlefield.Grid.GetCombatantsAtPosition(belt.Position);

            if (!combatants.Any())
            {
                continue;
            }

            // Get belt direction (from special_rules or default) - convert GridDirection to Direction
            var gridDirection = _assemblyLineBelts.GetValueOrDefault(belt.Position, GridDirection.East);
            var direction = gridDirection switch
            {
                GridDirection.North => Direction.North,
                GridDirection.South => Direction.South,
                GridDirection.East => Direction.East,
                GridDirection.West => Direction.West,
                _ => Direction.East
            };

            foreach (var combatant in combatants)
            {
                // Move 2 tiles in belt direction
                var combatantPos = combatant.GetPosition() ?? belt.Position;
                var destination = battlefield.Grid.GetPositionInDirection(combatantPos, direction, 2);

                if (destination != null && battlefield.Grid.IsValidPosition(destination))
                {
                    var oldPosition = combatantPos;
                    combatant.SetPosition(destination);

                    _log.Information("Assembly line moves {Combatant} from ({Column1},{Row1}) to ({Column2},{Row2})",
                        combatant.GetName(), oldPosition.Column, oldPosition.Row, destination.Column, destination.Row);

                    battlefield.CombatLog.Add(
                        $"{combatant.GetName()} is carried {2} tiles by the active conveyor belt");

                    // Check if moved into hazard
                    var destTile = battlefield.Grid.GetTile(destination);
                    if (destTile != null && destTile.HasEnvironmentalFeature("Live Power Conduit"))
                    {
                        // Wrap combatant in Combatant wrapper if needed
                        var combatantWrapper = combatant switch
                        {
                            Combatant c => c,
                            PlayerCharacter pc => new Combatant(pc),
                            Enemy e => new Combatant(e),
                            Companion comp => new Combatant(comp),
                            _ => null
                        };

                        if (combatantWrapper != null)
                        {
                            // Create GridTile wrapper for destTile
                            var gridTile = new GridTile(destTile);
                            _powerConduitService.ProcessForcedMovementIntoConduit(combatantWrapper, gridTile, battlefield);
                        }
                    }
                }
            }
        }
    }

    #endregion

    #region Utility

    /// <summary>
    /// Verify that Jötunheim has NO ambient condition (canonical design).
    /// </summary>
    public bool VerifyNoAmbientCondition()
    {
        var biome = _dataRepository.GetBiomeInfo();
        var hasNoAmbient = biome.AmbientConditionId == null;

        if (!hasNoAmbient)
        {
            _log.Error("INTEGRITY ERROR: Jötunheim has ambient condition set (should be NULL)");
        }

        return hasNoAmbient;
    }

    #endregion

    #region Enemy Generation (v0.32.4)

    /// <summary>
    /// Generate enemy group for Jötunheim encounter.
    /// Uses weighted spawn system with Undying dominance (~60%).
    /// </summary>
    public List<string> GenerateEnemyGroup(int difficulty, string? verticalityTier = null)
    {
        _log.Information("Generating Jötunheim enemy group - difficulty {Difficulty}, tier {Tier}",
            difficulty, verticalityTier ?? "Both");

        var groupSize = difficulty switch
        {
            1 => _diceService.Roll(2, 3), // Easy: 2-3 enemies
            2 => _diceService.Roll(3, 4), // Normal: 3-4 enemies
            3 => _diceService.Roll(4, 5), // Hard: 4-5 enemies
            4 => _diceService.Roll(5, 7), // Deadly: 5-7 enemies
            _ => 3 // Default
        };

        var enemies = new List<string>();

        for (int i = 0; i < groupSize; i++)
        {
            var enemy = RollWeightedEnemy(difficulty, verticalityTier);
            enemies.Add(enemy);
        }

        _log.Information("Generated Jötunheim enemy group: {Count} enemies - {Enemies}",
            enemies.Count, string.Join(", ", enemies));

        return enemies;
    }

    /// <summary>
    /// Roll weighted enemy spawn based on v0.32.3 spawn weights.
    /// Spawn weights: Servitor 200, Warden 150, Cultist 130, Boar 80, Tinker 70, Juggernaut 60
    /// Total: 690 (Undying: 410 = 59.4%, Humanoid: 200 = 29%, Beast: 80 = 11.6%)
    /// </summary>
    private string RollWeightedEnemy(int difficulty, string? verticalityTier)
    {
        // Get weighted spawn table from database
        var spawnWeights = _dataRepository.GetEnemySpawnWeights(verticalityTier);

        if (!spawnWeights.Any())
        {
            _log.Warning("No enemy spawn weights found for tier {Tier}, using default", verticalityTier ?? "Both");
            return "Rusted Servitor"; // Fallback
        }

        // Calculate total weight
        var totalWeight = spawnWeights.Sum(kvp => kvp.Value);

        // Roll weighted selection
        var roll = _diceService.Roll(1, totalWeight);
        var currentWeight = 0;

        foreach (var enemy in spawnWeights.OrderByDescending(kvp => kvp.Value))
        {
            currentWeight += enemy.Value;
            if (roll <= currentWeight)
            {
                _log.Debug("Rolled {Roll}/{Total} - selected {Enemy} (weight {Weight})",
                    roll, totalWeight, enemy.Key, enemy.Value);
                return enemy.Key;
            }
        }

        // Fallback (should never reach)
        _log.Warning("Weighted enemy selection fell through - using fallback");
        return spawnWeights.First().Key;
    }

    /// <summary>
    /// Get enemy spawn distribution for current battlefield.
    /// Used for analytics and balancing verification.
    /// </summary>
    public JotunheimEnemyDistribution GetEnemyDistribution()
    {
        return _dataRepository.GetEnemyDistribution();
    }

    /// <summary>
    /// Get all available enemy types for Jötunheim.
    /// </summary>
    public List<JotunheimEnemy> GetAvailableEnemies(string? verticalityTier = null)
    {
        return _dataRepository.GetEnemySpawns(verticalityTier: verticalityTier);
    }

    #endregion
}

#region Data Transfer Objects

public class JotunheimHazardReport
{
    public int PowerConduitsActive { get; set; }
    public int PowerConduitsFlooded { get; set; }
    public int SteamVentsActive { get; set; }
    public int UnstableCeilingsRemaining { get; set; }
    public int JotunCorpseTerrainTiles { get; set; }
    public int FloodedTiles { get; set; }
    public int ToxicHazeZones { get; set; }
    public bool AmbientConditionPresent { get; set; }

    public override string ToString()
    {
        return $"Jötunheim Hazards: {PowerConduitsActive} conduits ({PowerConduitsFlooded} flooded), " +
               $"{SteamVentsActive} vents, {UnstableCeilingsRemaining} unstable ceilings, " +
               $"NO ambient condition: {!AmbientConditionPresent}";
    }
}

#endregion
