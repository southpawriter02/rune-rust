namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the Intimidating Presence crowd control effect applied to a single target.
/// Tracks save results, penalty application, duration, and immunity state.
/// </summary>
/// <remarks>
/// <para>Introduced in v0.20.5b as part of Berserkr Tier 2 Abilities.</para>
/// <para>Intimidating Presence costs 2 AP and 10 Rage. It forces all enemies within
/// 4 spaces to make a Will save (DC 12 + Rage/20). On each target:</para>
/// <list type="bullet">
/// <item>Failed save: -2 Attack penalty for 3 turns</item>
/// <item>Successful save: 24-hour immunity to this ability</item>
/// <item>Mindless creatures and fear-immune targets are unaffected</item>
/// <item>Using against Coherent-aligned targets triggers +1 Corruption</item>
/// </list>
/// <para>Each target gets its own <see cref="IntimidationEffect"/> instance to
/// independently track save results and remaining duration.</para>
/// </remarks>
public sealed record IntimidationEffect
{
    /// <summary>
    /// Default duration of the fear penalty in turns (on failed save).
    /// </summary>
    private const int DefaultPenaltyDuration = 3;

    /// <summary>
    /// Attack penalty applied to targets who fail their Will save.
    /// </summary>
    private const int DefaultAttackPenalty = -2;

    /// <summary>
    /// Base save DC before Rage scaling is applied.
    /// </summary>
    private const int BaseSaveDc = 12;

    /// <summary>
    /// Rage divisor for save DC scaling (+1 DC per this many Rage points).
    /// </summary>
    private const int RageScalingDivisor = 20;

    /// <summary>
    /// Duration of immunity granted on a successful save.
    /// </summary>
    public static readonly TimeSpan ImmunityDuration = TimeSpan.FromHours(24);

    /// <summary>
    /// Unique identifier for this effect instance.
    /// </summary>
    public Guid EffectId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Character who cast Intimidating Presence.
    /// </summary>
    public Guid CasterId { get; init; }

    /// <summary>
    /// Target affected by this fear effect.
    /// </summary>
    public Guid TargetId { get; init; }

    /// <summary>
    /// Display name of the target for UI and logging.
    /// </summary>
    public string TargetName { get; init; } = string.Empty;

    /// <summary>
    /// The save DC the target had to beat (12 + Rage/20).
    /// </summary>
    public int SaveDc { get; init; }

    /// <summary>
    /// The target's save roll result (1-20 + modifiers).
    /// Null until <see cref="ApplySaveResult"/> is called.
    /// </summary>
    public int? SaveRoll { get; private set; }

    /// <summary>
    /// Gets whether the target successfully saved against the fear effect.
    /// True only if a save roll has been recorded and meets or exceeds the DC.
    /// </summary>
    public bool DidSave => SaveRoll.HasValue && SaveRoll.Value >= SaveDc;

    /// <summary>
    /// Turns remaining for the attack penalty (3 if failed save, 0 if saved).
    /// Decremented each turn via <see cref="Tick"/>.
    /// </summary>
    public int TurnsRemaining { get; private set; } = DefaultPenaltyDuration;

    /// <summary>
    /// Gets the attack penalty applied to targets who failed their save (-2).
    /// </summary>
    public int AttackPenalty => DefaultAttackPenalty;

    /// <summary>
    /// UTC timestamp when immunity expires (if the target saved successfully).
    /// Null if the target has not yet saved or failed their save.
    /// </summary>
    public DateTime? ImmuneUntil { get; private set; }

    /// <summary>
    /// Calculates the save DC for Intimidating Presence at the given Rage level.
    /// Formula: 12 + (currentRage / 20).
    /// </summary>
    /// <param name="currentRage">Current Rage value of the caster.</param>
    /// <returns>
    /// The save DC (12 at 0 Rage, up to 17 at 100 Rage).
    /// </returns>
    /// <example>
    /// At 0 Rage: 12 + 0 = DC 12.
    /// At 40 Rage: 12 + 2 = DC 14.
    /// At 80 Rage: 12 + 4 = DC 16.
    /// At 100 Rage: 12 + 5 = DC 17.
    /// </example>
    public static int CalculateSaveDc(int currentRage)
    {
        return BaseSaveDc + (currentRage / RageScalingDivisor);
    }

    /// <summary>
    /// Creates a new <see cref="IntimidationEffect"/> for a specific target.
    /// </summary>
    /// <param name="casterId">The Berserkr character casting the ability.</param>
    /// <param name="targetId">The target being affected.</param>
    /// <param name="targetName">Display name of the target.</param>
    /// <param name="currentRage">Caster's current Rage for DC calculation.</param>
    /// <returns>A new effect with DC calculated from current Rage.</returns>
    public static IntimidationEffect Create(
        Guid casterId,
        Guid targetId,
        string targetName,
        int currentRage)
    {
        return new IntimidationEffect
        {
            CasterId = casterId,
            TargetId = targetId,
            TargetName = targetName,
            SaveDc = CalculateSaveDc(currentRage)
        };
    }

    /// <summary>
    /// Records the save result for this target.
    /// If the target succeeds, grants 24-hour immunity and sets remaining turns to 0.
    /// If the target fails, the penalty duration remains at 3 turns.
    /// </summary>
    /// <param name="saveRoll">The target's save roll (1-20 + Will modifier).</param>
    public void ApplySaveResult(int saveRoll)
    {
        SaveRoll = saveRoll;

        if (DidSave)
        {
            ImmuneUntil = DateTime.UtcNow.Add(ImmunityDuration);
            TurnsRemaining = 0; // Effect ends immediately on successful save
        }
    }

    /// <summary>
    /// Decrements the remaining penalty duration by one turn.
    /// Only decrements if turns remain (will not go below 0).
    /// </summary>
    public void Tick()
    {
        if (TurnsRemaining > 0)
            TurnsRemaining--;
    }

    /// <summary>
    /// Checks if the fear effect is currently active on this target.
    /// Active means the target failed their save and turns remain.
    /// </summary>
    /// <returns>True if the target failed their save and the penalty has not yet expired.</returns>
    public bool IsActive() => !DidSave && TurnsRemaining > 0;

    /// <summary>
    /// Checks if the target is currently immune to Intimidating Presence.
    /// Immunity is granted for 24 hours after a successful save.
    /// </summary>
    /// <returns>True if the target has active immunity.</returns>
    public bool IsImmune() => ImmuneUntil.HasValue && DateTime.UtcNow < ImmuneUntil;

    /// <summary>
    /// Gets a human-readable description of the effect result for this target.
    /// </summary>
    /// <returns>
    /// A formatted string showing save status, remaining duration, or immunity.
    /// </returns>
    public string GetDescription()
    {
        if (!SaveRoll.HasValue)
            return $"{TargetName}: Save pending (DC {SaveDc})";

        if (DidSave)
            return $"{TargetName}: SAVED (immune 24h)";

        return $"{TargetName}: FAILED ({TurnsRemaining} turns remaining, {AttackPenalty} Attack)";
    }
}
