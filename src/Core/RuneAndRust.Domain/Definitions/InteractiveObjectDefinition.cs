using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Configuration definition for an interactive object type.
/// </summary>
/// <remarks>
/// <para>
/// InteractiveObjectDefinition provides the template for creating InteractiveObject
/// instances from JSON configuration. Each definition specifies the object type,
/// allowed interactions, and default state.
/// </para>
/// <para>
/// Definitions are loaded from config/interactive-objects.json and are immutable
/// after loading. Use <see cref="Entities.InteractiveObject.FromDefinition"/> to create
/// instances from definitions.
/// </para>
/// </remarks>
public class InteractiveObjectDefinition
{
    /// <summary>
    /// Gets or sets the unique identifier for this definition.
    /// </summary>
    /// <example>"iron-door-basic"</example>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name for objects created from this definition.
    /// </summary>
    /// <example>"Heavy Iron Door"</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description for objects created from this definition.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the type of interactive object.
    /// </summary>
    public InteractiveObjectType ObjectType { get; set; }

    /// <summary>
    /// Gets or sets the default state for new objects.
    /// </summary>
    public ObjectState DefaultState { get; set; } = ObjectState.Normal;

    /// <summary>
    /// Gets or sets the allowed interaction types.
    /// </summary>
    public List<InteractionType>? AllowedInteractions { get; set; }

    /// <summary>
    /// Gets or sets whether this object blocks passage when closed/locked.
    /// </summary>
    public bool BlocksPassage { get; set; }

    /// <summary>
    /// Gets or sets the direction this object blocks (for doors).
    /// </summary>
    public Direction? BlockedDirection { get; set; }

    /// <summary>
    /// Gets or sets keywords that can be used to reference this object.
    /// </summary>
    public List<string>? Keywords { get; set; }

    /// <summary>
    /// Gets or sets whether this object is visible by default.
    /// </summary>
    public bool IsVisible { get; set; } = true;

    // ===== Container Configuration (v0.4.0b) =====

    /// <summary>
    /// Gets or sets whether this object is a container.
    /// </summary>
    public bool IsContainer { get; set; }

    /// <summary>
    /// Gets or sets the container capacity (for containers).
    /// </summary>
    public int ContainerCapacity { get; set; }

    // ===== Lock Configuration (v0.4.0b) =====

    /// <summary>
    /// Gets or sets the lock definition for this object.
    /// </summary>
    public LockDefinition? Lock { get; set; }
}
