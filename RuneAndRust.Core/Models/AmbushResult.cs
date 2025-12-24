namespace RuneAndRust.Core.Models;

/// <summary>
/// Result of an ambush check during wilderness rest.
/// Contains probability calculations and optional encounter data.
/// </summary>
/// <remarks>See: SPEC-REST-001, Section "Data Models".</remarks>
/// <param name="IsAmbush">True if an ambush was triggered.</param>
/// <param name="Message">AAM-VOICE compliant narrative message.</param>
/// <param name="BaseRiskPercent">Initial risk based on DangerLevel.</param>
/// <param name="MitigationPercent">Risk reduction from Wits roll.</param>
/// <param name="FinalRiskPercent">Final risk after mitigation (minimum 5% in dangerous zones).</param>
/// <param name="RollValue">The d100 roll result (1-100).</param>
/// <param name="MitigationSuccesses">Number of successes on the Wits roll.</param>
/// <param name="Encounter">Optional encounter definition if ambush triggered.</param>
public record AmbushResult(
    bool IsAmbush,
    string Message,
    int BaseRiskPercent,
    int MitigationPercent,
    int FinalRiskPercent,
    int RollValue,
    int MitigationSuccesses,
    EncounterDefinition? Encounter = null
);
