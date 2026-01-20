namespace RuneAndRust.Application.Tracking;

/// <summary>
/// Tracks the runtime state of an active boss in combat.
/// </summary>
/// <remarks>
/// <para>
/// ActiveBossState separates runtime state from static definition data,
/// allowing the boss encounter system to track:
/// </para>
/// <list type="bullet">
///   <item><description><see cref="CurrentPhaseNumber"/> - Current phase progression</description></item>
///   <item><description><see cref="VulnerableTurns"/> - Vulnerability window countdown</description></item>
///   <item><description><see cref="TurnsSinceLastSummon"/> - Summon timing tracking</description></item>
///   <item><description><see cref="SummonedMinionIds"/> - Active summoned minions</description></item>
///   <item><description><see cref="EnrageStacks"/> - Future enrage mechanic support</description></item>
/// </list>
/// <para>
/// This class is mutable and updated throughout the encounter by the
/// <see cref="Interfaces.IBossMechanicsService"/> implementation.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create state for a new boss encounter
/// var state = new ActiveBossState("skeleton-king", startingPhase: 1);
///
/// // Track vulnerability window
/// state.VulnerableTurns = 2;
/// Console.WriteLine($"Vulnerable: {state.IsVulnerable}"); // true
///
/// // Track summoned minions
/// state.AddSummonedMinion(minionId);
/// Console.WriteLine($"Active summons: {state.ActiveSummonCount}");
///
/// // Check if more summons are allowed
/// if (state.CanSummon(maxActive: 4))
/// {
///     // Spawn more minions
/// }
/// </code>
/// </example>
public class ActiveBossState
{
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the boss definition ID this state tracks.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This ID references the <see cref="Domain.Definitions.BossDefinition"/>
    /// via <see cref="Interfaces.IBossProvider.GetBoss(string)"/>.
    /// </para>
    /// </remarks>
    public string BossId { get; }

    /// <summary>
    /// Gets or sets the current phase number (1-based).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Updated when health drops below phase thresholds.
    /// Only increases - phases do not reverse during an encounter.
    /// </para>
    /// <para>
    /// Phase transitions are detected by comparing this value with the
    /// result of <see cref="Domain.Definitions.BossDefinition.GetPhaseForHealth(int)"/>.
    /// </para>
    /// </remarks>
    public int CurrentPhaseNumber { get; set; }

    /// <summary>
    /// Gets or sets the remaining turns of vulnerability.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When greater than zero, the boss takes increased damage (1.5x default).
    /// Decremented each turn via
    /// <see cref="Interfaces.IBossMechanicsService.TickBoss(Domain.Entities.Monster)"/>.
    /// </para>
    /// <para>
    /// Set via <see cref="Interfaces.IBossMechanicsService.SetVulnerable(Domain.Entities.Monster, int)"/>.
    /// </para>
    /// </remarks>
    public int VulnerableTurns { get; set; }

    /// <summary>
    /// Gets or sets the turns since the boss last summoned minions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used to track summon interval timing based on the current phase's
    /// <see cref="Domain.ValueObjects.SummonConfiguration.IntervalTurns"/>.
    /// </para>
    /// <para>
    /// Reset to 0 after each successful summon attempt.
    /// Incremented each turn during <see cref="Interfaces.IBossMechanicsService.TickBoss(Domain.Entities.Monster)"/>.
    /// </para>
    /// </remarks>
    public int TurnsSinceLastSummon { get; set; }

    /// <summary>
    /// Gets the IDs of minions summoned by this boss.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used to track active summons against <see cref="Domain.ValueObjects.SummonConfiguration.MaxActive"/> limit.
    /// </para>
    /// <para>
    /// Minion IDs are added via <see cref="AddSummonedMinion(Guid)"/> and
    /// removed via <see cref="RemoveSummonedMinion(Guid)"/> when minions die.
    /// </para>
    /// </remarks>
    public IReadOnlyList<Guid> SummonedMinionIds => _summonedMinionIds;

    /// <summary>
    /// Gets or sets the optional enrage stack count for future mechanics.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Reserved for future enrage mechanics that accumulate stacks
    /// over the course of an encounter (e.g., time-based enrage).
    /// </para>
    /// <para>
    /// Not currently used by the base implementation.
    /// </para>
    /// </remarks>
    public int EnrageStacks { get; set; }

    // ═══════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether the boss is currently vulnerable.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returns true when <see cref="VulnerableTurns"/> is greater than zero.
    /// </para>
    /// <para>
    /// When vulnerable, the boss takes increased damage (typically 1.5x).
    /// </para>
    /// </remarks>
    public bool IsVulnerable => VulnerableTurns > 0;

    /// <summary>
    /// Gets the count of active summoned minions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used to check against <see cref="Domain.ValueObjects.SummonConfiguration.MaxActive"/>
    /// to determine if more minions can be summoned.
    /// </para>
    /// </remarks>
    public int ActiveSummonCount => _summonedMinionIds.Count;

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Backing list for <see cref="SummonedMinionIds"/>.
    /// </summary>
    private readonly List<Guid> _summonedMinionIds = [];

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new active boss state for tracking an encounter.
    /// </summary>
    /// <param name="bossId">The boss definition ID.</param>
    /// <param name="startingPhase">The starting phase number (typically 1).</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="bossId"/> is null or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="startingPhase"/> is less than 1.</exception>
    /// <remarks>
    /// <para>
    /// Initializes all counters to zero. The starting phase should match
    /// the first phase defined in the boss definition (typically phase 1).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var state = new ActiveBossState("skeleton-king", startingPhase: 1);
    /// // state.CurrentPhaseNumber == 1
    /// // state.VulnerableTurns == 0
    /// // state.TurnsSinceLastSummon == 0
    /// // state.EnrageStacks == 0
    /// </code>
    /// </example>
    public ActiveBossState(string bossId, int startingPhase)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bossId);
        ArgumentOutOfRangeException.ThrowIfLessThan(startingPhase, 1);

        BossId = bossId;
        CurrentPhaseNumber = startingPhase;
        VulnerableTurns = 0;
        TurnsSinceLastSummon = 0;
        EnrageStacks = 0;
    }

    // ═══════════════════════════════════════════════════════════════
    // PUBLIC METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Removes a minion from the tracked summons (on minion death).
    /// </summary>
    /// <param name="minionId">The minion's entity ID.</param>
    /// <remarks>
    /// <para>
    /// Should be called when a summoned minion dies to update the active count.
    /// This allows the boss to potentially summon new minions if under the limit.
    /// </para>
    /// <para>
    /// Safe to call with an ID that is not tracked (no-op).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // When a minion dies
    /// bossState.RemoveSummonedMinion(deadMinionId);
    /// Console.WriteLine($"Active summons: {bossState.ActiveSummonCount}");
    /// </code>
    /// </example>
    public void RemoveSummonedMinion(Guid minionId)
    {
        _summonedMinionIds.Remove(minionId);
    }

    /// <summary>
    /// Adds a minion to the tracked summons.
    /// </summary>
    /// <param name="minionId">The minion's entity ID.</param>
    /// <remarks>
    /// <para>
    /// Should be called when the boss successfully summons a minion.
    /// Prevents duplicate entries (safe to call multiple times with same ID).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // After spawning a minion
    /// var minion = spawnService.Spawn(monsterDef, position);
    /// bossState.AddSummonedMinion(minion.Id);
    /// </code>
    /// </example>
    public void AddSummonedMinion(Guid minionId)
    {
        if (!_summonedMinionIds.Contains(minionId))
        {
            _summonedMinionIds.Add(minionId);
        }
    }

    /// <summary>
    /// Checks if more minions can be summoned based on configuration.
    /// </summary>
    /// <param name="maxActive">Maximum active summons allowed from phase config.</param>
    /// <returns>True if under the summon limit; false otherwise.</returns>
    /// <remarks>
    /// <para>
    /// The <paramref name="maxActive"/> value should come from the current phase's
    /// <see cref="Domain.ValueObjects.SummonConfiguration.MaxActive"/> property.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var config = currentPhase.SummonConfig;
    /// if (bossState.CanSummon(config.MaxActive))
    /// {
    ///     // Spawn new minions
    /// }
    /// </code>
    /// </example>
    public bool CanSummon(int maxActive)
    {
        return _summonedMinionIds.Count < maxActive;
    }

    /// <summary>
    /// Returns a string representation of this boss state.
    /// </summary>
    /// <returns>A string showing the boss ID and current state.</returns>
    public override string ToString()
    {
        var vulnText = IsVulnerable ? $", vulnerable for {VulnerableTurns} turns" : "";
        return $"Boss[{BossId}]: Phase {CurrentPhaseNumber}, {ActiveSummonCount} summons{vulnText}";
    }
}
