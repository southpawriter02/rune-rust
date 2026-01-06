using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Composes context-aware narrative descriptions using tag propagation.
/// </summary>
public interface IDescriptorComposer
{
    /// <summary>
    /// Composes a room description that includes monster presence context.
    /// </summary>
    /// <param name="room">The room to describe.</param>
    /// <returns>A composed description string.</returns>
    string ComposeRoomDescription(Room room);

    /// <summary>
    /// Composes a description for an entity within a room context.
    /// </summary>
    /// <param name="monster">The monster to describe.</param>
    /// <param name="room">The room context for tag combination.</param>
    /// <returns>A composed description string.</returns>
    string ComposeEntityDescription(Monster monster, Room room);

    /// <summary>
    /// Gets modifier text based on combined tags.
    /// </summary>
    /// <param name="tags">The combined tags to evaluate.</param>
    /// <returns>Modifier text or null if no match.</returns>
    string? GetTagModifier(IEnumerable<string> tags);
}
