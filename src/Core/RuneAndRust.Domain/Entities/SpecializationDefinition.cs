// ═══════════════════════════════════════════════════════════════════════════════
// SpecializationDefinition.cs
// Entity defining a specialization with its display metadata, path type
// classification (Coherent/Heretical), parent archetype, unlock cost, and
// optional special resource. Specializations provide tactical identity and
// a unique ability tree for each character in Aethelgard.
// Version: 0.17.4c
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
/// Each specialization has three tiers of abilities (v0.17.4c), accessed via
/// <see cref="AbilityTiers"/>. Tiers contain 2-4 abilities each with escalating
/// Progression Point costs (0 PP, 2 PP, 3 PP).
/// </para>
/// <para>
/// Instances are typically loaded from configuration (specializations.json) via
/// the ISpecializationProvider (v0.17.4d).
/// </para>
/// </remarks>
/// <seealso cref="SpecializationId"/>
/// <seealso cref="SpecializationPathType"/>
/// <seealso cref="Archetype"/>
/// <seealso cref="SpecialResourceDefinition"/>
/// <seealso cref="SpecializationAbilityTier"/>
/// <seealso cref="SpecializationAbility"/>
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

    /// <summary>
    /// Gets the ability tiers for this specialization.
    /// </summary>
    /// <value>
    /// A read-only list of <see cref="SpecializationAbilityTier"/> instances,
    /// typically containing 3 tiers. Empty if ability tiers have not been configured.
    /// </value>
    /// <remarks>
    /// <para>
    /// Contains 3 tiers of abilities, each with 2-4 abilities. Tiers are unlocked
    /// sequentially via Progression Points:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Tier 1: 0 PP (free on selection)</description></item>
    ///   <item><description>Tier 2: 2 PP (requires Tier 1)</description></item>
    ///   <item><description>Tier 3: 3 PP (requires Tier 2)</description></item>
    /// </list>
    /// <para>
    /// Ability IDs are guaranteed unique across all tiers within this specialization.
    /// </para>
    /// </remarks>
    /// <seealso cref="SpecializationAbilityTier"/>
    /// <seealso cref="TotalAbilityCount"/>
    /// <seealso cref="AllAbilities"/>
    public IReadOnlyList<SpecializationAbilityTier> AbilityTiers { get; private set; }

    /// <summary>
    /// Gets whether this specialization has ability tiers configured.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="AbilityTiers"/> contains at least one tier;
    /// <c>false</c> for specializations without configured ability data.
    /// </value>
    public bool HasAbilityTiers => AbilityTiers.Count > 0;

    /// <summary>
    /// Gets the total count of abilities across all tiers.
    /// </summary>
    /// <value>
    /// The sum of <see cref="SpecializationAbilityTier.AbilityCount"/> for all tiers.
    /// Returns 0 if no tiers are configured.
    /// </value>
    public int TotalAbilityCount => AbilityTiers.Sum(t => t.AbilityCount);

    /// <summary>
    /// Gets all abilities across all tiers as a flat sequence.
    /// </summary>
    /// <value>
    /// An enumerable of all <see cref="SpecializationAbility"/> instances from
    /// all tiers. Empty if no tiers are configured.
    /// </value>
    /// <remarks>
    /// Abilities are returned in tier order (Tier 1 abilities first, then Tier 2, then Tier 3).
    /// </remarks>
    public IEnumerable<SpecializationAbility> AllAbilities =>
        AbilityTiers.SelectMany(t => t.Abilities);

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
        AbilityTiers = Array.Empty<SpecializationAbilityTier>();
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
    /// <param name="abilityTiers">Optional ability tiers. Must have unique tier numbers and unique ability IDs across all tiers.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <returns>A validated <see cref="SpecializationDefinition"/>.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when any required string parameter is null or whitespace, when
    /// <paramref name="pathType"/> does not match the inherent path type of
    /// <paramref name="specializationId"/>, when <paramref name="parentArchetype"/>
    /// does not match the inherent parent archetype, when <paramref name="abilityTiers"/>
    /// contains duplicate tier numbers, or when ability IDs are duplicated across tiers.
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
        IEnumerable<SpecializationAbilityTier>? abilityTiers = null,
        ILogger<SpecializationDefinition>? logger = null)
    {
        // Store logger for this creation context
        _logger = logger;

        _logger?.LogDebug(
            "Creating SpecializationDefinition for {SpecializationId} with display name '{DisplayName}', " +
            "parent archetype {ParentArchetype}, path type {PathType}, unlock cost {UnlockCost}, " +
            "has special resource: {HasSpecialResource}, has ability tiers: {HasAbilityTiers}",
            specializationId,
            displayName,
            parentArchetype,
            pathType,
            unlockCost,
            specialResource.HasValue,
            abilityTiers is not null);

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

        // Materialize ability tiers for validation
        var tiersList = abilityTiers?.ToList() ?? new List<SpecializationAbilityTier>();

        // Validate ability tiers if provided
        if (tiersList.Count > 0)
        {
            _logger?.LogDebug(
                "Validating {TierCount} ability tiers for {SpecializationId}",
                tiersList.Count,
                specializationId);

            // Validate tier numbers are unique
            var tierNumbers = tiersList.Select(t => t.Tier).OrderBy(t => t).ToList();
            if (tierNumbers.Distinct().Count() != tierNumbers.Count)
            {
                _logger?.LogWarning(
                    "Duplicate tier numbers found for {SpecializationId}: {TierNumbers}",
                    specializationId,
                    string.Join(", ", tierNumbers));

                throw new ArgumentException(
                    "Ability tiers must have unique tier numbers",
                    nameof(abilityTiers));
            }

            // Validate no duplicate ability IDs across tiers
            var allAbilityIds = tiersList
                .SelectMany(t => t.Abilities)
                .Select(a => a.AbilityId)
                .ToList();

            var duplicateIds = allAbilityIds
                .GroupBy(id => id)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateIds.Count > 0)
            {
                _logger?.LogWarning(
                    "Duplicate ability IDs across tiers for {SpecializationId}: {DuplicateIds}",
                    specializationId,
                    string.Join(", ", duplicateIds));

                throw new ArgumentException(
                    $"Duplicate ability IDs across tiers: {string.Join(", ", duplicateIds)}",
                    nameof(abilityTiers));
            }

            _logger?.LogDebug(
                "Ability tier validation passed for {SpecializationId}. " +
                "{TierCount} tiers with {AbilityCount} total abilities, all IDs unique",
                specializationId,
                tiersList.Count,
                allAbilityIds.Count);
        }

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
            SpecialResource = specialResource ?? SpecialResourceDefinition.None,
            AbilityTiers = tiersList.AsReadOnly()
        };

        _logger?.LogInformation(
            "Created SpecializationDefinition '{DisplayName}' (ID: {Id}) for {SpecializationId}. " +
            "Parent: {ParentArchetype}, PathType: {PathType}, UnlockCost: {UnlockCost}, " +
            "HasSpecialResource: {HasSpecialResource}, IsHeretical: {IsHeretical}, " +
            "AbilityTiers: {TierCount}, TotalAbilities: {TotalAbilities}",
            definition.DisplayName,
            definition.Id,
            definition.SpecializationId,
            definition.ParentArchetype,
            definition.PathType,
            definition.UnlockCost,
            definition.HasSpecialResource,
            definition.IsHeretical,
            definition.AbilityTiers.Count,
            definition.TotalAbilityCount);

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
    /// Gets a specific tier by tier number.
    /// </summary>
    /// <param name="tierNumber">The tier number (1, 2, or 3).</param>
    /// <returns>
    /// The matching <see cref="SpecializationAbilityTier"/>, or <c>null</c> if
    /// no tier with the specified number exists.
    /// </returns>
    /// <remarks>
    /// Returns <c>null</c> for the default struct value if the tier is not found,
    /// since <see cref="SpecializationAbilityTier"/> is a value type. Callers
    /// should check the returned value's <c>Tier</c> property or use pattern matching.
    /// </remarks>
    /// <example>
    /// <code>
    /// var tier2 = berserkrDef.GetTier(2);
    /// // tier2?.DisplayName == "Unleashed Beast"
    /// </code>
    /// </example>
    public SpecializationAbilityTier? GetTier(int tierNumber)
    {
        var tier = AbilityTiers.FirstOrDefault(t => t.Tier == tierNumber);

        // FirstOrDefault on a value type returns default struct (Tier == 0) if not found
        return tier.Tier == 0 ? null : tier;
    }

    /// <summary>
    /// Gets a specific ability by its ID from any tier.
    /// </summary>
    /// <param name="abilityId">The ability ID to search for (case-insensitive).</param>
    /// <returns>
    /// The matching <see cref="SpecializationAbility"/>, or <c>null</c> if
    /// no ability with the specified ID exists in any tier.
    /// </returns>
    /// <example>
    /// <code>
    /// var ability = berserkrDef.GetAbility("rage-strike");
    /// // ability?.DisplayName == "Rage Strike"
    /// </code>
    /// </example>
    public SpecializationAbility? GetAbility(string abilityId)
    {
        foreach (var ability in AllAbilities)
        {
            if (ability.AbilityId.Equals(abilityId, StringComparison.OrdinalIgnoreCase))
            {
                return ability;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the tier number containing a specific ability.
    /// </summary>
    /// <param name="abilityId">The ability ID to search for (case-insensitive).</param>
    /// <returns>
    /// The tier number (1, 2, or 3) containing the ability, or <c>null</c>
    /// if the ability is not found in any tier.
    /// </returns>
    /// <example>
    /// <code>
    /// var tierNum = berserkrDef.GetAbilityTier("berserker-charge");
    /// // tierNum == 2
    /// </code>
    /// </example>
    public int? GetAbilityTier(string abilityId)
    {
        foreach (var tier in AbilityTiers)
        {
            if (tier.Abilities.Any(a =>
                a.AbilityId.Equals(abilityId, StringComparison.OrdinalIgnoreCase)))
            {
                return tier.Tier;
            }
        }

        return null;
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
