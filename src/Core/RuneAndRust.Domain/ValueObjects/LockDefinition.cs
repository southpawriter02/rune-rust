namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Defines the lock properties of an interactive object.
/// </summary>
/// <remarks>
/// <para>
/// LockDefinition specifies how an object can be locked and unlocked, including
/// key requirements and lockpicking difficulty. Use factory methods to create
/// common lock configurations.
/// </para>
/// <para>
/// This is an immutable value object. Use <see cref="None"/> for objects without locks,
/// <see cref="KeyOnly"/> for key-only locks, or <see cref="Pickable"/> for lockpickable locks.
/// </para>
/// </remarks>
public readonly record struct LockDefinition
{
    /// <summary>
    /// Gets the lock ID that keys must match.
    /// </summary>
    public string LockId { get; init; }

    /// <summary>
    /// Gets the required key ID to unlock (null if lockpickable only).
    /// </summary>
    public string? RequiredKeyId { get; init; }

    /// <summary>
    /// Gets whether this lock can be picked.
    /// </summary>
    public bool IsLockpickable { get; init; }

    /// <summary>
    /// Gets the lockpicking difficulty (dice check DC).
    /// </summary>
    public int LockpickDC { get; init; }

    /// <summary>
    /// Gets whether the key is consumed when used.
    /// </summary>
    public bool KeyConsumedOnUse { get; init; }

    /// <summary>
    /// Gets whether the lock can be re-locked after opening.
    /// </summary>
    public bool CanRelock { get; init; }

    /// <summary>
    /// Gets whether this lock has been permanently unlocked.
    /// </summary>
    public bool IsPermanentlyUnlocked { get; init; }

    /// <summary>
    /// Gets whether this is a valid lock definition.
    /// </summary>
    public bool HasLock => !string.IsNullOrEmpty(LockId);

    /// <summary>
    /// Gets an empty lock definition (no lock).
    /// </summary>
    public static LockDefinition None => new()
    {
        LockId = string.Empty,
        RequiredKeyId = null,
        IsLockpickable = false,
        LockpickDC = 0,
        KeyConsumedOnUse = false,
        CanRelock = false
    };

    /// <summary>
    /// Creates a key-only lock (cannot be picked).
    /// </summary>
    /// <param name="lockId">The lock identifier.</param>
    /// <param name="keyId">The required key identifier.</param>
    /// <param name="consumeKey">Whether to consume the key on use.</param>
    /// <param name="canRelock">Whether the lock can be relocked.</param>
    /// <returns>A new LockDefinition.</returns>
    public static LockDefinition KeyOnly(
        string lockId,
        string keyId,
        bool consumeKey = false,
        bool canRelock = true) => new()
    {
        LockId = lockId,
        RequiredKeyId = keyId,
        IsLockpickable = false,
        LockpickDC = 0,
        KeyConsumedOnUse = consumeKey,
        CanRelock = canRelock
    };

    /// <summary>
    /// Creates a lockpickable lock.
    /// </summary>
    /// <param name="lockId">The lock identifier.</param>
    /// <param name="dc">The lockpicking difficulty class.</param>
    /// <param name="keyId">Optional key that also opens this lock.</param>
    /// <param name="canRelock">Whether the lock can be relocked.</param>
    /// <returns>A new LockDefinition.</returns>
    public static LockDefinition Pickable(
        string lockId,
        int dc,
        string? keyId = null,
        bool canRelock = true) => new()
    {
        LockId = lockId,
        RequiredKeyId = keyId,
        IsLockpickable = true,
        LockpickDC = dc,
        KeyConsumedOnUse = false,
        CanRelock = canRelock
    };

    /// <summary>
    /// Creates a lock that can be opened by key or lockpicking.
    /// </summary>
    /// <param name="lockId">The lock identifier.</param>
    /// <param name="keyId">The required key identifier.</param>
    /// <param name="dc">The lockpicking difficulty class.</param>
    /// <param name="canRelock">Whether the lock can be relocked.</param>
    /// <returns>A new LockDefinition.</returns>
    public static LockDefinition KeyOrPick(
        string lockId,
        string keyId,
        int dc,
        bool canRelock = true) => new()
    {
        LockId = lockId,
        RequiredKeyId = keyId,
        IsLockpickable = true,
        LockpickDC = dc,
        KeyConsumedOnUse = false,
        CanRelock = canRelock
    };

    /// <summary>
    /// Checks if a key matches this lock.
    /// </summary>
    /// <param name="keyId">The key ID to check.</param>
    /// <returns>True if the key matches this lock.</returns>
    public bool KeyMatches(string keyId)
    {
        if (string.IsNullOrEmpty(RequiredKeyId)) return false;
        return RequiredKeyId.Equals(keyId, StringComparison.OrdinalIgnoreCase);
    }
}
