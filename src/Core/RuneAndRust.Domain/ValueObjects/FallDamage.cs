namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the calculated damage from a fall.
/// </summary>
/// <remarks>
/// <para>
/// Fall damage is calculated as 1d10 per 10 feet fallen, capped at 10d10 for
/// falls of 100 feet or more. This value object encapsulates the damage
/// calculation and provides Crash Landing eligibility information.
/// </para>
/// <para>
/// Damage Formula:
/// <list type="bullet">
///   <item><description>Damage Dice = Height / 10 (rounded down)</description></item>
///   <item><description>Maximum Damage Dice = 10</description></item>
///   <item><description>Damage Type = Bludgeoning</description></item>
///   <item><description>Crash Landing available if Height >= 10ft</description></item>
/// </list>
/// </para>
/// <para>
/// Crash Landing DC (success-counting):
/// <list type="bullet">
///   <item><description>DC = 2 + (Height / 10) successes needed</description></item>
///   <item><description>Each success above DC reduces damage by 1d10</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="FallHeight">The height fallen in feet.</param>
/// <param name="DamageDice">The number of d10 damage dice (1 per 10ft, max 10).</param>
/// <param name="DamageType">The damage type (always Bludgeoning for falls).</param>
/// <param name="CanCrashLand">Whether a Crash Landing attempt is possible.</param>
/// <param name="BonusDamageDice">Additional damage dice from fumble effects.</param>
/// <param name="Source">The source of the fall for narrative purposes.</param>
public readonly record struct FallDamage(
    int FallHeight,
    int DamageDice,
    string DamageType,
    bool CanCrashLand,
    int BonusDamageDice = 0,
    FallSource Source = FallSource.Environmental)
{
    /// <summary>
    /// Maximum damage dice from falling (10d10 at 100+ feet).
    /// </summary>
    public const int MaxDamageDice = 10;

    /// <summary>
    /// Minimum height in feet to cause damage.
    /// </summary>
    public const int MinDamageHeight = 10;

    /// <summary>
    /// Height increment for each damage die.
    /// </summary>
    public const int HeightPerDie = 10;

    /// <summary>
    /// Base DC component for Crash Landing (success-counting system).
    /// </summary>
    public const int CrashLandingBaseDc = 2;

    /// <summary>
    /// Gets the total damage dice including any bonus dice.
    /// </summary>
    public int TotalDamageDice => DamageDice + BonusDamageDice;

    /// <summary>
    /// Gets a value indicating whether the fall causes damage.
    /// </summary>
    public bool CausesDamage => TotalDamageDice > 0;

    /// <summary>
    /// Gets the Crash Landing DC for this fall (success-counting system).
    /// </summary>
    /// <remarks>
    /// DC = BaseDC + (Height / 10) = 2 + (Height / 10) successes needed.
    /// </remarks>
    public int CrashLandingDc => CanCrashLand
        ? CrashLandingBaseDc + (FallHeight / HeightPerDie)
        : 0;

    /// <summary>
    /// Gets the average expected damage (5.5 per d10).
    /// </summary>
    public double AverageDamage => TotalDamageDice * 5.5;

    /// <summary>
    /// Gets the maximum possible damage.
    /// </summary>
    public int MaximumDamage => TotalDamageDice * 10;

    /// <summary>
    /// Gets the minimum possible damage (1 per die, or 0 if no damage).
    /// </summary>
    public int MinimumDamage => CausesDamage ? TotalDamageDice : 0;

    /// <summary>
    /// Creates a FallDamage from a height in feet.
    /// </summary>
    /// <param name="heightFeet">The height fallen in feet.</param>
    /// <param name="source">The source of the fall.</param>
    /// <param name="bonusDice">Additional damage dice from fumble effects.</param>
    /// <returns>A new FallDamage with calculated parameters.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when heightFeet is negative.
    /// </exception>
    /// <example>
    /// <code>
    /// var damage = FallDamage.FromHeight(30, FallSource.Climbing);
    /// // DamageDice = 3, CrashLandingDc = 5
    /// </code>
    /// </example>
    public static FallDamage FromHeight(
        int heightFeet,
        FallSource source = FallSource.Environmental,
        int bonusDice = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(heightFeet);

        var damageDice = Math.Min(MaxDamageDice, heightFeet / HeightPerDie);
        var canCrashLand = heightFeet >= MinDamageHeight;

        return new FallDamage(
            FallHeight: heightFeet,
            DamageDice: damageDice,
            DamageType: "Bludgeoning",
            CanCrashLand: canCrashLand,
            BonusDamageDice: bonusDice,
            Source: source);
    }

    /// <summary>
    /// Creates a FallDamage from an existing FallResult.
    /// </summary>
    /// <param name="fallResult">The fall result from climbing or leaping.</param>
    /// <returns>A new FallDamage with parameters derived from the FallResult.</returns>
    public static FallDamage FromFallResult(FallResult fallResult)
    {
        return new FallDamage(
            FallHeight: fallResult.FallHeight,
            DamageDice: fallResult.DamageDice,
            DamageType: fallResult.DamageType,
            CanCrashLand: fallResult.CanAttemptCrashLanding,
            BonusDamageDice: fallResult.BonusDamage,
            Source: fallResult.Source);
    }

    /// <summary>
    /// Creates a FallDamage for [The Long Fall] fumble with bonus damage.
    /// </summary>
    /// <param name="heightFeet">The height fallen in feet.</param>
    /// <returns>A new FallDamage with +1d10 bonus damage.</returns>
    public static FallDamage TheLongFall(int heightFeet)
    {
        return FromHeight(heightFeet, FallSource.Leaping, bonusDice: 1);
    }

    /// <summary>
    /// Creates a FallDamage for [The Slip] fumble from climbing.
    /// </summary>
    /// <param name="heightFeet">The height fallen in feet.</param>
    /// <returns>A new FallDamage from climbing source.</returns>
    public static FallDamage TheSlip(int heightFeet)
    {
        return FromHeight(heightFeet, FallSource.Climbing);
    }

    /// <summary>
    /// Creates a FallDamage with reduced dice after Crash Landing.
    /// </summary>
    /// <param name="diceReduction">Number of dice to reduce.</param>
    /// <returns>A new FallDamage with reduced damage dice (minimum 0).</returns>
    public FallDamage WithDiceReduction(int diceReduction)
    {
        var newDamageDice = Math.Max(0, DamageDice - diceReduction);
        return this with { DamageDice = newDamageDice };
    }

    /// <summary>
    /// Gets a description of the fall damage for display.
    /// </summary>
    /// <returns>A human-readable damage description.</returns>
    public string ToDescription()
    {
        if (!CausesDamage)
        {
            return $"Fell {FallHeight}ft (no damage - below {MinDamageHeight}ft threshold)";
        }

        var diceStr = TotalDamageDice == DamageDice
            ? $"{DamageDice}d10"
            : $"{DamageDice}d10 + {BonusDamageDice}d10 bonus";

        var crashStr = CanCrashLand
            ? $" (Crash Landing DC {CrashLandingDc})"
            : "";

        return $"Fell {FallHeight}ft - {diceStr} {DamageType} damage{crashStr}";
    }

    /// <summary>
    /// Gets a compact summary for log output.
    /// </summary>
    /// <returns>A compact damage summary string.</returns>
    public string ToSummary()
    {
        if (!CausesDamage)
        {
            return $"{FallHeight}ft (safe)";
        }

        return $"{FallHeight}ft → {TotalDamageDice}d10";
    }

    /// <inheritdoc/>
    public override string ToString() => ToDescription();
}
