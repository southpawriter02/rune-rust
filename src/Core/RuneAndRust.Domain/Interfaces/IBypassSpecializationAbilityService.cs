// ------------------------------------------------------------------------------
// <copyright file="IBypassSpecializationAbilityService.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Service interface for managing bypass specialization abilities, including
// passive effects, triggered abilities, and unique actions.
// Part of v0.15.4i Specialization Integration implementation.
// </summary>
// ------------------------------------------------------------------------------

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Interfaces;

/// <summary>
/// Service interface for managing bypass specialization abilities.
/// </summary>
/// <remarks>
/// <para>
/// This service handles all specialization abilities related to the System Bypass
/// skill system. It processes passive bonuses, triggered effects, and unique actions
/// for the four bypass specializations:
/// <list type="bullet">
///   <item><description><b>Scrap-Tinker:</b> [Master Craftsman], [Relock]</description></item>
///   <item><description><b>Ruin-Stalker:</b> [Trap Artist], [Sixth Sense]</description></item>
///   <item><description><b>Jötun-Reader:</b> [Deep Access], [Pattern Recognition]</description></item>
///   <item><description><b>Gantry-Runner:</b> [Fast Pick], [Bypass Under Fire]</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Ability Types:</b>
/// <list type="bullet">
///   <item><description><b>Passive:</b> Applied automatically via GetPassiveModifiers</description></item>
///   <item><description><b>Triggered:</b> Applied via TryTriggerAbility after outcomes</description></item>
///   <item><description><b>UniqueAction:</b> Activated via ExecuteUniqueAction</description></item>
/// </list>
/// </para>
/// </remarks>
public interface IBypassSpecializationAbilityService
{
    // =========================================================================
    // ABILITY QUERIES
    // =========================================================================

    /// <summary>
    /// Gets the bypass specialization for a character.
    /// </summary>
    /// <param name="characterId">The character's unique identifier.</param>
    /// <returns>The character's bypass specialization, or None if unspecialized.</returns>
    /// <remarks>
    /// <para>
    /// Bypass specialization is derived from the character's archetype.
    /// Characters without a bypass-focused archetype return <see cref="BypassSpecialization.None"/>.
    /// </para>
    /// </remarks>
    BypassSpecialization GetCharacterSpecialization(string characterId);

    /// <summary>
    /// Gets all abilities available to a character based on their specialization.
    /// </summary>
    /// <param name="characterId">The character's unique identifier.</param>
    /// <returns>A list of abilities the character has access to.</returns>
    /// <remarks>
    /// <para>
    /// Returns empty list for characters without a bypass specialization.
    /// </para>
    /// </remarks>
    IReadOnlyList<BypassSpecializationAbility> GetCharacterAbilities(string characterId);

    /// <summary>
    /// Checks if a character has a specific ability.
    /// </summary>
    /// <param name="characterId">The character's unique identifier.</param>
    /// <param name="abilityId">The ability identifier to check.</param>
    /// <returns>True if the character has access to the ability.</returns>
    bool HasAbility(string characterId, string abilityId);

    /// <summary>
    /// Gets an ability definition by its ID.
    /// </summary>
    /// <param name="abilityId">The ability identifier.</param>
    /// <returns>The ability definition, or null if not found.</returns>
    BypassSpecializationAbility? GetAbilityDefinition(string abilityId);

    // =========================================================================
    // PASSIVE ABILITY APPLICATION
    // =========================================================================

    /// <summary>
    /// Gets passive modifiers that should apply to a bypass attempt.
    /// </summary>
    /// <param name="characterId">The character attempting the bypass.</param>
    /// <param name="bypassType">The type of bypass being attempted.</param>
    /// <param name="isInDanger">Whether the character is in combat or under threat.</param>
    /// <param name="isTargetGlitched">Whether the target has [Glitched] status.</param>
    /// <returns>Context containing all applicable passive modifiers.</returns>
    /// <remarks>
    /// <para>
    /// This method evaluates all passive abilities and returns those that apply
    /// to the current bypass context. The calling service should apply these
    /// modifiers to the bypass attempt.
    /// </para>
    /// <para>
    /// <b>Passive abilities checked:</b>
    /// <list type="bullet">
    ///   <item><description>[Fast Pick] - If any bypass type</description></item>
    ///   <item><description>[Bypass Under Fire] - If isInDanger is true</description></item>
    ///   <item><description>[Pattern Recognition] - If isTargetGlitched is true</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    BypassAbilityContext GetPassiveModifiers(
        string characterId,
        BypassType bypassType,
        bool isInDanger,
        bool isTargetGlitched);

    /// <summary>
    /// Checks for trap detection via [Sixth Sense] when a character moves.
    /// </summary>
    /// <param name="characterId">The character who moved.</param>
    /// <param name="characterX">Character's X position.</param>
    /// <param name="characterY">Character's Y position.</param>
    /// <param name="nearbyTraps">Traps in the area (from game state).</param>
    /// <returns>Detection result containing any detected traps.</returns>
    /// <remarks>
    /// <para>
    /// Called when a Ruin-Stalker moves to check for nearby traps.
    /// Non-Ruin-Stalkers receive an empty result.
    /// </para>
    /// </remarks>
    TrapDetectionResult CheckTrapDetection(
        string characterId,
        int characterX,
        int characterY,
        IEnumerable<TrapInfo> nearbyTraps);

    // =========================================================================
    // TRIGGERED ABILITY APPLICATION
    // =========================================================================

    /// <summary>
    /// Attempts to trigger abilities based on a bypass outcome.
    /// </summary>
    /// <param name="characterId">The character who completed the bypass.</param>
    /// <param name="bypassType">The type of bypass that was performed.</param>
    /// <param name="wasSuccessful">Whether the bypass succeeded.</param>
    /// <param name="targetId">ID of the target (lock, terminal, trap, etc.).</param>
    /// <returns>Activation result if an ability triggered, null otherwise.</returns>
    /// <remarks>
    /// <para>
    /// Called after a bypass attempt completes to check for triggered abilities:
    /// <list type="bullet">
    ///   <item><description>[Deep Access] - On successful terminal hack</description></item>
    ///   <item><description>[Master Craftsman] - On craft attempt</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    BypassAbilityActivationResult? TryTriggerAbility(
        string characterId,
        BypassType bypassType,
        bool wasSuccessful,
        string targetId);

    // =========================================================================
    // UNIQUE ACTION EXECUTION
    // =========================================================================

    /// <summary>
    /// Executes the [Relock] unique action.
    /// </summary>
    /// <param name="characterId">The Scrap-Tinker character.</param>
    /// <param name="lockId">The ID of the lock to relock.</param>
    /// <param name="witsScore">The character's WITS attribute score.</param>
    /// <returns>The result of the relock attempt.</returns>
    /// <remarks>
    /// <para>
    /// <b>[Relock] Rules:</b>
    /// <list type="bullet">
    ///   <item><description>Requires Scrap-Tinker specialization</description></item>
    ///   <item><description>Lock must have been previously picked</description></item>
    ///   <item><description>WITS check DC 12</description></item>
    ///   <item><description>On success, lock returns to locked state</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if character doesn't have the [Relock] ability or lock wasn't picked.
    /// </exception>
    BypassAbilityActivationResult ExecuteRelock(
        string characterId,
        string lockId,
        int witsScore);

    /// <summary>
    /// Executes the [Trap Artist] unique action.
    /// </summary>
    /// <param name="characterId">The Ruin-Stalker character.</param>
    /// <param name="trapId">The ID of the trap to re-arm.</param>
    /// <param name="witsScore">The character's WITS attribute score.</param>
    /// <returns>The result of the re-arm attempt.</returns>
    /// <remarks>
    /// <para>
    /// <b>[Trap Artist] Rules:</b>
    /// <list type="bullet">
    ///   <item><description>Requires Ruin-Stalker specialization</description></item>
    ///   <item><description>Trap must have been previously disabled</description></item>
    ///   <item><description>WITS check DC 14</description></item>
    ///   <item><description>On success, trap becomes controlled by party</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if character doesn't have the [Trap Artist] ability or trap wasn't disabled.
    /// </exception>
    BypassAbilityActivationResult ExecuteTrapArtist(
        string characterId,
        string trapId,
        int witsScore);

    // =========================================================================
    // MASTERWORK CRAFTING
    // =========================================================================

    /// <summary>
    /// Gets masterwork recipes available to a character.
    /// </summary>
    /// <param name="characterId">The character to check.</param>
    /// <returns>List of available masterwork recipes, empty if not a Scrap-Tinker.</returns>
    /// <remarks>
    /// <para>
    /// Only Scrap-Tinkers with [Master Craftsman] have access to masterwork recipes.
    /// </para>
    /// </remarks>
    IReadOnlyList<MasterworkRecipe> GetAvailableMasterworkRecipes(string characterId);

    /// <summary>
    /// Checks if a character can craft a specific masterwork recipe.
    /// </summary>
    /// <param name="characterId">The character to check.</param>
    /// <param name="recipeId">The recipe to check.</param>
    /// <param name="availableComponents">Components the character has.</param>
    /// <returns>True if the character has the ability and required components.</returns>
    bool CanCraftMasterwork(
        string characterId,
        string recipeId,
        IEnumerable<string> availableComponents);

    // =========================================================================
    // CHARACTER REGISTRATION
    // =========================================================================

    /// <summary>
    /// Registers a character's bypass specialization.
    /// </summary>
    /// <param name="characterId">The character's unique identifier.</param>
    /// <param name="specialization">The bypass specialization to assign.</param>
    /// <remarks>
    /// <para>
    /// Called during character creation or when a character gains a bypass
    /// specialization through gameplay. This registration persists for the
    /// session and determines which abilities the character can use.
    /// </para>
    /// </remarks>
    void RegisterCharacterSpecialization(string characterId, BypassSpecialization specialization);

    /// <summary>
    /// Unregisters a character's bypass specialization.
    /// </summary>
    /// <param name="characterId">The character's unique identifier.</param>
    /// <remarks>
    /// <para>
    /// Called when a character is removed from the game or loses their
    /// specialization. The character will no longer have access to
    /// specialization abilities.
    /// </para>
    /// </remarks>
    void UnregisterCharacter(string characterId);
}

// =============================================================================
// SUPPORTING TYPES
// =============================================================================

/// <summary>
/// Context containing passive ability modifiers for a bypass attempt.
/// </summary>
/// <remarks>
/// <para>
/// Contains all modifiers that should be applied to a bypass attempt based
/// on the character's passive abilities and the current context.
/// </para>
/// </remarks>
/// <param name="TimeReductionRounds">Rounds to subtract from bypass time.</param>
/// <param name="DcReduction">Amount to reduce DC by.</param>
/// <param name="NegateCombatPenalties">Whether combat penalties are negated.</param>
/// <param name="AppliedAbilities">IDs of abilities that contributed modifiers.</param>
public readonly record struct BypassAbilityContext(
    int TimeReductionRounds,
    int DcReduction,
    bool NegateCombatPenalties,
    IReadOnlyList<string> AppliedAbilities)
{
    /// <summary>
    /// Gets a value indicating whether any modifiers apply.
    /// </summary>
    public bool HasModifiers => TimeReductionRounds > 0 ||
                                 DcReduction > 0 ||
                                 NegateCombatPenalties;

    /// <summary>
    /// Creates an empty context with no modifiers.
    /// </summary>
    /// <returns>An empty bypass ability context.</returns>
    public static BypassAbilityContext Empty() => new(
        TimeReductionRounds: 0,
        DcReduction: 0,
        NegateCombatPenalties: false,
        AppliedAbilities: Array.Empty<string>());

    /// <summary>
    /// Creates a display string for the context.
    /// </summary>
    /// <returns>A formatted string showing applied modifiers.</returns>
    public string ToDisplayString()
    {
        if (!HasModifiers)
        {
            return "No passive modifiers apply.";
        }

        var parts = new List<string>();
        if (TimeReductionRounds > 0)
        {
            parts.Add($"-{TimeReductionRounds} rounds");
        }

        if (DcReduction > 0)
        {
            parts.Add($"-{DcReduction} DC");
        }

        if (NegateCombatPenalties)
        {
            parts.Add("combat penalties negated");
        }

        return $"Passive modifiers: {string.Join(", ", parts)} " +
               $"(from {string.Join(", ", AppliedAbilities)})";
    }
}

/// <summary>
/// Information about a trap for detection purposes.
/// </summary>
/// <remarks>
/// <para>
/// Minimal trap information passed to the service for [Sixth Sense] detection.
/// The full trap entity remains in the game state.
/// </para>
/// </remarks>
/// <param name="TrapId">The trap's unique identifier.</param>
/// <param name="TrapName">Display name of the trap.</param>
/// <param name="PositionX">X coordinate of the trap.</param>
/// <param name="PositionY">Y coordinate of the trap.</param>
/// <param name="IsArmed">Whether the trap is currently armed.</param>
/// <param name="IsHidden">Whether the trap is hidden from normal perception.</param>
public readonly record struct TrapInfo(
    string TrapId,
    string TrapName,
    int PositionX,
    int PositionY,
    bool IsArmed,
    bool IsHidden);
