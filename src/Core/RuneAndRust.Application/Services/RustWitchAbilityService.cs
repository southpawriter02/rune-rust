using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service handling Rust-Witch specialization ability execution.
/// Implements Tier 1 (Foundation), Tier 2 (Discipline), Tier 3 (Mastery),
/// and Capstone (Ultimate) ability logic for the Heretical Debuffer/DoT path.
/// </summary>
/// <remarks>
/// <para>The Rust-Witch specialization revolves around the [Corroded] stacking DoT:
/// max 5 stacks per target, persistent until cleansed, with cumulative damage and Armor penalties.</para>
///
/// <para>Tier 1 abilities:</para>
/// <list type="bullet">
/// <item>Philosopher of Dust (Passive): +bonus dice vs corrupted entities. No execute method.</item>
/// <item>Corrosive Curse (Active): Apply [Corroded] stacks (1/2/3 by rank). 2 AP, +2 Corruption (R3: +1).</item>
/// <item>Entropic Field (Passive): Aura granting -1 Armor per [Corroded] stack. No execute method.</item>
/// </list>
///
/// <para>Tier 2 abilities:</para>
/// <list type="bullet">
/// <item>System Shock (Active): [Corroded] + [Stunned] on Mechanical. 3 AP, +3 Corruption (R3: +2).</item>
/// <item>Flash Rust (Active): AoE [Corroded] to all enemies. 4 AP, +4 Corruption (R3: +3).</item>
/// <item>Accelerated Entropy (Passive): [Corroded] DoT upgrades from 1d4 to 2d6/stack. No execute method.</item>
/// </list>
///
/// <para>Tier 3 abilities:</para>
/// <list type="bullet">
/// <item>Unmaking Word (Active): Doubles [Corroded] stacks (capped at 5). 4 AP, +4 Corruption.</item>
/// <item>Cascade Reaction (Passive): Spreads [Corroded] on enemy death. Handled via ProcessCascadeReaction.</item>
/// </list>
///
/// <para>Capstone ability:</para>
/// <list type="bullet">
/// <item>Entropic Cascade (Active): Execute if target has &gt;50 Corruption OR 5 [Corroded] stacks,
///   otherwise 6d6 Arcane damage. 5 AP, +6 Corruption.</item>
/// </list>
///
/// <para>Critical design principle: Corruption evaluation happens BEFORE resource spending.
/// Self-Corruption is deterministic — no dice roll, fixed amounts per ability and rank.</para>
///
/// <para>Dice roll methods are marked <c>internal virtual</c> for unit test overriding via
/// <c>TestRustWitchAbilityService</c>.</para>
/// </remarks>
public class RustWitchAbilityService : IRustWitchAbilityService
{
    // ===== Tier 1 Constants =====

    /// <summary>AP cost for Corrosive Curse.</summary>
    private const int CorrosiveCurseApCost = 2;

    // ===== Tier 2 Constants =====

    /// <summary>AP cost for System Shock.</summary>
    private const int SystemShockApCost = 3;

    /// <summary>AP cost for Flash Rust.</summary>
    private const int FlashRustApCost = 4;

    // ===== Tier 3 Constants =====

    /// <summary>AP cost for Unmaking Word.</summary>
    private const int UnmakingWordApCost = 4;

    // ===== Capstone Constants =====

    /// <summary>AP cost for Entropic Cascade capstone.</summary>
    private const int EntropicCascadeApCost = 5;

    /// <summary>Corruption threshold for Entropic Cascade execute mechanic.</summary>
    private const int ExecuteCorruptionThreshold = 50;

    /// <summary>Maximum [Corroded] stacks for execute threshold check.</summary>
    private const int ExecuteStackThreshold = CorrodedStackTracker.MaxStacks;

    // ===== PP Requirements =====

    /// <summary>PP threshold for Tier 2 ability unlock.</summary>
    private const int Tier2PpRequirement = 8;

    /// <summary>PP threshold for Tier 3 ability unlock.</summary>
    private const int Tier3PpRequirement = 16;

    /// <summary>PP threshold for Capstone ability unlock.</summary>
    private const int CapstonePpRequirement = 24;

    /// <summary>The specialization ID string for Rust-Witch.</summary>
    private const string RustWitchSpecId = "rust-witch";

    private readonly IRustWitchCorruptionService _corruptionService;
    private readonly ILogger<RustWitchAbilityService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RustWitchAbilityService"/> class.
    /// </summary>
    /// <param name="corruptionService">Service for deterministic Corruption evaluation.</param>
    /// <param name="logger">Logger for ability execution events.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public RustWitchAbilityService(
        IRustWitchCorruptionService corruptionService,
        ILogger<RustWitchAbilityService> logger)
    {
        _corruptionService = corruptionService ?? throw new ArgumentNullException(nameof(corruptionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ===== Tier 1: Corrosive Curse (25002) =====

    /// <inheritdoc />
    public CorrosiveCurseResult? ExecuteCorrosiveCurse(Player player, Guid targetId, int currentTargetStacks = 0, int rank = 1)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Guard chain: validate specialization → unlock → AP
        if (!ValidateSpecialization(player, "Corrosive Curse")) return null;
        if (!ValidateAbilityUnlocked(player, RustWitchAbilityId.CorrosiveCurse, "Corrosive Curse")) return null;
        if (!ValidateAP(player, CorrosiveCurseApCost, "Corrosive Curse")) return null;

        // Evaluate corruption BEFORE spending resources (deterministic)
        var corruptionResult = _corruptionService.EvaluateRisk(RustWitchAbilityId.CorrosiveCurse, rank);

        // Deduct AP
        player.CurrentAP -= CorrosiveCurseApCost;

        // Calculate stacks to apply based on rank
        var stacksToApply = rank switch
        {
            1 => 1,
            2 => 2,
            >= 3 => 3,
            _ => 1
        };

        // Apply stacks (capped at max)
        var newTotal = Math.Min(currentTargetStacks + stacksToApply, CorrodedStackTracker.MaxStacks);
        var actualStacksApplied = newTotal - currentTargetStacks;
        var wasCapped = (currentTargetStacks + stacksToApply) > CorrodedStackTracker.MaxStacks;

        // Apply self-corruption (via corruption service — Player.Corruption is not modified directly)
        if (corruptionResult.IsTriggered)
        {
            _corruptionService.ApplyCorruption(player.Id, corruptionResult);
        }

        _logger.LogInformation(
            "RustWitch.CorrosiveCurse: Player {PlayerId} applied {Stacks} [Corroded] to target {TargetId}. " +
            "Total stacks: {Total}/{Max}. Self-Corruption: +{Corruption}",
            player.Id, actualStacksApplied, targetId, newTotal, CorrodedStackTracker.MaxStacks,
            corruptionResult.CorruptionAmount);

        return new CorrosiveCurseResult
        {
            IsSuccess = true,
            Description = $"Corrosive Curse applies {actualStacksApplied} [Corroded] stack(s)",
            AetherSpent = CorrosiveCurseApCost,
            StacksApplied = actualStacksApplied,
            TotalStacksOnTarget = newTotal,
            WasStackCapped = wasCapped,
            TargetName = targetId.ToString(),
            CorruptionGained = corruptionResult.CorruptionAmount,
            Trigger = corruptionResult.Trigger,
            AbilityRank = rank
        };
    }

    // ===== Tier 2: System Shock (25004) =====

    /// <inheritdoc />
    public SystemShockResult? ExecuteSystemShock(Player player, Guid targetId, bool targetIsMechanical, int currentTargetStacks = 0, int rank = 1)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (!ValidateSpecialization(player, "System Shock")) return null;
        if (!ValidateAbilityUnlocked(player, RustWitchAbilityId.SystemShock, "System Shock")) return null;
        if (!ValidateTierRequirement(player, Tier2PpRequirement, "System Shock", 2)) return null;
        if (!ValidateAP(player, SystemShockApCost, "System Shock")) return null;

        var corruptionResult = _corruptionService.EvaluateRisk(RustWitchAbilityId.SystemShock, rank);

        player.CurrentAP -= SystemShockApCost;

        // Roll damage: 2d6 base, scaling with rank
        var damageRoll = RollD6() + RollD6();
        var damageBonus = (rank - 1) * 2; // +0 R1, +2 R2, +4 R3
        var totalDamage = damageRoll + damageBonus;

        // Apply [Corroded] stack
        var newTotal = Math.Min(currentTargetStacks + 1, CorrodedStackTracker.MaxStacks);
        var stackApplied = newTotal > currentTargetStacks ? 1 : 0;

        // [Stunned] only on Mechanical targets
        var targetStunned = targetIsMechanical;

        if (corruptionResult.IsTriggered)
        {
            _corruptionService.ApplyCorruption(player.Id, corruptionResult);
        }

        _logger.LogInformation(
            "RustWitch.SystemShock: Player {PlayerId} hit target {TargetId} for {Damage} damage. " +
            "[Corroded] +{Stacks}, Stunned={Stunned} (Mechanical={IsMechanical}). Self-Corruption: +{Corruption}",
            player.Id, targetId, totalDamage, stackApplied, targetStunned, targetIsMechanical,
            corruptionResult.CorruptionAmount);

        return new SystemShockResult
        {
            IsSuccess = true,
            Description = $"System Shock deals {totalDamage} damage",
            AetherSpent = SystemShockApCost,
            DamageDealt = totalDamage,
            StacksApplied = stackApplied,
            TotalStacksOnTarget = newTotal,
            TargetStunned = targetStunned,
            TargetIsMechanical = targetIsMechanical,
            TargetName = targetId.ToString(),
            CorruptionGained = corruptionResult.CorruptionAmount,
            Trigger = corruptionResult.Trigger,
            AbilityRank = rank
        };
    }

    // ===== Tier 2: Flash Rust (25005) =====

    /// <inheritdoc />
    public FlashRustResult? ExecuteFlashRust(Player player, IReadOnlyList<Guid> targetIds, int rank = 1)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(targetIds);

        if (!ValidateSpecialization(player, "Flash Rust")) return null;
        if (!ValidateAbilityUnlocked(player, RustWitchAbilityId.FlashRust, "Flash Rust")) return null;
        if (!ValidateTierRequirement(player, Tier2PpRequirement, "Flash Rust", 2)) return null;
        if (!ValidateAP(player, FlashRustApCost, "Flash Rust")) return null;

        var corruptionResult = _corruptionService.EvaluateRisk(RustWitchAbilityId.FlashRust, rank);

        player.CurrentAP -= FlashRustApCost;

        // Stacks per target based on rank: R1=1, R2=1, R3=2
        var stacksPerTarget = rank >= 3 ? 2 : 1;

        if (corruptionResult.IsTriggered)
        {
            _corruptionService.ApplyCorruption(player.Id, corruptionResult);
        }

        var targetNames = targetIds.Select(id => id.ToString()).ToArray();

        _logger.LogInformation(
            "RustWitch.FlashRust: Player {PlayerId} applied {StacksPerTarget} [Corroded] stack(s) " +
            "to {TargetCount} enemies. Self-Corruption: +{Corruption}",
            player.Id, stacksPerTarget, targetIds.Count, corruptionResult.CorruptionAmount);

        return new FlashRustResult
        {
            IsSuccess = true,
            Description = $"Flash Rust corrodes {targetIds.Count} enemies",
            AetherSpent = FlashRustApCost,
            StacksPerTarget = stacksPerTarget,
            TargetsAffected = targetIds.Count,
            AffectedTargetNames = targetNames,
            CorruptionGained = corruptionResult.CorruptionAmount,
            Trigger = corruptionResult.Trigger,
            AbilityRank = rank
        };
    }

    // ===== Tier 3: Unmaking Word (25007) =====

    /// <inheritdoc />
    public UnmakingWordResult? ExecuteUnmakingWord(Player player, Guid targetId, int currentTargetStacks, int rank = 1)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (!ValidateSpecialization(player, "Unmaking Word")) return null;
        if (!ValidateAbilityUnlocked(player, RustWitchAbilityId.UnmakingWord, "Unmaking Word")) return null;
        if (!ValidateTierRequirement(player, Tier3PpRequirement, "Unmaking Word", 3)) return null;
        if (!ValidateAP(player, UnmakingWordApCost, "Unmaking Word")) return null;

        var corruptionResult = _corruptionService.EvaluateRisk(RustWitchAbilityId.UnmakingWord, rank);

        player.CurrentAP -= UnmakingWordApCost;

        // Double stacks, capped at max
        var doubled = Math.Min(currentTargetStacks * 2, CorrodedStackTracker.MaxStacks);
        var effectiveGain = doubled - currentTargetStacks;
        var wasCapped = (currentTargetStacks * 2) > CorrodedStackTracker.MaxStacks;

        if (corruptionResult.IsTriggered)
        {
            _corruptionService.ApplyCorruption(player.Id, corruptionResult);
        }

        _logger.LogInformation(
            "RustWitch.UnmakingWord: Player {PlayerId} doubled [Corroded] on target {TargetId}: " +
            "{Before} → {After} (capped={Capped}). Self-Corruption: +{Corruption}",
            player.Id, targetId, currentTargetStacks, doubled, wasCapped,
            corruptionResult.CorruptionAmount);

        return new UnmakingWordResult
        {
            IsSuccess = true,
            Description = $"Unmaking Word doubles [Corroded] stacks: {currentTargetStacks} → {doubled}",
            AetherSpent = UnmakingWordApCost,
            StacksBefore = currentTargetStacks,
            StacksAfter = doubled,
            EffectiveStacksGained = effectiveGain,
            WasStackCapped = wasCapped,
            TargetName = targetId.ToString(),
            CorruptionGained = corruptionResult.CorruptionAmount,
            Trigger = corruptionResult.Trigger,
            AbilityRank = rank
        };
    }

    // ===== Capstone: Entropic Cascade (25009) =====

    /// <inheritdoc />
    public EntropicCascadeResult? ExecuteEntropicCascade(
        Player player,
        Guid targetId,
        int targetCorrodedStacks,
        int targetCorruption,
        int rank = 1)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (!ValidateSpecialization(player, "Entropic Cascade")) return null;
        if (!ValidateAbilityUnlocked(player, RustWitchAbilityId.EntropicCascade, "Entropic Cascade")) return null;
        if (!ValidateTierRequirement(player, CapstonePpRequirement, "Entropic Cascade", 4)) return null;
        if (!ValidateAP(player, EntropicCascadeApCost, "Entropic Cascade")) return null;

        var corruptionResult = _corruptionService.EvaluateRisk(RustWitchAbilityId.EntropicCascade, rank);

        player.CurrentAP -= EntropicCascadeApCost;

        // Check execute threshold: >50 Corruption OR 5 [Corroded] stacks
        var meetsCorruptionThreshold = targetCorruption > ExecuteCorruptionThreshold;
        var meetsStackThreshold = targetCorrodedStacks >= ExecuteStackThreshold;
        var isExecute = meetsCorruptionThreshold || meetsStackThreshold;

        var executeReason = isExecute
            ? (meetsStackThreshold
                ? $"Target has {targetCorrodedStacks}/{ExecuteStackThreshold} [Corroded] stacks"
                : $"Target Corruption ({targetCorruption}) exceeds threshold ({ExecuteCorruptionThreshold})")
            : string.Empty;

        // If not execute, deal 6d6 Arcane damage
        var damage = 0;
        if (!isExecute)
        {
            damage = RollD6() + RollD6() + RollD6() + RollD6() + RollD6() + RollD6();
        }

        if (corruptionResult.IsTriggered)
        {
            _corruptionService.ApplyCorruption(player.Id, corruptionResult);
        }

        if (isExecute)
        {
            _logger.LogInformation(
                "RustWitch.EntropicCascade: Player {PlayerId} EXECUTES target {TargetId}! " +
                "Reason: {ExecuteReason}. Self-Corruption: +{Corruption}",
                player.Id, targetId, executeReason, corruptionResult.CorruptionAmount);
        }
        else
        {
            _logger.LogInformation(
                "RustWitch.EntropicCascade: Player {PlayerId} deals {Damage} Arcane to target {TargetId}. " +
                "No execute ({Stacks}/5 stacks, {TargetCorruption} Corruption). Self-Corruption: +{Corruption}",
                player.Id, damage, targetId, targetCorrodedStacks, targetCorruption,
                corruptionResult.CorruptionAmount);
        }

        return new EntropicCascadeResult
        {
            IsSuccess = true,
            Description = isExecute ? $"Entropic Cascade EXECUTES target!" : $"Entropic Cascade deals {damage} Arcane damage",
            AetherSpent = EntropicCascadeApCost,
            WasExecute = isExecute,
            DamageDealt = damage,
            TargetCorrodedStacks = targetCorrodedStacks,
            TargetCorruption = targetCorruption,
            ExecuteReason = executeReason,
            TargetName = targetId.ToString(),
            CorruptionGained = corruptionResult.CorruptionAmount,
            Trigger = corruptionResult.Trigger,
            AbilityRank = rank
        };
    }

    // ===== Passive & Utility Methods =====

    /// <inheritdoc />
    public CorrodedDotTickResult ProcessCorrodedDotTick(CorrodedStackTracker tracker, bool hasAcceleratedEntropy)
    {
        ArgumentNullException.ThrowIfNull(tracker);

        if (tracker.CurrentStacks <= 0)
        {
            return new CorrodedDotTickResult
            {
                IsSuccess = true,
                Description = "No [Corroded] stacks — no DoT damage",
                TargetId = tracker.TargetId,
                TargetName = tracker.TargetName,
                StackCount = 0,
                DamagePerStack = 0,
                TotalDamage = 0,
                ArmorPenalty = 0,
                HasAcceleratedEntropy = hasAcceleratedEntropy
            };
        }

        // Calculate damage per stack
        var totalDamage = 0;
        for (var i = 0; i < tracker.CurrentStacks; i++)
        {
            totalDamage += hasAcceleratedEntropy
                ? RollD6() + RollD6()  // 2d6 per stack with Accelerated Entropy
                : RollD4();            // 1d4 per stack base
        }

        _logger.LogDebug(
            "RustWitch.CorrodedDotTick: Target {TargetId} ({TargetName}) takes {Damage} damage " +
            "from {Stacks} [Corroded] stacks (AcceleratedEntropy={HasAE})",
            tracker.TargetId, tracker.TargetName, totalDamage, tracker.CurrentStacks,
            hasAcceleratedEntropy);

        return new CorrodedDotTickResult
        {
            IsSuccess = true,
            Description = $"[Corroded] x{tracker.CurrentStacks} deals {totalDamage} damage",
            TargetId = tracker.TargetId,
            TargetName = tracker.TargetName,
            StackCount = tracker.CurrentStacks,
            DamagePerStack = totalDamage / tracker.CurrentStacks,
            TotalDamage = totalDamage,
            ArmorPenalty = tracker.ArmorPenalty,
            HasAcceleratedEntropy = hasAcceleratedEntropy
        };
    }

    /// <inheritdoc />
    public CascadeReactionResult ProcessCascadeReaction(
        CorrodedStackTracker deadTracker,
        IReadOnlyList<Guid> adjacentTargetIds)
    {
        ArgumentNullException.ThrowIfNull(deadTracker);
        ArgumentNullException.ThrowIfNull(adjacentTargetIds);

        if (deadTracker.CurrentStacks <= 0 || adjacentTargetIds.Count == 0)
        {
            return new CascadeReactionResult
            {
                IsSuccess = true,
                Description = "No stacks to spread or no adjacent targets",
                DeadTargetName = deadTracker.TargetName,
                StacksSpread = 0,
                TargetsAffected = 0,
                AffectedTargetNames = Array.Empty<string>()
            };
        }

        var targetNames = adjacentTargetIds.Select(id => id.ToString()).ToArray();

        _logger.LogInformation(
            "RustWitch.CascadeReaction: {DeadTarget} death spreads {Stacks} [Corroded] stacks " +
            "to {TargetCount} adjacent enemies",
            deadTracker.TargetName, deadTracker.CurrentStacks, adjacentTargetIds.Count);

        return new CascadeReactionResult
        {
            IsSuccess = true,
            Description = $"Cascade Reaction spreads {deadTracker.CurrentStacks} [Corroded] stacks",
            DeadTargetName = deadTracker.TargetName,
            StacksSpread = deadTracker.CurrentStacks,
            TargetsAffected = adjacentTargetIds.Count,
            AffectedTargetNames = targetNames
        };
    }

    /// <inheritdoc />
    public string GetAbilityReadiness(Player player, RustWitchAbilityId abilityId)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (!IsRustWitch(player))
            return "Requires Rust-Witch specialization";

        if (!player.HasRustWitchAbilityUnlocked(abilityId))
            return $"{abilityId} is not unlocked";

        var apCost = GetApCostForAbility(abilityId);
        if (apCost > 0 && player.CurrentAP < apCost)
            return $"Insufficient AP ({player.CurrentAP}/{apCost})";

        return "Ready";
    }

    // ===== Validation Helpers =====

    /// <summary>
    /// Validates that the player has the Rust-Witch specialization.
    /// </summary>
    private bool ValidateSpecialization(Player player, string abilityName)
    {
        if (IsRustWitch(player)) return true;

        _logger.LogWarning(
            "{Ability} failed: {Player} ({PlayerId}) is not a Rust-Witch (has: {Spec})",
            abilityName, player.Name, player.Id, player.SpecializationId ?? "none");
        return false;
    }

    /// <summary>
    /// Validates that the player has unlocked the specified ability.
    /// </summary>
    private bool ValidateAbilityUnlocked(Player player, RustWitchAbilityId abilityId, string abilityName)
    {
        if (player.HasRustWitchAbilityUnlocked(abilityId)) return true;

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
        var invested = player.GetRustWitchPPInvested();
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
    /// Checks if the player is a Rust-Witch (case-insensitive comparison).
    /// </summary>
    private static bool IsRustWitch(Player player)
    {
        return string.Equals(player.SpecializationId, RustWitchSpecId, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the AP cost for a given ability.
    /// </summary>
    private static int GetApCostForAbility(RustWitchAbilityId abilityId)
    {
        return abilityId switch
        {
            RustWitchAbilityId.CorrosiveCurse => CorrosiveCurseApCost,
            RustWitchAbilityId.SystemShock => SystemShockApCost,
            RustWitchAbilityId.FlashRust => FlashRustApCost,
            RustWitchAbilityId.UnmakingWord => UnmakingWordApCost,
            RustWitchAbilityId.EntropicCascade => EntropicCascadeApCost,
            _ => 0 // Passive abilities have no AP cost
        };
    }

    // ===== Dice Roll Methods (internal virtual for test overriding) =====

    /// <summary>
    /// Rolls a single d4 (1-4). Override in test subclass for deterministic results.
    /// </summary>
    internal virtual int RollD4()
    {
        return Random.Shared.Next(1, 5);
    }

    /// <summary>
    /// Rolls a single d6 (1-6). Override in test subclass for deterministic results.
    /// </summary>
    internal virtual int RollD6()
    {
        return Random.Shared.Next(1, 7);
    }
}
