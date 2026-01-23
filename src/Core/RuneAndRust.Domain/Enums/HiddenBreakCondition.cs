namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Conditions that can break the [Hidden] status.
/// </summary>
/// <remarks>
/// <para>
/// When a character with [Hidden] status triggers one of these conditions,
/// their [Hidden] status is immediately removed unless modified by abilities.
/// </para>
/// </remarks>
public enum HiddenBreakCondition
{
    /// <summary>
    /// Making any attack breaks [Hidden].
    /// </summary>
    /// <remarks>
    /// Exception: [Ghostly Form] ability allows staying hidden after attacking once per encounter.
    /// </remarks>
    Attack = 0,

    /// <summary>
    /// Performing a loud action breaks [Hidden].
    /// </summary>
    /// <example>
    /// Yelling, breaking objects, casting loud spells, triggering alarms.
    /// </example>
    LoudAction = 1,

    /// <summary>
    /// Enemy scores a critical success on Perception check.
    /// </summary>
    /// <remarks>
    /// Enemy must be actively searching and roll net ≥ 5 on Perception.
    /// </remarks>
    EnemyCriticalPerception = 2,

    /// <summary>
    /// Leaving the zone that granted [Hidden] (for zone-specific abilities).
    /// </summary>
    /// <remarks>
    /// Used by [One with the Static] when leaving [Psychic Resonance] zones.
    /// </remarks>
    LeaveZone = 3,

    /// <summary>
    /// Moving at more than half speed.
    /// </summary>
    /// <remarks>
    /// Running or dashing while hidden may break stealth in some contexts.
    /// </remarks>
    FastMovement = 4,

    /// <summary>
    /// Entering a brightly illuminated area.
    /// </summary>
    /// <remarks>
    /// Moving from shadow into full light may automatically break [Hidden].
    /// </remarks>
    EnterLight = 5,

    /// <summary>
    /// End of combat or encounter.
    /// </summary>
    /// <remarks>
    /// [Hidden] status typically resets when combat ends.
    /// </remarks>
    EncounterEnd = 6
}
