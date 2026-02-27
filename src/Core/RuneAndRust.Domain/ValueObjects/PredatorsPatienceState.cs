namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Tracks the state of the Predator's Patience stance for the Veiðimaðr (Hunter) specialization.
/// While in this stance, the hunter gains +3 to hit on all attacks, but any movement ends the stance.
/// </summary>
/// <remarks>
/// <para>Predator's Patience is a Tier 2 stance ability that rewards tactical patience.
/// The hunter sacrifices mobility for deadly accuracy.</para>
/// <para>Stance lifecycle:</para>
/// <list type="number">
/// <item><description>Activate stance (1 AP) — <see cref="Activate"/>.</description></item>
/// <item><description>While active: +3 to all attack rolls via <see cref="GetCurrentBonus"/>.</description></item>
/// <item><description>Any movement breaks stance — <see cref="RecordMovement"/>.</description></item>
/// <item><description>End of turn resets movement tracking — <see cref="ResetForNewTurn"/>.</description></item>
/// <item><description>Deactivate manually or automatically — <see cref="Deactivate"/>.</description></item>
/// </list>
/// <para>Introduced in v0.20.7b. Coherent path — zero Corruption risk.</para>
/// </remarks>
public sealed record PredatorsPatienceState
{
    /// <summary>
    /// Default hit bonus granted while stance is active and hunter has not moved.
    /// </summary>
    public const int DefaultHitBonus = 3;

    /// <summary>
    /// Gets the ID of the hunter in this stance.
    /// </summary>
    public Guid HunterId { get; init; }

    /// <summary>
    /// Gets whether the stance is currently active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Gets whether the hunter has moved this turn (which breaks the stance bonus).
    /// </summary>
    public bool HasMovedThisTurn { get; private set; }

    /// <summary>
    /// Gets the hit bonus provided by this stance (+3 by default).
    /// </summary>
    public int HitBonus { get; init; } = DefaultHitBonus;

    /// <summary>
    /// Gets the timestamp when the stance was last activated, or null if never activated.
    /// </summary>
    public DateTime? ActivatedAt { get; private set; }

    /// <summary>
    /// Gets the number of turns the stance has been continuously active.
    /// </summary>
    public int TurnsInStance { get; private set; }

    /// <summary>
    /// Creates a new inactive Predator's Patience state for the specified hunter.
    /// </summary>
    /// <param name="hunterId">The ID of the hunter. Must not be <see cref="Guid.Empty"/>.</param>
    /// <returns>A new <see cref="PredatorsPatienceState"/> in the inactive state.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="hunterId"/> is <see cref="Guid.Empty"/>.</exception>
    public static PredatorsPatienceState Create(Guid hunterId)
    {
        if (hunterId == Guid.Empty)
            throw new ArgumentException("Hunter ID cannot be empty.", nameof(hunterId));

        return new PredatorsPatienceState
        {
            HunterId = hunterId
        };
    }

    /// <summary>
    /// Activates the Predator's Patience stance.
    /// Sets <see cref="IsActive"/> to true and resets movement tracking.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        HasMovedThisTurn = false;
        ActivatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the Predator's Patience stance.
    /// The hunter loses the +3 hit bonus.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Records that the hunter has moved, which immediately deactivates the stance.
    /// Any movement — walking, teleporting, or forced displacement — breaks Predator's Patience.
    /// </summary>
    public void RecordMovement()
    {
        HasMovedThisTurn = true;
        Deactivate();
    }

    /// <summary>
    /// Resets turn-based tracking for a new turn.
    /// Clears the movement flag and increments the turns-in-stance counter if active.
    /// </summary>
    public void ResetForNewTurn()
    {
        HasMovedThisTurn = false;
        if (IsActive)
        {
            TurnsInStance++;
        }
    }

    /// <summary>
    /// Gets the current hit bonus from Predator's Patience.
    /// Returns <see cref="HitBonus"/> (+3) if active and hunter has not moved, 0 otherwise.
    /// </summary>
    /// <returns>The current hit bonus (0 or +3).</returns>
    public int GetCurrentBonus()
    {
        return IsActive && !HasMovedThisTurn ? HitBonus : 0;
    }

    /// <summary>
    /// Gets a narrative description of the current stance state for UI display.
    /// </summary>
    /// <returns>A formatted string describing whether the stance is active and its bonus.</returns>
    public string GetDescription()
    {
        if (!IsActive)
        {
            return "Predator's Patience: Inactive. Activate to gain +3 to hit while stationary.";
        }

        if (HasMovedThisTurn)
        {
            return "Predator's Patience: Broken by movement. No bonus this turn.";
        }

        return $"Predator's Patience: Active (+{HitBonus} to hit). " +
               $"Any movement will end the stance. Active for {TurnsInStance} turn{(TurnsInStance != 1 ? "s" : "")}.";
    }
}
