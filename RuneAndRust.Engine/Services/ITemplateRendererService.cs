using RuneAndRust.Core.Entities;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Service for rendering room names and descriptions from templates with variable substitution.
/// Part of the Dynamic Room Engine (v0.4.0).
/// </summary>
/// <remarks>See: SPEC-TEMPLATE-001 for Dynamic Room Template System design.</remarks>
public interface ITemplateRendererService
{
    /// <summary>
    /// Renders a randomized room name from a template.
    /// Selects a random NameTemplate and substitutes {Adjective} tokens with random values.
    /// </summary>
    /// <param name="template">The room template containing NameTemplates and Adjectives.</param>
    /// <returns>A rendered room name (e.g., "The Pulsing Reactor Core").</returns>
    string RenderRoomName(RoomTemplate template);

    /// <summary>
    /// Renders a randomized room description from a template.
    /// Selects a random DescriptionTemplate and substitutes {Adjective} and {Detail} tokens.
    /// Optionally appends atmospheric details (sounds, smells) from biome descriptor categories.
    /// </summary>
    /// <param name="template">The room template containing DescriptionTemplates, Adjectives, and Details.</param>
    /// <param name="biome">The biome definition for additional atmospheric descriptors.</param>
    /// <returns>A rendered room description with variable substitution applied.</returns>
    string RenderRoomDescription(RoomTemplate template, BiomeDefinition biome);
}
