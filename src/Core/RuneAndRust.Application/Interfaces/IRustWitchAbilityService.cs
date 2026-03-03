using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for executing Rust-Witch specialization abilities.
/// </summary>
/// <remarks>
/// <para>The Rust-Witch is a Heretical Mystic specialization focused on the [Corroded] status
/// effect — a stacking DoT (max 5 stacks/target) that deals damage per turn and reduces Armor.
/// Every active ability inflicts deterministic self-Corruption on the caster.</para>
///
/// <para><b>Ability execution pattern:</b></para>
/// <code>
/// // Validate specialization → Validate unlock → Calculate AP cost
/// // → Validate AP → Capture pre-state → Evaluate corruption (deterministic)
/// // → Deduct AP → Apply effects → Apply self-corruption → Return result
/// </code>
///
/// <para><b>Active Abilities (5 of 9):</b></para>
/// <list type="table">
///   <listheader><term>Ability</term><description>Cost / Effect</description></listheader>
///   <item><term>Corrosive Curse (25002)</term><description>2 AP — Apply [Corroded] stacks</description></item>
///   <item><term>System Shock (25004)</term><description>3 AP — [Corroded] + [Stunned] on Mechanical</description></item>
///   <item><term>Flash Rust (25005)</term><description>4 AP — AoE [Corroded] to all enemies</description></item>
///   <item><term>Unmaking Word (25007)</term><description>4 AP — Double [Corroded] stacks on target</description></item>
///   <item><term>Entropic Cascade (25009)</term><description>5 AP — Execute or 6d6 Arcane (Capstone)</description></item>
/// </list>
///
/// <para><b>Passive Abilities (4 of 9):</b> Philosopher of Dust (25001), Entropic Field (25003),
/// Accelerated Entropy (25006), Cascade Reaction (25008). Passives modify constants/state and
/// have no execute methods.</para>
///
/// <para><b>Usage in combat flow:</b></para>
/// <code>
/// var result = _rustWitchAbilityService.ExecuteCorrosiveCurse(player, targetId, rank);
/// if (result != null)
/// {
///     session.AddEvent(GameEvent.Ability("CorrosiveCurse", result.GetDescription(), player.Id));
///     if (result.CorruptionGained > 0)
///         session.AddEvent(GameEvent.Corruption("SelfCorruption", result.CorruptionGained, player.Id));
/// }
/// </code>
/// </remarks>
public interface IRustWitchAbilityService
{
    // ===== Tier 1 Active Abilities =====

    /// <summary>
    /// Executes Corrosive Curse: applies [Corroded] stacks to a single target.
    /// </summary>
    /// <param name="player">The Rust-Witch player executing the ability.</param>
    /// <param name="targetId">Unique identifier of the target to corrode.</param>
    /// <param name="currentTargetStacks">Current [Corroded] stacks on the target (0-5).</param>
    /// <param name="rank">The ability rank (1, 2, or 3). Determines stacks applied: R1=1, R2=2, R3=3.</param>
    /// <returns>
    /// A <see cref="CorrosiveCurseResult"/> with stacks applied, capping info, and self-corruption.
    /// Returns null if the player lacks the specialization, hasn't unlocked the ability, or has insufficient AP.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="player"/> is null.</exception>
    CorrosiveCurseResult? ExecuteCorrosiveCurse(Player player, Guid targetId, int currentTargetStacks = 0, int rank = 1);

    // ===== Tier 2 Active Abilities =====

    /// <summary>
    /// Executes System Shock: applies [Corroded] and [Stunned] (Mechanical enemies only).
    /// </summary>
    /// <param name="player">The Rust-Witch player executing the ability.</param>
    /// <param name="targetId">Unique identifier of the target.</param>
    /// <param name="targetIsMechanical">Whether the target is a Mechanical creature ([Stunned] only applies to Mechanical).</param>
    /// <param name="currentTargetStacks">Current [Corroded] stacks on the target.</param>
    /// <param name="rank">The ability rank (1, 2, or 3).</param>
    /// <returns>
    /// A <see cref="SystemShockResult"/> with damage, stacks, stun status, and self-corruption.
    /// Returns null on validation failure.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="player"/> is null.</exception>
    SystemShockResult? ExecuteSystemShock(Player player, Guid targetId, bool targetIsMechanical, int currentTargetStacks = 0, int rank = 1);

    /// <summary>
    /// Executes Flash Rust: applies [Corroded] stacks to all enemies in range.
    /// </summary>
    /// <param name="player">The Rust-Witch player executing the ability.</param>
    /// <param name="targetIds">Unique identifiers of all enemies in the AoE range.</param>
    /// <param name="rank">The ability rank (1, 2, or 3).</param>
    /// <returns>
    /// A <see cref="FlashRustResult"/> with targets affected, stacks per target, and self-corruption.
    /// Returns null on validation failure.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="player"/> is null.</exception>
    FlashRustResult? ExecuteFlashRust(Player player, IReadOnlyList<Guid> targetIds, int rank = 1);

    // ===== Tier 3 Active Abilities =====

    /// <summary>
    /// Executes Unmaking Word: doubles [Corroded] stacks on a target (capped at 5).
    /// </summary>
    /// <param name="player">The Rust-Witch player executing the ability.</param>
    /// <param name="targetId">Unique identifier of the target whose stacks will be doubled.</param>
    /// <param name="currentTargetStacks">Current [Corroded] stacks on the target (will be doubled, capped at 5).</param>
    /// <param name="rank">The ability rank (1, 2, or 3).</param>
    /// <returns>
    /// An <see cref="UnmakingWordResult"/> with before/after stack counts and self-corruption.
    /// Returns null on validation failure.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="player"/> is null.</exception>
    UnmakingWordResult? ExecuteUnmakingWord(Player player, Guid targetId, int currentTargetStacks, int rank = 1);

    // ===== Capstone Active Ability =====

    /// <summary>
    /// Executes Entropic Cascade: execute target if threshold met, otherwise 6d6 Arcane damage.
    /// </summary>
    /// <param name="player">The Rust-Witch player executing the ability.</param>
    /// <param name="targetId">Unique identifier of the target.</param>
    /// <param name="targetCorrodedStacks">Current [Corroded] stacks on target (for execute check).</param>
    /// <param name="targetCorruption">Current Corruption on target (for execute check).</param>
    /// <param name="rank">The ability rank (1, 2, or 3).</param>
    /// <returns>
    /// An <see cref="EntropicCascadeResult"/> indicating either an execute or damage dealt.
    /// Execute triggers if target has &gt;50 Corruption OR 5 [Corroded] stacks.
    /// Returns null on validation failure.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="player"/> is null.</exception>
    EntropicCascadeResult? ExecuteEntropicCascade(
        Player player,
        Guid targetId,
        int targetCorrodedStacks,
        int targetCorruption,
        int rank = 1);

    // ===== Passive & Utility Methods =====

    /// <summary>
    /// Processes a [Corroded] DoT tick for a target during turn start.
    /// </summary>
    /// <param name="tracker">The target's current [Corroded] stack tracker.</param>
    /// <param name="hasAcceleratedEntropy">Whether the Rust-Witch has unlocked Accelerated Entropy (25006).</param>
    /// <returns>A <see cref="CorrodedDotTickResult"/> with per-stack damage breakdown.</returns>
    CorrodedDotTickResult ProcessCorrodedDotTick(CorrodedStackTracker tracker, bool hasAcceleratedEntropy);

    /// <summary>
    /// Processes Cascade Reaction (25008) when a [Corroded] enemy dies.
    /// Spreads stacks to adjacent enemies within 1 tile.
    /// </summary>
    /// <param name="deadTracker">The dead target's [Corroded] stack tracker.</param>
    /// <param name="adjacentTargetIds">Identifiers of characters adjacent to the dead target (within 1 tile).</param>
    /// <returns>A <see cref="CascadeReactionResult"/> with spread information.</returns>
    CascadeReactionResult ProcessCascadeReaction(
        CorrodedStackTracker deadTracker,
        IReadOnlyList<Guid> adjacentTargetIds);

    /// <summary>
    /// Gets the readiness state of an ability for UI display (unlocked, available, AP check, etc.).
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="abilityId">The ability to evaluate.</param>
    /// <returns>A human-readable status string for the ability.</returns>
    string GetAbilityReadiness(Player player, RustWitchAbilityId abilityId);
}
