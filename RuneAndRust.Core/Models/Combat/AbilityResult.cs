namespace RuneAndRust.Core.Models.Combat;

/// <summary>
/// Represents the result of executing an ability during combat.
/// Contains success/failure status, narrative message for UI, and effect totals.
/// </summary>
/// <param name="Success">Whether the ability was successfully executed.</param>
/// <param name="Message">Narrative message describing what happened (for combat log).</param>
/// <param name="TotalDamage">Total damage dealt by the ability (0 if none).</param>
/// <param name="TotalHealing">Total healing done by the ability (0 if none).</param>
/// <param name="StatusesApplied">List of status effect names that were applied (null if none).</param>
public record AbilityResult(
    bool Success,
    string Message,
    int TotalDamage = 0,
    int TotalHealing = 0,
    List<string>? StatusesApplied = null)
{
    /// <summary>
    /// Creates a failure result with the specified reason.
    /// </summary>
    /// <param name="reason">The reason the ability failed.</param>
    /// <returns>A failure AbilityResult.</returns>
    public static AbilityResult Failure(string reason) => new(false, reason);

    /// <summary>
    /// Creates a successful result with the specified message and optional effect totals.
    /// </summary>
    /// <param name="message">The narrative message describing the ability's effect.</param>
    /// <param name="damage">Total damage dealt (default 0).</param>
    /// <param name="healing">Total healing done (default 0).</param>
    /// <param name="statuses">List of applied status effects (default null).</param>
    /// <returns>A successful AbilityResult.</returns>
    public static AbilityResult Ok(string message, int damage = 0, int healing = 0, List<string>? statuses = null)
        => new(true, message, damage, healing, statuses);
}
