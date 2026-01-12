using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Represents an interactive object in the game world.
/// </summary>
/// <remarks>
/// <para>
/// Interactive objects are environmental elements that players can interact with,
/// such as doors, chests, levers, and switches. Each object has a state that can
/// be changed through player interactions, and may block passage in certain directions.
/// </para>
/// <para>
/// Objects are created either directly via <see cref="Create"/> or from JSON definitions
/// via <see cref="FromDefinition"/>. State changes are validated to prevent invalid
/// transitions from permanent states like Broken or Destroyed.
/// </para>
/// </remarks>
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
    /// Gets the default state this object starts in.
    /// </summary>
    public ObjectState DefaultState { get; private set; }

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
    /// Gets whether this object can block passage (like doors).
    /// </summary>
    public bool BlocksPassage { get; private set; }

    /// <summary>
    /// Gets the direction this object blocks when closed/locked (null = doesn't block).
    /// </summary>
    public Direction? BlockedDirection { get; private set; }

    /// <summary>
    /// Gets whether this object is visible in room descriptions.
    /// </summary>
    public bool IsVisible { get; private set; } = true;

    /// <summary>
    /// Gets whether this object is currently blocking movement.
    /// </summary>
    /// <remarks>
    /// An object blocks movement when it blocks passage and is in a blocking state
    /// (Closed or Locked). Objects that are Open, Broken, or Destroyed do not block.
    /// </remarks>
    public bool IsCurrentlyBlocking =>
        BlocksPassage &&
        BlockedDirection.HasValue &&
        (State == ObjectState.Closed || State == ObjectState.Locked);

    /// <summary>
    /// Gets whether this object can be interacted with.
    /// </summary>
    /// <remarks>
    /// Broken and Destroyed objects cannot be interacted with.
    /// </remarks>
    public bool CanInteract => State != ObjectState.Broken && State != ObjectState.Destroyed;

    // ===== Container Properties (v0.4.0b) =====

    /// <summary>
    /// Gets the container inventory (null for non-containers).
    /// </summary>
    public ContainerInventory? ContainerInventory { get; private set; }

    /// <summary>
    /// Gets whether this object is a container.
    /// </summary>
    public bool IsContainer => ContainerInventory != null;

    // ===== Lock Properties (v0.4.0b) =====

    /// <summary>
    /// Gets the lock definition for this object.
    /// </summary>
    public LockDefinition Lock { get; private set; } = LockDefinition.None;

    /// <summary>
    /// Gets whether this object has a lock.
    /// </summary>
    public bool HasLock => Lock.HasLock;

    /// <summary>
    /// Gets whether this object is currently locked.
    /// </summary>
    public bool IsLocked => State == ObjectState.Locked;

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
    /// <param name="definitionId">The definition ID from configuration.</param>
    /// <param name="name">The display name.</param>
    /// <param name="description">The description.</param>
    /// <param name="objectType">The type of object.</param>
    /// <param name="defaultState">The initial and default state.</param>
    /// <param name="allowedInteractions">Allowed interaction types.</param>
    /// <param name="keywords">Search keywords.</param>
    /// <param name="blocksPassage">Whether this object can block movement.</param>
    /// <param name="blockedDirection">The direction blocked (if blocking).</param>
    /// <param name="isVisible">Whether visible in room descriptions.</param>
    /// <returns>A new InteractiveObject instance.</returns>
    public static InteractiveObject Create(
        string definitionId,
        string name,
        string description,
        InteractiveObjectType objectType,
        ObjectState defaultState = ObjectState.Closed,
        IReadOnlyList<InteractionType>? allowedInteractions = null,
        IReadOnlyList<string>? keywords = null,
        bool blocksPassage = false,
        Direction? blockedDirection = null,
        bool isVisible = true)
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
            State = defaultState,
            DefaultState = defaultState,
            AllowedInteractions = allowedInteractions ?? [InteractionType.Examine],
            Keywords = keywords ?? [name.ToLowerInvariant()],
            BlocksPassage = blocksPassage,
            BlockedDirection = blockedDirection,
            IsVisible = isVisible
        };
    }

    /// <summary>
    /// Creates an interactive object from a definition.
    /// </summary>
    /// <param name="definition">The definition to create from.</param>
    /// <returns>A new InteractiveObject instance.</returns>
    public static InteractiveObject FromDefinition(InteractiveObjectDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var obj = new InteractiveObject
        {
            Id = Guid.NewGuid(),
            DefinitionId = definition.Id.ToLowerInvariant(),
            Name = definition.Name,
            Description = definition.Description ?? string.Empty,
            ObjectType = definition.ObjectType,
            State = definition.DefaultState,
            DefaultState = definition.DefaultState,
            AllowedInteractions = definition.AllowedInteractions?.ToList() ?? [InteractionType.Examine],
            Keywords = definition.Keywords?.ToList() ?? [definition.Name.ToLowerInvariant()],
            BlocksPassage = definition.BlocksPassage,
            BlockedDirection = definition.BlockedDirection,
            IsVisible = definition.IsVisible
        };

        // Set up container if applicable
        if (definition.IsContainer && definition.ContainerCapacity > 0)
        {
            obj.SetupAsContainer(definition.ContainerCapacity);
        }

        // Set up lock if applicable
        if (definition.Lock.HasValue && definition.Lock.Value.HasLock)
        {
            obj.SetLock(definition.Lock.Value);
        }

        return obj;
    }

    /// <summary>
    /// Attempts to set the object state.
    /// </summary>
    /// <param name="newState">The new state to set.</param>
    /// <returns>True if state was changed; false if object is broken/destroyed.</returns>
    public bool TrySetState(ObjectState newState)
    {
        if (State == ObjectState.Broken || State == ObjectState.Destroyed)
            return false;

        State = newState;
        return true;
    }

    /// <summary>
    /// Resets the object to its default state.
    /// </summary>
    /// <returns>True if reset was successful; false if object cannot be reset.</returns>
    public bool Reset()
    {
        if (State == ObjectState.Broken || State == ObjectState.Destroyed)
            return false;

        State = DefaultState;
        return true;
    }

    /// <summary>
    /// Sets the visibility of this object.
    /// </summary>
    /// <param name="visible">Whether the object should be visible.</param>
    public void SetVisibility(bool visible)
    {
        IsVisible = visible;
    }

    /// <summary>
    /// Checks if an interaction is allowed.
    /// </summary>
    /// <param name="type">The interaction type to check.</param>
    /// <returns>True if the interaction is allowed.</returns>
    public bool CanPerformInteraction(InteractionType type) =>
        CanInteract && AllowedInteractions.Contains(type);

    /// <summary>
    /// Gets the default interaction based on current state.
    /// </summary>
    /// <returns>The default interaction type for the current state.</returns>
    public InteractionType GetDefaultInteraction()
    {
        return State switch
        {
            ObjectState.Closed when CanPerformInteraction(InteractionType.Open) => InteractionType.Open,
            ObjectState.Open when CanPerformInteraction(InteractionType.Close) => InteractionType.Close,
            ObjectState.Locked when CanPerformInteraction(InteractionType.Unlock) => InteractionType.Unlock,
            ObjectState.Inactive when CanPerformInteraction(InteractionType.Activate) => InteractionType.Activate,
            ObjectState.Active when CanPerformInteraction(InteractionType.Deactivate) => InteractionType.Deactivate,
            ObjectState.Down when CanPerformInteraction(InteractionType.Activate) => InteractionType.Activate,
            ObjectState.Up when CanPerformInteraction(InteractionType.Deactivate) => InteractionType.Deactivate,
            _ => InteractionType.Examine
        };
    }

    /// <summary>
    /// Checks if a keyword matches this object.
    /// </summary>
    /// <param name="keyword">The keyword to check.</param>
    /// <returns>True if the keyword matches.</returns>
    public bool MatchesKeyword(string keyword) =>
        Keywords.Any(k => k.Equals(keyword, StringComparison.OrdinalIgnoreCase)) ||
        Name.Contains(keyword, StringComparison.OrdinalIgnoreCase);

    // ===== Container Methods (v0.4.0b) =====

    /// <summary>
    /// Sets up this object as a container with the specified capacity.
    /// </summary>
    /// <param name="capacity">The container capacity.</param>
    public void SetupAsContainer(int capacity)
    {
        ContainerInventory = ContainerInventory.Create(capacity);
    }

    // ===== Lock Methods (v0.4.0b) =====

    /// <summary>
    /// Sets the lock definition for this object.
    /// </summary>
    /// <param name="lockDefinition">The lock definition.</param>
    public void SetLock(LockDefinition lockDefinition)
    {
        Lock = lockDefinition;
        if (lockDefinition.HasLock && State == ObjectState.Closed)
        {
            State = ObjectState.Locked;
        }
    }

    /// <summary>
    /// Attempts to unlock this object with a key.
    /// </summary>
    /// <param name="keyId">The key ID to try.</param>
    /// <returns>True if unlocked, false if key doesn't match.</returns>
    public bool TryUnlockWithKey(string keyId)
    {
        if (!IsLocked) return false;
        if (!HasLock) return false;
        if (!Lock.KeyMatches(keyId)) return false;

        State = ObjectState.Closed;
        return true;
    }

    /// <summary>
    /// Unlocks this object (e.g., after successful lockpick).
    /// </summary>
    /// <returns>True if state changed to unlocked.</returns>
    public bool Unlock()
    {
        if (!IsLocked) return false;

        State = ObjectState.Closed;
        return true;
    }

    /// <summary>
    /// Locks this object if it can be relocked.
    /// </summary>
    /// <returns>True if locked, false if cannot relock.</returns>
    public bool TryLock()
    {
        if (IsLocked) return false;
        if (!HasLock) return false;
        if (!Lock.CanRelock) return false;
        if (State != ObjectState.Closed) return false;

        State = ObjectState.Locked;
        return true;
    }
}
