namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Classification of wound severity based on a target's current HP percentage.
/// Used by the Diagnose ability to categorize target health status for tactical assessment.
/// </summary>
/// <remarks>
/// <para>Wound severity thresholds are based on the ratio of current HP to maximum HP:</para>
/// <list type="bullet">
/// <item>Minor: 90–100% HP — Healthy, minimal injuries</item>
/// <item>Light: 70–89% HP — Small cuts and light wounds</item>
/// <item>Moderate: 40–69% HP — Moderate wounds requiring attention</item>
/// <item>Serious: 15–39% HP — Serious wounds, high healing priority</item>
/// <item>Critical: 1–14% HP — Critical condition, death imminent</item>
/// <item>Unconscious: 0% HP — Unconscious or stable at zero HP</item>
/// </list>
/// <para>The "Bloodied" threshold (≤50% HP) overlaps with Moderate and below,
/// and is tracked separately in <see cref="RuneAndRust.Domain.ValueObjects.DiagnoseResult.IsBloodied"/>.</para>
/// </remarks>
public enum WoundSeverity
{
    /// <summary>90–100% HP — Healthy with only minor scratches and bruises.</summary>
    Minor = 0,

    /// <summary>70–89% HP — Small cuts and light wounds, low healing priority.</summary>
    Light = 1,

    /// <summary>40–69% HP — Moderate wounds requiring medical attention.</summary>
    Moderate = 2,

    /// <summary>15–39% HP — Serious wounds, high healing priority.</summary>
    Serious = 3,

    /// <summary>1–14% HP — Critical condition, death is imminent without intervention.</summary>
    Critical = 4,

    /// <summary>0% HP — Unconscious or stable at zero HP, requires revival.</summary>
    Unconscious = 5
}
