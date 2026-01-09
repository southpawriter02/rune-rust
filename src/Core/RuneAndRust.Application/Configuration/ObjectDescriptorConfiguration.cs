namespace RuneAndRust.Application.Configuration;

/// <summary>
/// Configuration for interactive object descriptor pools.
/// </summary>
public class ObjectDescriptorConfiguration
{
    /// <summary>
    /// Defined object types with their valid states.
    /// </summary>
    public IReadOnlyDictionary<string, ObjectTypeDefinition> ObjectTypes { get; init; } =
        new Dictionary<string, ObjectTypeDefinition>();
}

/// <summary>
/// Defines an interactive object type and its properties.
/// </summary>
public class ObjectTypeDefinition
{
    /// <summary>
    /// Object type identifier.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Display name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Valid states for this object type.
    /// </summary>
    public IReadOnlyList<string> ValidStates { get; init; } = [];

    /// <summary>
    /// Default state for new instances.
    /// </summary>
    public string DefaultState { get; init; } = "normal";

    /// <summary>
    /// Whether this object can be interacted with.
    /// </summary>
    public bool IsInteractable { get; init; } = true;
}
