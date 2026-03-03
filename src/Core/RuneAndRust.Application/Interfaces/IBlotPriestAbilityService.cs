using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service handling Blót-Priest specialization ability execution.
/// </summary>
/// <remarks>
/// <para>The Blót-Priest ("Sacrifice-Priest") is the Heretical Sacrificial Healer path
/// for the Mystic archetype. Core mechanics:</para>
/// <list type="bullet">
/// <item><b>Sacrificial Casting</b>: Spend HP instead of AP (via Sanguine Pact passive).</item>
/// <item><b>Life Siphon</b>: Offensive spells drain enemy HP and heal caster (+Corruption).</item>
/// <item><b>Blight Transference</b>: Healing allies transfers YOUR Corruption to THEM.</item>
/// <item><b>[Bloodied] Bonuses</b>: Multiple abilities gain power when below 50% HP.</item>
/// </list>
///
/// <para>This is the most Corruption-intensive specialization in the system. The question
/// is not "if" the Blót-Priest becomes corrupted, but "how fast" and "who else gets corrupted."</para>
///
/// <para>All methods return <c>null</c> on validation failure (wrong specialization, ability not
/// unlocked, insufficient AP, etc.). Non-null results always have <c>IsSuccess = true</c>.</para>
///
/// <para>Usage pattern follows the standard guard chain:</para>
/// <code>
/// // validate spec → validate unlock → validate tier PP → validate AP →
/// // evaluate corruption → deduct AP → apply effects → apply corruption → return result
/// </code>
/// </remarks>
public interface IBlotPriestAbilityService
{
    // ===== Tier 1: Foundation =====

    /// <summary>
    /// Evaluates a Sacrificial Cast (HP → AP conversion via Sanguine Pact).
    /// </summary>
    /// <param name="player">The Blót-Priest player.</param>
    /// <param name="hpToSpend">Amount of HP to convert.</param>
    /// <param name="rank">Sanguine Pact rank (1-3). Affects conversion ratio.</param>
    /// <returns>Conversion result, or null if validation fails.</returns>
    SacrificialCastResult? EvaluateSacrificialCast(Player player, int hpToSpend, int rank = 1);

    /// <summary>
    /// Executes Blood Siphon: damage + lifesteal + self-Corruption.
    /// </summary>
    /// <param name="player">The Blót-Priest player.</param>
    /// <param name="targetId">Target entity ID.</param>
    /// <param name="isBloodied">Whether the caster is currently [Bloodied] (below 50% HP).</param>
    /// <param name="hasCrimsonVigor">Whether the caster has Crimson Vigor unlocked.</param>
    /// <param name="rank">Blood Siphon rank (1-3).</param>
    /// <returns>Siphon result, or null if validation fails.</returns>
    BloodSiphonResult? ExecuteBloodSiphon(
        Player player,
        Guid targetId,
        bool isBloodied = false,
        bool hasCrimsonVigor = false,
        int rank = 1);

    /// <summary>
    /// Executes Gift of Vitae: heal ally + Corruption transfer.
    /// </summary>
    /// <param name="player">The Blót-Priest player.</param>
    /// <param name="allyId">Allied target to heal.</param>
    /// <param name="isBloodied">Whether the caster is currently [Bloodied].</param>
    /// <param name="hasCrimsonVigor">Whether the caster has Crimson Vigor unlocked.</param>
    /// <param name="rank">Gift of Vitae rank (1-3).</param>
    /// <returns>Healing result with Corruption transfer details, or null if validation fails.</returns>
    GiftOfVitaeResult? ExecuteGiftOfVitae(
        Player player,
        Guid allyId,
        bool isBloodied = false,
        bool hasCrimsonVigor = false,
        int rank = 1);

    // ===== Tier 2: Discipline =====

    /// <summary>
    /// Executes Blood Ward: sacrifice HP to create a temporary shield.
    /// </summary>
    /// <param name="player">The Blót-Priest player.</param>
    /// <param name="targetId">Target to shield (self or ally).</param>
    /// <param name="hpToSacrifice">HP to sacrifice for the shield.</param>
    /// <param name="rank">Blood Ward rank (1-3). Affects shield multiplier.</param>
    /// <returns>Shield result, or null if validation fails.</returns>
    BloodWardResult? ExecuteBloodWard(Player player, Guid targetId, int hpToSacrifice, int rank = 1);

    /// <summary>
    /// Executes Exsanguinate: applies a DoT curse with lifesteal.
    /// </summary>
    /// <param name="player">The Blót-Priest player.</param>
    /// <param name="targetId">Target entity ID to curse.</param>
    /// <param name="rank">Exsanguinate rank (1-3). Affects damage per tick.</param>
    /// <returns>DoT application result, or null if validation fails.</returns>
    ExsanguinateResult? ExecuteExsanguinate(Player player, Guid targetId, int rank = 1);

    // ===== Tier 3: Mastery =====

    /// <summary>
    /// Executes Hemorrhaging Curse: powerful DoT + [Bleeding] + anti-healing debuff.
    /// </summary>
    /// <param name="player">The Blót-Priest player.</param>
    /// <param name="targetId">Target entity ID to curse.</param>
    /// <param name="rank">Hemorrhaging Curse rank (always 1 — Tier 3 has no rank progression).</param>
    /// <returns>Curse result, or null if validation fails.</returns>
    HemorrhagingCurseResult? ExecuteHemorrhagingCurse(Player player, Guid targetId, int rank = 1);

    // ===== Capstone =====

    /// <summary>
    /// Executes Heartstopper in Crimson Deluge mode (AoE heal + Corruption spread).
    /// </summary>
    /// <param name="player">The Blót-Priest player.</param>
    /// <param name="allyIds">List of ally IDs to heal.</param>
    /// <param name="rank">Heartstopper rank (always 1 — Capstone has no rank progression).</param>
    /// <returns>AoE heal result, or null if validation fails or already used this combat.</returns>
    HeartstopperResult? ExecuteCrimsonDeluge(Player player, IReadOnlyList<Guid> allyIds, int rank = 1);

    /// <summary>
    /// Executes Heartstopper in Final Anathema mode (execute single target).
    /// </summary>
    /// <param name="player">The Blót-Priest player.</param>
    /// <param name="targetId">Target entity ID to execute.</param>
    /// <param name="targetCurrentHp">Target's current HP (for execute damage calc).</param>
    /// <param name="targetCorruption">Target's current Corruption (absorbed on kill).</param>
    /// <param name="rank">Heartstopper rank (always 1).</param>
    /// <returns>Execute result, or null if validation fails or already used this combat.</returns>
    HeartstopperResult? ExecuteFinalAnathema(
        Player player,
        Guid targetId,
        int targetCurrentHp,
        int targetCorruption,
        int rank = 1);

    // ===== Utility =====

    /// <summary>
    /// Processes an Exsanguinate DoT tick (per-turn damage + lifesteal).
    /// </summary>
    /// <param name="tickDamage">Pre-rolled tick damage.</param>
    /// <param name="targetId">Target entity ID.</param>
    /// <param name="targetName">Target display name.</param>
    /// <param name="lifestealPercent">Lifesteal percentage (25%).</param>
    /// <param name="remainingTicks">Remaining ticks after this one.</param>
    /// <returns>Tick result with damage, lifesteal, and Corruption details.</returns>
    ExsanguinateTickResult ProcessExsanguinateTick(
        int tickDamage,
        Guid targetId,
        string targetName,
        int lifestealPercent,
        int remainingTicks);

    /// <summary>
    /// Gets the readiness state of a specific ability for the given player.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="abilityId">The ability to check readiness for.</param>
    /// <returns>"Ready" if usable, otherwise a reason string.</returns>
    string GetAbilityReadiness(Player player, BlotPriestAbilityId abilityId);
}
