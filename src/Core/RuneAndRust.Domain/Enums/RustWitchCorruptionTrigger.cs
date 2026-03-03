namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Enumeration of Rust-Witch-specific Corruption triggers.
/// Each trigger represents an active ability cast that inflicts deterministic self-Corruption.
/// </summary>
/// <remarks>
/// <para>Unlike the Seiðkona's probability-based Corruption system (d100 vs percentage threshold),
/// the Rust-Witch uses <strong>deterministic self-Corruption</strong>: every active ability always
/// inflicts a fixed amount of Corruption on the caster. There is no dice roll — the cost is
/// guaranteed and represents the entropic price of weaponizing decay.</para>
///
/// <para>Self-Corruption amounts by ability and rank:</para>
/// <list type="table">
///   <listheader><term>Ability</term><description>R1/R2 → R3</description></listheader>
///   <item><term>Corrosive Curse</term><description>+2 → +1</description></item>
///   <item><term>System Shock</term><description>+3 → +2</description></item>
///   <item><term>Flash Rust</term><description>+4 → +3</description></item>
///   <item><term>Unmaking Word</term><description>+4 (all ranks)</description></item>
///   <item><term>Entropic Cascade</term><description>+6 (all ranks)</description></item>
/// </list>
///
/// <para>Passive abilities (Philosopher of Dust, Entropic Field, Accelerated Entropy,
/// Cascade Reaction) do NOT trigger self-Corruption.</para>
/// </remarks>
public enum RustWitchCorruptionTrigger
{
    /// <summary>
    /// Corrosive Curse cast — applies [Corroded] stacks to a single target.
    /// Self-Corruption: +2 at Rank 1-2, +1 at Rank 3.
    /// </summary>
    CorrosiveCurseCast = 1,

    /// <summary>
    /// System Shock cast — applies [Corroded] and [Stunned] (Mechanical only).
    /// Self-Corruption: +3 at Rank 1-2, +2 at Rank 3.
    /// </summary>
    SystemShockCast = 2,

    /// <summary>
    /// Flash Rust cast — AoE [Corroded] application to all enemies.
    /// Self-Corruption: +4 at Rank 1-2, +3 at Rank 3.
    /// </summary>
    FlashRustCast = 3,

    /// <summary>
    /// Unmaking Word cast — doubles [Corroded] stacks on target (capped at 5).
    /// Self-Corruption: +4 at all ranks (no Rank 3 reduction).
    /// </summary>
    UnmakingWordCast = 4,

    /// <summary>
    /// Entropic Cascade capstone cast — execute threshold or 6d6 Arcane damage.
    /// Self-Corruption: +6 at all ranks (highest single-ability Corruption in this spec).
    /// </summary>
    EntropicCascadeCast = 5
}
