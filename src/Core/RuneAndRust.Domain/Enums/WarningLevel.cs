// ═══════════════════════════════════════════════════════════════════════════════
// WarningLevel.cs
// Enum representing the warning level for trauma economy state.
// Used by TraumaEconomyState.GetWarningLevel() for UI/narrative guidance.
// Version: 0.18.5a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the warning level for trauma economy state assessment.
/// </summary>
/// <remarks>
/// <para>
/// WarningLevel provides a simplified severity indicator for the combined state
/// of all trauma economy systems. It is used for:
/// </para>
/// <list type="bullet">
///   <item><description>UI color coding and visual alerts</description></item>
///   <item><description>Narrative prompt generation</description></item>
///   <item><description>Priority ranking of status effects</description></item>
/// </list>
/// <para>
/// The level is determined by the highest severity across all systems (Stress,
/// Corruption, CPS), not an average. Any single system reaching critical or
/// terminal state elevates the entire warning level.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var state = traumaEconomyService.GetState(characterId);
/// var level = state.GetWarningLevel();
/// 
/// switch (level)
/// {
///     case WarningLevel.Critical:
///         DisplayWarningBanner("Your character is in danger!");
///         break;
///     case WarningLevel.Terminal:
///         DisplayUrgentModal("Immediate action required!");
///         break;
/// }
/// </code>
/// </example>
/// <seealso cref="Entities.TraumaEconomyState"/>
public enum WarningLevel
{
    /// <summary>
    /// All trauma economy systems are within normal operating parameters.
    /// </summary>
    /// <remarks>
    /// No systems at 70 or above. Character is stable.
    /// </remarks>
    None = 0,

    /// <summary>
    /// One or more systems are approaching dangerous thresholds.
    /// </summary>
    /// <remarks>
    /// At least one system (Stress or Corruption) is at 70-79.
    /// Character should consider rest or recovery actions.
    /// </remarks>
    Warning = 1,

    /// <summary>
    /// One or more systems are at or exceeding critical thresholds.
    /// </summary>
    /// <remarks>
    /// <para>
    /// At least one system (Stress or Corruption) is at 80-99, OR
    /// CPS requires a Panic Check (RuinMadness stage or higher).
    /// </para>
    /// <para>
    /// Character is in immediate danger and should prioritize recovery.
    /// Defense penalties and skill disadvantages may be severe.
    /// </para>
    /// </remarks>
    Critical = 2,

    /// <summary>
    /// One or more systems have reached terminal state (100).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Stress at 100 triggers a Trauma Check.
    /// Corruption at 100 triggers Terminal Error (character may be lost).
    /// </para>
    /// <para>
    /// This is the highest severity level. Immediate intervention required
    /// or character may suffer permanent consequences.
    /// </para>
    /// </remarks>
    Terminal = 3
}
