namespace RuneAndRust.Domain.Interfaces;

/// <summary>
/// Base interface for all domain entities that require a unique identifier.
/// </summary>
/// <remarks>
/// All aggregate roots and entities in the domain should implement this interface
/// to ensure consistent identity management across the application.
/// </remarks>
public interface IEntity
{
    /// <summary>
    /// Gets the unique identifier for this entity.
    /// </summary>
    /// <value>A <see cref="Guid"/> that uniquely identifies this entity.</value>
    Guid Id { get; }
}
