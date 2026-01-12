using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Represents an interactive object in the game world.
/// </summary>
public class InteractiveObject : IEntity
{
    /// <summary>
    /// Gets the unique identifier.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the definition ID.
    /// </summary>
    public string DefinitionId { get; private set; }

    /// <summary>
    /// Gets the display name.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the description.
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Gets the current state.
    /// </summary>
    public ObjectState State { get; private set; }

    /// <summary>
    /// Gets the object type.
    /// </summary>
    public InteractiveObjectType ObjectType { get; private set; }

    /// <summary>
    /// Gets the allowed interactions.
    /// </summary>
    public IReadOnlyList<InteractionType> AllowedInteractions { get; private set; }

    /// <summary>
    /// Gets the search keywords.
    /// </summary>
    public IReadOnlyList<string> Keywords { get; private set; }

    /// <summary>
    /// Gets the direction this object blocks when closed (null = doesn't block).
    /// </summary>
    public Direction? BlocksDirection { get; private set; }

    /// <summary>
    /// Gets whether this object is currently blocking movement.
    /// </summary>
    public bool IsCurrentlyBlocking => BlocksDirection.HasValue && State == ObjectState.Closed;

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private InteractiveObject()
    {
        DefinitionId = null!;
        Name = null!;
        Description = string.Empty;
        AllowedInteractions = [];
        Keywords = [];
    }

    /// <summary>
    /// Creates a new interactive object.
    /// </summary>
    public static InteractiveObject Create(
        string definitionId,
        string name,
        string description,
        InteractiveObjectType objectType,
        ObjectState initialState = ObjectState.Closed,
        IReadOnlyList<InteractionType>? allowedInteractions = null,
        IReadOnlyList<string>? keywords = null,
        Direction? blocksDirection = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(definitionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new InteractiveObject
        {
            Id = Guid.NewGuid(),
            DefinitionId = definitionId.ToLowerInvariant(),
            Name = name,
            Description = description ?? string.Empty,
            ObjectType = objectType,
            State = initialState,
            AllowedInteractions = allowedInteractions ?? [InteractionType.Examine],
            Keywords = keywords ?? [name.ToLowerInvariant()],
            BlocksDirection = blocksDirection
        };
    }

    /// <summary>
    /// Attempts to set the object state.
    /// </summary>
    public bool TrySetState(ObjectState newState)
    {
        if (State == ObjectState.Broken || State == ObjectState.Destroyed)
            return false;

        State = newState;
        return true;
    }

    /// <summary>
    /// Checks if an interaction is allowed.
    /// </summary>
    public bool CanPerformInteraction(InteractionType type) =>
        AllowedInteractions.Contains(type);

    /// <summary>
    /// Gets the default interaction based on current state.
    /// </summary>
    public InteractionType GetDefaultInteraction()
    {
        return State switch
        {
            ObjectState.Closed when CanPerformInteraction(InteractionType.Open) => InteractionType.Open,
            ObjectState.Open when CanPerformInteraction(InteractionType.Close) => InteractionType.Close,
            ObjectState.Inactive when CanPerformInteraction(InteractionType.Activate) => InteractionType.Activate,
            ObjectState.Active when CanPerformInteraction(InteractionType.Deactivate) => InteractionType.Deactivate,
            _ => InteractionType.Examine
        };
    }

    /// <summary>
    /// Checks if a keyword matches this object.
    /// </summary>
    public bool MatchesKeyword(string keyword) =>
        Keywords.Any(k => k.Equals(keyword, StringComparison.OrdinalIgnoreCase)) ||
        Name.Contains(keyword, StringComparison.OrdinalIgnoreCase);
}
