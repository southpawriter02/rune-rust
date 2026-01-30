// ═══════════════════════════════════════════════════════════════════════════════
// IDerivedStatCalculator.cs
// Interface defining the contract for calculating derived statistics from
// character attributes, archetype bonuses, and lineage bonuses. Supports both
// full calculation and real-time preview during attribute allocation.
// Version: 0.17.2g
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Calculates derived statistics from character attributes, archetype bonuses,
/// and lineage bonuses.
/// </summary>
/// <remarks>
/// <para>
/// IDerivedStatCalculator defines the contract for computing secondary character
/// statistics (HP, Stamina, Aether Pool, Initiative, Soak, Movement Speed,
/// Carrying Capacity) from core attributes. It applies formula-driven calculations
/// that combine multiple bonus sources:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <strong>Attribute Scaling:</strong> Core attribute values multiplied by
///       per-stat scaling factors (e.g., Sturdiness × 10 for HP).
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Archetype Bonuses:</strong> Flat bonuses based on the character's
///       archetype (e.g., Warrior: +49 HP, Mystic: +20 Aether Pool).
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Lineage Bonuses:</strong> Flat bonuses and optional multipliers
///       based on the character's lineage (e.g., Clan-Born: +5 HP,
///       Rune-Marked: +5 then ×1.10 Aether Pool).
///     </description>
///   </item>
/// </list>
/// <para>
/// The service provides three access patterns:
/// </para>
/// <list type="number">
///   <item>
///     <description>
///       <see cref="CalculateDerivedStats"/> — Calculate all 7 derived stats at once.
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="CalculateStat"/> — Calculate a single stat by name for efficiency.
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="GetPreview"/> — Generate a preview from an
///       <see cref="AttributeAllocationState"/> during character creation.
///     </description>
///   </item>
/// </list>
/// <para>
/// <strong>Usage Flow:</strong>
/// <code>
/// // Calculate final stats for a character
/// var stats = calculator.CalculateDerivedStats(attributes, "warrior", "clan-born");
/// // stats.MaxHp == 144, stats.MaxStamina == 60, ...
///
/// // Preview during attribute allocation
/// var preview = calculator.GetPreview(allocationState, "warrior", null);
/// // preview.MaxHp == 139 (updates in real-time as allocation changes)
/// </code>
/// </para>
/// </remarks>
/// <seealso cref="DerivedStats"/>
/// <seealso cref="DerivedStatFormula"/>
/// <seealso cref="AttributeAllocationState"/>
/// <seealso cref="CoreAttribute"/>
public interface IDerivedStatCalculator
{
    /// <summary>
    /// Calculates all derived stats from attribute values, archetype, and lineage.
    /// </summary>
    /// <param name="attributes">
    /// Dictionary mapping <see cref="CoreAttribute"/> values to their current integer values.
    /// Must contain entries for all five core attributes (Might, Finesse, Wits, Will, Sturdiness).
    /// </param>
    /// <param name="archetypeId">
    /// The lowercase archetype identifier (e.g., "warrior", "mystic", "skirmisher", "adept").
    /// Determines which archetype bonuses are applied to each derived stat.
    /// May be null if no archetype bonus should be applied.
    /// </param>
    /// <param name="lineageId">
    /// The lowercase lineage identifier (e.g., "clan-born", "rune-marked", "iron-blooded", "vargr-kin").
    /// Determines which lineage bonuses and multipliers are applied.
    /// May be null if no lineage bonus should be applied.
    /// </param>
    /// <returns>
    /// A <see cref="DerivedStats"/> value object containing all 7 calculated statistics:
    /// MaxHp, MaxStamina, MaxAetherPool, Initiative, Soak, MovementSpeed, and CarryingCapacity.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="attributes"/> is null.
    /// </exception>
    /// <example>
    /// <code>
    /// var attributes = new Dictionary&lt;CoreAttribute, int&gt;
    /// {
    ///     { CoreAttribute.Might, 4 },
    ///     { CoreAttribute.Finesse, 3 },
    ///     { CoreAttribute.Wits, 2 },
    ///     { CoreAttribute.Will, 2 },
    ///     { CoreAttribute.Sturdiness, 4 }
    /// };
    ///
    /// var stats = calculator.CalculateDerivedStats(attributes, "warrior", "clan-born");
    /// // stats.MaxHp == 144: (4 × 10) + 50 + 49 + 5
    /// // stats.MaxStamina == 60: (3 × 5) + (4 × 5) + 20 + 5
    /// // stats.MaxAetherPool == 30: (2 × 10) + (2 × 5) + 0
    /// </code>
    /// </example>
    DerivedStats CalculateDerivedStats(
        IReadOnlyDictionary<CoreAttribute, int> attributes,
        string? archetypeId,
        string? lineageId);

    /// <summary>
    /// Calculates a single derived stat by name.
    /// </summary>
    /// <param name="statName">
    /// The name of the stat to calculate (e.g., "MaxHp", "MaxStamina", "Initiative").
    /// Case-insensitive — the name is normalized to lowercase for lookup.
    /// </param>
    /// <param name="attributes">
    /// Dictionary mapping <see cref="CoreAttribute"/> values to their current integer values.
    /// </param>
    /// <param name="archetypeId">
    /// The lowercase archetype identifier, or null if no archetype bonus should be applied.
    /// </param>
    /// <param name="lineageId">
    /// The lowercase lineage identifier, or null if no lineage bonus should be applied.
    /// </param>
    /// <returns>
    /// The calculated stat value as an integer, or 0 if the stat name is not recognized.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is useful when only a single stat needs to be recalculated,
    /// avoiding the overhead of computing all 7 stats. The stat name is matched
    /// case-insensitively against the formula registry.
    /// </para>
    /// <para>
    /// Known stat names: "maxHp", "maxStamina", "maxAetherPool", "initiative",
    /// "soak", "movementSpeed", "carryingCapacity".
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var hp = calculator.CalculateStat("MaxHp", attributes, "warrior", null);
    /// // hp == 139
    ///
    /// var unknown = calculator.CalculateStat("UnknownStat", attributes, "warrior", null);
    /// // unknown == 0
    /// </code>
    /// </example>
    int CalculateStat(
        string statName,
        IReadOnlyDictionary<CoreAttribute, int> attributes,
        string? archetypeId,
        string? lineageId);

    /// <summary>
    /// Gets a preview of derived stats from an attribute allocation state.
    /// </summary>
    /// <param name="state">
    /// The current <see cref="AttributeAllocationState"/> containing attribute values
    /// to use for calculation. Attribute values are extracted from the state's
    /// CurrentMight, CurrentFinesse, CurrentWits, CurrentWill, and CurrentSturdiness properties.
    /// </param>
    /// <param name="archetypeId">
    /// The lowercase archetype identifier for bonus application.
    /// May be null if no archetype bonus should be applied.
    /// </param>
    /// <param name="lineageId">
    /// The lowercase lineage identifier for bonus and multiplier application.
    /// May be null if no lineage bonus should be applied.
    /// </param>
    /// <returns>
    /// A <see cref="DerivedStats"/> preview reflecting the current allocation state.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This convenience method extracts attribute values from an
    /// <see cref="AttributeAllocationState"/> and delegates to
    /// <see cref="CalculateDerivedStats"/>. Use this during the attribute
    /// allocation step (Step 3) of character creation to provide real-time
    /// feedback as the player adjusts attribute values.
    /// </para>
    /// <para>
    /// The preview updates each time this method is called with a new state,
    /// allowing the UI to show live stat changes during allocation.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var state = AttributeAllocationState.CreateFromRecommendedBuild(
    ///     "warrior", 4, 3, 2, 2, 4, 15);
    ///
    /// var preview = calculator.GetPreview(state, "warrior", null);
    /// // preview.MaxHp == 139, preview.MaxStamina == 60
    ///
    /// // After modifying an attribute, call again for updated preview
    /// var modifiedState = state.WithAttributeValue(CoreAttribute.Sturdiness, 5, 1);
    /// var updatedPreview = calculator.GetPreview(modifiedState, "warrior", null);
    /// // updatedPreview.MaxHp == 149 (10 more HP from +1 Sturdiness)
    /// </code>
    /// </example>
    DerivedStats GetPreview(
        AttributeAllocationState state,
        string? archetypeId,
        string? lineageId);
}
