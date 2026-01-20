// ═══════════════════════════════════════════════════════════════════════════════
// StatisticsMetrics.cs
// Record containing calculated statistics metrics derived from raw player statistics.
// Version: 0.12.0a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Models;

/// <summary>
/// Calculated statistics metrics derived from raw statistics.
/// </summary>
/// <remarks>
/// <para>
/// These metrics provide meaningful averages and rates calculated from the raw
/// statistics stored in <see cref="Domain.Entities.PlayerStatistics"/>. All rates
/// are expressed as values between 0.0 and 1.0 (representing 0% to 100%).
/// </para>
/// <para>Calculation formulas:</para>
/// <list type="bullet">
///   <item><description><see cref="AverageDamagePerHit"/>: TotalDamageDealt / (TotalAttacks - AttacksMissed)</description></item>
///   <item><description><see cref="AverageDamageReceived"/>: TotalDamageReceived / HitsTaken (derived)</description></item>
///   <item><description><see cref="CriticalHitRate"/>: CriticalHits / TotalAttacks</description></item>
///   <item><description><see cref="MissRate"/>: AttacksMissed / TotalAttacks</description></item>
///   <item><description><see cref="TrapAvoidanceRate"/>: TrapsAvoided / (TrapsTriggered + TrapsAvoided)</description></item>
///   <item><description><see cref="GoldBalance"/>: GoldEarned - GoldSpent</description></item>
///   <item><description><see cref="AverageSessionLength"/>: TotalPlaytime / SessionCount</description></item>
/// </list>
/// <para>
/// All division operations handle zero denominators by returning 0.0.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Get metrics from the statistics service
/// var metrics = statisticsService.GetMetrics(player);
///
/// // Display combat statistics
/// Console.WriteLine($"Average Damage: {metrics.AverageDamagePerHit:F1}");
/// Console.WriteLine($"Critical Hit Rate: {metrics.CriticalHitRate:P1}");
/// Console.WriteLine($"Miss Rate: {metrics.MissRate:P1}");
/// Console.WriteLine($"Combat Rating: {metrics.CombatRating}");
///
/// // Check trap avoidance
/// if (metrics.TrapAvoidanceRate > 0.75)
/// {
///     Console.WriteLine("Excellent trap awareness!");
/// }
/// </code>
/// </example>
/// <param name="AverageDamagePerHit">
/// Average damage dealt per successful attack.
/// Calculated as TotalDamageDealt divided by the number of hits (attacks minus misses).
/// Returns 0.0 if no successful hits have been made.
/// </param>
/// <param name="AverageDamageReceived">
/// Average damage received per hit taken.
/// Returns 0.0 if no damage has been received.
/// </param>
/// <param name="CriticalHitRate">
/// Rate of critical hits as a decimal between 0.0 and 1.0.
/// Calculated as CriticalHits divided by TotalAttacks.
/// Returns 0.0 if no attacks have been made.
/// </param>
/// <param name="MissRate">
/// Rate of missed attacks as a decimal between 0.0 and 1.0.
/// Calculated as AttacksMissed divided by TotalAttacks.
/// Returns 0.0 if no attacks have been made.
/// </param>
/// <param name="TrapAvoidanceRate">
/// Rate of successfully avoided traps as a decimal between 0.0 and 1.0.
/// Calculated as TrapsAvoided divided by total trap encounters.
/// Returns 0.0 if no traps have been encountered.
/// </param>
/// <param name="GoldBalance">
/// Net gold balance calculated as GoldEarned minus GoldSpent.
/// Can be negative if the player has spent more than earned.
/// </param>
/// <param name="AverageSessionLength">
/// Average duration of play sessions.
/// Calculated as TotalPlaytime divided by SessionCount.
/// Returns <see cref="TimeSpan.Zero"/> if no sessions have been recorded.
/// </param>
/// <param name="CombatRating">
/// Overall combat performance rating tier.
/// Calculated from K/D ratio, critical hit rate, miss rate, and boss kills.
/// See <see cref="Models.CombatRating"/> for tier definitions.
/// </param>
public record StatisticsMetrics(
    double AverageDamagePerHit,
    double AverageDamageReceived,
    double CriticalHitRate,
    double MissRate,
    double TrapAvoidanceRate,
    long GoldBalance,
    TimeSpan AverageSessionLength,
    CombatRating CombatRating)
{
    /// <summary>
    /// Gets a default empty metrics instance with zero values.
    /// </summary>
    /// <remarks>
    /// Useful for new players who have no statistics yet.
    /// All numeric values are 0 and CombatRating is <see cref="CombatRating.Novice"/>.
    /// </remarks>
    public static StatisticsMetrics Empty => new(
        AverageDamagePerHit: 0.0,
        AverageDamageReceived: 0.0,
        CriticalHitRate: 0.0,
        MissRate: 0.0,
        TrapAvoidanceRate: 0.0,
        GoldBalance: 0,
        AverageSessionLength: TimeSpan.Zero,
        CombatRating: CombatRating.Novice);

    /// <summary>
    /// Gets the critical hit rate as a percentage string.
    /// </summary>
    /// <example>
    /// A CriticalHitRate of 0.15 returns "15.0%".
    /// </example>
    public string CriticalHitRateDisplay => $"{CriticalHitRate * 100:F1}%";

    /// <summary>
    /// Gets the miss rate as a percentage string.
    /// </summary>
    /// <example>
    /// A MissRate of 0.08 returns "8.0%".
    /// </example>
    public string MissRateDisplay => $"{MissRate * 100:F1}%";

    /// <summary>
    /// Gets the trap avoidance rate as a percentage string.
    /// </summary>
    /// <example>
    /// A TrapAvoidanceRate of 0.75 returns "75.0%".
    /// </example>
    public string TrapAvoidanceRateDisplay => $"{TrapAvoidanceRate * 100:F1}%";

    /// <summary>
    /// Gets the hit rate (inverse of miss rate) as a decimal between 0.0 and 1.0.
    /// </summary>
    /// <remarks>
    /// Calculated as 1.0 minus <see cref="MissRate"/>.
    /// </remarks>
    public double HitRate => 1.0 - MissRate;

    /// <summary>
    /// Gets the hit rate as a percentage string.
    /// </summary>
    public string HitRateDisplay => $"{HitRate * 100:F1}%";

    /// <summary>
    /// Gets whether the player has any recorded combat activity.
    /// </summary>
    /// <remarks>
    /// Returns true if AverageDamagePerHit is greater than 0, indicating
    /// at least one successful attack has been recorded.
    /// </remarks>
    public bool HasCombatActivity => AverageDamagePerHit > 0;

    /// <summary>
    /// Gets whether the player has a positive gold balance.
    /// </summary>
    public bool HasPositiveGoldBalance => GoldBalance > 0;

    /// <summary>
    /// Gets the average session length formatted as a human-readable string.
    /// </summary>
    /// <example>
    /// Returns formats like "45m", "1h 30m", "2h 15m 30s".
    /// </example>
    public string AverageSessionLengthDisplay
    {
        get
        {
            if (AverageSessionLength == TimeSpan.Zero)
            {
                return "0m";
            }

            var parts = new List<string>();
            if (AverageSessionLength.Hours > 0)
            {
                parts.Add($"{AverageSessionLength.Hours}h");
            }
            if (AverageSessionLength.Minutes > 0)
            {
                parts.Add($"{AverageSessionLength.Minutes}m");
            }
            if (AverageSessionLength.Seconds > 0 && AverageSessionLength.TotalMinutes < 60)
            {
                parts.Add($"{AverageSessionLength.Seconds}s");
            }

            return parts.Count > 0 ? string.Join(" ", parts) : "0m";
        }
    }
}
