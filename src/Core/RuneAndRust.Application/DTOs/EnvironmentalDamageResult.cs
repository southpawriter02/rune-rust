using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Result of applying environmental damage to a character.
/// </summary>
/// <remarks>
/// <para>
/// This record captures all details of an environmental damage application,
/// including the resistance check outcome, damage dealt, and whether
/// mitigations were applied.
/// </para>
/// </remarks>
/// <param name="CharacterId">Target character ID.</param>
/// <param name="ConditionType">The environmental condition that triggered damage.</param>
/// <param name="CheckPassed">Whether the resistance check succeeded.</param>
/// <param name="CheckRoll">The resistance check roll value.</param>
/// <param name="CheckDc">The resistance check DC.</param>
/// <param name="DamageDealt">Damage dealt (0 if check passed).</param>
/// <param name="DamageType">Type of damage applied.</param>
/// <param name="MitigationApplied">Whether character had mitigation.</param>
public sealed record EnvironmentalDamageResult(
    Guid CharacterId,
    EnvironmentalConditionType ConditionType,
    bool CheckPassed,
    int CheckRoll,
    int CheckDc,
    int DamageDealt,
    string DamageType,
    bool MitigationApplied)
{
    /// <summary>
    /// Gets whether damage was actually dealt.
    /// </summary>
    public bool DamageWasDealt => DamageDealt > 0;

    /// <summary>
    /// Gets the margin of success/failure on the check.
    /// </summary>
    /// <remarks>
    /// Positive = success margin, Negative = failure margin.
    /// </remarks>
    public int CheckMargin => CheckRoll - CheckDc;

    /// <summary>
    /// Creates a result for a passed check (no damage).
    /// </summary>
    public static EnvironmentalDamageResult Passed(
        Guid characterId,
        EnvironmentalConditionType conditionType,
        int checkRoll,
        int checkDc,
        string damageType,
        bool mitigationApplied = false) => new(
            characterId,
            conditionType,
            CheckPassed: true,
            checkRoll,
            checkDc,
            DamageDealt: 0,
            damageType,
            mitigationApplied);

    /// <summary>
    /// Creates a result for a failed check (damage dealt).
    /// </summary>
    public static EnvironmentalDamageResult Failed(
        Guid characterId,
        EnvironmentalConditionType conditionType,
        int checkRoll,
        int checkDc,
        int damageDealt,
        string damageType,
        bool mitigationApplied = false) => new(
            characterId,
            conditionType,
            CheckPassed: false,
            checkRoll,
            checkDc,
            damageDealt,
            damageType,
            mitigationApplied);

    /// <inheritdoc/>
    public override string ToString() =>
        CheckPassed
            ? $"EnvDamage[{ConditionType}]: Check PASSED (rolled {CheckRoll} vs DC {CheckDc})"
            : $"EnvDamage[{ConditionType}]: {DamageDealt} {DamageType} (rolled {CheckRoll} vs DC {CheckDc})";
}
