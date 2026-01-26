namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Application.DTOs;

/// <summary>
/// Service interface for flora and fauna observation, examination, and
/// harvest information within the perception system.
/// </summary>
/// <remarks>
/// <para>
/// This service handles all biological entity observations separate from
/// the general object perception system, enabling specialized handling of
/// species data, ecological behaviors, and harvest mechanics.
/// </para>
/// <para>
/// The service integrates with the three-layer examination system from v0.15.6b
/// and uses the success-counting dice mechanics from v0.15.0.
/// </para>
/// </remarks>
public interface IFloraFaunaService
{
    /// <summary>
    /// Gets ambient flora and fauna observation for a room based on
    /// the player's Wits pool and the room's biome.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Ambient observation occurs automatically when entering a room or
    /// through passive perception. The Wits pool determines how many
    /// species the player notices and how much detail is provided.
    /// </para>
    /// <para>
    /// Higher Wits pools notice more species:
    /// - Wits ≤2: 1 species
    /// - Wits ≤4: 2 species
    /// - Wits ≤6: 3 species
    /// - Wits >6: 4 species
    /// </para>
    /// </remarks>
    /// <param name="roomId">The room being observed.</param>
    /// <param name="biome">The biome of the room (determines available species).</param>
    /// <param name="witsPool">The player's Wits dice pool for passive observation.</param>
    /// <returns>An ambient observation with atmospheric text and species list.</returns>
    AmbientObservationDto GetAmbientObservation(string roomId, Biome biome, int witsPool);

    /// <summary>
    /// Examines a specific flora or fauna species using the three-layer
    /// examination system with Wits-based success counting.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Examination follows the same DC thresholds as object examination:
    /// - Layer 1 (Cursory): DC 0 (automatic)
    /// - Layer 2 (Detailed): DC 12 (2 successes)
    /// - Layer 3 (Expert): DC 18 (4 successes)
    /// </para>
    /// <para>
    /// Expert examination of flora reveals alchemical uses and harvest
    /// information. Expert examination of fauna reveals behavioral
    /// patterns and environmental indicators.
    /// </para>
    /// </remarks>
    /// <param name="speciesName">The species name to examine.</param>
    /// <param name="biome">The current biome context.</param>
    /// <param name="witsPool">The player's Wits dice pool for the examination.</param>
    /// <returns>The examination result with all revealed information.</returns>
    SpeciesExaminationResult ExamineSpecies(string speciesName, Biome biome, int witsPool);

    /// <summary>
    /// Gets all harvestable flora in the specified room that the player
    /// has discovered through examination or passive perception.
    /// </summary>
    /// <remarks>
    /// This method returns flora that:
    /// 1. Is present in the room's biome
    /// 2. Has been noticed by the player (through ambient observation or examination)
    /// 3. Has harvest mechanics defined (HarvestDc is set)
    ///
    /// The actual harvest action is handled by a separate harvesting system
    /// in a future version; this method only provides discovery information.
    /// </remarks>
    /// <param name="roomId">The room to check for harvestable flora.</param>
    /// <returns>A list of harvestable flora with DC and risk information.</returns>
    IReadOnlyList<HarvestableFlora> GetHarvestableFlora(string roomId);

    /// <summary>
    /// Gets all flora species that can appear in the specified biome.
    /// </summary>
    /// <param name="biome">The biome to get flora for.</param>
    /// <returns>A list of flora species for this biome including universal species.</returns>
    IReadOnlyList<FloraFaunaDescriptor> GetFloraForBiome(Biome biome);

    /// <summary>
    /// Gets all fauna species that can appear in the specified biome.
    /// </summary>
    /// <param name="biome">The biome to get fauna for.</param>
    /// <returns>A list of fauna species for this biome including universal species.</returns>
    IReadOnlyList<FloraFaunaDescriptor> GetFaunaForBiome(Biome biome);

    /// <summary>
    /// Gets a specific species descriptor by name and biome.
    /// </summary>
    /// <param name="speciesName">The species common name.</param>
    /// <param name="biome">The biome context.</param>
    /// <returns>The species descriptor, or null if not found.</returns>
    FloraFaunaDescriptor? GetSpeciesByName(string speciesName, Biome biome);
}
