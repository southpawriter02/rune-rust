// ═══════════════════════════════════════════════════════════════════════════════
// GaldrInterference.cs
// Value object encapsulating Galdr/WITS interference rules for armor usage.
// Version: 0.16.2c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Encapsulates Galdr/WITS interference rules when wearing armor.
/// </summary>
/// <remarks>
/// <para>
/// GaldrInterference defines how armor affects a character's ability to channel
/// Galdr (mystic magic) or use WITS-based abilities. Different archetypes
/// experience different interference effects based on their training.
/// </para>
/// <para>
/// The Faraday-Cage Theory: Heavy armor made of conductive metals creates an
/// electromagnetic barrier that disrupts the subtle energy patterns required
/// for Galdr channeling. Mystics are completely blocked from casting, while
/// Adepts (who use WITS-based mental techniques) experience severe concentration
/// penalties.
/// </para>
/// <para>
/// Interference levels by archetype:
/// <list type="table">
///   <listheader>
///     <term>Archetype</term>
///     <description>Medium Armor | Heavy Armor | Shields</description>
///   </listheader>
///   <item>
///     <term>Mystic</term>
///     <description>-2 Galdr | BLOCKED | BLOCKED</description>
///   </item>
///   <item>
///     <term>Adept</term>
///     <description>-2 WITS | -4 WITS | BLOCKED</description>
///   </item>
///   <item>
///     <term>Warrior/Skirmisher</term>
///     <description>None | None | None</description>
///   </item>
/// </list>
/// </para>
/// </remarks>
/// <param name="MediumArmorPenalty">
/// Penalty applied to Galdr or WITS checks when wearing Medium armor.
/// Typically -2 for affected archetypes, 0 for non-casters.
/// </param>
/// <param name="HeavyArmorBlocked">
/// Whether Heavy armor completely blocks Galdr casting.
/// True for Mystics (Faraday-cage effect), false for others.
/// </param>
/// <param name="HeavyArmorPenalty">
/// Penalty applied to Galdr or WITS checks when wearing Heavy armor.
/// Only relevant if <paramref name="HeavyArmorBlocked"/> is false.
/// Typically -4 for Adepts.
/// </param>
/// <param name="AffectsWits">
/// Whether penalties apply to WITS-based abilities instead of Galdr.
/// True for Adepts who channel through mental focus rather than magic.
/// </param>
/// <param name="ShieldsBlocked">
/// Whether using shields blocks Galdr or imposes WITS penalties.
/// True for Mystics and Adepts who require free hands for channeling.
/// </param>
/// <param name="Description">
/// Lore description explaining the interference mechanism for this archetype.
/// Used for UI tooltips and player information.
/// </param>
/// <seealso cref="Entities.ArchetypeArmorProficiencySet"/>
/// <seealso cref="Enums.ArmorCategory"/>
public readonly record struct GaldrInterference(
    int MediumArmorPenalty,
    bool HeavyArmorBlocked,
    int HeavyArmorPenalty,
    bool AffectsWits,
    bool ShieldsBlocked,
    string Description)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Static Instances for Archetypes
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the Galdr interference rules for the Mystic archetype.
    /// </summary>
    /// <value>
    /// A <see cref="GaldrInterference"/> instance configured for Mystics who
    /// channel Galdr magic directly and are severely impacted by armor.
    /// </value>
    /// <remarks>
    /// <para>
    /// Mystics experience the most severe armor interference:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Medium armor: -2 penalty to Galdr checks</description></item>
    ///   <item><description>Heavy armor: Galdr BLOCKED (Faraday-cage effect)</description></item>
    ///   <item><description>Shields: BLOCKED (interferes with channeling gestures)</description></item>
    /// </list>
    /// <para>
    /// The Faraday-cage theory states that conductive metal armor creates an
    /// electromagnetic barrier that completely disrupts the subtle energy
    /// patterns required for Galdr channeling.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var mysticRules = GaldrInterference.MysticRules;
    /// Console.WriteLine(mysticRules.HeavyArmorBlocked);     // True
    /// Console.WriteLine(mysticRules.MediumArmorPenalty);    // -2
    /// Console.WriteLine(mysticRules.AffectsWits);           // False
    /// </code>
    /// </example>
    public static GaldrInterference MysticRules => new(
        MediumArmorPenalty: -2,
        HeavyArmorBlocked: true,
        HeavyArmorPenalty: 0,
        AffectsWits: false,
        ShieldsBlocked: true,
        Description: "Heavy armor creates a Faraday-cage effect that blocks Galdr entirely. " +
                     "Medium armor and shields interfere with the subtle gestures required for channeling.");

    /// <summary>
    /// Gets the Galdr interference rules for the Adept archetype.
    /// </summary>
    /// <value>
    /// A <see cref="GaldrInterference"/> instance configured for Adepts who
    /// use WITS-based mental techniques and experience concentration penalties.
    /// </value>
    /// <remarks>
    /// <para>
    /// Adepts experience interference through concentration disruption:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Medium armor: -2 penalty to WITS checks</description></item>
    ///   <item><description>Heavy armor: -4 penalty to WITS checks (severe)</description></item>
    ///   <item><description>Shields: BLOCKED (requires free hands for focus)</description></item>
    /// </list>
    /// <para>
    /// Unlike Mystics, Adepts are not completely blocked by heavy armor.
    /// Their WITS-based techniques are mental rather than magical, so they
    /// can still function but with significant concentration penalties.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var adeptRules = GaldrInterference.AdeptRules;
    /// Console.WriteLine(adeptRules.HeavyArmorBlocked);      // False
    /// Console.WriteLine(adeptRules.HeavyArmorPenalty);      // -4
    /// Console.WriteLine(adeptRules.AffectsWits);            // True
    /// </code>
    /// </example>
    public static GaldrInterference AdeptRules => new(
        MediumArmorPenalty: -2,
        HeavyArmorBlocked: false,
        HeavyArmorPenalty: -4,
        AffectsWits: true,
        ShieldsBlocked: true,
        Description: "Armor bulk impairs the mental focus required for WITS-based abilities. " +
                     "Medium armor causes minor distraction, heavy armor severely impairs concentration.");

    // ═══════════════════════════════════════════════════════════════════════════
    // Derived Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether any interference rules apply at all.
    /// </summary>
    /// <value>
    /// <c>true</c> if any penalty, blocking, or WITS effect is configured;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Returns <c>false</c> for martial archetypes (Warrior, Skirmisher) who
    /// have no Galdr/WITS interference regardless of armor worn.
    /// </remarks>
    public bool HasAnyInterference =>
        MediumArmorPenalty != 0 ||
        HeavyArmorBlocked ||
        HeavyArmorPenalty != 0 ||
        ShieldsBlocked;

    /// <summary>
    /// Gets a value indicating whether medium armor imposes any penalty.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="MediumArmorPenalty"/> is less than 0;
    /// otherwise, <c>false</c>.
    /// </value>
    public bool HasMediumArmorPenalty => MediumArmorPenalty < 0;

    /// <summary>
    /// Gets whether heavy armor causes any negative effect (blocked or penalty).
    /// </summary>
    /// <value>
    /// <c>true</c> if heavy armor is blocked or has a penalty;
    /// otherwise, <c>false</c>.
    /// </value>
    public bool HasHeavyArmorEffect => HeavyArmorBlocked || HeavyArmorPenalty < 0;

    // ═══════════════════════════════════════════════════════════════════════════
    // Query Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Determines if the specified armor category blocks casting entirely.
    /// </summary>
    /// <param name="category">The armor category to check.</param>
    /// <returns>
    /// <c>true</c> if casting is blocked for the specified category;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Blocking rules:
    /// <list type="bullet">
    ///   <item><description>Light armor: Never blocked</description></item>
    ///   <item><description>Medium armor: Never blocked (only penalties)</description></item>
    ///   <item><description>Heavy armor: Blocked if <see cref="HeavyArmorBlocked"/> is true</description></item>
    ///   <item><description>Shields: Blocked if <see cref="ShieldsBlocked"/> is true</description></item>
    ///   <item><description>Specialized: Check based on equivalent category</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var mysticRules = GaldrInterference.MysticRules;
    /// Console.WriteLine(mysticRules.IsBlockedBy(ArmorCategory.Heavy));   // True
    /// Console.WriteLine(mysticRules.IsBlockedBy(ArmorCategory.Medium));  // False (penalty only)
    /// Console.WriteLine(mysticRules.IsBlockedBy(ArmorCategory.Light));   // False
    /// </code>
    /// </example>
    public bool IsBlockedBy(Enums.ArmorCategory category) => category switch
    {
        Enums.ArmorCategory.Heavy => HeavyArmorBlocked,
        Enums.ArmorCategory.Shields => ShieldsBlocked,
        _ => false
    };

    /// <summary>
    /// Gets the penalty value for the specified armor category.
    /// </summary>
    /// <param name="category">The armor category to check.</param>
    /// <returns>
    /// The penalty value (negative number), or 0 if no penalty applies.
    /// Returns 0 if the category causes blocking rather than a penalty.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Penalty values indicate dice or modifier reductions:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>-2: Minor interference (Medium armor)</description></item>
    ///   <item><description>-4: Severe interference (Heavy armor for Adepts)</description></item>
    ///   <item><description>0: No penalty or blocked (check <see cref="IsBlockedBy"/>)</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var adeptRules = GaldrInterference.AdeptRules;
    /// Console.WriteLine(adeptRules.GetPenaltyFor(ArmorCategory.Medium));  // -2
    /// Console.WriteLine(adeptRules.GetPenaltyFor(ArmorCategory.Heavy));   // -4
    /// Console.WriteLine(adeptRules.GetPenaltyFor(ArmorCategory.Light));   // 0
    /// </code>
    /// </example>
    public int GetPenaltyFor(Enums.ArmorCategory category) => category switch
    {
        Enums.ArmorCategory.Medium => MediumArmorPenalty,
        Enums.ArmorCategory.Heavy when !HeavyArmorBlocked => HeavyArmorPenalty,
        _ => 0
    };

    /// <summary>
    /// Determines if Galdr (or WITS abilities) can be used with the specified armor.
    /// </summary>
    /// <param name="category">The armor category being worn.</param>
    /// <returns>
    /// <c>true</c> if casting/abilities can be used (possibly with penalty);
    /// <c>false</c> if completely blocked.
    /// </returns>
    /// <remarks>
    /// This is the inverse of <see cref="IsBlockedBy"/>. Use this method to
    /// determine if a character can attempt Galdr/WITS actions at all.
    /// </remarks>
    /// <example>
    /// <code>
    /// var mysticRules = GaldrInterference.MysticRules;
    /// if (mysticRules.CanCastWith(ArmorCategory.Medium))
    /// {
    ///     var penalty = mysticRules.GetPenaltyFor(ArmorCategory.Medium);
    ///     // Apply -2 penalty to Galdr roll
    /// }
    /// </code>
    /// </example>
    public bool CanCastWith(Enums.ArmorCategory category) =>
        !IsBlockedBy(category);

    // ═══════════════════════════════════════════════════════════════════════════
    // Formatting Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the effect type name based on whether WITS or Galdr is affected.
    /// </summary>
    /// <returns>"WITS" if <see cref="AffectsWits"/> is true; otherwise, "Galdr".</returns>
    public string GetEffectTypeName() => AffectsWits ? "WITS" : "Galdr";

    /// <summary>
    /// Formats the interference effect for a given armor category.
    /// </summary>
    /// <param name="category">The armor category to describe.</param>
    /// <returns>
    /// A human-readable description of the effect (e.g., "-2 Galdr", "BLOCKED", "None").
    /// </returns>
    /// <example>
    /// <code>
    /// var mysticRules = GaldrInterference.MysticRules;
    /// Console.WriteLine(mysticRules.FormatEffect(ArmorCategory.Medium)); // "-2 Galdr"
    /// Console.WriteLine(mysticRules.FormatEffect(ArmorCategory.Heavy));  // "BLOCKED"
    /// Console.WriteLine(mysticRules.FormatEffect(ArmorCategory.Light));  // "None"
    /// </code>
    /// </example>
    public string FormatEffect(Enums.ArmorCategory category)
    {
        if (IsBlockedBy(category))
            return "BLOCKED";

        var penalty = GetPenaltyFor(category);
        if (penalty == 0)
            return "None";

        return $"{penalty} {GetEffectTypeName()}";
    }

    /// <summary>
    /// Returns a string representation for debugging and logging.
    /// </summary>
    /// <returns>
    /// A formatted string summarizing all interference rules.
    /// </returns>
    /// <example>
    /// <code>
    /// var mysticRules = GaldrInterference.MysticRules;
    /// Console.WriteLine(mysticRules.ToString());
    /// // Output: "GaldrInterference { Type: Galdr, Medium: -2, Heavy: BLOCKED, Shields: BLOCKED }"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"GaldrInterference {{ Type: {GetEffectTypeName()}, " +
        $"Medium: {(MediumArmorPenalty != 0 ? MediumArmorPenalty.ToString() : "None")}, " +
        $"Heavy: {(HeavyArmorBlocked ? "BLOCKED" : HeavyArmorPenalty != 0 ? HeavyArmorPenalty.ToString() : "None")}, " +
        $"Shields: {(ShieldsBlocked ? "BLOCKED" : "None")} }}";
}
