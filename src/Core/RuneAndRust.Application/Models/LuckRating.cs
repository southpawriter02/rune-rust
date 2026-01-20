// ═══════════════════════════════════════════════════════════════════════════════
// LuckRating.cs
// Enum representing the player's overall luck rating based on dice roll history.
// Version: 0.12.0b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Models;

/// <summary>
/// Represents the player's overall luck rating based on dice roll history.
/// </summary>
/// <remarks>
/// <para>
/// Luck rating is calculated from the deviation of the player's actual
/// d20 average from the expected average of 10.5. The formula used is:
/// </para>
/// <code>
/// LuckPercentage = ((ActualAverage - 10.5) / 10.5) × 100
/// </code>
/// <para>
/// The thresholds for each rating tier are:
/// </para>
/// <list type="bullet">
///   <item><description><see cref="Cursed"/>: More than 10% below average (luck &lt; -10%)</description></item>
///   <item><description><see cref="Unlucky"/>: 5-10% below average (-10% &lt;= luck &lt; -5%)</description></item>
///   <item><description><see cref="Average"/>: Within 5% of expected (-5% &lt;= luck &lt;= +5%)</description></item>
///   <item><description><see cref="Lucky"/>: 5-10% above average (+5% &lt; luck &lt;= +10%)</description></item>
///   <item><description><see cref="Blessed"/>: More than 10% above average (luck &gt; +10%)</description></item>
/// </list>
/// <para>
/// This enum provides a fun meta-game element showing players whether they've been
/// "blessed" or "cursed" by the dice gods during their gameplay sessions.
/// </para>
/// </remarks>
public enum LuckRating
{
    /// <summary>
    /// Very unlucky: actual average is more than 10% below expected.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Display suggestion: "😢 CURSED"
    /// </para>
    /// <para>
    /// This rating indicates the player has been experiencing exceptionally
    /// poor luck with their d20 rolls, averaging below 9.45 on a d20.
    /// </para>
    /// </remarks>
    Cursed,

    /// <summary>
    /// Unlucky: actual average is 5-10% below expected.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Display suggestion: "😟 Unlucky"
    /// </para>
    /// <para>
    /// This rating indicates the player has been experiencing below-average
    /// luck, with an average between 9.45 and 9.975 on a d20.
    /// </para>
    /// </remarks>
    Unlucky,

    /// <summary>
    /// Average: actual average is within 5% of expected.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Display suggestion: "😐 Average"
    /// </para>
    /// <para>
    /// This rating indicates the player's rolls are within normal statistical
    /// variance, with an average between 9.975 and 11.025 on a d20.
    /// </para>
    /// </remarks>
    Average,

    /// <summary>
    /// Lucky: actual average is 5-10% above expected.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Display suggestion: "🍀 Lucky"
    /// </para>
    /// <para>
    /// This rating indicates the player has been experiencing above-average
    /// luck, with an average between 11.025 and 11.55 on a d20.
    /// </para>
    /// </remarks>
    Lucky,

    /// <summary>
    /// Very lucky: actual average is more than 10% above expected.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Display suggestion: "🌟 BLESSED"
    /// </para>
    /// <para>
    /// This rating indicates the player has been experiencing exceptionally
    /// good luck with their d20 rolls, averaging above 11.55 on a d20.
    /// </para>
    /// </remarks>
    Blessed
}
