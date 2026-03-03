using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Result of executing Corrosive Curse (25002): applies [Corroded] stacks to a target.
/// Rank 1: 1 stack, Rank 2: 2 stacks, Rank 3: 3 stacks. Cost: 2 AP. Self-Corruption: +2 (R3: +1).
/// </summary>
public sealed record CorrosiveCurseResult
{
    public bool IsSuccess { get; init; }
    public string Description { get; init; } = string.Empty;
    public int AetherSpent { get; init; }
    public int StacksApplied { get; init; }
    public int TotalStacksOnTarget { get; init; }
    public bool WasStackCapped { get; init; }
    public string TargetName { get; init; } = string.Empty;
    public int CorruptionGained { get; init; }
    public RustWitchCorruptionTrigger? Trigger { get; init; }
    public int AbilityRank { get; init; }

    public string GetDescription()
    {
        var capWarning = WasStackCapped ? " [STACK CAP REACHED]" : "";
        var corruptionTag = CorruptionGained > 0 ? $" [SELF-CORRUPTION +{CorruptionGained}]" : "";
        return $"Corrosive Curse applies {StacksApplied} [Corroded] stack(s) to {TargetName}. " +
               $"Total stacks: {TotalStacksOnTarget}/5{capWarning}{corruptionTag}";
    }
}

/// <summary>
/// Result of executing System Shock (25004): applies [Corroded] + [Stunned] (Mechanical only).
/// Cost: 3 AP. Self-Corruption: +3 (R3: +2).
/// </summary>
public sealed record SystemShockResult
{
    public bool IsSuccess { get; init; }
    public string Description { get; init; } = string.Empty;
    public int AetherSpent { get; init; }
    public int DamageDealt { get; init; }
    public int StacksApplied { get; init; }
    public int TotalStacksOnTarget { get; init; }
    public bool TargetStunned { get; init; }
    public bool TargetIsMechanical { get; init; }
    public string TargetName { get; init; } = string.Empty;
    public int CorruptionGained { get; init; }
    public RustWitchCorruptionTrigger? Trigger { get; init; }
    public int AbilityRank { get; init; }

    public string GetDescription()
    {
        var stunTag = TargetStunned ? " [STUNNED]" : "";
        var corruptionTag = CorruptionGained > 0 ? $" [SELF-CORRUPTION +{CorruptionGained}]" : "";
        return $"System Shock hits {TargetName} for {DamageDealt} damage, " +
               $"applies {StacksApplied} [Corroded] stack(s){stunTag}{corruptionTag}";
    }
}

/// <summary>
/// Result of executing Flash Rust (25005): AoE [Corroded] to all enemies.
/// Cost: 4 AP. Self-Corruption: +4 (R3: +3).
/// </summary>
public sealed record FlashRustResult
{
    public bool IsSuccess { get; init; }
    public string Description { get; init; } = string.Empty;
    public int AetherSpent { get; init; }
    public int StacksPerTarget { get; init; }
    public int TargetsAffected { get; init; }
    public string[] AffectedTargetNames { get; init; } = Array.Empty<string>();
    public int CorruptionGained { get; init; }
    public RustWitchCorruptionTrigger? Trigger { get; init; }
    public int AbilityRank { get; init; }

    public string GetDescription()
    {
        var corruptionTag = CorruptionGained > 0 ? $" [SELF-CORRUPTION +{CorruptionGained}]" : "";
        return $"Flash Rust applies {StacksPerTarget} [Corroded] stack(s) to {TargetsAffected} " +
               $"enemies{corruptionTag}";
    }
}

/// <summary>
/// Result of executing Unmaking Word (25007): doubles [Corroded] stacks on target (capped at 5).
/// Cost: 4 AP. Self-Corruption: +4 (all ranks).
/// </summary>
public sealed record UnmakingWordResult
{
    public bool IsSuccess { get; init; }
    public string Description { get; init; } = string.Empty;
    public int AetherSpent { get; init; }
    public int StacksBefore { get; init; }
    public int StacksAfter { get; init; }
    public int EffectiveStacksGained { get; init; }
    public bool WasStackCapped { get; init; }
    public string TargetName { get; init; } = string.Empty;
    public int CorruptionGained { get; init; }
    public RustWitchCorruptionTrigger? Trigger { get; init; }
    public int AbilityRank { get; init; }

    public string GetDescription()
    {
        var capWarning = WasStackCapped ? " [STACK CAP REACHED]" : "";
        var corruptionTag = CorruptionGained > 0 ? $" [SELF-CORRUPTION +{CorruptionGained}]" : "";
        return $"Unmaking Word doubles {TargetName}'s [Corroded] stacks: " +
               $"{StacksBefore} → {StacksAfter}{capWarning}{corruptionTag}";
    }
}

/// <summary>
/// Result of executing Entropic Cascade (25009): execute threshold OR 6d6 Arcane damage.
/// Cost: 5 AP. Self-Corruption: +6 (all ranks). Capstone ability.
/// </summary>
public sealed record EntropicCascadeResult
{
    public bool IsSuccess { get; init; }
    public string Description { get; init; } = string.Empty;
    public int AetherSpent { get; init; }
    public bool WasExecute { get; init; }
    public int DamageDealt { get; init; }
    public int TargetCorrodedStacks { get; init; }
    public int TargetCorruption { get; init; }
    public string ExecuteReason { get; init; } = string.Empty;
    public string TargetName { get; init; } = string.Empty;
    public int CorruptionGained { get; init; }
    public RustWitchCorruptionTrigger? Trigger { get; init; }
    public int AbilityRank { get; init; }

    public string GetDescription()
    {
        var corruptionTag = CorruptionGained > 0 ? $" [SELF-CORRUPTION +{CorruptionGained}]" : "";
        if (WasExecute)
        {
            return $"Entropic Cascade EXECUTES {TargetName}! ({ExecuteReason}){corruptionTag}";
        }
        return $"Entropic Cascade deals {DamageDealt} Arcane damage to {TargetName}. " +
               $"(No execute: {TargetCorrodedStacks}/5 stacks, {TargetCorruption} Corruption){corruptionTag}";
    }
}

/// <summary>
/// Result of a [Corroded] DoT tick on a target during turn processing.
/// Not an ability execution result — this tracks ongoing damage from existing stacks.
/// </summary>
public sealed record CorrodedDotTickResult
{
    public bool IsSuccess { get; init; }
    public string Description { get; init; } = string.Empty;
    public Guid TargetId { get; init; }
    public string TargetName { get; init; } = string.Empty;
    public int StackCount { get; init; }
    public int DamagePerStack { get; init; }
    public int TotalDamage { get; init; }
    public int ArmorPenalty { get; init; }
    public bool HasAcceleratedEntropy { get; init; }

    public string GetDescription()
    {
        var diceType = HasAcceleratedEntropy ? "2d6" : "1d4";
        return $"[Corroded] x{StackCount} ticks on {TargetName}: " +
               $"{StackCount} × {diceType} = {TotalDamage} damage ({ArmorPenalty} Armor penalty)";
    }
}

/// <summary>
/// Result of Cascade Reaction (25008) triggering on enemy death: [Corroded] stacks spread.
/// </summary>
public sealed record CascadeReactionResult
{
    public bool IsSuccess { get; init; }
    public string Description { get; init; } = string.Empty;
    public string DeadTargetName { get; init; } = string.Empty;
    public int StacksSpread { get; init; }
    public int TargetsAffected { get; init; }
    public string[] AffectedTargetNames { get; init; } = Array.Empty<string>();

    public string GetDescription()
    {
        return $"Cascade Reaction: {DeadTargetName}'s death spreads {StacksSpread} [Corroded] " +
               $"stacks to {TargetsAffected} adjacent enemies";
    }
}
