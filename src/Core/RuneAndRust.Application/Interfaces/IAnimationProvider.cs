using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Provides animation definitions from configuration.
/// </summary>
public interface IAnimationProvider
{
    /// <summary>
    /// Gets the animation definition for a type.
    /// </summary>
    /// <param name="type">Animation type.</param>
    /// <returns>Animation definition, or null if not found.</returns>
    AnimationDefinition? GetAnimation(AnimationType type);
    
    /// <summary>
    /// Gets all available animation types.
    /// </summary>
    IReadOnlyList<AnimationType> AvailableAnimations { get; }
}
