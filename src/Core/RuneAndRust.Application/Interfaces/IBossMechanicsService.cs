using RuneAndRust.Application.Tracking;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for managing boss encounter mechanics at runtime.
/// </summary>
/// <remarks>
/// <para>
/// IBossMechanicsService is responsible for the complete lifecycle of boss encounters:
/// </para>
/// <list type="bullet">
///   <item><description>Boss spawning with state initialization</description></item>
///   <item><description>Phase transition detection and execution</description></item>
///   <item><description>Transition effect processing (knockback, heal minions, create zones)</description></item>
///   <item><description>Vulnerability window management</description></item>
///   <item><description>Per-turn state updates (summons, cooldowns)</description></item>
///   <item><description>Boss defeat handling with loot event publishing</description></item>
/// </list>
/// <para>
/// The service separates runtime state (<see cref="ActiveBossState"/>) from
/// static definition data (<see cref="BossDefinition"/>), allowing dynamic
/// tracking of phase progression, vulnerability windows, and summon counts.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Spawn a boss at a position
/// var boss = bossMechanicsService.SpawnBoss("skeleton-king", new GridPosition(5, 5));
///
/// // After damage is applied, notify the service
/// bossMechanicsService.OnBossDamaged(boss, damageDealt);
///
/// // Check vulnerability for damage calculation
/// var multiplier = bossMechanicsService.GetVulnerabilityMultiplier(boss);
/// var finalDamage = (int)(baseDamage * multiplier);
///
/// // During boss turn, tick the boss state
/// bossMechanicsService.TickBoss(boss);
/// </code>
/// </example>
public interface IBossMechanicsService
{
    // ═══════════════════════════════════════════════════════════════
    // BOSS SPAWNING
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Spawns a boss at a grid position.
    /// </summary>
    /// <param name="bossId">The boss definition ID (e.g., "skeleton-king").</param>
    /// <param name="position">Grid position to spawn at.</param>
    /// <returns>The spawned boss monster instance.</returns>
    /// <exception cref="ArgumentException">Thrown when boss definition is not found.</exception>
    /// <remarks>
    /// <para>This method creates a Monster entity from the boss's base monster definition,
    /// initializes an <see cref="ActiveBossState"/> for tracking, and publishes a
    /// <see cref="Events.BossSpawnedEvent"/>.</para>
    /// <para>The spawned monster's <see cref="Monster.MonsterDefinitionId"/> will be set
    /// to the boss's <see cref="BossDefinition.BaseMonsterDefinitionId"/>.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var boss = bossMechanicsService.SpawnBoss("volcanic-wyrm", new GridPosition(10, 10));
    /// Console.WriteLine($"Spawned {boss.Name} at position");
    /// </code>
    /// </example>
    Monster SpawnBoss(string bossId, GridPosition position);

    // ═══════════════════════════════════════════════════════════════
    // DAMAGE NOTIFICATION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Called when a boss takes damage to check for phase transitions.
    /// </summary>
    /// <param name="boss">The boss monster that was damaged.</param>
    /// <param name="damage">Amount of damage taken (for logging purposes).</param>
    /// <remarks>
    /// <para>This method should be called from the combat service after damage is applied
    /// to the boss. It calculates the boss's current health percentage and checks if
    /// a phase transition should occur.</para>
    /// <para>If the health drops below a phase threshold, the service:</para>
    /// <list type="number">
    ///   <item><description>Executes transition effects (knockback, heal minions, etc.)</description></item>
    ///   <item><description>Applies new phase stat modifiers</description></item>
    ///   <item><description>Updates the boss's behavior pattern</description></item>
    ///   <item><description>Publishes a <see cref="Events.BossPhaseChangedEvent"/></description></item>
    /// </list>
    /// <para>If the boss is defeated (health ≤ 0), publishes a
    /// <see cref="Events.BossDefeatedEvent"/> with loot table.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // In combat service after dealing damage
    /// boss.TakeDamage(calculatedDamage);
    /// bossMechanicsService.OnBossDamaged(boss, calculatedDamage);
    /// </code>
    /// </example>
    void OnBossDamaged(Monster boss, int damage);

    // ═══════════════════════════════════════════════════════════════
    // PHASE QUERIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the current phase for a boss.
    /// </summary>
    /// <param name="boss">The boss monster.</param>
    /// <returns>The current <see cref="BossPhase"/>, or null if not a tracked boss.</returns>
    /// <remarks>
    /// <para>The current phase is determined by the <see cref="ActiveBossState.CurrentPhaseNumber"/>
    /// stored in the boss's runtime state.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var phase = bossMechanicsService.GetCurrentPhase(boss);
    /// if (phase?.Behavior == BossBehavior.Enraged)
    /// {
    ///     Console.WriteLine("Boss is enraged!");
    /// }
    /// </code>
    /// </example>
    BossPhase? GetCurrentPhase(Monster boss);

    /// <summary>
    /// Gets the active state for a boss.
    /// </summary>
    /// <param name="boss">The boss monster.</param>
    /// <returns>The <see cref="ActiveBossState"/>, or null if not a tracked boss.</returns>
    /// <remarks>
    /// <para>The active state contains runtime information such as current phase,
    /// vulnerability turns remaining, and summoned minion tracking.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var state = bossMechanicsService.GetBossState(boss);
    /// if (state != null)
    /// {
    ///     Console.WriteLine($"Phase {state.CurrentPhaseNumber}, {state.ActiveSummonCount} summons");
    /// }
    /// </code>
    /// </example>
    ActiveBossState? GetBossState(Monster boss);

    // ═══════════════════════════════════════════════════════════════
    // VULNERABILITY MANAGEMENT
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if a boss is currently in a vulnerable state.
    /// </summary>
    /// <param name="boss">The boss monster.</param>
    /// <returns>True if the boss is vulnerable to increased damage; false otherwise.</returns>
    /// <remarks>
    /// <para>A boss is vulnerable when <see cref="ActiveBossState.VulnerableTurns"/> is greater than zero.</para>
    /// <para>While vulnerable, damage dealt to the boss is multiplied by
    /// <see cref="GetVulnerabilityMultiplier"/>.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (bossMechanicsService.IsVulnerable(boss))
    /// {
    ///     Console.WriteLine("Strike now! The boss is vulnerable!");
    /// }
    /// </code>
    /// </example>
    bool IsVulnerable(Monster boss);

    /// <summary>
    /// Gets the damage multiplier for vulnerability.
    /// </summary>
    /// <param name="boss">The boss monster.</param>
    /// <returns>1.5f if vulnerable, 1.0f otherwise.</returns>
    /// <remarks>
    /// <para>Use this multiplier when calculating damage against a boss to apply
    /// the vulnerability bonus.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var multiplier = bossMechanicsService.GetVulnerabilityMultiplier(boss);
    /// var finalDamage = (int)(baseDamage * multiplier);
    /// </code>
    /// </example>
    float GetVulnerabilityMultiplier(Monster boss);

    /// <summary>
    /// Makes a boss vulnerable for a specified duration.
    /// </summary>
    /// <param name="boss">The boss monster.</param>
    /// <param name="turns">Number of turns to be vulnerable.</param>
    /// <remarks>
    /// <para>Sets the boss's <see cref="ActiveBossState.VulnerableTurns"/> to the specified value
    /// and publishes a <see cref="Events.BossVulnerableEvent"/>.</para>
    /// <para>If the boss is already vulnerable, this extends or resets the duration.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Make the boss vulnerable for 2 turns after exhausting a major ability
    /// bossMechanicsService.SetVulnerable(boss, 2);
    /// </code>
    /// </example>
    void SetVulnerable(Monster boss, int turns);

    // ═══════════════════════════════════════════════════════════════
    // TURN PROCESSING
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Called each turn to update boss state.
    /// </summary>
    /// <param name="boss">The boss monster.</param>
    /// <remarks>
    /// <para>This method handles per-turn mechanics:</para>
    /// <list type="bullet">
    ///   <item><description>Decrements vulnerability countdown</description></item>
    ///   <item><description>Increments summon interval timer</description></item>
    ///   <item><description>Publishes <see cref="Events.BossVulnerabilityEndedEvent"/> when vulnerability expires</description></item>
    /// </list>
    /// <para>Should be called at the start of the boss's turn.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // At start of boss turn
    /// bossMechanicsService.TickBoss(boss);
    /// </code>
    /// </example>
    void TickBoss(Monster boss);

    // ═══════════════════════════════════════════════════════════════
    // ABILITY QUERIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the abilities available in the boss's current phase.
    /// </summary>
    /// <param name="boss">The boss monster.</param>
    /// <returns>List of ability IDs available for use; empty list if not a tracked boss.</returns>
    /// <remarks>
    /// <para>The available abilities come from the current phase's
    /// <see cref="BossPhase.AbilityIds"/> property.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var abilities = bossMechanicsService.GetAvailableAbilities(boss);
    /// var randomAbility = abilities[Random.Shared.Next(abilities.Count)];
    /// </code>
    /// </example>
    IReadOnlyList<string> GetAvailableAbilities(Monster boss);

    // ═══════════════════════════════════════════════════════════════
    // BOSS QUERIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if a monster is a tracked boss.
    /// </summary>
    /// <param name="monster">The monster to check.</param>
    /// <returns>True if this monster is an active boss; false otherwise.</returns>
    /// <remarks>
    /// <para>A monster is considered a boss if it was spawned via
    /// <see cref="SpawnBoss"/> and has an associated <see cref="ActiveBossState"/>.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (bossMechanicsService.IsBoss(target))
    /// {
    ///     // Apply boss-specific combat logic
    /// }
    /// </code>
    /// </example>
    bool IsBoss(Monster monster);

    /// <summary>
    /// Gets all currently active bosses in combat.
    /// </summary>
    /// <returns>List of active boss monsters.</returns>
    /// <remarks>
    /// <para>Returns all monsters currently tracked by this service.</para>
    /// <para>Bosses are removed from tracking when defeated.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var activeBosses = bossMechanicsService.GetActiveBosses();
    /// Console.WriteLine($"Fighting {activeBosses.Count} boss(es)");
    /// </code>
    /// </example>
    IReadOnlyList<Monster> GetActiveBosses();

    // ═══════════════════════════════════════════════════════════════
    // MINION TRACKING
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Handles minion death to update boss summon tracking.
    /// </summary>
    /// <param name="boss">The boss that summoned the minion.</param>
    /// <param name="minionId">The deceased minion's ID.</param>
    /// <remarks>
    /// <para>Call this when a summoned minion dies to update the boss's
    /// <see cref="ActiveBossState.SummonedMinionIds"/> list.</para>
    /// <para>This allows the boss to potentially summon new minions if under
    /// the <see cref="SummonConfiguration.MaxActive"/> limit.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // When a minion is killed
    /// if (bossMechanicsService.IsBoss(summonerBoss))
    /// {
    ///     bossMechanicsService.OnMinionDeath(summonerBoss, deadMinion.Id);
    /// }
    /// </code>
    /// </example>
    void OnMinionDeath(Monster boss, Guid minionId);
}
