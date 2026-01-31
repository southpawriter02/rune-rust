// ═══════════════════════════════════════════════════════════════════════════════
// SpecializationConfigDto.cs
// DTOs for deserializing specialization configuration from JSON.
// Version: 0.17.4d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Root DTO for specializations.json configuration file.
/// </summary>
/// <remarks>
/// <para>
/// This DTO represents the top-level structure of the specializations.json file.
/// It contains the path type classifications (from v0.17.4a) and the full
/// specialization definitions with display metadata, special resources, and
/// ability tiers (from v0.17.4b-c).
/// </para>
/// <para>
/// The property names match the JSON structure exactly:
/// <code>
/// {
///   "$schema": "./schemas/specializations.schema.json",
///   "pathTypes": [ ... ],
///   "definitions": [ ... ]
/// }
/// </code>
/// </para>
/// </remarks>
/// <seealso cref="SpecializationPathTypeEntryDto"/>
/// <seealso cref="SpecializationDefinitionDto"/>
public class SpecializationsConfigDto
{
    /// <summary>
    /// Gets or sets the schema reference.
    /// </summary>
    /// <value>Path to the JSON schema file for validation (e.g., "./schemas/specializations.schema.json").</value>
    public string? Schema { get; set; }

    /// <summary>
    /// Gets or sets the path type classifications for all specializations.
    /// </summary>
    /// <value>
    /// Collection of 17 path type entries mapping each specialization to its
    /// Coherent or Heretical classification, with optional Corruption risk descriptions.
    /// </value>
    public List<SpecializationPathTypeEntryDto> PathTypes { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of specialization definitions.
    /// </summary>
    /// <value>
    /// Collection of all 17 specialization definitions, each containing
    /// display metadata, parent archetype, path type, special resource,
    /// and ability tiers. One for each <see cref="Domain.Enums.SpecializationId"/> value.
    /// </value>
    public List<SpecializationDefinitionDto> Definitions { get; set; } = new();
}

/// <summary>
/// DTO for a path type classification entry.
/// </summary>
/// <remarks>
/// <para>
/// Maps to a single entry in the "pathTypes" array of specializations.json.
/// These entries classify each specialization as Coherent (stable reality,
/// no Corruption risk) or Heretical (corrupted Aether, abilities may trigger
/// Corruption gain).
/// </para>
/// <para>
/// Path type data is also present on each definition entry. The pathTypes
/// array exists as a standalone reference from v0.17.4a and is preserved
/// for backward compatibility with the JSON schema.
/// </para>
/// </remarks>
public class SpecializationPathTypeEntryDto
{
    /// <summary>
    /// Gets or sets the specialization enum value as PascalCase string.
    /// </summary>
    /// <value>
    /// The SpecializationId enum name like "Berserkr", "Skjaldmaer", etc.
    /// Must match a valid <see cref="Domain.Enums.SpecializationId"/> enum value.
    /// </value>
    public string SpecializationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path type classification as string.
    /// </summary>
    /// <value>
    /// Either "Coherent" (12 specializations) or "Heretical" (5 specializations).
    /// Must match a valid <see cref="Domain.Enums.SpecializationPathType"/> enum value.
    /// </value>
    public string PathType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional Corruption risk description.
    /// </summary>
    /// <value>
    /// Description of how the specialization risks Corruption gain for Heretical paths
    /// (e.g., "Rage abilities may trigger Corruption gain"). Null for Coherent paths.
    /// </value>
    public string? CorruptionRisk { get; set; }
}

/// <summary>
/// DTO for a single specialization definition entry.
/// </summary>
/// <remarks>
/// <para>
/// Maps to a single entry in the "definitions" array of specializations.json.
/// Property names match the JSON keys exactly (camelCase).
/// </para>
/// <para>
/// Each specialization definition includes core display metadata (display name,
/// tagline, description, selection text), parent archetype linkage, path type
/// classification, unlock cost, optional special resource, and ability tiers.
/// </para>
/// <para>
/// The specialization choice is made during character creation Step 5 and
/// the first specialization is free. Additional specializations cost 3 PP
/// to unlock at runtime (not reflected in the configuration unlockCost).
/// </para>
/// </remarks>
/// <seealso cref="SpecialResourceDto"/>
/// <seealso cref="AbilityTierDto"/>
public class SpecializationDefinitionDto
{
    /// <summary>
    /// Gets or sets the specialization enum value as PascalCase string.
    /// </summary>
    /// <value>
    /// The SpecializationId enum name like "Berserkr", "IronBane", "Skjaldmaer", etc.
    /// Must match a valid <see cref="Domain.Enums.SpecializationId"/> enum value.
    /// </value>
    public string SpecializationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name shown to players.
    /// </summary>
    /// <value>
    /// Player-friendly name preserving Old Norse diacritics where applicable
    /// (e.g., "Berserkr", "Seiðkona", "Veiðimaðr", "Jötun-Reader").
    /// </value>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the thematic tagline for the specialization.
    /// </summary>
    /// <value>Short thematic phrase like "Fury Unleashed" or "The Living Shield".</value>
    public string Tagline { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full description of the specialization.
    /// </summary>
    /// <value>Multi-sentence description of the specialization's role, abilities, and lore.</value>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the character creation selection text.
    /// </summary>
    /// <value>
    /// Second-person evocative narrative describing the specialization experience,
    /// shown during specialization selection in the creation UI.
    /// </value>
    public string SelectionText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parent archetype as string.
    /// </summary>
    /// <value>
    /// The Archetype enum name: "Warrior", "Skirmisher", "Mystic", or "Adept".
    /// Must match a valid <see cref="Domain.Enums.Archetype"/> enum value.
    /// </value>
    public string ParentArchetype { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path type classification as string.
    /// </summary>
    /// <value>
    /// Either "Coherent" or "Heretical". Must match a valid
    /// <see cref="Domain.Enums.SpecializationPathType"/> enum value and must be
    /// consistent with the specialization's inherent path type from the enum
    /// extension methods.
    /// </value>
    public string PathType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Progression Point cost to unlock this specialization.
    /// </summary>
    /// <value>
    /// PP cost defined in configuration. Currently 0 for all definitions
    /// (the 3 PP cost for additional specializations is enforced at runtime
    /// by the application service, not in the definition).
    /// </value>
    public int UnlockCost { get; set; }

    /// <summary>
    /// Gets or sets the optional special resource for this specialization.
    /// </summary>
    /// <value>
    /// Special resource definition for 5 specializations (Berserkr: Rage,
    /// Skjaldmaer: Block Charges, IronBane: Righteous Fervor, Seidkona:
    /// Aether Resonance, EchoCaller: Echoes). Null for the remaining 12.
    /// </value>
    public SpecialResourceDto? SpecialResource { get; set; }

    /// <summary>
    /// Gets or sets the ability tiers for this specialization.
    /// </summary>
    /// <value>
    /// Array of up to 3 ability tiers. Currently only Berserkr has full
    /// ability tier data (9 abilities across 3 tiers); remaining 16
    /// specializations have empty arrays awaiting future population.
    /// </value>
    public List<AbilityTierDto>? AbilityTiers { get; set; }
}

/// <summary>
/// DTO for a specialization's special resource.
/// </summary>
/// <remarks>
/// <para>
/// Maps to the optional "specialResource" object within a specialization definition.
/// Special resources are unique per-specialization mechanics that fuel abilities
/// and provide tactical depth beyond the base Stamina/Aether Pool resources.
/// </para>
/// <para>
/// Special resources by specialization:
/// <list type="bullet">
///   <item><description>Berserkr: Rage (0-100), starts 0, decays 5/turn — powered by dealing/taking damage</description></item>
///   <item><description>Skjaldmaer: Block Charges (0-3), starts 3, regens 1/turn — consumed by defensive abilities</description></item>
///   <item><description>Iron-Bane: Righteous Fervor (0-50), starts 0 — gained from killing Blighted enemies</description></item>
///   <item><description>Seiðkona: Aether Resonance (0-10), starts 0, decays 1/turn — accumulates with spellcasting</description></item>
///   <item><description>Echo-Caller: Echoes (0-5), starts 0 — gained from fallen enemies</description></item>
/// </list>
/// </para>
/// </remarks>
public class SpecialResourceDto
{
    /// <summary>
    /// Gets or sets the resource identifier in kebab-case.
    /// </summary>
    /// <value>
    /// Lowercase kebab-case ID like "rage", "block_charges", "righteous_fervor",
    /// "aether_resonance", or "echoes". Normalized to lowercase on domain entity creation.
    /// </value>
    public string ResourceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of the resource.
    /// </summary>
    /// <value>Player-friendly name like "Rage", "Block Charges", or "Aether Resonance".</value>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the minimum resource value.
    /// </summary>
    /// <value>Floor value for the resource. Always 0 for current specializations.</value>
    public int MinValue { get; set; }

    /// <summary>
    /// Gets or sets the maximum resource value.
    /// </summary>
    /// <value>Ceiling value for the resource. Varies by specialization (3 to 100).</value>
    public int MaxValue { get; set; }

    /// <summary>
    /// Gets or sets the starting value at combat/rest initialization.
    /// </summary>
    /// <value>
    /// Initial value when combat begins or on rest. Must be within [MinValue, MaxValue].
    /// Skjaldmaer starts at max (3); all others start at 0.
    /// </value>
    public int StartsAt { get; set; }

    /// <summary>
    /// Gets or sets the passive regeneration per turn.
    /// </summary>
    /// <value>
    /// Amount regenerated each turn passively. Skjaldmaer: 1; all others: 0.
    /// </value>
    public int RegenPerTurn { get; set; }

    /// <summary>
    /// Gets or sets the passive decay per turn.
    /// </summary>
    /// <value>
    /// Amount lost each turn passively. Berserkr: 5, Seiðkona: 1; all others: 0.
    /// </value>
    public int DecayPerTurn { get; set; }

    /// <summary>
    /// Gets or sets the flavor description for the resource.
    /// </summary>
    /// <value>Human-readable description of how the resource works thematically.</value>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// DTO for a specialization ability tier.
/// </summary>
/// <remarks>
/// <para>
/// Maps to a single entry in the "abilityTiers" array within a specialization definition.
/// Each specialization has up to 3 tiers of abilities, unlocked via Progression Points
/// with escalating costs and rank requirements.
/// </para>
/// <para>
/// Tier unlock structure:
/// <list type="bullet">
///   <item><description>Tier 1: Free at selection (0 PP, Rank 1, no prerequisites)</description></item>
///   <item><description>Tier 2: 2 PP, Rank 2, requires Tier 1 completion</description></item>
///   <item><description>Tier 3: 3 PP, Rank 3, requires Tier 2 completion</description></item>
/// </list>
/// </para>
/// </remarks>
/// <seealso cref="SpecializationAbilityDto"/>
public class AbilityTierDto
{
    /// <summary>
    /// Gets or sets the tier number (1, 2, or 3).
    /// </summary>
    /// <value>Integer tier level. Tier 1 defines core identity, Tier 3 contains ultimate abilities.</value>
    public int Tier { get; set; }

    /// <summary>
    /// Gets or sets the display name for this tier.
    /// </summary>
    /// <value>Thematic tier name like "Primal Fury" (Berserkr T1) or "Avatar of Destruction" (Berserkr T3).</value>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Progression Point cost to unlock this tier.
    /// </summary>
    /// <value>PP cost: Tier 1 = 0, Tier 2 = 2, Tier 3 = 3.</value>
    public int UnlockCost { get; set; }

    /// <summary>
    /// Gets or sets whether the previous tier must be unlocked first.
    /// </summary>
    /// <value>False for Tier 1; true for Tiers 2 and 3.</value>
    public bool RequiresPreviousTier { get; set; }

    /// <summary>
    /// Gets or sets the minimum progression rank required to unlock.
    /// </summary>
    /// <value>Rank requirement: Tier 1 = 1, Tier 2 = 2, Tier 3 = 3.</value>
    public int RequiredRank { get; set; }

    /// <summary>
    /// Gets or sets the abilities contained in this tier.
    /// </summary>
    /// <value>
    /// List of 2-4 abilities. Each tier contains a mix of active and passive abilities
    /// that define the specialization's capabilities at that progression level.
    /// </value>
    public List<SpecializationAbilityDto> Abilities { get; set; } = new();
}

/// <summary>
/// DTO for a single specialization ability.
/// </summary>
/// <remarks>
/// <para>
/// Maps to a single entry in the "abilities" array within an ability tier.
/// Abilities are either active (requiring resource costs and/or cooldowns)
/// or passive (providing permanent bonuses). Heretical specialization abilities
/// may carry a Corruption risk when used.
/// </para>
/// <para>
/// Ability IDs use kebab-case format (e.g., "rage-strike", "blood-frenzy")
/// and are normalized to lowercase on domain entity creation for consistent lookups.
/// </para>
/// </remarks>
public class SpecializationAbilityDto
{
    /// <summary>
    /// Gets or sets the ability identifier in kebab-case.
    /// </summary>
    /// <value>
    /// Lowercase kebab-case ID like "rage-strike", "shield-wall", or "stalwart".
    /// Normalized to lowercase on domain entity creation.
    /// </value>
    public string AbilityId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of the ability.
    /// </summary>
    /// <value>Player-friendly name like "Rage Strike", "Shield Wall", or "Stalwart".</value>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ability effect description.
    /// </summary>
    /// <value>Brief description of the ability's combat effect for tooltips.</value>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the ability is passive.
    /// </summary>
    /// <value>True for passive abilities (permanent bonuses); false for active abilities (require activation).</value>
    public bool IsPassive { get; set; }

    /// <summary>
    /// Gets or sets the resource cost to activate the ability.
    /// </summary>
    /// <value>Amount of the specified resource type consumed on use. 0 for free or passive abilities.</value>
    public int ResourceCost { get; set; }

    /// <summary>
    /// Gets or sets the resource type consumed by this ability.
    /// </summary>
    /// <value>
    /// Resource identifier like "rage", "block_charges", "stamina", or empty string
    /// for abilities with no resource cost.
    /// </value>
    public string ResourceType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the cooldown in rounds between uses.
    /// </summary>
    /// <value>Number of rounds before the ability can be used again. 0 for no cooldown.</value>
    public int Cooldown { get; set; }

    /// <summary>
    /// Gets or sets the Corruption risk amount when using this ability.
    /// </summary>
    /// <value>
    /// Amount of Corruption potentially gained on ability use. 0 for abilities
    /// with no Corruption risk. Only Heretical specialization abilities have
    /// non-zero values.
    /// </value>
    public int CorruptionRisk { get; set; }
}
