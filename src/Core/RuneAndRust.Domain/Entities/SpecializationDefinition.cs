// ═══════════════════════════════════════════════════════════════════════════════
// SpecializationDefinition.cs
// Entity defining a specialization with its display metadata, path type
// classification (Coherent/Heretical), parent archetype, unlock cost, and
// optional special resource. Specializations provide tactical identity and
// a unique ability tree for each character in Aethelgard.
// Version: 0.17.4b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Entities;

using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a data-driven specialization definition loaded from configuration.
/// </summary>
/// <remarks>
/// <para>
/// SpecializationDefinition is immutable after creation and represents the template
/// for a character specialization. Each specialization belongs to a single parent
/// archetype and may have unique mechanics via <see cref="SpecialResourceDefinition"/>.
/// </para>
/// <para>
/// Specializations are classified as either Coherent (stable reality, no Corruption
/// risk) or Heretical (corrupted Aether, abilities may trigger Corruption gain).
/// The <see cref="PathType"/> classification is validated against the
/// <see cref="SpecializationId"/> enum's inherent path type to prevent configuration
/// mismatches.
/// </para>
/// <para>
/// The first specialization is free during character creation; additional
/// specializations cost 3 Progression Points.
/// </para>
/// <para>
/// Key data points:
/// </para>
/// <list type="bullet">
///   <item><description>17 total specializations across 4 archetypes</description></item>
///   <item><description>5 Heretical (Berserkr, GorgeMaw, MyrkGengr, Seidkona, EchoCaller)</description></item>
///   <item><description>12 Coherent (all others)</description></item>
///   <item><description>5 have special resources (Rage, Block Charges, Righteous Fervor, Aether Resonance, Echoes)</description></item>
///   <item><description>Warrior: 6, Skirmisher: 4, Mystic: 2, Adept: 5</description></item>
/// </list>
/// <para>
/// Instances are typically loaded from configuration (specializations.json) via
/// the ISpecializationProvider (v0.17.4d).
/// </para>
/// </remarks>
/// <seealso cref="SpecializationId"/>
/// <seealso cref="SpecializationPathType"/>
/// <seealso cref="Archetype"/>
/// <seealso cref="SpecialResourceDefinition"/>
/// <seealso cref="SpecializationIdExtensions"/>
/// <seealso cref="SpecializationPathTypeExtensions"/>
public class SpecializationDefinition : IEntity
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Logger for detailed diagnostic output during entity creation.
    /// </summary>
    private static ILogger<SpecializationDefinition>? _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique database identifier for this specialization definition.
    /// </summary>
    /// <value>A GUID that uniquely identifies this definition instance.</value>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the enum identifier for this specialization.
    /// </summary>
    /// <value>
    /// The <see cref="Enums.SpecializationId"/> enum value for this definition.
    /// </value>
    /// <remarks>
    /// This property links the definition entity to the corresponding
    /// <see cref="Enums.SpecializationId"/> enum value. Each specialization
    /// has exactly one definition. Valid values range from Berserkr (0)
    /// through Einbui (16).
    /// </remarks>
    public SpecializationId SpecializationId { get; private set; }

    /// <summary>
    /// Gets the display name shown in UI.
    /// </summary>
    /// <value>
    /// A player-friendly name, potentially with Old Norse diacritics
    /// (e.g., "Berserkr", "Seiðkona", "Einbúi").
    /// </value>
    /// <example>"Berserkr", "Skjaldmaer", "Seiðkona", "Jötun-Reader"</example>
    public string DisplayName { get; private set; }

    /// <summary>
    /// Gets the short tagline summarizing the specialization.
    /// </summary>
    /// <value>An evocative phrase capturing the specialization's identity.</value>
    /// <remarks>
    /// The tagline is displayed alongside the display name in the specialization
    /// selection UI to convey the thematic identity at a glance.
    /// </remarks>
    /// <example>"Fury Unleashed", "The Living Shield", "Weaver of Aether"</example>
    public string Tagline { get; private set; }

    /// <summary>
    /// Gets the full lore description.
    /// </summary>
    /// <value>A multi-sentence description of the specialization's identity and powers.</value>
    /// <remarks>
    /// This description provides detailed information about the specialization's
    /// combat style, thematic background, and narrative flavor. It is displayed
    /// in the specialization detail panel during character creation.
    /// </remarks>
    public string Description { get; private set; }

    /// <summary>
    /// Gets the text displayed during character creation selection.
    /// </summary>
    /// <value>An evocative second-person narrative text for the selection screen.</value>
    /// <remarks>
    /// This is a shorter, more actionable description used when the player
    /// is choosing their specialization during Step 5 of character creation.
    /// It uses second-person perspective to immerse the player.
    /// </remarks>
    public string SelectionText { get; private set; }

    /// <summary>
    /// Gets the parent archetype for this specialization.
    /// </summary>
    /// <value>
    /// The <see cref="Enums.Archetype"/> enum value indicating which archetype
    /// this specialization belongs to.
    /// </value>
    /// <remarks>
    /// <para>
    /// A character must have the parent archetype to select this specialization.
    /// Each specialization belongs to exactly one archetype:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Warrior (6): Berserkr, IronBane, Skjaldmaer, SkarHorde, AtgeirWielder, GorgeMaw</description></item>
    ///   <item><description>Skirmisher (4): Veidimadr, MyrkGengr, Strandhogg, HlekkrMaster</description></item>
    ///   <item><description>Mystic (2): Seidkona, EchoCaller</description></item>
    ///   <item><description>Adept (5): BoneSetter, JotunReader, Skald, ScrapTinker, Einbui</description></item>
    /// </list>
    /// </remarks>
    public Archetype ParentArchetype { get; private set; }

    /// <summary>
    /// Gets the path type classification.
    /// </summary>
    /// <value>
    /// <see cref="SpecializationPathType.Coherent"/> for stable reality paths,
    /// or <see cref="SpecializationPathType.Heretical"/> for corrupted Aether paths.
    /// </value>
    /// <remarks>
    /// <para>Coherent: Works within stable reality, no Corruption risk from abilities.</para>
    /// <para>Heretical: Interfaces with corrupted Aether, abilities may trigger Corruption gain.</para>
    /// <para>
    /// This value is validated during creation against the inherent path type
    /// of the <see cref="SpecializationId"/> to prevent configuration mismatches.
    /// </para>
    /// </remarks>
    public SpecializationPathType PathType { get; private set; }

    /// <summary>
    /// Gets the Progression Point cost to unlock this specialization.
    /// </summary>
    /// <value>
    /// Non-negative integer. 0 for the first specialization during character
    /// creation, 3 PP for additional specializations.
    /// </value>
    /// <remarks>
    /// The first specialization is free at character creation. Players may
    /// unlock additional specializations later by spending Progression Points.
    /// </remarks>
    public int UnlockCost { get; private set; }

    /// <summary>
    /// Gets the optional special resource for this specialization.
    /// </summary>
    /// <value>
    /// A <see cref="SpecialResourceDefinition"/> if this specialization has
    /// unique resource mechanics, or <see cref="SpecialResourceDefinition.None"/>
    /// if not.
    /// </value>
    /// <remarks>
    /// Not all specializations have special resources. Check
    /// <see cref="HasSpecialResource"/> before using. Only 5 of 17
    /// specializations have unique resources.
    /// </remarks>
    /// <seealso cref="SpecialResourceDefinition"/>
    public SpecialResourceDefinition SpecialResource { get; private set; }

    /// <summary>
    /// Gets whether this specialization has a special resource.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="SpecialResource"/> has a valid resource;
    /// <c>false</c> for specializations without unique resource mechanics.
    /// </value>
    public bool HasSpecialResource => SpecialResource.HasResource;

    /// <summary>
    /// Gets whether this specialization is Heretical (risks Corruption).
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="PathType"/> is
    /// <see cref="SpecializationPathType.Heretical"/>; <c>false</c> for Coherent.
    /// </value>
    public bool IsHeretical => PathType == SpecializationPathType.Heretical;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Private constructor for EF Core materialization.
    /// </summary>
    private SpecializationDefinition()
    {
        DisplayName = null!;
        Tagline = null!;
        Description = null!;
        SelectionText = null!;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new specialization definition with validation.
    /// </summary>
    /// <param name="specializationId">The enum identifier for this specialization.</param>
    /// <param name="displayName">UI display name (e.g., "Berserkr", "Seiðkona").</param>
    /// <param name="tagline">Short summary tagline (e.g., "Fury Unleashed").</param>
    /// <param name="description">Full lore description.</param>
    /// <param name="selectionText">Character creation selection text.</param>
    /// <param name="parentArchetype">Parent archetype requirement.</param>
    /// <param name="pathType">Coherent or Heretical classification.</param>
    /// <param name="unlockCost">PP cost to unlock (0 for first spec).</param>
    /// <param name="specialResource">Optional special resource definition.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <returns>A validated <see cref="SpecializationDefinition"/>.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when any required string parameter is null or whitespace, or when
    /// <paramref name="pathType"/> does not match the inherent path type of
    /// <paramref name="specializationId"/>, or when <paramref name="parentArchetype"/>
    /// does not match the inherent parent archetype.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="unlockCost"/> is negative.
    /// </exception>
    /// <example>
    /// <code>
    /// // Create Berserkr with Rage special resource
    /// var rage = SpecialResourceDefinition.Create(
    ///     "rage", "Rage", 0, 100, 0, 0, 5,
    ///     "Fury that builds with each strike");
    ///
    /// var berserkr = SpecializationDefinition.Create(
    ///     SpecializationId.Berserkr,
    ///     "Berserkr",
    ///     "Fury Unleashed",
    ///     "The Berserkr channels primal rage into devastating combat prowess.",
    ///     "Embrace the fury. Let rage fuel your strikes.",
    ///     Archetype.Warrior,
    ///     SpecializationPathType.Heretical,
    ///     unlockCost: 0,
    ///     specialResource: rage);
    ///
    /// // Create Skald without special resource
    /// var skald = SpecializationDefinition.Create(
    ///     SpecializationId.Skald,
    ///     "Skald",
    ///     "Voice of the Saga",
    ///     "The Skald weaves words into power.",
    ///     "Sing the songs that shape the world.",
    ///     Archetype.Adept,
    ///     SpecializationPathType.Coherent,
    ///     unlockCost: 0);
    /// </code>
    /// </example>
    public static SpecializationDefinition Create(
        SpecializationId specializationId,
        string displayName,
        string tagline,
        string description,
        string selectionText,
        Archetype parentArchetype,
        SpecializationPathType pathType,
        int unlockCost,
        SpecialResourceDefinition? specialResource = null,
        ILogger<SpecializationDefinition>? logger = null)
    {
        // Store logger for this creation context
        _logger = logger;

        _logger?.LogDebug(
            "Creating SpecializationDefinition for {SpecializationId} with display name '{DisplayName}', " +
            "parent archetype {ParentArchetype}, path type {PathType}, unlock cost {UnlockCost}, " +
            "has special resource: {HasSpecialResource}",
            specializationId,
            displayName,
            parentArchetype,
            pathType,
            unlockCost,
            specialResource.HasValue);

        // Validate required string parameters
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName, nameof(displayName));
        ArgumentException.ThrowIfNullOrWhiteSpace(tagline, nameof(tagline));
        ArgumentException.ThrowIfNullOrWhiteSpace(description, nameof(description));
        ArgumentException.ThrowIfNullOrWhiteSpace(selectionText, nameof(selectionText));

        // Validate unlock cost is non-negative
        ArgumentOutOfRangeException.ThrowIfNegative(unlockCost, nameof(unlockCost));

        // Validate path type matches the specialization's inherent path
        var expectedPathType = specializationId.GetPathType();
        if (pathType != expectedPathType)
        {
            _logger?.LogWarning(
                "PathType mismatch for {SpecializationId}: expected {Expected}, got {Actual}",
                specializationId,
                expectedPathType,
                pathType);

            throw new ArgumentException(
                $"PathType mismatch: {specializationId} is {expectedPathType}, not {pathType}",
                nameof(pathType));
        }

        // Validate parent archetype matches the specialization's inherent archetype
        var expectedArchetype = specializationId.GetParentArchetype();
        if (parentArchetype != expectedArchetype)
        {
            _logger?.LogWarning(
                "ParentArchetype mismatch for {SpecializationId}: expected {Expected}, got {Actual}",
                specializationId,
                expectedArchetype,
                parentArchetype);

            throw new ArgumentException(
                $"ParentArchetype mismatch: {specializationId} belongs to {expectedArchetype}, not {parentArchetype}",
                nameof(parentArchetype));
        }

        _logger?.LogDebug(
            "Validation passed for {SpecializationId}. All 4 required string parameters are non-empty. " +
            "PathType: {PathType} (matches expected), ParentArchetype: {ParentArchetype} (matches expected), " +
            "UnlockCost: {UnlockCost}",
            specializationId,
            pathType,
            parentArchetype,
            unlockCost);

        var definition = new SpecializationDefinition
        {
            Id = Guid.NewGuid(),
            SpecializationId = specializationId,
            DisplayName = displayName.Trim(),
            Tagline = tagline.Trim(),
            Description = description.Trim(),
            SelectionText = selectionText.Trim(),
            ParentArchetype = parentArchetype,
            PathType = pathType,
            UnlockCost = unlockCost,
            SpecialResource = specialResource ?? SpecialResourceDefinition.None
        };

        _logger?.LogInformation(
            "Created SpecializationDefinition '{DisplayName}' (ID: {Id}) for {SpecializationId}. " +
            "Parent: {ParentArchetype}, PathType: {PathType}, UnlockCost: {UnlockCost}, " +
            "HasSpecialResource: {HasSpecialResource}, IsHeretical: {IsHeretical}",
            definition.DisplayName,
            definition.Id,
            definition.SpecializationId,
            definition.ParentArchetype,
            definition.PathType,
            definition.UnlockCost,
            definition.HasSpecialResource,
            definition.IsHeretical);

        return definition;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the Corruption warning for character creation UI.
    /// </summary>
    /// <returns>
    /// Warning text containing information about Corruption risk if this
    /// specialization is Heretical, or <c>null</c> for Coherent specializations.
    /// </returns>
    /// <remarks>
    /// Delegates to <see cref="SpecializationPathTypeExtensions.GetCreationWarning"/>
    /// for consistent warning text across the application. Used by the character
    /// creation UI (v0.17.5) to display Corruption risk warnings before the
    /// player confirms their specialization choice.
    /// </remarks>
    /// <example>
    /// <code>
    /// var warning = berserkrDef.GetCorruptionWarning();
    /// // Returns: "Warning: This specialization uses corrupted Aether. Some abilities may trigger Corruption gain."
    ///
    /// var noWarning = skjaldmaerDef.GetCorruptionWarning();
    /// // Returns: null
    /// </code>
    /// </example>
    public string? GetCorruptionWarning()
    {
        return PathType.GetCreationWarning();
    }

    /// <summary>
    /// Gets the full selection display for character creation UI.
    /// </summary>
    /// <returns>
    /// A formatted multi-line string containing the display name, tagline,
    /// and selection text suitable for the specialization selection panel.
    /// </returns>
    /// <example>
    /// <code>
    /// var display = berserkrDef.GetSelectionDisplay();
    /// // Returns:
    /// // Berserkr
    /// // "Fury Unleashed"
    /// //
    /// // Embrace the fury. Let rage fuel your strikes and shatter your enemies.
    /// </code>
    /// </example>
    public string GetSelectionDisplay() =>
        $"{DisplayName}\n\"{Tagline}\"\n\n{SelectionText}";

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation of this specialization definition.
    /// </summary>
    /// <returns>
    /// A formatted string containing the display name, parent archetype,
    /// and path type (e.g., "Berserkr (Warrior, Heretical)").
    /// </returns>
    public override string ToString() =>
        $"{DisplayName} ({ParentArchetype}, {PathType})";
}
