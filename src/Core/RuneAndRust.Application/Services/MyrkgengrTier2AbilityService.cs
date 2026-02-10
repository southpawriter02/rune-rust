// ═══════════════════════════════════════════════════════════════════════════════
// MyrkgengrTier2AbilityService.cs
// Application service implementing Myrk-gengr Tier 2 abilities:
// Umbral Strike, Shadow Clone, and Void Touched.
// Version: 0.20.4c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Implements Myrk-gengr Tier 2 abilities: Umbral Strike, Shadow Clone,
/// and Void Touched.
/// </summary>
/// <remarks>
/// <para>
/// This service handles Tier 2 ability operations as an extension of
/// <see cref="IMyrkgengrAbilityService"/>. It operates on immutable value objects
/// and returns new instances for all state transitions.
/// </para>
/// <para><strong>Supported Abilities (v0.20.4b):</strong></para>
/// <list type="bullet">
///   <item><description>
///     <b>Umbral Strike:</b> Shadow-infused melee attack (2 AP, 15 Essence).
///     Deals weapon damage + 2d6 shadow damage. Must be in shadow (DimLight or darker).
///     Consuming a clone grants advantage + 1d4 bonus shadow damage.
///   </description></item>
///   <item><description>
///     <b>Shadow Clone:</b> Create an illusory duplicate (3 AP, 20 Essence).
///     1 HP, DC 14 to detect, 1 minute duration. Max 2 clones active.
///   </description></item>
///   <item><description>
///     <b>Void Touched:</b> Passive. Halves incoming Aetheric damage (floor).
///     Grants advantage on saves vs. Aetheric effects.
///   </description></item>
/// </list>
/// <para>
/// <b>Tier 2 requires 8 PP invested in the Myrk-gengr ability tree.</b>
/// Each Tier 2 ability costs 4 PP to unlock.
/// </para>
/// </remarks>
/// <seealso cref="IMyrkgengrAbilityService"/>
/// <seealso cref="IShadowCorruptionService"/>
public class MyrkgengrTier2AbilityService
{
    private readonly ILogger<MyrkgengrTier2AbilityService> _logger;
    private readonly IShadowCorruptionService _corruptionService;
    private readonly Random _random;

    // ─────────────────────────────────────────────────────────────────────────
    // Constants
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Shadow Essence cost for Umbral Strike.</summary>
    private const int UmbralStrikeEssenceCost = 15;

    /// <summary>Shadow Essence cost for Shadow Clone.</summary>
    private const int ShadowCloneEssenceCost = 20;

    /// <summary>Maximum number of active shadow clones.</summary>
    private const int MaxActiveClones = 2;

    /// <summary>PP threshold required to unlock Tier 2 abilities.</summary>
    private const int Tier2PPThreshold = 8;

    // ─────────────────────────────────────────────────────────────────────────
    // Constructor
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Initializes a new instance of <see cref="MyrkgengrTier2AbilityService"/>.
    /// </summary>
    /// <param name="logger">Logger for ability audit trail.</param>
    /// <param name="corruptionService">Service for evaluating Corruption risk.</param>
    /// <param name="random">Optional random instance for testability. Defaults to shared instance.</param>
    public MyrkgengrTier2AbilityService(
        ILogger<MyrkgengrTier2AbilityService> logger,
        IShadowCorruptionService corruptionService,
        Random? random = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _corruptionService = corruptionService ?? throw new ArgumentNullException(nameof(corruptionService));
        _random = random ?? Random.Shared;
    }

    // ═══════ Umbral Strike ═══════

    /// <summary>
    /// Checks whether Umbral Strike can be executed under current conditions.
    /// </summary>
    /// <param name="resource">Current Shadow Essence resource.</param>
    /// <param name="lightLevel">Light level at the caster's position.</param>
    /// <returns>True if the caster is in shadow and has sufficient essence.</returns>
    public bool CanExecuteUmbralStrike(
        ShadowEssenceResource resource,
        LightLevelType lightLevel)
    {
        ArgumentNullException.ThrowIfNull(resource);

        return lightLevel <= LightLevelType.DimLight
            && resource.CurrentEssence >= UmbralStrikeEssenceCost;
    }

    /// <summary>
    /// Executes an Umbral Strike: shadow-infused melee attack dealing weapon
    /// damage + 2d6 shadow damage. Requires shadow (DimLight or darker).
    /// </summary>
    /// <param name="resource">Current Shadow Essence resource.</param>
    /// <param name="lightLevel">Light level at the caster's position.</param>
    /// <param name="attackModifier">Total attack modifier (stat + weapon + proficiency).</param>
    /// <param name="targetDefense">Target's defense value.</param>
    /// <param name="weaponDamage">Base weapon damage.</param>
    /// <param name="consumeClone">Whether to consume a shadow clone for advantage.</param>
    /// <returns>An UmbralStrikeResult with hit/miss, damage, and corruption data.</returns>
    public UmbralStrikeResult ExecuteUmbralStrike(
        ShadowEssenceResource resource,
        LightLevelType lightLevel,
        int attackModifier,
        int targetDefense,
        int weaponDamage,
        bool consumeClone = false)
    {
        ArgumentNullException.ThrowIfNull(resource);

        // ── Validate prerequisites ──────────────────────────────────────────
        if (lightLevel > LightLevelType.DimLight)
        {
            _logger.LogWarning(
                "Umbral Strike failed: Must be in shadow (DimLight or darker). " +
                "Current light level: {LightLevel}",
                lightLevel);

            return UmbralStrikeResult.CreateFailure(
                $"Must be in shadow (DimLight or darker). Current: {lightLevel}",
                resource);
        }

        if (resource.CurrentEssence < UmbralStrikeEssenceCost)
        {
            _logger.LogWarning(
                "Umbral Strike failed: Insufficient Shadow Essence. " +
                "Current: {CurrentEssence}, Required: {RequiredEssence}",
                resource.CurrentEssence, UmbralStrikeEssenceCost);

            return UmbralStrikeResult.CreateFailure(
                $"Insufficient Shadow Essence ({resource.CurrentEssence}/{UmbralStrikeEssenceCost})",
                resource);
        }

        // ── Spend essence ───────────────────────────────────────────────────
        var (spendSuccess, updatedResource) = resource.TrySpend(UmbralStrikeEssenceCost);
        if (!spendSuccess)
        {
            return UmbralStrikeResult.CreateFailure(
                $"Failed to spend Shadow Essence ({resource.CurrentEssence}/{UmbralStrikeEssenceCost})",
                resource);
        }

        // ── Roll attack ─────────────────────────────────────────────────────
        var roll1 = _random.Next(1, 21); // d20
        int attackRoll;

        if (consumeClone)
        {
            // Advantage: roll twice, take higher
            var roll2 = _random.Next(1, 21);
            attackRoll = Math.Max(roll1, roll2) + attackModifier;

            _logger.LogInformation(
                "Umbral Strike attack with advantage (clone consumed): " +
                "rolls {Roll1} and {Roll2}, using {BestRoll} + {Modifier} = {Total}",
                roll1, roll2, Math.Max(roll1, roll2), attackModifier, attackRoll);
        }
        else
        {
            attackRoll = roll1 + attackModifier;

            _logger.LogInformation(
                "Umbral Strike attack: roll {Roll} + {Modifier} = {Total}",
                roll1, attackModifier, attackRoll);
        }

        // ── Evaluate corruption ─────────────────────────────────────────────
        var corruptionResult = _corruptionService.EvaluateRisk(
            MyrkgengrAbilityId.UmbralStrike, lightLevel);

        // ── Check hit ───────────────────────────────────────────────────────
        var isCritical = roll1 == 20 || (consumeClone && Math.Max(roll1, _random.Next(1, 21)) == 20);

        if (attackRoll < targetDefense)
        {
            _logger.LogInformation(
                "Umbral Strike missed: {AttackRoll} vs defense {TargetDefense}",
                attackRoll, targetDefense);

            return UmbralStrikeResult.CreateMiss(
                attackRoll, targetDefense, updatedResource, corruptionResult);
        }

        // ── Calculate damage ────────────────────────────────────────────────
        // Base shadow damage: 2d6
        var shadowDamage = _random.Next(1, 7) + _random.Next(1, 7);

        // Critical hit: +1d6 shadow damage
        if (isCritical)
        {
            shadowDamage += _random.Next(1, 7);
        }

        // Clone consumption bonus: +1d4 shadow damage
        var bonusDamage = 0;
        if (consumeClone)
        {
            bonusDamage = _random.Next(1, 5); // 1d4
        }

        var damage = DamageBreakdown.Create(weaponDamage, shadowDamage, bonusDamage);

        _logger.LogInformation(
            "Umbral Strike hit: {AttackRoll} vs {TargetDefense}. " +
            "Damage: {WeaponDamage} phys + {ShadowDamage} shadow + {BonusDamage} bonus " +
            "= {TotalDamage} total. Critical: {IsCritical}, Clone consumed: {CloneConsumed}",
            attackRoll, targetDefense, weaponDamage, shadowDamage, bonusDamage,
            damage.GetTotal(), isCritical, consumeClone);

        return UmbralStrikeResult.CreateHit(
            attackRoll, targetDefense, damage, isCritical,
            consumeClone, corruptionResult, updatedResource);
    }

    // ═══════ Shadow Clone ═══════

    /// <summary>
    /// Creates a shadow clone at the specified position.
    /// </summary>
    /// <param name="resource">Current Shadow Essence resource.</param>
    /// <param name="ownerId">ID of the caster.</param>
    /// <param name="x">X coordinate for clone placement.</param>
    /// <param name="y">Y coordinate for clone placement.</param>
    /// <param name="behavior">Clone behavior pattern.</param>
    /// <param name="lightLevel">Light level at the caster's position.</param>
    /// <param name="activeCloneCount">Number of currently active clones.</param>
    /// <returns>Tuple of the created clone and the updated resource, or null clone on failure.</returns>
    public (ShadowClone? Clone, ShadowEssenceResource Resource, CorruptionRiskResult? Corruption, string? FailureReason)
        CreateShadowClone(
            ShadowEssenceResource resource,
            Guid ownerId,
            int x,
            int y,
            CloneBehavior behavior,
            LightLevelType lightLevel,
            int activeCloneCount)
    {
        ArgumentNullException.ThrowIfNull(resource);

        // ── Validate clone limit ────────────────────────────────────────────
        if (activeCloneCount >= MaxActiveClones)
        {
            _logger.LogWarning(
                "Shadow Clone failed: Maximum active clones reached ({Max})",
                MaxActiveClones);

            return (null, resource, null,
                $"Maximum active clones reached ({MaxActiveClones})");
        }

        // ── Validate essence ────────────────────────────────────────────────
        if (resource.CurrentEssence < ShadowCloneEssenceCost)
        {
            _logger.LogWarning(
                "Shadow Clone failed: Insufficient Shadow Essence. " +
                "Current: {CurrentEssence}, Required: {RequiredEssence}",
                resource.CurrentEssence, ShadowCloneEssenceCost);

            return (null, resource, null,
                $"Insufficient Shadow Essence ({resource.CurrentEssence}/{ShadowCloneEssenceCost})");
        }

        // ── Spend essence ───────────────────────────────────────────────────
        var (spendSuccess, updatedResource) = resource.TrySpend(ShadowCloneEssenceCost);
        if (!spendSuccess)
        {
            return (null, resource, null,
                $"Failed to spend Shadow Essence ({resource.CurrentEssence}/{ShadowCloneEssenceCost})");
        }

        // ── Create clone ────────────────────────────────────────────────────
        var clone = ShadowClone.Create(ownerId, x, y, behavior);

        // ── Evaluate corruption ─────────────────────────────────────────────
        var corruptionResult = _corruptionService.EvaluateRisk(
            MyrkgengrAbilityId.ShadowClone, lightLevel);

        _logger.LogInformation(
            "Shadow Clone created: Owner {OwnerId}, Position ({X}, {Y}), " +
            "Behavior {Behavior}, Duration {Duration}s, Detection DC {DC}. " +
            "Active clones: {ActiveCount}/{MaxClones}. " +
            "Corruption triggered: {CorruptionTriggered}",
            ownerId, x, y, behavior, ShadowClone.DefaultDurationSeconds,
            ShadowClone.DefaultDetectionDC, activeCloneCount + 1, MaxActiveClones,
            corruptionResult.RiskTriggered);

        return (clone, updatedResource, corruptionResult, null);
    }

    /// <summary>
    /// Destroys a shadow clone (e.g., by taking damage).
    /// </summary>
    /// <param name="clone">The clone to destroy.</param>
    /// <returns>An inactive clone with WasDestroyed = true.</returns>
    public ShadowClone DestroyShadowClone(ShadowClone clone)
    {
        ArgumentNullException.ThrowIfNull(clone);

        var destroyed = clone.Destroy();

        _logger.LogInformation(
            "Shadow Clone destroyed: Clone {CloneId} at ({X}, {Y}) " +
            "owned by {OwnerId}",
            clone.CloneId, clone.X, clone.Y, clone.OwnerId);

        return destroyed;
    }

    /// <summary>
    /// Consumes a shadow clone for Umbral Strike advantage.
    /// </summary>
    /// <param name="clone">The clone to consume.</param>
    /// <returns>An inactive clone with WasConsumed = true.</returns>
    public ShadowClone ConsumeShadowClone(ShadowClone clone)
    {
        ArgumentNullException.ThrowIfNull(clone);

        var consumed = clone.Consume();

        _logger.LogInformation(
            "Shadow Clone consumed for Umbral Strike advantage: " +
            "Clone {CloneId} at ({X}, {Y}) owned by {OwnerId}",
            clone.CloneId, clone.X, clone.Y, clone.OwnerId);

        return consumed;
    }

    // ═══════ Void Touched ═══════

    /// <summary>
    /// Applies the Void Touched passive resistance, halving incoming
    /// Aetheric damage (floor division).
    /// </summary>
    /// <param name="incomingAethericDamage">Raw Aetheric damage before resistance.</param>
    /// <returns>Reduced damage after Void Touched halving.</returns>
    public int ApplyVoidTouched(int incomingAethericDamage)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(incomingAethericDamage);

        var reduced = incomingAethericDamage / 2; // integer division = floor

        _logger.LogInformation(
            "Void Touched applied: {IncomingDamage} Aetheric damage " +
            "reduced to {ReducedDamage} (50% resistance, floor)",
            incomingAethericDamage, reduced);

        return reduced;
    }

    /// <summary>
    /// Whether Void Touched grants advantage on saves vs. Aetheric effects.
    /// </summary>
    /// <returns>Always true when Void Touched is active.</returns>
    public bool GrantsAethericSaveAdvantage() => true;

    // ═══════ Prerequisite Helpers ═══════

    /// <summary>
    /// Checks whether Tier 2 abilities can be unlocked based on PP invested.
    /// Requires 8 PP invested in the Myrk-gengr tree.
    /// </summary>
    /// <param name="ppInvested">Total PP invested.</param>
    /// <returns>True if threshold is met.</returns>
    public bool CanUnlockTier2(int ppInvested)
    {
        var canUnlock = ppInvested >= Tier2PPThreshold;

        _logger.LogDebug(
            "Myrk-gengr Tier 2 unlock check: PP invested {PPInvested}, " +
            "threshold {Threshold}, result {CanUnlock}",
            ppInvested, Tier2PPThreshold, canUnlock);

        return canUnlock;
    }

    /// <summary>
    /// Calculates total PP invested from a list of unlocked abilities.
    /// </summary>
    /// <param name="unlockedAbilities">Currently unlocked abilities.</param>
    /// <returns>Total PP cost of all unlocked abilities.</returns>
    public int CalculatePPInvested(IReadOnlyList<MyrkgengrAbilityId> unlockedAbilities)
    {
        ArgumentNullException.ThrowIfNull(unlockedAbilities);

        var total = 0;
        foreach (var ability in unlockedAbilities)
        {
            total += GetAbilityPPCost(ability);
        }

        _logger.LogDebug(
            "Myrk-gengr PP invested calculation: {AbilityCount} abilities unlocked, " +
            "total PP invested: {TotalPP}",
            unlockedAbilities.Count, total);

        return total;
    }

    /// <summary>
    /// Gets the PP cost for a specific Myrk-gengr ability based on its tier.
    /// </summary>
    /// <param name="abilityId">The ability to look up.</param>
    /// <returns>PP cost: 0 (Tier 1), 4 (Tier 2), 5 (Tier 3), 6 (Capstone).</returns>
    public static int GetAbilityPPCost(MyrkgengrAbilityId abilityId) => abilityId switch
    {
        // Tier 1: Free
        MyrkgengrAbilityId.ShadowStep => 0,
        MyrkgengrAbilityId.CloakOfNight => 0,
        MyrkgengrAbilityId.DarkAdapted => 0,

        // Tier 2: 4 PP each
        MyrkgengrAbilityId.UmbralStrike => 4,
        MyrkgengrAbilityId.ShadowClone => 4,
        MyrkgengrAbilityId.VoidTouched => 4,

        // Tier 3: 5 PP each
        MyrkgengrAbilityId.MergeWithDarkness => 5,
        MyrkgengrAbilityId.ShadowSnare => 5,

        // Capstone: 6 PP
        MyrkgengrAbilityId.Eclipse => 6,

        _ => throw new ArgumentOutOfRangeException(nameof(abilityId), abilityId,
            $"Unknown Myrk-gengr ability: {abilityId}")
    };
}
