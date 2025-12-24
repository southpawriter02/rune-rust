using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Defines the contract for procedural description generation.
/// Implements the Three-Tier Composition model (Base + Modifier + Detail).
/// </summary>
/// <remarks>See: SPEC-DESC-001 for Descriptor Engine design.</remarks>
public interface IDescriptorEngine
{
    /// <summary>
    /// Composes a full description from template, modifier, and detail tiers.
    /// </summary>
    /// <param name="baseTemplate">The core description template.</param>
    /// <param name="modifier">Optional biome-specific modifier text.</param>
    /// <param name="detail">Optional danger-level specific detail text.</param>
    /// <returns>The composed description string.</returns>
    string ComposeDescription(string baseTemplate, string? modifier, string? detail);

    /// <summary>
    /// Gets an atmospheric modifier phrase for the given biome type.
    /// </summary>
    /// <param name="biome">The biome type.</param>
    /// <returns>A descriptive modifier phrase.</returns>
    string GetModifierForBiome(BiomeType biome);

    /// <summary>
    /// Gets an atmospheric detail phrase for the given danger level.
    /// </summary>
    /// <param name="danger">The danger level.</param>
    /// <returns>A descriptive detail phrase.</returns>
    string GetDetailForDangerLevel(DangerLevel danger);

    /// <summary>
    /// Generates a complete room description with biome and danger modifiers.
    /// </summary>
    /// <param name="baseDescription">The room's base description.</param>
    /// <param name="biome">The room's biome type.</param>
    /// <param name="danger">The room's danger level.</param>
    /// <returns>The composed description with all tiers applied.</returns>
    string GenerateRoomDescription(string baseDescription, BiomeType biome, DangerLevel danger);
}
