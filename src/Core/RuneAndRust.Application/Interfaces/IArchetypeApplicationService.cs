// ═══════════════════════════════════════════════════════════════════════════════
// IArchetypeApplicationService.cs
// Interface for the service that applies archetypes to characters during
// creation, including resource bonuses, starting abilities, and specialization
// mappings. Also defines ArchetypeValidationResult and ArchetypeApplicationPreview.
// Version: 0.17.3f
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Application.ValueObjects;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Provides services for applying archetypes to characters during creation.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IArchetypeApplicationService"/> orchestrates the application of archetype
/// bonuses, abilities, and specialization mappings to a <see cref="Player"/> entity
/// during character creation Step 4 (Archetype Selection). It coordinates with
/// <see cref="IArchetypeProvider"/> for data retrieval and applies the archetype in
/// the following order:
/// </para>
/// <list type="number">
///   <item><description>Validate preconditions (no existing archetype, valid enum, definition exists)</description></item>
///   <item><description>Retrieve archetype data from <see cref="IArchetypeProvider"/></description></item>
///   <item><description>Apply resource bonuses (HP, Stamina, Aether Pool, Movement, Special)</description></item>
///   <item><description>Grant starting abilities (3 per archetype: Active/Passive/Stance)</description></item>
///   <item><description>Set the archetype identifier on the character entity</description></item>
///   <item><description>Record available specializations for Step 5 selection</description></item>
/// </list>
/// <para>
/// <strong>Permanent Choice:</strong> Archetype selection is permanent and cannot be
/// changed after character creation. The service validates this precondition before
/// application, rejecting attempts to apply a second archetype.
/// </para>
/// <para>
/// <strong>Usage Pattern:</strong>
/// <list type="number">
///   <item><description>Call <see cref="GetApplicationPreview"/> to display archetype details before selection</description></item>
///   <item><description>Call <see cref="CanApplyArchetype"/> to validate before applying</description></item>
///   <item><description>Call <see cref="ApplyArchetype"/> to apply all bonuses and abilities</description></item>
///   <item><description>Use the returned <see cref="ArchetypeApplicationResult"/> for UI feedback</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Thread Safety:</strong> Implementations are not required to be thread-safe.
/// Archetype application is a single-threaded character creation operation.
/// </para>
/// </remarks>
/// <seealso cref="IArchetypeProvider"/>
/// <seealso cref="ArchetypeApplicationResult"/>
/// <seealso cref="ArchetypeValidationResult"/>
/// <seealso cref="ArchetypeApplicationPreview"/>
public interface IArchetypeApplicationService
{
    /// <summary>
    /// Applies an archetype's bonuses, abilities, and specialization mappings to a character.
    /// </summary>
    /// <param name="character">
    /// The player character to apply the archetype to. Must not be null.
    /// The character should not already have an archetype assigned (permanent choice).
    /// </param>
    /// <param name="archetype">
    /// The archetype to apply, identifying which resource bonuses, starting abilities,
    /// and specialization mappings to bestow upon the character.
    /// </param>
    /// <returns>
    /// An <see cref="ArchetypeApplicationResult"/> containing:
    /// <list type="bullet">
    ///   <item><description>Success status and any failure reason</description></item>
    ///   <item><description>Resource bonuses applied (HP, Stamina, Aether Pool, Movement, Special)</description></item>
    ///   <item><description>List of all abilities granted (3 per archetype)</description></item>
    ///   <item><description>Available specializations for future selection (Step 5)</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method performs the following operations in order:
    /// <list type="number">
    ///   <item><description>Validates preconditions via <see cref="CanApplyArchetype"/></description></item>
    ///   <item><description>Retrieves archetype definition, resource bonuses, abilities, and specializations from <see cref="IArchetypeProvider"/></description></item>
    ///   <item><description>Applies resource bonuses to the character's derived stats</description></item>
    ///   <item><description>Grants 3 starting abilities to the character</description></item>
    ///   <item><description>Sets the archetype identifier on the character entity</description></item>
    ///   <item><description>Records available specializations for Step 5</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// If validation fails or the archetype definition is not found, a failure result is
    /// returned and the character is not modified.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = service.ApplyArchetype(player, Archetype.Warrior);
    /// if (result.Success)
    /// {
    ///     Console.WriteLine(result.GetSummary());
    ///     // "Applied Warrior:
    ///     //   Resource Bonuses: +49 HP, +5 Stamina
    ///     //   Abilities Granted: 3
    ///     //   Specializations Available: 6"
    /// }
    /// </code>
    /// </example>
    ArchetypeApplicationResult ApplyArchetype(
        Player character,
        Archetype archetype);

    /// <summary>
    /// Checks whether an archetype can be applied to a character.
    /// </summary>
    /// <param name="character">
    /// The player character to check. May be null (produces an invalid result).
    /// </param>
    /// <param name="archetype">
    /// The archetype to validate against the character's current state.
    /// </param>
    /// <returns>
    /// An <see cref="ArchetypeValidationResult"/> containing:
    /// <list type="bullet">
    ///   <item><description><see cref="ArchetypeValidationResult.IsValid"/> indicating whether the archetype can be applied</description></item>
    ///   <item><description><see cref="ArchetypeValidationResult.Issues"/> listing any validation problems found</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// Validation checks performed:
    /// <list type="number">
    ///   <item><description>Character is not null</description></item>
    ///   <item><description>Character does not already have an archetype (permanent choice)</description></item>
    ///   <item><description>Archetype is a valid enum value</description></item>
    ///   <item><description>Archetype definition exists in the provider</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// This method is read-only and does not modify any state. Use it for early
    /// validation before calling <see cref="ApplyArchetype"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var validation = service.CanApplyArchetype(player, Archetype.Warrior);
    /// if (validation.IsValid)
    /// {
    ///     var result = service.ApplyArchetype(player, Archetype.Warrior);
    /// }
    /// else
    /// {
    ///     foreach (var issue in validation.Issues)
    ///         Console.WriteLine($"Issue: {issue}");
    /// }
    /// </code>
    /// </example>
    ArchetypeValidationResult CanApplyArchetype(
        Player? character,
        Archetype archetype);

    /// <summary>
    /// Gets a preview of what applying an archetype would grant.
    /// </summary>
    /// <param name="archetype">The archetype to preview.</param>
    /// <returns>
    /// An <see cref="ArchetypeApplicationPreview"/> containing the archetype's
    /// display name, resource bonuses, starting abilities, and available
    /// specializations. Used by the UI to show the player what they would
    /// receive before confirming their permanent selection.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is read-only and does not modify any state. It retrieves
    /// all archetype data from <see cref="IArchetypeProvider"/> and assembles
    /// a preview suitable for direct display in the character creation interface.
    /// </para>
    /// <para>
    /// If the archetype definition is not found, the preview uses the archetype
    /// enum name as the display name and returns default/empty values for other
    /// properties.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var preview = service.GetApplicationPreview(Archetype.Warrior);
    /// Console.WriteLine($"{preview.DisplayName} ({preview.Archetype})");
    /// Console.WriteLine($"  HP Bonus: +{preview.ResourceBonuses.MaxHpBonus}");
    /// Console.WriteLine($"  Abilities: {preview.StartingAbilities.Count}");
    /// Console.WriteLine($"  Specializations: {preview.AvailableSpecializations.Count}");
    /// </code>
    /// </example>
    ArchetypeApplicationPreview GetApplicationPreview(Archetype archetype);
}

/// <summary>
/// Result of validating whether an archetype can be applied to a character.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ArchetypeValidationResult"/> is a lightweight record used by
/// <see cref="IArchetypeApplicationService.CanApplyArchetype"/> to report
/// validation status and any issues found.
/// </para>
/// <para>
/// Use <see cref="Valid"/> for a successful validation or <see cref="Invalid"/>
/// to create a result with specific validation issues.
/// </para>
/// </remarks>
/// <param name="IsValid">Whether the archetype can be applied.</param>
/// <param name="Issues">List of validation issues, if any. Empty when valid.</param>
/// <seealso cref="IArchetypeApplicationService"/>
/// <seealso cref="ArchetypeApplicationResult"/>
public readonly record struct ArchetypeValidationResult(
    bool IsValid,
    IReadOnlyList<string> Issues)
{
    /// <summary>
    /// Creates a valid result indicating the archetype can be applied.
    /// </summary>
    /// <value>
    /// A <see cref="ArchetypeValidationResult"/> with <see cref="IsValid"/> set to
    /// <c>true</c> and an empty <see cref="Issues"/> list.
    /// </value>
    public static ArchetypeValidationResult Valid => new(true, Array.Empty<string>());

    /// <summary>
    /// Creates an invalid result with the specified validation issues.
    /// </summary>
    /// <param name="issues">
    /// One or more human-readable descriptions of validation problems.
    /// </param>
    /// <returns>
    /// A <see cref="ArchetypeValidationResult"/> with <see cref="IsValid"/> set to
    /// <c>false</c> and the provided issues.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = ArchetypeValidationResult.Invalid(
    ///     "Character already has archetype: Warrior. Archetype is a permanent choice.");
    /// </code>
    /// </example>
    public static ArchetypeValidationResult Invalid(params string[] issues) =>
        new(false, issues);
}

/// <summary>
/// Preview of archetype application for UI display before selection.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ArchetypeApplicationPreview"/> aggregates all archetype data into a
/// single object suitable for display in the character creation interface. It shows
/// what the player would receive if they confirm their archetype selection.
/// </para>
/// <para>
/// This is a read-only preview — no character modifications are made.
/// </para>
/// </remarks>
/// <param name="Archetype">The archetype being previewed.</param>
/// <param name="DisplayName">The human-readable display name of the archetype (e.g., "Warrior").</param>
/// <param name="ResourceBonuses">The resource bonuses the archetype would grant.</param>
/// <param name="StartingAbilities">The 3 starting abilities the archetype would grant.</param>
/// <param name="AvailableSpecializations">The specializations that would become available for Step 5.</param>
/// <seealso cref="IArchetypeApplicationService"/>
/// <seealso cref="ArchetypeResourceBonuses"/>
/// <seealso cref="ArchetypeAbilityGrant"/>
/// <seealso cref="ArchetypeSpecializationMapping"/>
public readonly record struct ArchetypeApplicationPreview(
    Archetype Archetype,
    string DisplayName,
    ArchetypeResourceBonuses ResourceBonuses,
    IReadOnlyList<ArchetypeAbilityGrant> StartingAbilities,
    ArchetypeSpecializationMapping AvailableSpecializations)
{
    /// <summary>
    /// Gets the total number of distinct elements in this preview.
    /// </summary>
    /// <value>
    /// The count of resource bonus categories (1 if any bonuses exist) plus
    /// the number of starting abilities plus the number of available specializations.
    /// </value>
    /// <remarks>
    /// Useful for UI layout calculations to determine the preview panel size.
    /// </remarks>
    public int TotalElements =>
        (ResourceBonuses.HasHpBonus || ResourceBonuses.HasStaminaBonus ||
         ResourceBonuses.HasAetherPoolBonus || ResourceBonuses.HasMovementBonus ||
         ResourceBonuses.HasSpecialBonus ? 1 : 0) +
        StartingAbilities.Count +
        AvailableSpecializations.Count;
}
