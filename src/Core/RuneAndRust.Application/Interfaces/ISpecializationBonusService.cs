// ------------------------------------------------------------------------------
// <copyright file="ISpecializationBonusService.cs" company="Rune & Rust">
//     Copyright (c) Rune & Rust. All rights reserved.
// </copyright>
// <summary>
// Service interface for managing Wasteland Survival specialization abilities,
// including passive bonuses, active abilities, and hunting grounds markers.
// Part of v0.15.5h Specialization Integration implementation.
// </summary>
// ------------------------------------------------------------------------------

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service interface for managing Wasteland Survival specialization bonuses and abilities.
/// </summary>
/// <remarks>
/// <para>
/// This service handles all specialization abilities related to the Wasteland Survival
/// skill system. It processes passive bonuses, triggered effects, and active abilities
/// for the three Wasteland Survival specializations:
/// <list type="bullet">
///   <item><description><b>Veioimaor (Hunter):</b> Beast Tracker, Predator's Eye, Hunting Grounds</description></item>
///   <item><description><b>Myr-Stalker:</b> Swamp Navigator, Toxin Resistance, Mire Knowledge</description></item>
///   <item><description><b>Gantry-Runner:</b> Urban Navigator, Rooftop Routes, Scrap Familiar</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Ability Types:</b>
/// <list type="bullet">
///   <item><description><b>Passive:</b> Applied automatically via <see cref="GetBonusesForCheck"/></description></item>
///   <item><description><b>Active:</b> Activated via <see cref="ActivateAbility"/></description></item>
/// </list>
/// </para>
/// <para>
/// <b>Integration with v0.15.5 Subsystems:</b>
/// <list type="bullet">
///   <item><description>TrackingService calls <see cref="GetBonusesForCheck"/> with <see cref="WastelandSurvivalCheckType.Tracking"/></description></item>
///   <item><description>ForagingService calls <see cref="GetBonusesForCheck"/> with <see cref="WastelandSurvivalCheckType.Foraging"/></description></item>
///   <item><description>NavigationService calls <see cref="GetBonusesForCheck"/> with <see cref="WastelandSurvivalCheckType.Navigation"/></description></item>
///   <item><description>HazardDetectionService calls <see cref="HasAdvantage"/> for poison hazard saves</description></item>
/// </list>
/// </para>
/// </remarks>
/// <seealso cref="WastelandSurvivalSpecializationType"/>
/// <seealso cref="SpecializationBonus"/>
/// <seealso cref="AbilityActivation"/>
/// <seealso cref="HuntingGroundsMarker"/>
public interface ISpecializationBonusService
{
    // =========================================================================
    // SPECIALIZATION QUERIES
    // =========================================================================

    /// <summary>
    /// Gets the Wasteland Survival specialization for a character.
    /// </summary>
    /// <param name="characterId">The character's unique identifier.</param>
    /// <returns>
    /// The character's Wasteland Survival specialization, or
    /// <see cref="WastelandSurvivalSpecializationType.None"/> if unspecialized.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Characters must be registered via <see cref="RegisterCharacterSpecialization"/>
    /// before their specialization can be queried. Unregistered characters
    /// return <see cref="WastelandSurvivalSpecializationType.None"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var spec = service.GetCharacterSpecialization("player-1");
    /// if (spec == WastelandSurvivalSpecializationType.Veioimaor)
    /// {
    ///     // Character is a Veioimaor (Hunter)
    /// }
    /// </code>
    /// </example>
    WastelandSurvivalSpecializationType GetCharacterSpecialization(string characterId);

    /// <summary>
    /// Registers a character's Wasteland Survival specialization.
    /// </summary>
    /// <param name="characterId">The character's unique identifier.</param>
    /// <param name="specialization">The specialization to assign.</param>
    /// <remarks>
    /// <para>
    /// Called during character creation or when a character gains a Wasteland Survival
    /// specialization through gameplay. This registration persists for the session
    /// and determines which abilities the character can use.
    /// </para>
    /// <para>
    /// Registering a character that is already registered will update their
    /// specialization. Registering with <see cref="WastelandSurvivalSpecializationType.None"/>
    /// effectively removes the character's specialization.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Register a new Veioimaor during character creation
    /// service.RegisterCharacterSpecialization("player-1", WastelandSurvivalSpecializationType.Veioimaor);
    /// </code>
    /// </example>
    void RegisterCharacterSpecialization(string characterId, WastelandSurvivalSpecializationType specialization);

    /// <summary>
    /// Unregisters a character, removing their specialization and clearing any active markers.
    /// </summary>
    /// <param name="characterId">The character's unique identifier.</param>
    /// <remarks>
    /// <para>
    /// Called when a character is removed from the game or loses their specialization.
    /// This also clears any active hunting grounds markers for the character.
    /// </para>
    /// </remarks>
    void UnregisterCharacter(string characterId);

    // =========================================================================
    // PASSIVE BONUS APPLICATION
    // =========================================================================

    /// <summary>
    /// Gets all applicable bonuses for a Wasteland Survival check.
    /// </summary>
    /// <param name="characterId">The character making the check.</param>
    /// <param name="checkType">The type of Wasteland Survival check.</param>
    /// <param name="terrain">The current terrain type (for Navigator abilities).</param>
    /// <param name="targetType">The target type (for tracking checks).</param>
    /// <param name="areaId">The current area ID (for Hunting Grounds).</param>
    /// <returns>A list of applicable bonuses to apply to the check.</returns>
    /// <remarks>
    /// <para>
    /// This method evaluates all passive abilities and returns those that apply
    /// to the current check context. The calling service should apply these
    /// bonuses to the skill check.
    /// </para>
    /// <para>
    /// <b>Passive abilities checked:</b>
    /// <list type="bullet">
    ///   <item><description><b>Beast Tracker:</b> If checkType is Tracking and targetType is LivingCreature or Group</description></item>
    ///   <item><description><b>Swamp Navigator:</b> If terrain matches swamp/marsh conditions</description></item>
    ///   <item><description><b>Urban Navigator:</b> If terrain is ModerateRuins or DenseRuins</description></item>
    ///   <item><description><b>Scrap Familiar:</b> If checkType is Foraging and terrain is ruins</description></item>
    ///   <item><description><b>Hunting Grounds:</b> If areaId matches an active marker for this character</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Get bonuses for a tracking check against a living creature
    /// var bonuses = service.GetBonusesForCheck(
    ///     "player-1",
    ///     WastelandSurvivalCheckType.Tracking,
    ///     NavigationTerrainType.OpenWasteland,
    ///     TargetType.LivingCreature,
    ///     "area-12");
    ///
    /// var totalDice = bonuses.Sum(b => b.BonusDice);
    /// // totalDice might be 4 if Beast Tracker (+2) and Hunting Grounds (+2) both apply
    /// </code>
    /// </example>
    IReadOnlyList<SpecializationBonus> GetBonusesForCheck(
        string characterId,
        WastelandSurvivalCheckType checkType,
        NavigationTerrainType terrain,
        TargetType targetType = TargetType.Unknown,
        string? areaId = null);

    /// <summary>
    /// Gets the total bonus dice to add to a check from all applicable abilities.
    /// </summary>
    /// <param name="characterId">The character making the check.</param>
    /// <param name="checkType">The type of Wasteland Survival check.</param>
    /// <param name="terrain">The current terrain type.</param>
    /// <param name="targetType">The target type (for tracking checks).</param>
    /// <param name="areaId">The current area ID (for Hunting Grounds).</param>
    /// <returns>The total number of bonus d10s to add to the check.</returns>
    /// <remarks>
    /// <para>
    /// This is a convenience method that sums the dice from all applicable bonuses.
    /// It is equivalent to calling <see cref="GetBonusesForCheck"/> and summing the
    /// <see cref="SpecializationBonus.BonusDice"/> values.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var bonusDice = service.GetTotalBonusDice(
    ///     "player-1",
    ///     WastelandSurvivalCheckType.Tracking,
    ///     NavigationTerrainType.OpenWasteland,
    ///     TargetType.LivingCreature);
    ///
    /// // Add bonusDice extra d10s to the skill check
    /// var totalDice = baseDice + bonusDice;
    /// </code>
    /// </example>
    int GetTotalBonusDice(
        string characterId,
        WastelandSurvivalCheckType checkType,
        NavigationTerrainType terrain,
        TargetType targetType = TargetType.Unknown,
        string? areaId = null);

    /// <summary>
    /// Checks if a character has advantage on a specific type of check.
    /// </summary>
    /// <param name="characterId">The character to check.</param>
    /// <param name="hazardType">The hazard type (for Toxin Resistance).</param>
    /// <returns>True if the character has advantage on this check.</returns>
    /// <remarks>
    /// <para>
    /// Currently, only Toxin Resistance grants advantage, specifically for
    /// <see cref="HazardType.PoisonGas"/> hazard saves.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Check if character has advantage on poison gas save
    /// if (service.HasAdvantage("player-1", HazardType.PoisonGas))
    /// {
    ///     // Roll twice, keep best result
    /// }
    /// </code>
    /// </example>
    bool HasAdvantage(string characterId, HazardType hazardType);

    // =========================================================================
    // ABILITY QUERIES
    // =========================================================================

    /// <summary>
    /// Checks if a character has access to a specific ability.
    /// </summary>
    /// <param name="characterId">The character to check.</param>
    /// <param name="abilityId">The ability ID to check for.</param>
    /// <returns>True if the character has access to this ability.</returns>
    /// <remarks>
    /// <para>
    /// Ability access is determined by the character's specialization:
    /// <list type="bullet">
    ///   <item><description>Veioimaor: beast-tracker, predators-eye, hunting-grounds</description></item>
    ///   <item><description>Myr-Stalker: swamp-navigator, toxin-resistance, mire-knowledge</description></item>
    ///   <item><description>Gantry-Runner: urban-navigator, rooftop-routes, scrap-familiar</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (service.HasAbility("player-1", "hunting-grounds"))
    /// {
    ///     // Character can mark hunting grounds
    /// }
    /// </code>
    /// </example>
    bool HasAbility(string characterId, string abilityId);

    // =========================================================================
    // ACTIVE ABILITY ACTIVATION
    // =========================================================================

    /// <summary>
    /// Activates an active ability for a character.
    /// </summary>
    /// <param name="characterId">The character activating the ability.</param>
    /// <param name="abilityId">The ID of the ability to activate.</param>
    /// <param name="context">Ability-specific context data.</param>
    /// <returns>The activation result, or null if activation failed.</returns>
    /// <remarks>
    /// <para>
    /// Active abilities require explicit activation by the player:
    /// <list type="bullet">
    ///   <item><description><b>Predator's Eye:</b> Context should include creatureWeakness and behaviorPattern</description></item>
    ///   <item><description><b>Hunting Grounds:</b> Context should include areaId and areaName</description></item>
    ///   <item><description><b>Mire Knowledge:</b> Context should include pathDescription and optional hazardsAvoided</description></item>
    ///   <item><description><b>Rooftop Routes:</b> Context should include routeDescription and optional destination</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Returns null if:
    /// <list type="bullet">
    ///   <item><description>Character doesn't have the ability</description></item>
    ///   <item><description>Required context data is missing</description></item>
    ///   <item><description>Activation conditions are not met</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Activate Hunting Grounds
    /// var context = new Dictionary&lt;string, string&gt;
    /// {
    ///     ["areaId"] = "area-12",
    ///     ["areaName"] = "The Rusted Valley"
    /// };
    /// var result = service.ActivateAbility("player-1", "hunting-grounds", context);
    /// if (result.HasValue)
    /// {
    ///     Console.WriteLine(result.Value.EffectDescription);
    /// }
    /// </code>
    /// </example>
    AbilityActivation? ActivateAbility(
        string characterId,
        string abilityId,
        IReadOnlyDictionary<string, string> context);

    // =========================================================================
    // HUNTING GROUNDS MANAGEMENT
    // =========================================================================

    /// <summary>
    /// Gets the active hunting grounds marker for a character.
    /// </summary>
    /// <param name="characterId">The character to check.</param>
    /// <returns>The active marker, or null if no hunting grounds are marked.</returns>
    /// <remarks>
    /// <para>
    /// A character can only have one active hunting grounds marker at a time.
    /// The marker persists until the character rests or marks a new area.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var marker = service.GetActiveHuntingGrounds("player-1");
    /// if (marker.HasValue &amp;&amp; marker.Value.IsActive)
    /// {
    ///     Console.WriteLine($"Hunting grounds active in {marker.Value.AreaName}");
    /// }
    /// </code>
    /// </example>
    HuntingGroundsMarker? GetActiveHuntingGrounds(string characterId);

    /// <summary>
    /// Marks an area as hunting grounds for a Veioimaor character.
    /// </summary>
    /// <param name="characterId">The character marking the area.</param>
    /// <param name="areaId">The ID of the area to mark.</param>
    /// <param name="areaName">The display name of the area.</param>
    /// <returns>The created marker, or null if the character cannot mark hunting grounds.</returns>
    /// <remarks>
    /// <para>
    /// Only Veioimaor characters with the Hunting Grounds ability can mark areas.
    /// Marking a new area automatically replaces any existing hunting grounds marker.
    /// </para>
    /// <para>
    /// Returns null if the character doesn't have the Hunting Grounds ability.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var marker = service.MarkHuntingGrounds("player-1", "area-12", "The Rusted Valley");
    /// if (marker.HasValue)
    /// {
    ///     // +2d10 to all WS checks in this area until rest
    /// }
    /// </code>
    /// </example>
    HuntingGroundsMarker? MarkHuntingGrounds(string characterId, string areaId, string areaName);

    /// <summary>
    /// Clears the hunting grounds marker for a character.
    /// </summary>
    /// <param name="characterId">The character whose marker should be cleared.</param>
    /// <remarks>
    /// <para>
    /// Called when a character rests or explicitly clears their marker.
    /// Has no effect if the character has no active marker.
    /// </para>
    /// </remarks>
    void ClearHuntingGrounds(string characterId);

    // =========================================================================
    // TERRAIN UTILITY METHODS
    // =========================================================================

    /// <summary>
    /// Checks if a terrain type qualifies as swamp or marsh terrain.
    /// </summary>
    /// <param name="terrain">The terrain type to check.</param>
    /// <returns>True if the terrain grants swamp/marsh bonuses.</returns>
    /// <remarks>
    /// <para>
    /// Swamp terrain enables Myr-Stalker abilities:
    /// <list type="bullet">
    ///   <item><description>Swamp Navigator: +1d10 to all WS checks</description></item>
    ///   <item><description>Mire Knowledge: Can reveal safe paths</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    bool IsSwampTerrain(NavigationTerrainType terrain);

    /// <summary>
    /// Checks if a terrain type qualifies as ruins terrain.
    /// </summary>
    /// <param name="terrain">The terrain type to check.</param>
    /// <returns>True if the terrain grants ruins bonuses.</returns>
    /// <remarks>
    /// <para>
    /// Ruins terrain enables Gantry-Runner abilities:
    /// <list type="bullet">
    ///   <item><description>Urban Navigator: +1d10 to all WS checks</description></item>
    ///   <item><description>Scrap Familiar: +1d10 to foraging</description></item>
    ///   <item><description>Rooftop Routes: Can reveal elevated paths</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Currently, <see cref="NavigationTerrainType.ModerateRuins"/> and
    /// <see cref="NavigationTerrainType.DenseRuins"/> qualify as ruins terrain.
    /// </para>
    /// </remarks>
    bool IsRuinsTerrain(NavigationTerrainType terrain);
}
