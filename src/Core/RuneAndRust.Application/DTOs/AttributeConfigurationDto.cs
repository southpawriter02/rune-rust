// ═══════════════════════════════════════════════════════════════════════════════
// AttributeConfigurationDto.cs
// DTOs for deserializing attribute configuration from JSON.
// Version: 0.17.2e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Root DTO for attributes.json configuration file.
/// </summary>
/// <remarks>
/// <para>
/// This DTO represents the top-level structure of the attributes.json file.
/// It contains attribute descriptions, archetype-recommended builds, and
/// point-buy configuration for the character creation attribute system.
/// </para>
/// <para>
/// The property names match the JSON structure exactly:
/// <code>
/// {
///   "$schema": "./schemas/attributes.schema.json",
///   "attributes": [ ... ],
///   "recommendedBuilds": [ ... ],
///   "pointBuy": { ... }
/// }
/// </code>
/// </para>
/// <para>
/// <strong>Note:</strong> The "derivedStats" section of attributes.json is
/// intentionally excluded from this DTO. Derived stat formulas are loaded
/// separately by the v0.17.2d derived stat calculation system.
/// </para>
/// </remarks>
public class AttributeConfigurationDto
{
    /// <summary>
    /// Gets or sets the schema reference.
    /// </summary>
    /// <value>Path to the JSON schema file for validation (e.g., "./schemas/attributes.schema.json").</value>
    public string? Schema { get; set; }

    /// <summary>
    /// Gets or sets the list of attribute definitions.
    /// </summary>
    /// <value>
    /// Collection of all core attribute definitions. Expected to contain exactly 5
    /// entries, one for each <see cref="Domain.Enums.CoreAttribute"/> value.
    /// </value>
    public List<AttributeDefinitionDto> Attributes { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of archetype-recommended attribute builds.
    /// </summary>
    /// <value>
    /// Collection of recommended attribute allocations for Simple mode.
    /// Expected to contain exactly 4 entries, one per archetype
    /// (Warrior, Skirmisher, Mystic, Adept).
    /// </value>
    public List<RecommendedBuildDto> RecommendedBuilds { get; set; } = new();

    /// <summary>
    /// Gets or sets the point-buy configuration for Advanced mode.
    /// </summary>
    /// <value>
    /// Configuration containing starting points, attribute value constraints,
    /// and the cost table for point-buy allocation. May be null if not present
    /// in the JSON file.
    /// </value>
    public PointBuyConfigurationDto? PointBuy { get; set; }
}

/// <summary>
/// DTO for a single attribute definition entry.
/// </summary>
/// <remarks>
/// <para>
/// Maps to a single entry in the "attributes" array of attributes.json.
/// Property names match the JSON keys exactly (camelCase).
/// </para>
/// <para>
/// Each attribute definition provides display information and relationship
/// mappings used for tooltips and UI presentation during character creation.
/// </para>
/// </remarks>
public class AttributeDefinitionDto
{
    /// <summary>
    /// Gets or sets the attribute enum value as string.
    /// </summary>
    /// <value>
    /// The CoreAttribute enum name like "Might", "Finesse", "Wits", "Will",
    /// or "Sturdiness". Must match a valid <see cref="Domain.Enums.CoreAttribute"/> value.
    /// </value>
    public string Attribute { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the uppercase display name shown to players.
    /// </summary>
    /// <value>Uppercase display name like "MIGHT" or "STURDINESS".</value>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the one-line summary for tooltips.
    /// </summary>
    /// <value>Brief description like "Physical power and raw strength".</value>
    public string ShortDescription { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full explanation for help screens.
    /// </summary>
    /// <value>
    /// Multi-sentence description of the attribute's role and effects,
    /// e.g., "Might represents your character's physical power. It affects
    /// melee damage, carrying capacity, and physical feats of strength."
    /// </value>
    public string DetailedDescription { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the derived stats influenced by this attribute.
    /// </summary>
    /// <value>
    /// List of stat names like "Melee Damage", "Carrying Capacity", "Max HP".
    /// Used for UI tooltips showing what each attribute affects.
    /// </value>
    public List<string> AffectedStats { get; set; } = new();

    /// <summary>
    /// Gets or sets the skills that use this attribute for checks.
    /// </summary>
    /// <value>
    /// List of skill names like "Combat", "Athletics", "Intimidation".
    /// Used for UI tooltips showing skill associations.
    /// </value>
    public List<string> AffectedSkills { get; set; } = new();
}

/// <summary>
/// DTO for an archetype-recommended attribute build.
/// </summary>
/// <remarks>
/// <para>
/// Maps to a single entry in the "recommendedBuilds" array of attributes.json.
/// Each build defines the pre-configured attribute allocation for Simple mode
/// when a player selects an archetype.
/// </para>
/// <para>
/// All attribute values are integers in the range [1, 10] and the sum of
/// their costs (via the point-buy table) should equal the <see cref="TotalPoints"/>.
/// </para>
/// </remarks>
public class RecommendedBuildDto
{
    /// <summary>
    /// Gets or sets the archetype identifier.
    /// </summary>
    /// <value>
    /// Lowercase archetype ID like "warrior", "skirmisher", "mystic", or "adept".
    /// Used as the lookup key for Simple mode build selection.
    /// </value>
    public string ArchetypeId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the recommended Might value.
    /// </summary>
    /// <value>Integer between 1 and 10 representing physical power allocation.</value>
    public int Might { get; set; }

    /// <summary>
    /// Gets or sets the recommended Finesse value.
    /// </summary>
    /// <value>Integer between 1 and 10 representing agility allocation.</value>
    public int Finesse { get; set; }

    /// <summary>
    /// Gets or sets the recommended Wits value.
    /// </summary>
    /// <value>Integer between 1 and 10 representing perception allocation.</value>
    public int Wits { get; set; }

    /// <summary>
    /// Gets or sets the recommended Will value.
    /// </summary>
    /// <value>Integer between 1 and 10 representing mental fortitude allocation.</value>
    public int Will { get; set; }

    /// <summary>
    /// Gets or sets the recommended Sturdiness value.
    /// </summary>
    /// <value>Integer between 1 and 10 representing endurance allocation.</value>
    public int Sturdiness { get; set; }

    /// <summary>
    /// Gets or sets the total point cost for this build.
    /// </summary>
    /// <value>
    /// Total points consumed by this allocation. Typically 15 for Warrior,
    /// Skirmisher, and Mystic, or 14 for Adept.
    /// </value>
    public int TotalPoints { get; set; }
}

/// <summary>
/// DTO for the point-buy configuration section.
/// </summary>
/// <remarks>
/// <para>
/// Maps to the "pointBuy" object in attributes.json. Defines all parameters
/// for Advanced mode attribute allocation including point pools, value
/// constraints, and the tiered cost table.
/// </para>
/// </remarks>
public class PointBuyConfigurationDto
{
    /// <summary>
    /// Gets or sets the standard starting point pool.
    /// </summary>
    /// <value>
    /// Points available for most archetypes. Default game value is 15.
    /// </value>
    public int StartingPoints { get; set; }

    /// <summary>
    /// Gets or sets the Adept archetype's starting point pool.
    /// </summary>
    /// <value>
    /// Points available for the Adept archetype. Default game value is 14.
    /// </value>
    public int AdeptStartingPoints { get; set; }

    /// <summary>
    /// Gets or sets the minimum attribute value.
    /// </summary>
    /// <value>The lowest value any attribute can have. Default is 1.</value>
    public int MinAttributeValue { get; set; } = 1;

    /// <summary>
    /// Gets or sets the maximum attribute value.
    /// </summary>
    /// <value>The highest value any attribute can reach. Default is 10.</value>
    public int MaxAttributeValue { get; set; } = 10;

    /// <summary>
    /// Gets or sets the cost table entries.
    /// </summary>
    /// <value>
    /// List of cost entries for each target value from 2 through MaxAttributeValue.
    /// Expected to contain 9 entries (values 2-10) with tiered pricing.
    /// </value>
    public List<PointBuyCostEntryDto> CostTable { get; set; } = new();
}

/// <summary>
/// DTO for a single point-buy cost table entry.
/// </summary>
/// <remarks>
/// Maps to a single entry in the "costTable" array of the point-buy configuration.
/// Each entry defines the individual and cumulative cost for reaching a specific
/// attribute value from the base.
/// </remarks>
public class PointBuyCostEntryDto
{
    /// <summary>
    /// Gets or sets the target attribute value.
    /// </summary>
    /// <value>
    /// The attribute value this cost applies to. Range is 2-10
    /// (base value 1 has no cost entry).
    /// </value>
    public int TargetValue { get; set; }

    /// <summary>
    /// Gets or sets the cost for a single increment to this value.
    /// </summary>
    /// <value>
    /// 1 for standard tier values (2-8), 2 for premium tier values (9-10).
    /// </value>
    public int IndividualCost { get; set; }

    /// <summary>
    /// Gets or sets the total cost from the base value to this value.
    /// </summary>
    /// <value>
    /// Cumulative points required to reach this value from 1.
    /// Range is 1 (for value 2) through 11 (for value 10).
    /// </value>
    public int CumulativeCost { get; set; }
}
