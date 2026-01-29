// ═══════════════════════════════════════════════════════════════════════════════
// LineageDefinition.cs
// Entity defining a lineage with its associated metadata and attribute modifiers.
// Version: 0.17.0a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Entities;

using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a lineage definition with its associated metadata and attribute modifiers.
/// </summary>
/// <remarks>
/// <para>
/// LineageDefinition is an immutable entity that defines a bloodline heritage
/// available during character creation. Each definition contains:
/// </para>
/// <list type="bullet">
///   <item><description>Display metadata (name, description, selection text)</description></item>
///   <item><description>Attribute modifiers via <see cref="LineageAttributeModifiers"/></description></item>
///   <item><description>Appearance notes for character visualization</description></item>
///   <item><description>Social role description for roleplay guidance</description></item>
/// </list>
/// <para>
/// <strong>Future Extensions (v0.17.0b+):</strong>
/// </para>
/// <list type="bullet">
///   <item><description>PassiveBonuses: HP/AP bonuses, skill bonuses</description></item>
///   <item><description>UniqueTraits: Lineage-specific abilities</description></item>
///   <item><description>TraumaBaseline: Corruption/Stress starting values</description></item>
/// </list>
/// <para>
/// Instances are typically loaded from configuration (lineages.json) via
/// the IGameConfigurationProvider.
/// </para>
/// </remarks>
/// <seealso cref="Lineage"/>
/// <seealso cref="LineageAttributeModifiers"/>
public sealed class LineageDefinition : IEntity
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Logger for detailed diagnostic output.
    /// </summary>
    private static ILogger<LineageDefinition>? _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique identifier for this lineage definition.
    /// </summary>
    /// <value>A GUID that uniquely identifies this definition instance.</value>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the lineage type this definition represents.
    /// </summary>
    /// <value>The <see cref="Lineage"/> enum value for this definition.</value>
    public Lineage LineageId { get; private set; }

    /// <summary>
    /// Gets the display name shown to players.
    /// </summary>
    /// <value>A player-friendly name such as "Clan-Born" or "Rune-Marked".</value>
    /// <example>"Clan-Born", "Rune-Marked", "Iron-Blooded", "Vargr-Kin"</example>
    public string DisplayName { get; private set; }

    /// <summary>
    /// Gets the lore description of this lineage.
    /// </summary>
    /// <value>A multi-sentence description of the lineage's history and nature.</value>
    public string Description { get; private set; }

    /// <summary>
    /// Gets the text displayed during character creation selection.
    /// </summary>
    /// <value>A shorter, evocative description for the selection screen.</value>
    /// <remarks>
    /// This text is displayed alongside the lineage name when the player
    /// is choosing their character's heritage during character creation.
    /// </remarks>
    public string SelectionText { get; private set; }

    /// <summary>
    /// Gets the attribute modifiers applied by this lineage.
    /// </summary>
    /// <value>
    /// A <see cref="LineageAttributeModifiers"/> containing all attribute
    /// bonuses and penalties for this lineage.
    /// </value>
    public LineageAttributeModifiers AttributeModifiers { get; private set; }

    /// <summary>
    /// Gets notes about typical physical appearance for this lineage.
    /// </summary>
    /// <value>
    /// A descriptive string with appearance traits, or <see cref="string.Empty"/>
    /// if no specific appearance notes apply.
    /// </value>
    /// <remarks>
    /// These notes help players and the system visualize characters of this lineage.
    /// Examples: "Often have faint glowing marks on skin" for Rune-Marked.
    /// </remarks>
    public string AppearanceNotes { get; private set; }

    /// <summary>
    /// Gets the typical social role of this lineage in Aethelgard.
    /// </summary>
    /// <value>
    /// A description of how society perceives members of this lineage,
    /// or <see cref="string.Empty"/> if not specified.
    /// </value>
    /// <remarks>
    /// Affects NPC reactions and available dialogue options.
    /// Example: "Trusted as community leaders" for Clan-Born.
    /// </remarks>
    public string SocialRole { get; private set; }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Private constructor for EF Core materialization.
    /// </summary>
    private LineageDefinition()
    {
        DisplayName = null!;
        Description = null!;
        SelectionText = null!;
        AppearanceNotes = null!;
        SocialRole = null!;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new lineage definition with the specified parameters.
    /// </summary>
    /// <param name="lineageId">The lineage type this definition represents.</param>
    /// <param name="displayName">The display name shown to players.</param>
    /// <param name="description">The lore description.</param>
    /// <param name="selectionText">The character creation selection text.</param>
    /// <param name="attributeModifiers">The attribute modifiers for this lineage.</param>
    /// <param name="appearanceNotes">Optional appearance notes.</param>
    /// <param name="socialRole">Optional social role description.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <returns>A new <see cref="LineageDefinition"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="displayName"/>, <paramref name="description"/>,
    /// or <paramref name="selectionText"/> is null or whitespace.
    /// </exception>
    /// <example>
    /// <code>
    /// var clanBorn = LineageDefinition.Create(
    ///     Lineage.ClanBorn,
    ///     "Clan-Born",
    ///     "Descendants of survivors with untainted bloodlines...",
    ///     "The Stable Code – Humanity's baseline.",
    ///     LineageAttributeModifiers.ClanBorn,
    ///     "No distinctive physical mutations.",
    ///     "Trusted as community leaders and diplomats."
    /// );
    /// </code>
    /// </example>
    public static LineageDefinition Create(
        Lineage lineageId,
        string displayName,
        string description,
        string selectionText,
        LineageAttributeModifiers attributeModifiers,
        string? appearanceNotes = null,
        string? socialRole = null,
        ILogger<LineageDefinition>? logger = null)
    {
        // Store logger for this creation context
        _logger = logger;

        _logger?.LogDebug(
            "Creating LineageDefinition for lineage {LineageId} with display name '{DisplayName}'",
            lineageId,
            displayName);

        // Validate required parameters
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName, nameof(displayName));
        ArgumentException.ThrowIfNullOrWhiteSpace(description, nameof(description));
        ArgumentException.ThrowIfNullOrWhiteSpace(selectionText, nameof(selectionText));

        _logger?.LogDebug(
            "Validation passed. Attribute modifiers: {AttributeModifiers}",
            attributeModifiers);

        var definition = new LineageDefinition
        {
            Id = Guid.NewGuid(),
            LineageId = lineageId,
            DisplayName = displayName.Trim(),
            Description = description.Trim(),
            SelectionText = selectionText.Trim(),
            AttributeModifiers = attributeModifiers,
            AppearanceNotes = appearanceNotes?.Trim() ?? string.Empty,
            SocialRole = socialRole?.Trim() ?? string.Empty
        };

        _logger?.LogInformation(
            "Created LineageDefinition '{DisplayName}' (ID: {Id}) for lineage {LineageId}. " +
            "Total fixed modifiers: {TotalFixedModifiers}, Has flexible bonus: {HasFlexibleBonus}",
            definition.DisplayName,
            definition.Id,
            definition.LineageId,
            definition.AttributeModifiers.TotalFixedModifiers,
            definition.AttributeModifiers.HasFlexibleBonus);

        return definition;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the total of all fixed modifiers for this lineage.
    /// </summary>
    /// <returns>
    /// The sum of all fixed attribute modifiers (excluding flexible bonus).
    /// </returns>
    /// <remarks>
    /// This is a convenience method that delegates to
    /// <see cref="LineageAttributeModifiers.TotalFixedModifiers"/>.
    /// </remarks>
    public int GetTotalFixedModifiers() => AttributeModifiers.TotalFixedModifiers;

    /// <summary>
    /// Determines whether this lineage requires the player to select
    /// a flexible bonus target during character creation.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the lineage has a flexible bonus that requires
    /// player selection; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Currently only Clan-Born requires flexible bonus selection.
    /// Other lineages have all modifiers pre-determined.
    /// </remarks>
    public bool RequiresFlexibleBonusSelection() => AttributeModifiers.HasFlexibleBonus;

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation of this lineage definition.
    /// </summary>
    /// <returns>
    /// A formatted string containing the display name, lineage ID, and modifier summary.
    /// </returns>
    public override string ToString() =>
        $"{DisplayName} ({LineageId}): {AttributeModifiers}";
}
