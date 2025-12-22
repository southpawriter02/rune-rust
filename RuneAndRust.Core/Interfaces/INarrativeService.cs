using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Generates narrative text for game sequences (v0.3.4c).
/// Provides Domain 4-compliant prologue text based on character background.
/// </summary>
public interface INarrativeService
{
    /// <summary>
    /// Gets the prologue text for a character based on their background.
    /// </summary>
    /// <param name="character">The character to generate prologue for.</param>
    /// <returns>Domain 4-compliant narrative text.</returns>
    string GetPrologueText(Character character);

    /// <summary>
    /// Gets the display name for a background type.
    /// </summary>
    /// <param name="background">The background type.</param>
    /// <returns>Human-readable display name.</returns>
    string GetBackgroundDisplayName(BackgroundType background);

    /// <summary>
    /// Gets the description for a background type.
    /// </summary>
    /// <param name="background">The background type.</param>
    /// <returns>Descriptive text for the background.</returns>
    string GetBackgroundDescription(BackgroundType background);
}
