using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Result of executing Runic Barrier (29011): creates a protective structure at a location.
/// Rank 1: 30 HP / 2 turns. Rank 2: 40 HP / 3 turns. Rank 3: 50 HP / 4 turns. Costs 3 AP.
/// </summary>
public sealed record RunicBarrierResult
{
    /// <summary>Whether the ability executed successfully.</summary>
    public bool IsSuccess { get; init; }

    /// <summary>Human-readable description of the ability outcome.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>AP spent to execute this ability.</summary>
    public int AetherSpent { get; init; }

    /// <summary>The HP of the created barrier.</summary>
    public int BarrierHp { get; init; }

    /// <summary>The duration in turns of the created barrier.</summary>
    public int Duration { get; init; }

    /// <summary>X coordinate where the barrier was placed.</summary>
    public int PositionX { get; init; }

    /// <summary>Y coordinate where the barrier was placed.</summary>
    public int PositionY { get; init; }

    /// <summary>Unique identifier of the created barrier.</summary>
    public Guid BarrierId { get; init; }

    /// <summary>Ability rank at time of cast.</summary>
    public int AbilityRank { get; init; }

    /// <summary>Gets a formatted description for combat log.</summary>
    public string GetDescription()
    {
        return $"Runic Barrier created at ({PositionX}, {PositionY}) with {BarrierHp} HP, duration {Duration} turns";
    }
}

/// <summary>
/// Result of executing Consecrate Ground (29012): creates a healing/damage zone.
/// Healing: R1 = 1d6, R2 = 1d6+2, R3 = 2d6 per turn. Damage same to Blighted/Undying. Costs 3 AP.
/// </summary>
public sealed record ConsecrateGroundResult
{
    /// <summary>Whether the ability executed successfully.</summary>
    public bool IsSuccess { get; init; }

    /// <summary>Human-readable description of the ability outcome.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>AP spent to execute this ability.</summary>
    public int AetherSpent { get; init; }

    /// <summary>Healing applied per turn to allies in the zone.</summary>
    public int HealPerTurn { get; init; }

    /// <summary>Damage applied per turn to Blighted/Undying enemies in the zone.</summary>
    public int DamagePerTurn { get; init; }

    /// <summary>The duration in turns of the consecrated zone.</summary>
    public int Duration { get; init; }

    /// <summary>The radius of the zone's effect (in tiles).</summary>
    public int Radius { get; init; }

    /// <summary>X coordinate where the zone was centered.</summary>
    public int PositionX { get; init; }

    /// <summary>Y coordinate where the zone was centered.</summary>
    public int PositionY { get; init; }

    /// <summary>Unique identifier of the created zone.</summary>
    public Guid ZoneId { get; init; }

    /// <summary>Ability rank at time of cast.</summary>
    public int AbilityRank { get; init; }

    /// <summary>Gets a formatted description for combat log.</summary>
    public string GetDescription()
    {
        return $"Consecrate Ground zone created at ({PositionX}, {PositionY}) radius {Radius}: " +
               $"{HealPerTurn} heal / {DamagePerTurn} damage per turn, {Duration} turns";
    }
}

/// <summary>
/// Result of executing Rune of Shielding (29013): buffs an ally with Soak and corruption resistance.
/// Soak: R1 = +3, R2 = +5, R3 = +7. Corruption resist: R1 = +10%, R2 = +15%, R3 = +20%. Duration: 4 turns. Costs 3 AP.
/// </summary>
public sealed record RuneOfShieldingResult
{
    /// <summary>Whether the ability executed successfully.</summary>
    public bool IsSuccess { get; init; }

    /// <summary>Human-readable description of the ability outcome.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>AP spent to execute this ability.</summary>
    public int AetherSpent { get; init; }

    /// <summary>Bonus to Soak granted to the target.</summary>
    public int SoakBonus { get; init; }

    /// <summary>Bonus to Corruption resistance granted as a percentage.</summary>
    public int CorruptionResistBonusPercent { get; init; }

    /// <summary>Name or identifier of the buffed ally.</summary>
    public string TargetName { get; init; } = string.Empty;

    /// <summary>Duration in turns of the buff.</summary>
    public int Duration { get; init; }

    /// <summary>Ability rank at time of cast.</summary>
    public int AbilityRank { get; init; }

    /// <summary>Gets a formatted description for combat log.</summary>
    public string GetDescription()
    {
        return $"Rune of Shielding grants {TargetName} +{SoakBonus} Soak, " +
               $"+{CorruptionResistBonusPercent}% Corruption resist for {Duration} turns";
    }
}

/// <summary>
/// Result of executing Reinforce Ward (29014): restores barrier HP or boosts zone effectiveness.
/// Barrier heal: 15/20/25 HP (R1/R2/R3). Zone boost: +50%/+75%/+100% effectiveness. Costs 2 AP.
/// </summary>
public sealed record ReinforceWardResult
{
    /// <summary>Whether the ability executed successfully.</summary>
    public bool IsSuccess { get; init; }

    /// <summary>Human-readable description of the ability outcome.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>AP spent to execute this ability.</summary>
    public int AetherSpent { get; init; }

    /// <summary>HP restored to a barrier (null if boosting a zone instead).</summary>
    public int? HpRestored { get; init; }

    /// <summary>Zone effectiveness boost percentage (null if reinforcing a barrier instead).</summary>
    public int? ZoneBoostPercent { get; init; }

    /// <summary>Name or identifier of the reinforced target (barrier or zone).</summary>
    public string TargetName { get; init; } = string.Empty;

    /// <summary>Whether the target being reinforced is a barrier (true) or zone (false).</summary>
    public bool IsBarrier { get; init; }

    /// <summary>Ability rank at time of cast.</summary>
    public int AbilityRank { get; init; }

    /// <summary>Gets a formatted description for combat log.</summary>
    public string GetDescription()
    {
        if (IsBarrier)
            return $"Reinforce Ward restores {HpRestored} HP to barrier {TargetName}";
        else
            return $"Reinforce Ward boosts zone {TargetName} effectiveness by +{ZoneBoostPercent}%";
    }
}

/// <summary>
/// Result of executing Glyph of Sanctuary (29016): grants all allies in range temp HP and Stress immunity.
/// Temp HP: 3d10 per ally. Stress immunity: 2 turns. Costs 4 AP.
/// </summary>
public sealed record GlyphOfSanctuaryResult
{
    /// <summary>Whether the ability executed successfully.</summary>
    public bool IsSuccess { get; init; }

    /// <summary>Human-readable description of the ability outcome.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>AP spent to execute this ability.</summary>
    public int AetherSpent { get; init; }

    /// <summary>Temporary HP granted to each affected ally.</summary>
    public int TempHpPerAlly { get; init; }

    /// <summary>Number of allies affected by the glyph.</summary>
    public int AlliesAffected { get; init; }

    /// <summary>Duration in turns of the Stress immunity effect.</summary>
    public int StressImmunityDuration { get; init; }

    /// <summary>Names/IDs of the affected allies.</summary>
    public string[] AffectedAllyNames { get; init; } = Array.Empty<string>();

    /// <summary>Ability rank at time of cast.</summary>
    public int AbilityRank { get; init; }

    /// <summary>Gets a formatted description for combat log.</summary>
    public string GetDescription()
    {
        var totalTempHp = TempHpPerAlly * AlliesAffected;
        return $"Glyph of Sanctuary grants {AlliesAffected} allies {TempHpPerAlly} temp HP each " +
               $"({totalTempHp} total) and Stress immunity for {StressImmunityDuration} turns";
    }
}

/// <summary>
/// Result of executing Indomitable Bastion (29018): reaction capstone that negates fatal damage.
/// Creates a 30 HP barrier on the saved ally. Once per expedition only. No AP cost (reaction).
/// </summary>
public sealed record IndomitableBastionResult
{
    /// <summary>Whether the ability executed successfully.</summary>
    public bool IsSuccess { get; init; }

    /// <summary>Human-readable description of the ability outcome.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Amount of damage that was negated (would have been fatal).</summary>
    public int DamageNegated { get; init; }

    /// <summary>Whether a protective barrier was created for the saved ally.</summary>
    public bool BarrierCreated { get; init; }

    /// <summary>Name or identifier of the ally saved from death.</summary>
    public string SavedAllyName { get; init; } = string.Empty;

    /// <summary>Whether this uses the once-per-expedition charge.</summary>
    public bool UsedExpeditionCharge { get; init; }

    /// <summary>Gets a formatted description for combat log.</summary>
    public string GetDescription()
    {
        var barrierTag = BarrierCreated ? " [30 HP barrier created]" : "";
        return $"Indomitable Bastion negates {DamageNegated} damage to {SavedAllyName}{barrierTag}";
    }
}

/// <summary>
/// Result of processing a consecrated zone tick (healing/damage application per turn).
/// Applied to allies in zone (healing) and Blighted/Undying enemies (damage).
/// </summary>
public sealed record ConsecratedZoneTickResult
{
    /// <summary>Whether the zone tick processed successfully.</summary>
    public bool IsSuccess { get; init; }

    /// <summary>Human-readable description of the zone tick outcome.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Healing amount per turn (for allies in zone).</summary>
    public int HealAmount { get; init; }

    /// <summary>Damage amount per turn (for Blighted/Undying in zone).</summary>
    public int DamageAmount { get; init; }

    /// <summary>Number of targets affected (healed or damaged).</summary>
    public int TargetsAffected { get; init; }

    /// <summary>Names/IDs of the affected targets.</summary>
    public string[] AffectedTargetNames { get; init; } = Array.Empty<string>();

    /// <summary>Gets a formatted description for combat log.</summary>
    public string GetDescription()
    {
        if (HealAmount > 0)
            return $"Consecrated zone heals {TargetsAffected} allies for {HealAmount} HP each";
        else if (DamageAmount > 0)
            return $"Consecrated zone damages {TargetsAffected} enemies for {DamageAmount} HP each";
        else
            return $"Consecrated zone tick: {TargetsAffected} targets affected";
    }
}
