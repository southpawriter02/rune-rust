// ═══════════════════════════════════════════════════════════════════════════════
// TraumaCheckTrigger.cs
// Defines the 8 events that can trigger trauma checks.
// Version: 0.18.3c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the events that trigger trauma checks.
/// </summary>
/// <remarks>
/// <para>
/// Trauma checks occur when characters experience stressful or horrific events.
/// Different triggers have different difficulties and result in different trauma types.
/// </para>
/// <para>
/// Trigger Mechanics:
/// <list type="bullet">
/// <item>StressThreshold100: When stress reaches 100 points (DC 3)</item>
/// <item>CorruptionThreshold100: When corruption reaches 100 points (DC 4)</item>
/// <item>AllyDeath: When a party member dies (DC 2)</item>
/// <item>NearDeathExperience: When character is reduced to 1 HP (DC 2)</item>
/// <item>CriticalFailure: When character fails opposed check by large margin (DC 1)</item>
/// <item>ProlongedExposure: When character spends extended time in dangerous location (DC 2)</item>
/// <item>WitnessingHorror: When character witnesses traumatic event (DC 3)</item>
/// <item>RuinMadnessEscape: When character survives Ruin-Madness stage (CPS >= 60) (DC 4)</item>
/// </list>
/// </para>
/// <para>
/// Check Resolution:
/// <list type="bullet">
/// <item>Roll RESOLVE attribute dice pool (base or reduced by corruption)</item>
/// <item>Count successes (each die showing 4+ is one success)</item>
/// <item>Compare to SuccessesNeeded for this trigger</item>
/// <item>If successes &lt; required: character acquires random trauma</item>
/// </list>
/// </para>
/// </remarks>
public enum TraumaCheckTrigger
{
    /// <summary>
    /// Character's psychic stress reaches maximum (100).
    /// </summary>
    /// <remarks>
    /// Difficulty: 3 successes
    /// Trauma Category: Cognitive/Emotional
    /// Result: Trauma from stress-related categories
    /// </remarks>
    StressThreshold100 = 0,

    /// <summary>
    /// Character's corruption reaches maximum (100).
    /// </summary>
    /// <remarks>
    /// Difficulty: 4 successes
    /// Trauma Category: Corruption
    /// Result: Corruption-type trauma
    /// </remarks>
    CorruptionThreshold100 = 1,

    /// <summary>
    /// A party member dies.
    /// </summary>
    /// <remarks>
    /// Difficulty: 2 successes
    /// Trauma Category: Emotional
    /// Result: Emotional trauma (grief, guilt)
    /// </remarks>
    AllyDeath = 2,

    /// <summary>
    /// Character is reduced to 1 HP (near-death experience).
    /// </summary>
    /// <remarks>
    /// Difficulty: 2 successes
    /// Trauma Category: Physical/Emotional
    /// Result: Physical or emotional trauma
    /// </remarks>
    NearDeathExperience = 3,

    /// <summary>
    /// Character critically fails an opposed check.
    /// </summary>
    /// <remarks>
    /// Difficulty: 1 success
    /// Trauma Category: Context-dependent
    /// Result: Trauma related to check type
    /// Note: May not always trigger (GM discretion)
    /// </remarks>
    CriticalFailure = 4,

    /// <summary>
    /// Character spends extended time in a dangerous area.
    /// </summary>
    /// <remarks>
    /// Difficulty: 2 successes
    /// Trauma Category: Existential
    /// Result: Reality/identity crisis trauma
    /// Duration: Exposure time varies by environment
    /// </remarks>
    ProlongedExposure = 5,

    /// <summary>
    /// Character witnesses a horrific or traumatic event.
    /// </summary>
    /// <remarks>
    /// Difficulty: 3 successes
    /// Trauma Category: Cognitive
    /// Result: Cognitive trauma (flashbacks, paranoia)
    /// Examples: Seeing someone disintegrated, ritual sacrifice
    /// </remarks>
    WitnessingHorror = 6,

    /// <summary>
    /// Character survives Ruin-Madness stage (CPS 60-79).
    /// </summary>
    /// <remarks>
    /// Difficulty: 4 successes
    /// Trauma Category: Cognitive/Existential
    /// Result: Severe cognitive or existential trauma
    /// Note: Only triggers if character reaches RuinMadness and escapes
    /// </remarks>
    RuinMadnessEscape = 7
}
