using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Defines the contract for Skjaldmær specialization ability operations.
/// Handles Tier 3 (Mastery) and Capstone ability logic including damage reduction,
/// ally protection, and invulnerability.
/// </summary>
/// <remarks>
/// <para>Tier 3 abilities (16+ PP required):</para>
/// <list type="bullet">
/// <item>Unbreakable (Passive) — flat damage reduction</item>
/// <item>Guardian's Sacrifice (Reaction) — absorb ally damage</item>
/// </list>
/// <para>Capstone ability (24+ PP required):</para>
/// <list type="bullet">
/// <item>The Wall Lives (Ultimate) — invulnerability for 3 turns, once per combat</item>
/// </list>
/// </remarks>
public interface ISkjaldmaerAbilityService
{
    /// <summary>
    /// Gets the flat damage reduction from the Unbreakable passive ability.
    /// </summary>
    /// <param name="player">The Skjaldmær player.</param>
    /// <returns>3 if Unbreakable is unlocked; otherwise 0.</returns>
    int GetDamageReduction(Player player);

    /// <summary>
    /// Attempts to execute Guardian's Sacrifice: absorb all damage for an ally.
    /// </summary>
    /// <param name="skjaldmaer">The Skjaldmær activating the reaction.</param>
    /// <param name="defendedAlly">The ally being protected.</param>
    /// <param name="incomingDamage">Damage that would hit the ally.</param>
    /// <returns>True if the sacrifice was successfully executed.</returns>
    /// <remarks>
    /// Prerequisites: Guardian's Sacrifice unlocked, 2+ Block Charges available.
    /// On success: spends 2 Block Charges, ally takes 0 damage, Skjaldmær takes all damage
    /// (subject to Unbreakable reduction if also unlocked).
    /// </remarks>
    bool TryGuardiansSacrifice(Player skjaldmaer, Player defendedAlly, int incomingDamage);

    /// <summary>
    /// Activates The Wall Lives capstone ability.
    /// </summary>
    /// <param name="player">The Skjaldmær activating the capstone.</param>
    /// <returns>True if activation was successful.</returns>
    /// <remarks>
    /// Requirements: The Wall Lives unlocked (24+ PP), 4+ AP available,
    /// not yet used this combat. On success: deducts 4 AP, creates active
    /// TheWallLivesState with 3-turn duration, sets HasUsedCapstoneThisCombat flag.
    /// </remarks>
    bool ActivateTheWallLives(Player player);

    /// <summary>
    /// Checks if the capstone can be used this combat.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player is a Skjaldmær who hasn't used capstone this combat.</returns>
    bool CanUseCapstone(Player player);

    /// <summary>
    /// Checks if the player meets Tier 3 unlock requirements (16+ PP invested).
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player has 16 or more PP invested in the Skjaldmær tree.</returns>
    bool CanUnlockTier3(Player player);

    /// <summary>
    /// Checks if the player meets Capstone unlock requirements (24+ PP invested).
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player has 24 or more PP invested in the Skjaldmær tree.</returns>
    bool CanUnlockCapstone(Player player);

    /// <summary>
    /// Gets total Progression Points invested in the Skjaldmær ability tree.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>Total PP invested.</returns>
    int GetPPInvested(Player player);
}
