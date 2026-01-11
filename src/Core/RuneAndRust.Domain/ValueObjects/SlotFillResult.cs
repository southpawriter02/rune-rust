namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Result of filling template slots during room instantiation.
/// </summary>
public readonly record struct SlotFillResult
{
    /// <summary>
    /// Gets whether all required slots were successfully filled.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Gets the filled slots with their content.
    /// </summary>
    public IReadOnlyDictionary<string, object> FilledSlots { get; init; }

    /// <summary>
    /// Gets the slot IDs that were not filled (optional slots that were skipped).
    /// </summary>
    public IReadOnlyList<string> UnfilledSlots { get; init; }

    /// <summary>
    /// Gets the slot IDs that failed to fill (required slots that couldn't be filled).
    /// </summary>
    public IReadOnlyList<string> FailedSlots { get; init; }

    /// <summary>
    /// Gets error messages for failed slots.
    /// </summary>
    public IReadOnlyDictionary<string, string> Errors { get; init; }

    /// <summary>
    /// Creates a successful result with the specified filled slots.
    /// </summary>
    public static SlotFillResult Success(
        IReadOnlyDictionary<string, object> filledSlots,
        IReadOnlyList<string>? unfilledSlots = null) => new()
    {
        IsSuccess = true,
        FilledSlots = filledSlots,
        UnfilledSlots = unfilledSlots ?? [],
        FailedSlots = [],
        Errors = new Dictionary<string, string>()
    };

    /// <summary>
    /// Creates a failed result with the specified errors.
    /// </summary>
    public static SlotFillResult Failure(
        IReadOnlyList<string> failedSlots,
        IReadOnlyDictionary<string, string> errors,
        IReadOnlyDictionary<string, object>? filledSlots = null) => new()
    {
        IsSuccess = false,
        FilledSlots = filledSlots ?? new Dictionary<string, object>(),
        UnfilledSlots = [],
        FailedSlots = failedSlots,
        Errors = errors
    };

    /// <summary>
    /// Gets an empty successful result.
    /// </summary>
    public static SlotFillResult Empty => Success(new Dictionary<string, object>());

    /// <summary>
    /// Gets the filled content for a specific slot.
    /// </summary>
    public T? GetContent<T>(string slotId) where T : class
    {
        if (FilledSlots.TryGetValue(slotId, out var content) && content is T typed)
            return typed;
        return null;
    }

    /// <summary>
    /// Gets the filled string content for a description slot.
    /// </summary>
    public string GetDescriptionContent(string slotId)
    {
        if (FilledSlots.TryGetValue(slotId, out var content) && content is string text)
            return text;
        return string.Empty;
    }

    /// <summary>
    /// Checks if a specific slot was filled.
    /// </summary>
    public bool HasContent(string slotId) => FilledSlots.ContainsKey(slotId);
}
