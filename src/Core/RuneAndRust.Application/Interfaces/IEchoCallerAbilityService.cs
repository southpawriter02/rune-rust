using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for executing Echo-Caller specialization abilities.
/// </summary>
/// <remarks>
/// <para>The Echo-Caller is a Coherent Mystic specialization focused on Psychic Artillery and Crowd Control.
/// Unlike Heretical paths (Rust-Witch, Blót-Priest, Seiðkona), the Echo-Caller has no Corruption mechanics.
/// Instead, it leverages [Echo] chain damage propagation and [Feared] status manipulation for control.</para>
///
/// <para><b>Ability execution pattern:</b></para>
/// <code>
/// // Validate specialization → Validate unlock → Validate tier (if Tier 2+) → Validate AP
/// // → Capture pre-state → Execute ability → Apply effects → Trigger TerrorFeedback (if applicable)
/// // → Process echo chains → Return result
/// </code>
///
/// <para><b>Active Abilities (6 of 9):</b></para>
/// <list type="table">
///   <listheader><term>Ability</term><description>Cost / Effect</description></listheader>
///   <item><term>ScreamOfSilence (28011)</term><description>2 AP — Psychic damage, bonus vs Feared, [Echo]</description></item>
///   <item><term>PhantomMenace (28012)</term><description>2 AP — Apply [Feared], [Echo], triggers TerrorFeedback</description></item>
///   <item><term>RealityFracture (28014)</term><description>3 AP — Damage + [Disoriented] + Push, [Echo]</description></item>
///   <item><term>FearCascade (28016)</term><description>4 AP — AoE [Feared], triggers TerrorFeedback per fear</description></item>
///   <item><term>EchoDisplacement (28017)</term><description>4 AP — Force teleport + [Disoriented], [Echo]</description></item>
///   <item><term>SilenceMadeWeapon (28018)</term><description>5 AP — Capstone, scaling damage per [Feared] enemy</description></item>
/// </list>
///
/// <para><b>Passive Abilities (3 of 9):</b> EchoAttunement (28010), EchoCascade (28013), TerrorFeedback (28015).
/// Passives modify constants and have no execute methods.</para>
///
/// <para><b>Key mechanics:</b></para>
/// <list type="bullet">
/// <item>[Echo] chains: Damage bounces to adjacent targets (1-3 tiles, 50-80% damage, 1-2 targets depending on EchoCascade).</item>
/// <item>[Feared] status: Increases damage taken (+1d8 or +2d8 bonus from Psychic attacks).</item>
/// <item>TerrorFeedback: Restores +15 Aether when [Feared] is applied (PhantomMenace or FearCascade).</item>
/// <item>Once-per-combat: SilenceMadeWeapon capstone (track HasUsedSilenceMadeWeaponThisCombat).</item>
/// </list>
///
/// <para><b>Usage in combat flow:</b></para>
/// <code>
/// var result = _echoCallerAbilityService.ExecuteScreamOfSilence(player, targetId, targetIsFeared: true, rank: 1);
/// if (result != null)
/// {
///     session.AddEvent(GameEvent.Ability("ScreamOfSilence", result.GetDescription(), player.Id));
///     if (result.EchoChain != null)
///         session.AddEvent(GameEvent.Ability("EchoChain", result.EchoChain.GetDescription(), player.Id));
/// }
/// </code>
/// </remarks>
public interface IEchoCallerAbilityService
{
    // ===== Tier 1 Active Abilities =====

    /// <summary>
    /// Executes Scream of Silence: psychic damage with fear bonus and echo chains.
    /// </summary>
    /// <param name="player">The Echo-Caller player executing the ability.</param>
    /// <param name="targetId">Unique identifier of the target to strike.</param>
    /// <param name="targetIsFeared">Whether the target currently has [Feared] status.</param>
    /// <param name="rank">The ability rank (1, 2, or 3). Determines damage scaling and fear bonus.</param>
    /// <returns>
    /// A <see cref="ScreamOfSilenceResult"/> with damage, fear bonus, echo chain data.
    /// Returns null if the player lacks the specialization, hasn't unlocked the ability, or has insufficient AP.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="player"/> is null.</exception>
    ScreamOfSilenceResult? ExecuteScreamOfSilence(Player player, Guid targetId, bool targetIsFeared, int rank = 1);

    /// <summary>
    /// Executes Phantom Menace: applies [Feared] status and triggers echo chains.
    /// </summary>
    /// <param name="player">The Echo-Caller player executing the ability.</param>
    /// <param name="targetId">Unique identifier of the target to frighten.</param>
    /// <param name="rank">The ability rank (1, 2, or 3). Determines fear duration (3 or 4 turns).</param>
    /// <returns>
    /// A <see cref="PhantomMenaceResult"/> with fear status, duration, aether restored via TerrorFeedback.
    /// Returns null on validation failure.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="player"/> is null.</exception>
    PhantomMenaceResult? ExecutePhantomMenace(Player player, Guid targetId, int rank = 1);

    /// <summary>
    /// Executes Reality Fracture: damage, crowd control, and echo chains.
    /// </summary>
    /// <param name="player">The Echo-Caller player executing the ability.</param>
    /// <param name="targetId">Unique identifier of the target to fracture.</param>
    /// <param name="targetIsFeared">Whether the target currently has [Feared] status (for potential bonus calculation).</param>
    /// <param name="rank">The ability rank (1, 2, or 3).</param>
    /// <returns>
    /// A <see cref="RealityFractureResult"/> with damage, disorientation, push distance, echo chains.
    /// Returns null on validation failure.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="player"/> is null.</exception>
    RealityFractureResult? ExecuteRealityFracture(Player player, Guid targetId, bool targetIsFeared, int rank = 1);

    /// <summary>
    /// Executes Fear Cascade: AoE fear application to all targets in range.
    /// </summary>
    /// <param name="player">The Echo-Caller player executing the ability.</param>
    /// <param name="targetIds">Unique identifiers of all enemies in the AoE range.</param>
    /// <param name="rank">The ability rank (1, 2, or 3). Determines fear duration.</param>
    /// <returns>
    /// A <see cref="FearCascadeResult"/> with targets affected, fears applied count, aether restored via TerrorFeedback per fear.
    /// Returns null on validation failure.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="player"/> is null.</exception>
    FearCascadeResult? ExecuteFearCascade(Player player, IReadOnlyList<Guid> targetIds, int rank = 1);

    /// <summary>
    /// Executes Echo Displacement: forces a target to be teleported with crowd control.
    /// </summary>
    /// <param name="player">The Echo-Caller player executing the ability.</param>
    /// <param name="targetId">Unique identifier of the target to displace.</param>
    /// <param name="rank">The ability rank (1, 2, or 3).</param>
    /// <returns>
    /// An <see cref="EchoDisplacementResult"/> with displacement info, disorientation status, and echo chains.
    /// Returns null on validation failure.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="player"/> is null.</exception>
    EchoDisplacementResult? ExecuteEchoDisplacement(Player player, Guid targetId, int rank = 1);

    /// <summary>
    /// Executes Silence Made Weapon: capstone ultimate with fear-based damage scaling.
    /// </summary>
    /// <param name="player">The Echo-Caller player executing the ability.</param>
    /// <param name="targetIds">Unique identifiers of all targets in the AoE range.</param>
    /// <param name="fearedTargetCount">Number of [Feared] enemies contributing to damage scaling.</param>
    /// <param name="rank">The ability rank (1, 2, or 3).</param>
    /// <returns>
    /// A <see cref="SilenceMadeWeaponResult"/> with total damage, targets hit, fear scaling bonus, echo chains.
    /// Returns null if the player hasn't unlocked the capstone, has insufficient AP, or has already used it this combat.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="player"/> is null.</exception>
    SilenceMadeWeaponResult? ExecuteSilenceMadeWeapon(
        Player player,
        IReadOnlyList<Guid> targetIds,
        int fearedTargetCount,
        int rank = 1);

    // ===== Echo Chain Processing =====

    /// <summary>
    /// Processes an [Echo] chain for an ability, propagating damage to adjacent targets.
    /// </summary>
    /// <param name="baseDamage">The primary ability's damage to the main target.</param>
    /// <param name="adjacentTargetIds">Unique identifiers of targets adjacent to the primary target.</param>
    /// <param name="hasEchoCascade">Whether the Echo-Caller has unlocked the EchoCascade passive (28013).</param>
    /// <param name="echoCascadeRank">The rank of the EchoCascade passive (1, 2, or 3).</param>
    /// <returns>An <see cref="EchoChainResult"/> with propagated damage and affected targets.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="adjacentTargetIds"/> is null.</exception>
    EchoChainResult ProcessEchoChain(
        int baseDamage,
        IReadOnlyList<Guid> adjacentTargetIds,
        bool hasEchoCascade,
        int echoCascadeRank = 1);

    // ===== Utility Methods =====

    /// <summary>
    /// Determines the readiness state of an ability for a given player.
    /// </summary>
    /// <param name="player">The Echo-Caller player to check.</param>
    /// <param name="abilityId">The ability ID to check.</param>
    /// <returns>
    /// A human-readable string describing why the ability is or isn't ready:
    /// "Ready" if all conditions are met,
    /// "Not unlocked" if the ability hasn't been learned,
    /// "Insufficient AP" if the player lacks the required AP,
    /// "Tier requirement not met" if the player doesn't have enough PP,
    /// or similar messages for other validation failures.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="player"/> is null.</exception>
    string GetAbilityReadiness(Player player, EchoCallerAbilityId abilityId);
}
