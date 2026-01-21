using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// Data transfer object for phase indicator display.
/// </summary>
/// <param name="PhaseNumber">The current phase number (1-based).</param>
/// <param name="PhaseName">The display name of the phase.</param>
/// <param name="Behavior">The boss behavior pattern for this phase.</param>
/// <param name="AbilityHints">Hints about phase-specific abilities.</param>
/// <param name="StatModifiers">Stat multipliers for the phase (stat ID to multiplier).</param>
/// <remarks>
/// <para>Used to transfer phase state from the combat system to the
/// <see cref="UI.PhaseIndicator"/> for rendering.</para>
/// <para>The <paramref name="Behavior"/> determines the color of the phase display
/// and helps players anticipate boss attack patterns.</para>
/// </remarks>
/// <example>
/// <code>
/// var dto = new PhaseDisplayDto(
///     PhaseNumber: 2,
///     PhaseName: "Empowered",
///     Behavior: BossBehavior.Aggressive,
///     AbilityHints: new[] { "Increased damage output", "Watch for AoE attacks" },
///     StatModifiers: new Dictionary&lt;string, float&gt; { { "damage", 1.5f } });
/// </code>
/// </example>
public record PhaseDisplayDto(
    int PhaseNumber,
    string PhaseName,
    BossBehavior Behavior,
    IReadOnlyList<string> AbilityHints,
    IReadOnlyDictionary<string, float> StatModifiers);

/// <summary>
/// Data transfer object for phase transition effect display.
/// </summary>
/// <param name="OldPhaseNumber">The previous phase number.</param>
/// <param name="NewPhaseNumber">The new phase number.</param>
/// <param name="PhaseName">The display name of the new phase.</param>
/// <param name="TransitionText">Optional dramatic transition text.</param>
/// <param name="BossName">The boss display name.</param>
/// <remarks>
/// <para>Used to display a prominent visual notification when the boss
/// transitions to a new phase during combat.</para>
/// <para>The <paramref name="TransitionText"/> is typically flavor text like
/// "Rise, my minions!" that adds atmosphere to the transition.</para>
/// </remarks>
/// <example>
/// <code>
/// var dto = new PhaseTransitionDto(
///     OldPhaseNumber: 1,
///     NewPhaseNumber: 2,
///     PhaseName: "Empowered",
///     TransitionText: "Rise, my minions!",
///     BossName: "Skeleton King");
/// </code>
/// </example>
public record PhaseTransitionDto(
    int OldPhaseNumber,
    int NewPhaseNumber,
    string PhaseName,
    string? TransitionText,
    string BossName);

/// <summary>
/// Data transfer object for enrage status display.
/// </summary>
/// <param name="IsEnraged">Whether the boss is currently in an enraged phase.</param>
/// <param name="HealthPercentToEnrage">Health percentage remaining until next enrage phase (null if no enrage phase exists).</param>
/// <param name="StatModifiers">Stat multipliers when enraged (stat ID to multiplier).</param>
/// <remarks>
/// <para>Enrage in v0.10.4b is phase-based (triggered by health thresholds),
/// not time-based. This DTO tracks the current enrage status and proximity
/// to the next enrage phase.</para>
/// <para>The <paramref name="HealthPercentToEnrage"/> is used to show warnings
/// when the boss is approaching an enrage threshold.</para>
/// </remarks>
/// <example>
/// <code>
/// // Boss is enraged
/// var enragedDto = new EnrageStatusDto(
///     IsEnraged: true,
///     HealthPercentToEnrage: null,
///     StatModifiers: new Dictionary&lt;string, float&gt; { { "damage", 1.5f }, { "attackSpeed", 1.25f } });
///
/// // Boss is approaching enrage
/// var warningDto = new EnrageStatusDto(
///     IsEnraged: false,
///     HealthPercentToEnrage: 8,
///     StatModifiers: new Dictionary&lt;string, float&gt;());
/// </code>
/// </example>
public record EnrageStatusDto(
    bool IsEnraged,
    int? HealthPercentToEnrage,
    IReadOnlyDictionary<string, float> StatModifiers);

/// <summary>
/// Data transfer object for vulnerability window display.
/// </summary>
/// <param name="IsActive">Whether the vulnerability window is currently active.</param>
/// <param name="TurnsRemaining">Number of turns remaining in the window.</param>
/// <param name="DamageMultiplier">The damage multiplier during vulnerability (default 1.5 = +50%).</param>
/// <remarks>
/// <para>The vulnerability window is triggered by <see cref="Application.Interfaces.IBossMechanicsService.SetVulnerable"/>
/// and lasts for a configured number of turns. During this time, the boss takes
/// increased damage (typically 1.5x = 50% bonus).</para>
/// </remarks>
/// <example>
/// <code>
/// // Vulnerability window is active
/// var activeDto = new VulnerabilityDisplayDto(
///     IsActive: true,
///     TurnsRemaining: 2,
///     DamageMultiplier: 1.5f);
///
/// // Vulnerability window is inactive
/// var inactiveDto = new VulnerabilityDisplayDto(
///     IsActive: false,
///     TurnsRemaining: 0,
///     DamageMultiplier: 1.0f);
/// </code>
/// </example>
public record VulnerabilityDisplayDto(
    bool IsActive,
    int TurnsRemaining,
    float DamageMultiplier);
