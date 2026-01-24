// ------------------------------------------------------------------------------
// <copyright file="ElectrocutionSaveResult.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// The result of an electrocution save attempt when using Wire Manipulation.
// Part of v0.15.4e Jury-Rigging System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// The result of an electrocution save attempt when using Wire Manipulation.
/// </summary>
/// <remarks>
/// <para>
/// Wire Manipulation requires a FINESSE save DC 12 before the bypass attempt.
/// <list type="bullet">
///   <item><description>Success: No damage, proceed with bypass attempt</description></item>
///   <item><description>Failure: Take 2d10 lightning damage, then proceed with bypass</description></item>
/// </list>
/// </para>
/// <para>
/// The electrocution risk is a trade-off for Wire Manipulation's -2 DC bonus.
/// Damage is dealt regardless of whether the subsequent bypass succeeds.
/// </para>
/// </remarks>
/// <param name="SaveSucceeded">Whether the FINESSE save succeeded.</param>
/// <param name="NetSuccesses">The net successes from the FINESSE check.</param>
/// <param name="SaveDc">The save DC (typically 12).</param>
/// <param name="DamageRolled">The damage taken if save failed (0 if succeeded).</param>
/// <param name="DamageType">The type of damage ("lightning" or "none").</param>
/// <param name="NarrativeText">Flavor text describing the result.</param>
public readonly record struct ElectrocutionSaveResult(
    bool SaveSucceeded,
    int NetSuccesses,
    int SaveDc,
    int DamageRolled,
    string DamageType,
    string NarrativeText)
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    /// <summary>
    /// The standard DC for electrocution saves.
    /// </summary>
    public const int StandardSaveDc = 12;

    /// <summary>
    /// The number of dice rolled for electrocution damage.
    /// </summary>
    public const int DamageDiceCount = 2;

    /// <summary>
    /// The size of dice rolled for electrocution damage.
    /// </summary>
    public const int DamageDieSize = 10;

    /// <summary>
    /// The damage expression for display (2d10).
    /// </summary>
    public const string DamageExpression = "2d10";

    // -------------------------------------------------------------------------
    // Computed Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets a value indicating whether damage was taken.
    /// </summary>
    public bool TookDamage => DamageRolled > 0;

    /// <summary>
    /// Gets a value indicating whether the character can proceed with the bypass.
    /// </summary>
    /// <remarks>
    /// Characters can always proceed with the bypass attempt after an
    /// electrocution check—they just take damage on failed saves.
    /// </remarks>
    public bool CanProceed => true;

    /// <summary>
    /// Gets the minimum possible damage from electrocution.
    /// </summary>
    public static int MinimumDamage => DamageDiceCount;

    /// <summary>
    /// Gets the maximum possible damage from electrocution.
    /// </summary>
    public static int MaximumDamage => DamageDiceCount * DamageDieSize;

    /// <summary>
    /// Gets the average damage from electrocution.
    /// </summary>
    public static double AverageDamage => DamageDiceCount * ((DamageDieSize + 1) / 2.0);

    // -------------------------------------------------------------------------
    // Factory Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a successful save result (no damage).
    /// </summary>
    /// <param name="netSuccesses">The net successes from the FINESSE check.</param>
    /// <param name="narrative">Optional narrative text.</param>
    /// <returns>An ElectrocutionSaveResult for a successful save.</returns>
    /// <remarks>
    /// On a successful save, the character deftly avoids the electrical
    /// discharge while accessing the mechanism's wiring.
    /// </remarks>
    public static ElectrocutionSaveResult Success(
        int netSuccesses,
        string? narrative = null)
    {
        return new ElectrocutionSaveResult(
            SaveSucceeded: true,
            NetSuccesses: netSuccesses,
            SaveDc: StandardSaveDc,
            DamageRolled: 0,
            DamageType: "none",
            NarrativeText: narrative ?? "Your nimble fingers dance around the live wires. " +
                                        "A spark leaps toward you but you pull back just in time.");
    }

    /// <summary>
    /// Creates a failed save result with damage.
    /// </summary>
    /// <param name="netSuccesses">The net successes from the FINESSE check.</param>
    /// <param name="damage">The damage rolled (2d10).</param>
    /// <param name="narrative">Optional narrative text.</param>
    /// <returns>An ElectrocutionSaveResult for a failed save.</returns>
    /// <remarks>
    /// On a failed save, electrical current courses through the character,
    /// dealing 2d10 lightning damage before they can proceed with the bypass.
    /// </remarks>
    public static ElectrocutionSaveResult Failure(
        int netSuccesses,
        int damage,
        string? narrative = null)
    {
        return new ElectrocutionSaveResult(
            SaveSucceeded: false,
            NetSuccesses: netSuccesses,
            SaveDc: StandardSaveDc,
            DamageRolled: damage,
            DamageType: "lightning",
            NarrativeText: narrative ?? $"Lightning arcs through your hands! The current " +
                                        $"courses through your body ({damage} damage) before you " +
                                        "can wrench yourself free. The mechanism still awaits.");
    }

    /// <summary>
    /// Creates a result for when electrocution check was not required.
    /// </summary>
    /// <returns>An ElectrocutionSaveResult indicating no check was needed.</returns>
    /// <remarks>
    /// Only Wire Manipulation requires an electrocution check.
    /// Other bypass methods skip this step entirely.
    /// </remarks>
    public static ElectrocutionSaveResult NotRequired()
    {
        return new ElectrocutionSaveResult(
            SaveSucceeded: true,
            NetSuccesses: 0,
            SaveDc: 0,
            DamageRolled: 0,
            DamageType: "none",
            NarrativeText: "This bypass method does not carry electrocution risk.");
    }

    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns a formatted display string for the result.
    /// </summary>
    /// <returns>A human-readable summary of the electrocution save result.</returns>
    public string ToDisplayString()
    {
        if (SaveDc == 0)
        {
            return "Electrocution Check: Not Required";
        }

        var statusStr = SaveSucceeded ? "SAVED" : "FAILED";
        var damageStr = TookDamage ? $" [{DamageRolled} {DamageType} damage]" : "";

        return $"Electrocution Check: {statusStr} " +
               $"(Roll {NetSuccesses} vs DC {SaveDc}){damageStr}";
    }

    /// <summary>
    /// Returns a compact string for logging purposes.
    /// </summary>
    /// <returns>A log-friendly string representation.</returns>
    public string ToLogString()
    {
        return $"ElectrocutionSaveResult[Saved={SaveSucceeded} Net={NetSuccesses} " +
               $"DC={SaveDc} Damage={DamageRolled}]";
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return ToDisplayString();
    }
}
