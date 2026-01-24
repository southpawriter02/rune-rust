// ------------------------------------------------------------------------------
// <copyright file="ElectrocutionRiskService.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Service for handling electrocution risk during Wire Manipulation bypass attempts.
// Part of v0.15.4e Jury-Rigging System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service for handling electrocution risk during Wire Manipulation bypass attempts.
/// </summary>
/// <remarks>
/// <para>
/// Wire Manipulation carries electrocution risk as a trade-off for its -2 DC bonus:
/// <list type="bullet">
///   <item><description>FINESSE save DC 12 required before bypass attempt</description></item>
///   <item><description>Failed save: 2d10 lightning damage</description></item>
///   <item><description>Successful save: No damage</description></item>
///   <item><description>Bypass attempt proceeds regardless of save result</description></item>
/// </list>
/// </para>
/// <para>
/// This service is separate from the main jury-rigging service to allow
/// callers to handle the save/damage flow before proceeding with bypass,
/// and to enable proper UI feedback for the electrocution risk.
/// </para>
/// </remarks>
public sealed class ElectrocutionRiskService : IElectrocutionRiskService
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    /// <summary>
    /// The DC for electrocution saves (FINESSE check).
    /// </summary>
    private const int ElectrocutionSaveDc = 12;

    /// <summary>
    /// The number of dice rolled for electrocution damage.
    /// </summary>
    private const int ElectrocutionDamageDiceCount = 2;

    /// <summary>
    /// The size of dice rolled for electrocution damage.
    /// </summary>
    private const int ElectrocutionDamageDieSize = 10;

    /// <summary>
    /// The damage expression string for display.
    /// </summary>
    private const string ElectrocutionDamageExpressionString = "2d10";

    /// <summary>
    /// The damage type for electrocution.
    /// </summary>
    private const string ElectrocutionDamageTypeString = "lightning";

    // -------------------------------------------------------------------------
    // Dependencies
    // -------------------------------------------------------------------------

    private readonly IDiceService _diceService;
    private readonly ILogger<ElectrocutionRiskService> _logger;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ElectrocutionRiskService"/> class.
    /// </summary>
    /// <param name="diceService">Service for rolling dice.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public ElectrocutionRiskService(
        IDiceService diceService,
        ILogger<ElectrocutionRiskService> logger)
    {
        _diceService = diceService ?? throw new ArgumentNullException(nameof(diceService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogInformation("ElectrocutionRiskService initialized");
    }

    // -------------------------------------------------------------------------
    // Risk Evaluation
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public bool EvaluateRisk(JuryRigContext context)
    {
        var hasRisk = context.MethodUsed == BypassMethod.WireManipulation;

        _logger.LogDebug(
            "Evaluating electrocution risk: Method={Method}, HasRisk={HasRisk}",
            context.MethodUsed,
            hasRisk);

        return hasRisk;
    }

    // -------------------------------------------------------------------------
    // Save Attempt
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public ElectrocutionSaveResult AttemptSave(int finesseScore)
    {
        _logger.LogInformation(
            "Attempting electrocution save: FinesseScore={FinesseScore}, DC={DC}",
            finesseScore,
            ElectrocutionSaveDc);

        // Roll FINESSE check
        var dicePool = DicePool.D10(Math.Max(1, finesseScore));
        var rollResult = _diceService.Roll(dicePool);
        var netSuccesses = rollResult.NetSuccesses;
        var success = netSuccesses > 0;

        _logger.LogDebug(
            "Electrocution save roll: Successes={Successes}, Botches={Botches}, " +
            "NetSuccesses={NetSuccesses}, Success={Success}",
            rollResult.TotalSuccesses,
            rollResult.TotalBotches,
            netSuccesses,
            success);

        if (success)
        {
            _logger.LogInformation(
                "Electrocution save SUCCEEDED: NetSuccesses={NetSuccesses}",
                netSuccesses);

            return ElectrocutionSaveResult.Success(netSuccesses);
        }
        else
        {
            // Roll damage (2d10 lightning)
            var damagePool = DicePool.D10(ElectrocutionDamageDiceCount);
            var damageResult = _diceService.Roll(damagePool);
            var damage = damageResult.Total;

            _logger.LogWarning(
                "Electrocution save FAILED: NetSuccesses={NetSuccesses}, " +
                "Damage={Damage} ({DamageExpression})",
                netSuccesses,
                damage,
                ElectrocutionDamageExpressionString);

            return ElectrocutionSaveResult.Failure(netSuccesses, damage);
        }
    }

    // -------------------------------------------------------------------------
    // Information Queries
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public int GetSaveDC()
    {
        return ElectrocutionSaveDc;
    }

    /// <inheritdoc />
    public string GetDamageExpression()
    {
        return ElectrocutionDamageExpressionString;
    }

    /// <inheritdoc />
    public string GetDamageType()
    {
        return ElectrocutionDamageTypeString;
    }

    /// <inheritdoc />
    public int GetMinimumDamage()
    {
        return ElectrocutionDamageDiceCount; // 2 (for 2d10)
    }

    /// <inheritdoc />
    public int GetMaximumDamage()
    {
        return ElectrocutionDamageDiceCount * ElectrocutionDamageDieSize; // 20 (for 2d10)
    }

    /// <inheritdoc />
    public double GetAverageDamage()
    {
        // Average of d10 is 5.5
        return ElectrocutionDamageDiceCount * ((ElectrocutionDamageDieSize + 1) / 2.0); // 11 (for 2d10)
    }
}
