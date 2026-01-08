using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// A single entry in the combat log tracking events during an encounter.
/// </summary>
/// <remarks>
/// <para>CombatLogEntry records individual events during combat for display and history.</para>
/// <para>Entries are immutable once created and include:</para>
/// <list type="bullet">
/// <item>Timestamp and round number for ordering</item>
/// <item>Type categorization for filtering and display</item>
/// <item>Actor and target information</item>
/// <item>Numeric data (damage, healing) where applicable</item>
/// <item>Critical/miss flags for special formatting</item>
/// </list>
/// </remarks>
public class CombatLogEntry : IEntity
{
    /// <summary>Gets the unique identifier for this entry.</summary>
    public Guid Id { get; private set; }

    /// <summary>Gets the timestamp when the event occurred.</summary>
    public DateTime Timestamp { get; private set; }

    /// <summary>Gets the round number when this occurred.</summary>
    public int RoundNumber { get; private set; }

    /// <summary>Gets the type of log entry.</summary>
    public CombatLogType Type { get; private set; }

    /// <summary>Gets the message to display.</summary>
    public string Message { get; private set; } = string.Empty;

    /// <summary>Gets the combatant who performed the action (if any).</summary>
    public string? ActorName { get; private set; }

    /// <summary>Gets the target of the action (if any).</summary>
    public string? TargetName { get; private set; }

    /// <summary>Gets the damage dealt (if applicable).</summary>
    public int? Damage { get; private set; }

    /// <summary>Gets the healing done (if applicable).</summary>
    public int? Healing { get; private set; }

    /// <summary>Gets whether this was a critical hit/success.</summary>
    public bool IsCritical { get; private set; }

    /// <summary>Gets whether this was a miss/failure.</summary>
    public bool IsMiss { get; private set; }

    /// <summary>
    /// Private parameterless constructor for Entity Framework Core.
    /// </summary>
    private CombatLogEntry() { }

    /// <summary>
    /// Creates a combat log entry with the specified parameters.
    /// </summary>
    /// <param name="roundNumber">The current round number.</param>
    /// <param name="type">The type of event.</param>
    /// <param name="message">The display message.</param>
    /// <param name="actorName">The actor's name (optional).</param>
    /// <param name="targetName">The target's name (optional).</param>
    /// <param name="damage">Damage dealt (optional).</param>
    /// <param name="healing">Healing done (optional).</param>
    /// <param name="isCritical">Whether this was a critical (optional).</param>
    /// <param name="isMiss">Whether this was a miss (optional).</param>
    /// <returns>A new combat log entry.</returns>
    public static CombatLogEntry Create(
        int roundNumber,
        CombatLogType type,
        string message,
        string? actorName = null,
        string? targetName = null,
        int? damage = null,
        int? healing = null,
        bool isCritical = false,
        bool isMiss = false)
    {
        return new CombatLogEntry
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            RoundNumber = roundNumber,
            Type = type,
            Message = message,
            ActorName = actorName,
            TargetName = targetName,
            Damage = damage,
            Healing = healing,
            IsCritical = isCritical,
            IsMiss = isMiss
        };
    }

    /// <summary>
    /// Returns a formatted string representation of this entry.
    /// </summary>
    public override string ToString() =>
        $"[R{RoundNumber}] {Type}: {Message}";
}
