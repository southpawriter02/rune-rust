// ═══════════════════════════════════════════════════════════════════════════════
// IBackgroundApplicationService.cs
// Interface for the service that applies background grants (skills, equipment)
// to characters during creation and generates previews for UI display.
// Version: 0.17.1e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Application.ValueObjects;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Provides services for applying backgrounds to characters during creation.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IBackgroundApplicationService"/> orchestrates the application of background
/// grants to a <see cref="Player"/> entity during character creation. It coordinates with
/// <see cref="IBackgroundProvider"/> for data retrieval and applies skill and equipment
/// grants to the character.
/// </para>
/// <para>
/// <strong>Responsibilities:</strong>
/// <list type="bullet">
///   <item><description>Skill grant application (Permanent bonuses from background profession)</description></item>
///   <item><description>Equipment grant creation and inventory addition</description></item>
///   <item><description>Auto-equip logic for items marked as equipped (weapons, armor, shields)</description></item>
///   <item><description>Preview generation for UI display before selection</description></item>
///   <item><description>Validation that a background exists and can be applied</description></item>
///   <item><description>Setting the background identifier on the character entity</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Usage Pattern:</strong>
/// <list type="number">
///   <item><description>Call <see cref="GetAllBackgroundPreviews"/> to display selection options</description></item>
///   <item><description>Call <see cref="GetBackgroundPreview"/> for detailed preview on hover/select</description></item>
///   <item><description>Call <see cref="CanApplyBackground"/> to validate before applying</description></item>
///   <item><description>Call <see cref="ApplyBackgroundToCharacter"/> to apply all grants</description></item>
///   <item><description>Use the returned <see cref="BackgroundApplicationResult"/> for UI feedback</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Thread Safety:</strong> Implementations are not required to be thread-safe.
/// Background application is a single-threaded character creation operation.
/// </para>
/// </remarks>
/// <seealso cref="IBackgroundProvider"/>
/// <seealso cref="BackgroundApplicationResult"/>
/// <seealso cref="BackgroundPreview"/>
public interface IBackgroundApplicationService
{
    /// <summary>
    /// Applies a background's grants to a character during creation.
    /// </summary>
    /// <param name="character">
    /// The player character to apply the background to. Must not be null.
    /// The character should not already have a background assigned.
    /// </param>
    /// <param name="background">
    /// The background to apply, identifying which skill and equipment grants
    /// to bestow upon the character.
    /// </param>
    /// <returns>
    /// A <see cref="BackgroundApplicationResult"/> containing:
    /// <list type="bullet">
    ///   <item><description>Success status and any error messages</description></item>
    ///   <item><description>List of all skills granted with amounts and types</description></item>
    ///   <item><description>List of all equipment granted with quantities and equip status</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method performs the following operations in order:
    /// <list type="number">
    ///   <item><description>Retrieves the background definition from <see cref="IBackgroundProvider"/></description></item>
    ///   <item><description>Applies all skill grants to the character via <c>ModifySkill</c></description></item>
    ///   <item><description>Creates equipment items and adds them to inventory</description></item>
    ///   <item><description>Auto-equips items marked as equipped to their designated slots</description></item>
    ///   <item><description>Sets the background identifier on the character entity</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// If the background is not found in the provider, a failure result is returned
    /// and the character is not modified.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = service.ApplyBackgroundToCharacter(player, Background.VillageSmith);
    /// if (result.Success)
    /// {
    ///     Console.WriteLine($"Skills: {result.GetSkillSummary()}");
    ///     Console.WriteLine($"Equipment: {result.GetEquipmentSummary()}");
    /// }
    /// </code>
    /// </example>
    BackgroundApplicationResult ApplyBackgroundToCharacter(
        Player character,
        Background background);

    /// <summary>
    /// Gets a preview of a background for UI display.
    /// </summary>
    /// <param name="background">The background to preview.</param>
    /// <returns>
    /// A <see cref="BackgroundPreview"/> containing formatted summaries of the
    /// background's grants, suitable for direct display in the character creation
    /// interface. Returns <see cref="BackgroundPreview.Empty"/> if the background
    /// is not found.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is read-only and does not modify any state. It retrieves the
    /// background definition and formats its data into a UI-ready preview object.
    /// </para>
    /// <para>
    /// Preview data includes: display name, description, selection text,
    /// profession before the Silence, social standing, skill summary,
    /// equipment summary, and narrative hook count.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var preview = service.GetBackgroundPreview(Background.ClanGuard);
    /// Console.WriteLine($"{preview.DisplayName}: {preview.Description}");
    /// Console.WriteLine($"Skills: {preview.SkillSummary}");
    /// Console.WriteLine($"Equipment: {preview.EquipmentSummary}");
    /// </code>
    /// </example>
    BackgroundPreview GetBackgroundPreview(Background background);

    /// <summary>
    /// Gets previews for all available backgrounds.
    /// </summary>
    /// <returns>
    /// A read-only list of <see cref="BackgroundPreview"/> objects, one for each
    /// available background. The list is guaranteed to contain 6 previews when
    /// configuration is valid.
    /// </returns>
    /// <remarks>
    /// This method is used by the character creation UI to populate the background
    /// selection screen with all available options and their summaries.
    /// </remarks>
    /// <example>
    /// <code>
    /// var previews = service.GetAllBackgroundPreviews();
    /// foreach (var preview in previews)
    /// {
    ///     Console.WriteLine($"{preview.DisplayName}: {preview.SkillSummary}");
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<BackgroundPreview> GetAllBackgroundPreviews();

    /// <summary>
    /// Validates that a background can be applied.
    /// </summary>
    /// <param name="background">The background to validate.</param>
    /// <returns>
    /// <c>true</c> if the background exists in the provider and can be applied;
    /// <c>false</c> if the background is not found or is otherwise invalid.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is a lightweight validation check that delegates to
    /// <see cref="IBackgroundProvider.HasBackground"/>. It does not check
    /// character state (e.g., whether the character already has a background).
    /// </para>
    /// <para>
    /// Use this method before calling <see cref="ApplyBackgroundToCharacter"/>
    /// to provide early feedback in the UI.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (service.CanApplyBackground(Background.WanderingSkald))
    /// {
    ///     var result = service.ApplyBackgroundToCharacter(player, Background.WanderingSkald);
    /// }
    /// </code>
    /// </example>
    bool CanApplyBackground(Background background);
}
