namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the result of a fall, including height and damage calculation parameters.
/// </summary>
/// <remarks>
/// <para>
/// Fall results are created when a character falls from climbing, leaping, or other sources.
/// The actual damage calculation is handled by the Fall Damage System (v0.15.2c).
/// </para>
/// <para>
/// Damage formula (calculated in v0.15.2c):
/// <list type="bullet">
///   <item><description>Damage = (Height / 10) × 1d10</description></item>
///   <item><description>Maximum = 10d10 (100+ feet)</description></item>
///   <item><description>Crash Landing DC = 12 + (Height / 10)</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="FallHeight">The height fallen in feet.</param>
/// <param name="Source">The source of the fall.</param>
/// <param name="DamageDice">The number of d10 damage dice (calculated from height).</param>
/// <param name="CrashLandingDc">The DC for a Crash Landing attempt.</param>
/// <param name="CanAttemptCrashLanding">Whether a Crash Landing attempt is possible.</param>
/// <param name="TriggeredByFumble">Whether the fall was triggered by a fumble.</param>
/// <param name="BonusDamage">Any bonus damage dice from fumble effects.</param>
public readonly record struct FallResult(
    int FallHeight,
    FallSource Source,
    int DamageDice,
    int CrashLandingDc,
    bool CanAttemptCrashLanding = true,
    bool TriggeredByFumble = false,
    int BonusDamage = 0)
{
    /// <summary>
    /// Gets a value indicating whether the fall would cause damage.
    /// </summary>
    /// <remarks>
    /// Falls from less than 10 feet typically don't cause damage.
    /// </remarks>
    public bool CausesDamage => DamageDice > 0;

    /// <summary>
    /// Gets the damage type for the fall (always Bludgeoning).
    /// </summary>
    public string DamageType => "Bludgeoning";

    /// <summary>
    /// Gets the maximum damage dice (capped at 10d10).
    /// </summary>
    public const int MaxDamageDice = 10;

    /// <summary>
    /// Creates a FallResult from a height in feet.
    /// </summary>
    /// <param name="heightFeet">The height fallen in feet.</param>
    /// <param name="source">The source of the fall.</param>
    /// <param name="triggeredByFumble">Whether the fall was triggered by a fumble.</param>
    /// <returns>A new FallResult with calculated damage parameters.</returns>
    /// <example>
    /// <code>
    /// var fallResult = FallResult.FromHeight(30, FallSource.Climbing);
    /// // DamageDice = 3, CrashLandingDc = 15
    /// </code>
    /// </example>
    public static FallResult FromHeight(
        int heightFeet,
        FallSource source,
        bool triggeredByFumble = true)
    {
        var damageDice = Math.Min(MaxDamageDice, heightFeet / 10);
        var crashLandingDc = 12 + (heightFeet / 10);

        return new FallResult(
            FallHeight: heightFeet,
            Source: source,
            DamageDice: damageDice,
            CrashLandingDc: crashLandingDc,
            CanAttemptCrashLanding: heightFeet >= 10,
            TriggeredByFumble: triggeredByFumble);
    }

    /// <summary>
    /// Creates a FallResult for [The Long Fall] fumble with bonus damage.
    /// </summary>
    /// <param name="heightFeet">The height fallen in feet.</param>
    /// <param name="bonusDamageDice">Additional damage dice from the fumble.</param>
    /// <returns>A new FallResult with bonus damage applied.</returns>
    /// <remarks>
    /// [The Long Fall] is triggered by leaping fumbles and adds bonus damage
    /// plus the [Disoriented] status for 2 rounds.
    /// </remarks>
    public static FallResult TheLongFall(int heightFeet, int bonusDamageDice = 1)
    {
        var result = FromHeight(heightFeet, FallSource.Leaping, triggeredByFumble: true);
        return result with { BonusDamage = bonusDamageDice };
    }

    /// <summary>
    /// Gets a formatted description of the fall result.
    /// </summary>
    /// <returns>A human-readable description of the fall.</returns>
    public string ToDescription()
    {
        if (!CausesDamage)
        {
            return $"Fell {FallHeight}ft (no damage)";
        }

        var damageStr = BonusDamage > 0
            ? $"{DamageDice}d10 + {BonusDamage}d10 bonus"
            : $"{DamageDice}d10";

        return $"Fell {FallHeight}ft - {damageStr} {DamageType} damage (Crash Landing DC {CrashLandingDc})";
    }

    /// <inheritdoc/>
    public override string ToString() => ToDescription();
}

/// <summary>
/// Identifies the source of a fall for tracking and narrative purposes.
/// </summary>
public enum FallSource
{
    /// <summary>
    /// Fall from a climbing attempt.
    /// </summary>
    /// <remarks>
    /// Triggered by [The Slip] fumble during climbing.
    /// </remarks>
    Climbing = 0,

    /// <summary>
    /// Fall from a failed leap attempt.
    /// </summary>
    /// <remarks>
    /// May trigger [The Long Fall] with bonus damage.
    /// </remarks>
    Leaping = 1,

    /// <summary>
    /// Fall from a balance check failure.
    /// </summary>
    Balance = 2,

    /// <summary>
    /// Fall caused by external force (push, knockback).
    /// </summary>
    Pushed = 3,

    /// <summary>
    /// Fall from terrain collapse or trap.
    /// </summary>
    Environmental = 4
}
