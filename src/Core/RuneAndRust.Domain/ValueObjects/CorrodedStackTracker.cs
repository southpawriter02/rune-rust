namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Immutable value object tracking [Corroded] status effect stacks on a target.
/// Used by the Rust-Witch specialization for DoT management and execution threshold evaluation.
/// </summary>
/// <remarks>
/// <para>[Corroded] is a stacking DoT status effect unique to the Rust-Witch specialization.
/// Key properties:</para>
/// <list type="bullet">
/// <item>Maximum 5 stacks per target</item>
/// <item>Base damage: 1d4 per stack per turn</item>
/// <item>With Accelerated Entropy (25006): 2d6 per stack per turn</item>
/// <item>Each stack imposes -1 Armor penalty</item>
/// <item>Stacks persist until cleansed (no natural decay)</item>
/// <item>At 5 stacks, Entropic Cascade (25009) can execute the target</item>
/// </list>
///
/// <para>Follows the immutable pattern established by <see cref="AccumulatedAethericDamage"/>.
/// All mutation operations return new instances rather than modifying existing ones.</para>
/// </remarks>
public sealed record CorrodedStackTracker
{
    /// <summary>
    /// Maximum [Corroded] stacks allowed on a single target.
    /// </summary>
    public const int MaxStacks = 5;

    /// <summary>
    /// Gets the target entity's identifier.
    /// </summary>
    public Guid TargetId { get; init; }

    /// <summary>
    /// Gets the target's display name for logging and UI purposes.
    /// </summary>
    public string TargetName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the current number of [Corroded] stacks on this target.
    /// Always in the range [0, <see cref="MaxStacks"/>].
    /// </summary>
    public int CurrentStacks { get; init; }

    /// <summary>
    /// Gets the total number of stacks applied over the target's lifetime (before capping).
    /// Used for analytics and logging. May exceed <see cref="MaxStacks"/>.
    /// </summary>
    public int TotalStacksApplied { get; init; }

    /// <summary>
    /// Gets the total DoT damage dealt to this target by [Corroded] over all turns.
    /// </summary>
    public int TotalDotDamageDealt { get; init; }

    /// <summary>
    /// Gets the total Armor penalty currently imposed by [Corroded] stacks.
    /// Equal to negative <see cref="CurrentStacks"/> (each stack = -1 Armor).
    /// </summary>
    public int ArmorPenalty => -CurrentStacks;

    /// <summary>
    /// Gets whether the target has reached maximum [Corroded] stacks (5).
    /// At max stacks, the Entropic Cascade capstone can execute the target.
    /// </summary>
    public bool IsAtMaxStacks => CurrentStacks >= MaxStacks;

    /// <summary>
    /// Gets whether the target meets the Entropic Cascade execution threshold.
    /// Requires 5 [Corroded] stacks (Corruption threshold checked separately by the service).
    /// </summary>
    public bool MeetsStackExecutionThreshold => CurrentStacks >= MaxStacks;

    /// <summary>
    /// Creates a new tracker for a target with zero stacks.
    /// </summary>
    /// <param name="targetId">The target entity's unique identifier.</param>
    /// <param name="targetName">The target's display name.</param>
    /// <returns>A new CorrodedStackTracker at zero stacks.</returns>
    public static CorrodedStackTracker Create(Guid targetId, string targetName)
    {
        return new CorrodedStackTracker
        {
            TargetId = targetId,
            TargetName = targetName,
            CurrentStacks = 0,
            TotalStacksApplied = 0,
            TotalDotDamageDealt = 0
        };
    }

    /// <summary>
    /// Returns a new tracker with additional [Corroded] stacks applied.
    /// The result is capped at <see cref="MaxStacks"/>.
    /// </summary>
    /// <param name="stacks">The number of stacks to add (must be positive).</param>
    /// <returns>
    /// A new CorrodedStackTracker with updated stack count.
    /// <see cref="TotalStacksApplied"/> reflects the full amount requested even if capped.
    /// </returns>
    public CorrodedStackTracker WithAdditionalStacks(int stacks)
    {
        if (stacks <= 0) return this;

        var newCurrent = Math.Min(CurrentStacks + stacks, MaxStacks);
        return this with
        {
            CurrentStacks = newCurrent,
            TotalStacksApplied = TotalStacksApplied + stacks
        };
    }

    /// <summary>
    /// Returns a new tracker with doubled [Corroded] stacks (Unmaking Word effect).
    /// The result is capped at <see cref="MaxStacks"/>.
    /// </summary>
    /// <returns>A new CorrodedStackTracker with doubled (but capped) stack count.</returns>
    public CorrodedStackTracker WithDoubledStacks()
    {
        var doubled = Math.Min(CurrentStacks * 2, MaxStacks);
        var added = doubled - CurrentStacks;
        return this with
        {
            CurrentStacks = doubled,
            TotalStacksApplied = TotalStacksApplied + added
        };
    }

    /// <summary>
    /// Returns a new tracker with recorded DoT damage from a turn tick.
    /// </summary>
    /// <param name="damage">The damage dealt this turn from [Corroded] stacks.</param>
    /// <returns>A new CorrodedStackTracker with updated total DoT damage.</returns>
    public CorrodedStackTracker WithDotDamage(int damage)
    {
        return this with
        {
            TotalDotDamageDealt = TotalDotDamageDealt + Math.Max(damage, 0)
        };
    }

    /// <summary>
    /// Returns a new tracker with all stacks cleansed (removed).
    /// </summary>
    /// <returns>A new CorrodedStackTracker at zero stacks (lifetime totals preserved).</returns>
    public CorrodedStackTracker Cleansed()
    {
        return this with { CurrentStacks = 0 };
    }

    /// <summary>
    /// Returns a formatted status string for combat log display.
    /// </summary>
    /// <returns>A string like "[Corroded] x3 on Rust Golem (-3 Armor, 42 total DoT damage)".</returns>
    public override string ToString()
    {
        return $"[Corroded] x{CurrentStacks} on {TargetName} " +
               $"({ArmorPenalty} Armor, {TotalDotDamageDealt} total DoT damage)";
    }
}
