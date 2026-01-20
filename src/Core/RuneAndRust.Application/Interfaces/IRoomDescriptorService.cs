using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for three-tier descriptor composition.
/// Generates room names and descriptions from templates + modifiers + fragments.
/// </summary>
public interface IRoomDescriptorService
{
    /// <summary>
    /// Generates a room name from base template and thematic modifier.
    /// </summary>
    /// <param name="template">The base descriptor template.</param>
    /// <param name="modifier">The thematic modifier for the biome.</param>
    /// <param name="function">Optional room function for specialized chambers.</param>
    /// <returns>A generated room name with placeholders replaced.</returns>
    string GenerateRoomName(
        BaseDescriptorTemplate template,
        ThematicModifier modifier,
        RoomFunction? function = null);

    /// <summary>
    /// Generates a full room description with all placeholder tokens replaced.
    /// </summary>
    /// <param name="template">The base descriptor template.</param>
    /// <param name="modifier">The thematic modifier for the biome.</param>
    /// <param name="roomTags">Tags from the room for fragment filtering.</param>
    /// <param name="random">Random instance for weighted selection.</param>
    /// <param name="function">Optional room function for specialized chambers.</param>
    /// <returns>A generated room description with all placeholders filled.</returns>
    string GenerateRoomDescription(
        BaseDescriptorTemplate template,
        ThematicModifier modifier,
        IReadOnlyList<string> roomTags,
        Random random,
        RoomFunction? function = null);

    /// <summary>
    /// Generates a description for a room feature within biome context.
    /// </summary>
    /// <param name="feature">The room feature to describe.</param>
    /// <param name="biome">The biome context.</param>
    /// <param name="random">Random instance for weighted selection.</param>
    /// <returns>A generated feature description.</returns>
    string GenerateFeatureDescription(
        RoomFeature feature,
        Biome biome,
        Random random);
}
