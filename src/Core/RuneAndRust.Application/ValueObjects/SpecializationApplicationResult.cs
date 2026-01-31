// ═══════════════════════════════════════════════════════════════════════════════
// SpecializationApplicationResult.cs
// Value object capturing the result of applying a specialization to a character,
// including granted Tier 1 abilities, special resource initialization, Heretical
// path warnings, PP cost, and any failure reasons.
// Version: 0.17.4e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.ValueObjects;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the result of applying a specialization to a character during creation.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="SpecializationApplicationResult"/> captures comprehensive information about
/// what was granted when a specialization was applied during character creation Step 5
/// (Specialization Selection) or during progression via PP spending.
/// </para>
/// <para>
/// Use the factory methods <see cref="Succeeded"/> and <see cref="Failed"/> to create
/// instances rather than constructing directly. This ensures consistent initialization
/// of all properties.
/// </para>
/// <para>
/// <strong>Success Result:</strong> Contains the applied specialization definition, granted
/// Tier 1 abilities, special resource initialization status, Heretical path warning, and
/// PP cost. <see cref="Success"/> is <c>true</c> and <see cref="FailureReason"/> is <c>null</c>.
/// </para>
/// <para>
/// <strong>Failure Result:</strong> Contains a <see cref="FailureReason"/> explaining why
/// the specialization could not be applied. <see cref="GrantedAbilities"/> is empty, and
/// nullable properties are <c>null</c>. <see cref="Success"/> is <c>false</c>.
/// </para>
/// <para>
/// Specialization application grants:
/// <list type="bullet">
///   <item><description>Tier 1 abilities (3 abilities, free at selection)</description></item>
///   <item><description>Special resource initialization if the specialization has one (e.g., Rage for Berserkr)</description></item>
///   <item><description>Heretical path warning if the specialization interfaces with corrupted Aether</description></item>
/// </list>
/// </para>
/// </remarks>
/// <seealso cref="SpecializationDefinition"/>
/// <seealso cref="SpecializationAbility"/>
/// <seealso cref="SpecializationId"/>
/// <seealso cref="TierUnlockResult"/>
public sealed class SpecializationApplicationResult
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether the specialization was applied successfully.
    /// </summary>
    /// <value>
    /// <c>true</c> if the specialization was applied and all Tier 1 abilities were granted;
    /// <c>false</c> if the specialization could not be applied (e.g., wrong archetype,
    /// already has specialization, insufficient PP, definition not found).
    /// </value>
    public bool Success { get; }

    /// <summary>
    /// Gets the specialization ID that was applied, if successful.
    /// </summary>
    /// <value>
    /// The <see cref="SpecializationId"/> enum value identifying which specialization
    /// was applied; or <c>null</c> if the application failed.
    /// </value>
    public SpecializationId? AppliedSpecializationId { get; }

    /// <summary>
    /// Gets the full specialization definition that was applied, if successful.
    /// </summary>
    /// <value>
    /// The <see cref="SpecializationDefinition"/> containing display metadata, path type,
    /// special resource, and ability tiers; or <c>null</c> if the application failed.
    /// </value>
    public SpecializationDefinition? AppliedDefinition { get; }

    /// <summary>
    /// Gets the Tier 1 abilities that were granted to the character.
    /// </summary>
    /// <value>
    /// A read-only list of <see cref="SpecializationAbility"/> records describing
    /// each ability granted. Each specialization grants 3 Tier 1 abilities on selection.
    /// Empty on failure.
    /// </value>
    public IReadOnlyList<SpecializationAbility> GrantedAbilities { get; }

    /// <summary>
    /// Gets whether a special resource was initialized for the character.
    /// </summary>
    /// <value>
    /// <c>true</c> if the specialization has a special resource (e.g., Rage, Block Charges)
    /// and it was initialized on the character; <c>false</c> otherwise.
    /// </value>
    /// <remarks>
    /// 5 of 17 specializations have special resources: Berserkr (Rage), Skjaldmaer
    /// (Block Charges), Iron-Bane (Righteous Fervor), Seiðkona (Aether Resonance),
    /// Echo-Caller (Echoes).
    /// </remarks>
    public bool SpecialResourceInitialized { get; }

    /// <summary>
    /// Gets whether the applied specialization follows a Heretical path.
    /// </summary>
    /// <value>
    /// <c>true</c> if the specialization is Heretical (interfacing with corrupted Aether,
    /// abilities may trigger Corruption gain); <c>false</c> for Coherent or if application failed.
    /// </value>
    public bool IsHeretical { get; }

    /// <summary>
    /// Gets the Corruption warning message for Heretical specializations.
    /// </summary>
    /// <value>
    /// A human-readable warning about Corruption risks for Heretical specializations;
    /// or <c>null</c> for Coherent specializations or if the application failed.
    /// </value>
    public string? CorruptionWarning { get; }

    /// <summary>
    /// Gets the Progression Point cost that was deducted for this application.
    /// </summary>
    /// <value>
    /// 0 for the first specialization (free at creation), 3 for additional
    /// specializations. 0 on failure.
    /// </value>
    public int PpCost { get; }

    /// <summary>
    /// Gets the failure reason, if the application was unsuccessful.
    /// </summary>
    /// <value>
    /// A human-readable description of why the specialization application failed;
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
    /// <param name="appliedSpecializationId">The specialization ID applied, if successful.</param>
    /// <param name="appliedDefinition">The full definition applied, if successful.</param>
    /// <param name="grantedAbilities">Tier 1 abilities granted to the character.</param>
    /// <param name="specialResourceInitialized">Whether a special resource was initialized.</param>
    /// <param name="isHeretical">Whether the specialization is Heretical.</param>
    /// <param name="corruptionWarning">Corruption warning for Heretical paths.</param>
    /// <param name="ppCost">The PP cost deducted.</param>
    /// <param name="failureReason">Failure reason, if unsuccessful.</param>
    private SpecializationApplicationResult(
        bool success,
        SpecializationId? appliedSpecializationId,
        SpecializationDefinition? appliedDefinition,
        IReadOnlyList<SpecializationAbility> grantedAbilities,
        bool specialResourceInitialized,
        bool isHeretical,
        string? corruptionWarning,
        int ppCost,
        string? failureReason)
    {
        Success = success;
        AppliedSpecializationId = appliedSpecializationId;
        AppliedDefinition = appliedDefinition;
        GrantedAbilities = grantedAbilities;
        SpecialResourceInitialized = specialResourceInitialized;
        IsHeretical = isHeretical;
        CorruptionWarning = corruptionWarning;
        PpCost = ppCost;
        FailureReason = failureReason;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a successful application result with all granted data.
    /// </summary>
    /// <param name="definition">The specialization definition that was applied.</param>
    /// <param name="grantedAbilities">The Tier 1 abilities that were granted.</param>
    /// <param name="specialResourceInitialized">Whether a special resource was initialized.</param>
    /// <param name="ppCost">The PP cost that was deducted (0 for first, 3 for additional).</param>
    /// <returns>
    /// A <see cref="SpecializationApplicationResult"/> with <see cref="Success"/> set to
    /// <c>true</c> and all grant details populated.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = SpecializationApplicationResult.Succeeded(
    ///     berserkrDefinition,
    ///     tier1Abilities,
    ///     specialResourceInitialized: true,
    ///     ppCost: 0);
    /// </code>
    /// </example>
    public static SpecializationApplicationResult Succeeded(
        SpecializationDefinition definition,
        IReadOnlyList<SpecializationAbility> grantedAbilities,
        bool specialResourceInitialized,
        int ppCost) =>
        new(
            success: true,
            appliedSpecializationId: definition.SpecializationId,
            appliedDefinition: definition,
            grantedAbilities: grantedAbilities,
            specialResourceInitialized: specialResourceInitialized,
            isHeretical: definition.IsHeretical,
            corruptionWarning: definition.GetCorruptionWarning(),
            ppCost: ppCost,
            failureReason: null);

    /// <summary>
    /// Creates a failed application result with a reason.
    /// </summary>
    /// <param name="reason">A human-readable description of why the application failed.</param>
    /// <returns>
    /// A <see cref="SpecializationApplicationResult"/> with <see cref="Success"/> set to
    /// <c>false</c> and empty grant details.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = SpecializationApplicationResult.Failed("Requires Warrior archetype");
    /// </code>
    /// </example>
    public static SpecializationApplicationResult Failed(string reason) =>
        new(
            success: false,
            appliedSpecializationId: null,
            appliedDefinition: null,
            grantedAbilities: Array.Empty<SpecializationAbility>(),
            specialResourceInitialized: false,
            isHeretical: false,
            corruptionWarning: null,
            ppCost: 0,
            failureReason: reason);

    /// <summary>
    /// Creates a failed result indicating the character already has this specialization.
    /// </summary>
    /// <param name="existing">The specialization already assigned to the character.</param>
    /// <returns>
    /// A failed <see cref="SpecializationApplicationResult"/> with a descriptive message.
    /// </returns>
    public static SpecializationApplicationResult AlreadyHasSpecialization(
        SpecializationId existing) =>
        Failed($"Character already has specialization: {existing}.");

    /// <summary>
    /// Gets a failed result indicating that a character is required.
    /// </summary>
    /// <value>
    /// A pre-built failed <see cref="SpecializationApplicationResult"/> for null character scenarios.
    /// </value>
    public static SpecializationApplicationResult CharacterRequired =>
        Failed("A character is required to apply a specialization.");

    /// <summary>
    /// Creates a failed result indicating archetype mismatch.
    /// </summary>
    /// <param name="required">The required archetype for the specialization.</param>
    /// <param name="actual">The character's current archetype.</param>
    /// <returns>
    /// A failed <see cref="SpecializationApplicationResult"/> with a descriptive message.
    /// </returns>
    public static SpecializationApplicationResult ArchetypeMismatch(
        Archetype required, string? actual) =>
        Failed($"Requires {required} archetype (character has: {actual ?? "none"}).");

    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a display-friendly summary of the application result.
    /// </summary>
    /// <returns>
    /// A formatted summary string. On success, includes the applied specialization name,
    /// path type, granted ability count, special resource status, and PP cost. On failure,
    /// includes the failure reason.
    /// </returns>
    /// <example>
    /// <code>
    /// // Success:
    /// // "Applied Berserkr (Heretical):
    /// //   Abilities Granted: 3
    /// //   Special Resource: Rage (initialized)
    /// //   PP Cost: 0
    /// //   ⚠ WARNING: Heretical path — abilities may trigger Corruption gain"
    /// //
    /// // Failure:
    /// // "Failed: Requires Warrior archetype"
    /// </code>
    /// </example>
    public string GetSummary()
    {
        if (!Success)
            return $"Failed: {FailureReason}";

        var summary = $"Applied {AppliedDefinition?.DisplayName} ({AppliedDefinition?.PathType}):\n" +
                      $"  Abilities Granted: {GrantedAbilities.Count}\n" +
                      $"  Special Resource: {(SpecialResourceInitialized ? AppliedDefinition?.SpecialResource.ToString() + " (initialized)" : "None")}\n" +
                      $"  PP Cost: {PpCost}";

        if (IsHeretical && CorruptionWarning != null)
        {
            summary += $"\n  WARNING: {CorruptionWarning}";
        }

        return summary;
    }

    /// <summary>
    /// Gets a formatted list of granted ability display strings.
    /// </summary>
    /// <returns>
    /// A read-only list of formatted ability strings (e.g., "Rage Strike [ACTIVE]").
    /// Returns an empty list if no abilities were granted.
    /// </returns>
    /// <example>
    /// <code>
    /// var abilities = result.GetGrantedAbilitiesList();
    /// // ["Rage Strike [ACTIVE]", "Blood Frenzy [ACTIVE]", "Primal Toughness [PASSIVE]"]
    /// </code>
    /// </example>
    public IReadOnlyList<string> GetGrantedAbilitiesList() =>
        GrantedAbilities.Select(a => a.GetShortDisplay()).ToList();
}
