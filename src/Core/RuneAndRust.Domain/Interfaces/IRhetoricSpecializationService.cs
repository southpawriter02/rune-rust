// ------------------------------------------------------------------------------
// <copyright file="IRhetoricSpecializationService.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Service interface for applying archetype specialization abilities to rhetoric checks.
// Part of v0.15.3i Specialization Integration implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Interfaces;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service for applying archetype specialization abilities to rhetoric checks.
/// </summary>
/// <remarks>
/// <para>
/// This service implements the Specialization Bonus Hook Pattern for Rhetoric skills,
/// enabling archetype-specific abilities to modify social interaction mechanics.
/// </para>
/// <para>
/// Supported archetypes and their abilities:
/// <list type="bullet">
///   <item><description>Thul: Voice of Reason, Scholarly Authority</description></item>
///   <item><description>Skald: Inspiring Words, Saga of Heroes</description></item>
///   <item><description>Kupmaðr: Silver Tongue, Sniff Out Lies</description></item>
///   <item><description>Myrk-gengr: Maintain Cover, Forge Documents</description></item>
/// </list>
/// </para>
/// <para>
/// The service is called at three points in the social interaction flow:
/// <list type="bullet">
///   <item><description>Pre-check: For dice bonuses and auto-success conditions</description></item>
///   <item><description>Post-check: For outcome modifications (prevent locking, fumble downgrade)</description></item>
///   <item><description>Special actions: For party buffs, stress relief, and asset creation</description></item>
/// </list>
/// </para>
/// </remarks>
public interface IRhetoricSpecializationService
{
    // -------------------------------------------------------------------------
    // Pre-Check Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets any pre-check bonuses applicable to the social interaction.
    /// </summary>
    /// <param name="playerId">The player character's ID.</param>
    /// <param name="interactionType">The type of social interaction.</param>
    /// <param name="context">The full social interaction context.</param>
    /// <returns>
    /// An effect containing dice bonuses or auto-success conditions.
    /// Returns <see cref="SpecializationAbilityEffect.None"/> if no ability applies.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method checks for abilities that modify the dice pool before the check:
    /// <list type="bullet">
    ///   <item><description>[Scholarly Authority]: +2d10 on lore/history topics</description></item>
    ///   <item><description>[Sniff Out Lies]: +2d10 when detecting deception</description></item>
    ///   <item><description>[Maintain Cover]: +2d10 when cover is challenged</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    SpecializationAbilityEffect GetPreCheckBonus(
        Guid playerId,
        SocialInteractionType interactionType,
        SocialContext context);

    /// <summary>
    /// Checks if the player can auto-succeed on a check.
    /// </summary>
    /// <param name="playerId">The player character's ID.</param>
    /// <param name="interactionType">The type of social interaction.</param>
    /// <param name="finalDc">The final DC after all modifiers.</param>
    /// <returns>True if auto-success applies and the check should be bypassed.</returns>
    /// <remarks>
    /// <para>
    /// Currently only [Silver Tongue] provides auto-success:
    /// <list type="bullet">
    ///   <item><description>Kupmaðr archetype</description></item>
    ///   <item><description>Negotiation interaction type</description></item>
    ///   <item><description>Final DC ≤ 12</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    bool CanAutoSucceed(
        Guid playerId,
        SocialInteractionType interactionType,
        int finalDc);

    // -------------------------------------------------------------------------
    // Post-Check Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Applies post-check modifications based on specialization abilities.
    /// </summary>
    /// <param name="playerId">The player character's ID.</param>
    /// <param name="result">The social check result.</param>
    /// <param name="context">The social interaction context.</param>
    /// <returns>
    /// An effect describing outcome modifications.
    /// Returns <see cref="SpecializationAbilityEffect.None"/> if no ability applies.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method checks for abilities that modify the outcome:
    /// <list type="bullet">
    ///   <item><description>[Voice of Reason]: Prevents option locking on failed persuasion</description></item>
    ///   <item><description>[Maintain Cover]: Downgrades fumble consequences</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Note: [Voice of Reason] does NOT prevent fumble consequences (Trust Shattered still locks).
    /// </para>
    /// </remarks>
    SpecializationAbilityEffect ApplyPostCheckModification(
        Guid playerId,
        SocialResult result,
        SocialContext context);

    // -------------------------------------------------------------------------
    // Skald Party Actions
    // -------------------------------------------------------------------------

    /// <summary>
    /// Applies Skald's [Inspiring Words] to party members.
    /// </summary>
    /// <param name="skaldId">The Skald player's ID.</param>
    /// <param name="targetIds">Party members to inspire.</param>
    /// <returns>
    /// Result of the inspiration attempt. On success, grants +1d10 to targets.
    /// On exceptional success (net ≥ 3), affects all party members.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The Skald must succeed a DC 12 Rhetoric check to activate this ability.
    /// </para>
    /// <para>
    /// Cooldown: Once per scene (cannot spam inspiration).
    /// Duration: One check per target (within next 10 minutes).
    /// </para>
    /// </remarks>
    Task<SpecializationAbilityEffect> ApplyInspirationAsync(
        Guid skaldId,
        IEnumerable<Guid> targetIds);

    /// <summary>
    /// Performs Skald's [Saga of Heroes] during rest.
    /// </summary>
    /// <param name="skaldId">The Skald player's ID.</param>
    /// <param name="listenerIds">Party members listening to the story.</param>
    /// <returns>
    /// Result of the storytelling session. On success, reduces stress for all listeners.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The Skald must succeed a DC 10 Rhetoric check.
    /// </para>
    /// <para>
    /// Stress reduction based on success tier:
    /// <list type="bullet">
    ///   <item><description>Marginal: -1 Stress</description></item>
    ///   <item><description>Full: -2 Stress</description></item>
    ///   <item><description>Exceptional: -3 Stress</description></item>
    ///   <item><description>Critical: -4 Stress</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Cooldown: Once per rest period.
    /// Duration: Takes approximately 30 minutes in-game.
    /// </para>
    /// </remarks>
    Task<SpecializationAbilityEffect> PerformStorytellingAsync(
        Guid skaldId,
        IEnumerable<Guid> listenerIds);

    // -------------------------------------------------------------------------
    // Myrk-gengr Actions
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a forged document using Myrk-gengr's [Forge Documents].
    /// </summary>
    /// <param name="forgerId">The Myrk-gengr player's ID.</param>
    /// <param name="documentType">The type of document to forge.</param>
    /// <param name="hasReferenceSample">Whether a reference sample is available (+2d10).</param>
    /// <returns>
    /// Result of the forgery attempt. On success, creates a forged document asset.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Document types and their DCs:
    /// <list type="bullet">
    ///   <item><description>Simple note: DC 10, 10 minutes</description></item>
    ///   <item><description>Travel papers: DC 12, 30 minutes</description></item>
    ///   <item><description>Guild credentials: DC 14, 1 hour</description></item>
    ///   <item><description>Official orders: DC 16, 2 hours</description></item>
    ///   <item><description>Faction authorization: DC 18, 4 hours</description></item>
    ///   <item><description>Rare/complex: DC 20+, 8+ hours</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Forgery quality based on success tier:
    /// <list type="bullet">
    ///   <item><description>Passable: -2 DC to detect</description></item>
    ///   <item><description>Good: -4 DC to detect</description></item>
    ///   <item><description>Excellent: -6 DC to detect</description></item>
    ///   <item><description>Masterwork: Requires expertise to detect</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    Task<SpecializationAbilityEffect> CreateForgedDocumentAsync(
        Guid forgerId,
        string documentType,
        bool hasReferenceSample);

    // -------------------------------------------------------------------------
    // Query Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets all rhetoric abilities for a player's archetype.
    /// </summary>
    /// <param name="playerId">The player character's ID.</param>
    /// <returns>List of available abilities, or empty if archetype has none.</returns>
    /// <remarks>
    /// Returns abilities based on archetype:
    /// <list type="bullet">
    ///   <item><description>Thul: [Voice of Reason], [Scholarly Authority]</description></item>
    ///   <item><description>Skald: [Inspiring Words], [Saga of Heroes]</description></item>
    ///   <item><description>Kupmaðr: [Silver Tongue], [Sniff Out Lies]</description></item>
    ///   <item><description>Myrk-gengr: [Maintain Cover], [Forge Documents]</description></item>
    /// </list>
    /// </remarks>
    IReadOnlyList<RhetoricSpecializationAbility> GetPlayerAbilities(Guid playerId);

    /// <summary>
    /// Gets the archetype ID for a player.
    /// </summary>
    /// <param name="playerId">The player character's ID.</param>
    /// <returns>The archetype ID string, or null if not found.</returns>
    string? GetPlayerArchetype(Guid playerId);

    /// <summary>
    /// Checks whether a player has a specific rhetoric ability.
    /// </summary>
    /// <param name="playerId">The player character's ID.</param>
    /// <param name="ability">The ability to check for.</param>
    /// <returns>True if the player's archetype has the specified ability.</returns>
    bool HasAbility(Guid playerId, RhetoricSpecializationAbility ability);

    /// <summary>
    /// Gets ability details for display purposes.
    /// </summary>
    /// <param name="ability">The ability to get details for.</param>
    /// <returns>A tuple containing display name, description, and archetype.</returns>
    (string Name, string Description, string Archetype) GetAbilityDetails(
        RhetoricSpecializationAbility ability);
}
