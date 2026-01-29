// ═══════════════════════════════════════════════════════════════════════════════
// BackgroundConfigurationDto.cs
// DTOs for deserializing background configuration from JSON.
// Version: 0.17.1d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Root DTO for backgrounds.json configuration file.
/// </summary>
/// <remarks>
/// <para>
/// This DTO represents the top-level structure of the backgrounds.json file.
/// It contains the list of background definitions with their associated skill
/// and equipment grants.
/// </para>
/// <para>
/// The property names match the JSON structure exactly:
/// <code>
/// {
///   "$schema": "./schemas/backgrounds.schema.json",
///   "backgrounds": [ ... ]
/// }
/// </code>
/// </para>
/// </remarks>
public class BackgroundConfigurationDto
{
    /// <summary>
    /// Gets or sets the schema reference.
    /// </summary>
    /// <value>Path to the JSON schema file for validation.</value>
    public string? Schema { get; set; }

    /// <summary>
    /// Gets or sets the list of background definitions.
    /// </summary>
    /// <value>Collection of all background definitions in the configuration.</value>
    public List<BackgroundDefinitionDto> Backgrounds { get; set; } = new();
}

/// <summary>
/// DTO for a single background definition.
/// </summary>
/// <remarks>
/// <para>
/// Maps to a single entry in the "backgrounds" array of backgrounds.json.
/// Property names match the JSON keys exactly (camelCase).
/// </para>
/// <para>
/// Each background represents a pre-Silence profession with associated
/// skill grants, equipment grants, and narrative hooks.
/// </para>
/// </remarks>
public class BackgroundDefinitionDto
{
    /// <summary>
    /// Gets or sets the background enum value as string.
    /// </summary>
    /// <value>
    /// The Background enum name like "VillageSmith" or "TravelingHealer".
    /// Must match a valid <see cref="Domain.Enums.Background"/> enum value.
    /// </value>
    public string BackgroundId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name shown to players.
    /// </summary>
    /// <value>Player-friendly name like "Village Smith" or "Traveling Healer".</value>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the lore description.
    /// </summary>
    /// <value>Multi-sentence description of the background's history and nature.</value>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the character creation selection text.
    /// </summary>
    /// <value>
    /// Second-person evocative narrative describing the character's past,
    /// shown during background selection in the creation UI.
    /// </value>
    public string SelectionText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a brief description of the pre-Silence profession.
    /// </summary>
    /// <value>A concise profession label like "Blacksmith and metalworker".</value>
    public string ProfessionBefore { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets how society typically views characters with this background.
    /// </summary>
    /// <value>Social perception description like "Respected craftsperson, essential to any settlement".</value>
    public string SocialStanding { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the narrative hooks for quest and dialogue triggers.
    /// </summary>
    /// <value>
    /// Optional list of string descriptions for narrative event matching.
    /// Each background typically has 3 hooks covering knowledge, social, and story aspects.
    /// </value>
    public List<string>? NarrativeHooks { get; set; }

    /// <summary>
    /// Gets or sets the skill grants for this background.
    /// </summary>
    /// <value>
    /// Optional list of skill bonuses. Each background typically has a primary (+2)
    /// and secondary (+1) skill grant, both using the Permanent grant type.
    /// </value>
    public List<BackgroundSkillGrantDto>? SkillGrants { get; set; }

    /// <summary>
    /// Gets or sets the equipment grants for this background.
    /// </summary>
    /// <value>
    /// Optional list of starting equipment. Items may be auto-equipped to specific
    /// slots or placed in inventory only. Consumables may have quantity greater than 1.
    /// </value>
    public List<BackgroundEquipmentGrantDto>? EquipmentGrants { get; set; }
}

/// <summary>
/// DTO for a background skill grant configuration entry.
/// </summary>
/// <remarks>
/// Maps to a single entry in the "skillGrants" array of a background definition.
/// Each grant specifies a skill to boost and how the boost is applied.
/// </remarks>
public class BackgroundSkillGrantDto
{
    /// <summary>
    /// Gets or sets the skill identifier.
    /// </summary>
    /// <value>
    /// Lowercase skill ID like "craft", "medicine", "exploration", "combat",
    /// "performance", or "survival".
    /// </value>
    public string SkillId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the bonus amount to grant.
    /// </summary>
    /// <value>The numeric bonus value. Primary skills get +2, secondary skills get +1.</value>
    public int BonusAmount { get; set; }

    /// <summary>
    /// Gets or sets the grant type as string.
    /// </summary>
    /// <value>
    /// The SkillGrantType enum name. Standard backgrounds use "Permanent".
    /// Valid values: "Permanent", "StartingBonus", "Proficiency".
    /// </value>
    public string GrantType { get; set; } = "Permanent";
}

/// <summary>
/// DTO for a background equipment grant configuration entry.
/// </summary>
/// <remarks>
/// <para>
/// Maps to a single entry in the "equipmentGrants" array of a background definition.
/// Each grant specifies an item to give the character at creation.
/// </para>
/// <para>
/// Equipment grants support three patterns:
/// <list type="bullet">
///   <item><description>Auto-equip: IsEquipped=true, Slot specified (weapons, armor)</description></item>
///   <item><description>Inventory: IsEquipped=false, Slot=null (tools, utility items)</description></item>
///   <item><description>Consumable: IsEquipped=false, Quantity &gt; 1 (bandages, rations)</description></item>
/// </list>
/// </para>
/// </remarks>
public class BackgroundEquipmentGrantDto
{
    /// <summary>
    /// Gets or sets the item identifier.
    /// </summary>
    /// <value>
    /// Kebab-case lowercase item ID like "smiths-hammer", "healers-kit",
    /// "shield", or "travel-cloak".
    /// </value>
    public string ItemId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the quantity of items to grant.
    /// </summary>
    /// <value>
    /// Number of items to grant (defaults to 1). Consumables like bandages
    /// or rations may have higher quantities.
    /// </value>
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// Gets or sets whether the item should be auto-equipped during creation.
    /// </summary>
    /// <value>
    /// <c>true</c> if the item should be placed in an equipment slot;
    /// <c>false</c> if it should go to inventory only.
    /// </value>
    public bool IsEquipped { get; set; }

    /// <summary>
    /// Gets or sets the equipment slot for auto-equipped items.
    /// </summary>
    /// <value>
    /// The EquipmentSlot enum name (e.g., "Weapon", "Armor", "Shield") when
    /// IsEquipped is true; null when IsEquipped is false.
    /// </value>
    public string? Slot { get; set; }
}
