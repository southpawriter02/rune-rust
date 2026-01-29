// ═══════════════════════════════════════════════════════════════════════════════
// IBackgroundProvider.cs
// Interface providing access to background definitions and their components.
// Version: 0.17.1d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Application.Exceptions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Provides access to background definitions and their components.
/// </summary>
/// <remarks>
/// <para>
/// IBackgroundProvider is the primary interface for accessing background data.
/// Implementations load background definitions from configuration and cache
/// them for efficient access. All methods are synchronous as the data
/// is loaded once and cached in memory.
/// </para>
/// <para>
/// The provider exposes both full <see cref="BackgroundDefinition"/> access and
/// convenience methods for accessing individual components (selection text, skill
/// grants, equipment grants, narrative hooks). This allows consumers to request
/// only the data they need without retrieving the entire definition.
/// </para>
/// <para>
/// <strong>Thread Safety:</strong> Implementations should be thread-safe for
/// concurrent reads. Configuration is loaded once on first access and never
/// modified thereafter.
/// </para>
/// <para>
/// <strong>Usage Examples:</strong>
/// <list type="bullet">
///   <item><description>Character creation UI: <see cref="GetAllBackgrounds"/> to display selection options</description></item>
///   <item><description>Background selection panel: <see cref="GetSelectionText"/> for flavor text display</description></item>
///   <item><description>Character factory: <see cref="GetSkillGrants"/> to apply skill bonuses during creation</description></item>
///   <item><description>Inventory initialization: <see cref="GetEquipmentGrants"/> to grant starting items</description></item>
///   <item><description>Narrative system: <see cref="GetNarrativeHooks"/> for dialogue/quest triggers</description></item>
/// </list>
/// </para>
/// </remarks>
/// <seealso cref="BackgroundDefinition"/>
/// <seealso cref="Background"/>
/// <seealso cref="BackgroundSkillGrant"/>
/// <seealso cref="BackgroundEquipmentGrant"/>
public interface IBackgroundProvider
{
    /// <summary>
    /// Gets all available background definitions.
    /// </summary>
    /// <returns>
    /// A read-only list of all background definitions, one for each <see cref="Background"/>
    /// enum value. The list is guaranteed to contain exactly 6 backgrounds when
    /// configuration is valid.
    /// </returns>
    /// <exception cref="BackgroundConfigurationException">
    /// Thrown if configuration cannot be loaded or validated.
    /// </exception>
    /// <example>
    /// <code>
    /// var backgrounds = backgroundProvider.GetAllBackgrounds();
    /// foreach (var bg in backgrounds)
    /// {
    ///     Console.WriteLine($"{bg.DisplayName}: {bg.Description}");
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<BackgroundDefinition> GetAllBackgrounds();

    /// <summary>
    /// Gets the background definition for a specific background.
    /// </summary>
    /// <param name="backgroundId">The background to retrieve.</param>
    /// <returns>
    /// The background definition, or <c>null</c> if not found (should not happen
    /// in normal operation as configuration is validated on startup).
    /// </returns>
    /// <example>
    /// <code>
    /// var villageSmith = backgroundProvider.GetBackground(Background.VillageSmith);
    /// if (villageSmith != null)
    /// {
    ///     Console.WriteLine($"Profession: {villageSmith.ProfessionBefore}");
    ///     Console.WriteLine($"Standing: {villageSmith.SocialStanding}");
    /// }
    /// </code>
    /// </example>
    BackgroundDefinition? GetBackground(Background backgroundId);

    /// <summary>
    /// Gets the selection text for a background, used in character creation UI.
    /// </summary>
    /// <param name="backgroundId">The background enum value.</param>
    /// <returns>
    /// The evocative second-person selection text for display in the creation UI,
    /// or a default "Unknown background" message if the background is not found.
    /// </returns>
    /// <remarks>
    /// Selection text is written in second person ("You worked...", "You learned...")
    /// to help players connect with the character's backstory during creation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var text = backgroundProvider.GetSelectionText(Background.VillageSmith);
    /// // Returns: "The ring of hammer on anvil was your morning song..."
    /// </code>
    /// </example>
    string GetSelectionText(Background backgroundId);

    /// <summary>
    /// Gets the skill grants for a specific background.
    /// </summary>
    /// <param name="backgroundId">The background enum value.</param>
    /// <returns>
    /// A read-only list of <see cref="BackgroundSkillGrant"/> instances for the
    /// specified background. Each background typically has a primary (+2) and
    /// secondary (+1) skill grant. Returns an empty list if the background is
    /// not found.
    /// </returns>
    /// <remarks>
    /// Skill grants are used by the IBackgroundApplicationService (v0.17.1e)
    /// to apply skill bonuses to the character during creation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var grants = backgroundProvider.GetSkillGrants(Background.VillageSmith);
    /// foreach (var grant in grants)
    /// {
    ///     Console.WriteLine($"{grant.SkillId}: +{grant.BonusAmount}");
    /// }
    /// // Output: craft: +2
    /// //         might: +1
    /// </code>
    /// </example>
    IReadOnlyList<BackgroundSkillGrant> GetSkillGrants(Background backgroundId);

    /// <summary>
    /// Gets the equipment grants for a specific background.
    /// </summary>
    /// <param name="backgroundId">The background enum value.</param>
    /// <returns>
    /// A read-only list of <see cref="BackgroundEquipmentGrant"/> instances for
    /// the specified background. Items may be auto-equipped or placed in inventory.
    /// Returns an empty list if the background is not found.
    /// </returns>
    /// <remarks>
    /// Equipment grants are used by the IBackgroundApplicationService (v0.17.1e)
    /// to create items and optionally equip them during character creation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var grants = backgroundProvider.GetEquipmentGrants(Background.ClanGuard);
    /// foreach (var grant in grants)
    /// {
    ///     Console.WriteLine($"{grant.ItemId} x{grant.Quantity} (equipped: {grant.IsEquipped})");
    /// }
    /// // Output: shield x1 (equipped: True)
    /// //         spear x1 (equipped: True)
    /// </code>
    /// </example>
    IReadOnlyList<BackgroundEquipmentGrant> GetEquipmentGrants(Background backgroundId);

    /// <summary>
    /// Gets the narrative hooks for a specific background.
    /// </summary>
    /// <param name="backgroundId">The background enum value.</param>
    /// <returns>
    /// A read-only list of narrative hook strings for the specified background.
    /// Each background typically has 3 hooks covering knowledge, social, and
    /// story aspects. Returns an empty list if the background is not found.
    /// </returns>
    /// <remarks>
    /// Narrative hooks are used by the narrative event system to determine
    /// whether background-specific dialogue or quest interactions should be
    /// triggered. Matching is typically performed via case-insensitive
    /// substring search.
    /// </remarks>
    /// <example>
    /// <code>
    /// var hooks = backgroundProvider.GetNarrativeHooks(Background.RuinDelver);
    /// // Returns: ["Know ruin layouts and hazards",
    /// //           "Scrap-Tinkers respect your finds",
    /// //           "May have enemies among rival delvers"]
    /// </code>
    /// </example>
    IReadOnlyList<string> GetNarrativeHooks(Background backgroundId);

    /// <summary>
    /// Checks if a background exists in the provider.
    /// </summary>
    /// <param name="backgroundId">The background enum value.</param>
    /// <returns>
    /// <c>true</c> if the background definition has been loaded and is available
    /// for retrieval; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// In normal operation after successful initialization, this should return
    /// <c>true</c> for all valid <see cref="Background"/> enum values.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (backgroundProvider.HasBackground(Background.VillageSmith))
    /// {
    ///     var definition = backgroundProvider.GetBackground(Background.VillageSmith);
    ///     // Safe to use definition here
    /// }
    /// </code>
    /// </example>
    bool HasBackground(Background backgroundId);
}
