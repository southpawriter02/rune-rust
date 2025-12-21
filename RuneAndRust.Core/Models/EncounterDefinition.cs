namespace RuneAndRust.Core.Models;

/// <summary>
/// Defines a combat encounter to be spawned.
/// Used by ambush system and future random encounter generation.
/// </summary>
/// <param name="TemplateIds">List of enemy template IDs to spawn.</param>
/// <param name="Budget">The encounter budget used for scaling.</param>
/// <param name="IsAmbush">Whether this is an ambush encounter (affects initiative).</param>
/// <param name="EncounterType">Type classification for logging and loot tables.</param>
public record EncounterDefinition(
    IReadOnlyList<string> TemplateIds,
    float Budget,
    bool IsAmbush,
    string EncounterType = "Ambush"
);
