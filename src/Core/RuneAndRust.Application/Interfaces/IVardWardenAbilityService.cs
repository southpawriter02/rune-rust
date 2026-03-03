using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for executing Varð-Warden specialization abilities.
/// </summary>
/// <remarks>
/// <para>The Varð-Warden is a Coherent Mystic specialization focused on Defensive Casting and Battlefield Control.
/// Unlike Heretical paths (Rust-Witch, Blót-Priest, Seiðkona), the Varð-Warden has no Corruption mechanics.
/// Instead, it leverages barriers, zones, ally buffs, and a once-per-expedition reaction capstone to protect allies and control the battlefield.</para>
///
/// <para><b>Ability execution pattern:</b></para>
/// <code>
/// // Validate specialization → Validate unlock → Validate tier (if Tier 2+) → Validate AP
/// // → Execute ability → Apply effects → Return result
/// </code>
///
/// <para><b>Active Abilities (6 of 9):</b></para>
/// <list type="table">
///   <listheader><term>Ability</term><description>Cost / Effect</description></listheader>
///   <item><term>RunicBarrier (29011)</term><description>3 AP — Creates barrier 30-50 HP, 2-4 turns</description></item>
///   <item><term>ConsecrateGround (29012)</term><description>3 AP — Creates zone, heal/damage 1d6-2d6, 3-4 turns</description></item>
///   <item><term>RuneOfShielding (29013)</term><description>3 AP — Buff ally +Soak +corruption resist, 4 turns</description></item>
///   <item><term>ReinforceWard (29014)</term><description>2 AP — Heal barrier or boost zone</description></item>
///   <item><term>GlyphOfSanctuary (29016)</term><description>4 AP — Party temp HP + Stress immunity</description></item>
///   <item><term>IndomitableBastion (29018)</term><description>Reaction, 0 AP — Negate fatal damage, once per expedition</description></item>
/// </list>
///
/// <para><b>Passive Abilities (3 of 9):</b> SanctifiedResolve (29010), WardensVigil (29015), AegisOfSanctity (29017).
/// Passives modify constants and have no execute methods.</para>
///
/// <para><b>Key mechanics:</b></para>
/// <list type="bullet">
/// <item>Barriers: Protective structures with HP scaling (30/40/50) and duration (2/3/4 turns).</item>
/// <item>Zones: Persistent area effects that heal/damage per turn, can be reinforced.</item>
/// <item>Buffs: RuneOfShielding grants Soak and Corruption resistance; GlyphOfSanctuary grants temp HP.</item>
/// <item>Once-per-expedition: IndomitableBastion (reaction capstone) can only be used once per expedition session.</item>
/// </list>
///
/// <para><b>Usage in combat flow:</b></para>
/// <code>
/// var result = _vardWardenAbilityService.ExecuteRunicBarrier(player, 5, 3, rank: 2);
/// if (result != null)
/// {
///     session.AddEvent(GameEvent.Ability("RunicBarrier", result.GetDescription(), player.Id));
/// }
/// </code>
/// </remarks>
public interface IVardWardenAbilityService
{
    // ===== Tier 1 Active Abilities =====

    /// <summary>
    /// Executes Runic Barrier: creates a protective barrier at a designated location.
    /// </summary>
    /// <param name="player">The Varð-Warden player executing the ability.</param>
    /// <param name="positionX">X coordinate where the barrier should be placed.</param>
    /// <param name="positionY">Y coordinate where the barrier should be placed.</param>
    /// <param name="rank">The ability rank (1, 2, or 3). Determines HP and duration.</param>
    /// <returns>
    /// A <see cref="RunicBarrierResult"/> with barrier HP, duration, and position.
    /// Returns null if the player lacks the specialization, hasn't unlocked the ability, or has insufficient AP.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="player"/> is null.</exception>
    RunicBarrierResult? ExecuteRunicBarrier(Player player, int positionX, int positionY, int rank = 1);

    /// <summary>
    /// Executes Consecrate Ground: creates a persistent zone that heals allies and damages Blighted/Undying enemies.
    /// </summary>
    /// <param name="player">The Varð-Warden player executing the ability.</param>
    /// <param name="positionX">X coordinate where the zone should be centered.</param>
    /// <param name="positionY">Y coordinate where the zone should be centered.</param>
    /// <param name="rank">The ability rank (1, 2, or 3). Determines heal/damage per turn.</param>
    /// <returns>
    /// A <see cref="ConsecrateGroundResult"/> with heal/damage per turn, duration, and radius.
    /// Returns null on validation failure.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="player"/> is null.</exception>
    ConsecrateGroundResult? ExecuteConsecrateGround(Player player, int positionX, int positionY, int rank = 1);

    // ===== Tier 2 Active Abilities =====

    /// <summary>
    /// Executes Rune of Shielding: buffs an ally with Soak and Corruption resistance.
    /// </summary>
    /// <param name="player">The Varð-Warden player executing the ability.</param>
    /// <param name="allyId">Unique identifier of the ally to buff.</param>
    /// <param name="rank">The ability rank (1, 2, or 3). Determines Soak and resistance bonuses.</param>
    /// <returns>
    /// A <see cref="RuneOfShieldingResult"/> with Soak/resistance bonuses and duration.
    /// Returns null on validation failure.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="player"/> is null.</exception>
    RuneOfShieldingResult? ExecuteRuneOfShielding(Player player, Guid allyId, int rank = 1);

    /// <summary>
    /// Executes Reinforce Ward: restores HP to an existing barrier or boosts a zone's effectiveness.
    /// </summary>
    /// <param name="player">The Varð-Warden player executing the ability.</param>
    /// <param name="barrierOrZoneId">Unique identifier of the barrier or zone to reinforce.</param>
    /// <param name="isBarrier">Whether the target is a barrier (true) or zone (false).</param>
    /// <param name="rank">The ability rank (1, 2, or 3). Determines amount restored or boost percentage.</param>
    /// <returns>
    /// A <see cref="ReinforceWardResult"/> with HP restored or zone boost amount.
    /// Returns null on validation failure.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="player"/> is null.</exception>
    ReinforceWardResult? ExecuteReinforceWard(Player player, Guid barrierOrZoneId, bool isBarrier, int rank = 1);

    // ===== Tier 3 Active Abilities =====

    /// <summary>
    /// Executes Glyph of Sanctuary: grants all allies in range temporary HP and Stress immunity.
    /// </summary>
    /// <param name="player">The Varð-Warden player executing the ability.</param>
    /// <param name="allyIds">Unique identifiers of all allies affected by the glyph.</param>
    /// <param name="rank">The ability rank (1, 2, or 3). Determines temp HP amount.</param>
    /// <returns>
    /// A <see cref="GlyphOfSanctuaryResult"/> with temp HP per ally, allies affected, and immunity duration.
    /// Returns null on validation failure.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="player"/> is null.</exception>
    GlyphOfSanctuaryResult? ExecuteGlyphOfSanctuary(Player player, IReadOnlyList<Guid> allyIds, int rank = 1);

    // ===== Capstone Reaction Ability =====

    /// <summary>
    /// Executes Indomitable Bastion: reaction capstone that negates fatal damage to an ally.
    /// </summary>
    /// <param name="player">The Varð-Warden player executing the ability (reaction).</param>
    /// <param name="allyId">Unique identifier of the ally being saved.</param>
    /// <param name="incomingDamage">The amount of damage that would have been fatal.</param>
    /// <param name="rank">The ability rank (1, 2, or 3). Determines barrier size on save.</param>
    /// <returns>
    /// An <see cref="IndomitableBastionResult"/> with damage negated and barrier creation confirmation.
    /// Returns null if the player hasn't unlocked the capstone or has already used it this expedition.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="player"/> is null.</exception>
    IndomitableBastionResult? ExecuteIndomitableBastion(Player player, Guid allyId, int incomingDamage, int rank = 1);

    // ===== Zone Processing =====

    /// <summary>
    /// Processes a consecrated zone tick, applying healing to allies and damage to Blighted/Undying enemies.
    /// </summary>
    /// <param name="alliesInZone">Unique identifiers of allies in the zone.</param>
    /// <param name="enemiesInZone">Unique identifiers of enemies in the zone.</param>
    /// <param name="isConsecratedGround">Whether this is a Consecrate Ground zone (vs other zone types).</param>
    /// <param name="rank">The rank of the zone ability (1, 2, or 3). Determines heal/damage amounts.</param>
    /// <returns>A <see cref="ConsecratedZoneTickResult"/> with healing/damage applied and targets affected.</returns>
    ConsecratedZoneTickResult ProcessZoneTick(
        IReadOnlyList<Guid> alliesInZone,
        IReadOnlyList<Guid> enemiesInZone,
        bool isConsecratedGround,
        int rank = 1);

    // ===== Utility Methods =====

    /// <summary>
    /// Determines the readiness state of an ability for a given player.
    /// </summary>
    /// <param name="player">The Varð-Warden player to check.</param>
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
    string GetAbilityReadiness(Player player, VardWardenAbilityId abilityId);
}
