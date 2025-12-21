using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models;
using CharacterEntity = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Service for calculating ambush risk and generating ambush encounters.
/// Integrates with the rest system for wilderness rest danger mechanics.
/// </summary>
public interface IAmbushService
{
    /// <summary>
    /// Calculates the ambush risk for a wilderness rest in the given room.
    /// Uses Room.DangerLevel for base risk and character Wits for mitigation.
    /// </summary>
    /// <param name="character">The character performing the rest.</param>
    /// <param name="room">The room where rest is attempted.</param>
    /// <returns>An AmbushResult containing the determination and any encounter.</returns>
    Task<AmbushResult> CalculateAmbushAsync(CharacterEntity character, Room room);

    /// <summary>
    /// Generates an ambush encounter appropriate for the room's biome and danger level.
    /// Prioritizes fast/stealthy enemy types and uses reduced budget (0.8x standard).
    /// </summary>
    /// <param name="room">The room context for enemy selection.</param>
    /// <param name="partyLevel">The party level for enemy scaling.</param>
    /// <returns>An EncounterDefinition with enemies to spawn.</returns>
    EncounterDefinition GenerateAmbushEncounter(Room room, int partyLevel = 1);

    /// <summary>
    /// Gets the base ambush risk percentage for a given danger level.
    /// </summary>
    /// <param name="dangerLevel">The room's danger level.</param>
    /// <returns>Base risk: Safe=0%, Unstable=15%, Hostile=30%, Lethal=50%.</returns>
    int GetBaseRisk(DangerLevel dangerLevel);
}
