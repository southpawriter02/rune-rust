// ═══════════════════════════════════════════════════════════════════════════════
// RunasmidrTier3AbilityService.cs
// Implements Tier 3 and Capstone ability operations for the Rúnasmiðr
// specialization: Master Scrivener, Living Runes, and Word of Unmaking.
// Version: 0.20.2c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Implements Tier 3 and Capstone Rúnasmiðr abilities: Master Scrivener,
/// Living Runes, and Word of Unmaking.
/// </summary>
/// <remarks>
/// <para>
/// This service handles all Tier 3 and Capstone ability operations for the
/// Rúnasmiðr specialization. It operates on immutable value objects and returns
/// new instances for all state transitions.
/// </para>
/// <para>
/// <b>Tier 3 requires 16 PP invested in the Rúnasmiðr ability tree.</b>
/// Each Tier 3 ability costs 5 PP to unlock.
/// <b>Capstone requires 24 PP invested.</b>
/// The Capstone ability costs 6 PP to unlock.
/// </para>
/// <para>
/// Logging follows the established specialization service pattern:
/// </para>
/// <list type="bullet">
///   <item><description>
///     <b>Information:</b> Successful ability activations, summoning events,
///     dispel results, duration modifications, entity destruction/expiry
///   </description></item>
///   <item><description>
///     <b>Warning:</b> Rejected operations (already used Word of Unmaking,
///     insufficient PP)
///   </description></item>
///   <item><description>
///     <b>Debug:</b> Tick events, prerequisite checks, attack rolls
///   </description></item>
/// </list>
/// </remarks>
/// <seealso cref="LivingRuneEntity"/>
/// <seealso cref="DispelEffectResult"/>
/// <seealso cref="RunasmidrTier2AbilityService"/>
public class RunasmidrTier3AbilityService
{
    private readonly ILogger<RunasmidrTier3AbilityService> _logger;

    // Random instance for Living Rune attack damage rolls (1d8)
    private readonly Random _random;

    // ═══════ Constants ═══════

    /// <summary>
    /// Duration multiplier applied by Master Scrivener passive.
    /// </summary>
    public const int MasterScrivenerMultiplier = 2;

    /// <summary>
    /// Rune Charge cost for Living Runes ability.
    /// </summary>
    public const int LivingRunesChargeCost = 3;

    /// <summary>
    /// Rune Charge cost for Word of Unmaking ability.
    /// </summary>
    public const int WordOfUnmakingChargeCost = 4;

    /// <summary>
    /// Dispel radius (in spaces) for Word of Unmaking.
    /// </summary>
    public const int DispelRadius = 4;

    /// <summary>
    /// PP threshold required to unlock Tier 3 Rúnasmiðr abilities.
    /// </summary>
    public const int Tier3Threshold = 16;

    /// <summary>
    /// PP threshold required to unlock the Capstone Rúnasmiðr ability.
    /// </summary>
    public const int CapstoneThreshold = 24;

    /// <summary>
    /// Initializes a new instance of <see cref="RunasmidrTier3AbilityService"/>.
    /// </summary>
    /// <param name="logger">Logger for ability audit trail.</param>
    /// <param name="random">Optional random instance for testability. Defaults to shared instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
    public RunasmidrTier3AbilityService(
        ILogger<RunasmidrTier3AbilityService> logger,
        Random? random = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _random = random ?? Random.Shared;
    }

    // ═══════ Master Scrivener (Passive) ═══════

    /// <summary>
    /// Applies the Master Scrivener 2x duration multiplier to a rune's turn-based duration.
    /// </summary>
    /// <param name="baseDuration">Original duration in turns (e.g., 10 for Inscribe Rune).</param>
    /// <returns>The modified duration (baseDuration × 2).</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="baseDuration"/> is negative.
    /// </exception>
    public int ApplyMasterScrivenerDuration(int baseDuration)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(baseDuration);

        var modifiedDuration = baseDuration * MasterScrivenerMultiplier;

        _logger.LogInformation(
            "Master Scrivener applied: rune duration modified from {BaseDuration} to " +
            "{ModifiedDuration} turns ({Multiplier}x multiplier)",
            baseDuration,
            modifiedDuration,
            MasterScrivenerMultiplier);

        return modifiedDuration;
    }

    /// <summary>
    /// Applies the Master Scrivener 2x duration multiplier to a trap's hour-based duration.
    /// </summary>
    /// <param name="baseDurationHours">Original duration in hours (e.g., 1 for Runic Trap).</param>
    /// <returns>The modified duration in hours (baseDurationHours × 2).</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="baseDurationHours"/> is negative.
    /// </exception>
    public int ApplyMasterScrivenerTrapDuration(int baseDurationHours)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(baseDurationHours);

        var modifiedDuration = baseDurationHours * MasterScrivenerMultiplier;

        _logger.LogInformation(
            "Master Scrivener applied: trap duration modified from {BaseHours}h to " +
            "{ModifiedHours}h ({Multiplier}x multiplier)",
            baseDurationHours,
            modifiedDuration,
            MasterScrivenerMultiplier);

        return modifiedDuration;
    }

    /// <summary>
    /// Checks whether Master Scrivener is active (unlocked) for a character.
    /// </summary>
    /// <param name="unlockedAbilities">The character's unlocked Rúnasmiðr abilities.</param>
    /// <returns><c>true</c> if Master Scrivener is in the unlocked abilities list.</returns>
    public bool IsMasterScrivenerActive(IReadOnlyList<RunasmidrAbilityId> unlockedAbilities)
    {
        ArgumentNullException.ThrowIfNull(unlockedAbilities);

        var isActive = unlockedAbilities.Contains(RunasmidrAbilityId.MasterScrivener);

        _logger.LogDebug(
            "Master Scrivener active check: {IsActive} ({AbilityCount} abilities unlocked)",
            isActive,
            unlockedAbilities.Count);

        return isActive;
    }

    // ═══════ Living Runes (Active) ═══════

    /// <summary>
    /// Summons 2 Living Rune entities at the specified position.
    /// </summary>
    /// <param name="ownerId">ID of the character summoning the entities.</param>
    /// <param name="position">Grid position (X, Y) for the summoned entities.</param>
    /// <returns>
    /// A list of 2 <see cref="LivingRuneEntity"/> instances with default stats.
    /// </returns>
    public IReadOnlyList<LivingRuneEntity> SummonLivingRunes(
        Guid ownerId,
        (int X, int Y) position)
    {
        var runes = new List<LivingRuneEntity>();
        for (var i = 0; i < LivingRuneEntity.SummonCount; i++)
        {
            runes.Add(LivingRuneEntity.Create(ownerId, position));
        }

        _logger.LogInformation(
            "Living Runes summoned by {OwnerId}: {Count} entities at ({X}, {Y}). " +
            "Stats: {MaxHp} HP, {Defense} DEF, +{AttackBonus} ATK, {DamageDice} DMG. " +
            "Duration: {Duration} turns. Charge cost: {ChargeCost}",
            ownerId,
            runes.Count,
            position.X,
            position.Y,
            LivingRuneEntity.DefaultMaxHp,
            LivingRuneEntity.DefaultDefense,
            LivingRuneEntity.DefaultAttackBonus,
            LivingRuneEntity.DefaultDamageDice,
            LivingRuneEntity.DefaultDuration,
            LivingRunesChargeCost);

        return runes;
    }

    /// <summary>
    /// Advances a Living Rune entity by one turn, decrementing its duration.
    /// </summary>
    /// <param name="rune">The current Living Rune entity state.</param>
    /// <returns>Updated entity with decremented duration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rune"/> is null.</exception>
    public LivingRuneEntity TickLivingRune(LivingRuneEntity rune)
    {
        ArgumentNullException.ThrowIfNull(rune);

        var updated = rune.Tick();

        if (!updated.IsActive && updated.IsExpired)
        {
            _logger.LogInformation(
                "Living Rune {EntityId} expired. Owner: {OwnerId}. " +
                "Final HP: {CurrentHp}/{MaxHp}",
                rune.EntityId,
                rune.OwnerId,
                updated.CurrentHp,
                updated.MaxHp);
        }
        else
        {
            _logger.LogDebug(
                "Living Rune {EntityId} ticked. Turns remaining: " +
                "{TurnsRemaining}/{OriginalDuration}. HP: {CurrentHp}/{MaxHp}",
                rune.EntityId,
                updated.TurnsRemaining,
                rune.OriginalDuration,
                updated.CurrentHp,
                updated.MaxHp);
        }

        return updated;
    }

    /// <summary>
    /// Applies damage to a Living Rune entity.
    /// </summary>
    /// <param name="rune">The current Living Rune entity state.</param>
    /// <param name="damage">Amount of damage to apply.</param>
    /// <returns>Updated entity with reduced HP.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rune"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="damage"/> is negative.
    /// </exception>
    public LivingRuneEntity ApplyDamageToLivingRune(LivingRuneEntity rune, int damage)
    {
        ArgumentNullException.ThrowIfNull(rune);
        ArgumentOutOfRangeException.ThrowIfNegative(damage);

        var updated = rune.TakeDamage(damage);

        if (updated.IsDestroyed)
        {
            _logger.LogInformation(
                "Living Rune {EntityId} destroyed. Damage taken: {Damage}. " +
                "Owner: {OwnerId}",
                rune.EntityId,
                damage,
                rune.OwnerId);
        }
        else
        {
            _logger.LogDebug(
                "Living Rune {EntityId} took {Damage} damage. " +
                "HP: {CurrentHp}/{MaxHp}. Owner: {OwnerId}",
                rune.EntityId,
                damage,
                updated.CurrentHp,
                updated.MaxHp,
                rune.OwnerId);
        }

        return updated;
    }

    /// <summary>
    /// Rolls attack damage for a Living Rune entity (1d8).
    /// </summary>
    /// <param name="rune">The attacking Living Rune entity.</param>
    /// <returns>The damage rolled (1–8).</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rune"/> is null.</exception>
    public int RollLivingRuneAttackDamage(LivingRuneEntity rune)
    {
        ArgumentNullException.ThrowIfNull(rune);

        var damage = rune.RollAttackDamage(_random);

        _logger.LogInformation(
            "Living Rune {EntityId} attack roll: {DamageRoll} damage ({DamageDice}). " +
            "Owner: {OwnerId}",
            rune.EntityId,
            damage,
            rune.DamageDice,
            rune.OwnerId);

        return damage;
    }

    // ═══════ Word of Unmaking (Capstone) ═══════

    /// <summary>
    /// Executes the Word of Unmaking, building a <see cref="DispelEffectResult"/>
    /// from the provided effects data.
    /// </summary>
    /// <param name="casterId">ID of the character casting the ability.</param>
    /// <param name="centerPosition">Center point of the dispel area.</param>
    /// <param name="activeEffects">Names of active magical effects in the area.</param>
    /// <param name="summonedEntities">IDs of summoned entities in the area.</param>
    /// <param name="temporaryEnchantmentItems">IDs of items with temporary enchantments.</param>
    /// <param name="affectedCharacters">IDs of characters in the area.</param>
    /// <returns>A <see cref="DispelEffectResult"/> summarizing all effects removed.</returns>
    public DispelEffectResult ExecuteWordOfUnmaking(
        Guid casterId,
        (int X, int Y) centerPosition,
        IReadOnlyList<string> activeEffects,
        IReadOnlyList<Guid> summonedEntities,
        IReadOnlyList<Guid> temporaryEnchantmentItems,
        IReadOnlyList<Guid> affectedCharacters)
    {
        ArgumentNullException.ThrowIfNull(activeEffects);
        ArgumentNullException.ThrowIfNull(summonedEntities);
        ArgumentNullException.ThrowIfNull(temporaryEnchantmentItems);
        ArgumentNullException.ThrowIfNull(affectedCharacters);

        var result = DispelEffectResult.Create(
            activeEffects,
            summonedEntities,
            temporaryEnchantmentItems,
            affectedCharacters);

        _logger.LogInformation(
            "Word of Unmaking executed by {CasterId} at ({X}, {Y}). " +
            "Radius: {Radius} spaces. Dispelled {TotalEffects} effects: " +
            "{EffectsCount} statuses, {EntitiesCount} entities, " +
            "{ItemsCount} items. Affected {CharactersCount} characters. " +
            "Charge cost: {ChargeCost}",
            casterId,
            centerPosition.X,
            centerPosition.Y,
            DispelRadius,
            result.TotalEffectsDispelled,
            result.EffectsRemoved.Count,
            result.EntitiesDestroyed.Count,
            result.ItemsAffected.Count,
            result.AffectedCharacters.Count,
            WordOfUnmakingChargeCost);

        return result;
    }

    /// <summary>
    /// Checks whether Word of Unmaking has already been used in the current combat
    /// by the specified character.
    /// </summary>
    /// <param name="characterId">ID of the character to check.</param>
    /// <param name="usageLog">List of character IDs that have used Word of Unmaking this combat.</param>
    /// <returns><c>true</c> if the character has already used Word of Unmaking.</returns>
    public bool HasUsedWordOfUnmaking(Guid characterId, IReadOnlyList<Guid> usageLog)
    {
        ArgumentNullException.ThrowIfNull(usageLog);

        var hasUsed = usageLog.Contains(characterId);

        if (hasUsed)
        {
            _logger.LogWarning(
                "Word of Unmaking REJECTED for {CharacterId}: already used this combat",
                characterId);
        }

        return hasUsed;
    }

    // ═══════ Prerequisite Helpers ═══════

    /// <summary>
    /// Checks whether Tier 3 abilities can be unlocked based on PP invested.
    /// Requires 16 PP invested in the Rúnasmiðr tree.
    /// </summary>
    /// <param name="ppInvested">Total PP invested.</param>
    /// <returns>True if threshold is met.</returns>
    public bool CanUnlockTier3(int ppInvested)
    {
        var canUnlock = ppInvested >= Tier3Threshold;

        _logger.LogDebug(
            "Rúnasmiðr Tier 3 unlock check: PP invested {PPInvested}, " +
            "threshold {Threshold}, result {CanUnlock}",
            ppInvested, Tier3Threshold, canUnlock);

        return canUnlock;
    }

    /// <summary>
    /// Checks whether the Capstone ability can be unlocked based on PP invested.
    /// Requires 24 PP invested in the Rúnasmiðr tree.
    /// </summary>
    /// <param name="ppInvested">Total PP invested.</param>
    /// <returns>True if threshold is met.</returns>
    public bool CanUnlockCapstone(int ppInvested)
    {
        var canUnlock = ppInvested >= CapstoneThreshold;

        _logger.LogDebug(
            "Rúnasmiðr Capstone unlock check: PP invested {PPInvested}, " +
            "threshold {Threshold}, result {CanUnlock}",
            ppInvested, CapstoneThreshold, canUnlock);

        return canUnlock;
    }
}
