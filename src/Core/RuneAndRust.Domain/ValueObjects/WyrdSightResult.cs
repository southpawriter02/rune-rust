namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the state and outcome of the Wyrd Sight ability activation.
/// Tracks duration, detection capabilities, and provides turn management.
/// </summary>
/// <remarks>
/// <para>Wyrd Sight is the Seiðkona's Tier 1 detection ability:</para>
/// <list type="bullet">
/// <item>Cost: 2 AP</item>
/// <item>Duration: 3 turns</item>
/// <item>Radius: 10 spaces</item>
/// <item>Detects: invisible creatures, magic auras, Corruption sources</item>
/// <item>Does NOT grant Resonance — pure detection has no Aetheric cost</item>
/// <item>Does NOT trigger Corruption checks — no Aether is channeled</item>
/// </list>
/// <para>Uses mutable <c>private set</c> for <see cref="TurnsRemaining"/> since
/// the turn counter decrements each round during combat. The detection flags
/// and radius are immutable after creation.</para>
/// </remarks>
public sealed record WyrdSightResult
{
    /// <summary>
    /// Default duration of Wyrd Sight in turns.
    /// </summary>
    private const int DefaultDurationTurns = 3;

    /// <summary>
    /// Default detection radius in spaces.
    /// </summary>
    private const int DefaultDetectionRadius = 10;

    /// <summary>
    /// Unique identifier of the caster who activated Wyrd Sight.
    /// Used to associate the effect with the correct player.
    /// </summary>
    public Guid CasterId { get; init; }

    /// <summary>
    /// Number of turns remaining for the Wyrd Sight effect.
    /// Decrements each turn via <see cref="DecrementTurn"/>.
    /// </summary>
    public int TurnsRemaining { get; private set; }

    /// <summary>
    /// Detection radius in spaces from the caster's position.
    /// </summary>
    public int DetectionRadius { get; init; } = DefaultDetectionRadius;

    /// <summary>
    /// Whether Wyrd Sight can detect invisible creatures within radius.
    /// Always true for the base ability.
    /// </summary>
    public bool DetectsInvisible { get; init; } = true;

    /// <summary>
    /// Whether Wyrd Sight can detect magic auras within radius.
    /// Always true for the base ability.
    /// </summary>
    public bool DetectsMagic { get; init; } = true;

    /// <summary>
    /// Whether Wyrd Sight can detect Corruption sources within radius.
    /// Always true for the base ability.
    /// </summary>
    public bool DetectsCorruption { get; init; } = true;

    /// <summary>
    /// UTC timestamp when Wyrd Sight was activated.
    /// </summary>
    public DateTime ActivatedAt { get; init; }

    /// <summary>
    /// Creates a new Wyrd Sight effect for the specified caster.
    /// Initializes with default 3-turn duration and 10-space detection radius.
    /// </summary>
    /// <param name="casterId">The unique identifier of the caster activating Wyrd Sight.</param>
    /// <returns>A new <see cref="WyrdSightResult"/> ready for combat tracking.</returns>
    public static WyrdSightResult Create(Guid casterId)
    {
        return new WyrdSightResult
        {
            CasterId = casterId,
            TurnsRemaining = DefaultDurationTurns,
            DetectionRadius = DefaultDetectionRadius,
            DetectsInvisible = true,
            DetectsMagic = true,
            DetectsCorruption = true,
            ActivatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Decrements the remaining turns by one. Does not go below zero.
    /// Called at the end of each combat turn while Wyrd Sight is active.
    /// </summary>
    public void DecrementTurn()
    {
        if (TurnsRemaining > 0)
            TurnsRemaining--;
    }

    /// <summary>
    /// Gets whether Wyrd Sight is still active (has remaining turns).
    /// </summary>
    /// <returns>True if <see cref="TurnsRemaining"/> is greater than zero.</returns>
    public bool IsActive()
    {
        return TurnsRemaining > 0;
    }

    /// <summary>
    /// Gets a formatted description of the Wyrd Sight activation for combat log display.
    /// </summary>
    /// <returns>A string describing the Wyrd Sight effect and its capabilities.</returns>
    public string GetDescription()
    {
        var capabilities = new List<string>();
        if (DetectsInvisible) capabilities.Add("invisible creatures");
        if (DetectsMagic) capabilities.Add("magic auras");
        if (DetectsCorruption) capabilities.Add("Corruption sources");

        return $"Wyrd Sight activated: detecting {string.Join(", ", capabilities)} " +
               $"within {DetectionRadius} spaces for {TurnsRemaining} turns. " +
               "No Resonance gained — pure detection.";
    }

    /// <summary>
    /// Gets a status string showing remaining duration for UI display.
    /// </summary>
    /// <returns>A string in the format "Wyrd Sight: 2 turns remaining (radius: 10)".</returns>
    public string GetStatusString()
    {
        return IsActive()
            ? $"Wyrd Sight: {TurnsRemaining} turns remaining (radius: {DetectionRadius})"
            : "Wyrd Sight: expired";
    }
}
