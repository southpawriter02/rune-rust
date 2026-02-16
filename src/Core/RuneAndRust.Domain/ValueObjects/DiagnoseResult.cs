using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the outcome of a Diagnose ability execution.
/// Contains detailed analysis of a target's health status, active conditions,
/// vulnerabilities, and resistances.
/// </summary>
/// <remarks>
/// <para>Diagnose is the Bone-Setter's Tier 1 information-gathering ability:</para>
/// <list type="bullet">
/// <item>Cost: 1 AP, no Medical Supplies required</item>
/// <item>Range: 5 spaces with line of sight</item>
/// <item>Reveals: HP, wound severity, status effects, vulnerabilities, resistances</item>
/// <item>Targets: Any character (ally, enemy, or neutral)</item>
/// <item>Corruption: None (Coherent path)</item>
/// </list>
/// <para>The diagnostic information persists until the target is defeated or
/// the encounter ends, enabling informed healing and tactical decisions.</para>
/// </remarks>
public sealed record DiagnoseResult
{
    /// <summary>
    /// Unique identifier of the diagnosed target.
    /// </summary>
    public Guid TargetId { get; init; }

    /// <summary>
    /// Display name of the diagnosed target.
    /// </summary>
    public string TargetName { get; init; } = string.Empty;

    /// <summary>
    /// Target's current HP at time of diagnosis.
    /// </summary>
    public int CurrentHp { get; init; }

    /// <summary>
    /// Target's maximum HP.
    /// </summary>
    public int MaxHp { get; init; }

    /// <summary>
    /// Current HP as a fraction of max HP (0.0–1.0).
    /// Computed from <see cref="CurrentHp"/> / <see cref="MaxHp"/>.
    /// </summary>
    public float HpPercentage => MaxHp > 0 ? (float)CurrentHp / MaxHp : 0f;

    /// <summary>
    /// Whether the target is considered "bloodied" (at or below 50% HP).
    /// Used for tactical decision-making and Triage priority (v0.20.6b).
    /// </summary>
    public bool IsBloodied => HpPercentage <= 0.5f;

    /// <summary>
    /// Classification of wound severity based on HP percentage.
    /// See <see cref="WoundSeverity"/> for threshold definitions.
    /// </summary>
    public WoundSeverity WoundSeverity { get; init; }

    /// <summary>
    /// Collection of active status effects on the target (buffs, debuffs, conditions).
    /// Empty if no active effects.
    /// </summary>
    public IReadOnlyList<string> StatusEffects { get; init; } = [];

    /// <summary>
    /// Collection of known damage type or condition vulnerabilities.
    /// Empty if no known vulnerabilities.
    /// </summary>
    public IReadOnlyList<string> Vulnerabilities { get; init; } = [];

    /// <summary>
    /// Collection of known damage type or condition resistances.
    /// Empty if no known resistances.
    /// </summary>
    public IReadOnlyList<string> Resistances { get; init; } = [];

    /// <summary>
    /// Returns a human-readable description of the target's health status.
    /// Example: "Seriously wounded (32%)" or "Unconscious".
    /// </summary>
    /// <returns>A health description string based on wound severity.</returns>
    public string GetHealthDescription() =>
        WoundSeverity switch
        {
            WoundSeverity.Minor => $"Healthy ({HpPercentage:P0})",
            WoundSeverity.Light => $"Lightly wounded ({HpPercentage:P0})",
            WoundSeverity.Moderate => $"Moderately wounded ({HpPercentage:P0})",
            WoundSeverity.Serious => $"Seriously wounded ({HpPercentage:P0})",
            WoundSeverity.Critical => $"CRITICAL ({HpPercentage:P0})",
            WoundSeverity.Unconscious => "Unconscious",
            _ => "Unknown"
        };

    /// <summary>
    /// Returns a summary of all active status effects.
    /// Returns "None" if no effects are active.
    /// </summary>
    /// <returns>A comma-separated list of status effects, or "None".</returns>
    public string GetStatusSummary() =>
        StatusEffects.Count > 0 ? string.Join(", ", StatusEffects) : "None";

    /// <summary>
    /// Checks if the target has any active status effects.
    /// </summary>
    /// <returns>True if at least one status effect is active.</returns>
    public bool HasActiveEffects() =>
        StatusEffects.Count > 0;

    /// <summary>
    /// Formats the complete diagnosis as a multi-line display string for combat output.
    /// Includes HP, severity, status effects, vulnerabilities, and resistances.
    /// </summary>
    /// <returns>A formatted multi-line diagnostic report.</returns>
    public string GetFormattedOutput()
    {
        var lines = new List<string>
        {
            $"\u2550\u2550\u2550 Diagnosis: {TargetName} \u2550\u2550\u2550",
            $"HP: {CurrentHp}/{MaxHp} ({HpPercentage:P0}){(IsBloodied ? " [BLOODIED]" : "")}",
            $"Status: {GetHealthDescription()}",
            $"Wound Severity: {WoundSeverity}"
        };

        if (StatusEffects.Count > 0)
            lines.Add($"Afflictions: {GetStatusSummary()}");

        if (Vulnerabilities.Count > 0)
            lines.Add($"Vulnerabilities: {string.Join(", ", Vulnerabilities)}");

        if (Resistances.Count > 0)
            lines.Add($"Resistances: {string.Join(", ", Resistances)}");

        lines.Add("\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550");

        return string.Join("\n", lines);
    }
}
