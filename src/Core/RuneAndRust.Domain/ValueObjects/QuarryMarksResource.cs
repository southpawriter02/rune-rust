using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the Quarry Marks tracking resource for the Veiðimaðr (Hunter) specialization.
/// Maintains up to 3 active marks on targets with FIFO (First In, First Out) replacement
/// when the maximum is exceeded.
/// </summary>
/// <remarks>
/// <para>Quarry Marks are the Veiðimaðr's special resource, fundamentally different from
/// other specialization resources:</para>
/// <list type="bullet">
/// <item>Unlike <see cref="RageResource"/> (numeric meter 0–100), marks are discrete target-tracking objects</item>
/// <item>Unlike Medical Supplies (consumable inventory), marks are not consumed — they persist until cleared</item>
/// <item>Maximum 3 simultaneous marks; oldest replaced via FIFO when exceeded</item>
/// <item>All marks cleared at encounter end</item>
/// </list>
/// <para>This resource uses a mutable model (like <see cref="RageResource"/>) with a private
/// <see cref="List{T}"/> backing store and <see cref="IReadOnlyList{T}"/> public accessor,
/// since marks are frequently added/removed during combat.</para>
/// <para>Created via <see cref="Create"/> factory method. Introduced in v0.20.7a.</para>
/// </remarks>
public sealed record QuarryMarksResource
{
    /// <summary>
    /// Default maximum number of simultaneous Quarry Marks allowed.
    /// </summary>
    public const int DefaultMaxMarks = 3;

    /// <summary>
    /// Backing store for active marks. Maintained in creation order (oldest first at index 0).
    /// </summary>
    private readonly List<QuarryMark> _activeMarks = new();

    /// <summary>
    /// Gets the collection of currently active Quarry Marks (0–3 marks).
    /// Maintained in creation order (oldest first).
    /// </summary>
    public IReadOnlyList<QuarryMark> ActiveMarks => _activeMarks.AsReadOnly();

    /// <summary>
    /// Maximum number of simultaneous Quarry Marks allowed (default 3).
    /// </summary>
    public int MaxMarks { get; init; } = DefaultMaxMarks;

    /// <summary>
    /// Current number of active marks (0–3). Computed from <see cref="ActiveMarks"/>.
    /// </summary>
    public int CurrentMarkCount => _activeMarks.Count;

    /// <summary>
    /// UTC timestamp of when the most recent mark was created.
    /// Null if no marks have been created yet.
    /// </summary>
    public DateTime? LastMarkGainedAt { get; private set; }

    /// <summary>
    /// UTC timestamp of the last resource state change (mark added, removed, or cleared).
    /// </summary>
    public DateTime LastUpdatedAt { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a new empty QuarryMarksResource with default maximum of 3 marks.
    /// </summary>
    /// <returns>A new <see cref="QuarryMarksResource"/> instance with 0 active marks.</returns>
    public static QuarryMarksResource Create()
    {
        return new QuarryMarksResource();
    }

    /// <summary>
    /// Adds a new Quarry Mark. If at maximum capacity, the oldest mark is replaced (FIFO).
    /// </summary>
    /// <param name="mark">The mark to add. Must not be null.</param>
    /// <returns>
    /// The replaced <see cref="QuarryMark"/> if a mark was removed to make room (FIFO replacement),
    /// or null if a mark was added without replacement.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="mark"/> is null.</exception>
    public QuarryMark? AddMark(QuarryMark mark)
    {
        ArgumentNullException.ThrowIfNull(mark);

        QuarryMark? replacedMark = null;

        // If at capacity, remove oldest mark (FIFO: index 0 is oldest)
        if (_activeMarks.Count >= MaxMarks)
        {
            replacedMark = _activeMarks[0];
            _activeMarks.RemoveAt(0);
        }

        _activeMarks.Add(mark);
        LastMarkGainedAt = DateTime.UtcNow;
        LastUpdatedAt = DateTime.UtcNow;

        return replacedMark;
    }

    /// <summary>
    /// Removes a specific mark by target ID.
    /// </summary>
    /// <param name="targetId">The target ID whose mark should be removed.</param>
    /// <returns>True if a mark was found and removed; false if no mark existed for the target.</returns>
    public bool RemoveMark(Guid targetId)
    {
        var markToRemove = _activeMarks.FirstOrDefault(m => m.TargetId == targetId);
        if (markToRemove == null)
            return false;

        _activeMarks.Remove(markToRemove);
        LastUpdatedAt = DateTime.UtcNow;
        return true;
    }

    /// <summary>
    /// Checks if a target is currently marked with an Active status.
    /// </summary>
    /// <param name="targetId">The target ID to check.</param>
    /// <returns>True if the target has an active Quarry Mark.</returns>
    public bool HasMark(Guid targetId)
    {
        return _activeMarks.Any(m => m.TargetId == targetId && m.Status == QuarryStatus.Active);
    }

    /// <summary>
    /// Gets a specific mark for a target, or null if not marked.
    /// </summary>
    /// <param name="targetId">The target ID to look up.</param>
    /// <returns>The <see cref="QuarryMark"/> for the target, or null if not found.</returns>
    public QuarryMark? GetMark(Guid targetId)
    {
        return _activeMarks.FirstOrDefault(m => m.TargetId == targetId);
    }

    /// <summary>
    /// Gets the oldest mark in the FIFO queue (the next mark that would be replaced).
    /// </summary>
    /// <returns>The oldest <see cref="QuarryMark"/>, or null if no marks exist.</returns>
    public QuarryMark? GetOldestMark()
    {
        return _activeMarks.Count > 0 ? _activeMarks[0] : null;
    }

    /// <summary>
    /// Checks if another mark can be added without replacing an existing one.
    /// </summary>
    /// <returns>True if the current mark count is below the maximum.</returns>
    public bool CanAddMark()
    {
        return _activeMarks.Count < MaxMarks;
    }

    /// <summary>
    /// Clears all marks. Called at encounter end or when resource is reset.
    /// </summary>
    public void ClearAllMarks()
    {
        _activeMarks.Clear();
        LastUpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes all marks that have expired (status == Expired) and returns the count removed.
    /// In v0.20.7a, marks do not auto-expire by turn count. This method is provided
    /// for Tier 2+ abilities that may add expiration mechanics.
    /// </summary>
    /// <returns>The number of expired marks removed.</returns>
    public int ClearExpiredMarks()
    {
        var expiredCount = _activeMarks.Count(m => m.IsExpired());
        _activeMarks.RemoveAll(m => m.IsExpired());
        if (expiredCount > 0)
            LastUpdatedAt = DateTime.UtcNow;
        return expiredCount;
    }

    /// <summary>
    /// Refreshes all marks for a new turn: increments <see cref="QuarryMark.TurnsActive"/>
    /// on each active mark. Called at the start of each hunter turn.
    /// </summary>
    public void RefreshMarksForNewTurn()
    {
        foreach (var mark in _activeMarks)
        {
            mark.IncrementTurn();
        }
        LastUpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Returns a formatted string showing current marks vs maximum (e.g., "2/3").
    /// </summary>
    /// <returns>A formatted resource value string.</returns>
    public string GetFormattedValue()
    {
        return $"{CurrentMarkCount}/{MaxMarks}";
    }
}
