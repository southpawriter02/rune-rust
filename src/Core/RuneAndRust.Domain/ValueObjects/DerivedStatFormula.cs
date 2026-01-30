// ═══════════════════════════════════════════════════════════════════════════════
// DerivedStatFormula.cs
// Value object defining the formula for calculating a single derived stat from
// core attributes, archetype bonuses, lineage bonuses, and lineage multipliers.
// Each formula encapsulates the complete calculation logic for one stat.
// Version: 0.17.2d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the formula for calculating a single derived stat from attributes and bonuses.
/// </summary>
/// <remarks>
/// <para>
/// DerivedStatFormula is a value object that encapsulates the complete calculation
/// logic for one derived stat. Each formula consists of:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       A <b>base value</b> added to all calculations (e.g., 50 for Max HP,
///       20 for Max Stamina, 5 for Movement Speed).
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>Attribute scaling multipliers</b> that define how core attributes
///       contribute to the stat (e.g., Sturdiness × 10 for HP).
///     </description>
///   </item>
///   <item>
///     <description>
///       Optional <b>archetype bonuses</b> that add flat values based on the
///       character's archetype (e.g., Warrior: +49 HP).
///     </description>
///   </item>
///   <item>
///     <description>
///       Optional <b>lineage bonuses</b> that add flat values based on the
///       character's lineage (e.g., Clan-Born: +5 HP, Iron-Blooded: +2 Soak).
///     </description>
///   </item>
///   <item>
///     <description>
///       Optional <b>lineage multipliers</b> applied after all additive bonuses
///       (e.g., Rune-Marked: ×1.10 to Aether Pool via Aether-Tainted trait).
///     </description>
///   </item>
/// </list>
/// <para>
/// The calculation order is: BaseValue + AttributeScaling + ArchetypeBonus +
/// LineageBonus, then LineageMultiplier, then truncate to integer.
/// </para>
/// <para>
/// Formulas are typically loaded from the <c>derivedStats</c> section of
/// <c>attributes.json</c> and used by the derived stat calculator service
/// (v0.17.2g) to compute character stats during creation and preview.
/// </para>
/// </remarks>
/// <seealso cref="DerivedStats"/>
/// <seealso cref="CoreAttribute"/>
public readonly record struct DerivedStatFormula
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Logger for detailed diagnostic output during formula creation and calculation.
    /// </summary>
    private static ILogger<DerivedStatFormula>? _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the stat name this formula calculates (e.g., "MaxHp", "MaxStamina").
    /// </summary>
    /// <value>
    /// A non-null, non-whitespace string identifying the derived stat.
    /// Used for logging, display, and configuration lookup.
    /// </value>
    public string StatName { get; init; }

    /// <summary>
    /// Gets the base value added before any scaling or bonuses.
    /// </summary>
    /// <value>
    /// The flat base value for the stat. For example, Max HP has a base of 50,
    /// Max Stamina has a base of 20, and Movement Speed has a base of 5.
    /// </value>
    public int BaseValue { get; init; }

    /// <summary>
    /// Gets the attribute scaling multipliers.
    /// </summary>
    /// <value>
    /// A dictionary mapping <see cref="CoreAttribute"/> values to their scaling
    /// factors. For example, Max HP maps Sturdiness → 10.0, meaning each point
    /// of Sturdiness adds 10 HP. Float values support fractional scaling
    /// (e.g., Wits × 0.5 for Initiative).
    /// </value>
    public IReadOnlyDictionary<CoreAttribute, float> AttributeScaling { get; init; }

    /// <summary>
    /// Gets the archetype-specific flat bonuses.
    /// </summary>
    /// <value>
    /// A dictionary mapping lowercase archetype identifiers to their flat bonus
    /// values. For example, "warrior" → 49 for Max HP, "mystic" → 20 for
    /// Max Aether Pool.
    /// </value>
    public IReadOnlyDictionary<string, int> ArchetypeBonuses { get; init; }

    /// <summary>
    /// Gets the lineage-specific flat bonuses.
    /// </summary>
    /// <value>
    /// A dictionary mapping lowercase lineage identifiers to their flat bonus
    /// values. For example, "clan-born" → 5 for Max HP, "iron-blooded" → 2
    /// for Soak.
    /// </value>
    public IReadOnlyDictionary<string, int> LineageBonuses { get; init; }

    /// <summary>
    /// Gets the lineage-specific multipliers applied after all additive bonuses.
    /// </summary>
    /// <value>
    /// A dictionary mapping lowercase lineage identifiers to their multiplicative
    /// factors. For example, "rune-marked" → 1.10 for Max Aether Pool, representing
    /// the Aether-Tainted trait's 10% bonus.
    /// </value>
    /// <remarks>
    /// Multipliers are applied after all additive bonuses (base + scaling + archetype
    /// + lineage) have been summed. The result is then truncated to an integer.
    /// A multiplier of 1.0 has no effect.
    /// </remarks>
    public IReadOnlyDictionary<string, float> LineageMultipliers { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this formula has any archetype-specific bonuses defined.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="ArchetypeBonuses"/> contains at least one entry;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Stats like Initiative and Carrying Capacity have no archetype bonuses,
    /// while Max HP and Max Stamina have bonuses for specific archetypes.
    /// </remarks>
    public bool HasArchetypeBonuses => ArchetypeBonuses.Count > 0;

    /// <summary>
    /// Gets whether this formula has any lineage-specific flat bonuses defined.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="LineageBonuses"/> contains at least one entry;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Only certain stats have lineage bonuses: Max HP (Clan-Born +5),
    /// Max Aether Pool (Rune-Marked +5), Soak (Iron-Blooded +2),
    /// and Movement Speed (Vargr-Kin +1).
    /// </remarks>
    public bool HasLineageBonuses => LineageBonuses.Count > 0;

    /// <summary>
    /// Gets whether this formula has any lineage-specific multipliers defined.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="LineageMultipliers"/> contains at least one entry;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Currently only Max Aether Pool has a lineage multiplier (Rune-Marked: ×1.10).
    /// </remarks>
    public bool HasLineageMultipliers => LineageMultipliers.Count > 0;

    /// <summary>
    /// Gets the number of core attributes that scale this derived stat.
    /// </summary>
    /// <value>
    /// The count of entries in <see cref="AttributeScaling"/>.
    /// For example, Max Stamina has 2 (Finesse and Might), while Movement Speed has 0.
    /// </value>
    public int ScaledAttributeCount => AttributeScaling.Count;

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new <see cref="DerivedStatFormula"/> with validation.
    /// </summary>
    /// <param name="statName">
    /// The name of the derived stat (e.g., "MaxHp", "Initiative").
    /// Must be non-null and non-whitespace.
    /// </param>
    /// <param name="baseValue">
    /// The flat base value before any scaling or bonuses are applied.
    /// </param>
    /// <param name="attributeScaling">
    /// Dictionary of core attribute to scaling factor mappings.
    /// Pass an empty dictionary for stats with no attribute scaling.
    /// </param>
    /// <param name="archetypeBonuses">
    /// Optional dictionary of archetype ID to flat bonus mappings.
    /// Defaults to an empty dictionary if null.
    /// </param>
    /// <param name="lineageBonuses">
    /// Optional dictionary of lineage ID to flat bonus mappings.
    /// Defaults to an empty dictionary if null.
    /// </param>
    /// <param name="lineageMultipliers">
    /// Optional dictionary of lineage ID to multiplier mappings.
    /// Defaults to an empty dictionary if null.
    /// </param>
    /// <param name="logger">Optional logger for diagnostic output during creation.</param>
    /// <returns>A new <see cref="DerivedStatFormula"/> instance with validated data.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="statName"/> is null or whitespace.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="attributeScaling"/> is null.
    /// </exception>
    /// <example>
    /// <code>
    /// // Create the Max HP formula
    /// var maxHpFormula = DerivedStatFormula.Create(
    ///     statName: "MaxHp",
    ///     baseValue: 50,
    ///     attributeScaling: new Dictionary&lt;CoreAttribute, float&gt;
    ///     {
    ///         { CoreAttribute.Sturdiness, 10f }
    ///     },
    ///     archetypeBonuses: new Dictionary&lt;string, int&gt;
    ///     {
    ///         { "warrior", 49 }, { "skirmisher", 30 },
    ///         { "mystic", 20 }, { "adept", 30 }
    ///     },
    ///     lineageBonuses: new Dictionary&lt;string, int&gt;
    ///     {
    ///         { "clan-born", 5 }
    ///     });
    /// </code>
    /// </example>
    /// <remarks>
    /// <para>
    /// This factory method validates the stat name and attribute scaling dictionary,
    /// then creates the formula with empty dictionaries for any null optional parameters.
    /// </para>
    /// </remarks>
    public static DerivedStatFormula Create(
        string statName,
        int baseValue,
        IReadOnlyDictionary<CoreAttribute, float> attributeScaling,
        IReadOnlyDictionary<string, int>? archetypeBonuses = null,
        IReadOnlyDictionary<string, int>? lineageBonuses = null,
        IReadOnlyDictionary<string, float>? lineageMultipliers = null,
        ILogger<DerivedStatFormula>? logger = null)
    {
        _logger = logger;

        _logger?.LogDebug(
            "Creating DerivedStatFormula. StatName={StatName}, BaseValue={BaseValue}, " +
            "ScalingCount={ScalingCount}, ArchetypeBonusCount={ArchetypeBonusCount}, " +
            "LineageBonusCount={LineageBonusCount}, LineageMultiplierCount={LineageMultiplierCount}",
            statName,
            baseValue,
            attributeScaling?.Count ?? 0,
            archetypeBonuses?.Count ?? 0,
            lineageBonuses?.Count ?? 0,
            lineageMultipliers?.Count ?? 0);

        // Validate stat name is provided
        ArgumentException.ThrowIfNullOrWhiteSpace(statName);

        // Validate attribute scaling dictionary is provided (can be empty but not null)
        ArgumentNullException.ThrowIfNull(attributeScaling);

        _logger?.LogDebug(
            "Validation passed for DerivedStatFormula. StatName={StatName}, " +
            "BaseValue={BaseValue}, AttributeScalingEntries={ScalingEntries}",
            statName,
            baseValue,
            string.Join(", ", attributeScaling.Select(
                kvp => $"{kvp.Key}×{kvp.Value}")));

        var formula = new DerivedStatFormula
        {
            StatName = statName,
            BaseValue = baseValue,
            AttributeScaling = attributeScaling,
            ArchetypeBonuses = archetypeBonuses ?? new Dictionary<string, int>(),
            LineageBonuses = lineageBonuses ?? new Dictionary<string, int>(),
            LineageMultipliers = lineageMultipliers ?? new Dictionary<string, float>()
        };

        _logger?.LogInformation(
            "Created DerivedStatFormula for {StatName}. " +
            "BaseValue={BaseValue}, ScaledAttributes={ScaledAttributeCount}, " +
            "HasArchetypeBonuses={HasArchetypeBonuses}, " +
            "HasLineageBonuses={HasLineageBonuses}, " +
            "HasLineageMultipliers={HasLineageMultipliers}",
            formula.StatName,
            formula.BaseValue,
            formula.ScaledAttributeCount,
            formula.HasArchetypeBonuses,
            formula.HasLineageBonuses,
            formula.HasLineageMultipliers);

        return formula;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CALCULATION METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculates the derived stat value for the given attribute values, archetype, and lineage.
    /// </summary>
    /// <param name="attributeValues">
    /// A dictionary mapping <see cref="CoreAttribute"/> values to their current integer values.
    /// Must contain entries for all attributes referenced in <see cref="AttributeScaling"/>.
    /// </param>
    /// <param name="archetypeId">
    /// The lowercase archetype identifier (e.g., "warrior", "mystic"), or null if
    /// no archetype bonus should be applied.
    /// </param>
    /// <param name="lineageId">
    /// The lowercase lineage identifier (e.g., "clan-born", "rune-marked"), or null if
    /// no lineage bonus or multiplier should be applied.
    /// </param>
    /// <returns>
    /// The calculated stat value as an integer. The result is the sum of base value,
    /// attribute scaling, archetype bonus, and lineage bonus, optionally multiplied
    /// by a lineage multiplier, then truncated to an integer.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="attributeValues"/> is null.
    /// </exception>
    /// <example>
    /// <code>
    /// var formula = DerivedStatFormula.Create("MaxHp", 50,
    ///     new Dictionary&lt;CoreAttribute, float&gt; { { CoreAttribute.Sturdiness, 10f } },
    ///     archetypeBonuses: new Dictionary&lt;string, int&gt; { { "warrior", 49 } },
    ///     lineageBonuses: new Dictionary&lt;string, int&gt; { { "clan-born", 5 } });
    ///
    /// var attributes = new Dictionary&lt;CoreAttribute, int&gt;
    /// {
    ///     { CoreAttribute.Sturdiness, 4 }
    /// };
    ///
    /// var hp = formula.Calculate(attributes, "warrior", "clan-born");
    /// // hp == 144: (4 × 10) + 50 + 49 + 5 = 144
    /// </code>
    /// </example>
    /// <remarks>
    /// <para>
    /// The calculation follows this exact order:
    /// </para>
    /// <list type="number">
    ///   <item><description>Start with <see cref="BaseValue"/></description></item>
    ///   <item><description>Add attribute × scaling for each entry in <see cref="AttributeScaling"/></description></item>
    ///   <item><description>Add archetype bonus if <paramref name="archetypeId"/> matches</description></item>
    ///   <item><description>Add lineage bonus if <paramref name="lineageId"/> matches</description></item>
    ///   <item><description>Multiply by lineage multiplier if <paramref name="lineageId"/> matches</description></item>
    ///   <item><description>Truncate to integer (floor via cast)</description></item>
    /// </list>
    /// </remarks>
    public int Calculate(
        IReadOnlyDictionary<CoreAttribute, int> attributeValues,
        string? archetypeId,
        string? lineageId)
    {
        ArgumentNullException.ThrowIfNull(attributeValues);

        _logger?.LogDebug(
            "Calculating {StatName}. BaseValue={BaseValue}, " +
            "ArchetypeId={ArchetypeId}, LineageId={LineageId}",
            StatName,
            BaseValue,
            archetypeId ?? "(none)",
            lineageId ?? "(none)");

        // Step 1: Start with base value
        float value = BaseValue;

        _logger?.LogDebug(
            "Formula {StatName}: Starting with base value {BaseValue}",
            StatName,
            BaseValue);

        // Step 2: Apply attribute scaling
        foreach (var (attr, scale) in AttributeScaling)
        {
            if (attributeValues.TryGetValue(attr, out var attrValue))
            {
                var contribution = attrValue * scale;
                value += contribution;

                _logger?.LogDebug(
                    "Formula {StatName}: Applied {Attribute} scaling — " +
                    "{AttrValue} × {Scale} = +{Contribution} (running total: {RunningTotal})",
                    StatName,
                    attr,
                    attrValue,
                    scale,
                    contribution,
                    value);
            }
            else
            {
                _logger?.LogDebug(
                    "Formula {StatName}: Attribute {Attribute} not found in values, " +
                    "skipping scaling contribution",
                    StatName,
                    attr);
            }
        }

        // Step 3: Apply archetype bonus
        if (archetypeId != null && ArchetypeBonuses.TryGetValue(archetypeId, out var archBonus))
        {
            value += archBonus;

            _logger?.LogDebug(
                "Formula {StatName}: Applied archetype bonus — " +
                "{ArchetypeId}: +{Bonus} (running total: {RunningTotal})",
                StatName,
                archetypeId,
                archBonus,
                value);
        }

        // Step 4: Apply lineage bonus
        if (lineageId != null && LineageBonuses.TryGetValue(lineageId, out var lineageBonus))
        {
            value += lineageBonus;

            _logger?.LogDebug(
                "Formula {StatName}: Applied lineage bonus — " +
                "{LineageId}: +{Bonus} (running total: {RunningTotal})",
                StatName,
                lineageId,
                lineageBonus,
                value);
        }

        // Step 5: Apply lineage multiplier (after all additive bonuses)
        if (lineageId != null && LineageMultipliers.TryGetValue(lineageId, out var multiplier))
        {
            var preMultiplierValue = value;
            value *= multiplier;

            _logger?.LogDebug(
                "Formula {StatName}: Applied lineage multiplier — " +
                "{LineageId}: ×{Multiplier} ({PreValue} → {PostValue})",
                StatName,
                lineageId,
                multiplier,
                preMultiplierValue,
                value);
        }

        // Step 6: Truncate to integer
        var result = (int)value;

        _logger?.LogInformation(
            "Calculated {StatName} = {Result}. " +
            "Base={BaseValue}, ArchetypeId={ArchetypeId}, LineageId={LineageId}",
            StatName,
            result,
            BaseValue,
            archetypeId ?? "(none)",
            lineageId ?? "(none)");

        return result;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the archetype bonus for a specific archetype.
    /// </summary>
    /// <param name="archetypeId">
    /// The lowercase archetype identifier (e.g., "warrior", "mystic").
    /// </param>
    /// <returns>
    /// The flat bonus value for the specified archetype, or 0 if the archetype
    /// has no bonus defined for this stat.
    /// </returns>
    /// <example>
    /// <code>
    /// var formula = DerivedStatFormula.Create("MaxHp", 50, ...,
    ///     archetypeBonuses: new Dictionary&lt;string, int&gt; { { "warrior", 49 } });
    ///
    /// formula.GetArchetypeBonus("warrior");    // returns 49
    /// formula.GetArchetypeBonus("mystic");     // returns 0 (not in dictionary)
    /// </code>
    /// </example>
    public int GetArchetypeBonus(string archetypeId) =>
        ArchetypeBonuses.TryGetValue(archetypeId, out var bonus) ? bonus : 0;

    /// <summary>
    /// Gets the lineage bonus for a specific lineage.
    /// </summary>
    /// <param name="lineageId">
    /// The lowercase lineage identifier (e.g., "clan-born", "iron-blooded").
    /// </param>
    /// <returns>
    /// The flat bonus value for the specified lineage, or 0 if the lineage
    /// has no bonus defined for this stat.
    /// </returns>
    /// <example>
    /// <code>
    /// var formula = DerivedStatFormula.Create("Soak", 0, ...,
    ///     lineageBonuses: new Dictionary&lt;string, int&gt; { { "iron-blooded", 2 } });
    ///
    /// formula.GetLineageBonus("iron-blooded");  // returns 2
    /// formula.GetLineageBonus("clan-born");     // returns 0 (not in dictionary)
    /// </code>
    /// </example>
    public int GetLineageBonus(string lineageId) =>
        LineageBonuses.TryGetValue(lineageId, out var bonus) ? bonus : 0;

    /// <summary>
    /// Gets the lineage multiplier for a specific lineage.
    /// </summary>
    /// <param name="lineageId">
    /// The lowercase lineage identifier (e.g., "rune-marked").
    /// </param>
    /// <returns>
    /// The multiplier value for the specified lineage, or 1.0 if the lineage
    /// has no multiplier defined for this stat (i.e., no effect).
    /// </returns>
    /// <example>
    /// <code>
    /// var formula = DerivedStatFormula.Create("MaxAetherPool", 0, ...,
    ///     lineageMultipliers: new Dictionary&lt;string, float&gt; { { "rune-marked", 1.1f } });
    ///
    /// formula.GetLineageMultiplier("rune-marked");  // returns 1.1
    /// formula.GetLineageMultiplier("clan-born");    // returns 1.0 (default)
    /// </code>
    /// </example>
    public float GetLineageMultiplier(string lineageId) =>
        LineageMultipliers.TryGetValue(lineageId, out var multiplier) ? multiplier : 1.0f;

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a formatted string representation of this formula.
    /// </summary>
    /// <returns>
    /// A string in the format "Formula [StatName]: base=BaseValue, scaling=N, archetypes=N, lineages=N"
    /// showing the formula configuration for debugging and logging.
    /// </returns>
    public override string ToString() =>
        $"Formula [{StatName}]: base={BaseValue}, scaling={ScaledAttributeCount}, " +
        $"archetypes={ArchetypeBonuses.Count}, lineages={LineageBonuses.Count}";
}
