// ═══════════════════════════════════════════════════════════════════════════════
// ArchetypeConfigurationDto.cs
// DTOs for deserializing archetype configuration from JSON.
// Version: 0.17.3e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Root DTO for archetypes.json configuration file.
/// </summary>
/// <remarks>
/// <para>
/// This DTO represents the top-level structure of the archetypes.json file.
/// It contains the list of archetype definitions with their associated
/// resource bonuses, starting abilities, specialization mappings, and
/// optional recommended attribute builds.
/// </para>
/// <para>
/// The property names match the JSON structure exactly:
/// <code>
/// {
///   "$schema": "./schemas/archetypes.schema.json",
///   "archetypes": [ ... ]
/// }
/// </code>
/// </para>
/// </remarks>
/// <seealso cref="ArchetypeDefinitionDto"/>
public class ArchetypeConfigurationDto
{
    /// <summary>
    /// Gets or sets the schema reference.
    /// </summary>
    /// <value>Path to the JSON schema file for validation (e.g., "./schemas/archetypes.schema.json").</value>
    public string? Schema { get; set; }

    /// <summary>
    /// Gets or sets the list of archetype definitions.
    /// </summary>
    /// <value>
    /// Collection of all archetype definitions. Expected to contain exactly 4
    /// entries, one for each <see cref="Domain.Enums.Archetype"/> value:
    /// Warrior, Skirmisher, Mystic, Adept.
    /// </value>
    public List<ArchetypeDefinitionDto> Archetypes { get; set; } = new();
}

/// <summary>
/// DTO for a single archetype definition entry.
/// </summary>
/// <remarks>
/// <para>
/// Maps to a single entry in the "archetypes" array of archetypes.json.
/// Property names match the JSON keys exactly (camelCase).
/// </para>
/// <para>
/// Each archetype definition includes core metadata (display name, tagline,
/// description), combat role information, resource pool bonuses, starting
/// abilities, specialization mappings, and optional recommended builds.
/// </para>
/// <para>
/// The archetype choice is permanent and cannot be changed after character
/// creation. This is enforced by the <see cref="IsPermanent"/> field which
/// must always be <c>true</c>.
/// </para>
/// </remarks>
/// <seealso cref="ArchetypeResourceBonusesDto"/>
/// <seealso cref="ArchetypeAbilityGrantDto"/>
/// <seealso cref="ArchetypeSpecializationsDto"/>
public class ArchetypeDefinitionDto
{
    /// <summary>
    /// Gets or sets the archetype enum value as string.
    /// </summary>
    /// <value>
    /// The Archetype enum name like "Warrior", "Skirmisher", "Mystic", or "Adept".
    /// Must match a valid <see cref="Domain.Enums.Archetype"/> enum value.
    /// </value>
    public string ArchetypeId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name shown to players.
    /// </summary>
    /// <value>Player-friendly name like "Warrior" or "Mystic".</value>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the thematic tagline for the archetype.
    /// </summary>
    /// <value>Short thematic phrase like "The Unyielding Bulwark" or "Wielder of Tainted Aether".</value>
    public string Tagline { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full description of the archetype.
    /// </summary>
    /// <value>Multi-sentence description of the archetype's role and characteristics.</value>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the character creation selection text.
    /// </summary>
    /// <value>
    /// Second-person evocative narrative describing the archetype experience,
    /// shown during archetype selection in the creation UI.
    /// </value>
    public string SelectionText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the combat role descriptor.
    /// </summary>
    /// <value>Short role label like "Tank / Melee DPS", "Mobile DPS", "Caster / Control", or "Support / Utility".</value>
    public string CombatRole { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the primary resource type as string.
    /// </summary>
    /// <value>
    /// The ResourceType enum name: "Stamina" or "AetherPool".
    /// Must match a valid <see cref="Domain.Enums.ResourceType"/> enum value.
    /// </value>
    public string PrimaryResource { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the brief playstyle guidance for the player.
    /// </summary>
    /// <value>Concise gameplay summary like "Stand in the front, absorb damage, protect allies".</value>
    public string PlaystyleDescription { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the archetype choice is permanent.
    /// </summary>
    /// <value>
    /// Must always be <c>true</c>. Archetype cannot be changed after character creation.
    /// </value>
    public bool IsPermanent { get; set; }

    /// <summary>
    /// Gets or sets the resource pool bonuses for this archetype.
    /// </summary>
    /// <value>
    /// Object containing HP, Stamina, Aether Pool, Movement, and optional special bonuses.
    /// </value>
    public ArchetypeResourceBonusesDto ResourceBonuses { get; set; } = new();

    /// <summary>
    /// Gets or sets the starting abilities granted by this archetype.
    /// </summary>
    /// <value>
    /// List of exactly 3 ability grants. Each archetype provides one
    /// combination of Active, Passive, and/or Stance abilities.
    /// </value>
    public List<ArchetypeAbilityGrantDto> StartingAbilities { get; set; } = new();

    /// <summary>
    /// Gets or sets the available specializations for this archetype.
    /// </summary>
    /// <value>
    /// Object containing the specialization ID list and recommended first choice.
    /// </value>
    public ArchetypeSpecializationsDto AvailableSpecializations { get; set; } = new();

    /// <summary>
    /// Gets or sets the optional recommended attribute builds for this archetype.
    /// </summary>
    /// <value>
    /// Optional list of recommended attribute allocations for Simple mode.
    /// Each build defines optimal attribute distributions, optionally
    /// optimized for a specific lineage. May be null if not configured.
    /// </value>
    public List<ArchetypeRecommendedBuildDto>? RecommendedBuilds { get; set; }
}

/// <summary>
/// DTO for archetype resource pool bonuses.
/// </summary>
/// <remarks>
/// <para>
/// Maps to the "resourceBonuses" object within an archetype definition.
/// Each field represents a bonus added to the character's base resource
/// pool during character creation.
/// </para>
/// <para>
/// Resource bonuses by archetype:
/// <list type="bullet">
///   <item><description>Warrior: +49 HP, +5 Stamina</description></item>
///   <item><description>Skirmisher: +30 HP, +5 Stamina, +1 Movement</description></item>
///   <item><description>Mystic: +20 HP, +20 Aether Pool</description></item>
///   <item><description>Adept: +30 HP, +20% Consumable Effectiveness</description></item>
/// </list>
/// </para>
/// </remarks>
public class ArchetypeResourceBonusesDto
{
    /// <summary>
    /// Gets or sets the bonus to maximum HP.
    /// </summary>
    /// <value>HP bonus (0-100). Warrior: +49, Skirmisher: +30, Mystic: +20, Adept: +30.</value>
    public int MaxHpBonus { get; set; }

    /// <summary>
    /// Gets or sets the bonus to maximum Stamina.
    /// </summary>
    /// <value>Stamina bonus (0-50). Warrior: +5, Skirmisher: +5, Mystic: 0, Adept: 0.</value>
    public int MaxStaminaBonus { get; set; }

    /// <summary>
    /// Gets or sets the bonus to maximum Aether Pool.
    /// </summary>
    /// <value>Aether Pool bonus (0-50). Only Mystic receives +20.</value>
    public int MaxAetherPoolBonus { get; set; }

    /// <summary>
    /// Gets or sets the bonus to movement speed.
    /// </summary>
    /// <value>Movement bonus in tiles per turn (0-5). Only Skirmisher receives +1.</value>
    public int MovementBonus { get; set; }

    /// <summary>
    /// Gets or sets the optional special bonus.
    /// </summary>
    /// <value>
    /// Unique bonus for special effects (e.g., Adept's +20% ConsumableEffectiveness).
    /// Null for archetypes without special bonuses (Warrior, Skirmisher, Mystic).
    /// </value>
    public ArchetypeSpecialBonusDto? SpecialBonus { get; set; }
}

/// <summary>
/// DTO for an archetype's special bonus effect.
/// </summary>
/// <remarks>
/// Maps to the optional "specialBonus" object within resource bonuses.
/// Currently only used by the Adept archetype for ConsumableEffectiveness.
/// </remarks>
public class ArchetypeSpecialBonusDto
{
    /// <summary>
    /// Gets or sets the bonus type identifier.
    /// </summary>
    /// <value>
    /// Type string identifying the bonus mechanic. Currently only
    /// "ConsumableEffectiveness" is supported.
    /// </value>
    public string BonusType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the bonus value as a decimal fraction.
    /// </summary>
    /// <value>
    /// Percentage expressed as a decimal (e.g., 0.20 for +20%).
    /// Combined with base 1.0 to produce the multiplier.
    /// </value>
    public float BonusValue { get; set; }

    /// <summary>
    /// Gets or sets the human-readable description.
    /// </summary>
    /// <value>UI display text like "+20% effectiveness from all consumable items".</value>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// DTO for an archetype starting ability grant.
/// </summary>
/// <remarks>
/// <para>
/// Maps to a single entry in the "startingAbilities" array of an archetype definition.
/// Each archetype grants exactly 3 starting abilities during character creation.
/// </para>
/// <para>
/// Ability IDs use kebab-case format (e.g., "power-strike", "defensive-stance")
/// to reference abilities in the Ability System (v0.15.x).
/// </para>
/// </remarks>
public class ArchetypeAbilityGrantDto
{
    /// <summary>
    /// Gets or sets the ability identifier in kebab-case.
    /// </summary>
    /// <value>
    /// Lowercase kebab-case ID like "power-strike", "defensive-stance",
    /// or "iron-will". References the Ability System.
    /// </value>
    public string AbilityId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of the ability.
    /// </summary>
    /// <value>Player-friendly name like "Power Strike" or "Defensive Stance".</value>
    public string AbilityName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ability type as string.
    /// </summary>
    /// <value>
    /// The AbilityType enum name: "Active", "Passive", or "Stance".
    /// Must match a valid <see cref="Domain.Enums.AbilityType"/> enum value.
    /// </value>
    public string AbilityType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ability description for tooltips.
    /// </summary>
    /// <value>Brief description of the ability's effect.</value>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// DTO for archetype available specializations.
/// </summary>
/// <remarks>
/// <para>
/// Maps to the "availableSpecializations" object within an archetype definition.
/// Each archetype has a curated list of specializations with one recommended
/// as the player's first choice.
/// </para>
/// <para>
/// Specialization counts by archetype:
/// <list type="bullet">
///   <item><description>Warrior: 6 specializations (recommended: guardian)</description></item>
///   <item><description>Skirmisher: 4 specializations (recommended: shadow-dancer)</description></item>
///   <item><description>Mystic: 2 specializations (recommended: elementalist)</description></item>
///   <item><description>Adept: 5 specializations (recommended: alchemist)</description></item>
/// </list>
/// </para>
/// </remarks>
public class ArchetypeSpecializationsDto
{
    /// <summary>
    /// Gets or sets the list of available specialization IDs.
    /// </summary>
    /// <value>
    /// Array of kebab-case specialization IDs (e.g., "guardian", "berserker",
    /// "weapon-master"). Each ID references a specialization in the
    /// Specialization System (v0.17.4).
    /// </value>
    public List<string> Specializations { get; set; } = new();

    /// <summary>
    /// Gets or sets the recommended first specialization ID.
    /// </summary>
    /// <value>
    /// Kebab-case ID of the suggested starting specialization for new players.
    /// Must be present in the <see cref="Specializations"/> array.
    /// </value>
    public string RecommendedFirst { get; set; } = string.Empty;
}

/// <summary>
/// DTO for an archetype-specific recommended attribute build.
/// </summary>
/// <remarks>
/// <para>
/// Maps to a single entry in the optional "recommendedBuilds" array within
/// an archetype definition. Each build provides pre-configured attribute
/// values for Simple mode during character creation.
/// </para>
/// <para>
/// All attribute values are integers in the range [1, 10]. The sum of
/// attribute values equals the archetype's total attribute budget
/// (15 for most archetypes, 14 for Adept).
/// </para>
/// </remarks>
public class ArchetypeRecommendedBuildDto
{
    /// <summary>
    /// Gets or sets the display name for this build.
    /// </summary>
    /// <value>Human-readable build name like "Standard Warrior" or "Mystic ClanBorn Optimized".</value>
    public string Name { get; set; } = string.Empty;

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
    /// Gets or sets the optimal lineage for this build.
    /// </summary>
    /// <value>
    /// Optional Lineage enum name (e.g., "ClanBorn", "RuneMarked") indicating
    /// which lineage synergizes best with this build. Null for the default
    /// lineage-agnostic build.
    /// </value>
    public string? OptimalLineage { get; set; }
}
