namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Result of ticking all active zones during a combat round.
/// </summary>
/// <remarks>
/// <para>ZoneTickResult aggregates the effects of processing all zones for a round:</para>
/// <list type="bullet">
///   <item><description>Total damage dealt across all damage zones</description></item>
///   <item><description>Total healing done across all healing zones</description></item>
///   <item><description>Lists of affected entities and expired zones</description></item>
/// </list>
/// <para>Used for combat logging, UI updates, and determining round outcomes.</para>
/// </remarks>
/// <example>
/// <code>
/// var result = zoneService.TickZones(combatants);
/// if (result.HasEffects)
/// {
///     logger.LogInformation(
///         "Zone tick: {Damage} damage, {Healing} healing, {Expired} expired",
///         result.TotalDamageDealt, result.TotalHealingDone, result.ExpiredZones.Count);
/// }
/// </code>
/// </example>
public class ZoneTickResult
{
    // ═══════════════════════════════════════════════════════════════
    // AGGREGATE TOTALS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets or sets the total damage dealt by all zones this tick.
    /// </summary>
    public int TotalDamageDealt { get; set; }

    /// <summary>
    /// Gets or sets the total healing done by all zones this tick.
    /// </summary>
    public int TotalHealingDone { get; set; }

    // ═══════════════════════════════════════════════════════════════
    // ENTITY TRACKING
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the IDs of entities that took damage from zones.
    /// </summary>
    public List<Guid> EntitiesDamaged { get; } = new();

    /// <summary>
    /// Gets the IDs of entities that were healed by zones.
    /// </summary>
    public List<Guid> EntitiesHealed { get; } = new();

    /// <summary>
    /// Gets the IDs of entities that had status effects applied.
    /// </summary>
    public List<Guid> EntitiesAffectedByStatus { get; } = new();

    // ═══════════════════════════════════════════════════════════════
    // ZONE TRACKING
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the zone IDs that expired this tick.
    /// </summary>
    public List<string> ExpiredZones { get; } = new();

    /// <summary>
    /// Gets the zone IDs that applied effects this tick.
    /// </summary>
    public List<string> ActiveZonesTriggered { get; } = new();

    // ═══════════════════════════════════════════════════════════════
    // DETAILED BREAKDOWNS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets damage dealt broken down by zone ID.
    /// </summary>
    public Dictionary<string, int> DamageByZone { get; } = new();

    /// <summary>
    /// Gets healing done broken down by zone ID.
    /// </summary>
    public Dictionary<string, int> HealingByZone { get; } = new();

    /// <summary>
    /// Gets damage dealt broken down by entity ID.
    /// </summary>
    public Dictionary<Guid, int> DamageByEntity { get; } = new();

    /// <summary>
    /// Gets healing received broken down by entity ID.
    /// </summary>
    public Dictionary<Guid, int> HealingByEntity { get; } = new();

    // ═══════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether any effects were applied during this tick.
    /// </summary>
    public bool HasEffects =>
        TotalDamageDealt > 0 ||
        TotalHealingDone > 0 ||
        ExpiredZones.Count > 0 ||
        EntitiesAffectedByStatus.Count > 0;

    /// <summary>
    /// Gets the count of entities affected by zones this tick.
    /// </summary>
    public int TotalEntitiesAffected =>
        EntitiesDamaged.Concat(EntitiesHealed).Concat(EntitiesAffectedByStatus).Distinct().Count();

    /// <summary>
    /// Gets the count of zones that triggered effects this tick.
    /// </summary>
    public int ZonesTriggeredCount => ActiveZonesTriggered.Distinct().Count();

    /// <summary>
    /// Gets the count of zones that expired this tick.
    /// </summary>
    public int ZonesExpiredCount => ExpiredZones.Count;

    // ═══════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Records damage dealt by a zone to an entity.
    /// </summary>
    /// <param name="zoneId">The zone ID that dealt damage.</param>
    /// <param name="entityId">The entity ID that took damage.</param>
    /// <param name="damage">The amount of damage dealt.</param>
    public void RecordDamage(string zoneId, Guid entityId, int damage)
    {
        TotalDamageDealt += damage;

        // Track by zone
        if (!DamageByZone.TryAdd(zoneId, damage))
        {
            DamageByZone[zoneId] += damage;
        }

        // Track by entity
        if (!DamageByEntity.TryAdd(entityId, damage))
        {
            DamageByEntity[entityId] += damage;
        }

        // Track affected entities
        if (!EntitiesDamaged.Contains(entityId))
        {
            EntitiesDamaged.Add(entityId);
        }

        // Track triggered zones
        if (!ActiveZonesTriggered.Contains(zoneId))
        {
            ActiveZonesTriggered.Add(zoneId);
        }
    }

    /// <summary>
    /// Records healing done by a zone to an entity.
    /// </summary>
    /// <param name="zoneId">The zone ID that provided healing.</param>
    /// <param name="entityId">The entity ID that was healed.</param>
    /// <param name="healing">The amount of healing done.</param>
    public void RecordHealing(string zoneId, Guid entityId, int healing)
    {
        TotalHealingDone += healing;

        // Track by zone
        if (!HealingByZone.TryAdd(zoneId, healing))
        {
            HealingByZone[zoneId] += healing;
        }

        // Track by entity
        if (!HealingByEntity.TryAdd(entityId, healing))
        {
            HealingByEntity[entityId] += healing;
        }

        // Track affected entities
        if (!EntitiesHealed.Contains(entityId))
        {
            EntitiesHealed.Add(entityId);
        }

        // Track triggered zones
        if (!ActiveZonesTriggered.Contains(zoneId))
        {
            ActiveZonesTriggered.Add(zoneId);
        }
    }

    /// <summary>
    /// Records a status effect application by a zone.
    /// </summary>
    /// <param name="zoneId">The zone ID that applied the status.</param>
    /// <param name="entityId">The entity ID that received the status.</param>
    public void RecordStatusApplication(string zoneId, Guid entityId)
    {
        if (!EntitiesAffectedByStatus.Contains(entityId))
        {
            EntitiesAffectedByStatus.Add(entityId);
        }

        if (!ActiveZonesTriggered.Contains(zoneId))
        {
            ActiveZonesTriggered.Add(zoneId);
        }
    }

    /// <summary>
    /// Records a zone expiration.
    /// </summary>
    /// <param name="zoneId">The zone ID that expired.</param>
    public void RecordExpiration(string zoneId)
    {
        if (!ExpiredZones.Contains(zoneId))
        {
            ExpiredZones.Add(zoneId);
        }
    }

    /// <summary>
    /// Gets a summary string for logging.
    /// </summary>
    /// <returns>A formatted summary of the tick results.</returns>
    public string GetSummary()
    {
        if (!HasEffects)
        {
            return "No zone effects this tick";
        }

        var parts = new List<string>();

        if (TotalDamageDealt > 0)
        {
            parts.Add($"{TotalDamageDealt} damage to {EntitiesDamaged.Count} entities");
        }

        if (TotalHealingDone > 0)
        {
            parts.Add($"{TotalHealingDone} healing to {EntitiesHealed.Count} entities");
        }

        if (EntitiesAffectedByStatus.Count > 0)
        {
            parts.Add($"{EntitiesAffectedByStatus.Count} status effects applied");
        }

        if (ExpiredZones.Count > 0)
        {
            parts.Add($"{ExpiredZones.Count} zones expired");
        }

        return string.Join(", ", parts);
    }
}
