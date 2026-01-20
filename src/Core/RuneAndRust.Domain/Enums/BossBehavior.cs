namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the combat behavior patterns for boss encounters.
/// </summary>
/// <remarks>
/// <para>
/// BossBehavior determines how a boss prioritizes actions during combat:
/// <list type="bullet">
///   <item><description><see cref="Aggressive"/>: Maximizes damage output</description></item>
///   <item><description><see cref="Tactical"/>: Balances offense and defense strategically</description></item>
///   <item><description><see cref="Defensive"/>: Prioritizes survival and mitigation</description></item>
///   <item><description><see cref="Enraged"/>: All-out attack ignoring self-preservation</description></item>
///   <item><description><see cref="Summoner"/>: Focuses on spawning and supporting minions</description></item>
/// </list>
/// </para>
/// <para>
/// Behaviors typically change between boss phases as health thresholds are crossed.
/// For example, a boss might start <see cref="Tactical"/> and become <see cref="Enraged"/>
/// when entering a low-health phase.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var phase = new BossPhase
/// {
///     PhaseNumber = 2,
///     HealthThreshold = 50,
///     Behavior = BossBehavior.Enraged
/// };
/// </code>
/// </example>
public enum BossBehavior
{
    /// <summary>
    /// Boss focuses on maximizing damage output.
    /// </summary>
    /// <remarks>
    /// Aggressive bosses prioritize high-damage abilities and direct attacks.
    /// They may ignore tactical advantages in favor of raw damage.
    /// </remarks>
    Aggressive,

    /// <summary>
    /// Boss uses abilities strategically based on situation.
    /// </summary>
    /// <remarks>
    /// Tactical bosses evaluate the combat state and choose optimal abilities.
    /// They balance offense and defense based on current conditions.
    /// </remarks>
    Tactical,

    /// <summary>
    /// Boss prioritizes self-preservation and damage mitigation.
    /// </summary>
    /// <remarks>
    /// Defensive bosses favor healing, shields, and damage reduction.
    /// They may delay attacking to maintain defensive buffs.
    /// </remarks>
    Defensive,

    /// <summary>
    /// Boss enters an all-out attack mode, ignoring defense.
    /// </summary>
    /// <remarks>
    /// Enraged bosses deal increased damage but take more damage themselves.
    /// This behavior is typically triggered at low health thresholds.
    /// </remarks>
    Enraged,

    /// <summary>
    /// Boss focuses on summoning and supporting minions.
    /// </summary>
    /// <remarks>
    /// Summoner bosses prioritize spawning adds and buffing them.
    /// Direct attacks are secondary to maintaining minion presence.
    /// </remarks>
    Summoner
}
