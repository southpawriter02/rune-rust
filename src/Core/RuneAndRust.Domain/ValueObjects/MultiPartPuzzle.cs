namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Configuration for a puzzle spanning multiple rooms or objects.
/// </summary>
/// <remarks>
/// <para>
/// Multi-part puzzles track components distributed across multiple rooms.
/// Players must solve all components (possibly in order) to complete the master puzzle.
/// </para>
/// <list type="bullet">
///   <item><description>Component puzzles can be in different rooms</description></item>
///   <item><description>Optional solve order requirements</description></item>
///   <item><description>Completion tracking for master puzzle rewards</description></item>
/// </list>
/// </remarks>
public class MultiPartPuzzle
{
    // ===== Core Properties =====

    /// <summary>
    /// Gets the master puzzle ID.
    /// </summary>
    public string MasterPuzzleId { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the IDs of component puzzles that must be solved.
    /// </summary>
    public IReadOnlyList<string> ComponentPuzzleIds { get; private set; } = Array.Empty<string>();

    /// <summary>
    /// Gets whether components must be solved in order.
    /// </summary>
    public bool RequiresOrder { get; private set; }

    /// <summary>
    /// Gets the required order (if applicable).
    /// </summary>
    public IReadOnlyList<string> RequiredOrder { get; private set; } = Array.Empty<string>();

    /// <summary>
    /// Gets room IDs where components are located.
    /// </summary>
    public IReadOnlyDictionary<string, Guid> ComponentRooms { get; private set; }
        = new Dictionary<string, Guid>();

    // ===== State Properties =====

    /// <summary>
    /// Gets the number of components solved.
    /// </summary>
    public int SolvedCount { get; private set; }

    /// <summary>
    /// Gets IDs of solved components.
    /// </summary>
    private readonly List<string> _solvedComponents = [];
    public IReadOnlyList<string> SolvedComponents => _solvedComponents;

    // ===== Computed Properties =====

    /// <summary>
    /// Checks if the multi-part puzzle is complete.
    /// </summary>
    public bool IsComplete => SolvedCount == ComponentPuzzleIds.Count;

    /// <summary>
    /// Gets the total number of components.
    /// </summary>
    public int TotalComponents => ComponentPuzzleIds.Count;

    /// <summary>
    /// Gets the completion percentage.
    /// </summary>
    public int CompletionPercent => TotalComponents > 0 ? (SolvedCount * 100) / TotalComponents : 0;

    // ===== Constructors =====

    /// <summary>
    /// Private parameterless constructor for EF Core.
    /// </summary>
    private MultiPartPuzzle() { }

    // ===== Factory Methods =====

    /// <summary>
    /// Creates a multi-part puzzle configuration.
    /// </summary>
    /// <param name="masterPuzzleId">The master puzzle ID.</param>
    /// <param name="componentIds">IDs of component puzzles.</param>
    /// <param name="requiresOrder">Whether order matters.</param>
    /// <param name="requiredOrder">The required order (if applicable).</param>
    /// <param name="componentRooms">Room locations for components.</param>
    /// <returns>A new MultiPartPuzzle instance.</returns>
    public static MultiPartPuzzle Create(
        string masterPuzzleId,
        IEnumerable<string> componentIds,
        bool requiresOrder = false,
        IEnumerable<string>? requiredOrder = null,
        IDictionary<string, Guid>? componentRooms = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(masterPuzzleId);

        var componentList = componentIds.ToList();
        if (componentList.Count == 0)
            throw new ArgumentException("At least one component is required.", nameof(componentIds));

        return new MultiPartPuzzle
        {
            MasterPuzzleId = masterPuzzleId,
            ComponentPuzzleIds = componentList,
            RequiresOrder = requiresOrder,
            RequiredOrder = (IReadOnlyList<string>?)requiredOrder?.ToList() ?? Array.Empty<string>(),
            ComponentRooms = componentRooms?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                ?? new Dictionary<string, Guid>()
        };
    }

    // ===== Component Tracking Methods =====

    /// <summary>
    /// Records a component as solved.
    /// </summary>
    /// <param name="componentId">The component puzzle ID.</param>
    /// <returns>True if successfully recorded, false if invalid or out of order.</returns>
    public bool RecordComponentSolved(string componentId)
    {
        if (string.IsNullOrWhiteSpace(componentId))
            return false;

        if (!ComponentPuzzleIds.Contains(componentId))
            return false;

        if (_solvedComponents.Contains(componentId))
            return false;

        if (RequiresOrder && RequiredOrder.Count > 0)
        {
            var expectedNext = RequiredOrder[SolvedCount];
            if (!componentId.Equals(expectedNext, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        _solvedComponents.Add(componentId);
        SolvedCount++;
        return true;
    }

    /// <summary>
    /// Checks if a specific component is solved.
    /// </summary>
    /// <param name="componentId">The component ID to check.</param>
    /// <returns>True if the component is solved.</returns>
    public bool IsComponentSolved(string componentId)
    {
        return _solvedComponents.Contains(componentId);
    }

    /// <summary>
    /// Gets the next expected component (if ordered).
    /// </summary>
    /// <returns>The next expected component ID, or null if unordered or complete.</returns>
    public string? GetNextExpectedComponent()
    {
        if (!RequiresOrder || IsComplete || RequiredOrder.Count == 0)
            return null;

        return SolvedCount < RequiredOrder.Count ? RequiredOrder[SolvedCount] : null;
    }

    /// <summary>
    /// Gets remaining unsolved components.
    /// </summary>
    /// <returns>IDs of unsolved component puzzles.</returns>
    public IEnumerable<string> GetRemainingComponents()
    {
        return ComponentPuzzleIds.Except(_solvedComponents);
    }

    /// <summary>
    /// Gets the room ID for a component.
    /// </summary>
    /// <param name="componentId">The component puzzle ID.</param>
    /// <returns>The room GUID, or null if not mapped.</returns>
    public Guid? GetComponentRoom(string componentId)
    {
        return ComponentRooms.TryGetValue(componentId, out var roomId) ? roomId : null;
    }

    /// <summary>
    /// Resets all solved components.
    /// </summary>
    public void Reset()
    {
        _solvedComponents.Clear();
        SolvedCount = 0;
    }

    /// <summary>
    /// Returns a string representation of this multi-part puzzle.
    /// </summary>
    public override string ToString() =>
        $"MultiPartPuzzle({MasterPuzzleId}, {SolvedCount}/{TotalComponents}, Order={RequiresOrder})";
}
