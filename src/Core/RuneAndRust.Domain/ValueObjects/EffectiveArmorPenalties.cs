// ═══════════════════════════════════════════════════════════════════════════════
// EffectiveArmorPenalties.cs
// Value object containing calculated armor penalties after applying proficiency effects.
// Version: 0.16.2d
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Contains effective armor penalties after applying proficiency level effects.
/// </summary>
/// <remarks>
/// <para>
/// EffectiveArmorPenalties is the result of applying proficiency effects to base
/// armor penalties. It encapsulates both the calculated values and metadata about
/// the calculation for UI feedback.
/// </para>
/// <para>
/// The penalty calculation pipeline:
/// <list type="number">
///   <item><description>Get base penalties from armor category</description></item>
///   <item><description>Apply tier reduction if Expert/Master (Heavy → Medium)</description></item>
///   <item><description>Apply penalty multiplier if NonProficient (2.0x)</description></item>
///   <item><description>Apply attack/defense modifiers from proficiency level</description></item>
/// </list>
/// </para>
/// <para>
/// Key proficiency effects:
/// <list type="table">
///   <listheader>
///     <term>Level</term>
///     <description>Penalty Multiplier | Tier Reduction | Attack | Defense</description>
///   </listheader>
///   <item>
///     <term>NonProficient</term>
///     <description>2.0x | None | -2 | +0</description>
///   </item>
///   <item>
///     <term>Proficient</term>
///     <description>1.0x | None | +0 | +0</description>
///   </item>
///   <item>
///     <term>Expert</term>
///     <description>1.0x | -1 | +0 | +0</description>
///   </item>
///   <item>
///     <term>Master</term>
///     <description>1.0x | -1 | +0 | +1</description>
///   </item>
/// </list>
/// </para>
/// </remarks>
/// <param name="OriginalCategory">The armor category before tier reduction.</param>
/// <param name="BasePenalties">The base penalties from the effective tier.</param>
/// <param name="EffectivePenalties">The penalties after applying the proficiency multiplier.</param>
/// <param name="ProficiencyLevel">The proficiency level used in calculation.</param>
/// <param name="AttackModifier">Attack roll modifier from proficiency (-2 to 0).</param>
/// <param name="DefenseModifier">Defense modifier from proficiency (0 to +1).</param>
/// <param name="OriginalTier">The original weight tier before reduction.</param>
/// <param name="EffectiveTier">The effective tier after applying proficiency tier reduction.</param>
/// <param name="WasMultiplied">Whether the penalty multiplier was applied (NonProficient).</param>
/// <param name="WasTierReduced">Whether tier reduction was applied (Expert/Master).</param>
/// <seealso cref="ArmorCategory"/>
/// <seealso cref="ArmorPenalties"/>
/// <seealso cref="ArmorProficiencyLevel"/>
public readonly record struct EffectiveArmorPenalties(
    ArmorCategory OriginalCategory,
    ArmorPenalties BasePenalties,
    ArmorPenalties EffectivePenalties,
    ArmorProficiencyLevel ProficiencyLevel,
    int AttackModifier,
    int DefenseModifier,
    int OriginalTier,
    int EffectiveTier,
    bool WasMultiplied,
    bool WasTierReduced)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Static Instances
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets zero penalties (for Light armor with Proficient or higher).
    /// </summary>
    /// <value>
    /// An <see cref="EffectiveArmorPenalties"/> instance with all zero values
    /// representing proficient use of Light armor.
    /// </value>
    /// <remarks>
    /// Use this instance when a character is proficient with Light armor,
    /// which has no base penalties to modify.
    /// </remarks>
    public static EffectiveArmorPenalties None => new(
        ArmorCategory.Light,
        ArmorPenalties.None,
        ArmorPenalties.None,
        ArmorProficiencyLevel.Proficient,
        AttackModifier: 0,
        DefenseModifier: 0,
        OriginalTier: 0,
        EffectiveTier: 0,
        WasMultiplied: false,
        WasTierReduced: false);

    // ═══════════════════════════════════════════════════════════════════════════
    // Derived Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the tier difference from reduction.
    /// </summary>
    /// <value>
    /// The number of tiers reduced (typically 0 or 1).
    /// </value>
    /// <remarks>
    /// Non-zero when Expert or Master proficiency applies tier reduction
    /// (e.g., Heavy → Medium penalties).
    /// </remarks>
    public int TierReductionApplied => OriginalTier - EffectiveTier;

    /// <summary>
    /// Gets whether there are any effective penalties.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="EffectivePenalties"/> has any penalty values;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Useful for UI decisions about whether to show the penalty section.
    /// Light armor with any proficiency level typically returns <c>false</c>.
    /// </remarks>
    public bool HasAnyPenalty => EffectivePenalties.HasAnyPenalty;

    /// <summary>
    /// Gets whether this has any attack penalty.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="AttackModifier"/> is negative; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Only NonProficient characters have attack penalties (-2).
    /// </remarks>
    public bool HasAttackPenalty => AttackModifier < 0;

    /// <summary>
    /// Gets whether this has any defense bonus.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="DefenseModifier"/> is positive; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Only Master proficiency has defense bonuses (+1).
    /// </remarks>
    public bool HasDefenseBonus => DefenseModifier > 0;

    /// <summary>
    /// Gets whether the character is non-proficient.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="ProficiencyLevel"/> equals
    /// <see cref="ArmorProficiencyLevel.NonProficient"/>; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Non-proficient characters have doubled penalties and should be warned
    /// about their equipment choices.
    /// </remarks>
    public bool IsNonProficient => ProficiencyLevel == ArmorProficiencyLevel.NonProficient;

    /// <summary>
    /// Gets whether the character has mastery.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="ProficiencyLevel"/> equals
    /// <see cref="ArmorProficiencyLevel.Master"/>; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Master proficiency provides tier reduction and a defense bonus.
    /// </remarks>
    public bool IsMaster => ProficiencyLevel == ArmorProficiencyLevel.Master;

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates EffectiveArmorPenalties with validation.
    /// </summary>
    /// <param name="originalCategory">The armor category before reduction.</param>
    /// <param name="basePenalties">Base penalties from the effective tier.</param>
    /// <param name="effectivePenalties">Penalties after multiplier application.</param>
    /// <param name="proficiencyLevel">The proficiency level used.</param>
    /// <param name="attackModifier">Attack modifier from proficiency (-5 to 0).</param>
    /// <param name="defenseModifier">Defense modifier from proficiency (0 to 3).</param>
    /// <param name="originalTier">Original weight tier before reduction (-1 to 3).</param>
    /// <param name="effectiveTier">Effective tier after reduction (-1 to 3).</param>
    /// <param name="wasMultiplied">Whether multiplier was applied.</param>
    /// <param name="wasTierReduced">Whether tier reduction was applied.</param>
    /// <returns>A new validated <see cref="EffectiveArmorPenalties"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when numeric parameters are outside valid ranges.
    /// </exception>
    /// <example>
    /// <code>
    /// // Create effective penalties for non-proficient Heavy armor
    /// var heavyBase = ArmorPenalties.Create(-2, 5, -10, true);
    /// var doubled = heavyBase.Multiply(2.0m);
    /// var effective = EffectiveArmorPenalties.Create(
    ///     ArmorCategory.Heavy,
    ///     heavyBase,
    ///     doubled,
    ///     ArmorProficiencyLevel.NonProficient,
    ///     attackModifier: -2,
    ///     defenseModifier: 0,
    ///     originalTier: 2,
    ///     effectiveTier: 2,
    ///     wasMultiplied: true,
    ///     wasTierReduced: false);
    /// </code>
    /// </example>
    public static EffectiveArmorPenalties Create(
        ArmorCategory originalCategory,
        ArmorPenalties basePenalties,
        ArmorPenalties effectivePenalties,
        ArmorProficiencyLevel proficiencyLevel,
        int attackModifier,
        int defenseModifier,
        int originalTier,
        int effectiveTier,
        bool wasMultiplied,
        bool wasTierReduced)
    {
        // Validate attack modifier range (-5 to 0)
        ArgumentOutOfRangeException.ThrowIfLessThan(attackModifier, -5);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(attackModifier, 0);

        // Validate defense modifier range (0 to 3)
        ArgumentOutOfRangeException.ThrowIfNegative(defenseModifier);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(defenseModifier, 3);

        // Validate tier ranges (-1 to 3)
        ArgumentOutOfRangeException.ThrowIfLessThan(originalTier, -1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(originalTier, 3);
        ArgumentOutOfRangeException.ThrowIfLessThan(effectiveTier, -1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(effectiveTier, 3);

        return new EffectiveArmorPenalties(
            originalCategory,
            basePenalties,
            effectivePenalties,
            proficiencyLevel,
            attackModifier,
            defenseModifier,
            originalTier,
            effectiveTier,
            wasMultiplied,
            wasTierReduced);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Formatting Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats the attack modifier for display.
    /// </summary>
    /// <returns>
    /// A formatted string with a leading sign (e.g., "+0", "-2").
    /// </returns>
    /// <example>
    /// <code>
    /// var penalties = EffectiveArmorPenalties.None;
    /// Console.WriteLine(penalties.FormatAttackModifier()); // Output: "+0"
    /// </code>
    /// </example>
    public string FormatAttackModifier() =>
        AttackModifier >= 0 ? $"+{AttackModifier}" : $"{AttackModifier}";

    /// <summary>
    /// Formats the defense modifier for display.
    /// </summary>
    /// <returns>
    /// A formatted string with a leading sign (e.g., "+0", "+1").
    /// </returns>
    /// <example>
    /// <code>
    /// // Master proficiency gives +1 defense
    /// Console.WriteLine(masterPenalties.FormatDefenseModifier()); // Output: "+1"
    /// </code>
    /// </example>
    public string FormatDefenseModifier() =>
        DefenseModifier >= 0 ? $"+{DefenseModifier}" : $"{DefenseModifier}";

    /// <summary>
    /// Creates a summary string showing key calculation results.
    /// </summary>
    /// <returns>
    /// A compact string showing category, level, and effective penalties.
    /// </returns>
    /// <example>
    /// <code>
    /// var penalties = ... // Expert Heavy armor
    /// Console.WriteLine(penalties.FormatSummary());
    /// // Output: "Heavy (Expert): Tier 2→1, Atk: +0, Def: +0"
    /// </code>
    /// </example>
    public string FormatSummary()
    {
        var tierInfo = WasTierReduced
            ? $"Tier {OriginalTier}→{EffectiveTier}"
            : $"Tier {OriginalTier}";

        return $"{OriginalCategory} ({ProficiencyLevel}): {tierInfo}, " +
               $"Atk: {FormatAttackModifier()}, Def: {FormatDefenseModifier()}";
    }

    /// <summary>
    /// Creates a list of modification notes for UI tooltips.
    /// </summary>
    /// <returns>
    /// A read-only list of strings describing active modifications.
    /// </returns>
    /// <remarks>
    /// Returns an empty list if no special modifications apply.
    /// Useful for building dynamic tooltip content.
    /// </remarks>
    /// <example>
    /// <code>
    /// var notes = penalties.GetModificationNotes();
    /// // For NonProficient Heavy: ["Penalties doubled (2.0x)", "Attack -2"]
    /// // For Expert Heavy: ["Tier reduced: Heavy → Medium"]
    /// // For Master Heavy: ["Tier reduced: Heavy → Medium", "Defense +1"]
    /// </code>
    /// </example>
    public IReadOnlyList<string> GetModificationNotes()
    {
        var notes = new List<string>();

        if (WasMultiplied)
        {
            notes.Add("Penalties doubled (2.0x)");
        }

        if (WasTierReduced)
        {
            notes.Add($"Tier reduced: {OriginalCategory} → Tier {EffectiveTier}");
        }

        if (HasAttackPenalty)
        {
            notes.Add($"Attack {FormatAttackModifier()}");
        }

        if (HasDefenseBonus)
        {
            notes.Add($"Defense {FormatDefenseModifier()}");
        }

        return notes;
    }

    /// <summary>
    /// Creates a display string for debug/logging.
    /// </summary>
    /// <returns>
    /// A formatted string summarizing all penalty values.
    /// </returns>
    /// <example>
    /// <code>
    /// var penalties = ... // NonProficient Heavy armor
    /// Console.WriteLine(penalties.ToString());
    /// // Output: "Heavy (NonProficient): Tier 2, Multiplied, Atk: -2, Def: +0"
    /// </code>
    /// </example>
    public override string ToString()
    {
        var flags = new List<string>();
        if (WasMultiplied) flags.Add("Multiplied");
        if (WasTierReduced) flags.Add("TierReduced");
        var flagStr = flags.Count > 0 ? string.Join(", ", flags) + ", " : "";

        return $"{OriginalCategory} ({ProficiencyLevel}): Tier {OriginalTier}→{EffectiveTier}, " +
               $"{flagStr}Atk: {FormatAttackModifier()}, Def: {FormatDefenseModifier()}";
    }

    /// <summary>
    /// Creates a detailed debug string with all property values.
    /// </summary>
    /// <returns>
    /// A verbose string representation suitable for detailed debugging.
    /// </returns>
    /// <example>
    /// <code>
    /// var penalties = ... // Expert Heavy armor
    /// Console.WriteLine(penalties.ToDebugString());
    /// // Output: "EffectiveArmorPenalties { Category: Heavy, Level: Expert, OrigTier: 2, EffTier: 1, ... }"
    /// </code>
    /// </example>
    public string ToDebugString() =>
        $"EffectiveArmorPenalties {{ Category: {OriginalCategory}, Level: {ProficiencyLevel}, " +
        $"OrigTier: {OriginalTier}, EffTier: {EffectiveTier}, Atk: {AttackModifier}, Def: {DefenseModifier}, " +
        $"Multiplied: {WasMultiplied}, TierReduced: {WasTierReduced}, " +
        $"Base: {BasePenalties.ToDebugString()}, Effective: {EffectivePenalties.ToDebugString()} }}";
}
