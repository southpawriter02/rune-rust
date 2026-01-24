// ------------------------------------------------------------------------------
// <copyright file="IElectrocutionRiskService.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Service interface for handling electrocution risk during Wire Manipulation.
// Part of v0.15.4e Jury-Rigging System implementation.
// </summary>
// ------------------------------------------------------------------------------

using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Interfaces;

/// <summary>
/// Service interface for handling electrocution risk during Wire Manipulation.
/// </summary>
/// <remarks>
/// <para>
/// Wire Manipulation (-2 DC) carries electrocution risk:
/// <list type="bullet">
///   <item><description>FINESSE save DC 12 before attempting bypass</description></item>
///   <item><description>Failure: Take 2d10 lightning damage</description></item>
///   <item><description>Success: No damage, proceed with bypass</description></item>
///   <item><description>Bypass attempt proceeds regardless of save result</description></item>
/// </list>
/// </para>
/// <para>
/// This service is separate from the main jury-rigging service to allow
/// callers to handle the save/damage flow before proceeding with bypass.
/// </para>
/// </remarks>
public interface IElectrocutionRiskService
{
    // -------------------------------------------------------------------------
    // Risk Evaluation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Evaluates whether the current context carries electrocution risk.
    /// </summary>
    /// <param name="context">The jury-rig context.</param>
    /// <returns>True if electrocution save is required before proceeding.</returns>
    /// <remarks>
    /// Electrocution risk is only present when using Wire Manipulation.
    /// </remarks>
    bool EvaluateRisk(JuryRigContext context);

    // -------------------------------------------------------------------------
    // Save Attempt
    // -------------------------------------------------------------------------

    /// <summary>
    /// Attempts the electrocution save.
    /// </summary>
    /// <param name="finesseScore">The character's FINESSE attribute score.</param>
    /// <returns>The save result including any damage taken.</returns>
    /// <remarks>
    /// <para>
    /// Uses FINESSE check against DC 12.
    /// </para>
    /// <para>
    /// Outcomes:
    /// <list type="bullet">
    ///   <item><description>Success: No damage, proceed with bypass</description></item>
    ///   <item><description>Failure: Take 2d10 lightning damage, then proceed</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    ElectrocutionSaveResult AttemptSave(int finesseScore);

    // -------------------------------------------------------------------------
    // Information Queries
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the DC for electrocution saves.
    /// </summary>
    /// <returns>The save DC (typically 12).</returns>
    int GetSaveDC();

    /// <summary>
    /// Gets the damage expression for electrocution.
    /// </summary>
    /// <returns>The damage dice expression (e.g., "2d10").</returns>
    string GetDamageExpression();

    /// <summary>
    /// Gets the damage type for electrocution.
    /// </summary>
    /// <returns>The damage type (typically "lightning").</returns>
    string GetDamageType();

    /// <summary>
    /// Gets the minimum possible electrocution damage.
    /// </summary>
    /// <returns>The minimum damage (2 for 2d10).</returns>
    int GetMinimumDamage();

    /// <summary>
    /// Gets the maximum possible electrocution damage.
    /// </summary>
    /// <returns>The maximum damage (20 for 2d10).</returns>
    int GetMaximumDamage();

    /// <summary>
    /// Gets the average electrocution damage.
    /// </summary>
    /// <returns>The average damage (11 for 2d10).</returns>
    double GetAverageDamage();
}
