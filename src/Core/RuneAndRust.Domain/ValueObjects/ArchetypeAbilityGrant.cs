// ═══════════════════════════════════════════════════════════════════════════════
// ArchetypeAbilityGrant.cs
// Value object representing a starting ability granted by an archetype during
// character creation. Each archetype provides exactly 3 starting abilities
// that define the character's initial combat capabilities. Abilities are
// classified by AbilityType (Active, Passive, Stance) and reference the
// Ability System (v0.15.x) via string AbilityId.
// Version: 0.17.3c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents a starting ability granted by an archetype during character creation.
/// </summary>
/// <remarks>
/// <para>
/// ArchetypeAbilityGrant is an immutable value object that defines which ability
/// a character receives when selecting an archetype. The <see cref="AbilityId"/>
/// references an ability in the Ability System (v0.15.x) using kebab-case
/// identifiers (e.g., "power-strike", "aether-bolt").
/// </para>
/// <para>
/// Each archetype grants exactly 3 abilities, typically distributed as:
/// </para>
/// <list type="bullet">
///   <item><description>1-2 Active abilities for combat actions</description></item>
///   <item><description>1 Passive ability for constant benefits</description></item>
///   <item><description>0-1 Stance ability for tactical options (Warrior only)</description></item>
/// </list>
/// <para>
/// Starting abilities by archetype:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <b>Warrior:</b> Power Strike (Active), Defensive Stance (Stance), Iron Will (Passive)
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>Skirmisher:</b> Quick Strike (Active), Evasive Maneuvers (Active), Opportunist (Passive)
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>Mystic:</b> Aether Bolt (Active), Aether Shield (Active), Aether Sense (Passive)
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>Adept:</b> Precise Strike (Active), Assess Weakness (Active), Resourceful (Passive)
///     </description>
///   </item>
/// </list>
/// <para>
/// Use the <see cref="Create"/> factory method for validated construction from
/// arbitrary data (e.g., configuration loading). Convenience factory methods
/// <see cref="CreateActive"/>, <see cref="CreatePassive"/>, and
/// <see cref="CreateStance"/> provide type-safe shortcuts for common patterns.
/// </para>
/// <para>
/// The <see cref="AbilityId"/> is normalized to lowercase during creation to
/// ensure consistent lookups against the Ability System. Ability IDs follow
/// kebab-case convention (e.g., "power-strike", "defensive-stance").
/// </para>
/// </remarks>
/// <param name="AbilityId">
/// Unique identifier for the ability in kebab-case format (e.g., "power-strike").
/// Normalized to lowercase during creation. Used to reference the full ability
/// definition in the Ability System (v0.15.x).
/// </param>
/// <param name="AbilityName">
/// Display name for the ability shown in UI (e.g., "Power Strike").
/// Used in character creation screens, ability lists, and action bars.
/// </param>
/// <param name="AbilityType">
/// Classification of the ability determining activation behavior.
/// See <see cref="Enums.AbilityType"/> for Active, Passive, and Stance types.
/// </param>
/// <param name="Description">
/// Brief description of the ability's effect for display in tooltips and
/// ability panels (e.g., "A powerful melee attack that deals bonus damage.").
/// </param>
/// <seealso cref="AbilityType"/>
/// <seealso cref="RuneAndRust.Domain.Enums.Archetype"/>
/// <seealso cref="RuneAndRust.Domain.Entities.ArchetypeDefinition"/>
/// <seealso cref="ArchetypeResourceBonuses"/>
public readonly record struct ArchetypeAbilityGrant(
    string AbilityId,
    string AbilityName,
    AbilityType AbilityType,
    string Description)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Logger for detailed diagnostic output during ability grant creation
    /// and property access.
    /// </summary>
    private static ILogger<ArchetypeAbilityGrant>? _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this is a passive ability.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="AbilityType"/> is <see cref="Enums.AbilityType.Passive"/>;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Passive abilities are always active and require no player activation.
    /// Each archetype has exactly one passive starting ability:
    /// Warrior (Iron Will), Skirmisher (Opportunist), Mystic (Aether Sense),
    /// Adept (Resourceful).
    /// </remarks>
    public bool IsPassive => AbilityType == AbilityType.Passive;

    /// <summary>
    /// Gets whether this is an active ability.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="AbilityType"/> is <see cref="Enums.AbilityType.Active"/>;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Active abilities require player activation and typically cost resources.
    /// Most archetypes have 1-2 active starting abilities.
    /// </remarks>
    public bool IsActive => AbilityType == AbilityType.Active;

    /// <summary>
    /// Gets whether this is a stance ability.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="AbilityType"/> is <see cref="Enums.AbilityType.Stance"/>;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Stance abilities are toggleable modes that modify character behavior.
    /// Among starting abilities, only the Warrior receives a stance
    /// (Defensive Stance).
    /// </remarks>
    public bool IsStance => AbilityType == AbilityType.Stance;

    /// <summary>
    /// Gets whether this ability requires activation (Active or Stance).
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="AbilityType"/> is not
    /// <see cref="Enums.AbilityType.Passive"/>; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Both Active and Stance abilities require player interaction to use.
    /// Passive abilities are always on and do not require activation.
    /// This property is useful for UI logic that determines which abilities
    /// to display on the action bar vs. the passive abilities panel.
    /// </remarks>
    public bool RequiresActivation => AbilityType != AbilityType.Passive;

    /// <summary>
    /// Gets the ability type as a formatted display string for UI.
    /// </summary>
    /// <value>
    /// A bracketed uppercase tag: "[ACTIVE]", "[PASSIVE]", or "[STANCE]".
    /// Returns "[UNKNOWN]" for undefined AbilityType values.
    /// </value>
    /// <remarks>
    /// Used in character creation UI and ability lists to clearly label
    /// the activation type of each ability. The format matches the
    /// presentation convention used in ability tooltips and selection screens.
    /// </remarks>
    /// <example>
    /// <code>
    /// var grant = ArchetypeAbilityGrant.CreateActive(
    ///     "power-strike", "Power Strike", "A powerful melee attack.");
    /// // grant.TypeDisplay == "[ACTIVE]"
    /// </code>
    /// </example>
    public string TypeDisplay => AbilityType switch
    {
        AbilityType.Active => "[ACTIVE]",
        AbilityType.Passive => "[PASSIVE]",
        AbilityType.Stance => "[STANCE]",
        _ => "[UNKNOWN]"
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates an ability grant with validation and AbilityId normalization.
    /// </summary>
    /// <param name="abilityId">
    /// The ability identifier in kebab-case format (e.g., "power-strike").
    /// Will be normalized to lowercase. Must not be null or whitespace.
    /// </param>
    /// <param name="abilityName">
    /// The display name for the ability (e.g., "Power Strike").
    /// Must not be null or whitespace.
    /// </param>
    /// <param name="abilityType">
    /// The classification of the ability (Active, Passive, or Stance).
    /// </param>
    /// <param name="description">
    /// Brief description of the ability's effect. Must not be null or whitespace.
    /// </param>
    /// <param name="logger">Optional logger for diagnostic output during creation.</param>
    /// <returns>
    /// A new <see cref="ArchetypeAbilityGrant"/> instance with validated data
    /// and a normalized (lowercase) AbilityId.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="abilityId"/>, <paramref name="abilityName"/>,
    /// or <paramref name="description"/> is null, empty, or whitespace.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This factory method validates all string parameters and normalizes
    /// the <paramref name="abilityId"/> to lowercase for consistent lookups
    /// against the Ability System (v0.15.x).
    /// </para>
    /// <para>
    /// For type-specific construction, consider using the convenience methods:
    /// <see cref="CreateActive"/>, <see cref="CreatePassive"/>, or
    /// <see cref="CreateStance"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create a Warrior's Power Strike ability grant
    /// var grant = ArchetypeAbilityGrant.Create(
    ///     "power-strike",
    ///     "Power Strike",
    ///     AbilityType.Active,
    ///     "A powerful melee attack that deals bonus damage.");
    /// // grant.AbilityId == "power-strike"
    /// // grant.IsActive == true
    /// // grant.RequiresActivation == true
    /// </code>
    /// </example>
    public static ArchetypeAbilityGrant Create(
        string abilityId,
        string abilityName,
        AbilityType abilityType,
        string description,
        ILogger<ArchetypeAbilityGrant>? logger = null)
    {
        _logger = logger;

        _logger?.LogDebug(
            "Creating ArchetypeAbilityGrant with AbilityId='{AbilityId}', " +
            "AbilityName='{AbilityName}', AbilityType={AbilityType}, " +
            "Description='{Description}'",
            abilityId,
            abilityName,
            abilityType,
            description);

        // Validate all string parameters are not null or whitespace
        ArgumentException.ThrowIfNullOrWhiteSpace(abilityId, nameof(abilityId));
        ArgumentException.ThrowIfNullOrWhiteSpace(abilityName, nameof(abilityName));
        ArgumentException.ThrowIfNullOrWhiteSpace(description, nameof(description));

        _logger?.LogDebug(
            "Validation passed for ArchetypeAbilityGrant. AbilityId='{AbilityId}', " +
            "AbilityType={AbilityType}. All required string parameters are non-empty.",
            abilityId,
            abilityType);

        // Normalize ability ID to lowercase for consistent lookups
        var normalizedId = abilityId.ToLowerInvariant();

        var grant = new ArchetypeAbilityGrant(
            normalizedId,
            abilityName,
            abilityType,
            description);

        _logger?.LogInformation(
            "Created ArchetypeAbilityGrant. AbilityId='{AbilityId}', " +
            "AbilityName='{AbilityName}', AbilityType={AbilityType}, " +
            "IsPassive={IsPassive}, RequiresActivation={RequiresActivation}, " +
            "Description='{Description}'",
            grant.AbilityId,
            grant.AbilityName,
            grant.AbilityType,
            grant.IsPassive,
            grant.RequiresActivation,
            grant.Description);

        return grant;
    }

    /// <summary>
    /// Creates an active ability grant with validation.
    /// </summary>
    /// <param name="abilityId">
    /// The ability identifier in kebab-case format. Will be normalized to lowercase.
    /// </param>
    /// <param name="abilityName">The display name for the ability.</param>
    /// <param name="description">Brief description of the ability's effect.</param>
    /// <returns>
    /// A new <see cref="ArchetypeAbilityGrant"/> with
    /// <see cref="AbilityType"/> set to <see cref="Enums.AbilityType.Active"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when any string parameter is null, empty, or whitespace.
    /// </exception>
    /// <remarks>
    /// Convenience factory for creating Active ability grants. Delegates to
    /// <see cref="Create"/> with <see cref="Enums.AbilityType.Active"/>.
    /// </remarks>
    /// <example>
    /// <code>
    /// var grant = ArchetypeAbilityGrant.CreateActive(
    ///     "power-strike",
    ///     "Power Strike",
    ///     "A powerful melee attack that deals bonus damage.");
    /// // grant.IsActive == true
    /// // grant.RequiresActivation == true
    /// </code>
    /// </example>
    public static ArchetypeAbilityGrant CreateActive(
        string abilityId,
        string abilityName,
        string description) =>
        Create(abilityId, abilityName, AbilityType.Active, description);

    /// <summary>
    /// Creates a passive ability grant with validation.
    /// </summary>
    /// <param name="abilityId">
    /// The ability identifier in kebab-case format. Will be normalized to lowercase.
    /// </param>
    /// <param name="abilityName">The display name for the ability.</param>
    /// <param name="description">Brief description of the ability's effect.</param>
    /// <returns>
    /// A new <see cref="ArchetypeAbilityGrant"/> with
    /// <see cref="AbilityType"/> set to <see cref="Enums.AbilityType.Passive"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when any string parameter is null, empty, or whitespace.
    /// </exception>
    /// <remarks>
    /// Convenience factory for creating Passive ability grants. Delegates to
    /// <see cref="Create"/> with <see cref="Enums.AbilityType.Passive"/>.
    /// </remarks>
    /// <example>
    /// <code>
    /// var grant = ArchetypeAbilityGrant.CreatePassive(
    ///     "iron-will",
    ///     "Iron Will",
    ///     "Resistance to fear and mental effects.");
    /// // grant.IsPassive == true
    /// // grant.RequiresActivation == false
    /// </code>
    /// </example>
    public static ArchetypeAbilityGrant CreatePassive(
        string abilityId,
        string abilityName,
        string description) =>
        Create(abilityId, abilityName, AbilityType.Passive, description);

    /// <summary>
    /// Creates a stance ability grant with validation.
    /// </summary>
    /// <param name="abilityId">
    /// The ability identifier in kebab-case format. Will be normalized to lowercase.
    /// </param>
    /// <param name="abilityName">The display name for the ability.</param>
    /// <param name="description">Brief description of the ability's effect.</param>
    /// <returns>
    /// A new <see cref="ArchetypeAbilityGrant"/> with
    /// <see cref="AbilityType"/> set to <see cref="Enums.AbilityType.Stance"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when any string parameter is null, empty, or whitespace.
    /// </exception>
    /// <remarks>
    /// Convenience factory for creating Stance ability grants. Delegates to
    /// <see cref="Create"/> with <see cref="Enums.AbilityType.Stance"/>.
    /// Among starting abilities, only the Warrior archetype has a stance
    /// ability (Defensive Stance).
    /// </remarks>
    /// <example>
    /// <code>
    /// var grant = ArchetypeAbilityGrant.CreateStance(
    ///     "defensive-stance",
    ///     "Defensive Stance",
    ///     "Reduces damage taken but slows movement.");
    /// // grant.IsStance == true
    /// // grant.RequiresActivation == true
    /// </code>
    /// </example>
    public static ArchetypeAbilityGrant CreateStance(
        string abilityId,
        string abilityName,
        string description) =>
        Create(abilityId, abilityName, AbilityType.Stance, description);

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a formatted display string for UI presentation.
    /// </summary>
    /// <returns>
    /// A multi-line string containing the ability name with type tag on the
    /// first line and the description on the second line.
    /// Format: "{AbilityName} {TypeDisplay}\n{Description}"
    /// </returns>
    /// <remarks>
    /// This format is designed for ability detail panels and selection screens
    /// in the character creation UI. The type tag provides at-a-glance
    /// classification while the description explains the ability's effect.
    /// </remarks>
    /// <example>
    /// <code>
    /// var grant = ArchetypeAbilityGrant.CreateActive(
    ///     "power-strike",
    ///     "Power Strike",
    ///     "A powerful melee attack that deals bonus damage.");
    /// var display = grant.GetDisplayString();
    /// // "Power Strike [ACTIVE]\nA powerful melee attack that deals bonus damage."
    /// </code>
    /// </example>
    public string GetDisplayString() =>
        $"{AbilityName} {TypeDisplay}\n{Description}";

    /// <summary>
    /// Gets a short display string for compact UI elements.
    /// </summary>
    /// <returns>
    /// A single-line string containing the ability name and type tag.
    /// Format: "{AbilityName} {TypeDisplay}"
    /// </returns>
    /// <remarks>
    /// This compact format is designed for lists, tooltips, and summary
    /// panels where space is limited. For full details including the
    /// description, use <see cref="GetDisplayString"/>.
    /// </remarks>
    /// <example>
    /// <code>
    /// var grant = ArchetypeAbilityGrant.CreateActive(
    ///     "power-strike",
    ///     "Power Strike",
    ///     "A powerful melee attack that deals bonus damage.");
    /// var short = grant.GetShortDisplay();
    /// // "Power Strike [ACTIVE]"
    /// </code>
    /// </example>
    public string GetShortDisplay() =>
        $"{AbilityName} {TypeDisplay}";

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a formatted string representation of this ability grant.
    /// </summary>
    /// <returns>
    /// A string in the format "{AbilityId}: {AbilityName} ({AbilityType})"
    /// (e.g., "power-strike: Power Strike (Active)").
    /// </returns>
    public override string ToString() =>
        $"{AbilityId}: {AbilityName} ({AbilityType})";
}
