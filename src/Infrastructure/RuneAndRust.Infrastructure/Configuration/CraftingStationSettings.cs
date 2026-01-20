namespace RuneAndRust.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for crafting station definitions.
/// </summary>
/// <remarks>
/// <para>
/// This class is used for JSON binding via IOptions pattern. It contains
/// the raw DTO data loaded from the CraftingStations configuration section.
/// </para>
/// <para>
/// Expected JSON format (in appsettings.json or separate config):
/// </para>
/// <code>
/// {
///   "CraftingStations": {
///     "Stations": [
///       {
///         "Id": "anvil",
///         "Name": "Anvil",
///         "Description": "A sturdy anvil for smithing metal items.",
///         "SupportedCategories": ["Weapon", "Tool", "Material"],
///         "CraftingSkill": "smithing",
///         "Icon": "icons/stations/anvil.png"
///       }
///     ]
///   }
/// }
/// </code>
/// <para>
/// The provider uses <see cref="Stations"/> to create
/// <see cref="Domain.Definitions.CraftingStationDefinition"/> instances.
/// </para>
/// </remarks>
public class CraftingStationSettings
{
    /// <summary>
    /// Gets or sets the list of crafting station configurations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each entry defines a crafting station type with its supported categories,
    /// associated skill, and display information.
    /// </para>
    /// <para>
    /// Standard station types defined in v0.11.2a:
    /// <list type="bullet">
    ///   <item><description>anvil - Smithing station for Weapon, Tool, Material</description></item>
    ///   <item><description>workbench - Crafting bench for Armor, Accessory</description></item>
    ///   <item><description>alchemy-table - Brewing station for Potion, Consumable</description></item>
    ///   <item><description>enchanting-altar - Magic station for Weapon, Armor, Accessory</description></item>
    ///   <item><description>cooking-fire - Cooking station for Consumable</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public List<CraftingStationConfigDto> Stations { get; set; } = [];
}

/// <summary>
/// DTO for a single crafting station configuration entry.
/// </summary>
/// <remarks>
/// <para>
/// This DTO is used for JSON deserialization. The provider converts these
/// DTOs into <see cref="Domain.Definitions.CraftingStationDefinition"/> entities.
/// </para>
/// <para>
/// Property names use PascalCase for JSON binding compatibility with
/// Microsoft.Extensions.Configuration's default naming conventions.
/// </para>
/// </remarks>
public class CraftingStationConfigDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the station type.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Should use kebab-case format (lowercase with hyphens).
    /// The provider normalizes IDs to lowercase during loading.
    /// </para>
    /// </remarks>
    /// <example>"anvil", "alchemy-table", "enchanting-altar"</example>
    public string Id { get; set; } = null!;

    /// <summary>
    /// Gets or sets the display name of the station.
    /// </summary>
    /// <remarks>
    /// Shown in room descriptions, examine output, and UI elements.
    /// </remarks>
    /// <example>"Anvil", "Alchemy Table", "Enchanting Altar"</example>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the description shown when examining the station.
    /// </summary>
    /// <remarks>
    /// Provides atmospheric flavor text describing the station's appearance
    /// and purpose. Should be 1-2 sentences.
    /// </remarks>
    public string Description { get; set; } = null!;

    /// <summary>
    /// Gets or sets the list of recipe categories this station supports.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Valid values correspond to <see cref="Domain.Enums.RecipeCategory"/> enum:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>"Weapon" - Swords, axes, bows, etc.</description></item>
    ///   <item><description>"Armor" - Helmets, chestplates, shields, etc.</description></item>
    ///   <item><description>"Potion" - Health, mana, buff potions</description></item>
    ///   <item><description>"Consumable" - Food, drinks, scrolls</description></item>
    ///   <item><description>"Accessory" - Rings, amulets, cloaks</description></item>
    ///   <item><description>"Tool" - Pickaxes, hammers, etc.</description></item>
    ///   <item><description>"Material" - Refined materials for other recipes</description></item>
    /// </list>
    /// <para>
    /// Must contain at least one category. Categories are parsed case-insensitively.
    /// </para>
    /// </remarks>
    public List<string> SupportedCategories { get; set; } = [];

    /// <summary>
    /// Gets or sets the skill ID used for crafting checks at this station.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When a player crafts at this station, their modifier for this skill
    /// is added to their 2d6 roll for the crafting check.
    /// </para>
    /// <para>
    /// Standard skills:
    /// <list type="bullet">
    ///   <item><description>"smithing" - Metal working (anvil)</description></item>
    ///   <item><description>"leatherworking" - Leather/cloth work (workbench)</description></item>
    ///   <item><description>"alchemy" - Potion brewing (alchemy-table)</description></item>
    ///   <item><description>"enchanting" - Magical enhancement (enchanting-altar)</description></item>
    ///   <item><description>"cooking" - Food preparation (cooking-fire)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public string CraftingSkill { get; set; } = null!;

    /// <summary>
    /// Gets or sets the optional icon path for UI display.
    /// </summary>
    /// <remarks>
    /// Path is relative to the game's asset directory.
    /// If null or empty, a default icon may be used.
    /// </remarks>
    /// <example>"icons/stations/anvil.png"</example>
    public string? Icon { get; set; }
}
