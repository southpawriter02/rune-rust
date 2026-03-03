using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service handling Echo-Caller specialization ability execution.
/// Implements Tier 1 (Foundation), Tier 2 (Discipline), Tier 3 (Mastery),
/// and Capstone (Ultimate) ability logic for the Coherent Psychic Artillery path.
/// </summary>
/// <remarks>
/// <para>The Echo-Caller specialization revolves around the [Echo] chain system and [Feared] status manipulation.
/// No Corruption mechanics—this is a Coherent path. Abilities trigger echo chains that propagate damage
/// to adjacent targets and fear effects that increase damage vulnerability.</para>
///
/// <para>Tier 1 abilities:</para>
/// <list type="bullet">
/// <item>Echo Attunement (Passive): -1 Aether cost, +2 Psychic Resistance. No execute method.</item>
/// <item>Scream of Silence (Active): 2d6 Psychic damage, +1d8 vs Feared (R2: +2d8). 2 AP, [Echo] chain.</item>
/// <item>Phantom Menace (Active): Apply [Feared] (3-4 turns). 2 AP, TerrorFeedback grants +15 Aether.</item>
/// </list>
///
/// <para>Tier 2 abilities:</para>
/// <list type="bullet">
/// <item>Echo Cascade (Passive): Enhanced echo chains. Base 1 tile/50%, R2: 2 tiles/70%/2 targets, R3: 3 tiles/80%/2 targets.</item>
/// <item>Reality Fracture (Active): 3d6 damage + [Disoriented] + Push 1 tile. 3 AP, [Echo] chain.</item>
/// <item>Terror Feedback (Passive): +15 Aether when [Feared] is applied.</item>
/// </list>
///
/// <para>Tier 3 abilities:</para>
/// <list type="bullet">
/// <item>Fear Cascade (Active): AoE [Feared] (3 tiles). 4 AP, TerrorFeedback per fear.</item>
/// <item>Echo Displacement (Active): Teleport 2 tiles + [Disoriented]. 4 AP, [Echo] chain.</item>
/// </list>
///
/// <para>Capstone ability:</para>
/// <list type="bullet">
/// <item>Silence Made Weapon (Active): 4d10 base + 2d10 per [Feared] target. 5 AP, once per combat, [Echo] chain.</item>
/// </list>
///
/// <para>Critical design principle: No Corruption. Echo chains are deterministic damage propagation.
/// Fear bonuses and TerrorFeedback are core mechanics. Dice roll methods are marked <c>internal virtual</c>
/// for unit test overriding via <c>TestEchoCallerAbilityService</c>.</para>
/// </remarks>
public class EchoCallerAbilityService : IEchoCallerAbilityService
{
    // ===== Tier 1 Constants =====

    /// <summary>AP cost for Scream of Silence.</summary>
    private const int ScreamOfSilenceApCost = 2;

    /// <summary>AP cost for Phantom Menace.</summary>
    private const int PhantomMenaceApCost = 2;

    // ===== Tier 2 Constants =====

    /// <summary>AP cost for Reality Fracture.</summary>
    private const int RealityFractureApCost = 3;

    // ===== Tier 3 Constants =====

    /// <summary>AP cost for Fear Cascade.</summary>
    private const int FearCascadeApCost = 4;

    /// <summary>AP cost for Echo Displacement.</summary>
    private const int EchoDisplacementApCost = 4;

    // ===== Capstone Constants =====

    /// <summary>AP cost for Silence Made Weapon capstone.</summary>
    private const int SilenceMadeWeaponApCost = 5;

    /// <summary>Aether restoration from TerrorFeedback passive.</summary>
    private const int TerrorFeedbackAetherRestore = 15;

    /// <summary>Bonus damage dice per feared enemy for Silence Made Weapon.</summary>
    private const int SilenceMadeWeaponBonusPerFeared = 2; // d10 per feared, so 2d10 becomes 4d10 with 1 feared

    // ===== PP Requirements =====

    /// <summary>PP threshold for Tier 2 ability unlock.</summary>
    private const int Tier2PpRequirement = 8;

    /// <summary>PP threshold for Tier 3 ability unlock.</summary>
    private const int Tier3PpRequirement = 16;

    /// <summary>PP threshold for Capstone ability unlock.</summary>
    private const int CapstonePpRequirement = 24;

    /// <summary>The specialization ID string for Echo-Caller.</summary>
    private const string EchoCallerSpecId = "echo-caller";

    private readonly ILogger<EchoCallerAbilityService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EchoCallerAbilityService"/> class.
    /// </summary>
    /// <param name="logger">Logger for ability execution events.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
    public EchoCallerAbilityService(ILogger<EchoCallerAbilityService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ===== Tier 1: Scream of Silence (28011) =====

    /// <inheritdoc />
    public ScreamOfSilenceResult? ExecuteScreamOfSilence(Player player, Guid targetId, bool targetIsFeared, int rank = 1)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Guard chain: validate specialization → unlock → AP
        if (!ValidateSpecialization(player, "Scream of Silence")) return null;
        if (!ValidateAbilityUnlocked(player, EchoCallerAbilityId.ScreamOfSilence, "Scream of Silence")) return null;
        if (!ValidateAP(player, ScreamOfSilenceApCost, "Scream of Silence")) return null;

        player.CurrentAP -= ScreamOfSilenceApCost;

        // Base damage: 2d6
        var baseDamage = RollD6() + RollD6();

        // Fear bonus: +1d8 (R2: +2d8)
        var fearBonusDamage = 0;
        if (targetIsFeared)
        {
            fearBonusDamage = rank >= 2
                ? RollD8() + RollD8()  // +2d8 at R2+
                : RollD8();            // +1d8 at R1
        }

        var totalDamage = baseDamage + fearBonusDamage;

        _logger.LogInformation(
            "EchoCaller.ScreamOfSilence: Player {PlayerId} hit target {TargetId} for {BaseDamage} + {FearBonus} = {Total} damage. " +
            "Target was feared: {WasFeared}. Rank: {Rank}",
            player.Id, targetId, baseDamage, fearBonusDamage, totalDamage, targetIsFeared, rank);

        return new ScreamOfSilenceResult
        {
            IsSuccess = true,
            Description = $"Scream of Silence deals {totalDamage} Psychic damage",
            AetherSpent = ScreamOfSilenceApCost,
            DamageDealt = baseDamage,
            FearBonusDamage = fearBonusDamage,
            TargetWasFeared = targetIsFeared,
            TargetName = targetId.ToString(),
            EchoChain = null,  // Echo chain would be processed separately
            AbilityRank = rank
        };
    }

    // ===== Tier 1: Phantom Menace (28012) =====

    /// <inheritdoc />
    public PhantomMenaceResult? ExecutePhantomMenace(Player player, Guid targetId, int rank = 1)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (!ValidateSpecialization(player, "Phantom Menace")) return null;
        if (!ValidateAbilityUnlocked(player, EchoCallerAbilityId.PhantomMenace, "Phantom Menace")) return null;
        if (!ValidateAP(player, PhantomMenaceApCost, "Phantom Menace")) return null;

        player.CurrentAP -= PhantomMenaceApCost;

        // Fear duration: 3 turns base, 4 turns at R2+
        var fearDuration = rank >= 2 ? 4 : 3;

        // TerrorFeedback: check if passive is unlocked
        var hasTerorFeedback = player.HasEchoCallerAbilityUnlocked(EchoCallerAbilityId.TerrorFeedback);
        var aetherRestored = hasTerorFeedback ? TerrorFeedbackAetherRestore : 0;

        // NOTE: Aether restoration is returned in the result for the combat system to apply.
        // The service does not directly mutate player resource state.

        _logger.LogInformation(
            "EchoCaller.PhantomMenace: Player {PlayerId} applied [Feared] x{Duration} to target {TargetId}. " +
            "TerrorFeedback: +{Aether} Aether. Rank: {Rank}",
            player.Id, fearDuration, targetId, aetherRestored, rank);

        return new PhantomMenaceResult
        {
            IsSuccess = true,
            Description = $"Phantom Menace applies [Feared] for {fearDuration} turns",
            AetherSpent = PhantomMenaceApCost,
            FearDuration = fearDuration,
            FearApplied = true,
            TargetName = targetId.ToString(),
            AetherRestored = aetherRestored,
            EchoChain = null,
            AbilityRank = rank
        };
    }

    // ===== Tier 2: Reality Fracture (28014) =====

    /// <inheritdoc />
    public RealityFractureResult? ExecuteRealityFracture(Player player, Guid targetId, bool targetIsFeared, int rank = 1)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (!ValidateSpecialization(player, "Reality Fracture")) return null;
        if (!ValidateAbilityUnlocked(player, EchoCallerAbilityId.RealityFracture, "Reality Fracture")) return null;
        if (!ValidateTierRequirement(player, Tier2PpRequirement, "Reality Fracture", 2)) return null;
        if (!ValidateAP(player, RealityFractureApCost, "Reality Fracture")) return null;

        player.CurrentAP -= RealityFractureApCost;

        // Damage: 3d6
        var damage = RollD6() + RollD6() + RollD6();

        // [Disoriented] is always applied
        var disorientedApplied = true;

        // Push distance: always 1 tile
        var pushDistance = 1;

        _logger.LogInformation(
            "EchoCaller.RealityFracture: Player {PlayerId} hit target {TargetId} for {Damage} damage. " +
            "[Disoriented] applied, pushed {Distance} tile(s). Rank: {Rank}",
            player.Id, targetId, damage, pushDistance, rank);

        return new RealityFractureResult
        {
            IsSuccess = true,
            Description = $"Reality Fracture deals {damage} damage",
            AetherSpent = RealityFractureApCost,
            DamageDealt = damage,
            DisorientedApplied = disorientedApplied,
            PushDistance = pushDistance,
            TargetName = targetId.ToString(),
            EchoChain = null,
            AbilityRank = rank
        };
    }

    // ===== Tier 3: Fear Cascade (28016) =====

    /// <inheritdoc />
    public FearCascadeResult? ExecuteFearCascade(Player player, IReadOnlyList<Guid> targetIds, int rank = 1)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(targetIds);

        if (!ValidateSpecialization(player, "Fear Cascade")) return null;
        if (!ValidateAbilityUnlocked(player, EchoCallerAbilityId.FearCascade, "Fear Cascade")) return null;
        if (!ValidateTierRequirement(player, Tier3PpRequirement, "Fear Cascade", 3)) return null;
        if (!ValidateAP(player, FearCascadeApCost, "Fear Cascade")) return null;

        player.CurrentAP -= FearCascadeApCost;

        // Fear duration: 3 turns base, 4 turns at R2+
        var fearDuration = rank >= 2 ? 4 : 3;

        // All targets affected by the AoE
        var targetsAffected = targetIds.Count;
        var fearsApplied = targetsAffected; // All get feared

        // TerrorFeedback: restore +15 Aether per fear applied
        var hasTerorFeedback = player.HasEchoCallerAbilityUnlocked(EchoCallerAbilityId.TerrorFeedback);
        var totalAetherRestored = hasTerorFeedback ? (fearsApplied * TerrorFeedbackAetherRestore) : 0;

        // NOTE: Aether restoration is returned in the result for the combat system to apply.
        // The service does not directly mutate player resource state.

        var targetNames = targetIds.Select(id => id.ToString()).ToArray();

        _logger.LogInformation(
            "EchoCaller.FearCascade: Player {PlayerId} applied [Feared] x{Duration} to {Count} enemies. " +
            "TerrorFeedback: +{Aether} Aether. Rank: {Rank}",
            player.Id, fearDuration, targetsAffected, totalAetherRestored, rank);

        return new FearCascadeResult
        {
            IsSuccess = true,
            Description = $"Fear Cascade frightens {targetsAffected} enemies",
            AetherSpent = FearCascadeApCost,
            TargetsAffected = targetsAffected,
            FearsApplied = fearsApplied,
            FearDuration = fearDuration,
            AffectedTargetNames = targetNames,
            TotalAetherRestored = totalAetherRestored,
            AbilityRank = rank
        };
    }

    // ===== Tier 3: Echo Displacement (28017) =====

    /// <inheritdoc />
    public EchoDisplacementResult? ExecuteEchoDisplacement(Player player, Guid targetId, int rank = 1)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (!ValidateSpecialization(player, "Echo Displacement")) return null;
        if (!ValidateAbilityUnlocked(player, EchoCallerAbilityId.EchoDisplacement, "Echo Displacement")) return null;
        if (!ValidateTierRequirement(player, Tier3PpRequirement, "Echo Displacement", 3)) return null;
        if (!ValidateAP(player, EchoDisplacementApCost, "Echo Displacement")) return null;

        player.CurrentAP -= EchoDisplacementApCost;

        // Displacement distance: 2 tiles
        var displaced = true;
        var originalPosition = "current location";
        var newPosition = "2 tiles away";

        // [Disoriented] on arrival
        var disorientedApplied = true;

        _logger.LogInformation(
            "EchoCaller.EchoDisplacement: Player {PlayerId} displaced target {TargetId} 2 tiles. " +
            "[Disoriented] applied on arrival. Rank: {Rank}",
            player.Id, targetId, rank);

        return new EchoDisplacementResult
        {
            IsSuccess = true,
            Description = "Echo Displacement forces target away",
            AetherSpent = EchoDisplacementApCost,
            TargetDisplaced = displaced,
            OriginalPosition = originalPosition,
            NewPosition = newPosition,
            DisorientedApplied = disorientedApplied,
            TargetName = targetId.ToString(),
            EchoChain = null,
            AbilityRank = rank
        };
    }

    // ===== Capstone: Silence Made Weapon (28018) =====

    /// <inheritdoc />
    public SilenceMadeWeaponResult? ExecuteSilenceMadeWeapon(
        Player player,
        IReadOnlyList<Guid> targetIds,
        int fearedTargetCount,
        int rank = 1)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(targetIds);

        if (!ValidateSpecialization(player, "Silence Made Weapon")) return null;
        if (!ValidateAbilityUnlocked(player, EchoCallerAbilityId.SilenceMadeWeapon, "Silence Made Weapon")) return null;
        if (!ValidateTierRequirement(player, CapstonePpRequirement, "Silence Made Weapon", 4)) return null;
        if (!ValidateAP(player, SilenceMadeWeaponApCost, "Silence Made Weapon")) return null;

        // Check once-per-combat flag
        if (player.HasUsedSilenceMadeWeaponThisCombat)
        {
            _logger.LogWarning(
                "Silence Made Weapon failed: {Player} ({PlayerId}) has already used capstone this combat",
                player.Name, player.Id);
            return null;
        }

        player.CurrentAP -= SilenceMadeWeaponApCost;
        player.HasUsedSilenceMadeWeaponThisCombat = true;

        // Base damage: 4d10
        var baseDamage = RollD10() + RollD10() + RollD10() + RollD10();

        // Scaling: +2d10 per feared target
        var scalingBonus = 0;
        for (var i = 0; i < fearedTargetCount; i++)
        {
            scalingBonus += RollD10() + RollD10();
        }

        var totalDamage = baseDamage + scalingBonus;
        var targetsHit = targetIds.Count;

        // TerrorFeedback restoration (if applicable)
        var hasTerorFeedback = player.HasEchoCallerAbilityUnlocked(EchoCallerAbilityId.TerrorFeedback);
        var aetherRestored = hasTerorFeedback ? TerrorFeedbackAetherRestore : 0;

        // NOTE: Aether restoration is returned in the result for the combat system to apply.
        // The service does not directly mutate player resource state.

        _logger.LogInformation(
            "EchoCaller.SilenceMadeWeapon: Player {PlayerId} unleashed capstone for {TotalDamage} damage " +
            "({BaseDamage} base + {ScalingBonus} scaling from {FearCount} feared). Targets: {Count}. " +
            "TerrorFeedback: +{Aether} Aether. Rank: {Rank}",
            player.Id, totalDamage, baseDamage, scalingBonus, fearedTargetCount, targetsHit, aetherRestored, rank);

        return new SilenceMadeWeaponResult
        {
            IsSuccess = true,
            Description = $"Silence Made Weapon deals {totalDamage} damage",
            AetherSpent = SilenceMadeWeaponApCost,
            TotalDamage = totalDamage,
            TargetsHit = targetsHit,
            FearedTargetCount = fearedTargetCount,
            FearScalingBonus = scalingBonus,
            FearApplied = false, // Capstone doesn't apply fear, just scales from existing fears
            AetherRestored = aetherRestored,
            EchoChain = null,
            AbilityRank = rank
        };
    }

    // ===== Echo Chain Processing =====

    /// <inheritdoc />
    public EchoChainResult ProcessEchoChain(
        int baseDamage,
        IReadOnlyList<Guid> adjacentTargetIds,
        bool hasEchoCascade,
        int echoCascadeRank = 1)
    {
        ArgumentNullException.ThrowIfNull(adjacentTargetIds);

        if (baseDamage <= 0 || adjacentTargetIds.Count == 0)
        {
            return new EchoChainResult
            {
                ChainDamage = 0,
                ChainTargets = 0,
                ChainRange = 0,
                ChainDamagePercent = 0,
                EchoTargetNames = Array.Empty<string>()
            };
        }

        // Determine echo range and damage based on EchoCascade
        int chainRange;
        int chainDamagePercent;
        int maxChainTargets;

        if (hasEchoCascade)
        {
            if (echoCascadeRank >= 3)
            {
                chainRange = 3;
                chainDamagePercent = 80;
                maxChainTargets = 2;
            }
            else
            {
                chainRange = 2;
                chainDamagePercent = 70;
                maxChainTargets = 2;
            }
        }
        else
        {
            chainRange = 1;
            chainDamagePercent = 50;
            maxChainTargets = 1;
        }

        // Calculate echo damage
        var chainDamage = (baseDamage * chainDamagePercent) / 100;

        // Apply cap on number of targets hit
        var targetsHit = Math.Min(adjacentTargetIds.Count, maxChainTargets);
        var targetNames = adjacentTargetIds.Take(targetsHit).Select(id => id.ToString()).ToArray();

        _logger.LogInformation(
            "EchoCaller.ProcessEchoChain: {Damage} damage propagated to {Targets} adjacent targets " +
            "within {Range} tiles ({Percent}% of base). EchoCascade: {HasCascade} (Rank {Rank})",
            chainDamage, targetsHit, chainRange, chainDamagePercent, hasEchoCascade, echoCascadeRank);

        return new EchoChainResult
        {
            ChainDamage = chainDamage,
            ChainTargets = targetsHit,
            ChainRange = chainRange,
            ChainDamagePercent = chainDamagePercent,
            EchoTargetNames = targetNames
        };
    }

    // ===== Utility Methods =====

    /// <inheritdoc />
    public string GetAbilityReadiness(Player player, EchoCallerAbilityId abilityId)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (!IsEchoCaller(player))
            return "Requires Echo-Caller specialization";

        if (!player.HasEchoCallerAbilityUnlocked(abilityId))
            return $"{abilityId} is not unlocked";

        // Capstone once-per-combat check
        if (abilityId == EchoCallerAbilityId.SilenceMadeWeapon && player.HasUsedSilenceMadeWeaponThisCombat)
            return "Silence Made Weapon already used this combat";

        var ppRequired = GetPpRequiredForAbility(abilityId);
        if (ppRequired > 0)
        {
            var ppInvested = player.GetEchoCallerPPInvested();
            if (ppInvested < ppRequired)
                return $"Insufficient PP ({ppInvested}/{ppRequired})";
        }

        var apCost = GetApCostForAbility(abilityId);
        if (apCost > 0 && player.CurrentAP < apCost)
            return $"Insufficient AP ({player.CurrentAP}/{apCost})";

        return "Ready";
    }

    // ===== Validation Helpers =====

    /// <summary>
    /// Validates that the player has the Echo-Caller specialization.
    /// </summary>
    private bool ValidateSpecialization(Player player, string abilityName)
    {
        if (IsEchoCaller(player)) return true;

        _logger.LogWarning(
            "{Ability} failed: {Player} ({PlayerId}) is not an Echo-Caller (has: {Spec})",
            abilityName, player.Name, player.Id, player.SpecializationId ?? "none");
        return false;
    }

    /// <summary>
    /// Validates that the player has unlocked the specified ability.
    /// </summary>
    private bool ValidateAbilityUnlocked(Player player, EchoCallerAbilityId abilityId, string abilityName)
    {
        if (player.HasEchoCallerAbilityUnlocked(abilityId)) return true;

        _logger.LogWarning(
            "{Ability} failed: {Player} ({PlayerId}) has not unlocked {AbilityId}",
            abilityName, player.Name, player.Id, abilityId);
        return false;
    }

    /// <summary>
    /// Validates that the player meets the PP investment requirement for a tier.
    /// </summary>
    private bool ValidateTierRequirement(Player player, int requiredPP, string abilityName, int tier)
    {
        var invested = player.GetEchoCallerPPInvested();
        if (invested >= requiredPP) return true;

        _logger.LogWarning(
            "{Ability} failed: {Player} ({PlayerId}) needs {Required} PP invested for Tier {Tier}, " +
            "has {Invested}",
            abilityName, player.Name, player.Id, requiredPP, tier, invested);
        return false;
    }

    /// <summary>
    /// Validates that the player has enough AP for the ability.
    /// </summary>
    private bool ValidateAP(Player player, int requiredAP, string abilityName)
    {
        if (player.CurrentAP >= requiredAP) return true;

        _logger.LogWarning(
            "{Ability} failed: {Player} ({PlayerId}) has insufficient AP (need {Required}, have {Available})",
            abilityName, player.Name, player.Id, requiredAP, player.CurrentAP);
        return false;
    }

    /// <summary>
    /// Checks if the player is an Echo-Caller (case-insensitive comparison).
    /// </summary>
    private static bool IsEchoCaller(Player player)
    {
        return string.Equals(player.SpecializationId, EchoCallerSpecId, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the AP cost for a given ability.
    /// </summary>
    private static int GetApCostForAbility(EchoCallerAbilityId abilityId)
    {
        return abilityId switch
        {
            EchoCallerAbilityId.ScreamOfSilence => ScreamOfSilenceApCost,
            EchoCallerAbilityId.PhantomMenace => PhantomMenaceApCost,
            EchoCallerAbilityId.RealityFracture => RealityFractureApCost,
            EchoCallerAbilityId.FearCascade => FearCascadeApCost,
            EchoCallerAbilityId.EchoDisplacement => EchoDisplacementApCost,
            EchoCallerAbilityId.SilenceMadeWeapon => SilenceMadeWeaponApCost,
            _ => 0 // Passive abilities have no AP cost
        };
    }

    /// <summary>
    /// Gets the PP requirement for a given ability (0 if Tier 1, 8 for Tier 2, 16 for Tier 3, 24 for Capstone).
    /// </summary>
    private static int GetPpRequiredForAbility(EchoCallerAbilityId abilityId)
    {
        return abilityId switch
        {
            // Tier 1: no requirement
            EchoCallerAbilityId.EchoAttunement => 0,
            EchoCallerAbilityId.ScreamOfSilence => 0,
            EchoCallerAbilityId.PhantomMenace => 0,
            // Tier 2: 8 PP
            EchoCallerAbilityId.EchoCascade => Tier2PpRequirement,
            EchoCallerAbilityId.RealityFracture => Tier2PpRequirement,
            EchoCallerAbilityId.TerrorFeedback => Tier2PpRequirement,
            // Tier 3: 16 PP
            EchoCallerAbilityId.FearCascade => Tier3PpRequirement,
            EchoCallerAbilityId.EchoDisplacement => Tier3PpRequirement,
            // Capstone: 24 PP
            EchoCallerAbilityId.SilenceMadeWeapon => CapstonePpRequirement,
            _ => 0
        };
    }

    // ===== Dice Roll Methods (internal virtual for test overriding) =====

    /// <summary>
    /// Rolls a single d6 (1-6). Override in test subclass for deterministic results.
    /// </summary>
    internal virtual int RollD6()
    {
        return Random.Shared.Next(1, 7);
    }

    /// <summary>
    /// Rolls a single d8 (1-8). Override in test subclass for deterministic results.
    /// </summary>
    internal virtual int RollD8()
    {
        return Random.Shared.Next(1, 9);
    }

    /// <summary>
    /// Rolls a single d10 (1-10). Override in test subclass for deterministic results.
    /// </summary>
    internal virtual int RollD10()
    {
        return Random.Shared.Next(1, 11);
    }
}
