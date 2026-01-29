// ═══════════════════════════════════════════════════════════════════════════════
// LineageDefinition.cs
// Entity defining a lineage with its associated metadata, attribute modifiers,
// passive bonuses, unique traits, and Trauma Economy baseline values.
// Version: 0.17.0d
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
///   <item><description>Passive bonuses via <see cref="LineagePassiveBonuses"/> (HP, AP, Soak, Movement, Skills)</description></item>
///   <item><description>Unique trait via <see cref="LineageTrait"/> for signature abilities</description></item>
///   <item><description>Trauma baseline via <see cref="LineageTraumaBaseline"/> for Corruption/Stress starting values</description></item>
///   <item><description>Appearance notes for character visualization</description></item>
///   <item><description>Social role description for roleplay guidance</description></item>
/// </list>
/// <para>
/// Instances are typically loaded from configuration (lineages.json) via
/// the IGameConfigurationProvider.
/// </para>
/// </remarks>
/// <seealso cref="Lineage"/>
/// <seealso cref="LineageAttributeModifiers"/>
/// <seealso cref="LineagePassiveBonuses"/>
/// <seealso cref="LineageTrait"/>
/// <seealso cref="LineageTraumaBaseline"/>
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
    /// Gets the passive stat and skill bonuses granted by this lineage.
    /// </summary>
    /// <value>
    /// A <see cref="LineagePassiveBonuses"/> containing HP, AP, Soak, Movement,
    /// and skill bonuses for this lineage.
    /// </value>
    /// <remarks>
    /// Passive bonuses are applied during character creation and include:
    /// <list type="bullet">
    ///   <item><description>Stat bonuses: Max HP, Max AP, Soak, Movement</description></item>
    ///   <item><description>Skill bonuses: One or more skill modifiers</description></item>
    /// </list>
    /// Each lineage provides one stat bonus and one skill bonus. These bonuses
    /// are permanent and affect derived stat calculations throughout the game.
    /// </remarks>
    /// <seealso cref="GetHpBonus"/>
    /// <seealso cref="GetApBonus"/>
    /// <seealso cref="GetSkillBonus"/>
    public LineagePassiveBonuses PassiveBonuses { get; private set; }

    /// <summary>
    /// Gets the unique trait granted by this lineage.
    /// </summary>
    /// <value>
    /// A <see cref="LineageTrait"/> containing the signature ability unique to this bloodline.
    /// </value>
    /// <remarks>
    /// <para>
    /// Each lineage has exactly one unique trait that provides a signature ability.
    /// Traits are conditional, activating under specific circumstances unlike passive
    /// bonuses which are always active.
    /// </para>
    /// <para>
    /// The four lineage traits are:
    /// <list type="bullet">
    ///   <item><description>Clan-Born: [Survivor's Resolve] - +1d10 to Rhetoric with Clan-Born NPCs</description></item>
    ///   <item><description>Rune-Marked: [Aether-Tainted] - +10% Maximum Aether Pool</description></item>
    ///   <item><description>Iron-Blooded: [Hazard Acclimation] - +1d10 vs environmental hazards</description></item>
    ///   <item><description>Vargr-Kin: [Primal Clarity] - -10% Psychic Stress</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <seealso cref="GetTraitName"/>
    /// <seealso cref="GetTraitDescription"/>
    /// <seealso cref="HasBonusDiceTrait"/>
    /// <seealso cref="HasPercentModifierTrait"/>
    public LineageTrait UniqueTrait { get; private set; }

    /// <summary>
    /// Gets the Trauma Economy baseline values for this lineage.
    /// </summary>
    /// <value>
    /// A <see cref="LineageTraumaBaseline"/> containing starting Corruption/Stress
    /// and resistance modifiers for this lineage.
    /// </value>
    /// <remarks>
    /// <para>
    /// TraumaBaseline defines how this lineage interacts with the Trauma Economy:
    /// <list type="bullet">
    ///   <item><description>StartingCorruption: Permanent Corruption at character creation</description></item>
    ///   <item><description>StartingStress: Initial Stress at character creation</description></item>
    ///   <item><description>CorruptionResistanceModifier: Bonus/penalty to Corruption resistance checks</description></item>
    ///   <item><description>StressResistanceModifier: Bonus/penalty to Stress resistance checks</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// StartingCorruption is PERMANENT and cannot be cleansed below this value.
    /// This represents the indelible mark of the Runic Blight on certain bloodlines.
    /// </para>
    /// <para>
    /// Trauma baselines by lineage:
    /// <list type="bullet">
    ///   <item><description>Clan-Born: (0, 0, 0, 0) - Baseline humans</description></item>
    ///   <item><description>Rune-Marked: (5, 0, -1, 0) - 5 permanent Corruption, -1 Corruption resistance</description></item>
    ///   <item><description>Iron-Blooded: (0, 0, 0, -1) - -1 Stress resistance</description></item>
    ///   <item><description>Vargr-Kin: (0, 0, 0, 0) - Baseline values</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <seealso cref="GetPermanentCorruptionFloor"/>
    /// <seealso cref="HasTraumaVulnerabilities"/>
    /// <seealso cref="GetStartingCorruption"/>
    /// <seealso cref="GetCorruptionResistanceModifier"/>
    /// <seealso cref="GetStressResistanceModifier"/>
    public LineageTraumaBaseline TraumaBaseline { get; private set; }

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
    /// <param name="passiveBonuses">The passive stat and skill bonuses for this lineage.</param>
    /// <param name="uniqueTrait">The unique trait for this lineage.</param>
    /// <param name="traumaBaseline">The Trauma Economy baseline values for this lineage.</param>
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
    ///     LineagePassiveBonuses.ClanBorn,
    ///     LineageTrait.SurvivorsResolve,
    ///     LineageTraumaBaseline.ClanBorn,
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
        LineagePassiveBonuses passiveBonuses,
        LineageTrait uniqueTrait,
        LineageTraumaBaseline traumaBaseline,
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
            "Validation passed. Attribute modifiers: {AttributeModifiers}, Passive bonuses: {PassiveBonuses}, " +
            "Trait: {TraitName}, Trauma baseline: {TraumaBaseline}",
            attributeModifiers,
            passiveBonuses,
            uniqueTrait.TraitName,
            traumaBaseline);

        var definition = new LineageDefinition
        {
            Id = Guid.NewGuid(),
            LineageId = lineageId,
            DisplayName = displayName.Trim(),
            Description = description.Trim(),
            SelectionText = selectionText.Trim(),
            AttributeModifiers = attributeModifiers,
            PassiveBonuses = passiveBonuses,
            UniqueTrait = uniqueTrait,
            TraumaBaseline = traumaBaseline,
            AppearanceNotes = appearanceNotes?.Trim() ?? string.Empty,
            SocialRole = socialRole?.Trim() ?? string.Empty
        };

        _logger?.LogInformation(
            "Created LineageDefinition '{DisplayName}' (ID: {Id}) for lineage {LineageId}. " +
            "Total fixed modifiers: {TotalFixedModifiers}, Has flexible bonus: {HasFlexibleBonus}, " +
            "Passive bonuses: HP={HpBonus}, AP={ApBonus}, Soak={SoakBonus}, Move={MoveBonus}, Skills={SkillCount}, " +
            "Unique trait: {TraitName}, Trauma baseline: StartCorr={StartCorr}, CorrResist={CorrResist}, StressResist={StressResist}",
            definition.DisplayName,
            definition.Id,
            definition.LineageId,
            definition.AttributeModifiers.TotalFixedModifiers,
            definition.AttributeModifiers.HasFlexibleBonus,
            definition.PassiveBonuses.MaxHpBonus,
            definition.PassiveBonuses.MaxApBonus,
            definition.PassiveBonuses.SoakBonus,
            definition.PassiveBonuses.MovementBonus,
            definition.PassiveBonuses.SkillBonuses.Count,
            definition.UniqueTrait.TraitName,
            definition.TraumaBaseline.StartingCorruption,
            definition.TraumaBaseline.CorruptionResistanceModifier,
            definition.TraumaBaseline.StressResistanceModifier);

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

    /// <summary>
    /// Gets the Max HP bonus from this lineage's passive bonuses.
    /// </summary>
    /// <returns>The Max HP bonus amount.</returns>
    /// <remarks>
    /// This is a convenience method that delegates to
    /// <see cref="LineagePassiveBonuses.MaxHpBonus"/>.
    /// </remarks>
    public int GetHpBonus() => PassiveBonuses.MaxHpBonus;

    /// <summary>
    /// Gets the Max AP bonus from this lineage's passive bonuses.
    /// </summary>
    /// <returns>The Max AP bonus amount.</returns>
    /// <remarks>
    /// This is a convenience method that delegates to
    /// <see cref="LineagePassiveBonuses.MaxApBonus"/>.
    /// </remarks>
    public int GetApBonus() => PassiveBonuses.MaxApBonus;

    /// <summary>
    /// Gets the Soak bonus from this lineage's passive bonuses.
    /// </summary>
    /// <returns>The Soak bonus amount.</returns>
    /// <remarks>
    /// This is a convenience method that delegates to
    /// <see cref="LineagePassiveBonuses.SoakBonus"/>.
    /// </remarks>
    public int GetSoakBonus() => PassiveBonuses.SoakBonus;

    /// <summary>
    /// Gets the Movement bonus from this lineage's passive bonuses.
    /// </summary>
    /// <returns>The Movement bonus amount.</returns>
    /// <remarks>
    /// This is a convenience method that delegates to
    /// <see cref="LineagePassiveBonuses.MovementBonus"/>.
    /// </remarks>
    public int GetMovementBonus() => PassiveBonuses.MovementBonus;

    /// <summary>
    /// Gets the skill bonus for a specific skill from this lineage.
    /// </summary>
    /// <param name="skillId">The skill identifier to look up.</param>
    /// <returns>The bonus amount, or 0 if no bonus exists for the skill.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="skillId"/> is null or whitespace.
    /// </exception>
    /// <remarks>
    /// This is a convenience method that delegates to
    /// <see cref="LineagePassiveBonuses.GetSkillBonus"/>.
    /// </remarks>
    public int GetSkillBonus(string skillId) => PassiveBonuses.GetSkillBonus(skillId);

    /// <summary>
    /// Gets all skill bonuses from this lineage.
    /// </summary>
    /// <returns>A read-only list of skill bonuses.</returns>
    /// <remarks>
    /// Each lineage typically grants one skill bonus, but the design
    /// supports multiple skill bonuses for future extensibility.
    /// </remarks>
    public IReadOnlyList<SkillBonus> GetAllSkillBonuses() => PassiveBonuses.SkillBonuses;

    // ═══════════════════════════════════════════════════════════════════════════
    // TRAIT HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the display name of the unique trait.
    /// </summary>
    /// <returns>The trait name with brackets (e.g., "[Survivor's Resolve]").</returns>
    public string GetTraitName() => UniqueTrait.TraitName;

    /// <summary>
    /// Gets the description of the unique trait.
    /// </summary>
    /// <returns>The player-facing trait description.</returns>
    public string GetTraitDescription() => UniqueTrait.Description;

    /// <summary>
    /// Gets whether the unique trait uses bonus dice.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the trait adds dice to skill or resolve checks;
    /// otherwise, <c>false</c>.
    /// </returns>
    public bool HasBonusDiceTrait() => UniqueTrait.UsesBonusDice;

    /// <summary>
    /// Gets whether the unique trait uses percentage modification.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the trait scales values by percentage;
    /// otherwise, <c>false</c>.
    /// </returns>
    public bool HasPercentModifierTrait() => UniqueTrait.UsesPercentModifier;

    /// <summary>
    /// Gets the unique trait for this lineage.
    /// </summary>
    /// <returns>The <see cref="LineageTrait"/> for this lineage.</returns>
    /// <remarks>
    /// This is a convenience method that returns the same value as
    /// <see cref="UniqueTrait"/> property.
    /// </remarks>
    public LineageTrait GetTrait() => UniqueTrait;

    // ═══════════════════════════════════════════════════════════════════════════
    // TRAUMA BASELINE HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the permanent Corruption floor for this lineage.
    /// </summary>
    /// <returns>The minimum Corruption value after cleansing.</returns>
    /// <remarks>
    /// This is a convenience method that delegates to
    /// <see cref="LineageTraumaBaseline.PermanentCorruptionFloor"/>.
    /// For Rune-Marked, this is 5. For all other lineages, this is 0.
    /// </remarks>
    public int GetPermanentCorruptionFloor() =>
        TraumaBaseline.PermanentCorruptionFloor;

    /// <summary>
    /// Gets whether this lineage has any trauma vulnerabilities.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the lineage has Corruption or Stress resistance penalties;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Rune-Marked has Corruption vulnerability (-1 resistance).
    /// Iron-Blooded has Stress vulnerability (-1 resistance).
    /// </remarks>
    public bool HasTraumaVulnerabilities() =>
        TraumaBaseline.HasAnyVulnerability;

    /// <summary>
    /// Gets the starting Corruption value for new characters of this lineage.
    /// </summary>
    /// <returns>Starting Corruption from lineage.</returns>
    /// <remarks>
    /// This is a convenience method that delegates to
    /// <see cref="LineageTraumaBaseline.StartingCorruption"/>.
    /// Only Rune-Marked has starting Corruption (5).
    /// </remarks>
    public int GetStartingCorruption() =>
        TraumaBaseline.StartingCorruption;

    /// <summary>
    /// Gets the Corruption resistance modifier for this lineage.
    /// </summary>
    /// <returns>Modifier to add to Corruption resistance checks.</returns>
    /// <remarks>
    /// This is a convenience method that delegates to
    /// <see cref="LineageTraumaBaseline.CorruptionResistanceModifier"/>.
    /// Rune-Marked has -1, making them more susceptible to Corruption.
    /// </remarks>
    public int GetCorruptionResistanceModifier() =>
        TraumaBaseline.CorruptionResistanceModifier;

    /// <summary>
    /// Gets the Stress resistance modifier for this lineage.
    /// </summary>
    /// <returns>Modifier to add to Stress resistance checks.</returns>
    /// <remarks>
    /// This is a convenience method that delegates to
    /// <see cref="LineageTraumaBaseline.StressResistanceModifier"/>.
    /// Iron-Blooded has -1, making them more susceptible to Stress.
    /// </remarks>
    public int GetStressResistanceModifier() =>
        TraumaBaseline.StressResistanceModifier;

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
        $"{DisplayName} ({LineageId}): {AttributeModifiers}, {PassiveBonuses}, Trait: {UniqueTrait.TraitName}, Trauma: {TraumaBaseline}";
}
