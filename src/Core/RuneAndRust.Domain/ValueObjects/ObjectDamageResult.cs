namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Entities;

/// <summary>
/// Represents the result of damaging an interactive object.
/// </summary>
/// <remarks>
/// ObjectDamageResult captures the outcome of attacking a destructible object,
/// including damage dealt, destruction status, dropped items, and passage clearing.
/// </remarks>
public readonly record struct ObjectDamageResult
{
    /// <summary>
    /// Gets whether the attack was successful (damage could be applied).
    /// </summary>
    /// <remarks>
    /// An attack can "succeed" even if it dealt 0 damage (e.g., immunity).
    /// Failure indicates the object cannot be damaged at all.
    /// </remarks>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the message to display to the player.
    /// </summary>
    public string Message { get; init; }

    /// <summary>
    /// Gets the damage dealt to the object.
    /// </summary>
    public int DamageDealt { get; init; }

    /// <summary>
    /// Gets whether the object was destroyed by this attack.
    /// </summary>
    public bool WasDestroyed { get; init; }

    /// <summary>
    /// Gets items dropped from the destruction (if any).
    /// </summary>
    /// <remarks>
    /// Items come from container contents or loot tables.
    /// Empty if object wasn't destroyed or had no loot.
    /// </remarks>
    public IReadOnlyList<Item> DroppedItems { get; init; }

    /// <summary>
    /// Gets whether passage was cleared by the destruction.
    /// </summary>
    /// <remarks>
    /// True if the destroyed object was previously blocking passage.
    /// </remarks>
    public bool PassageCleared { get; init; }

    /// <summary>
    /// Gets whether this attack dealt no damage due to immunity.
    /// </summary>
    public bool WasImmune => Success && DamageDealt == 0 && !WasDestroyed;

    /// <summary>
    /// Creates a successful damage result (hit).
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="damage">The damage dealt.</param>
    /// <param name="destroyed">Whether the object was destroyed.</param>
    /// <param name="droppedItems">Items dropped from destruction.</param>
    /// <param name="passageCleared">Whether passage was cleared.</param>
    /// <returns>A successful ObjectDamageResult.</returns>
    public static ObjectDamageResult Hit(
        string message,
        int damage,
        bool destroyed = false,
        IEnumerable<Item>? droppedItems = null,
        bool passageCleared = false) => new()
    {
        Success = true,
        Message = message,
        DamageDealt = damage,
        WasDestroyed = destroyed,
        DroppedItems = droppedItems?.ToList() ?? [],
        PassageCleared = passageCleared
    };

    /// <summary>
    /// Creates a result for damage that destroyed the object.
    /// </summary>
    /// <param name="objectName">The name of the destroyed object.</param>
    /// <param name="damage">The damage dealt.</param>
    /// <param name="droppedItems">Items dropped from destruction.</param>
    /// <param name="passageCleared">Whether passage was cleared.</param>
    /// <returns>A destruction ObjectDamageResult.</returns>
    public static ObjectDamageResult Destroyed(
        string objectName,
        int damage,
        IEnumerable<Item>? droppedItems = null,
        bool passageCleared = false)
    {
        var message = $"The {objectName} is destroyed!";
        if (passageCleared)
            message += " The passage is now clear.";

        return Hit(message, damage, destroyed: true, droppedItems, passageCleared);
    }

    /// <summary>
    /// Creates a result for immune damage.
    /// </summary>
    /// <param name="objectName">The name of the object.</param>
    /// <param name="damageType">The damage type that was immune.</param>
    /// <returns>An immunity ObjectDamageResult.</returns>
    public static ObjectDamageResult Immune(string objectName, string damageType) => new()
    {
        Success = true, // Attack succeeded, just did no damage
        Message = $"The {objectName} is immune to {damageType} damage!",
        DamageDealt = 0,
        WasDestroyed = false,
        DroppedItems = [],
        PassageCleared = false
    };

    /// <summary>
    /// Creates a failed result (object not destructible).
    /// </summary>
    /// <param name="objectName">The name of the object.</param>
    /// <returns>A failure ObjectDamageResult.</returns>
    public static ObjectDamageResult NotDestructible(string objectName) => new()
    {
        Success = false,
        Message = $"The {objectName} cannot be destroyed.",
        DamageDealt = 0,
        WasDestroyed = false,
        DroppedItems = [],
        PassageCleared = false
    };

    /// <summary>
    /// Creates a result for already destroyed objects.
    /// </summary>
    /// <param name="objectName">The name of the object.</param>
    /// <returns>A failure ObjectDamageResult.</returns>
    public static ObjectDamageResult AlreadyDestroyed(string objectName) => new()
    {
        Success = false,
        Message = $"The {objectName} is already destroyed.",
        DamageDealt = 0,
        WasDestroyed = false,
        DroppedItems = [],
        PassageCleared = false
    };

    /// <summary>
    /// Creates a result when the object resisted some damage.
    /// </summary>
    /// <param name="objectName">The name of the object.</param>
    /// <param name="damage">The reduced damage dealt.</param>
    /// <param name="damageType">The damage type that was resisted.</param>
    /// <returns>A resistance ObjectDamageResult.</returns>
    public static ObjectDamageResult Resisted(string objectName, int damage, string damageType) =>
        Hit($"The {objectName} resists the {damageType} damage! ({damage} damage)", damage);

    /// <summary>
    /// Creates a result when the object was vulnerable to damage.
    /// </summary>
    /// <param name="objectName">The name of the object.</param>
    /// <param name="damage">The increased damage dealt.</param>
    /// <param name="damageType">The damage type that was effective.</param>
    /// <param name="destroyed">Whether the object was destroyed.</param>
    /// <param name="droppedItems">Items dropped if destroyed.</param>
    /// <param name="passageCleared">Whether passage was cleared.</param>
    /// <returns>A vulnerability ObjectDamageResult.</returns>
    public static ObjectDamageResult Vulnerable(
        string objectName,
        int damage,
        string damageType,
        bool destroyed = false,
        IEnumerable<Item>? droppedItems = null,
        bool passageCleared = false)
    {
        var message = $"The {objectName} is vulnerable to {damageType}! ({damage} damage)";
        if (destroyed)
            message = $"The {objectName} is vulnerable to {damageType} and shatters! ({damage} damage)";
        if (passageCleared)
            message += " The passage is now clear.";

        return Hit(message, damage, destroyed, droppedItems, passageCleared);
    }

    /// <summary>
    /// Returns a string representation of this result.
    /// </summary>
    /// <returns>A string describing the result.</returns>
    public override string ToString() =>
        WasDestroyed
            ? $"Destroyed ({DamageDealt} damage)"
            : Success
                ? $"Hit for {DamageDealt} damage"
                : $"Failed: {Message}";
}
