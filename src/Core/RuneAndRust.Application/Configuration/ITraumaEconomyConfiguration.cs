// ═══════════════════════════════════════════════════════════════════════════════
// ITraumaEconomyConfiguration.cs
// Interface defining the contract for accessing trauma economy configuration
// settings loaded from trauma-economy.json. Provides access to damage integration,
// rest recovery, turn effects, thresholds, and warning message settings.
// Version: 0.18.5e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Configuration;

/// <summary>
/// Provides access to trauma economy configuration settings.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Overview:</strong>
/// ITraumaEconomyConfiguration exposes all configurable values for the trauma
/// economy integration layer. Values are loaded from <c>trauma-economy.json</c>
/// and provide sensible defaults when not specified.
/// </para>
/// <para>
/// <strong>Configuration Categories:</strong>
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <strong>Damage Integration:</strong> Settings for damage-to-stress conversion,
///       including the base formula and bonus stress for critical hits, near-death,
///       and ally death scenarios.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Rest Recovery:</strong> Specialization resource reset values for
///       different rest types (Short, Long, Sanctuary).
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Turn Effects:</strong> Per-turn decay rates for Rage and Momentum,
///       Apotheosis stress costs, and environmental stress caps.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Thresholds:</strong> Critical warning and terminal trigger thresholds
///       for UI feedback and game mechanics.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Warning Messages:</strong> Player-facing narrative text for various
///       trauma economy warning conditions.
///     </description>
///   </item>
/// </list>
/// <para>
/// <strong>Thread Safety:</strong> Implementations must be thread-safe for concurrent
/// reads after initialization. Configuration values are read-only at runtime.
/// </para>
/// <para>
/// <strong>Consumers:</strong> TraumaEconomyService, UnifiedDamageHandler,
/// UnifiedRestHandler, UnifiedTurnHandler, UI warning systems.
/// </para>
/// </remarks>
/// <seealso cref="TraumaEconomyConfiguration"/>
public interface ITraumaEconomyConfiguration
{
    #region Damage Integration

    /// <summary>
    /// Gets whether damage-to-stress conversion is enabled.
    /// </summary>
    /// <value>
    /// <c>true</c> if damage should generate stress; <c>false</c> to disable
    /// the damage-stress link entirely. Default: <c>true</c>.
    /// </value>
    bool DamageToStressEnabled { get; }

    /// <summary>
    /// Gets the formula for calculating base stress from damage.
    /// </summary>
    /// <value>
    /// A formula string such as "floor(damage / 10)". Default: "floor(damage / 10)".
    /// </value>
    /// <remarks>
    /// The formula is evaluated by the damage handler. The variable "damage"
    /// represents damage after soak is applied.
    /// </remarks>
    string DamageToStressFormula { get; }

    /// <summary>
    /// Gets the bonus stress applied when damage is from a critical hit.
    /// </summary>
    /// <value>Stress bonus for critical hits. Default: 5.</value>
    int CriticalHitStressBonus { get; }

    /// <summary>
    /// Gets the bonus stress applied when character is at near-death (HP below 25%).
    /// </summary>
    /// <value>Stress bonus for near-death damage. Default: 10.</value>
    int NearDeathStressBonus { get; }

    /// <summary>
    /// Gets the bonus stress applied when witnessing an ally's death.
    /// </summary>
    /// <value>Stress bonus for ally death. Default: 15.</value>
    int AllyDeathStressBonus { get; }

    #endregion

    #region Rest Recovery

    /// <summary>
    /// Gets the Rage reset value after a Short Rest.
    /// </summary>
    /// <value>Rage value after Short Rest. Default: 0.</value>
    int ShortRestRageReset { get; }

    /// <summary>
    /// Gets the Momentum reset value after a Short Rest.
    /// </summary>
    /// <value>Momentum value after Short Rest. Default: 0.</value>
    int ShortRestMomentumReset { get; }

    /// <summary>
    /// Gets the Coherence restore value after a Long Rest.
    /// </summary>
    /// <value>Coherence value restored to after Long Rest. Default: 50.</value>
    int LongRestCoherenceRestore { get; }

    /// <summary>
    /// Gets the Coherence restore value after a Sanctuary Rest.
    /// </summary>
    /// <value>Coherence value restored to after Sanctuary Rest. Default: 50.</value>
    int SanctuaryCoherenceRestore { get; }

    #endregion

    #region Turn Effects

    /// <summary>
    /// Gets the Rage decay amount per turn when out of combat.
    /// </summary>
    /// <value>Rage decay per turn outside combat. Default: 10.</value>
    int RageDecayOutOfCombat { get; }

    /// <summary>
    /// Gets the Momentum decay amount per turn when idle (no action taken).
    /// </summary>
    /// <value>Momentum decay per idle turn. Default: 15.</value>
    int MomentumDecayIdle { get; }

    /// <summary>
    /// Gets the stress cost per turn while in Apotheosis state.
    /// </summary>
    /// <value>Stress applied each turn during Apotheosis. Default: 10.</value>
    int ApotheosisStressCost { get; }

    /// <summary>
    /// Gets the maximum environmental stress that can be applied per turn.
    /// </summary>
    /// <value>Cap on environmental stress per turn. Default: 5.</value>
    int MaxEnvironmentalStress { get; }

    #endregion

    #region Thresholds

    /// <summary>
    /// Gets the threshold at which critical warnings should be displayed.
    /// </summary>
    /// <value>Percentage (0-100) for critical warning. Default: 80.</value>
    int CriticalWarningThreshold { get; }

    /// <summary>
    /// Gets the threshold that triggers terminal effects (trauma checks, etc.).
    /// </summary>
    /// <value>Percentage (0-100) for terminal trigger. Default: 100.</value>
    int TerminalTriggerThreshold { get; }

    #endregion

    #region Warning Messages

    /// <summary>
    /// Gets the warning message displayed when stress is critically high.
    /// </summary>
    /// <value>Player-facing stress warning text.</value>
    string StressHighMessage { get; }

    /// <summary>
    /// Gets the warning message displayed when corruption is rising dangerously.
    /// </summary>
    /// <value>Player-facing corruption warning text.</value>
    string CorruptionRisingMessage { get; }

    /// <summary>
    /// Gets the warning message displayed when rage is about to overflow.
    /// </summary>
    /// <value>Player-facing rage overflow warning text.</value>
    string RageOverflowWarning { get; }

    /// <summary>
    /// Gets the warning message displayed when momentum is critically high.
    /// </summary>
    /// <value>Player-facing momentum warning text.</value>
    string MomentumCriticalMessage { get; }

    /// <summary>
    /// Gets the warning message displayed when coherence is dangerously high.
    /// </summary>
    /// <value>Player-facing coherence/Apotheosis warning text.</value>
    string CoherenceCriticalMessage { get; }

    #endregion
}
