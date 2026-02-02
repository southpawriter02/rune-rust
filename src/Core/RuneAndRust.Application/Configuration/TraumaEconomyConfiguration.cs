// ═══════════════════════════════════════════════════════════════════════════════
// TraumaEconomyConfiguration.cs
// Implementation of ITraumaEconomyConfiguration that loads settings from
// Microsoft.Extensions.Configuration (typically backed by trauma-economy.json).
// Provides default values and validates configuration on construction.
// Version: 0.18.5e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Configuration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

/// <summary>
/// Loads and provides access to trauma economy configuration settings.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Overview:</strong>
/// TraumaEconomyConfiguration reads values from the application's IConfiguration
/// instance, typically populated from <c>trauma-economy.json</c>. All properties
/// provide sensible defaults when configuration values are missing.
/// </para>
/// <para>
/// <strong>Configuration Path:</strong>
/// All values are read from the "traumaEconomy" section of configuration:
/// <list type="bullet">
///   <item><description>traumaEconomy:integration:damageToStress:*</description></item>
///   <item><description>traumaEconomy:integration:restRecovery:*</description></item>
///   <item><description>traumaEconomy:integration:turnEffects:*</description></item>
///   <item><description>traumaEconomy:thresholds:*</description></item>
///   <item><description>traumaEconomy:warningMessages:*</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Validation:</strong>
/// The constructor validates that threshold values are within acceptable ranges
/// (0-100) and throws <see cref="InvalidOperationException"/> if validation fails.
/// </para>
/// <para>
/// <strong>Thread Safety:</strong>
/// This class is thread-safe after construction. All properties are read-only
/// and backed by the immutable IConfiguration instance.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Registration in DI
/// services.AddSingleton&lt;ITraumaEconomyConfiguration, TraumaEconomyConfiguration&gt;();
/// 
/// // Usage in a service
/// public class SomeService
/// {
///     private readonly ITraumaEconomyConfiguration _config;
///     
///     public SomeService(ITraumaEconomyConfiguration config)
///     {
///         _config = config;
///         var bonus = _config.CriticalHitStressBonus; // 5
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="ITraumaEconomyConfiguration"/>
public class TraumaEconomyConfiguration : ITraumaEconomyConfiguration
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TraumaEconomyConfiguration> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TraumaEconomyConfiguration"/> class.
    /// </summary>
    /// <param name="configuration">The application configuration instance.</param>
    /// <param name="logger">Optional logger for configuration diagnostics.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="configuration"/> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when configuration validation fails (e.g., threshold out of range).
    /// </exception>
    public TraumaEconomyConfiguration(
        IConfiguration configuration,
        ILogger<TraumaEconomyConfiguration>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        
        _configuration = configuration;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<TraumaEconomyConfiguration>.Instance;
        
        Validate();
        
        _logger.LogInformation(
            "TraumaEconomyConfiguration initialized: DamageToStress={Enabled}, CriticalThreshold={Threshold}",
            DamageToStressEnabled,
            CriticalWarningThreshold);
    }

    #region Damage Integration

    /// <inheritdoc/>
    public bool DamageToStressEnabled =>
        _configuration.GetValue("traumaEconomy:integration:damageToStress:enabled", true);

    /// <inheritdoc/>
    public string DamageToStressFormula =>
        _configuration.GetValue("traumaEconomy:integration:damageToStress:formula", "floor(damage / 10)")!;

    /// <inheritdoc/>
    public int CriticalHitStressBonus =>
        _configuration.GetValue("traumaEconomy:integration:damageToStress:criticalBonus", 5);

    /// <inheritdoc/>
    public int NearDeathStressBonus =>
        _configuration.GetValue("traumaEconomy:integration:damageToStress:nearDeathBonus", 10);

    /// <inheritdoc/>
    public int AllyDeathStressBonus =>
        _configuration.GetValue("traumaEconomy:integration:damageToStress:allyDeathBonus", 15);

    #endregion

    #region Rest Recovery

    /// <inheritdoc/>
    public int ShortRestRageReset =>
        _configuration.GetValue("traumaEconomy:integration:restRecovery:shortRest:rageReset", 0);

    /// <inheritdoc/>
    public int ShortRestMomentumReset =>
        _configuration.GetValue("traumaEconomy:integration:restRecovery:shortRest:momentumReset", 0);

    /// <inheritdoc/>
    public int LongRestCoherenceRestore =>
        _configuration.GetValue("traumaEconomy:integration:restRecovery:longRest:coherenceRestore", 50);

    /// <inheritdoc/>
    public int SanctuaryCoherenceRestore =>
        _configuration.GetValue("traumaEconomy:integration:restRecovery:sanctuary:coherenceRestore", 50);

    #endregion

    #region Turn Effects

    /// <inheritdoc/>
    public int RageDecayOutOfCombat =>
        _configuration.GetValue("traumaEconomy:integration:turnEffects:rageDecayOutOfCombat", 10);

    /// <inheritdoc/>
    public int MomentumDecayIdle =>
        _configuration.GetValue("traumaEconomy:integration:turnEffects:momentumDecayIdle", 15);

    /// <inheritdoc/>
    public int ApotheosisStressCost =>
        _configuration.GetValue("traumaEconomy:integration:turnEffects:apotheosisStressCost", 10);

    /// <inheritdoc/>
    public int MaxEnvironmentalStress =>
        _configuration.GetValue("traumaEconomy:integration:turnEffects:maxEnvironmentalStress", 5);

    #endregion

    #region Thresholds

    /// <inheritdoc/>
    public int CriticalWarningThreshold =>
        _configuration.GetValue("traumaEconomy:thresholds:criticalWarning", 80);

    /// <inheritdoc/>
    public int TerminalTriggerThreshold =>
        _configuration.GetValue("traumaEconomy:thresholds:terminalTrigger", 100);

    #endregion

    #region Warning Messages

    /// <inheritdoc/>
    public string StressHighMessage =>
        _configuration.GetValue(
            "traumaEconomy:warningMessages:stressHigh",
            "Your mind strains under the weight of reality.")!;

    /// <inheritdoc/>
    public string CorruptionRisingMessage =>
        _configuration.GetValue(
            "traumaEconomy:warningMessages:corruptionRising",
            "The corruption spreads through your essence.")!;

    /// <inheritdoc/>
    public string RageOverflowWarning =>
        _configuration.GetValue(
            "traumaEconomy:warningMessages:rageOverflow",
            "Your rage threatens to consume you.")!;

    /// <inheritdoc/>
    public string MomentumCriticalMessage =>
        _configuration.GetValue(
            "traumaEconomy:warningMessages:momentumCritical",
            "Your momentum reaches a fever pitch.")!;

    /// <inheritdoc/>
    public string CoherenceCriticalMessage =>
        _configuration.GetValue(
            "traumaEconomy:warningMessages:coherenceCritical",
            "Reality bends around your heightened awareness.")!;

    #endregion

    #region Validation

    /// <summary>
    /// Validates that all configuration values are within acceptable ranges.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when any configuration value is outside its valid range.
    /// </exception>
    private void Validate()
    {
        // Threshold validation
        if (CriticalWarningThreshold < 0 || CriticalWarningThreshold > 100)
        {
            throw new InvalidOperationException(
                $"CriticalWarningThreshold must be 0-100, got {CriticalWarningThreshold}");
        }

        if (TerminalTriggerThreshold < 0 || TerminalTriggerThreshold > 100)
        {
            throw new InvalidOperationException(
                $"TerminalTriggerThreshold must be 0-100, got {TerminalTriggerThreshold}");
        }

        if (CriticalWarningThreshold >= TerminalTriggerThreshold)
        {
            _logger.LogWarning(
                "CriticalWarningThreshold ({Critical}) >= TerminalTriggerThreshold ({Terminal})",
                CriticalWarningThreshold,
                TerminalTriggerThreshold);
        }

        // Decay and cost validation
        if (RageDecayOutOfCombat < 0)
        {
            throw new InvalidOperationException(
                $"RageDecayOutOfCombat must be non-negative, got {RageDecayOutOfCombat}");
        }

        if (MomentumDecayIdle < 0)
        {
            throw new InvalidOperationException(
                $"MomentumDecayIdle must be non-negative, got {MomentumDecayIdle}");
        }

        if (ApotheosisStressCost < 0)
        {
            throw new InvalidOperationException(
                $"ApotheosisStressCost must be non-negative, got {ApotheosisStressCost}");
        }

        if (MaxEnvironmentalStress < 0)
        {
            throw new InvalidOperationException(
                $"MaxEnvironmentalStress must be non-negative, got {MaxEnvironmentalStress}");
        }

        // Bonus validation
        if (CriticalHitStressBonus < 0)
        {
            throw new InvalidOperationException(
                $"CriticalHitStressBonus must be non-negative, got {CriticalHitStressBonus}");
        }

        if (NearDeathStressBonus < 0)
        {
            throw new InvalidOperationException(
                $"NearDeathStressBonus must be non-negative, got {NearDeathStressBonus}");
        }

        if (AllyDeathStressBonus < 0)
        {
            throw new InvalidOperationException(
                $"AllyDeathStressBonus must be non-negative, got {AllyDeathStressBonus}");
        }

        _logger.LogDebug("TraumaEconomyConfiguration validation passed");
    }

    #endregion
}
