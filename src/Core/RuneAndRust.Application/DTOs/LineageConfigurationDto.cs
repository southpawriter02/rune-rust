// ═══════════════════════════════════════════════════════════════════════════════
// LineageConfigurationDto.cs
// DTOs for deserializing lineage configuration from JSON.
// Version: 0.17.0e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Root DTO for lineages.json configuration file.
/// </summary>
/// <remarks>
/// <para>
/// This DTO represents the top-level structure of the lineages.json file.
/// It contains versioning information and the list of lineage definitions.
/// </para>
/// <para>
/// The property names match the JSON structure exactly:
/// <code>
/// {
///   "$schema": "./schemas/lineages.schema.json",
///   "version": "1.1",
///   "lineages": [ ... ]
/// }
/// </code>
/// </para>
/// </remarks>
public class LineageConfigurationDto
{
    /// <summary>
    /// Gets or sets the schema reference.
    /// </summary>
    /// <value>Path to the JSON schema file for validation.</value>
    public string? Schema { get; set; }

    /// <summary>
    /// Gets or sets the configuration version.
    /// </summary>
    /// <value>The semantic version string of this configuration format.</value>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of lineage definitions.
    /// </summary>
    /// <value>Collection of all lineage definitions in the configuration.</value>
    public List<LineageDefinitionDto> Lineages { get; set; } = new();
}

/// <summary>
/// DTO for a single lineage definition.
/// </summary>
/// <remarks>
/// Maps to a single entry in the "lineages" array of lineages.json.
/// Property names match the JSON keys exactly (camelCase).
/// </remarks>
public class LineageDefinitionDto
{
    /// <summary>
    /// Gets or sets the unique identifier for this lineage.
    /// </summary>
    /// <value>Lowercase kebab-case ID like "clan-born" or "rune-marked".</value>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the lineage type enum value as string.
    /// </summary>
    /// <value>The Lineage enum name like "ClanBorn" or "RuneMarked".</value>
    public string LineageType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name shown to players.
    /// </summary>
    /// <value>Player-friendly name like "Clan-Born" or "Rune-Marked".</value>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the lore description.
    /// </summary>
    /// <value>Multi-sentence description of the lineage's history and nature.</value>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the character creation selection text.
    /// </summary>
    /// <value>Short evocative description for the selection screen.</value>
    public string SelectionText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the attribute modifiers for this lineage.
    /// </summary>
    public AttributeModifiersDto AttributeModifiers { get; set; } = new();

    /// <summary>
    /// Gets or sets appearance notes for character visualization.
    /// </summary>
    /// <value>Description of typical physical traits for this lineage.</value>
    public string AppearanceNotes { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the social role description.
    /// </summary>
    /// <value>Description of how society perceives this lineage.</value>
    public string SocialRole { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the passive bonuses for this lineage.
    /// </summary>
    public PassiveBonusesDto PassiveBonuses { get; set; } = new();

    /// <summary>
    /// Gets or sets the unique trait for this lineage.
    /// </summary>
    public UniqueTraitDto UniqueTrait { get; set; } = new();

    /// <summary>
    /// Gets or sets the trauma baseline for this lineage.
    /// </summary>
    public TraumaBaselineDto TraumaBaseline { get; set; } = new();
}

/// <summary>
/// DTO for attribute modifiers.
/// </summary>
/// <remarks>
/// Property names match the JSON structure: "might", "finesse", etc.
/// (not "mightModifier", "finesseModifier").
/// </remarks>
public class AttributeModifiersDto
{
    /// <summary>Gets or sets the Might attribute modifier.</summary>
    public int Might { get; set; }

    /// <summary>Gets or sets the Finesse attribute modifier.</summary>
    public int Finesse { get; set; }

    /// <summary>Gets or sets the Wits attribute modifier.</summary>
    public int Wits { get; set; }

    /// <summary>Gets or sets the Will attribute modifier.</summary>
    public int Will { get; set; }

    /// <summary>Gets or sets the Sturdiness attribute modifier.</summary>
    public int Sturdiness { get; set; }

    /// <summary>Gets or sets whether this lineage has a flexible bonus.</summary>
    /// <value><c>true</c> if player can choose which attribute to boost.</value>
    public bool HasFlexibleBonus { get; set; }

    /// <summary>Gets or sets the flexible bonus amount.</summary>
    /// <value>The amount of the flexible bonus (typically 1 for Clan-Born).</value>
    public int FlexibleBonusAmount { get; set; }
}

/// <summary>
/// DTO for passive bonuses.
/// </summary>
public class PassiveBonusesDto
{
    /// <summary>Gets or sets the Max HP bonus.</summary>
    public int MaxHpBonus { get; set; }

    /// <summary>Gets or sets the Max AP bonus.</summary>
    public int MaxApBonus { get; set; }

    /// <summary>Gets or sets the Soak bonus.</summary>
    public int SoakBonus { get; set; }

    /// <summary>Gets or sets the Movement bonus.</summary>
    public int MovementBonus { get; set; }

    /// <summary>Gets or sets the list of skill bonuses.</summary>
    public List<SkillBonusDto> SkillBonuses { get; set; } = new();
}

/// <summary>
/// DTO for a skill bonus.
/// </summary>
public class SkillBonusDto
{
    /// <summary>Gets or sets the skill identifier.</summary>
    /// <value>Lowercase skill ID like "social", "lore", "craft", "survival".</value>
    public string SkillId { get; set; } = string.Empty;

    /// <summary>Gets or sets the bonus amount.</summary>
    public int BonusAmount { get; set; }
}

/// <summary>
/// DTO for unique trait.
/// </summary>
public class UniqueTraitDto
{
    /// <summary>Gets or sets the trait identifier.</summary>
    /// <value>Snake_case trait ID like "survivors_resolve".</value>
    public string TraitId { get; set; } = string.Empty;

    /// <summary>Gets or sets the trait display name.</summary>
    /// <value>Bracketed name like "[Survivor's Resolve]".</value>
    public string TraitName { get; set; } = string.Empty;

    /// <summary>Gets or sets the trait description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the effect type as string.</summary>
    /// <value>LineageTraitEffectType enum name like "BonusDiceToSkill".</value>
    public string EffectType { get; set; } = string.Empty;

    /// <summary>Gets or sets the trigger condition.</summary>
    /// <value>Condition string like "rhetoric_check_initiated".</value>
    public string? TriggerCondition { get; set; }

    /// <summary>Gets or sets the bonus dice count for dice pool traits.</summary>
    public int? BonusDice { get; set; }

    /// <summary>Gets or sets the percent modifier for percentage traits.</summary>
    /// <value>Decimal modifier like 0.10 for +10% or -0.10 for -10%.</value>
    public float? PercentModifier { get; set; }

    /// <summary>Gets or sets the target check type for conditional traits.</summary>
    /// <value>Skill or resolve check type like "rhetoric" or "sturdiness".</value>
    public string? TargetCheck { get; set; }

    /// <summary>Gets or sets the target condition for conditional traits.</summary>
    /// <value>Condition expression like "npc.lineage == ClanBorn".</value>
    public string? TargetCondition { get; set; }
}

/// <summary>
/// DTO for trauma baseline.
/// </summary>
public class TraumaBaselineDto
{
    /// <summary>Gets or sets the starting Corruption value.</summary>
    /// <value>Permanent starting Corruption (5 for Rune-Marked, 0 for others).</value>
    public int StartingCorruption { get; set; }

    /// <summary>Gets or sets the starting Stress value.</summary>
    public int StartingStress { get; set; }

    /// <summary>Gets or sets the Corruption resistance modifier.</summary>
    /// <value>Modifier to Corruption resistance checks (-1 for Rune-Marked).</value>
    public int CorruptionResistanceModifier { get; set; }

    /// <summary>Gets or sets the Stress resistance modifier.</summary>
    /// <value>Modifier to Stress resistance checks (-1 for Iron-Blooded).</value>
    public int StressResistanceModifier { get; set; }
}
