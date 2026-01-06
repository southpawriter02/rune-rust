using RuneAndRust.Application.DTOs;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service interface for examination and perception operations.
/// </summary>
public interface IExaminationService
{
    /// <summary>
    /// Examines an object with a WITS check, returning layered description.
    /// </summary>
    /// <param name="objectId">The ID of the object being examined.</param>
    /// <param name="objectType">The type/name of the object.</param>
    /// <param name="category">The category of the object.</param>
    /// <param name="witsValue">The player's WITS attribute value.</param>
    /// <param name="currentBiome">The current room's biome for context-specific descriptors.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The examination result with layered descriptions.</returns>
    Task<ExaminationResultDto> ExamineObjectAsync(
        Guid objectId,
        string objectType,
        ObjectCategory category,
        int witsValue,
        Biome? currentBiome = null,
        CancellationToken ct = default);

    /// <summary>
    /// Gets passive perception result for room entry.
    /// </summary>
    /// <param name="roomId">The ID of the room being entered.</param>
    /// <param name="passivePerception">The player's passive perception value.</param>
    /// <param name="hiddenElements">The hidden elements in the room.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The perception result with any revealed elements.</returns>
    Task<PassivePerceptionResultDto> CheckPassivePerceptionAsync(
        Guid roomId,
        int passivePerception,
        IReadOnlyList<Domain.Entities.HiddenElement> hiddenElements,
        CancellationToken ct = default);

    /// <summary>
    /// Performs an active search with a WITS check.
    /// </summary>
    /// <param name="roomId">The ID of the room being searched.</param>
    /// <param name="witsValue">The player's WITS attribute value.</param>
    /// <param name="hiddenElements">The hidden elements in the room.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The search result with any revealed elements and check details.</returns>
    Task<SearchResultDto> PerformActiveSearchAsync(
        Guid roomId,
        int witsValue,
        IReadOnlyList<Domain.Entities.HiddenElement> hiddenElements,
        CancellationToken ct = default);

    /// <summary>
    /// Gets flora/fauna observation for a biome.
    /// </summary>
    /// <param name="biome">The biome to get observations for.</param>
    /// <param name="witsValue">The player's WITS attribute value.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An ambient observation string, or null if none available.</returns>
    Task<AmbientObservationDto?> GetAmbientObservationAsync(
        Biome biome,
        int witsValue,
        CancellationToken ct = default);
}
