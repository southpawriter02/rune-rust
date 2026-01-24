// ------------------------------------------------------------------------------
// <copyright file="BypassAbilityActivationResult.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Represents the result of activating a bypass specialization ability,
// including success/failure status, effects applied, and narrative text.
// Part of v0.15.4i Specialization Integration implementation.
// </summary>
// ------------------------------------------------------------------------------

using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the result of activating a bypass specialization ability.
/// </summary>
/// <remarks>
/// <para>
/// This value object captures all information about an ability activation:
/// <list type="bullet">
///   <item><description>Whether the activation succeeded</description></item>
///   <item><description>The ability that was activated</description></item>
///   <item><description>Effects that were applied</description></item>
///   <item><description>Narrative description of the result</description></item>
///   <item><description>Any additional data (e.g., detected traps, crafted items)</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="Success">Whether the ability activated successfully.</param>
/// <param name="AbilityId">The ID of the ability that was activated.</param>
/// <param name="AbilityName">The display name of the ability.</param>
/// <param name="EffectType">The type of effect that was applied.</param>
/// <param name="EffectMagnitude">Numeric value of the effect (if applicable).</param>
/// <param name="FailureReason">Reason for failure, if not successful.</param>
/// <param name="NarrativeText">Descriptive text for the activation result.</param>
/// <param name="AdditionalData">Extra data specific to the ability (e.g., detected trap IDs).</param>
public readonly record struct BypassAbilityActivationResult(
    bool Success,
    string AbilityId,
    string AbilityName,
    BypassEffectType EffectType,
    int EffectMagnitude,
    string FailureReason,
    string NarrativeText,
    IReadOnlyDictionary<string, object> AdditionalData)
{
    // =========================================================================
    // COMPUTED PROPERTIES
    // =========================================================================

    /// <summary>
    /// Gets a value indicating whether the activation failed.
    /// </summary>
    public bool Failed => !Success;

    /// <summary>
    /// Gets a value indicating whether there is additional data.
    /// </summary>
    public bool HasAdditionalData => AdditionalData.Count > 0;

    /// <summary>
    /// Gets a value indicating whether there is a failure reason.
    /// </summary>
    public bool HasFailureReason => !string.IsNullOrEmpty(FailureReason);

    // =========================================================================
    // STATIC FACTORY METHODS - SUCCESS RESULTS
    // =========================================================================

    /// <summary>
    /// Creates a successful passive ability activation result.
    /// </summary>
    /// <param name="ability">The ability that was activated.</param>
    /// <param name="narrative">Optional narrative override.</param>
    /// <returns>A successful activation result.</returns>
    /// <remarks>
    /// <para>
    /// Used for passive abilities that activate automatically when their
    /// conditions are met. The effect is applied immediately.
    /// </para>
    /// </remarks>
    public static BypassAbilityActivationResult PassiveActivated(
        BypassSpecializationAbility ability,
        string? narrative = null)
    {
        return new BypassAbilityActivationResult(
            Success: true,
            AbilityId: ability.AbilityId,
            AbilityName: ability.Name,
            EffectType: ability.EffectType,
            EffectMagnitude: ability.EffectMagnitude,
            FailureReason: string.Empty,
            NarrativeText: narrative ?? $"{ability.Name} applies its effect.",
            AdditionalData: new Dictionary<string, object>());
    }

    /// <summary>
    /// Creates a successful triggered ability activation result.
    /// </summary>
    /// <param name="ability">The ability that was triggered.</param>
    /// <param name="narrative">Narrative describing the trigger outcome.</param>
    /// <param name="additionalData">Additional data about the trigger effect.</param>
    /// <returns>A successful trigger activation result.</returns>
    /// <remarks>
    /// <para>
    /// Used for triggered abilities that fire automatically on specific outcomes.
    /// </para>
    /// </remarks>
    public static BypassAbilityActivationResult TriggeredActivated(
        BypassSpecializationAbility ability,
        string narrative,
        IReadOnlyDictionary<string, object>? additionalData = null)
    {
        return new BypassAbilityActivationResult(
            Success: true,
            AbilityId: ability.AbilityId,
            AbilityName: ability.Name,
            EffectType: ability.EffectType,
            EffectMagnitude: ability.EffectMagnitude,
            FailureReason: string.Empty,
            NarrativeText: narrative,
            AdditionalData: additionalData ?? new Dictionary<string, object>());
    }

    /// <summary>
    /// Creates a successful unique action result.
    /// </summary>
    /// <param name="ability">The ability that was activated.</param>
    /// <param name="narrative">Narrative describing the action result.</param>
    /// <param name="additionalData">Additional data about the action.</param>
    /// <returns>A successful unique action result.</returns>
    /// <remarks>
    /// <para>
    /// Used for unique actions that were successfully executed after a check.
    /// </para>
    /// </remarks>
    public static BypassAbilityActivationResult UniqueActionSuccess(
        BypassSpecializationAbility ability,
        string narrative,
        IReadOnlyDictionary<string, object>? additionalData = null)
    {
        return new BypassAbilityActivationResult(
            Success: true,
            AbilityId: ability.AbilityId,
            AbilityName: ability.Name,
            EffectType: ability.EffectType,
            EffectMagnitude: ability.EffectMagnitude,
            FailureReason: string.Empty,
            NarrativeText: narrative,
            AdditionalData: additionalData ?? new Dictionary<string, object>());
    }

    /// <summary>
    /// Creates a result for trap detection via [Sixth Sense].
    /// </summary>
    /// <param name="detectedTrapIds">IDs of traps that were detected.</param>
    /// <param name="characterPositionX">Character X position.</param>
    /// <param name="characterPositionY">Character Y position.</param>
    /// <returns>A trap detection result with detected trap information.</returns>
    /// <remarks>
    /// <para>
    /// Special factory for the [Sixth Sense] ability that returns detected trap IDs.
    /// </para>
    /// </remarks>
    public static BypassAbilityActivationResult SixthSenseDetection(
        IReadOnlyList<string> detectedTrapIds,
        int characterPositionX,
        int characterPositionY)
    {
        var ability = BypassSpecializationAbility.SixthSense();
        var data = new Dictionary<string, object>
        {
            ["detectedTrapIds"] = detectedTrapIds,
            ["characterPositionX"] = characterPositionX,
            ["characterPositionY"] = characterPositionY,
            ["detectionRadius"] = ability.EffectMagnitude
        };

        var narrative = detectedTrapIds.Count switch
        {
            0 => "Your senses detect no hidden dangers nearby.",
            1 => "Your sixth sense tingles... you detect a trap nearby!",
            _ => $"Your sixth sense screams... you detect {detectedTrapIds.Count} traps nearby!"
        };

        return new BypassAbilityActivationResult(
            Success: true,
            AbilityId: ability.AbilityId,
            AbilityName: ability.Name,
            EffectType: ability.EffectType,
            EffectMagnitude: detectedTrapIds.Count,
            FailureReason: string.Empty,
            NarrativeText: narrative,
            AdditionalData: data);
    }

    /// <summary>
    /// Creates a result for [Deep Access] admin upgrade.
    /// </summary>
    /// <param name="terminalId">The terminal that was hacked.</param>
    /// <param name="previousAccessLevel">The access level before upgrade.</param>
    /// <returns>A deep access upgrade result.</returns>
    /// <remarks>
    /// <para>
    /// Special factory for the [Deep Access] ability that upgrades terminal access.
    /// </para>
    /// </remarks>
    public static BypassAbilityActivationResult DeepAccessUpgrade(
        string terminalId,
        int previousAccessLevel)
    {
        var ability = BypassSpecializationAbility.DeepAccess();
        var data = new Dictionary<string, object>
        {
            ["terminalId"] = terminalId,
            ["previousAccessLevel"] = previousAccessLevel,
            ["newAccessLevel"] = 2 // Admin level
        };

        return new BypassAbilityActivationResult(
            Success: true,
            AbilityId: ability.AbilityId,
            AbilityName: ability.Name,
            EffectType: ability.EffectType,
            EffectMagnitude: ability.EffectMagnitude,
            FailureReason: string.Empty,
            NarrativeText: "Your deep communion with the machine grants Admin-Level access. " +
                          "All terminal functions are now available to you.",
            AdditionalData: data);
    }

    // =========================================================================
    // STATIC FACTORY METHODS - FAILURE RESULTS
    // =========================================================================

    /// <summary>
    /// Creates a result for when the character doesn't have the required specialization.
    /// </summary>
    /// <param name="abilityId">The ability that was attempted.</param>
    /// <param name="requiredSpecialization">The specialization that would be required.</param>
    /// <returns>A failure result indicating missing specialization.</returns>
    public static BypassAbilityActivationResult MissingSpecialization(
        string abilityId,
        BypassSpecialization requiredSpecialization)
    {
        return new BypassAbilityActivationResult(
            Success: false,
            AbilityId: abilityId,
            AbilityName: string.Empty,
            EffectType: BypassEffectType.None,
            EffectMagnitude: 0,
            FailureReason: $"Requires {requiredSpecialization} specialization",
            NarrativeText: $"This ability requires the {requiredSpecialization} specialization.",
            AdditionalData: new Dictionary<string, object>());
    }

    /// <summary>
    /// Creates a result for when the unique action check fails.
    /// </summary>
    /// <param name="ability">The ability that was attempted.</param>
    /// <param name="rollResult">The result of the check roll.</param>
    /// <param name="narrative">Failure narrative.</param>
    /// <returns>A failure result for the failed check.</returns>
    public static BypassAbilityActivationResult UniqueActionFailed(
        BypassSpecializationAbility ability,
        int rollResult,
        string narrative)
    {
        var data = new Dictionary<string, object>
        {
            ["rollResult"] = rollResult,
            ["requiredDc"] = ability.CheckDc,
            ["checkAttribute"] = ability.CheckAttribute
        };

        return new BypassAbilityActivationResult(
            Success: false,
            AbilityId: ability.AbilityId,
            AbilityName: ability.Name,
            EffectType: BypassEffectType.None,
            EffectMagnitude: 0,
            FailureReason: $"Check failed (rolled {rollResult} vs DC {ability.CheckDc})",
            NarrativeText: narrative,
            AdditionalData: data);
    }

    /// <summary>
    /// Creates a result for when conditions for the ability are not met.
    /// </summary>
    /// <param name="ability">The ability that was attempted.</param>
    /// <param name="missingCondition">Description of the missing condition.</param>
    /// <returns>A failure result indicating missing conditions.</returns>
    public static BypassAbilityActivationResult ConditionsNotMet(
        BypassSpecializationAbility ability,
        string missingCondition)
    {
        return new BypassAbilityActivationResult(
            Success: false,
            AbilityId: ability.AbilityId,
            AbilityName: ability.Name,
            EffectType: BypassEffectType.None,
            EffectMagnitude: 0,
            FailureReason: missingCondition,
            NarrativeText: $"Cannot activate {ability.Name}: {missingCondition}",
            AdditionalData: new Dictionary<string, object>());
    }

    /// <summary>
    /// Creates a result for when the ability doesn't apply to this bypass type.
    /// </summary>
    /// <param name="ability">The ability that was checked.</param>
    /// <param name="attemptedBypassType">The bypass type that was attempted.</param>
    /// <returns>A failure result indicating wrong bypass type.</returns>
    public static BypassAbilityActivationResult WrongBypassType(
        BypassSpecializationAbility ability,
        BypassType attemptedBypassType)
    {
        return new BypassAbilityActivationResult(
            Success: false,
            AbilityId: ability.AbilityId,
            AbilityName: ability.Name,
            EffectType: BypassEffectType.None,
            EffectMagnitude: 0,
            FailureReason: $"Only applies to {ability.RequiredBypassType}, not {attemptedBypassType}",
            NarrativeText: $"{ability.Name} does not apply to {attemptedBypassType} attempts.",
            AdditionalData: new Dictionary<string, object>());
    }

    // =========================================================================
    // DISPLAY METHODS
    // =========================================================================

    /// <summary>
    /// Creates a display string for the activation result.
    /// </summary>
    /// <returns>A formatted string showing the result.</returns>
    /// <example>
    /// <code>
    /// var result = BypassAbilityActivationResult.SixthSenseDetection(new[] { "trap-001" }, 5, 10);
    /// Console.WriteLine(result.ToDisplayString());
    /// // Output:
    /// // [Sixth Sense] ACTIVATED
    /// // Effect: Auto-Detection (1 traps detected)
    /// // Your sixth sense tingles... you detect a trap nearby!
    /// </code>
    /// </example>
    public string ToDisplayString()
    {
        var lines = new List<string>();

        if (Success)
        {
            lines.Add($"{AbilityName} ACTIVATED");
            lines.Add($"Effect: {FormatEffect()}");
        }
        else
        {
            lines.Add($"{AbilityName} FAILED");
            if (HasFailureReason)
            {
                lines.Add($"Reason: {FailureReason}");
            }
        }

        lines.Add(string.Empty);
        lines.Add(NarrativeText);

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Returns a compact log-friendly string.
    /// </summary>
    /// <returns>A single-line string for logging.</returns>
    public string ToLogString()
    {
        return Success
            ? $"BypassAbility[{AbilityId}] Success: {EffectType} ({EffectMagnitude})"
            : $"BypassAbility[{AbilityId}] Failed: {FailureReason}";
    }

    /// <inheritdoc/>
    public override string ToString() => ToLogString();

    // =========================================================================
    // PRIVATE HELPERS
    // =========================================================================

    /// <summary>
    /// Formats the effect for display.
    /// </summary>
    private string FormatEffect()
    {
        return EffectType switch
        {
            BypassEffectType.AutoDetection => $"Auto-Detection ({EffectMagnitude} detected)",
            BypassEffectType.DcReduction => $"DC Reduction (-{EffectMagnitude})",
            BypassEffectType.TimeReduction => $"Time Reduction (-{EffectMagnitude} rounds)",
            BypassEffectType.PenaltyNegation => "Penalties Negated",
            BypassEffectType.UpgradeResult => "Result Upgraded",
            BypassEffectType.UnlockRecipes => "Recipes Unlocked",
            BypassEffectType.UniqueAction => "Action Completed",
            _ => EffectType.ToString()
        };
    }
}
