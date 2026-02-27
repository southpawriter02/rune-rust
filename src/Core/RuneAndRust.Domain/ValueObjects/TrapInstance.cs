using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a single hunting trap placed by the Veiðimaðr (Hunter) specialization.
/// Traps deal 1d8 damage and immobilize targets for 1 turn when triggered.
/// </summary>
/// <remarks>
/// <para>Traps follow a one-way lifecycle: Armed → Triggered/Disarmed → Destroyed.
/// Once triggered or destroyed, a trap cannot be rearmed. This prevents exploit
/// of reusing traps and encourages thoughtful placement strategy.</para>
/// <para>Key invariants:</para>
/// <list type="bullet">
/// <item><description>Only Armed traps can be triggered or disarmed.</description></item>
/// <item><description>Any trap can be destroyed regardless of current status.</description></item>
/// <item><description>Maximum 2 active traps per Veiðimaðr at any time.</description></item>
/// </list>
/// <para>Introduced in v0.20.7b as part of the Trap Mastery ability. Coherent path — zero Corruption risk.</para>
/// </remarks>
public sealed record TrapInstance
{
    /// <summary>
    /// Default DC for enemies to detect a hidden trap.
    /// </summary>
    public const int DefaultDetectionDc = 13;

    /// <summary>
    /// Default number of turns a triggered trap immobilizes its target.
    /// </summary>
    public const int DefaultImmobilizeTurns = 1;

    /// <summary>
    /// Default damage roll string for trap damage.
    /// </summary>
    public const string DefaultDamageRoll = "1d8";

    /// <summary>
    /// Gets the unique identifier for this trap instance.
    /// </summary>
    public Guid TrapId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the ID of the hunter who placed this trap.
    /// </summary>
    public Guid PlacedBy { get; init; }

    /// <summary>
    /// Gets the X coordinate of the trap on the map.
    /// </summary>
    public int X { get; init; }

    /// <summary>
    /// Gets the Y coordinate of the trap on the map.
    /// </summary>
    public int Y { get; init; }

    /// <summary>
    /// Gets the type of trap (Spike, Net, PitFall, Deadfall, Snare).
    /// </summary>
    public TrapType Type { get; init; }

    /// <summary>
    /// Gets the current lifecycle status of the trap.
    /// Starts as <see cref="TrapStatus.Armed"/> and transitions to other states.
    /// </summary>
    public TrapStatus Status { get; private set; } = TrapStatus.Armed;

    /// <summary>
    /// Gets the damage roll string for this trap (default "1d8").
    /// </summary>
    public string DamageRoll { get; init; } = DefaultDamageRoll;

    /// <summary>
    /// Gets the number of turns a triggered trap immobilizes its target (default 1).
    /// </summary>
    public int ImmobilizeTurns { get; init; } = DefaultImmobilizeTurns;

    /// <summary>
    /// Gets the Difficulty Class for enemies to detect this trap (default 13).
    /// </summary>
    public int DetectionDc { get; init; } = DefaultDetectionDc;

    /// <summary>
    /// Gets the timestamp when this trap was triggered, or null if not yet triggered.
    /// </summary>
    public DateTime? TriggeredAt { get; private set; }

    /// <summary>
    /// Gets the ID of the target that triggered this trap, or null if not yet triggered.
    /// </summary>
    public Guid? TriggeringTarget { get; private set; }

    /// <summary>
    /// Gets the number of turns since this trap was placed.
    /// Incremented each turn while the trap remains armed.
    /// </summary>
    public int TurnsPlaced { get; private set; }

    /// <summary>
    /// Creates a new armed hunting trap at the specified location.
    /// </summary>
    /// <param name="placedBy">The ID of the hunter placing the trap. Must not be <see cref="Guid.Empty"/>.</param>
    /// <param name="x">The X coordinate on the map.</param>
    /// <param name="y">The Y coordinate on the map.</param>
    /// <param name="type">The type of trap to place.</param>
    /// <returns>A new <see cref="TrapInstance"/> in the <see cref="TrapStatus.Armed"/> state.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="placedBy"/> is <see cref="Guid.Empty"/>.</exception>
    public static TrapInstance Create(Guid placedBy, int x, int y, TrapType type)
    {
        if (placedBy == Guid.Empty)
            throw new ArgumentException("PlacedBy ID cannot be empty.", nameof(placedBy));

        return new TrapInstance
        {
            PlacedBy = placedBy,
            X = x,
            Y = y,
            Type = type
        };
    }

    /// <summary>
    /// Triggers this trap when an enemy enters its space.
    /// Sets the status to <see cref="TrapStatus.Triggered"/> and records the triggering target.
    /// </summary>
    /// <param name="targetId">The ID of the target that triggered the trap.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the trap is not in the <see cref="TrapStatus.Armed"/> state.
    /// </exception>
    public void Trigger(Guid targetId)
    {
        if (Status != TrapStatus.Armed)
            throw new InvalidOperationException(
                $"Cannot trigger trap {TrapId}: current status is {Status}. Only Armed traps can be triggered.");

        Status = TrapStatus.Triggered;
        TriggeredAt = DateTime.UtcNow;
        TriggeringTarget = targetId;
    }

    /// <summary>
    /// Safely disarms this trap without triggering it.
    /// Sets the status to <see cref="TrapStatus.Disarmed"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the trap is not in the <see cref="TrapStatus.Armed"/> state.
    /// </exception>
    public void Disarm()
    {
        if (Status != TrapStatus.Armed)
            throw new InvalidOperationException(
                $"Cannot disarm trap {TrapId}: current status is {Status}. Only Armed traps can be disarmed.");

        Status = TrapStatus.Disarmed;
    }

    /// <summary>
    /// Destroys this trap permanently. Can be called from any state.
    /// Sets the status to <see cref="TrapStatus.Destroyed"/>.
    /// </summary>
    public void Destroy()
    {
        Status = TrapStatus.Destroyed;
    }

    /// <summary>
    /// Increments the turn counter for this trap.
    /// Called during turn refresh processing.
    /// </summary>
    public void IncrementTurn()
    {
        TurnsPlaced++;
    }

    /// <summary>
    /// Gets a narrative description of this trap for display in the UI.
    /// </summary>
    /// <returns>A formatted string describing the trap's type, location, and status.</returns>
    public string GetDescription()
    {
        var statusText = Status switch
        {
            TrapStatus.Armed => "armed and hidden",
            TrapStatus.Triggered => "triggered",
            TrapStatus.Disarmed => "disarmed",
            TrapStatus.Destroyed => "destroyed",
            _ => "unknown"
        };

        return $"{Type} trap at ({X}, {Y}) — {statusText} (placed {TurnsPlaced} turns ago)";
    }

    /// <summary>
    /// Gets a summary of the trap's damage and effects.
    /// </summary>
    /// <returns>A formatted string showing damage and immobilize duration.</returns>
    public string GetDamageText()
    {
        return $"{DamageRoll} piercing + {ImmobilizeTurns}-turn immobilize";
    }
}
