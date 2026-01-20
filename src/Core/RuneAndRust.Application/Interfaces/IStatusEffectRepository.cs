using RuneAndRust.Domain.Definitions;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Repository for status effect definitions.
/// </summary>
public interface IStatusEffectRepository
{
    /// <summary>
    /// Gets a status effect definition by ID.
    /// </summary>
    /// <param name="effectId">The effect definition ID.</param>
    /// <returns>The definition, or null if not found.</returns>
    StatusEffectDefinition? GetById(string effectId);

    /// <summary>
    /// Gets all status effect definitions.
    /// </summary>
    /// <returns>All registered effect definitions.</returns>
    IEnumerable<StatusEffectDefinition> GetAll();

    /// <summary>
    /// Gets all effects in a specific category.
    /// </summary>
    /// <param name="category">The effect category.</param>
    /// <returns>Effects in the specified category.</returns>
    IEnumerable<StatusEffectDefinition> GetByCategory(Domain.Enums.EffectCategory category);
}
