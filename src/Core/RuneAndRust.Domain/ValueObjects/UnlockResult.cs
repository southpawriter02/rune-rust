namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the result of an unlock attempt.
/// </summary>
/// <remarks>
/// <para>
/// UnlockResult captures the outcome of attempting to unlock an object,
/// whether by key or lockpicking. It includes the method used, success status,
/// and relevant details like consumed keys or skill check results.
/// </para>
/// <para>
/// Use factory methods to create appropriate results for different scenarios.
/// </para>
/// </remarks>
public readonly record struct UnlockResult
{
    /// <summary>
    /// Gets whether the unlock was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the message to display to the player.
    /// </summary>
    public string Message { get; init; }

    /// <summary>
    /// Gets the method used to unlock (Key, Lockpick, or None).
    /// </summary>
    public UnlockMethod Method { get; init; }

    /// <summary>
    /// Gets whether a key was consumed in the process.
    /// </summary>
    public bool KeyConsumed { get; init; }

    /// <summary>
    /// Gets the name of the key used (if applicable).
    /// </summary>
    public string? KeyUsed { get; init; }

    /// <summary>
    /// Gets the roll result from lockpicking (if applicable).
    /// </summary>
    public int? RollResult { get; init; }

    /// <summary>
    /// Gets the DC that was checked against (if applicable).
    /// </summary>
    public int? DifficultyClass { get; init; }

    /// <summary>
    /// Creates a successful unlock result using a key.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="keyName">The name of the key used.</param>
    /// <param name="consumed">Whether the key was consumed.</param>
    /// <returns>A successful unlock result.</returns>
    public static UnlockResult SuccessWithKey(string message, string keyName, bool consumed) => new()
    {
        Success = true,
        Message = message,
        Method = UnlockMethod.Key,
        KeyConsumed = consumed,
        KeyUsed = keyName
    };

    /// <summary>
    /// Creates a successful unlock result using lockpicking.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="rollResult">The total roll result.</param>
    /// <param name="dc">The difficulty class.</param>
    /// <returns>A successful unlock result.</returns>
    public static UnlockResult SuccessWithLockpick(string message, int rollResult, int dc) => new()
    {
        Success = true,
        Message = message,
        Method = UnlockMethod.Lockpick,
        RollResult = rollResult,
        DifficultyClass = dc
    };

    /// <summary>
    /// Creates a failed unlock result.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="method">The method that was attempted.</param>
    /// <param name="rollResult">Optional roll result for lockpicking.</param>
    /// <param name="dc">Optional difficulty class for lockpicking.</param>
    /// <returns>A failed unlock result.</returns>
    public static UnlockResult Failed(string message, UnlockMethod method = UnlockMethod.None,
        int? rollResult = null, int? dc = null) => new()
    {
        Success = false,
        Message = message,
        Method = method,
        RollResult = rollResult,
        DifficultyClass = dc
    };

    /// <summary>
    /// Creates result for when object is not locked.
    /// </summary>
    /// <param name="objectName">The name of the object.</param>
    /// <returns>A failed result indicating object is not locked.</returns>
    public static UnlockResult NotLocked(string objectName) =>
        Failed($"The {objectName} is not locked.");

    /// <summary>
    /// Creates result for when no matching key is found.
    /// </summary>
    /// <param name="objectName">The name of the object.</param>
    /// <returns>A failed result indicating no key found.</returns>
    public static UnlockResult NoKey(string objectName) =>
        Failed($"You don't have a key that fits this lock.", UnlockMethod.Key);

    /// <summary>
    /// Creates result for when lock cannot be picked.
    /// </summary>
    /// <param name="objectName">The name of the object.</param>
    /// <returns>A failed result indicating lock cannot be picked.</returns>
    public static UnlockResult CannotPick(string objectName) =>
        Failed($"This lock cannot be picked.", UnlockMethod.Lockpick);
}

/// <summary>
/// Methods of unlocking a locked object.
/// </summary>
public enum UnlockMethod
{
    /// <summary>No method (failed or not applicable).</summary>
    None,

    /// <summary>Unlocked using a key.</summary>
    Key,

    /// <summary>Unlocked using lockpicking skill.</summary>
    Lockpick
}
