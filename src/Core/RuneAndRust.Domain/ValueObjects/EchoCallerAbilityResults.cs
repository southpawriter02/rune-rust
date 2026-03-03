using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Result of executing Scream of Silence (28011): psychic damage with optional fear bonus and echo chains.
/// Base Damage: 2d6. If target is [Feared]: +1d8 (R2: +2d8). Costs 2 AP.
/// </summary>
public sealed record ScreamOfSilenceResult
{
    /// <summary>Whether the ability executed successfully.</summary>
    public bool IsSuccess { get; init; }

    /// <summary>Human-readable description of the ability outcome.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>AP spent to execute this ability.</summary>
    public int AetherSpent { get; init; }

    /// <summary>Damage dealt to the primary target.</summary>
    public int DamageDealt { get; init; }

    /// <summary>Bonus damage applied due to target being [Feared].</summary>
    public int FearBonusDamage { get; init; }

    /// <summary>Whether the target was [Feared] at time of cast.</summary>
    public bool TargetWasFeared { get; init; }

    /// <summary>Name/ID of the target.</summary>
    public string TargetName { get; init; } = string.Empty;

    /// <summary>Echo chain data for this ability (if applicable).</summary>
    public EchoChainResult? EchoChain { get; init; }

    /// <summary>Ability rank at time of cast.</summary>
    public int AbilityRank { get; init; }

    /// <summary>Gets a formatted description for combat log.</summary>
    public string GetDescription()
    {
        var fearTag = TargetWasFeared ? $" [+{FearBonusDamage} FEAR BONUS]" : "";
        var echoTag = EchoChain != null ? $" [ECHO: {EchoChain.ChainDamage} to {EchoChain.ChainTargets} targets]" : "";
        return $"Scream of Silence hits {TargetName} for {DamageDealt} damage{fearTag}{echoTag}";
    }
}

/// <summary>
/// Result of executing Phantom Menace (28012): applies [Feared] status and triggers echo chains.
/// Costs 2 AP. Duration: 3 turns (4 turns R2+). Triggers TerrorFeedback (+15 Aether if unlocked).
/// </summary>
public sealed record PhantomMenaceResult
{
    /// <summary>Whether the ability executed successfully.</summary>
    public bool IsSuccess { get; init; }

    /// <summary>Human-readable description of the ability outcome.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>AP spent to execute this ability.</summary>
    public int AetherSpent { get; init; }

    /// <summary>Duration of the [Feared] status applied.</summary>
    public int FearDuration { get; init; }

    /// <summary>Whether [Feared] was successfully applied to the target.</summary>
    public bool FearApplied { get; init; }

    /// <summary>Name/ID of the target.</summary>
    public string TargetName { get; init; } = string.Empty;

    /// <summary>Aether restored via TerrorFeedback passive (if applicable).</summary>
    public int AetherRestored { get; init; }

    /// <summary>Echo chain data for this ability (if applicable).</summary>
    public EchoChainResult? EchoChain { get; init; }

    /// <summary>Ability rank at time of cast.</summary>
    public int AbilityRank { get; init; }

    /// <summary>Gets a formatted description for combat log.</summary>
    public string GetDescription()
    {
        var terrorTag = AetherRestored > 0 ? $" [TERROR FEEDBACK +{AetherRestored} Aether]" : "";
        var echoTag = EchoChain != null ? $" [ECHO: {EchoChain.ChainDamage} to {EchoChain.ChainTargets} targets]" : "";
        return $"Phantom Menace applies [Feared] x{FearDuration} to {TargetName}{terrorTag}{echoTag}";
    }
}

/// <summary>
/// Result of executing Reality Fracture (28014): damage + crowd control + echo chains.
/// Damage: 3d6. Applies [Disoriented], pushes 1 tile. Costs 3 AP.
/// </summary>
public sealed record RealityFractureResult
{
    /// <summary>Whether the ability executed successfully.</summary>
    public bool IsSuccess { get; init; }

    /// <summary>Human-readable description of the ability outcome.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>AP spent to execute this ability.</summary>
    public int AetherSpent { get; init; }

    /// <summary>Damage dealt to the primary target.</summary>
    public int DamageDealt { get; init; }

    /// <summary>Whether [Disoriented] was applied to the target.</summary>
    public bool DisorientedApplied { get; init; }

    /// <summary>Distance the target was pushed (in tiles).</summary>
    public int PushDistance { get; init; }

    /// <summary>Name/ID of the target.</summary>
    public string TargetName { get; init; } = string.Empty;

    /// <summary>Echo chain data for this ability (if applicable).</summary>
    public EchoChainResult? EchoChain { get; init; }

    /// <summary>Ability rank at time of cast.</summary>
    public int AbilityRank { get; init; }

    /// <summary>Gets a formatted description for combat log.</summary>
    public string GetDescription()
    {
        var disorientTag = DisorientedApplied ? " [DISORIENTED]" : "";
        var pushTag = PushDistance > 0 ? $" [PUSHED {PushDistance} tiles]" : "";
        var echoTag = EchoChain != null ? $" [ECHO: {EchoChain.ChainDamage} to {EchoChain.ChainTargets} targets]" : "";
        return $"Reality Fracture hits {TargetName} for {DamageDealt} damage{disorientTag}{pushTag}{echoTag}";
    }
}

/// <summary>
/// Result of executing Fear Cascade (28016): AoE [Feared] application to multiple targets.
/// Duration: 3 turns (4 turns R2+). Triggers TerrorFeedback for each fear applied.
/// Costs 4 AP.
/// </summary>
public sealed record FearCascadeResult
{
    /// <summary>Whether the ability executed successfully.</summary>
    public bool IsSuccess { get; init; }

    /// <summary>Human-readable description of the ability outcome.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>AP spent to execute this ability.</summary>
    public int AetherSpent { get; init; }

    /// <summary>Total number of targets affected by the AoE.</summary>
    public int TargetsAffected { get; init; }

    /// <summary>Number of [Feared] statuses successfully applied.</summary>
    public int FearsApplied { get; init; }

    /// <summary>Duration of [Feared] applied to each target.</summary>
    public int FearDuration { get; init; }

    /// <summary>Names/IDs of affected targets.</summary>
    public string[] AffectedTargetNames { get; init; } = Array.Empty<string>();

    /// <summary>Total Aether restored via TerrorFeedback (per fear applied).</summary>
    public int TotalAetherRestored { get; init; }

    /// <summary>Ability rank at time of cast.</summary>
    public int AbilityRank { get; init; }

    /// <summary>Gets a formatted description for combat log.</summary>
    public string GetDescription()
    {
        var terrorTag = TotalAetherRestored > 0 ? $" [TERROR FEEDBACK +{TotalAetherRestored} Aether]" : "";
        return $"Fear Cascade applies [Feared] x{FearDuration} to {FearsApplied} of {TargetsAffected} enemies{terrorTag}";
    }
}

/// <summary>
/// Result of executing Echo Displacement (28017): forced teleportation with crowd control.
/// Displaces target 2 tiles away, applies [Disoriented], triggers echo chains.
/// Costs 4 AP.
/// </summary>
public sealed record EchoDisplacementResult
{
    /// <summary>Whether the ability executed successfully.</summary>
    public bool IsSuccess { get; init; }

    /// <summary>Human-readable description of the ability outcome.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>AP spent to execute this ability.</summary>
    public int AetherSpent { get; init; }

    /// <summary>Whether the target was successfully displaced.</summary>
    public bool TargetDisplaced { get; init; }

    /// <summary>Original position of the target (descriptive).</summary>
    public string OriginalPosition { get; init; } = string.Empty;

    /// <summary>New position of the target (descriptive).</summary>
    public string NewPosition { get; init; } = string.Empty;

    /// <summary>Whether [Disoriented] was applied on arrival.</summary>
    public bool DisorientedApplied { get; init; }

    /// <summary>Name/ID of the target.</summary>
    public string TargetName { get; init; } = string.Empty;

    /// <summary>Echo chain data triggered from original position (if applicable).</summary>
    public EchoChainResult? EchoChain { get; init; }

    /// <summary>Ability rank at time of cast.</summary>
    public int AbilityRank { get; init; }

    /// <summary>Gets a formatted description for combat log.</summary>
    public string GetDescription()
    {
        var disorientTag = DisorientedApplied ? " [DISORIENTED on arrival]" : "";
        var echoTag = EchoChain != null ? $" [ECHO: {EchoChain.ChainDamage} to {EchoChain.ChainTargets} targets]" : "";
        return $"Echo Displacement forces {TargetName} from {OriginalPosition} to {NewPosition}{disorientTag}{echoTag}";
    }
}

/// <summary>
/// Result of executing Silence Made Weapon (28018): capstone ultimate AoE damage scaling with [Feared] targets.
/// Base Damage: 4d10. Bonus: +2d10 per [Feared] target. Once per combat. Costs 5 AP.
/// </summary>
public sealed record SilenceMadeWeaponResult
{
    /// <summary>Whether the ability executed successfully.</summary>
    public bool IsSuccess { get; init; }

    /// <summary>Human-readable description of the ability outcome.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>AP spent to execute this ability.</summary>
    public int AetherSpent { get; init; }

    /// <summary>Total damage dealt across all targets.</summary>
    public int TotalDamage { get; init; }

    /// <summary>Number of targets hit by this ability.</summary>
    public int TargetsHit { get; init; }

    /// <summary>Number of [Feared] enemies that contributed damage scaling.</summary>
    public int FearedTargetCount { get; init; }

    /// <summary>Damage scaling bonus applied (2d10 per feared enemy).</summary>
    public int FearScalingBonus { get; init; }

    /// <summary>Whether [Feared] was applied to any targets hit (secondary effect if unlocked).</summary>
    public bool FearApplied { get; init; }

    /// <summary>Aether restored via TerrorFeedback (if fear was applied and passive unlocked).</summary>
    public int AetherRestored { get; init; }

    /// <summary>Echo chain data triggered to adjacent targets (if applicable).</summary>
    public EchoChainResult? EchoChain { get; init; }

    /// <summary>Ability rank at time of cast.</summary>
    public int AbilityRank { get; init; }

    /// <summary>Gets a formatted description for combat log.</summary>
    public string GetDescription()
    {
        var scalingTag = FearedTargetCount > 0 ? $" [+{FearScalingBonus} scaling from {FearedTargetCount} feared]" : "";
        var terrorTag = AetherRestored > 0 ? $" [+{AetherRestored} Aether]" : "";
        var echoTag = EchoChain != null ? $" [ECHO: {EchoChain.ChainDamage} to {EchoChain.ChainTargets} targets]" : "";
        return $"Silence Made Weapon deals {TotalDamage} damage to {TargetsHit} targets{scalingTag}{terrorTag}{echoTag}";
    }
}

/// <summary>
/// Result of processing an [Echo] chain for an Echo-Caller ability.
/// Represents damage propagation to adjacent targets without applying additional effects.
/// </summary>
public sealed record EchoChainResult
{
    /// <summary>Base damage for the echo (percentage of primary damage).</summary>
    public int ChainDamage { get; init; }

    /// <summary>Number of targets hit by the echo chain.</summary>
    public int ChainTargets { get; init; }

    /// <summary>Range of the echo chain in tiles.</summary>
    public int ChainRange { get; init; }

    /// <summary>Percentage of primary damage applied to echoed targets.</summary>
    public int ChainDamagePercent { get; init; }

    /// <summary>Names/IDs of targets hit by the echo chain.</summary>
    public string[] EchoTargetNames { get; init; } = Array.Empty<string>();

    /// <summary>Gets a formatted description for combat log.</summary>
    public string GetDescription()
    {
        return $"[Echo] hits {ChainTargets} targets within {ChainRange} tiles for {ChainDamage} damage " +
               $"({ChainDamagePercent}% of primary)";
    }
}
