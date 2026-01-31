// ═══════════════════════════════════════════════════════════════════════════════
// DerivedStatsPreview.cs
// Preview of derived statistics based on current character creation selections.
// Shows the calculated HP, Stamina, and Aether Pool (AP) values with breakdown
// by source (lineage bonuses, archetype bonuses). Used by the ViewModelBuilder
// to provide real-time stat feedback during the attribute allocation step and
// summary display in the creation wizard.
// Version: 0.17.5b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Preview of derived stats based on current character creation selections.
/// Shows the calculated HP, Stamina, and AP values with breakdown by source.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="DerivedStatsPreview"/> is a read-only presentation value object that
/// provides a simplified view of the character's derived statistics during the
/// creation workflow. Unlike <see cref="DerivedStats"/>, which contains all 7
/// derived stats for a fully-created character, this preview focuses on the 3
/// primary resource pools (HP, Stamina, Aether Pool) and their bonus sources.
/// </para>
/// <para>
/// The preview is rebuilt whenever the creation state changes — particularly
/// during Step 3 (Attribute Allocation) where it updates in real-time as the
/// player adjusts attribute values. It is also displayed in the Step 6 (Summary)
/// screen to show final resource values before confirmation.
/// </para>
/// <para>
/// <strong>Source Breakdown:</strong> The preview tracks bonus contributions from
/// two sources:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <strong>Lineage:</strong> HP bonus (Clan-Born: +5) and AP bonus
///       (Rune-Marked: +5 base, plus ×1.10 multiplier applied during calculation).
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Archetype:</strong> HP bonus (Warrior: +49, Skirmisher/Adept: +30,
///       Mystic: +20), Stamina bonus (Warrior/Skirmisher: +5), and AP bonus
///       (Mystic: +20).
///     </description>
///   </item>
/// </list>
/// <para>
/// <strong>Note:</strong> The <see cref="MaxAp"/> property maps to
/// <see cref="DerivedStats.MaxAetherPool"/> in the domain model. The shorter
/// "AP" name is used in the presentation layer for display brevity.
/// </para>
/// </remarks>
/// <seealso cref="DerivedStats"/>
/// <seealso cref="CharacterCreationViewModel"/>
/// <seealso cref="RuneAndRust.Domain.Entities.CharacterCreationState"/>
public readonly record struct DerivedStatsPreview
{
    // ═══════════════════════════════════════════════════════════════════════════
    // RESOURCE POOL TOTALS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the calculated maximum HP.
    /// </summary>
    /// <value>
    /// Total HP from formula: (STURDINESS × 10) + 50 + ArchetypeBonus + LineageBonus.
    /// Includes all bonus sources. Zero when no attributes are allocated.
    /// </value>
    /// <remarks>
    /// <para>
    /// HP values range from approximately 80 (Mystic, minimum Sturdiness) to 154
    /// (Warrior with Clan-Born, maximum Sturdiness). The preview updates in real-time
    /// as the player adjusts the Sturdiness attribute during Step 3.
    /// </para>
    /// </remarks>
    public int MaxHp { get; init; }

    /// <summary>
    /// Gets the calculated maximum Stamina.
    /// </summary>
    /// <value>
    /// Total Stamina from formula: (FINESSE × 5) + (MIGHT × 5) + 20 + ArchetypeBonus.
    /// Zero when no attributes are allocated.
    /// </value>
    /// <remarks>
    /// <para>
    /// Stamina scales with both Finesse and Might, making it responsive to two
    /// attribute adjustments during Step 3. Only Warrior and Skirmisher archetypes
    /// receive a Stamina bonus (+5 each).
    /// </para>
    /// </remarks>
    public int MaxStamina { get; init; }

    /// <summary>
    /// Gets the calculated maximum Aether Pool (AP).
    /// </summary>
    /// <value>
    /// Total AP from formula: (WILL × 10) + (WITS × 5) + ArchetypeBonus + LineageBonus.
    /// Maps to <see cref="DerivedStats.MaxAetherPool"/> in the domain model.
    /// Zero when no attributes are allocated.
    /// </value>
    /// <remarks>
    /// <para>
    /// The Aether Pool is the primary resource for Mystic archetype abilities.
    /// Will provides the strongest scaling (×10), making it the most impactful
    /// attribute for AP. The Rune-Marked lineage's Aether-Tainted trait applies
    /// a ×1.10 multiplier after all additive bonuses (handled by the
    /// DerivedStatCalculator, not this preview).
    /// </para>
    /// <para>
    /// <strong>Naming:</strong> This property uses "Ap" (Aether Pool) as a
    /// presentation-layer abbreviation. The domain model uses "AetherPool"
    /// for the full name.
    /// </para>
    /// </remarks>
    public int MaxAp { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // SOURCE BREAKDOWN — LINEAGE
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the HP bonus contributed by the selected lineage.
    /// </summary>
    /// <value>
    /// Lineage HP bonus value. Clan-Born: +5, all others: 0.
    /// Zero when no lineage is selected.
    /// </value>
    /// <remarks>
    /// <para>
    /// Only the Clan-Born lineage provides an HP bonus (+5 from the
    /// "[Survivor's Resolve]" passive). This value is displayed alongside
    /// the total MaxHp to help the player understand where their HP comes from.
    /// </para>
    /// </remarks>
    public int HpFromLineage { get; init; }

    /// <summary>
    /// Gets the AP bonus contributed by the selected lineage.
    /// </summary>
    /// <value>
    /// Lineage AP bonus value. Rune-Marked: +5, all others: 0.
    /// Zero when no lineage is selected.
    /// </value>
    /// <remarks>
    /// <para>
    /// Only the Rune-Marked lineage provides an AP bonus (+5 from the
    /// "[Aether-Tainted]" passive). The additional ×1.10 multiplier is
    /// applied by the DerivedStatCalculator and reflected in <see cref="MaxAp"/>
    /// but not broken out separately in this preview.
    /// </para>
    /// </remarks>
    public int ApFromLineage { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // SOURCE BREAKDOWN — ARCHETYPE
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the HP bonus contributed by the selected archetype.
    /// </summary>
    /// <value>
    /// Archetype HP bonus: Warrior +49, Skirmisher +30, Mystic +20, Adept +30.
    /// Zero when no archetype is selected.
    /// </value>
    /// <remarks>
    /// <para>
    /// The archetype HP bonus is the largest single contributor to MaxHp.
    /// Warriors have the highest bonus (+49), reflecting their role as the
    /// primary tank archetype.
    /// </para>
    /// </remarks>
    public int HpFromArchetype { get; init; }

    /// <summary>
    /// Gets the AP bonus contributed by the selected archetype.
    /// </summary>
    /// <value>
    /// Archetype AP bonus: Mystic +20, all others 0.
    /// Zero when no archetype is selected.
    /// </value>
    /// <remarks>
    /// <para>
    /// Only the Mystic archetype receives an Aether Pool bonus (+20),
    /// reflecting their exclusive reliance on magical abilities.
    /// </para>
    /// </remarks>
    public int ApFromArchetype { get; init; }

    /// <summary>
    /// Gets the Stamina bonus contributed by the selected archetype.
    /// </summary>
    /// <value>
    /// Archetype Stamina bonus: Warrior +5, Skirmisher +5, Mystic 0, Adept 0.
    /// Zero when no archetype is selected.
    /// </value>
    /// <remarks>
    /// <para>
    /// Physical combat archetypes (Warrior and Skirmisher) receive a Stamina
    /// bonus to support their stamina-based abilities.
    /// </para>
    /// </remarks>
    public int StaminaFromArchetype { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this preview contains any non-zero resource values.
    /// </summary>
    /// <value>
    /// <c>true</c> if any of <see cref="MaxHp"/>, <see cref="MaxStamina"/>,
    /// or <see cref="MaxAp"/> is greater than 0; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Returns <c>false</c> for <see cref="Empty"/> and for states where
    /// no attributes have been allocated yet. The TUI uses this property to
    /// determine whether to display the stats preview panel.
    /// </remarks>
    public bool HasValues => MaxHp > 0 || MaxStamina > 0 || MaxAp > 0;

    /// <summary>
    /// Gets the total combined resource pool (HP + Stamina + AP).
    /// </summary>
    /// <value>
    /// The sum of <see cref="MaxHp"/>, <see cref="MaxStamina"/>,
    /// and <see cref="MaxAp"/>. Zero for <see cref="Empty"/>.
    /// </value>
    public int TotalResourcePool => MaxHp + MaxStamina + MaxAp;

    // ═══════════════════════════════════════════════════════════════════════════
    // STATIC PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates an empty preview with all values at zero.
    /// </summary>
    /// <value>
    /// A <see cref="DerivedStatsPreview"/> with all numeric properties set to 0.
    /// </value>
    /// <remarks>
    /// Returned by the ViewModelBuilder when no attributes have been allocated
    /// (i.e., <c>CharacterCreationState.Attributes</c> is null). The TUI checks
    /// <see cref="HasValues"/> to decide whether to render the preview panel.
    /// </remarks>
    public static DerivedStatsPreview Empty => new()
    {
        MaxHp = 0,
        MaxStamina = 0,
        MaxAp = 0,
        HpFromLineage = 0,
        HpFromArchetype = 0,
        ApFromLineage = 0,
        ApFromArchetype = 0,
        StaminaFromArchetype = 0
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a formatted summary of the preview for display.
    /// </summary>
    /// <returns>
    /// A single-line string showing all three resource pools,
    /// e.g., "HP: 104 | Stamina: 55 | AP: 30".
    /// Returns "No stats preview available" for <see cref="Empty"/>.
    /// </returns>
    /// <remarks>
    /// Designed for compact display in the TUI stats preview panel.
    /// Does not include the source breakdown — use individual properties
    /// for detailed display.
    /// </remarks>
    /// <example>
    /// <code>
    /// var preview = new DerivedStatsPreview { MaxHp = 104, MaxStamina = 55, MaxAp = 30 };
    /// preview.GetSummary(); // "HP: 104 | Stamina: 55 | AP: 30"
    /// </code>
    /// </example>
    public string GetSummary() =>
        HasValues
            ? $"HP: {MaxHp} | Stamina: {MaxStamina} | AP: {MaxAp}"
            : "No stats preview available";

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a formatted string representation of the preview for debugging.
    /// </summary>
    /// <returns>
    /// A string in the format "DerivedStatsPreview [HP:104 ST:55 AP:30 | Lineage(HP+5,AP+0) Archetype(HP+49,ST+5,AP+0)]".
    /// </returns>
    public override string ToString() =>
        $"DerivedStatsPreview [HP:{MaxHp} ST:{MaxStamina} AP:{MaxAp} | " +
        $"Lineage(HP+{HpFromLineage},AP+{ApFromLineage}) " +
        $"Archetype(HP+{HpFromArchetype},ST+{StaminaFromArchetype},AP+{ApFromArchetype})]";
}
