namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the complete result of fall damage processing.
/// </summary>
/// <remarks>
/// <para>
/// This aggregates the FallDamage, CrashLandingResult, and final damage
/// into a single result object for the complete fall sequence.
/// </para>
/// <para>
/// Processing flow:
/// <list type="number">
///   <item><description>Calculate FallDamage from height</description></item>
///   <item><description>Attempt CrashLanding if eligible</description></item>
///   <item><description>Roll final damage dice</description></item>
///   <item><description>Create FallDamageResult with all data</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="FallDamage">The calculated fall damage parameters.</param>
/// <param name="CrashLanding">The Crash Landing attempt result.</param>
/// <param name="FinalDamageDice">The final damage dice after reduction.</param>
/// <param name="DamageRolled">The actual damage rolled.</param>
/// <param name="DamageType">The damage type applied.</param>
/// <param name="CharacterId">The ID of the character who fell.</param>
public readonly record struct FallDamageResult(
    FallDamage FallDamage,
    CrashLandingResult CrashLanding,
    int FinalDamageDice,
    int DamageRolled,
    string DamageType,
    string CharacterId)
{
    /// <summary>
    /// Gets a value indicating whether any damage was taken.
    /// </summary>
    public bool TookDamage => DamageRolled > 0;

    /// <summary>
    /// Gets a value indicating whether Crash Landing helped.
    /// </summary>
    public bool CrashLandingHelped => CrashLanding.ReducedDamage;

    /// <summary>
    /// Gets the amount of damage avoided through Crash Landing.
    /// </summary>
    /// <remarks>
    /// Estimated as 5.5 average per die reduced.
    /// </remarks>
    public double EstimatedDamageAvoided => CrashLanding.DiceReduced * 5.5;

    /// <summary>
    /// Gets the fall height in feet.
    /// </summary>
    public int FallHeight => FallDamage.FallHeight;

    /// <summary>
    /// Gets a value indicating whether Crash Landing was attempted.
    /// </summary>
    public bool CrashLandingAttempted => CrashLanding.WasAttempted;

    /// <summary>
    /// Gets a value indicating whether all damage was avoided.
    /// </summary>
    public bool AvoidedAllDamage => !TookDamage;

    /// <summary>
    /// Creates a FallDamageResult for a fall that caused no damage.
    /// </summary>
    /// <param name="fallDamage">The fall damage (no damage dice).</param>
    /// <param name="characterId">The character ID.</param>
    /// <returns>A result indicating no damage taken.</returns>
    public static FallDamageResult NoDamage(FallDamage fallDamage, string characterId)
    {
        return new FallDamageResult(
            FallDamage: fallDamage,
            CrashLanding: CrashLandingResult.NoAttempt(0),
            FinalDamageDice: 0,
            DamageRolled: 0,
            DamageType: "Bludgeoning",
            CharacterId: characterId);
    }

    /// <summary>
    /// Creates a FallDamageResult with the specified parameters.
    /// </summary>
    /// <param name="fallDamage">The calculated fall damage.</param>
    /// <param name="crashLanding">The Crash Landing result.</param>
    /// <param name="damageRolled">The actual damage rolled.</param>
    /// <param name="characterId">The character ID.</param>
    /// <returns>A complete FallDamageResult.</returns>
    public static FallDamageResult Create(
        FallDamage fallDamage,
        CrashLandingResult crashLanding,
        int damageRolled,
        string characterId)
    {
        return new FallDamageResult(
            FallDamage: fallDamage,
            CrashLanding: crashLanding,
            FinalDamageDice: crashLanding.FinalDamageDice,
            DamageRolled: damageRolled,
            DamageType: fallDamage.DamageType,
            CharacterId: characterId);
    }

    /// <summary>
    /// Gets a description of the complete fall damage result.
    /// </summary>
    /// <returns>A multi-line description of the fall outcome.</returns>
    public string ToDescription()
    {
        var lines = new List<string>
        {
            $"=== FALL DAMAGE RESULT ===",
            $"Character: {CharacterId}",
            FallDamage.ToDescription()
        };

        if (CrashLanding.WasAttempted)
        {
            lines.Add(CrashLanding.ToDescription());
        }

        if (TookDamage)
        {
            lines.Add($"DAMAGE TAKEN: {DamageRolled} {DamageType}");
        }
        else
        {
            lines.Add("NO DAMAGE TAKEN");
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Gets a compact summary for log output.
    /// </summary>
    /// <returns>A compact result summary.</returns>
    public string ToSummary()
    {
        if (!TookDamage)
        {
            return $"{CharacterId}: {FallHeight}ft fall - no damage";
        }

        var crashStr = CrashLanding.WasAttempted
            ? $" (Crash: {CrashLanding.ToSummary()})"
            : "";

        return $"{CharacterId}: {FallHeight}ft fall - {DamageRolled} damage{crashStr}";
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"Fall {FallDamage.FallHeight}ft: {DamageRolled} damage ({CharacterId})";
}
