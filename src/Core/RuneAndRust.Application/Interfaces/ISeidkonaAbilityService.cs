using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Defines the contract for Seiðkona specialization ability execution.
/// Handles Tier 1 (Foundation), Tier 2 (Discipline), Tier 3 (Mastery),
/// and Capstone (Ultimate) ability logic.
/// </summary>
/// <remarks>
/// <para>Tier 1 abilities (0 PP required, v0.20.8a):</para>
/// <list type="bullet">
/// <item>Seiðr Bolt (Active): 2d6 Aetheric damage, +1 Resonance, +1 Accumulated Damage, costs 1 AP.
///   Subject to probability-based Corruption check at Resonance 5+ (Heretical path).</item>
/// <item>Wyrd Sight (Active): detect invisible/magic/Corruption within 10 spaces for 3 turns, costs 2 AP.
///   Does NOT build Resonance, does NOT trigger Corruption checks.</item>
/// <item>Aether Attunement (Passive): +10% AP regeneration rate. Always active, no Resonance or Corruption.</item>
/// </list>
/// <para>Tier 2 abilities (4 PP each, v0.20.8b):</para>
/// <list type="bullet">
/// <item>Fate's Thread (Active): Divination/precognition, +2 Resonance, costs 2 AP (1 with Cascade).
///   Subject to probability-based Corruption check at Resonance 5+. No damage, no Accumulated Damage.</item>
/// <item>Weave Disruption (Active): Dispel/counterspell, d20 + Resonance vs effect DC, +1 Resonance,
///   costs 3 AP (2 with Cascade). Subject to Corruption check. No direct damage.</item>
/// <item>Resonance Cascade (Passive): Reduces all Seiðkona ability AP costs by 1 (min 1) when
///   Resonance is 5+. No Resonance gain, no Corruption risk. Does NOT affect Unraveling capstone.</item>
/// </list>
/// <para>Tier 3 abilities (5 PP each, v0.20.8c):</para>
/// <list type="bullet">
/// <item>Völva's Vision (Active): Mass detection revealing all hidden/invisible/concealed enemies
///   within 15 spaces, +2 Resonance, costs 3 AP (2 with Cascade). Subject to Corruption check.
///   No damage, no Accumulated Damage.</item>
/// <item>Aether Storm (Active): 4d6 Aetheric damage (cone AoE), +2 Resonance, costs 5 AP
///   (4 with Cascade). Subject to Corruption check. Adds damage to Accumulated Aetheric Damage.</item>
/// </list>
/// <para>Capstone ability (6 PP, v0.20.8c):</para>
/// <list type="bullet">
/// <item>The Unraveling (Active): Releases 100% of Accumulated Aetheric Damage as a single burst,
///   costs 5 AP (immune to Cascade reduction), requires Resonance exactly 10 and Accumulated
///   Damage &gt; 0. Resets Resonance to 0 and Accumulated Damage to 0. Guaranteed 20% Corruption
///   check (+2 Corruption if triggered). Once per combat.</item>
/// </list>
/// </remarks>
public interface ISeidkonaAbilityService
{
    // ===== Tier 1 Abilities (v0.20.8a) =====

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

    // ===== Tier 2 Abilities (v0.20.8b) =====

    /// <summary>
    /// Executes the Fate's Thread active divination ability against a target.
    /// Glimpses the target's next action through the Wyrd.
    /// </summary>
    /// <param name="player">The Seiðkona player casting the divination.</param>
    /// <param name="targetId">Unique identifier of the target being observed.</param>
    /// <returns>
    /// A <see cref="FatesThreadResult"/> containing Resonance changes, Corruption status,
    /// and AP cost details if the ability was successfully cast; null if prerequisites were not met.
    /// </returns>
    /// <remarks>
    /// <para>Prerequisites: Seiðkona specialization, Fate's Thread unlocked, sufficient AP
    /// (2 AP base, 1 AP with Resonance Cascade active at Resonance 5+).</para>
    /// <para>Execution order:</para>
    /// <list type="number">
    /// <item>Validate prerequisites (spec, ability, AP with Cascade consideration)</item>
    /// <item>Evaluate Corruption risk BEFORE spending resources (uses current Resonance)</item>
    /// <item>Spend AP (2 base or 1 with Cascade)</item>
    /// <item>Build +2 Resonance (higher than Tier 1's +1)</item>
    /// <item>Apply Corruption if triggered</item>
    /// <item>Return complete result</item>
    /// </list>
    /// <para>Important: Fate's Thread does NOT deal damage and does NOT add to the
    /// Accumulated Aetheric Damage tracker. The +2 Resonance gain represents significant
    /// escalation risk.</para>
    /// </remarks>
    FatesThreadResult? ExecuteFatesThread(Player player, Guid targetId);

    /// <summary>
    /// Executes the Weave Disruption active dispel ability against a target.
    /// Attempts to unravel magical effects using d20 + Resonance vs effect DC.
    /// </summary>
    /// <param name="player">The Seiðkona player casting the dispel.</param>
    /// <param name="targetId">Unique identifier of the target whose effects are being disrupted.</param>
    /// <returns>
    /// A <see cref="WeaveDisruptionResult"/> containing the dispel roll, Resonance changes,
    /// Corruption status, and AP cost details if the ability was successfully cast;
    /// null if prerequisites were not met.
    /// </returns>
    /// <remarks>
    /// <para>Prerequisites: Seiðkona specialization, Weave Disruption unlocked, sufficient AP
    /// (3 AP base, 2 AP with Resonance Cascade active at Resonance 5+).</para>
    /// <para>Execution order:</para>
    /// <list type="number">
    /// <item>Validate prerequisites (spec, ability, AP with Cascade consideration)</item>
    /// <item>Evaluate Corruption risk BEFORE spending resources (uses current Resonance)</item>
    /// <item>Spend AP (3 base or 2 with Cascade)</item>
    /// <item>Roll d20 for dispel attempt (stored in result for combat system resolution)</item>
    /// <item>Build +1 Resonance</item>
    /// <item>Apply Corruption if triggered</item>
    /// <item>Return complete result</item>
    /// </list>
    /// <para>Important: Weave Disruption does NOT deal direct damage and does NOT add to the
    /// Accumulated Aetheric Damage tracker. The d20 + Resonance bonus dispel roll is stored
    /// for the combat system to resolve against effect DCs.</para>
    /// </remarks>
    WeaveDisruptionResult? ExecuteWeaveDisruption(Player player, Guid targetId);

    /// <summary>
    /// Gets the current Resonance Cascade passive ability state.
    /// Returns whether the Cascade is active, its cost reduction, and current Resonance.
    /// </summary>
    /// <param name="player">The Seiðkona player to evaluate.</param>
    /// <returns>
    /// A <see cref="ResonanceCascadeState"/> reflecting the current Cascade status.
    /// Cascade is active when the ability is unlocked AND Resonance is 5+.
    /// </returns>
    ResonanceCascadeState GetResonanceCascadeState(Player player);

    /// <summary>
    /// Gets the effective AP cost for a Seiðkona ability, accounting for Resonance Cascade reduction.
    /// Used for UI display and AP validation.
    /// </summary>
    /// <param name="player">The Seiðkona player to evaluate.</param>
    /// <param name="abilityId">The ability to check the effective cost for.</param>
    /// <returns>
    /// The effective AP cost after Cascade reduction (min 1) if Cascade is active;
    /// the base AP cost if Cascade is not active or not unlocked.
    /// Returns 0 for passive abilities (Aether Attunement, Resonance Cascade).
    /// </returns>
    int GetEffectiveApCost(Player player, SeidkonaAbilityId abilityId);

    // ===== Tier 3 Abilities (v0.20.8c) =====

    /// <summary>
    /// Executes the Völva's Vision active mass-detection ability.
    /// Reveals all hidden, invisible, and concealed enemies within a 15-space radius.
    /// </summary>
    /// <param name="player">The Seiðkona player activating Völva's Vision.</param>
    /// <returns>
    /// A <see cref="VolvasVisionResult"/> containing reveal radius, Resonance changes,
    /// Corruption status, and AP cost details if the ability was successfully cast;
    /// null if prerequisites were not met.
    /// </returns>
    /// <remarks>
    /// <para>Prerequisites: Seiðkona specialization, Völva's Vision unlocked, sufficient AP
    /// (3 AP base, 2 AP with Resonance Cascade active at Resonance 5+).</para>
    /// <para>Execution order:</para>
    /// <list type="number">
    /// <item>Validate prerequisites (spec, ability, AP with Cascade consideration)</item>
    /// <item>Evaluate Corruption risk BEFORE spending resources (uses current Resonance)</item>
    /// <item>Spend AP (3 base or 2 with Cascade)</item>
    /// <item>Build +2 Resonance (high buildup, shared with Aether Storm)</item>
    /// <item>Apply Corruption if triggered</item>
    /// <item>Return complete result</item>
    /// </list>
    /// <para>Important: Völva's Vision does NOT deal damage and does NOT add to the
    /// Accumulated Aetheric Damage tracker. It is the enhanced version of the Tier 1
    /// Wyrd Sight ability, offering mass revelation instead of individual detection.
    /// The +2 Resonance gain represents significant escalation risk.</para>
    /// </remarks>
    VolvasVisionResult? ExecuteVolvasVision(Player player);

    /// <summary>
    /// Executes the Aether Storm active area-of-effect damage ability against a target.
    /// Unleashes 4d6 Aetheric damage in a cone area-of-effect.
    /// </summary>
    /// <param name="player">The Seiðkona player casting the storm.</param>
    /// <param name="targetId">Unique identifier of the primary target.</param>
    /// <returns>
    /// A <see cref="AetherStormResult"/> containing the full damage breakdown, Resonance changes,
    /// Corruption status, and AP cost details if the ability was successfully cast;
    /// null if prerequisites were not met.
    /// </returns>
    /// <remarks>
    /// <para>Prerequisites: Seiðkona specialization, Aether Storm unlocked, sufficient AP
    /// (5 AP base, 4 AP with Resonance Cascade active at Resonance 5+).</para>
    /// <para>Execution order:</para>
    /// <list type="number">
    /// <item>Validate prerequisites (spec, ability, AP with Cascade consideration)</item>
    /// <item>Evaluate Corruption risk BEFORE spending resources (uses current Resonance)</item>
    /// <item>Spend AP (5 base or 4 with Cascade)</item>
    /// <item>Roll damage (4d6 Aetheric)</item>
    /// <item>Build +2 Resonance (high buildup, shared with Völva's Vision)</item>
    /// <item>Add damage to Accumulated Aetheric Damage tracker</item>
    /// <item>Apply Corruption if triggered</item>
    /// <item>Return complete result</item>
    /// </list>
    /// <para>Aether Storm is the highest single-ability damage source in the Seiðkona arsenal
    /// before the Unraveling capstone. Its 4d6 damage combined with +2 Resonance makes it the
    /// fastest path to building both Accumulated Aetheric Damage and Resonance toward capstone
    /// readiness.</para>
    /// </remarks>
    AetherStormResult? ExecuteAetherStorm(Player player, Guid targetId);

    // ===== Capstone Ability (v0.20.8c) =====

    /// <summary>
    /// Executes the Unraveling capstone ability — the ultimate expression of Aetheric power.
    /// Releases 100% of Accumulated Aetheric Damage as a single devastating burst.
    /// </summary>
    /// <param name="player">The Seiðkona player executing the Unraveling.</param>
    /// <returns>
    /// An <see cref="UnravelingResult"/> containing the accumulated damage consumed, resource resets,
    /// Corruption outcome, and cooldown activation if the ability was successfully executed;
    /// null if prerequisites were not met.
    /// </returns>
    /// <remarks>
    /// <para>Prerequisites: Seiðkona specialization, Unraveling unlocked, 5+ AP, Resonance exactly 10,
    /// Accumulated Aetheric Damage &gt; 0, not already used this combat.</para>
    /// <para>Execution order:</para>
    /// <list type="number">
    /// <item>Validate prerequisites (spec, ability, cooldown, AP, Resonance == 10, AccumulatedDamage &gt; 0)</item>
    /// <item>Evaluate Corruption risk BEFORE spending resources (guaranteed 20% capstone check)</item>
    /// <item>Spend 5 AP (immune to Resonance Cascade reduction)</item>
    /// <item>Capture accumulated damage for release</item>
    /// <item>Reset Resonance from 10 to 0</item>
    /// <item>Reset Accumulated Aetheric Damage to 0</item>
    /// <item>Set per-combat cooldown flag</item>
    /// <item>Apply Corruption if triggered (+2 Corruption at CapstoneActivation tier)</item>
    /// <item>Return complete result</item>
    /// </list>
    /// <para>The Unraveling is the only Seiðkona ability that is immune to Resonance Cascade
    /// AP reduction, always costing exactly 5 AP. It can only be used once per combat encounter,
    /// resetting at combat end. The guaranteed 20% Corruption check is actually lower than the
    /// standard 25% risk at Resonance 10, but triggers +2 Corruption (elevated from standard +1).</para>
    /// </remarks>
    UnravelingResult? ExecuteUnraveling(Player player);

    // ===== Utility Methods =====

    /// <summary>
    /// Gets a readiness summary for all Seiðkona abilities for the specified player.
    /// Used for UI display to show which abilities are available.
    /// Tier 2 ability readiness accounts for Resonance Cascade AP cost reduction.
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
