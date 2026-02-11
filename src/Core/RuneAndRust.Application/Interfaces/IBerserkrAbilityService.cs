// ═══════════════════════════════════════════════════════════════════════════════
// IBerserkrAbilityService.cs
// Interface for executing Berserkr Tier 1 abilities: Fury Strike (active),
// Blood Scent (passive trigger), and Pain is Fuel (passive trigger).
// Version: 0.20.5a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service interface for executing Berserkr specialization abilities.
/// </summary>
/// <remarks>
/// <para>
/// Handles the three Tier 1 abilities:
/// </para>
/// <list type="bullet">
///   <item><description><b>Fury Strike (Active):</b> 2 AP, 20 Rage — weapon damage + 3d6 fury</description></item>
///   <item><description><b>Blood Scent (Passive):</b> +10 Rage when enemy bloodied, +1 Attack vs bloodied</description></item>
///   <item><description><b>Pain is Fuel (Passive):</b> +5 Rage when taking damage</description></item>
/// </list>
/// <para>
/// Active abilities check Corruption risk via <see cref="IBerserkrCorruptionService"/>
/// when used at 80+ Rage. Passive abilities do not trigger Corruption.
/// </para>
/// </remarks>
/// <seealso cref="BerserkrAbilityId"/>
/// <seealso cref="IBerserkrRageService"/>
/// <seealso cref="IBerserkrCorruptionService"/>
public interface IBerserkrAbilityService
{
    /// <summary>
    /// Executes Fury Strike against a target.
    /// Costs 2 AP and 20 Rage. Deals weapon damage + 3d6 fury damage.
    /// On critical hit (natural 20): +1d6 bonus fury damage.
    /// Triggers +1 Corruption if used at 80+ Rage.
    /// </summary>
    /// <param name="characterId">Attacking character's unique identifier.</param>
    /// <param name="targetId">Target creature's unique identifier.</param>
    /// <returns>
    /// A <see cref="FuryStrikeResult"/> containing the full damage breakdown,
    /// Rage spent, and Corruption status.
    /// </returns>
    FuryStrikeResult UseFuryStrike(Guid characterId, Guid targetId);

    /// <summary>
    /// Checks and applies the Blood Scent passive trigger when an enemy
    /// crosses the bloodied threshold (≤50% HP).
    /// </summary>
    /// <param name="characterId">Berserkr character's unique identifier.</param>
    /// <param name="targetId">Target creature's unique identifier.</param>
    /// <param name="previousHp">Target's HP before the current damage.</param>
    /// <param name="currentHp">Target's HP after the current damage.</param>
    /// <param name="maxHp">Target's maximum HP.</param>
    /// <param name="targetName">Target creature's display name.</param>
    /// <returns><c>true</c> if the Blood Scent trigger fired (target was just bloodied).</returns>
    bool CheckBloodScent(
        Guid characterId,
        Guid targetId,
        int previousHp,
        int currentHp,
        int maxHp,
        string targetName);

    /// <summary>
    /// Checks and applies the Pain is Fuel passive trigger when the
    /// Berserkr takes damage.
    /// </summary>
    /// <param name="characterId">Berserkr character's unique identifier.</param>
    /// <param name="damageReceived">Amount of damage received. Must be positive.</param>
    /// <returns>The amount of Rage gained (always <see cref="RageResource.PainIsFuelGain"/> if triggered).</returns>
    int CheckPainIsFuel(Guid characterId, int damageReceived);

    /// <summary>
    /// Checks whether a specific ability is ready to use.
    /// </summary>
    /// <param name="characterId">Character's unique identifier.</param>
    /// <param name="abilityId">The ability to check.</param>
    /// <returns><c>true</c> if the ability can be used (sufficient resources, no cooldown).</returns>
    bool GetAbilityReadiness(Guid characterId, BerserkrAbilityId abilityId);
}
