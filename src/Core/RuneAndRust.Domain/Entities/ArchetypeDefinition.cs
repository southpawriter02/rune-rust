// ═══════════════════════════════════════════════════════════════════════════════
// ArchetypeDefinition.cs
// Entity defining an archetype with its associated display metadata, combat
// role, primary resource type, and permanent selection flag. Archetypes are
// the fundamental combat identity of every character in Aethelgard.
// Version: 0.17.3a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Entities;

using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

/// <summary>
/// Represents the core definition of an archetype loaded from configuration.
/// </summary>
/// <remarks>
/// <para>
/// ArchetypeDefinition is an immutable entity that defines a character combat
/// role available during character creation. Each definition contains:
/// </para>
/// <list type="bullet">
///   <item><description>Display metadata (name, tagline, description, selection text)</description></item>
///   <item><description>Combat role descriptor for UI presentation</description></item>
///   <item><description>Primary resource type (<see cref="ResourceType.Stamina"/> or <see cref="ResourceType.AetherPool"/>)</description></item>
///   <item><description>Playstyle description for player guidance</description></item>
///   <item><description>Permanent choice flag (always true)</description></item>
/// </list>
/// <para>
/// This entity contains only the core definition properties. Resource bonuses,
/// starting abilities, and specialization mappings are defined in separate
/// value objects (v0.17.3b, v0.17.3c, v0.17.3d respectively).
/// </para>
/// <para>
/// Archetype is a PERMANENT choice—once selected during character creation,
/// it cannot be changed. The <see cref="IsPermanent"/> property is always true.
/// </para>
/// <para>
/// Instances are typically loaded from configuration (archetypes.json) via
/// the IArchetypeProvider (v0.17.3e).
/// </para>
/// </remarks>
/// <seealso cref="Archetype"/>
/// <seealso cref="ResourceType"/>
public sealed class ArchetypeDefinition : IEntity
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Logger for detailed diagnostic output during entity creation.
    /// </summary>
    private static ILogger<ArchetypeDefinition>? _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique identifier for this archetype definition.
    /// </summary>
    /// <value>A GUID that uniquely identifies this definition instance.</value>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the archetype enum value this definition represents.
    /// </summary>
    /// <value>The <see cref="Archetype"/> enum value for this definition.</value>
    /// <remarks>
    /// This property links the definition entity to the corresponding
    /// <see cref="Archetype"/> enum value. Each archetype has exactly one
    /// definition. Valid values are Warrior (0), Skirmisher (1), Mystic (2),
    /// and Adept (3).
    /// </remarks>
    public Archetype ArchetypeId { get; private set; }

    /// <summary>
    /// Gets the display name shown in UI.
    /// </summary>
    /// <value>A player-friendly name such as "Warrior" or "Mystic".</value>
    /// <example>"Warrior", "Skirmisher", "Mystic", "Adept"</example>
    public string DisplayName { get; private set; }

    /// <summary>
    /// Gets the thematic tagline for this archetype.
    /// </summary>
    /// <value>A short evocative phrase such as "The Unyielding Bulwark".</value>
    /// <remarks>
    /// The tagline is displayed alongside the display name in the archetype
    /// selection UI to convey the thematic identity of the archetype at a glance.
    /// </remarks>
    /// <example>"The Unyielding Bulwark", "Swift as Shadow", "Wielder of Tainted Aether", "Master of Mundane Arts"</example>
    public string Tagline { get; private set; }

    /// <summary>
    /// Gets the full description of the archetype.
    /// </summary>
    /// <value>A multi-sentence description of the archetype's role and identity.</value>
    /// <remarks>
    /// This description provides detailed information about the archetype's
    /// combat style, strengths, and thematic background. It is displayed
    /// in the archetype detail panel during character creation.
    /// </remarks>
    public string Description { get; private set; }

    /// <summary>
    /// Gets the flavor text shown during character creation selection.
    /// </summary>
    /// <value>An evocative second-person narrative text for the selection screen.</value>
    /// <remarks>
    /// This text is displayed when the player highlights an archetype during
    /// Step 4 of character creation. It uses second-person perspective to
    /// immerse the player in the archetype's identity.
    /// </remarks>
    public string SelectionText { get; private set; }

    /// <summary>
    /// Gets the combat role descriptor.
    /// </summary>
    /// <value>A short role label such as "Tank / Melee DPS" or "Caster / Control".</value>
    /// <remarks>
    /// The combat role provides a quick-reference label for the archetype's
    /// primary function in combat. Displayed in both the selection list and
    /// detail panels during character creation.
    /// </remarks>
    /// <example>"Tank / Melee DPS", "Mobile DPS", "Caster / Control", "Support / Utility"</example>
    public string CombatRole { get; private set; }

    /// <summary>
    /// Gets the primary resource type used by this archetype.
    /// </summary>
    /// <value>
    /// The <see cref="ResourceType"/> enum value indicating whether this archetype
    /// uses Stamina or Aether Pool as its primary resource.
    /// </value>
    /// <remarks>
    /// <para>
    /// The primary resource determines which pool abilities draw from:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Warrior: <see cref="ResourceType.Stamina"/></description></item>
    ///   <item><description>Skirmisher: <see cref="ResourceType.Stamina"/></description></item>
    ///   <item><description>Mystic: <see cref="ResourceType.AetherPool"/></description></item>
    ///   <item><description>Adept: <see cref="ResourceType.Stamina"/></description></item>
    /// </list>
    /// <para>
    /// Mystic is the only archetype using Aether Pool as its primary resource.
    /// </para>
    /// </remarks>
    /// <seealso cref="ResourceType"/>
    public ResourceType PrimaryResource { get; private set; }

    /// <summary>
    /// Gets the playstyle description for player guidance.
    /// </summary>
    /// <value>A brief summary of how the archetype plays in practice.</value>
    /// <remarks>
    /// This description helps players understand the gameplay experience
    /// of each archetype before committing to a permanent choice.
    /// </remarks>
    /// <example>"Stand in the front, absorb damage, protect allies"</example>
    public string PlaystyleDescription { get; private set; }

    /// <summary>
    /// Gets whether this archetype choice is permanent.
    /// </summary>
    /// <value>Always <c>true</c>. Archetype cannot be changed after character creation.</value>
    /// <remarks>
    /// <para>
    /// This property is always true. Archetype is a permanent choice that
    /// defines the character's combat identity for the entire saga. The
    /// character creation UI displays a permanent choice warning when
    /// the player is about to confirm their archetype selection.
    /// </para>
    /// <para>
    /// This property exists explicitly in the entity to support:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Configuration-driven permanence (loaded from JSON)</description></item>
    ///   <item><description>UI warning generation during character creation</description></item>
    ///   <item><description>Validation logic that prevents archetype changes</description></item>
    /// </list>
    /// </remarks>
    public bool IsPermanent { get; private set; }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Private constructor for EF Core materialization.
    /// </summary>
    private ArchetypeDefinition()
    {
        DisplayName = null!;
        Tagline = null!;
        Description = null!;
        SelectionText = null!;
        CombatRole = null!;
        PlaystyleDescription = null!;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new archetype definition with the specified parameters.
    /// </summary>
    /// <param name="archetypeId">The archetype enum value this definition represents.</param>
    /// <param name="displayName">The display name shown in UI.</param>
    /// <param name="tagline">The thematic tagline.</param>
    /// <param name="description">The full description of the archetype.</param>
    /// <param name="selectionText">The flavor text for character creation selection.</param>
    /// <param name="combatRole">The combat role descriptor.</param>
    /// <param name="primaryResource">The primary resource type.</param>
    /// <param name="playstyleDescription">The playstyle description for player guidance.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <returns>A new <see cref="ArchetypeDefinition"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="displayName"/>, <paramref name="tagline"/>,
    /// <paramref name="description"/>, <paramref name="selectionText"/>,
    /// <paramref name="combatRole"/>, or <paramref name="playstyleDescription"/>
    /// is null or whitespace.
    /// </exception>
    /// <example>
    /// <code>
    /// var warrior = ArchetypeDefinition.Create(
    ///     Archetype.Warrior,
    ///     "Warrior",
    ///     "The Unyielding Bulwark",
    ///     "Warriors are the frontline fighters who absorb punishment and deal devastating melee damage.",
    ///     "You are the shield between the innocent and the horror.",
    ///     "Tank / Melee DPS",
    ///     ResourceType.Stamina,
    ///     "Stand in the front, absorb damage, protect allies"
    /// );
    /// </code>
    /// </example>
    public static ArchetypeDefinition Create(
        Archetype archetypeId,
        string displayName,
        string tagline,
        string description,
        string selectionText,
        string combatRole,
        ResourceType primaryResource,
        string playstyleDescription,
        ILogger<ArchetypeDefinition>? logger = null)
    {
        // Store logger for this creation context
        _logger = logger;

        _logger?.LogDebug(
            "Creating ArchetypeDefinition for archetype {ArchetypeId} with display name '{DisplayName}', " +
            "combat role '{CombatRole}', primary resource {PrimaryResource}",
            archetypeId,
            displayName,
            combatRole,
            primaryResource);

        // Validate required string parameters
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName, nameof(displayName));
        ArgumentException.ThrowIfNullOrWhiteSpace(tagline, nameof(tagline));
        ArgumentException.ThrowIfNullOrWhiteSpace(description, nameof(description));
        ArgumentException.ThrowIfNullOrWhiteSpace(selectionText, nameof(selectionText));
        ArgumentException.ThrowIfNullOrWhiteSpace(combatRole, nameof(combatRole));
        ArgumentException.ThrowIfNullOrWhiteSpace(playstyleDescription, nameof(playstyleDescription));

        _logger?.LogDebug(
            "Validation passed for archetype {ArchetypeId}. All 6 required string parameters are non-empty. " +
            "Tagline: '{Tagline}', Primary resource: {PrimaryResource}",
            archetypeId,
            tagline,
            primaryResource);

        var definition = new ArchetypeDefinition
        {
            Id = Guid.NewGuid(),
            ArchetypeId = archetypeId,
            DisplayName = displayName.Trim(),
            Tagline = tagline.Trim(),
            Description = description.Trim(),
            SelectionText = selectionText.Trim(),
            CombatRole = combatRole.Trim(),
            PrimaryResource = primaryResource,
            PlaystyleDescription = playstyleDescription.Trim(),
            IsPermanent = true // Always true - archetype is a permanent choice
        };

        _logger?.LogInformation(
            "Created ArchetypeDefinition '{DisplayName}' (ID: {Id}) for archetype {ArchetypeId}. " +
            "Combat role: '{CombatRole}', Primary resource: {PrimaryResource}, " +
            "Tagline: '{Tagline}', IsPermanent: {IsPermanent}",
            definition.DisplayName,
            definition.Id,
            definition.ArchetypeId,
            definition.CombatRole,
            definition.PrimaryResource,
            definition.Tagline,
            definition.IsPermanent);

        return definition;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a summary string for logging and debugging.
    /// </summary>
    /// <returns>
    /// A formatted summary containing the display name, combat role, and primary resource.
    /// </returns>
    /// <example>
    /// <code>
    /// var summary = warrior.GetSummary();
    /// // Returns: "Warrior (Tank / Melee DPS) - Stamina"
    /// </code>
    /// </example>
    public string GetSummary() =>
        $"{DisplayName} ({CombatRole}) - {PrimaryResource}";

    /// <summary>
    /// Gets the full selection display for character creation UI.
    /// </summary>
    /// <returns>
    /// A formatted multi-line string containing the display name, tagline,
    /// and selection text suitable for the archetype selection panel.
    /// </returns>
    /// <example>
    /// <code>
    /// var display = warrior.GetSelectionDisplay();
    /// // Returns:
    /// // Warrior
    /// // "The Unyielding Bulwark"
    /// //
    /// // You are the shield between the innocent and the horror...
    /// </code>
    /// </example>
    public string GetSelectionDisplay() =>
        $"{DisplayName}\n\"{Tagline}\"\n\n{SelectionText}";

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation of this archetype definition.
    /// </summary>
    /// <returns>
    /// A formatted string containing the display name and archetype ID.
    /// </returns>
    public override string ToString() =>
        $"ArchetypeDefinition: {DisplayName} [{ArchetypeId}]";
}
