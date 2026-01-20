using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Provides access to crafting station definitions loaded from configuration.
/// </summary>
/// <remarks>
/// <para>
/// This interface abstracts the loading and retrieval of crafting station definitions,
/// allowing for different implementations (JSON, database, etc.) while maintaining
/// a consistent API for the application layer.
/// </para>
/// <para>
/// Station providers are typically registered as singletons in the DI container,
/// loading all definitions once at startup and caching them for efficient access.
/// Multiple indexes are maintained for fast lookups by category and skill.
/// </para>
/// <para>
/// Key features:
/// <list type="bullet">
///   <item><description>Case-insensitive station ID lookups</description></item>
///   <item><description>Filtering by recipe category for crafting validation</description></item>
///   <item><description>Filtering by crafting skill for skill-based queries</description></item>
///   <item><description>Skill ID lookup for crafting check calculations</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Get a specific station
/// var anvil = stationProvider.GetStation("anvil");
///
/// // Get all stations that can craft weapons
/// var weaponStations = stationProvider.GetStationsForCategory(RecipeCategory.Weapon);
///
/// // Get the crafting skill for a station
/// var skill = stationProvider.GetCraftingSkill("anvil"); // "smithing"
///
/// // Check if a station exists
/// if (stationProvider.Exists("alchemy-table"))
/// {
///     Console.WriteLine("Alchemy table is available!");
/// }
/// </code>
/// </example>
public interface ICraftingStationProvider
{
    /// <summary>
    /// Gets a crafting station definition by its unique string identifier.
    /// </summary>
    /// <param name="stationId">The station identifier (case-insensitive).</param>
    /// <returns>The station definition, or null if not found.</returns>
    /// <remarks>
    /// Station IDs are normalized to lowercase during loading, so lookups
    /// with any casing will work correctly.
    /// </remarks>
    /// <example>
    /// <code>
    /// var station = stationProvider.GetStation("anvil");
    /// if (station is not null)
    /// {
    ///     Console.WriteLine($"Found: {station.Name}, Skill: {station.CraftingSkillId}");
    /// }
    /// </code>
    /// </example>
    CraftingStationDefinition? GetStation(string stationId);

    /// <summary>
    /// Gets all registered crafting station definitions.
    /// </summary>
    /// <returns>A read-only list of all station definitions.</returns>
    /// <remarks>
    /// The returned list is a snapshot and will not reflect any subsequent changes
    /// if the provider supports dynamic reloading.
    /// </remarks>
    /// <example>
    /// <code>
    /// var allStations = stationProvider.GetAllStations();
    /// Console.WriteLine($"Total stations: {allStations.Count}");
    /// foreach (var station in allStations)
    /// {
    ///     Console.WriteLine($"- {station.Name}: {station.GetCategoriesDisplay()}");
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<CraftingStationDefinition> GetAllStations();

    /// <summary>
    /// Gets all stations that support a specific recipe category.
    /// </summary>
    /// <param name="category">The recipe category to filter by.</param>
    /// <returns>A read-only list of stations that can craft recipes in this category.</returns>
    /// <remarks>
    /// <para>
    /// Used to determine which stations a player can use to craft a recipe,
    /// or to display available crafting options at a location.
    /// </para>
    /// <para>
    /// A recipe can be crafted at any station that supports its category,
    /// though the station must also be present in the player's current room.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Find where weapons can be crafted
    /// var weaponStations = stationProvider.GetStationsForCategory(RecipeCategory.Weapon);
    /// Console.WriteLine($"Weapons can be crafted at: {string.Join(", ", weaponStations.Select(s => s.Name))}");
    /// // Output: Weapons can be crafted at: Anvil, Enchanting Altar
    /// </code>
    /// </example>
    IReadOnlyList<CraftingStationDefinition> GetStationsForCategory(RecipeCategory category);

    /// <summary>
    /// Gets the crafting skill ID for a station.
    /// </summary>
    /// <param name="stationId">The station identifier (case-insensitive).</param>
    /// <returns>The skill ID used for crafting checks, or null if station not found.</returns>
    /// <remarks>
    /// <para>
    /// The skill ID is used to look up the player's skill modifier for crafting checks.
    /// For example, the "anvil" station uses "smithing", so the player's smithing
    /// modifier is added to their 2d6 roll when crafting at the anvil.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var skillId = stationProvider.GetCraftingSkill("anvil");
    /// if (skillId is not null)
    /// {
    ///     var playerModifier = player.GetSkillModifier(skillId);
    ///     Console.WriteLine($"Player's smithing modifier: {playerModifier}");
    /// }
    /// </code>
    /// </example>
    string? GetCraftingSkill(string stationId);

    /// <summary>
    /// Checks if a station definition exists.
    /// </summary>
    /// <param name="stationId">The station identifier to check (case-insensitive).</param>
    /// <returns>True if the station exists, false otherwise.</returns>
    /// <remarks>
    /// Use this for quick existence checks without retrieving the full definition.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (stationProvider.Exists("mythril-forge"))
    /// {
    ///     Console.WriteLine("Mythril forge is available!");
    /// }
    /// </code>
    /// </example>
    bool Exists(string stationId);

    /// <summary>
    /// Gets the total count of registered station definitions.
    /// </summary>
    /// <returns>The number of station definitions loaded.</returns>
    /// <remarks>
    /// Useful for logging and diagnostics to verify expected configuration loading.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Total crafting stations: {stationProvider.GetStationCount()}");
    /// </code>
    /// </example>
    int GetStationCount();

    /// <summary>
    /// Gets all stations that use a specific crafting skill.
    /// </summary>
    /// <param name="skillId">The skill identifier (case-insensitive).</param>
    /// <returns>A read-only list of stations that use this skill for crafting checks.</returns>
    /// <remarks>
    /// <para>
    /// Useful for displaying which stations a player can improve at by leveling
    /// a particular skill, or for skill-based filtering in the UI.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var smithingStations = stationProvider.GetStationsBySkill("smithing");
    /// Console.WriteLine($"Smithing is used at: {string.Join(", ", smithingStations.Select(s => s.Name))}");
    /// // Output: Smithing is used at: Anvil
    /// </code>
    /// </example>
    IReadOnlyList<CraftingStationDefinition> GetStationsBySkill(string skillId);
}
