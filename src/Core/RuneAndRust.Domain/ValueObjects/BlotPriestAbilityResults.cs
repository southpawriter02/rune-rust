using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Result of Blood Siphon (Tier 1 Active): damage + lifesteal + self-Corruption.
/// </summary>
/// <remarks>
/// Blood Siphon deals 3d6→5d6 damage and heals the caster for a percentage
/// of the damage dealt. Each cast adds +1 self-Corruption from consuming
/// Blighted life force.
/// </remarks>
public sealed record BloodSiphonResult
{
    /// <summary>Whether the ability executed successfully.</summary>
    public bool IsSuccess { get; init; }

    /// <summary>Human-readable description of the ability outcome.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>AP spent to execute this ability.</summary>
    public int AetherSpent { get; init; }

    /// <summary>Damage dealt to the target.</summary>
    public int DamageDealt { get; init; }

    /// <summary>HP healed via Life Siphon (percentage of damage dealt).</summary>
    public int HealAmount { get; init; }

    /// <summary>Siphon percentage applied (25%, 35%, or 50% by rank).</summary>
    public int SiphonPercent { get; init; }

    /// <summary>Name/ID of the target.</summary>
    public string TargetName { get; init; } = string.Empty;

    /// <summary>Self-Corruption gained from this cast.</summary>
    public int CorruptionGained { get; init; }

    /// <summary>The Corruption trigger for logging.</summary>
    public BlotPriestCorruptionTrigger? Trigger { get; init; }

    /// <summary>Ability rank at time of cast.</summary>
    public int AbilityRank { get; init; }

    /// <summary>Gets a formatted description for combat log.</summary>
    public string GetDescription() => $"Blood Siphon: {DamageDealt} damage, healed {HealAmount} HP ({SiphonPercent}% siphon), +{CorruptionGained} Corruption";
}

/// <summary>
/// Result of Gift of Vitae (Tier 1 Active): heal ally + Corruption transfer.
/// </summary>
/// <remarks>
/// The Blót-Priest's defining healing ability. Heals an ally for 4d10→8d10 HP
/// but transfers 1-2 Corruption to the target through Blight Transference.
/// Also adds +1 self-Corruption per cast.
/// </remarks>
public sealed record GiftOfVitaeResult
{
    /// <summary>Whether the ability executed successfully.</summary>
    public bool IsSuccess { get; init; }

    /// <summary>Human-readable description of the ability outcome.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>AP spent to execute this ability.</summary>
    public int AetherSpent { get; init; }

    /// <summary>HP healed on the ally.</summary>
    public int HealAmount { get; init; }

    /// <summary>Corruption transferred to the ally (Blight Transference).</summary>
    public int CorruptionTransferred { get; init; }

    /// <summary>Name/ID of the healed ally.</summary>
    public string AllyName { get; init; } = string.Empty;

    /// <summary>Self-Corruption gained from this cast.</summary>
    public int CorruptionGained { get; init; }

    /// <summary>The Corruption trigger for logging.</summary>
    public BlotPriestCorruptionTrigger? Trigger { get; init; }

    /// <summary>Ability rank at time of cast.</summary>
    public int AbilityRank { get; init; }

    /// <summary>Gets a formatted description for combat log.</summary>
    public string GetDescription() => $"Gift of Vitae: healed {AllyName} for {HealAmount} HP, transferred {CorruptionTransferred} Corruption";
}

/// <summary>
/// Result of Blood Ward (Tier 2 Active): HP sacrifice → temporary shield.
/// </summary>
/// <remarks>
/// Sacrifices HP to create a shield on self or ally. Shield value is 2.5–3.5×
/// the HP sacrificed (scaling by rank). Also applies a Stress effect to attackers
/// who strike the shielded target.
/// </remarks>
public sealed record BloodWardResult
{
    /// <summary>Whether the ability executed successfully.</summary>
    public bool IsSuccess { get; init; }

    /// <summary>Human-readable description of the ability outcome.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>AP spent to execute this ability.</summary>
    public int AetherSpent { get; init; }

    /// <summary>HP sacrificed to create the ward.</summary>
    public int HpSacrificed { get; init; }

    /// <summary>Shield value generated (multiplier × HP sacrificed).</summary>
    public int ShieldValue { get; init; }

    /// <summary>Multiplier applied to HP sacrifice (2.5, 3.0, or 3.5 by rank).</summary>
    public double Multiplier { get; init; }

    /// <summary>Name/ID of the protected target (self or ally).</summary>
    public string TargetName { get; init; } = string.Empty;

    /// <summary>Self-Corruption gained from this cast.</summary>
    public int CorruptionGained { get; init; }

    /// <summary>The Corruption trigger for logging.</summary>
    public BlotPriestCorruptionTrigger? Trigger { get; init; }

    /// <summary>Ability rank at time of cast.</summary>
    public int AbilityRank { get; init; }

    /// <summary>Gets a formatted description for combat log.</summary>
    public string GetDescription() => $"Blood Ward: sacrificed {HpSacrificed} HP → {ShieldValue} shield ({Multiplier:F1}×)";
}

/// <summary>
/// Result of Exsanguinate (Tier 2 Active): DoT curse with lifesteal.
/// </summary>
/// <remarks>
/// Applies a 3-turn DoT that deals 2d6→4d6 per tick with 25% lifesteal.
/// Each tick generates +1 self-Corruption (total +3 over full duration).
/// </remarks>
public sealed record ExsanguinateResult
{
    /// <summary>Whether the ability executed successfully.</summary>
    public bool IsSuccess { get; init; }

    /// <summary>Human-readable description of the ability outcome.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>AP spent to execute this ability.</summary>
    public int AetherSpent { get; init; }

    /// <summary>Damage per tick of the DoT.</summary>
    public int DamagePerTick { get; init; }

    /// <summary>Number of ticks remaining (starts at 3).</summary>
    public int Duration { get; init; }

    /// <summary>Lifesteal percentage (25%).</summary>
    public int LifestealPercent { get; init; }

    /// <summary>Name/ID of the cursed target.</summary>
    public string TargetName { get; init; } = string.Empty;

    /// <summary>Self-Corruption gained from initial cast (tick Corruption tracked separately).</summary>
    public int CorruptionGained { get; init; }

    /// <summary>Total Corruption that will be gained over full duration.</summary>
    public int TotalCorruptionOverDuration { get; init; }

    /// <summary>The Corruption trigger for logging.</summary>
    public BlotPriestCorruptionTrigger? Trigger { get; init; }

    /// <summary>Ability rank at time of cast.</summary>
    public int AbilityRank { get; init; }

    /// <summary>Gets a formatted description for combat log.</summary>
    public string GetDescription() => $"Exsanguinate: {DamagePerTick} damage/tick for {Duration} turns, {LifestealPercent}% lifesteal, +{TotalCorruptionOverDuration} total Corruption";
}

/// <summary>
/// Result of Hemorrhaging Curse (Tier 3 Active): DoT + anti-heal + lifesteal.
/// </summary>
/// <remarks>
/// Powerful DoT that applies [Bleeding] and reduces healing received by 50%.
/// Damage: 3d8/tick for 4 turns. Lifesteal: 30%. Self-Corruption: +2 (fixed).
/// </remarks>
public sealed record HemorrhagingCurseResult
{
    /// <summary>Whether the ability executed successfully.</summary>
    public bool IsSuccess { get; init; }

    /// <summary>Human-readable description of the ability outcome.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>AP spent to execute this ability.</summary>
    public int AetherSpent { get; init; }

    /// <summary>Damage per tick of the DoT.</summary>
    public int DamagePerTick { get; init; }

    /// <summary>Duration of the curse in turns.</summary>
    public int Duration { get; init; }

    /// <summary>Healing reduction percentage applied to target.</summary>
    public int HealingReductionPercent { get; init; }

    /// <summary>Lifesteal percentage.</summary>
    public int LifestealPercent { get; init; }

    /// <summary>Whether [Bleeding] was applied to the target.</summary>
    public bool BleedingApplied { get; init; }

    /// <summary>Name/ID of the cursed target.</summary>
    public string TargetName { get; init; } = string.Empty;

    /// <summary>Self-Corruption gained from this cast.</summary>
    public int CorruptionGained { get; init; }

    /// <summary>The Corruption trigger for logging.</summary>
    public BlotPriestCorruptionTrigger? Trigger { get; init; }

    /// <summary>Ability rank at time of cast.</summary>
    public int AbilityRank { get; init; }

    /// <summary>Gets a formatted description for combat log.</summary>
    public string GetDescription() => $"Hemorrhaging Curse: {DamagePerTick}/tick for {Duration} turns, -50% healing, +{CorruptionGained} Corruption";
}

/// <summary>
/// Result of Heartstopper Capstone (dual-mode: Crimson Deluge OR Final Anathema).
/// </summary>
/// <remarks>
/// <para><b>Crimson Deluge</b>: AoE heal (8d10 to all allies) + massive Corruption spread
/// (+10 self, +5 to each ally).</para>
/// <para><b>Final Anathema</b>: Execute single target. On kill, absorbs target's remaining
/// Corruption. Self-Corruption: +15.</para>
/// <para>Once per combat.</para>
/// </remarks>
public sealed record HeartstopperResult
{
    /// <summary>Whether the ability executed successfully.</summary>
    public bool IsSuccess { get; init; }

    /// <summary>Human-readable description of the ability outcome.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>AP spent to execute this ability.</summary>
    public int AetherSpent { get; init; }

    /// <summary>Which mode was used: "CrimsonDeluge" or "FinalAnathema".</summary>
    public string Mode { get; init; } = string.Empty;

    /// <summary>True if Crimson Deluge mode was used (AoE heal).</summary>
    public bool IsCrimsonDeluge { get; init; }

    /// <summary>True if Final Anathema mode was used (execute).</summary>
    public bool IsFinalAnathema { get; init; }

    // ===== Crimson Deluge properties =====

    /// <summary>HP healed per ally (Crimson Deluge only).</summary>
    public int HealPerAlly { get; init; }

    /// <summary>Number of allies healed (Crimson Deluge only).</summary>
    public int AlliesHealed { get; init; }

    /// <summary>Corruption transferred to each ally (Crimson Deluge only).</summary>
    public int CorruptionPerAlly { get; init; }

    // ===== Final Anathema properties =====

    /// <summary>Damage dealt to the target (Final Anathema only).</summary>
    public int DamageDealt { get; init; }

    /// <summary>Whether the target was killed (Final Anathema only).</summary>
    public bool TargetKilled { get; init; }

    /// <summary>Corruption absorbed from the killed target (Final Anathema only).</summary>
    public int CorruptionAbsorbed { get; init; }

    /// <summary>Name/ID of the target.</summary>
    public string TargetName { get; init; } = string.Empty;

    // ===== Common properties =====

    /// <summary>Self-Corruption gained from this cast.</summary>
    public int CorruptionGained { get; init; }

    /// <summary>The Corruption trigger for logging.</summary>
    public BlotPriestCorruptionTrigger? Trigger { get; init; }

    /// <summary>Ability rank at time of cast.</summary>
    public int AbilityRank { get; init; }

    /// <summary>Gets a formatted description for combat log.</summary>
    public string GetDescription()
    {
        if (IsCrimsonDeluge)
        {
            return $"Heartstopper: Crimson Deluge heals {AlliesHealed} allies for {HealPerAlly} HP each, " +
                   $"+{CorruptionPerAlly} Corruption each, +{CorruptionGained} self-Corruption";
        }

        return $"Heartstopper: Final Anathema deals {DamageDealt} damage" +
               (TargetKilled ? $", target killed! Absorbed {CorruptionAbsorbed} Corruption" : "") +
               $", +{CorruptionGained} self-Corruption";
    }
}

/// <summary>
/// Result of processing an Exsanguinate DoT tick (per-turn damage + lifesteal).
/// </summary>
public sealed record ExsanguinateTickResult
{
    /// <summary>Whether the tick processed successfully.</summary>
    public bool IsSuccess { get; init; }

    /// <summary>Human-readable description.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Target ID for this tick.</summary>
    public Guid TargetId { get; init; }

    /// <summary>Target name for display.</summary>
    public string TargetName { get; init; } = string.Empty;

    /// <summary>Damage dealt this tick.</summary>
    public int DamageDealt { get; init; }

    /// <summary>HP healed via lifesteal this tick.</summary>
    public int LifestealHeal { get; init; }

    /// <summary>Remaining ticks on the DoT.</summary>
    public int RemainingTicks { get; init; }

    /// <summary>Corruption from this tick.</summary>
    public int CorruptionGained { get; init; }

    /// <summary>Gets a formatted description for combat log.</summary>
    public string GetDescription() => $"Exsanguinate tick: {DamageDealt} damage to {TargetName}, healed {LifestealHeal} HP, +{CorruptionGained} Corruption, {RemainingTicks} ticks remaining";
}

/// <summary>
/// Result of evaluating Sacrificial Casting conversion (HP → AP via Sanguine Pact).
/// </summary>
/// <remarks>
/// Tracks the HP cost and equivalent AP gained when the Blót-Priest
/// uses Sacrificial Casting. Conversion rate improves with rank:
/// R1 = 2:1, R2 = 1.5:1, R3 = 1:1.
/// </remarks>
public sealed record SacrificialCastResult
{
    /// <summary>Whether the conversion was successful.</summary>
    public bool IsSuccess { get; init; }

    /// <summary>Human-readable description.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>HP spent for the conversion.</summary>
    public int HpSpent { get; init; }

    /// <summary>AP equivalent gained.</summary>
    public int ApGained { get; init; }

    /// <summary>The HP:AP conversion ratio used.</summary>
    public double ConversionRatio { get; init; }

    /// <summary>Corruption gained from the sacrificial cast.</summary>
    public int CorruptionGained { get; init; }

    /// <summary>Player's remaining HP after the sacrifice.</summary>
    public int RemainingHp { get; init; }

    /// <summary>The Corruption trigger for logging.</summary>
    public BlotPriestCorruptionTrigger? Trigger { get; init; }

    /// <summary>Ability rank at time of cast.</summary>
    public int AbilityRank { get; init; }

    /// <summary>Gets a formatted description for combat log.</summary>
    public string GetDescription() => $"Sacrificial Cast: spent {HpSpent} HP for {ApGained} AP ({ConversionRatio:F1}:1), +{CorruptionGained} Corruption";
}
