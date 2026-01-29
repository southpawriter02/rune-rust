// ═══════════════════════════════════════════════════════════════════════════════
// ILineageApplicationService.cs
// Interface for the service that applies lineage bonuses, traits, and trauma
// baselines to characters during creation.
// Version: 0.17.0f
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Application.DTOs;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Applies lineage bonuses, traits, and trauma baselines to characters during creation.
/// </summary>
/// <remarks>
/// <para>
/// ILineageApplicationService orchestrates the application of all lineage components
/// to a <see cref="Player"/> entity during character creation. It coordinates:
/// </para>
/// <list type="bullet">
///   <item><description>Attribute modifier application (fixed bonuses/penalties per lineage)</description></item>
///   <item><description>Passive bonus application (HP, AP, Soak, Movement, Skills)</description></item>
///   <item><description>Unique trait registration (signature lineage ability)</description></item>
///   <item><description>Trauma Economy baseline configuration (starting Corruption/Stress, resistance modifiers)</description></item>
///   <item><description>Flexible bonus handling (Clan-Born's +1 to any attribute)</description></item>
/// </list>
/// <para>
/// <strong>Usage Pattern:</strong>
/// <list type="number">
///   <item><description>Call <see cref="ValidateLineageSelection"/> to check eligibility (no side effects)</description></item>
///   <item><description>Show the player what will be applied (using <see cref="ILineageProvider"/>)</description></item>
///   <item><description>Call <see cref="ApplyLineage"/> to apply all components atomically</description></item>
///   <item><description>Use the returned <see cref="LineageApplicationResult"/> for UI display</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Thread Safety:</strong> Implementations are not required to be thread-safe.
/// Lineage application is a single-threaded character creation operation.
/// </para>
/// </remarks>
/// <seealso cref="ILineageProvider"/>
/// <seealso cref="LineageApplicationResult"/>
/// <seealso cref="LineageValidationResult"/>
public interface ILineageApplicationService
{
    /// <summary>
    /// Applies the specified lineage to a character during creation.
    /// </summary>
    /// <param name="character">The player character to apply lineage to. Must not be null.</param>
    /// <param name="lineage">The lineage to apply.</param>
    /// <param name="flexibleBonusAttribute">
    /// The attribute to apply the flexible bonus to. Required for Clan-Born lineage
    /// (which grants +1 to any attribute of the player's choice). Must be <c>null</c>
    /// for all other lineages.
    /// </param>
    /// <returns>
    /// A <see cref="LineageApplicationResult"/> containing:
    /// <list type="bullet">
    ///   <item><description>Success status and any failure reason</description></item>
    ///   <item><description>All attribute modifications applied</description></item>
    ///   <item><description>Passive bonuses granted</description></item>
    ///   <item><description>Unique trait registered</description></item>
    ///   <item><description>Trauma baseline values set</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method performs validation before applying any changes. If validation
    /// fails, the character is not modified and a failure result is returned.
    /// </para>
    /// <para>
    /// On success, all lineage components are applied to the character in order:
    /// attribute modifiers, passive bonuses, unique trait, trauma baseline, then
    /// the lineage selection itself.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Apply Rune-Marked lineage (no flexible bonus)
    /// var result = service.ApplyLineage(player, Lineage.RuneMarked);
    ///
    /// // Apply Clan-Born lineage with +1 Might
    /// var result = service.ApplyLineage(player, Lineage.ClanBorn, CoreAttribute.Might);
    /// </code>
    /// </example>
    LineageApplicationResult ApplyLineage(
        Player character,
        Lineage lineage,
        CoreAttribute? flexibleBonusAttribute = null);

    /// <summary>
    /// Validates whether a lineage can be applied to a character without side effects.
    /// </summary>
    /// <param name="character">The player character to validate. Must not be null.</param>
    /// <param name="lineage">The lineage to validate.</param>
    /// <param name="flexibleBonusAttribute">
    /// The flexible bonus attribute selection, if applicable. Required for Clan-Born,
    /// must be <c>null</c> for other lineages.
    /// </param>
    /// <returns>
    /// A <see cref="LineageValidationResult"/> indicating whether the lineage can be applied.
    /// If invalid, <see cref="LineageValidationResult.FailureReason"/> describes the issue.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is read-only and does not modify the character in any way.
    /// It checks:
    /// <list type="bullet">
    ///   <item><description>Character is not null</description></item>
    ///   <item><description>Character does not already have a lineage</description></item>
    ///   <item><description>Lineage exists in the provider</description></item>
    ///   <item><description>Clan-Born has a flexible bonus attribute selected</description></item>
    ///   <item><description>Non-Clan-Born does not have a flexible bonus attribute</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var validation = service.ValidateLineageSelection(player, Lineage.ClanBorn, CoreAttribute.Will);
    /// if (!validation.IsValid)
    /// {
    ///     Console.WriteLine($"Cannot apply lineage: {validation.FailureReason}");
    /// }
    /// </code>
    /// </example>
    LineageValidationResult ValidateLineageSelection(
        Player character,
        Lineage lineage,
        CoreAttribute? flexibleBonusAttribute = null);

    /// <summary>
    /// Gets all lineages that can be applied to the character.
    /// </summary>
    /// <param name="character">The player character to check.</param>
    /// <returns>
    /// A read-only list of applicable <see cref="Lineage"/> values.
    /// Returns an empty list if the character already has a lineage assigned.
    /// Returns all four lineages if the character has no lineage.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is used by the character creation UI to populate the
    /// lineage selection screen. If the player has already chosen a lineage,
    /// no further selections are available.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var lineages = service.GetApplicableLineages(player);
    /// foreach (var lineage in lineages)
    /// {
    ///     Console.WriteLine(lineage);
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<Lineage> GetApplicableLineages(Player character);
}
