// ═══════════════════════════════════════════════════════════════════════════════
// ArchetypeApplicationResult.cs
// Value object capturing the result of applying an archetype to a character,
// including all resource bonuses applied, abilities granted, available
// specializations set, and any failure reasons.
// Version: 0.17.3f
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.ValueObjects;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the result of applying an archetype to a character during creation.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ArchetypeApplicationResult"/> captures comprehensive information about
/// what was granted when an archetype was applied during character creation Step 4.
/// This enables UI feedback, logging, and character creation summaries.
/// </para>
/// <para>
/// Use the factory methods <see cref="Succeeded"/> and <see cref="Failed"/> to create
/// instances rather than constructing directly. This ensures consistent initialization
/// of all properties.
/// </para>
/// <para>
/// <strong>Success Result:</strong> Contains the applied archetype, resource bonuses,
/// granted abilities, and available specializations. <see cref="Success"/> is <c>true</c>
/// and <see cref="FailureReason"/> is <c>null</c>.
/// </para>
/// <para>
/// <strong>Failure Result:</strong> Contains a <see cref="FailureReason"/> explaining why
/// the archetype could not be applied. <see cref="AbilitiesGranted"/> is empty, and
/// nullable properties are <c>null</c>. <see cref="Success"/> is <c>false</c>.
/// </para>
/// <para>
/// Archetype selection is a <strong>permanent choice</strong> that cannot be changed after
/// character creation. The service validates this precondition before application.
/// </para>
/// </remarks>
/// <seealso cref="ArchetypeResourceBonuses"/>
/// <seealso cref="ArchetypeAbilityGrant"/>
/// <seealso cref="ArchetypeSpecializationMapping"/>
/// <seealso cref="Archetype"/>
public sealed class ArchetypeApplicationResult
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether the archetype was applied successfully.
    /// </summary>
    /// <value>
    /// <c>true</c> if all archetype grants were applied without errors;
    /// <c>false</c> if the archetype could not be applied (e.g., character
    /// already has an archetype, definition not found, null character).
    /// </value>
    public bool Success { get; }

    /// <summary>
    /// Gets the archetype that was applied, if successful.
    /// </summary>
    /// <value>
    /// The <see cref="Archetype"/> enum value identifying which archetype
    /// was applied; or <c>null</c> if the application failed.
    /// </value>
    public Archetype? AppliedArchetype { get; }

    /// <summary>
    /// Gets the resource bonuses that were applied to the character.
    /// </summary>
    /// <value>
    /// The <see cref="ArchetypeResourceBonuses"/> containing HP, Stamina,
    /// Aether Pool, Movement, and Special bonuses applied; or <c>null</c>
    /// if the application failed.
    /// </value>
    /// <remarks>
    /// <para>
    /// Resource bonuses vary by archetype:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Warrior: HP +49, Stamina +5</description></item>
    ///   <item><description>Skirmisher: HP +30, Stamina +5, Movement +1</description></item>
    ///   <item><description>Mystic: HP +20, Aether Pool +20</description></item>
    ///   <item><description>Adept: HP +30, +20% Consumable Effectiveness</description></item>
    /// </list>
    /// </remarks>
    public ArchetypeResourceBonuses? ResourceBonusesApplied { get; }

    /// <summary>
    /// Gets the abilities that were granted to the character.
    /// </summary>
    /// <value>
    /// A read-only list of <see cref="ArchetypeAbilityGrant"/> records describing
    /// each ability granted. Each archetype provides exactly 3 starting abilities.
    /// Empty on failure.
    /// </value>
    /// <remarks>
    /// <para>
    /// Each archetype grants 3 starting abilities of varying types:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Warrior: Power Strike (Active), Defensive Stance (Stance), Iron Will (Passive)</description></item>
    ///   <item><description>Skirmisher: Quick Strike (Active), Evasive Maneuvers (Active), Opportunist (Passive)</description></item>
    ///   <item><description>Mystic: Aether Bolt (Active), Aether Shield (Active), Aether Sense (Passive)</description></item>
    ///   <item><description>Adept: Precise Strike (Active), Assess Weakness (Active), Resourceful (Passive)</description></item>
    /// </list>
    /// </remarks>
    public IReadOnlyList<ArchetypeAbilityGrant> AbilitiesGranted { get; }

    /// <summary>
    /// Gets the available specializations for the applied archetype.
    /// </summary>
    /// <value>
    /// The <see cref="ArchetypeSpecializationMapping"/> containing the available
    /// specialization IDs and recommended first choice for character creation
    /// Step 5; or <c>null</c> if the application failed.
    /// </value>
    /// <remarks>
    /// Specialization count varies by archetype: Warrior (6), Skirmisher (4),
    /// Mystic (2), Adept (5).
    /// </remarks>
    public ArchetypeSpecializationMapping? AvailableSpecializations { get; }

    /// <summary>
    /// Gets the failure reason, if the application was unsuccessful.
    /// </summary>
    /// <value>
    /// A human-readable description of why the archetype application failed;
    /// or <c>null</c> if the application was successful.
    /// </value>
    public string? FailureReason { get; }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Private constructor enforcing use of factory methods.
    /// </summary>
    /// <param name="success">Whether the application succeeded.</param>
    /// <param name="appliedArchetype">The archetype applied, if successful.</param>
    /// <param name="resourceBonusesApplied">Resource bonuses applied, if successful.</param>
    /// <param name="abilitiesGranted">Abilities granted to the character.</param>
    /// <param name="availableSpecializations">Specializations available for Step 5.</param>
    /// <param name="failureReason">Failure reason, if unsuccessful.</param>
    private ArchetypeApplicationResult(
        bool success,
        Archetype? appliedArchetype,
        ArchetypeResourceBonuses? resourceBonusesApplied,
        IReadOnlyList<ArchetypeAbilityGrant> abilitiesGranted,
        ArchetypeSpecializationMapping? availableSpecializations,
        string? failureReason)
    {
        Success = success;
        AppliedArchetype = appliedArchetype;
        ResourceBonusesApplied = resourceBonusesApplied;
        AbilitiesGranted = abilitiesGranted;
        AvailableSpecializations = availableSpecializations;
        FailureReason = failureReason;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a successful application result with all granted data.
    /// </summary>
    /// <param name="archetype">The archetype that was applied.</param>
    /// <param name="bonuses">The resource bonuses that were applied.</param>
    /// <param name="abilities">The starting abilities that were granted.</param>
    /// <param name="specializations">The specializations now available.</param>
    /// <returns>
    /// A <see cref="ArchetypeApplicationResult"/> with <see cref="Success"/> set to
    /// <c>true</c> and all grant details populated.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = ArchetypeApplicationResult.Succeeded(
    ///     Archetype.Warrior,
    ///     ArchetypeResourceBonuses.Warrior,
    ///     warriorAbilities,
    ///     ArchetypeSpecializationMapping.Warrior);
    /// </code>
    /// </example>
    public static ArchetypeApplicationResult Succeeded(
        Archetype archetype,
        ArchetypeResourceBonuses bonuses,
        IReadOnlyList<ArchetypeAbilityGrant> abilities,
        ArchetypeSpecializationMapping specializations) =>
        new(
            success: true,
            appliedArchetype: archetype,
            resourceBonusesApplied: bonuses,
            abilitiesGranted: abilities,
            availableSpecializations: specializations,
            failureReason: null);

    /// <summary>
    /// Creates a failed application result with a reason.
    /// </summary>
    /// <param name="reason">A human-readable description of why the application failed.</param>
    /// <returns>
    /// A <see cref="ArchetypeApplicationResult"/> with <see cref="Success"/> set to
    /// <c>false</c> and empty grant details.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = ArchetypeApplicationResult.Failed("Character cannot be null");
    /// </code>
    /// </example>
    public static ArchetypeApplicationResult Failed(string reason) =>
        new(
            success: false,
            appliedArchetype: null,
            resourceBonusesApplied: null,
            abilitiesGranted: Array.Empty<ArchetypeAbilityGrant>(),
            availableSpecializations: null,
            failureReason: reason);

    /// <summary>
    /// Creates a failed result indicating the character already has an archetype.
    /// </summary>
    /// <param name="existing">The archetype already assigned to the character.</param>
    /// <returns>
    /// A failed <see cref="ArchetypeApplicationResult"/> with a descriptive message
    /// indicating that archetype is a permanent choice.
    /// </returns>
    /// <remarks>
    /// Archetype is a permanent choice that cannot be changed after character creation.
    /// This factory method produces a user-friendly failure message for that scenario.
    /// </remarks>
    public static ArchetypeApplicationResult AlreadyHasArchetype(Archetype existing) =>
        Failed($"Character already has archetype: {existing}. Archetype is a permanent choice.");

    /// <summary>
    /// Gets a failed result indicating that a character is required.
    /// </summary>
    /// <value>
    /// A pre-built failed <see cref="ArchetypeApplicationResult"/> for null character scenarios.
    /// </value>
    public static ArchetypeApplicationResult CharacterRequired =>
        Failed("A character is required to apply an archetype.");

    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a display-friendly summary of the application result.
    /// </summary>
    /// <returns>
    /// A formatted summary string. On success, includes the applied archetype name,
    /// resource bonus summary, ability count, and specialization count. On failure,
    /// includes the failure reason.
    /// </returns>
    /// <example>
    /// <code>
    /// // Success:
    /// // "Applied Warrior:
    /// //   Resource Bonuses: +49 HP, +5 Stamina
    /// //   Abilities Granted: 3
    /// //   Specializations Available: 6"
    /// //
    /// // Failure:
    /// // "Failed: Character already has archetype: Skirmisher. Archetype is a permanent choice."
    /// </code>
    /// </example>
    public string GetSummary()
    {
        if (!Success)
            return $"Failed: {FailureReason}";

        return $"Applied {AppliedArchetype}:\n" +
               $"  Resource Bonuses: {ResourceBonusesApplied?.GetDisplaySummary()}\n" +
               $"  Abilities Granted: {AbilitiesGranted.Count}\n" +
               $"  Specializations Available: {AvailableSpecializations?.Count}";
    }

    /// <summary>
    /// Gets a formatted list of granted ability display strings.
    /// </summary>
    /// <returns>
    /// A read-only list of formatted ability strings (e.g., "Power Strike [ACTIVE]").
    /// Returns an empty list if no abilities were granted.
    /// </returns>
    /// <example>
    /// <code>
    /// var abilities = result.GetGrantedAbilitiesList();
    /// // ["Power Strike [ACTIVE]", "Defensive Stance [STANCE]", "Iron Will [PASSIVE]"]
    /// </code>
    /// </example>
    public IReadOnlyList<string> GetGrantedAbilitiesList() =>
        AbilitiesGranted.Select(a => a.GetShortDisplay()).ToList();

    /// <summary>
    /// Gets a formatted list of available specialization display strings.
    /// </summary>
    /// <returns>
    /// A read-only list of formatted specialization names with the recommended
    /// choice highlighted. Returns an empty list if no specializations are available.
    /// </returns>
    /// <example>
    /// <code>
    /// var specs = result.GetSpecializationsList();
    /// // ["★ Guardian (Recommended)", "Berserker", "Weapon Master", ...]
    /// </code>
    /// </example>
    public IReadOnlyList<string> GetSpecializationsList() =>
        AvailableSpecializations?.GetDisplayList() ?? Array.Empty<string>();
}
