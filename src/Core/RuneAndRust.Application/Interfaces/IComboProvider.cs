using RuneAndRust.Domain.Definitions;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Provides combo definitions from configuration for the combo system.
/// </summary>
/// <remarks>
/// <para>IComboProvider is the primary interface for accessing combo definitions loaded from configuration.</para>
/// <para>Implementations should load combos at startup and provide efficient lookup methods:</para>
/// <list type="bullet">
///   <item><description><see cref="GetCombo"/> - Direct lookup by combo ID</description></item>
///   <item><description><see cref="GetAllCombos"/> - Full list for UI display</description></item>
///   <item><description><see cref="GetCombosForClass"/> - Class-filtered list for player-specific UI</description></item>
///   <item><description><see cref="GetCombosContaining"/> - Find combos using a specific ability</description></item>
///   <item><description><see cref="GetCombosStartingWith"/> - Find potential combos when ability is used</description></item>
/// </list>
/// <para>
/// The combo detection system (v0.10.3b) will use <see cref="GetCombosStartingWith"/> to efficiently
/// find potential combos when a player uses an ability.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Get all combos available to a warrior
/// var warriorCombos = comboProvider.GetCombosForClass("warrior");
///
/// // Find combos that start with "charge" ability
/// var chargeCombos = comboProvider.GetCombosStartingWith("charge");
///
/// // Get a specific combo by ID
/// var elementalBurst = comboProvider.GetCombo("elemental-burst");
/// </code>
/// </example>
public interface IComboProvider
{
    /// <summary>
    /// Gets a combo definition by its unique identifier.
    /// </summary>
    /// <param name="comboId">The combo identifier to look up.</param>
    /// <returns>The combo definition, or null if not found.</returns>
    /// <remarks>
    /// <para>Lookup is case-insensitive.</para>
    /// <para>Returns null for unknown combo IDs rather than throwing.</para>
    /// </remarks>
    ComboDefinition? GetCombo(string comboId);

    /// <summary>
    /// Gets all loaded combo definitions.
    /// </summary>
    /// <returns>A read-only list of all combo definitions.</returns>
    /// <remarks>
    /// <para>Returns an empty list if no combos are loaded.</para>
    /// <para>Results are not filtered by class or any other criteria.</para>
    /// </remarks>
    IReadOnlyList<ComboDefinition> GetAllCombos();

    /// <summary>
    /// Gets combo definitions available to a specific class.
    /// </summary>
    /// <param name="classId">The class identifier to filter by.</param>
    /// <returns>Combos available to the specified class.</returns>
    /// <remarks>
    /// <para>Includes combos with no class restriction (available to all).</para>
    /// <para>Class matching is case-insensitive.</para>
    /// </remarks>
    IReadOnlyList<ComboDefinition> GetCombosForClass(string classId);

    /// <summary>
    /// Gets combo definitions that contain a specific ability in any step.
    /// </summary>
    /// <param name="abilityId">The ability identifier to search for.</param>
    /// <returns>Combos that include the specified ability.</returns>
    /// <remarks>
    /// <para>Searches all steps of all combos.</para>
    /// <para>Ability matching is case-insensitive.</para>
    /// </remarks>
    IReadOnlyList<ComboDefinition> GetCombosContaining(string abilityId);

    /// <summary>
    /// Gets combo definitions that start with a specific ability.
    /// </summary>
    /// <param name="abilityId">The ability identifier to search for.</param>
    /// <returns>Combos whose first step requires the specified ability.</returns>
    /// <remarks>
    /// <para>Only checks the first step of each combo.</para>
    /// <para>Used by combo detection to efficiently find potential combos.</para>
    /// <para>Ability matching is case-insensitive.</para>
    /// </remarks>
    IReadOnlyList<ComboDefinition> GetCombosStartingWith(string abilityId);
}
