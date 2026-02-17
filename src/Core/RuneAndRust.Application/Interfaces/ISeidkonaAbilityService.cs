using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Defines the contract for Seiðkona specialization ability execution.
/// Handles Tier 1 (Foundation) ability logic including Seiðr Bolt (damage + Resonance build),
/// Wyrd Sight (detection), and Aether Attunement (passive AP regen).
/// </summary>
/// <remarks>
/// <para>Tier 1 abilities (0 PP required):</para>
/// <list type="bullet">
/// <item>Seiðr Bolt (Active): 2d6 Aetheric damage, +1 Resonance, +1 Accumulated Damage, costs 1 AP.
///   Subject to probability-based Corruption check at Resonance 5+ (Heretical path).</item>
/// <item>Wyrd Sight (Active): detect invisible/magic/Corruption within 10 spaces for 3 turns, costs 2 AP.
///   Does NOT build Resonance, does NOT trigger Corruption checks.</item>
/// <item>Aether Attunement (Passive): +10% AP regeneration rate. Always active, no Resonance or Corruption.</item>
/// </list>
/// <para>This interface will be extended in v0.20.8b with Tier 2 ability methods
/// following the same pattern as <see cref="IBerserkrAbilityService"/>.</para>
/// </remarks>
public interface ISeidkonaAbilityService
{
    /// <summary>
    /// Executes the Seiðr Bolt active ability against a target.
    /// </summary>
    /// <param name="player">The Seiðkona player casting the bolt.</param>
    /// <param name="targetId">Unique identifier of the target being attacked.</param>
    /// <returns>
    /// A <see cref="SeidrBoltResult"/> containing the full damage breakdown, Resonance changes,
    /// and Corruption status if the bolt was successfully cast; null if prerequisites were not met.
    /// </returns>
    /// <remarks>
    /// <para>Prerequisites: Seiðkona specialization, Seiðr Bolt unlocked, 1+ AP.</para>
    /// <para>Execution order:</para>
    /// <list type="number">
    /// <item>Validate prerequisites (spec, ability, AP)</item>
    /// <item>Evaluate Corruption risk BEFORE spending resources (uses current Resonance)</item>
    /// <item>Spend 1 AP</item>
    /// <item>Roll damage (2d6 Aetheric)</item>
    /// <item>Build +1 Resonance</item>
    /// <item>Add damage to Accumulated Aetheric Damage tracker</item>
    /// <item>Apply Corruption if triggered</item>
    /// <item>Return complete result</item>
    /// </list>
    /// </remarks>
    SeidrBoltResult? ExecuteSeidrBolt(Player player, Guid targetId);

    /// <summary>
    /// Executes the Wyrd Sight active ability to gain supernatural perception.
    /// </summary>
    /// <param name="player">The Seiðkona player activating Wyrd Sight.</param>
    /// <returns>
    /// A <see cref="WyrdSightResult"/> representing the active detection effect if successful;
    /// null if prerequisites were not met.
    /// </returns>
    /// <remarks>
    /// <para>Prerequisites: Seiðkona specialization, Wyrd Sight unlocked, 2+ AP.</para>
    /// <para>Execution order:</para>
    /// <list type="number">
    /// <item>Validate prerequisites (spec, ability, AP)</item>
    /// <item>Spend 2 AP</item>
    /// <item>Create WyrdSight effect (3 turns, 10 spaces, detects invisible/magic/Corruption)</item>
    /// <item>Set on player</item>
    /// </list>
    /// <para>Important: Wyrd Sight does NOT build Resonance and does NOT trigger
    /// Corruption checks. Pure detection has no Aetheric cost.</para>
    /// </remarks>
    WyrdSightResult? ExecuteWyrdSight(Player player);

    /// <summary>
    /// Gets the passive Aether Attunement AP regeneration bonus.
    /// Returns the bonus value if the ability is unlocked, 0 otherwise.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>
    /// 10 (representing +10% AP regen) if the player is a Seiðkona with Aether Attunement unlocked;
    /// 0 otherwise.
    /// </returns>
    int GetAetherAttunementBonus(Player player);

    /// <summary>
    /// Gets a readiness summary for all Seiðkona abilities for the specified player.
    /// Used for UI display to show which abilities are available.
    /// </summary>
    /// <param name="player">The Seiðkona player to check.</param>
    /// <returns>
    /// A dictionary mapping each unlocked <see cref="SeidkonaAbilityId"/> to a boolean
    /// indicating whether the ability can currently be used (sufficient AP, etc.).
    /// </returns>
    Dictionary<SeidkonaAbilityId, bool> GetAbilityReadiness(Player player);

    /// <summary>
    /// Checks if the player meets Tier 2 unlock requirements (8+ PP invested).
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player has 8 or more PP invested in the Seiðkona tree.</returns>
    bool CanUnlockTier2(Player player);

    /// <summary>
    /// Checks if the player meets Tier 3 unlock requirements (16+ PP invested).
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player has 16 or more PP invested in the Seiðkona tree.</returns>
    bool CanUnlockTier3(Player player);

    /// <summary>
    /// Checks if the player meets Capstone unlock requirements (24+ PP invested).
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player has 24 or more PP invested in the Seiðkona tree.</returns>
    bool CanUnlockCapstone(Player player);

    /// <summary>
    /// Gets total Progression Points invested in the Seiðkona ability tree.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>Total PP invested.</returns>
    int GetPPInvested(Player player);
}
