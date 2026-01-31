// ═══════════════════════════════════════════════════════════════════════════════
// AbilitiesPreview.cs
// Preview of abilities that will be granted based on archetype and specialization
// selections during character creation. Displays archetype starting abilities
// (3 abilities from Step 4) and specialization Tier 1 abilities (typically 3
// abilities from Step 5) for the TUI summary and selection screens.
// Version: 0.17.5b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Preview of abilities that will be granted based on archetype and specialization.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="AbilitiesPreview"/> is a read-only presentation value object that lists
/// the ability names a character will receive at creation. It separates abilities by
/// source (archetype vs. specialization) so the TUI can display them in distinct
/// sections.
/// </para>
/// <para>
/// The preview becomes populated in two stages:
/// </para>
/// <list type="number">
///   <item>
///     <description>
///       <strong>Step 4 (Archetype):</strong> <see cref="ArchetypeAbilities"/> is populated
///       with 3 starting abilities (e.g., Power Strike, Defensive Stance, Iron Will for Warrior).
///       <see cref="SpecializationAbilities"/> remains empty.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Step 5 (Specialization):</strong> <see cref="SpecializationAbilities"/> is
///       populated with Tier 1 abilities (typically 2-4 abilities depending on the
///       specialization). Both lists are now populated.
///     </description>
///   </item>
/// </list>
/// <para>
/// <strong>Data Sources:</strong> Archetype ability names come from
/// <c>IArchetypeProvider.GetStartingAbilities()</c> via
/// <see cref="ArchetypeAbilityGrant.AbilityName"/>. Specialization ability names come
/// from <c>SpecializationDefinition.GetTier(1)</c> via
/// <see cref="SpecializationAbility.DisplayName"/>.
/// </para>
/// </remarks>
/// <seealso cref="CharacterCreationViewModel"/>
/// <seealso cref="ArchetypeAbilityGrant"/>
/// <seealso cref="SpecializationAbilityTier"/>
public readonly record struct AbilitiesPreview
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the abilities granted by the selected archetype (typically 3).
    /// </summary>
    /// <value>
    /// A read-only list of ability display names from the archetype's starting
    /// ability set. Empty until an archetype is selected (Step 4).
    /// </value>
    /// <remarks>
    /// <para>
    /// Each archetype grants exactly 3 starting abilities:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Warrior: Power Strike, Defensive Stance, Iron Will</description></item>
    ///   <item><description>Skirmisher: Quick Strike, Evasive Maneuvers, Opportunist</description></item>
    ///   <item><description>Mystic: Aether Bolt, Aether Shield, Aether Sense</description></item>
    ///   <item><description>Adept: Precise Strike, Assess Weakness, Resourceful</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var preview = new AbilitiesPreview
    /// {
    ///     ArchetypeAbilities = new[] { "Power Strike", "Defensive Stance", "Iron Will" },
    ///     SpecializationAbilities = Array.Empty&lt;string&gt;()
    /// };
    /// // preview.ArchetypeAbilities.Count == 3
    /// </code>
    /// </example>
    public IReadOnlyList<string> ArchetypeAbilities { get; init; }

    /// <summary>
    /// Gets the Tier 1 abilities granted by the selected specialization (typically 2-4).
    /// </summary>
    /// <value>
    /// A read-only list of ability display names from the specialization's
    /// Tier 1 ability set. Empty until a specialization is selected (Step 5).
    /// </value>
    /// <remarks>
    /// <para>
    /// The first specialization is free at character creation. Tier 1 abilities
    /// are unlocked immediately upon selection. The number of Tier 1 abilities
    /// varies by specialization (typically 2-4).
    /// </para>
    /// </remarks>
    public IReadOnlyList<string> SpecializationAbilities { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the total count of abilities to be granted.
    /// </summary>
    /// <value>
    /// The combined count of <see cref="ArchetypeAbilities"/> and
    /// <see cref="SpecializationAbilities"/>. Typically 5-7 at creation
    /// (3 archetype + 2-4 specialization).
    /// </value>
    public int TotalCount => (ArchetypeAbilities?.Count ?? 0) + (SpecializationAbilities?.Count ?? 0);

    /// <summary>
    /// Gets whether this preview contains any abilities.
    /// </summary>
    /// <value>
    /// <c>true</c> if either <see cref="ArchetypeAbilities"/> or
    /// <see cref="SpecializationAbilities"/> contains at least one entry;
    /// otherwise, <c>false</c>.
    /// </value>
    public bool HasAbilities => TotalCount > 0;

    /// <summary>
    /// Gets whether specialization abilities have been populated.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="SpecializationAbilities"/> has at least one entry;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Useful for the TUI to determine whether to display the specialization
    /// abilities section (only after Step 5 is completed).
    /// </remarks>
    public bool HasSpecializationAbilities => (SpecializationAbilities?.Count ?? 0) > 0;

    // ═══════════════════════════════════════════════════════════════════════════
    // STATIC PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates an empty preview with no abilities.
    /// </summary>
    /// <value>
    /// An <see cref="AbilitiesPreview"/> with both ability lists set to empty arrays.
    /// </value>
    /// <remarks>
    /// Returned by the ViewModelBuilder when no archetype has been selected yet.
    /// </remarks>
    public static AbilitiesPreview Empty => new()
    {
        ArchetypeAbilities = Array.Empty<string>(),
        SpecializationAbilities = Array.Empty<string>()
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a formatted summary of all abilities for display.
    /// </summary>
    /// <returns>
    /// A comma-separated string of all ability names from both archetype and
    /// specialization, e.g., "Power Strike, Defensive Stance, Iron Will + Shield Wall, Guard, Stalwart".
    /// Returns "No abilities preview available" for <see cref="Empty"/>.
    /// </returns>
    /// <example>
    /// <code>
    /// var preview = new AbilitiesPreview
    /// {
    ///     ArchetypeAbilities = new[] { "Power Strike", "Defensive Stance", "Iron Will" },
    ///     SpecializationAbilities = new[] { "Shield Wall", "Guard", "Stalwart" }
    /// };
    /// preview.GetSummary();
    /// // "Power Strike, Defensive Stance, Iron Will + Shield Wall, Guard, Stalwart"
    /// </code>
    /// </example>
    public string GetSummary()
    {
        if (!HasAbilities)
            return "No abilities preview available";

        var archPart = ArchetypeAbilities?.Count > 0
            ? string.Join(", ", ArchetypeAbilities)
            : "";
        var specPart = SpecializationAbilities?.Count > 0
            ? string.Join(", ", SpecializationAbilities)
            : "";

        if (!string.IsNullOrEmpty(archPart) && !string.IsNullOrEmpty(specPart))
            return $"{archPart} + {specPart}";

        return !string.IsNullOrEmpty(archPart) ? archPart : specPart;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a formatted string representation of the preview for debugging.
    /// </summary>
    /// <returns>
    /// A string in the format "AbilitiesPreview [Archetype: 3, Specialization: 3, Total: 6]".
    /// </returns>
    public override string ToString() =>
        $"AbilitiesPreview [Archetype: {ArchetypeAbilities?.Count ?? 0}, " +
        $"Specialization: {SpecializationAbilities?.Count ?? 0}, Total: {TotalCount}]";
}
