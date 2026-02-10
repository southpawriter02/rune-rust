// ═══════════════════════════════════════════════════════════════════════════════
// RunasmidrTier2AbilityService.cs
// Implements Tier 2 ability operations for the Rúnasmiðr specialization:
// Empowered Inscription, Runic Trap, and Dvergr Techniques.
// Version: 0.20.2b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Implements Tier 2 Rúnasmiðr abilities: Empowered Inscription, Runic Trap,
/// and Dvergr Techniques.
/// </summary>
/// <remarks>
/// <para>
/// This service handles all Tier 2 ability operations for the Rúnasmiðr
/// specialization. It operates on immutable value objects and returns new
/// instances for all state transitions.
/// </para>
/// <para>
/// <b>Tier 2 requires 8 PP invested in the Rúnasmiðr ability tree.</b>
/// Each Tier 2 ability costs 4 PP to unlock.
/// </para>
/// <para>
/// Logging follows the established specialization service pattern:
/// </para>
/// <list type="bullet">
///   <item><description>
///     <b>Information:</b> Successful ability activations, trap triggers,
///     cost reductions, and expiry events
///   </description></item>
///   <item><description>
///     <b>Warning:</b> Rejected operations (trap limit exceeded, already
///     triggered, invalid element, insufficient PP)
///   </description></item>
///   <item><description>
///     <b>Debug:</b> Tick events, prerequisite checks, validation results
///   </description></item>
/// </list>
/// </remarks>
/// <seealso cref="EmpoweredRune"/>
/// <seealso cref="RunicTrap"/>
/// <seealso cref="TrapTriggerResult"/>
public class RunasmidrTier2AbilityService
{
    private readonly ILogger<RunasmidrTier2AbilityService> _logger;

    // Random instance for Runic Trap damage rolls (3d6)
    private readonly Random _random;

    /// <summary>
    /// Default crafting cost reduction percentage for Dvergr Techniques.
    /// </summary>
    public const decimal DvergrCostReductionPercent = 0.20m;

    /// <summary>
    /// Default crafting time reduction percentage for Dvergr Techniques.
    /// </summary>
    public const decimal DvergrTimeReductionPercent = 0.20m;

    /// <summary>
    /// Initializes a new instance of <see cref="RunasmidrTier2AbilityService"/>.
    /// </summary>
    /// <param name="logger">Logger for ability audit trail.</param>
    /// <param name="random">Optional random instance for testability. Defaults to shared instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
    public RunasmidrTier2AbilityService(
        ILogger<RunasmidrTier2AbilityService> logger,
        Random? random = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _random = random ?? Random.Shared;
    }

    // ═══════ Empowered Inscription ═══════

    /// <summary>
    /// Creates an empowered rune on the specified weapon with elemental damage.
    /// </summary>
    /// <param name="targetItemId">ID of the weapon to inscribe.</param>
    /// <param name="elementalDamageTypeId">
    /// Elemental damage type ID ("fire", "cold", "lightning", or "aetheric").
    /// </param>
    /// <returns>
    /// An <see cref="EmpoweredRune"/> instance if the element is valid;
    /// otherwise, <c>null</c>.
    /// </returns>
    public EmpoweredRune? CreateEmpoweredRune(
        Guid targetItemId,
        string elementalDamageTypeId)
    {
        if (!IsValidElement(elementalDamageTypeId))
        {
            _logger.LogWarning(
                "Empowered Inscription REJECTED: invalid element '{Element}'. " +
                "Valid elements: {ValidElements}",
                elementalDamageTypeId,
                string.Join(", ", EmpoweredRune.ValidElements));

            return null;
        }

        var rune = EmpoweredRune.Create(targetItemId, elementalDamageTypeId);

        _logger.LogInformation(
            "Empowered Inscription applied: {Element} rune inscribed on weapon {TargetItemId}. " +
            "Bonus: +{BonusDice} {Element} damage. Duration: {Duration} turns. RuneId: {RuneId}",
            rune.ElementalDamageTypeId,
            targetItemId,
            rune.BonusDice,
            rune.ElementalDamageTypeId,
            rune.OriginalDuration,
            rune.RuneId);

        return rune;
    }

    /// <summary>
    /// Advances an empowered rune by one turn, decrementing its duration.
    /// </summary>
    /// <param name="rune">The current empowered rune state.</param>
    /// <returns>Updated rune with decremented duration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rune"/> is null.</exception>
    public EmpoweredRune TickEmpoweredRune(EmpoweredRune rune)
    {
        ArgumentNullException.ThrowIfNull(rune);

        var updated = rune.Tick();

        if (updated.IsExpired)
        {
            _logger.LogInformation(
                "Empowered Rune expired on weapon {TargetItemId}. " +
                "Element: {Element}. RuneId: {RuneId}",
                rune.TargetItemId,
                rune.ElementalDamageTypeId,
                rune.RuneId);
        }
        else
        {
            _logger.LogDebug(
                "Empowered Rune ticked on weapon {TargetItemId}. " +
                "Element: {Element}. Turns remaining: {TurnsRemaining}/{OriginalDuration}",
                rune.TargetItemId,
                rune.ElementalDamageTypeId,
                updated.RemainingDuration,
                rune.OriginalDuration);
        }

        return updated;
    }

    /// <summary>
    /// Validates whether the specified elemental damage type ID is valid for
    /// Empowered Inscription.
    /// </summary>
    /// <param name="elementId">Damage type ID to validate.</param>
    /// <returns><c>true</c> if valid; otherwise, <c>false</c>.</returns>
    public bool IsValidElement(string elementId) =>
        EmpoweredRune.IsValidElement(elementId);

    // ═══════ Runic Trap ═══════

    /// <summary>
    /// Places a runic trap at the specified position.
    /// </summary>
    /// <param name="ownerId">ID of the character placing the trap.</param>
    /// <param name="position">Grid position (X, Y) for the trap.</param>
    /// <param name="activeTraps">Currently active traps owned by this character.</param>
    /// <returns>
    /// The new <see cref="RunicTrap"/> if placement is allowed;
    /// otherwise, <c>null</c> (and logs a warning).
    /// </returns>
    public RunicTrap? PlaceRunicTrap(
        Guid ownerId,
        (int X, int Y) position,
        IReadOnlyList<RunicTrap> activeTraps)
    {
        ArgumentNullException.ThrowIfNull(activeTraps);

        if (!CanPlaceTrap(activeTraps))
        {
            _logger.LogWarning(
                "Runic Trap placement REJECTED for {OwnerId}: maximum active traps reached " +
                "({ActiveCount}/{MaxTraps})",
                ownerId,
                activeTraps.Count,
                RunicTrap.MaxActiveTraps);

            return null;
        }

        var trap = RunicTrap.Create(ownerId, position);

        _logger.LogInformation(
            "Runic Trap placed by {OwnerId} at position ({X}, {Y}). " +
            "Damage: {DamageDice}, Detection DC: {DetectionDc}, " +
            "Expires: {ExpiresAt:u}. TrapId: {TrapId}. " +
            "Active traps: {ActiveCount}/{MaxTraps}",
            ownerId,
            position.X,
            position.Y,
            trap.DamageDice,
            trap.DetectionDc,
            trap.ExpiresAt,
            trap.TrapId,
            activeTraps.Count + 1,
            RunicTrap.MaxActiveTraps);

        return trap;
    }

    /// <summary>
    /// Triggers a runic trap when an enemy enters the trap's space.
    /// </summary>
    /// <param name="trap">The trap to trigger.</param>
    /// <param name="targetId">ID of the character who triggered the trap.</param>
    /// <returns>
    /// A tuple of (result, updatedTrap). If the trap cannot be triggered,
    /// result.Success is <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="trap"/> is null.</exception>
    public (TrapTriggerResult Result, RunicTrap UpdatedTrap) TriggerTrap(
        RunicTrap trap,
        Guid targetId)
    {
        ArgumentNullException.ThrowIfNull(trap);

        var (result, updatedTrap) = trap.Trigger(targetId, _random);

        if (result.Success)
        {
            _logger.LogInformation(
                "Runic Trap TRIGGERED: TrapId {TrapId} at ({X}, {Y}) dealt " +
                "{DamageRoll} damage to target {TargetId}. " +
                "Owner: {OwnerId}",
                trap.TrapId,
                trap.Position.X,
                trap.Position.Y,
                result.DamageRoll,
                targetId,
                trap.OwnerId);
        }
        else
        {
            _logger.LogWarning(
                "Runic Trap trigger FAILED: TrapId {TrapId} at ({X}, {Y}). " +
                "Reason: {Reason}. IsTriggered: {IsTriggered}, IsExpired: {IsExpired}",
                trap.TrapId,
                trap.Position.X,
                trap.Position.Y,
                result.Message,
                trap.IsTriggered,
                trap.IsExpired);
        }

        return (result, updatedTrap);
    }

    /// <summary>
    /// Checks whether a character can place another trap (limit of 3 active).
    /// </summary>
    /// <param name="activeTraps">Currently active traps.</param>
    /// <returns><c>true</c> if fewer than <see cref="RunicTrap.MaxActiveTraps"/> are active.</returns>
    public bool CanPlaceTrap(IReadOnlyList<RunicTrap> activeTraps)
    {
        ArgumentNullException.ThrowIfNull(activeTraps);
        return activeTraps.Count < RunicTrap.MaxActiveTraps;
    }

    // ═══════ Dvergr Techniques (Passive) ═══════

    /// <summary>
    /// Applies the Dvergr Techniques 20% crafting material cost reduction.
    /// </summary>
    /// <param name="baseCost">Original material cost.</param>
    /// <returns>
    /// The reduced cost, rounded down. Never returns less than 0.
    /// </returns>
    public int ApplyDvergrCostReduction(int baseCost)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(baseCost);

        if (baseCost == 0)
            return 0;

        var reduction = (int)Math.Floor(baseCost * DvergrCostReductionPercent);
        var reducedCost = Math.Max(0, baseCost - reduction);

        _logger.LogInformation(
            "Dvergr Techniques applied: material cost reduced from {BaseCost} to " +
            "{ReducedCost} ({ReductionPercent:P0} reduction, saved {Reduction})",
            baseCost,
            reducedCost,
            DvergrCostReductionPercent,
            reduction);

        return reducedCost;
    }

    /// <summary>
    /// Applies the Dvergr Techniques 20% crafting time reduction.
    /// </summary>
    /// <param name="baseTimeMinutes">Original crafting time in minutes.</param>
    /// <returns>
    /// The reduced time in minutes, rounded down. Never returns less than 0.
    /// </returns>
    public int ApplyDvergrTimeReduction(int baseTimeMinutes)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(baseTimeMinutes);

        if (baseTimeMinutes == 0)
            return 0;

        var reduction = (int)Math.Floor(baseTimeMinutes * DvergrTimeReductionPercent);
        var reducedTime = Math.Max(0, baseTimeMinutes - reduction);

        _logger.LogInformation(
            "Dvergr Techniques applied: crafting time reduced from {BaseTime}m to " +
            "{ReducedTime}m ({ReductionPercent:P0} reduction, saved {Reduction}m)",
            baseTimeMinutes,
            reducedTime,
            DvergrTimeReductionPercent,
            reduction);

        return reducedTime;
    }

    // ═══════ Prerequisite Helpers ═══════

    /// <summary>
    /// PP threshold required to unlock Tier 2 Rúnasmiðr abilities.
    /// </summary>
    /// <remarks>
    /// Mirrors <see cref="PrerequisiteService.Tier2Threshold"/> but kept local
    /// for Rúnasmiðr-specific PP calculations.
    /// </remarks>
    public const int Tier2Threshold = 8;

    /// <summary>
    /// Checks whether Tier 2 abilities can be unlocked based on PP invested.
    /// Requires 8 PP invested in the Rúnasmiðr tree.
    /// </summary>
    /// <param name="ppInvested">Total PP invested.</param>
    /// <returns>True if threshold is met.</returns>
    public bool CanUnlockTier2(int ppInvested)
    {
        var canUnlock = ppInvested >= Tier2Threshold;

        _logger.LogDebug(
            "Rúnasmiðr Tier 2 unlock check: PP invested {PPInvested}, " +
            "threshold {Threshold}, result {CanUnlock}",
            ppInvested, Tier2Threshold, canUnlock);

        return canUnlock;
    }

    /// <summary>
    /// Calculates total PP invested from a list of unlocked Rúnasmiðr abilities.
    /// </summary>
    /// <param name="unlockedAbilities">Currently unlocked abilities.</param>
    /// <returns>Total PP cost of all unlocked abilities.</returns>
    public int CalculatePPInvested(IReadOnlyList<RunasmidrAbilityId> unlockedAbilities)
    {
        ArgumentNullException.ThrowIfNull(unlockedAbilities);

        var total = 0;
        foreach (var ability in unlockedAbilities)
        {
            total += GetAbilityPPCost(ability);
        }

        _logger.LogDebug(
            "Rúnasmiðr PP invested calculation: {AbilityCount} abilities unlocked, " +
            "total PP invested: {TotalPP}",
            unlockedAbilities.Count, total);

        return total;
    }

    /// <summary>
    /// Gets the PP cost for a specific Rúnasmiðr ability based on its tier.
    /// </summary>
    /// <param name="abilityId">The ability to look up.</param>
    /// <returns>PP cost: 0 (Tier 1), 4 (Tier 2), 5 (Tier 3), 6 (Capstone).</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown for unknown abilities.</exception>
    public static int GetAbilityPPCost(RunasmidrAbilityId abilityId) => abilityId switch
    {
        // Tier 1: Free
        RunasmidrAbilityId.InscribeRune => 0,
        RunasmidrAbilityId.ReadTheMarks => 0,
        RunasmidrAbilityId.RunestoneWard => 0,

        // Tier 2: 4 PP each
        RunasmidrAbilityId.EmpoweredInscription => 4,
        RunasmidrAbilityId.RunicTrap => 4,
        RunasmidrAbilityId.DvergrTechniques => 4,

        // Tier 3: 5 PP each (future — v0.20.2c)
        RunasmidrAbilityId.MasterScrivener => 5,
        RunasmidrAbilityId.LivingRunes => 5,

        // Capstone: 6 PP (future — v0.20.2d)
        RunasmidrAbilityId.WordOfUnmaking => 6,

        _ => throw new ArgumentOutOfRangeException(nameof(abilityId), abilityId,
            $"Unknown Rúnasmiðr ability: {abilityId}")
    };
}
