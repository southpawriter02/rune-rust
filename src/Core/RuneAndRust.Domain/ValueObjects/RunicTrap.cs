using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a hidden runic trap placed on the battlefield by the Rúnasmiðr.
/// Deals 3d6 damage when triggered by an enemy entering the trap's space.
/// </summary>
/// <remarks>
/// <para>Runic Trap is a Tier 2 Rúnasmiðr ability (v0.20.2b) providing area denial
/// and burst damage capabilities:</para>
/// <list type="bullet">
/// <item>Cost: 3 AP + 2 Rune Charges</item>
/// <item>Deals 3d6 damage when triggered</item>
/// <item>Invisible to enemies (DC 14 Perception to detect)</item>
/// <item>Maximum 3 active traps per character</item>
/// <item>Expires after 1 hour if not triggered</item>
/// <item>Destroyed after triggering (one use only)</item>
/// <item>Allies can see traps and don't trigger them</item>
/// </list>
/// <para>Follows the same sealed record pattern as <see cref="RunestoneWard"/> for consistency
/// across Rúnasmiðr effect types.</para>
/// </remarks>
public sealed record RunicTrap
{
    /// <summary>
    /// Default damage dice for Runic Trap.
    /// </summary>
    public const string DefaultDamage = "3d6";

    /// <summary>
    /// Default Perception DC to detect the trap.
    /// </summary>
    public const int DefaultDetectionDc = 14;

    /// <summary>
    /// Maximum number of active traps a single character can maintain.
    /// </summary>
    public const int MaxActiveTraps = 3;

    /// <summary>
    /// Default expiration time in hours after placement.
    /// </summary>
    public const int DefaultExpirationHours = 1;

    /// <summary>
    /// Unique identifier for this trap instance.
    /// </summary>
    public Guid TrapId { get; private set; }

    /// <summary>
    /// The character ID of the player who placed this trap.
    /// </summary>
    public Guid OwnerId { get; private set; }

    /// <summary>
    /// The X coordinate of the trap's grid position.
    /// </summary>
    public int PositionX { get; private set; }

    /// <summary>
    /// The Y coordinate of the trap's grid position.
    /// </summary>
    public int PositionY { get; private set; }

    /// <summary>
    /// The damage dice expression for this trap (always "3d6").
    /// </summary>
    public string Damage { get; private set; } = DefaultDamage;

    /// <summary>
    /// The Perception DC required to detect this trap.
    /// </summary>
    public int DetectionDc { get; private set; } = DefaultDetectionDc;

    /// <summary>
    /// UTC timestamp when this trap was placed.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// UTC timestamp when this trap expires if not triggered.
    /// </summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>
    /// Whether this trap has been triggered by a creature.
    /// Once triggered, the trap is destroyed.
    /// </summary>
    public bool IsTriggered { get; private set; }

    /// <summary>
    /// The character ID that triggered this trap (null if not yet triggered).
    /// </summary>
    public Guid? TriggeredBy { get; private set; }

    /// <summary>
    /// UTC timestamp when this trap was triggered (null if not yet triggered).
    /// </summary>
    public DateTime? TriggeredAt { get; private set; }

    /// <summary>
    /// How this trap is triggered (default: <see cref="TrapTriggerType.Movement"/>).
    /// </summary>
    public TrapTriggerType TriggerType { get; private set; } = TrapTriggerType.Movement;

    /// <summary>
    /// Creates a new RunicTrap at the specified grid position.
    /// </summary>
    /// <param name="ownerId">The character ID placing the trap.</param>
    /// <param name="positionX">The X grid coordinate.</param>
    /// <param name="positionY">The Y grid coordinate.</param>
    /// <returns>A new trap initialized with default damage, detection DC, and 1-hour expiration.</returns>
    public static RunicTrap Create(Guid ownerId, int positionX, int positionY)
    {
        var now = DateTime.UtcNow;
        return new RunicTrap
        {
            TrapId = Guid.NewGuid(),
            OwnerId = ownerId,
            PositionX = positionX,
            PositionY = positionY,
            Damage = DefaultDamage,
            DetectionDc = DefaultDetectionDc,
            CreatedAt = now,
            ExpiresAt = now.AddHours(DefaultExpirationHours),
            IsTriggered = false,
            TriggeredBy = null,
            TriggeredAt = null,
            TriggerType = TrapTriggerType.Movement
        };
    }

    /// <summary>
    /// Triggers the trap when a creature enters the space.
    /// </summary>
    /// <param name="targetId">The character ID that triggered the trap.</param>
    /// <param name="damageDealt">The damage dealt by the trap (pre-rolled by caller).</param>
    /// <returns>
    /// A <see cref="TrapTriggerResult"/> indicating success with damage dealt,
    /// or failure if the trap was already triggered or expired.
    /// </returns>
    /// <remarks>
    /// The caller is responsible for rolling the 3d6 damage dice and passing the result.
    /// This ensures dice rolling logic remains in the application/combat layer.
    /// </remarks>
    public TrapTriggerResult Trigger(Guid targetId, int damageDealt)
    {
        if (IsTriggered)
            return TrapTriggerResult.Failed("Trap has already been triggered.");

        if (IsExpired())
            return TrapTriggerResult.Failed("Trap has expired.");

        IsTriggered = true;
        TriggeredBy = targetId;
        TriggeredAt = DateTime.UtcNow;

        return TrapTriggerResult.Triggered(TrapId, targetId, damageDealt);
    }

    /// <summary>
    /// Determines if the trap has expired (past expiration time).
    /// </summary>
    /// <returns>True if the current time is at or past <see cref="ExpiresAt"/>.</returns>
    public bool IsExpired() => DateTime.UtcNow >= ExpiresAt;

    /// <summary>
    /// Determines if a specific character can see this trap.
    /// </summary>
    /// <param name="characterId">The character to check visibility for.</param>
    /// <returns>True if the character is the owner of this trap.</returns>
    /// <remarks>
    /// Currently only the owner can see traps. Future party logic will extend
    /// visibility to allies.
    /// </remarks>
    public bool IsVisibleTo(Guid characterId) => characterId == OwnerId;

    /// <summary>
    /// Determines if this trap can be detected via a Perception check.
    /// </summary>
    /// <returns>True if the trap has not been triggered and has not expired.</returns>
    public bool CanBeDetected() => !IsTriggered && !IsExpired();

    /// <summary>
    /// Gets remaining time before expiration.
    /// </summary>
    /// <returns>
    /// TimeSpan representing remaining time, or <see cref="TimeSpan.Zero"/> if expired.
    /// </returns>
    public TimeSpan GetRemainingTime()
    {
        var remaining = ExpiresAt - DateTime.UtcNow;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    /// <summary>
    /// Gets remaining time formatted for display.
    /// </summary>
    /// <returns>
    /// Human-readable time string (e.g., "45m", "30s", "expired").
    /// </returns>
    public string GetRemainingTimeDisplay()
    {
        if (IsExpired())
            return "expired";

        var remaining = GetRemainingTime();
        if (remaining.TotalMinutes < 1)
            return $"{(int)remaining.TotalSeconds}s";
        if (remaining.TotalHours < 1)
            return $"{(int)remaining.TotalMinutes}m";
        return $"{remaining.Hours}h {remaining.Minutes}m";
    }

    /// <summary>
    /// Gets a display string showing the trap's position.
    /// </summary>
    /// <returns>Position as "(X, Y)" format.</returns>
    public string GetPositionDisplay() => $"({PositionX}, {PositionY})";
}
