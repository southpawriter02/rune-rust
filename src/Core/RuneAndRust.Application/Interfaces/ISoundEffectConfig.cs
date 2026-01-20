namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Application.DTOs;

/// <summary>
/// Configuration for sound effect definitions.
/// </summary>
/// <remarks>
/// <para>
/// Provides configuration lookup:
/// <list type="bullet">
///   <item><description>Get effect by ID</description></item>
///   <item><description>Get effects by category</description></item>
///   <item><description>List all effects and categories</description></item>
/// </list>
/// </para>
/// </remarks>
public interface ISoundEffectConfig
{
    /// <summary>
    /// Gets an effect definition by ID.
    /// </summary>
    /// <param name="effectId">The effect ID.</param>
    /// <returns>Effect definition, or null if not found.</returns>
    SoundEffectDefinition? GetEffect(string effectId);

    /// <summary>
    /// Gets all effect IDs in a category.
    /// </summary>
    /// <param name="category">Category name.</param>
    /// <returns>List of effect IDs.</returns>
    IReadOnlyList<string> GetEffectsInCategory(string category);

    /// <summary>
    /// Gets all effect IDs.
    /// </summary>
    /// <returns>List of all effect IDs.</returns>
    IReadOnlyList<string> GetAllEffectIds();

    /// <summary>
    /// Gets all category names.
    /// </summary>
    /// <returns>List of category names.</returns>
    IReadOnlyList<string> GetAllCategories();

    /// <summary>
    /// Gets global settings.
    /// </summary>
    /// <returns>Sound effect settings.</returns>
    SoundEffectSettings GetSettings();
}
