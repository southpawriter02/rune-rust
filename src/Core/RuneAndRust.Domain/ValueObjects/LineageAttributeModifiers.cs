// ═══════════════════════════════════════════════════════════════════════════════
// LineageAttributeModifiers.cs
// Value object containing attribute modifiers applied by a lineage.
// Version: 0.17.0a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Contains attribute modifiers applied by a lineage during character creation.
/// </summary>
/// <remarks>
/// <para>
/// LineageAttributeModifiers defines both fixed modifiers (applied automatically)
/// and flexible bonuses (player chooses which attribute). Only Clan-Born has
/// a flexible bonus; other lineages have fixed distributions.
/// </para>
/// <para>
/// Modifiers can be positive or negative. The net sum of all modifiers varies
/// by lineage, but all lineages provide a net +1 total modifier:
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Lineage</term>
///     <description>Net Modifier Total</description>
///   </listheader>
///   <item>
///     <term>Clan-Born</term>
///     <description>+1 (flexible to any attribute)</description>
///   </item>
///   <item>
///     <term>Rune-Marked</term>
///     <description>+1 (WILL +2, STURDINESS -1)</description>
///   </item>
///   <item>
///     <term>Iron-Blooded</term>
///     <description>+1 (STURDINESS +2, WILL -1)</description>
///   </item>
///   <item>
///     <term>Vargr-Kin</term>
///     <description>+1 (MIGHT +1, FINESSE +1, WILL -1)</description>
///   </item>
/// </list>
/// </remarks>
/// <param name="MightModifier">Modifier to MIGHT attribute.</param>
/// <param name="FinesseModifier">Modifier to FINESSE attribute.</param>
/// <param name="WitsModifier">Modifier to WITS attribute.</param>
/// <param name="WillModifier">Modifier to WILL attribute.</param>
/// <param name="SturdinessModifier">Modifier to STURDINESS attribute.</param>
/// <param name="HasFlexibleBonus">Whether this lineage grants a player-chosen bonus.</param>
/// <param name="FlexibleBonusAmount">The amount of the flexible bonus (if any).</param>
public readonly record struct LineageAttributeModifiers(
    int MightModifier,
    int FinesseModifier,
    int WitsModifier,
    int WillModifier,
    int SturdinessModifier,
    bool HasFlexibleBonus,
    int FlexibleBonusAmount)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the total of all fixed modifiers (excluding flexible bonus).
    /// </summary>
    /// <value>
    /// The sum of all fixed attribute modifiers. This value may be positive,
    /// negative, or zero depending on the lineage configuration.
    /// </value>
    public int TotalFixedModifiers =>
        MightModifier + FinesseModifier + WitsModifier +
        WillModifier + SturdinessModifier;

    // ═══════════════════════════════════════════════════════════════════════════
    // METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the fixed modifier for a specific attribute.
    /// </summary>
    /// <param name="attribute">The attribute to get the modifier for.</param>
    /// <returns>The fixed modifier value for the specified attribute.</returns>
    /// <remarks>
    /// This method returns only the fixed modifier and does not include
    /// any flexible bonus. Use <see cref="GetEffectiveModifier"/> to include
    /// the flexible bonus in the calculation.
    /// </remarks>
    public int GetModifier(CoreAttribute attribute) => attribute switch
    {
        CoreAttribute.Might => MightModifier,
        CoreAttribute.Finesse => FinesseModifier,
        CoreAttribute.Wits => WitsModifier,
        CoreAttribute.Will => WillModifier,
        CoreAttribute.Sturdiness => SturdinessModifier,
        _ => 0
    };

    /// <summary>
    /// Calculates the effective modifier for an attribute including flexible bonus.
    /// </summary>
    /// <param name="attribute">The attribute to calculate the modifier for.</param>
    /// <param name="flexibleBonusTarget">
    /// The attribute receiving the flexible bonus (if applicable).
    /// Pass null if no flexible bonus target has been selected.
    /// </param>
    /// <returns>
    /// The total modifier including the fixed modifier and any applicable flexible bonus.
    /// </returns>
    /// <remarks>
    /// <para>
    /// For lineages without a flexible bonus (HasFlexibleBonus = false),
    /// this method returns the same value as <see cref="GetModifier"/>.
    /// </para>
    /// <para>
    /// For Clan-Born (which has HasFlexibleBonus = true), the flexible bonus
    /// is added only to the attribute specified by <paramref name="flexibleBonusTarget"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var clanBorn = LineageAttributeModifiers.ClanBorn;
    /// 
    /// // Without specifying a target, returns 0 for all attributes
    /// var mightNoTarget = clanBorn.GetEffectiveModifier(CoreAttribute.Might, null);
    /// // mightNoTarget == 0
    /// 
    /// // With MIGHT as the target, returns +1 for MIGHT only
    /// var mightWithTarget = clanBorn.GetEffectiveModifier(CoreAttribute.Might, CoreAttribute.Might);
    /// // mightWithTarget == 1
    /// </code>
    /// </example>
    public int GetEffectiveModifier(CoreAttribute attribute, CoreAttribute? flexibleBonusTarget)
    {
        var baseModifier = GetModifier(attribute);

        if (HasFlexibleBonus && flexibleBonusTarget == attribute)
        {
            return baseModifier + FlexibleBonusAmount;
        }

        return baseModifier;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STATIC FACTORY PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the standard attribute modifiers for Clan-Born lineage.
    /// </summary>
    /// <value>
    /// Modifiers: No fixed bonuses, +1 flexible bonus to any attribute.
    /// </value>
    /// <remarks>
    /// <para>
    /// Clan-Born is the only lineage with a flexible bonus. The player
    /// chooses which attribute receives the +1 bonus during character creation.
    /// </para>
    /// </remarks>
    public static LineageAttributeModifiers ClanBorn => new(
        MightModifier: 0,
        FinesseModifier: 0,
        WitsModifier: 0,
        WillModifier: 0,
        SturdinessModifier: 0,
        HasFlexibleBonus: true,
        FlexibleBonusAmount: 1);

    /// <summary>
    /// Gets the standard attribute modifiers for Rune-Marked lineage.
    /// </summary>
    /// <value>
    /// Modifiers: WILL +2, STURDINESS -1. Net total: +1.
    /// </value>
    /// <remarks>
    /// <para>
    /// Rune-Marked characters have enhanced willpower and connection to
    /// the Aether, but their bodies are less resilient due to their
    /// ancestors' exposure to the All-Rune's corruption.
    /// </para>
    /// </remarks>
    public static LineageAttributeModifiers RuneMarked => new(
        MightModifier: 0,
        FinesseModifier: 0,
        WitsModifier: 0,
        WillModifier: 2,
        SturdinessModifier: -1,
        HasFlexibleBonus: false,
        FlexibleBonusAmount: 0);

    /// <summary>
    /// Gets the standard attribute modifiers for Iron-Blooded lineage.
    /// </summary>
    /// <value>
    /// Modifiers: STURDINESS +2, WILL -1. Net total: +1.
    /// </value>
    /// <remarks>
    /// <para>
    /// Iron-Blooded characters have bodies hardened by generations of
    /// working with Blight-metal, but their minds are less fortified
    /// against psychic assault.
    /// </para>
    /// </remarks>
    public static LineageAttributeModifiers IronBlooded => new(
        MightModifier: 0,
        FinesseModifier: 0,
        WitsModifier: 0,
        WillModifier: -1,
        SturdinessModifier: 2,
        HasFlexibleBonus: false,
        FlexibleBonusAmount: 0);

    /// <summary>
    /// Gets the standard attribute modifiers for Vargr-Kin lineage.
    /// </summary>
    /// <value>
    /// Modifiers: MIGHT +1, FINESSE +1, WILL -1. Net total: +1.
    /// </value>
    /// <remarks>
    /// <para>
    /// Vargr-Kin characters inherit the physical prowess of their
    /// primal-spirit ancestors, with enhanced strength and agility,
    /// but slightly diminished mental fortitude.
    /// </para>
    /// </remarks>
    public static LineageAttributeModifiers VargrKin => new(
        MightModifier: 1,
        FinesseModifier: 1,
        WitsModifier: 0,
        WillModifier: -1,
        SturdinessModifier: 0,
        HasFlexibleBonus: false,
        FlexibleBonusAmount: 0);

    // ═══════════════════════════════════════════════════════════════════════════
    // TOSTRING OVERRIDE
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation of the attribute modifiers.
    /// </summary>
    /// <returns>
    /// A human-readable string showing all non-zero modifiers and any flexible bonus.
    /// Returns "No modifiers" if all values are zero.
    /// </returns>
    /// <example>
    /// <code>
    /// var runeMarked = LineageAttributeModifiers.RuneMarked;
    /// Console.WriteLine(runeMarked.ToString());
    /// // Output: "WILL +2, STURDINESS -1"
    /// 
    /// var clanBorn = LineageAttributeModifiers.ClanBorn;
    /// Console.WriteLine(clanBorn.ToString());
    /// // Output: "+1 to any"
    /// </code>
    /// </example>
    public override string ToString()
    {
        var parts = new List<string>();

        if (MightModifier != 0)
            parts.Add($"MIGHT {MightModifier:+#;-#;0}");
        if (FinesseModifier != 0)
            parts.Add($"FINESSE {FinesseModifier:+#;-#;0}");
        if (WitsModifier != 0)
            parts.Add($"WITS {WitsModifier:+#;-#;0}");
        if (WillModifier != 0)
            parts.Add($"WILL {WillModifier:+#;-#;0}");
        if (SturdinessModifier != 0)
            parts.Add($"STURDINESS {SturdinessModifier:+#;-#;0}");
        if (HasFlexibleBonus)
            parts.Add($"+{FlexibleBonusAmount} to any");

        return parts.Count > 0 ? string.Join(", ", parts) : "No modifiers";
    }
}
