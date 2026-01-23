namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Defines operations for specialization ability management.
/// </summary>
/// <remarks>
/// <para>
/// The specialization ability service handles activation of specialization-specific
/// abilities, checks prerequisites and uses, and applies effects to the underlying
/// skill systems (climbing, leaping, stealth, etc.).
/// </para>
/// <para>
/// <b>Gantry-Runner Abilities:</b>
/// <list type="bullet">
///   <item><description>[Roof-Runner]: Reduce climbing stages by 1</description></item>
///   <item><description>[Death-Defying Leap]: +10 ft maximum leap</description></item>
///   <item><description>[Wall-Run]: Vertical run combat action</description></item>
///   <item><description>[Double Jump]: Reroll failed leap (1/day)</description></item>
///   <item><description>[Featherfall]: Auto-succeed Crash Landing DC ≤ 3</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Myrk-gengr Abilities:</b>
/// <list type="bullet">
///   <item><description>[Slip into Shadow]: Enter [Hidden] without action</description></item>
///   <item><description>[Ghostly Form]: Stay [Hidden] after attack (1/encounter)</description></item>
///   <item><description>[Cloak the Party]: +2d10 party Passive Stealth</description></item>
///   <item><description>[One with the Static]: Auto-[Hidden] in [Psychic Resonance]</description></item>
/// </list>
/// </para>
/// <para>
/// <b>v0.15.2g:</b> Initial implementation of specialization ability service.
/// </para>
/// </remarks>
public interface ISpecializationAbilityService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // ABILITY QUERIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if a character has a specific Gantry-Runner ability.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <param name="ability">The ability to check.</param>
    /// <returns>True if the character has the ability.</returns>
    bool HasAbility(string characterId, GantryRunnerAbility ability);

    /// <summary>
    /// Checks if a character has a specific Myrk-gengr ability.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <param name="ability">The ability to check.</param>
    /// <returns>True if the character has the ability.</returns>
    bool HasAbility(string characterId, MyrkengrAbility ability);

    /// <summary>
    /// Gets the definition for a Gantry-Runner ability.
    /// </summary>
    /// <param name="ability">The ability.</param>
    /// <returns>The ability definition.</returns>
    SpecializationAbilityDefinition GetDefinition(GantryRunnerAbility ability);

    /// <summary>
    /// Gets the definition for a Myrk-gengr ability.
    /// </summary>
    /// <param name="ability">The ability.</param>
    /// <returns>The ability definition.</returns>
    SpecializationAbilityDefinition GetDefinition(MyrkengrAbility ability);

    /// <summary>
    /// Gets remaining uses of a limited ability.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <param name="abilityId">The ability ID.</param>
    /// <returns>Uses remaining, or -1 for unlimited.</returns>
    int GetRemainingUses(string characterId, string abilityId);

    // ═══════════════════════════════════════════════════════════════════════════
    // GANTRY-RUNNER ABILITY ACTIVATIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Applies [Roof-Runner] stage reduction to a climb.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <param name="originalStages">Original number of stages.</param>
    /// <returns>The activation result with modified stage count.</returns>
    AbilityActivationResult ApplyRoofRunner(string characterId, int originalStages);

    /// <summary>
    /// Applies [Death-Defying Leap] distance bonus.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <param name="baseMaxDistance">Base maximum leap distance.</param>
    /// <returns>The activation result with modified max distance.</returns>
    AbilityActivationResult ApplyDeathDefyingLeap(string characterId, int baseMaxDistance);

    /// <summary>
    /// Activates [Wall-Run] combat action.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <param name="heightFeet">Height to run up.</param>
    /// <param name="dicePool">Available dice pool for the check.</param>
    /// <returns>The activation result.</returns>
    AbilityActivationResult ActivateWallRun(string characterId, int heightFeet, int dicePool);

    /// <summary>
    /// Activates [Double Jump] to reroll a failed leap.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <returns>The activation result with dice bonus.</returns>
    AbilityActivationResult ActivateDoubleJump(string characterId);

    /// <summary>
    /// Checks if [Featherfall] auto-succeeds for a crash landing DC.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <param name="crashLandingDc">The crash landing DC.</param>
    /// <returns>The activation result.</returns>
    AbilityActivationResult CheckFeatherfall(string characterId, int crashLandingDc);

    // ═══════════════════════════════════════════════════════════════════════════
    // MYRK-GENGR ABILITY ACTIVATIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Activates [Slip into Shadow] for free [Hidden] entry.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <param name="isInShadows">Whether character is in shadows.</param>
    /// <param name="isObserved">Whether character is actively observed.</param>
    /// <returns>The activation result.</returns>
    AbilityActivationResult ActivateSlipIntoShadow(string characterId, bool isInShadows, bool isObserved);

    /// <summary>
    /// Activates [Ghostly Form] to maintain [Hidden] after attacking.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <returns>The activation result.</returns>
    AbilityActivationResult ActivateGhostlyForm(string characterId);

    /// <summary>
    /// Activates [Cloak the Party] to grant party stealth bonus.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <param name="partyMemberIds">IDs of party members in range.</param>
    /// <returns>The activation result with dice bonus.</returns>
    AbilityActivationResult ActivateCloakTheParty(string characterId, IReadOnlyList<string> partyMemberIds);

    /// <summary>
    /// Checks [One with the Static] for auto-[Hidden] in [Psychic Resonance].
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <param name="isInPsychicResonance">Whether in [Psychic Resonance] zone.</param>
    /// <returns>The activation result.</returns>
    AbilityActivationResult CheckOneWithTheStatic(string characterId, bool isInPsychicResonance);

    // ═══════════════════════════════════════════════════════════════════════════
    // USE MANAGEMENT
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Resets daily ability uses for a character (on long rest).
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    void ResetDailyUses(string characterId);

    /// <summary>
    /// Resets encounter ability uses for a character (on combat end).
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    void ResetEncounterUses(string characterId);
}
