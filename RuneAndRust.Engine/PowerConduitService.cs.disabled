using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.32.2: Service for [Live Power Conduit] signature hazard in Jötunheim.
/// Handles electrical damage with flooded terrain amplification.
/// </summary>
public class PowerConduitService
{
    private static readonly ILogger _log = Log.ForContext<PowerConduitService>();

    private readonly DiceService _diceService;
    private readonly EnvironmentalObjectService _environmentalObjectService;

    private const string BASE_DAMAGE = "1d8";
    private const string FLOODED_DAMAGE = "2d10";
    private const int FLOODED_RADIUS = 2;
    private const int CONDUIT_HP = 15;

    public PowerConduitService(
        DiceService diceService,
        EnvironmentalObjectService environmentalObjectService)
    {
        _diceService = diceService;
        _environmentalObjectService = environmentalObjectService;

        _log.Information("PowerConduitService initialized");
    }

    #region Power Conduit Damage Processing

    /// <summary>
    /// Process all Live Power Conduits on the battlefield.
    /// Checks for flooded terrain amplification.
    /// </summary>
    public void ProcessPowerConduitsForTurn(BattlefieldState battlefield)
    {
        var conduitTiles = battlefield.Grid.Tiles
            .Where(t => t.HasEnvironmentalFeature("Live Power Conduit"))
            .ToList();

        if (!conduitTiles.Any())
        {
            return;
        }

        _log.Information("Processing {Count} Live Power Conduits", conduitTiles.Count);

        foreach (var conduitTile in conduitTiles)
        {
            // Check if conduit is in flooded terrain
            var isFlooded = conduitTile.HasTerrain("Flooded (Coolant)") || conduitTile.HasTerrain("Flooded");

            if (isFlooded)
            {
                ProcessFloodedConduit(conduitTile, battlefield);
            }
            else
            {
                ProcessStandardConduit(conduitTile, battlefield);
            }
        }
    }

    /// <summary>
    /// Process standard power conduit (1d8 Energy damage, adjacent only).
    /// </summary>
    private void ProcessStandardConduit(GridTile conduitTile, BattlefieldState battlefield)
    {
        var adjacentCombatants = battlefield.Grid.GetAdjacentCombatants(conduitTile.Position);

        if (!adjacentCombatants.Any())
        {
            return;
        }

        _log.Debug("Standard conduit at ({X}, {Y}) affecting {Count} adjacent combatants",
            conduitTile.Position.X, conduitTile.Position.Y, adjacentCombatants.Count);

        foreach (var combatant in adjacentCombatants)
        {
            var damage = _diceService.RollDice(BASE_DAMAGE);
            combatant.TakeDamage(damage, DamageType.Energy);

            _log.Information("{Combatant} takes {Damage} Energy damage from Live Power Conduit at ({X}, {Y})",
                combatant.Name, damage, conduitTile.Position.X, conduitTile.Position.Y);

            // Apply message to combat log
            battlefield.CombatLog.Add(
                $"{combatant.Name} takes {damage} Energy damage from [Live Power Conduit]");
        }
    }

    /// <summary>
    /// Process flooded power conduit (2d10 Energy damage, 2-tile radius).
    /// AMPLIFIED: Conductive coolant fluid spreads electrical discharge.
    /// </summary>
    private void ProcessFloodedConduit(GridTile conduitTile, BattlefieldState battlefield)
    {
        var affectedCombatants = battlefield.Grid.GetCombatantsInRadius(conduitTile.Position, FLOODED_RADIUS);

        if (!affectedCombatants.Any())
        {
            return;
        }

        _log.Warning("FLOODED conduit at ({X}, {Y}) AMPLIFIED - affecting {Count} combatants in {Radius}-tile radius",
            conduitTile.Position.X, conduitTile.Position.Y, affectedCombatants.Count, FLOODED_RADIUS);

        foreach (var combatant in affectedCombatants)
        {
            var distance = battlefield.Grid.GetDistance(conduitTile.Position, combatant.Position);
            var damage = _diceService.RollDice(FLOODED_DAMAGE);
            combatant.TakeDamage(damage, DamageType.Energy);

            _log.Warning("AMPLIFIED: {Combatant} (distance {Distance}) takes {Damage} Energy damage from flooded conduit",
                combatant.Name, distance, damage);

            // Dramatic combat log entry for amplified damage
            battlefield.CombatLog.Add(
                $"⚡ AMPLIFIED: {combatant.Name} takes {damage} Energy damage as electricity arcs through conductive coolant!");
        }
    }

    #endregion

    #region Forced Movement into Conduits

    /// <summary>
    /// Apply double damage when a character is forced into a power conduit.
    /// Called by ForcedMovementService when destination has [Live Power Conduit].
    /// </summary>
    public void ProcessForcedMovementIntoConduit(Combatant target, GridTile destinationTile, BattlefieldState battlefield)
    {
        if (!destinationTile.HasEnvironmentalFeature("Live Power Conduit"))
        {
            return;
        }

        // Double damage from forced contact with conduit
        var baseDamage = _diceService.RollDice(BASE_DAMAGE);
        var actualDamage = baseDamage * 2;

        target.TakeDamage(actualDamage, DamageType.Energy);

        _log.Warning("{Target} forced into Live Power Conduit - takes {Damage} Energy damage (DOUBLED from {Base})",
            target.Name, actualDamage, baseDamage);

        battlefield.CombatLog.Add(
            $"⚡ {target.Name} is FORCED into [Live Power Conduit] - {actualDamage} Energy damage (direct contact)!");
    }

    #endregion

    #region Conduit Destruction

    /// <summary>
    /// Attempt to destroy a power conduit (15 HP).
    /// Returns true if destroyed.
    /// </summary>
    public bool AttemptDestroyConduit(GridTile conduitTile, int damageDealt, BattlefieldState battlefield)
    {
        if (!conduitTile.HasEnvironmentalFeature("Live Power Conduit"))
        {
            return false;
        }

        // Track conduit HP (simplified - using EnvironmentalObjectService)
        var currentHP = _environmentalObjectService.GetObjectHP(conduitTile.Position) ?? CONDUIT_HP;
        var newHP = currentHP - damageDealt;

        _log.Debug("Power conduit at ({X}, {Y}) takes {Damage} damage ({Current} HP -> {New} HP)",
            conduitTile.Position.X, conduitTile.Position.Y, damageDealt, currentHP, newHP);

        if (newHP <= 0)
        {
            // Conduit destroyed
            conduitTile.RemoveEnvironmentalFeature("Live Power Conduit");
            _environmentalObjectService.RemoveObject(conduitTile.Position);

            _log.Information("Power conduit at ({X}, {Y}) DESTROYED",
                conduitTile.Position.X, conduitTile.Position.Y);

            battlefield.CombatLog.Add(
                $"The [Live Power Conduit] sparks violently and goes dark - destroyed!");

            return true;
        }
        else
        {
            // Conduit damaged but still active
            _environmentalObjectService.SetObjectHP(conduitTile.Position, newHP);

            battlefield.CombatLog.Add(
                $"The [Live Power Conduit] crackles - damaged but still conducting ({newHP}/{CONDUIT_HP} HP)");

            return false;
        }
    }

    #endregion

    #region Flooded Terrain Interaction

    /// <summary>
    /// Check if a power conduit is in flooded terrain.
    /// Logs warning for dangerous combo.
    /// </summary>
    public bool IsConduitFlooded(GridTile conduitTile)
    {
        var isFlooded = conduitTile.HasTerrain("Flooded (Coolant)") || conduitTile.HasTerrain("Flooded");

        if (isFlooded)
        {
            _log.Warning("DANGEROUS COMBO: Live Power Conduit in flooded terrain at ({X}, {Y}) - amplified damage active",
                conduitTile.Position.X, conduitTile.Position.Y);
        }

        return isFlooded;
    }

    /// <summary>
    /// Apply flooded terrain to a tile containing a power conduit.
    /// Triggers amplification effect.
    /// </summary>
    public void ApplyFloodingToConduit(GridTile conduitTile, BattlefieldState battlefield)
    {
        if (!conduitTile.HasEnvironmentalFeature("Live Power Conduit"))
        {
            return;
        }

        conduitTile.AddTerrain("Flooded (Coolant)");

        _log.Warning("⚡ Power conduit at ({X}, {Y}) now FLOODED - AMPLIFICATION ACTIVE (1d8 → 2d10, radius 1 → 2)",
            conduitTile.Position.X, conduitTile.Position.Y);

        battlefield.CombatLog.Add(
            "⚡ WARNING: Coolant floods the power conduit area - electrical discharge AMPLIFIED!");
    }

    #endregion

    #region Utility

    /// <summary>
    /// Get damage dice for a conduit (standard or flooded).
    /// </summary>
    public string GetConduitDamageDice(GridTile conduitTile)
    {
        return IsConduitFlooded(conduitTile) ? FLOODED_DAMAGE : BASE_DAMAGE;
    }

    /// <summary>
    /// Get effective radius for a conduit (standard or flooded).
    /// </summary>
    public int GetConduitRadius(GridTile conduitTile)
    {
        return IsConduitFlooded(conduitTile) ? FLOODED_RADIUS : 1;
    }

    #endregion
}

#region Data Transfer Objects

/// <summary>
/// Report of power conduit status on battlefield.
/// </summary>
public class PowerConduitReport
{
    public int TotalConduits { get; set; }
    public int FloodedConduits { get; set; }
    public int StandardConduits { get; set; }
    public List<ConduitStatus> ConduitDetails { get; set; } = new();

    public override string ToString()
    {
        return $"Power Conduits: {TotalConduits} total ({FloodedConduits} flooded, {StandardConduits} standard)";
    }
}

public class ConduitStatus
{
    public GridPosition Position { get; set; }
    public bool IsFlooded { get; set; }
    public int CurrentHP { get; set; }
    public int AffectedCombatants { get; set; }
    public string DamageDice { get; set; } = string.Empty;
    public int EffectiveRadius { get; set; }
}

#endregion
