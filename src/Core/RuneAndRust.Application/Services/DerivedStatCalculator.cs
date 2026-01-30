// ═══════════════════════════════════════════════════════════════════════════════
// DerivedStatCalculator.cs
// Service that calculates derived statistics from core attributes, archetype
// bonuses, and lineage bonuses using configurable formula definitions. Supports
// full stat calculation, single stat lookup, and real-time preview during
// attribute allocation in the character creation workflow.
// Version: 0.17.2g
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Calculates derived statistics from attributes and bonuses using formula definitions.
/// </summary>
/// <remarks>
/// <para>
/// DerivedStatCalculator implements <see cref="IDerivedStatCalculator"/> to provide
/// the core logic for computing secondary character statistics. It uses a dictionary
/// of <see cref="DerivedStatFormula"/> objects that define the calculation rules for
/// each derived stat.
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <strong>CalculateDerivedStats:</strong> Computes all 7 derived stats at once,
///       returning a <see cref="DerivedStats"/> value object.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>CalculateStat:</strong> Computes a single stat by name for efficiency
///       when only one value is needed.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>GetPreview:</strong> Extracts attributes from an
///       <see cref="AttributeAllocationState"/> and delegates to
///       <see cref="CalculateDerivedStats"/> for real-time preview during allocation.
///     </description>
///   </item>
/// </list>
/// <para>
/// Each formula combines a base value, attribute scaling multipliers, archetype bonuses,
/// lineage bonuses, and optional lineage multipliers. The calculation order is:
/// BaseValue + AttributeScaling + ArchetypeBonus + LineageBonus, then LineageMultiplier,
/// then truncate to integer.
/// </para>
/// <para>
/// The <see cref="CreateWithDefaultFormulas"/> static factory method provides a convenience
/// constructor that creates a calculator pre-configured with the standard game formulas
/// for all 7 derived stats.
/// </para>
/// </remarks>
/// <seealso cref="IDerivedStatCalculator"/>
/// <seealso cref="DerivedStatFormula"/>
/// <seealso cref="DerivedStats"/>
/// <seealso cref="AttributeAllocationState"/>
public class DerivedStatCalculator : IDerivedStatCalculator
{
    // ═══════════════════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Dictionary of formula definitions keyed by lowercase stat name.
    /// Each formula defines the calculation rules for one derived stat.
    /// </summary>
    private readonly IReadOnlyDictionary<string, DerivedStatFormula> _formulas;

    /// <summary>
    /// Logger for diagnostic output during stat calculation operations.
    /// </summary>
    private readonly ILogger<DerivedStatCalculator> _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of <see cref="DerivedStatCalculator"/>.
    /// </summary>
    /// <param name="formulas">
    /// Dictionary of <see cref="DerivedStatFormula"/> definitions keyed by lowercase
    /// stat name (e.g., "maxhp", "maxstamina", "initiative"). Must not be null.
    /// Each formula defines the base value, attribute scaling, archetype bonuses,
    /// lineage bonuses, and lineage multipliers for one derived stat.
    /// </param>
    /// <param name="logger">
    /// The logger for diagnostic output during calculation. Must not be null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="formulas"/> or <paramref name="logger"/> is null.
    /// </exception>
    public DerivedStatCalculator(
        IReadOnlyDictionary<string, DerivedStatFormula> formulas,
        ILogger<DerivedStatCalculator> logger)
    {
        ArgumentNullException.ThrowIfNull(formulas, nameof(formulas));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _formulas = formulas;
        _logger = logger;

        _logger.LogInformation(
            "DerivedStatCalculator initialized with {FormulaCount} formulas. " +
            "Available stats: [{StatNames}]",
            _formulas.Count,
            string.Join(", ", _formulas.Keys));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STATIC FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a <see cref="DerivedStatCalculator"/> pre-configured with the standard
    /// game formulas for all 7 derived stats.
    /// </summary>
    /// <param name="logger">
    /// The logger for diagnostic output. Must not be null.
    /// </param>
    /// <returns>
    /// A new <see cref="DerivedStatCalculator"/> instance with default formulas for
    /// MaxHp, MaxStamina, MaxAetherPool, Initiative, Soak, MovementSpeed, and CarryingCapacity.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="logger"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// The default formulas encode the standard game balance values:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Max HP: base 50, Sturdiness × 10, archetype and lineage bonuses</description></item>
    ///   <item><description>Max Stamina: base 20, (Finesse + Might) × 5, archetype bonuses</description></item>
    ///   <item><description>Max Aether Pool: base 0, Will × 10 + Wits × 5, Mystic bonus, Rune-Marked multiplier</description></item>
    ///   <item><description>Initiative: Finesse × 1 + Wits × 0.5</description></item>
    ///   <item><description>Soak: Sturdiness × 0.5, Iron-Blooded bonus</description></item>
    ///   <item><description>Movement Speed: flat 5, Vargr-Kin bonus</description></item>
    ///   <item><description>Carrying Capacity: Might × 10</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var logger = loggerFactory.CreateLogger&lt;DerivedStatCalculator&gt;();
    /// var calculator = DerivedStatCalculator.CreateWithDefaultFormulas(logger);
    ///
    /// var stats = calculator.CalculateDerivedStats(attributes, "warrior", null);
    /// </code>
    /// </example>
    public static DerivedStatCalculator CreateWithDefaultFormulas(
        ILogger<DerivedStatCalculator> logger)
    {
        var formulas = CreateDefaultFormulas();
        return new DerivedStatCalculator(formulas, logger);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS - FULL CALCULATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public DerivedStats CalculateDerivedStats(
        IReadOnlyDictionary<CoreAttribute, int> attributes,
        string? archetypeId,
        string? lineageId)
    {
        ArgumentNullException.ThrowIfNull(attributes, nameof(attributes));

        _logger.LogDebug(
            "Calculating all derived stats. " +
            "ArchetypeId='{ArchetypeId}', LineageId='{LineageId}', " +
            "AttributeCount={AttributeCount}, FormulaCount={FormulaCount}",
            archetypeId ?? "null",
            lineageId ?? "null",
            attributes.Count,
            _formulas.Count);

        // Calculate each derived stat using the corresponding formula
        var maxHp = CalculateStatInternal("maxHp", attributes, archetypeId, lineageId);
        var maxStamina = CalculateStatInternal("maxStamina", attributes, archetypeId, lineageId);
        var maxAetherPool = CalculateStatInternal("maxAetherPool", attributes, archetypeId, lineageId);
        var initiative = CalculateStatInternal("initiative", attributes, archetypeId, lineageId);
        var soak = CalculateStatInternal("soak", attributes, archetypeId, lineageId);
        var movementSpeed = CalculateStatInternal("movementSpeed", attributes, archetypeId, lineageId);
        var carryingCapacity = CalculateStatInternal("carryingCapacity", attributes, archetypeId, lineageId);

        _logger.LogDebug(
            "Individual stat calculations complete. " +
            "MaxHp={MaxHp}, MaxStamina={MaxStamina}, MaxAetherPool={MaxAetherPool}, " +
            "Initiative={Initiative}, Soak={Soak}, " +
            "MovementSpeed={MovementSpeed}, CarryingCapacity={CarryingCapacity}",
            maxHp, maxStamina, maxAetherPool,
            initiative, soak,
            movementSpeed, carryingCapacity);

        // Assemble the DerivedStats value object
        var stats = DerivedStats.Create(
            maxHp, maxStamina, maxAetherPool,
            initiative, soak, movementSpeed, carryingCapacity);

        _logger.LogInformation(
            "Calculated all derived stats. " +
            "ArchetypeId='{ArchetypeId}', LineageId='{LineageId}', " +
            "Summary={Summary}",
            archetypeId ?? "null",
            lineageId ?? "null",
            stats.GetSummary());

        return stats;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS - SINGLE STAT CALCULATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public int CalculateStat(
        string statName,
        IReadOnlyDictionary<CoreAttribute, int> attributes,
        string? archetypeId,
        string? lineageId)
    {
        ArgumentNullException.ThrowIfNull(attributes, nameof(attributes));
        ArgumentException.ThrowIfNullOrWhiteSpace(statName, nameof(statName));

        _logger.LogDebug(
            "Calculating single stat. StatName='{StatName}', " +
            "ArchetypeId='{ArchetypeId}', LineageId='{LineageId}'",
            statName,
            archetypeId ?? "null",
            lineageId ?? "null");

        // Normalize the stat name to lowercase for dictionary lookup
        var normalizedName = statName.ToLowerInvariant();

        _logger.LogDebug(
            "Normalized stat name: '{Original}' → '{Normalized}'",
            statName,
            normalizedName);

        var result = CalculateStatInternal(normalizedName, attributes, archetypeId, lineageId);

        _logger.LogDebug(
            "Single stat calculation complete. " +
            "StatName='{StatName}', Result={Result}",
            statName,
            result);

        return result;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS - PREVIEW
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public DerivedStats GetPreview(
        AttributeAllocationState state,
        string? archetypeId,
        string? lineageId)
    {
        _logger.LogDebug(
            "Generating preview from allocation state. " +
            "Mode={Mode}, ArchetypeId='{ArchetypeId}', LineageId='{LineageId}', " +
            "M:{Might} F:{Finesse} Wi:{Wits} Wl:{Will} S:{Sturdiness}",
            state.Mode,
            archetypeId ?? "null",
            lineageId ?? "null",
            state.CurrentMight,
            state.CurrentFinesse,
            state.CurrentWits,
            state.CurrentWill,
            state.CurrentSturdiness);

        // Extract attribute values from the allocation state into a dictionary
        var attributes = ExtractAttributesFromState(state);

        _logger.LogDebug(
            "Extracted {AttributeCount} attribute values from allocation state. " +
            "Delegating to CalculateDerivedStats",
            attributes.Count);

        // Delegate to the full calculation method
        var preview = CalculateDerivedStats(attributes, archetypeId, lineageId);

        _logger.LogDebug(
            "Preview generation complete. Summary={Summary}",
            preview.GetSummary());

        return preview;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE METHODS - CALCULATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculates a single derived stat by looking up the formula and applying it.
    /// </summary>
    /// <param name="statName">
    /// The lowercase stat name to look up in the formula registry.
    /// </param>
    /// <param name="attributes">
    /// Dictionary of attribute values to pass to the formula.
    /// </param>
    /// <param name="archetypeId">
    /// The archetype identifier for bonus lookup, or null.
    /// </param>
    /// <param name="lineageId">
    /// The lineage identifier for bonus and multiplier lookup, or null.
    /// </param>
    /// <returns>
    /// The calculated stat value, or 0 if the stat name is not found in the formula registry.
    /// </returns>
    private int CalculateStatInternal(
        string statName,
        IReadOnlyDictionary<CoreAttribute, int> attributes,
        string? archetypeId,
        string? lineageId)
    {
        if (!_formulas.TryGetValue(statName, out var formula))
        {
            _logger.LogWarning(
                "Unknown stat requested: '{StatName}'. " +
                "Available stats: [{AvailableStats}]. Returning 0.",
                statName,
                string.Join(", ", _formulas.Keys));
            return 0;
        }

        _logger.LogDebug(
            "Found formula for '{StatName}'. " +
            "BaseValue={BaseValue}, ScaledAttributes={ScaledAttributeCount}, " +
            "HasArchetypeBonuses={HasArchetypeBonuses}, " +
            "HasLineageBonuses={HasLineageBonuses}, " +
            "HasLineageMultipliers={HasLineageMultipliers}",
            statName,
            formula.BaseValue,
            formula.ScaledAttributeCount,
            formula.HasArchetypeBonuses,
            formula.HasLineageBonuses,
            formula.HasLineageMultipliers);

        var result = formula.Calculate(attributes, archetypeId, lineageId);

        _logger.LogDebug(
            "Calculated {StatName}={Value}. " +
            "ArchetypeId='{ArchetypeId}', LineageId='{LineageId}'",
            statName,
            result,
            archetypeId ?? "null",
            lineageId ?? "null");

        return result;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE METHODS - STATE EXTRACTION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Extracts attribute values from an <see cref="AttributeAllocationState"/>
    /// into a dictionary keyed by <see cref="CoreAttribute"/>.
    /// </summary>
    /// <param name="state">
    /// The allocation state to extract attribute values from.
    /// </param>
    /// <returns>
    /// A dictionary mapping each <see cref="CoreAttribute"/> to its current integer
    /// value from the allocation state.
    /// </returns>
    private static Dictionary<CoreAttribute, int> ExtractAttributesFromState(
        AttributeAllocationState state) =>
        new()
        {
            { CoreAttribute.Might, state.CurrentMight },
            { CoreAttribute.Finesse, state.CurrentFinesse },
            { CoreAttribute.Wits, state.CurrentWits },
            { CoreAttribute.Will, state.CurrentWill },
            { CoreAttribute.Sturdiness, state.CurrentSturdiness }
        };

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE METHODS - DEFAULT FORMULA DEFINITIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates the default formula definitions for all 7 derived stats.
    /// </summary>
    /// <returns>
    /// A dictionary of <see cref="DerivedStatFormula"/> objects keyed by lowercase
    /// stat name, containing the standard game balance values.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The default formulas encode the following stat calculations:
    /// </para>
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <strong>Max HP:</strong> (STURDINESS × 10) + 50 + ArchetypeBonus + LineageBonus.
    ///       Warrior: +49, Skirmisher: +30, Mystic: +20, Adept: +30. Clan-Born: +5.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <strong>Max Stamina:</strong> (FINESSE × 5) + (MIGHT × 5) + 20 + ArchetypeBonus.
    ///       Warrior: +5, Skirmisher: +5.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <strong>Max Aether Pool:</strong> (WILL × 10) + (WITS × 5) + ArchetypeBonus + LineageBonus × LineageMultiplier.
    ///       Mystic: +20. Rune-Marked: +5 flat then ×1.10 multiplier.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <strong>Initiative:</strong> FINESSE × 1 + WITS × 0.5. No bonuses.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <strong>Soak:</strong> STURDINESS × 0.5. Iron-Blooded: +2.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <strong>Movement Speed:</strong> Flat base of 5. Vargr-Kin: +1.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <strong>Carrying Capacity:</strong> MIGHT × 10. No bonuses.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    private static Dictionary<string, DerivedStatFormula> CreateDefaultFormulas() =>
        new(StringComparer.OrdinalIgnoreCase)
        {
            // Max HP = (STURDINESS × 10) + 50 + ArchetypeBonus + LineageBonus
            ["maxHp"] = DerivedStatFormula.Create(
                "MaxHp",
                baseValue: 50,
                attributeScaling: new Dictionary<CoreAttribute, float>
                    { { CoreAttribute.Sturdiness, 10f } },
                archetypeBonuses: new Dictionary<string, int>
                    { { "warrior", 49 }, { "skirmisher", 30 }, { "mystic", 20 }, { "adept", 30 } },
                lineageBonuses: new Dictionary<string, int>
                    { { "clan-born", 5 } }),

            // Max Stamina = (FINESSE × 5) + (MIGHT × 5) + 20 + ArchetypeBonus
            ["maxStamina"] = DerivedStatFormula.Create(
                "MaxStamina",
                baseValue: 20,
                attributeScaling: new Dictionary<CoreAttribute, float>
                    { { CoreAttribute.Finesse, 5f }, { CoreAttribute.Might, 5f } },
                archetypeBonuses: new Dictionary<string, int>
                    { { "warrior", 5 }, { "skirmisher", 5 } }),

            // Max Aether Pool = (WILL × 10) + (WITS × 5) + ArchetypeBonus + LineageBonus × Multiplier
            ["maxAetherPool"] = DerivedStatFormula.Create(
                "MaxAetherPool",
                baseValue: 0,
                attributeScaling: new Dictionary<CoreAttribute, float>
                    { { CoreAttribute.Will, 10f }, { CoreAttribute.Wits, 5f } },
                archetypeBonuses: new Dictionary<string, int>
                    { { "mystic", 20 } },
                lineageBonuses: new Dictionary<string, int>
                    { { "rune-marked", 5 } },
                lineageMultipliers: new Dictionary<string, float>
                    { { "rune-marked", 1.10f } }),

            // Initiative = FINESSE × 1 + WITS × 0.5
            ["initiative"] = DerivedStatFormula.Create(
                "Initiative",
                baseValue: 0,
                attributeScaling: new Dictionary<CoreAttribute, float>
                    { { CoreAttribute.Finesse, 1f }, { CoreAttribute.Wits, 0.5f } }),

            // Soak = STURDINESS × 0.5 + LineageBonus
            ["soak"] = DerivedStatFormula.Create(
                "Soak",
                baseValue: 0,
                attributeScaling: new Dictionary<CoreAttribute, float>
                    { { CoreAttribute.Sturdiness, 0.5f } },
                lineageBonuses: new Dictionary<string, int>
                    { { "iron-blooded", 2 } }),

            // Movement Speed = 5 (flat) + LineageBonus
            ["movementSpeed"] = DerivedStatFormula.Create(
                "MovementSpeed",
                baseValue: 5,
                attributeScaling: new Dictionary<CoreAttribute, float>(),
                lineageBonuses: new Dictionary<string, int>
                    { { "vargr-kin", 1 } }),

            // Carrying Capacity = MIGHT × 10
            ["carryingCapacity"] = DerivedStatFormula.Create(
                "CarryingCapacity",
                baseValue: 0,
                attributeScaling: new Dictionary<CoreAttribute, float>
                    { { CoreAttribute.Might, 10f } })
        };
}
