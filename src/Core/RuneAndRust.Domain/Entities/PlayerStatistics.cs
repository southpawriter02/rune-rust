// ═══════════════════════════════════════════════════════════════════════════════
// PlayerStatistics.cs
// Entity tracking comprehensive game statistics for a player.
// Version: 0.12.0a
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Tracks comprehensive game statistics for a player.
/// </summary>
/// <remarks>
/// <para>
/// Statistics are organized into four main categories:
/// </para>
/// <list type="bullet">
///   <item><description><b>Combat</b>: Kills, damage dealt/received, critical hits, deaths</description></item>
///   <item><description><b>Exploration</b>: Rooms discovered, secrets found, traps, doors</description></item>
///   <item><description><b>Progression</b>: XP earned, levels gained, items, gold, quests</description></item>
///   <item><description><b>Time</b>: Total playtime, session count, first/last played dates</description></item>
/// </list>
/// <para>
/// Statistics are updated via the StatisticsService
/// which is called by game services during gameplay events.
/// </para>
/// <para>
/// Each player has exactly one PlayerStatistics instance, established via a
/// one-to-one relationship with the <see cref="Player"/> entity.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create statistics for a new player
/// var stats = PlayerStatistics.Create(player.Id);
///
/// // Record a monster kill
/// stats.RecordMonsterKill("goblin");
///
/// // Increment a statistic
/// stats.IncrementStat("damageDealt", 25);
///
/// // Get a statistic value
/// var monstersKilled = stats.GetStatistic("monstersKilled");
///
/// // Record session playtime
/// stats.RecordPlaytime(TimeSpan.FromMinutes(45));
/// </code>
/// </example>
public sealed class PlayerStatistics : IEntity
{
    // ═══════════════════════════════════════════════════════════════════════════
    // IDENTITY PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique identifier for this statistics record.
    /// </summary>
    /// <remarks>
    /// This is the primary key for the PlayerStatistics entity,
    /// satisfying the <see cref="IEntity"/> interface requirement.
    /// </remarks>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the ID of the player these statistics belong to.
    /// </summary>
    /// <remarks>
    /// Each player has exactly one PlayerStatistics entity. This establishes
    /// a one-to-one relationship between Player and PlayerStatistics.
    /// </remarks>
    public Guid PlayerId { get; private set; }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMBAT STATISTICS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the total number of monsters killed.
    /// </summary>
    /// <remarks>
    /// Incremented by <see cref="RecordMonsterKill(string)"/> when any monster is defeated.
    /// Includes both regular monsters and bosses.
    /// </remarks>
    public int MonstersKilled { get; private set; }

    /// <summary>
    /// Internal storage for monster kills by type.
    /// </summary>
    /// <remarks>
    /// Uses case-insensitive comparison to ensure consistent lookups
    /// regardless of how monster types are provided.
    /// </remarks>
    private readonly Dictionary<string, int> _monstersByType = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a read-only view of monsters killed by type.
    /// </summary>
    /// <remarks>
    /// Keys are normalized to lowercase. Values represent the count of each
    /// monster type killed. Use this to display detailed kill breakdowns.
    /// </remarks>
    /// <example>
    /// <code>
    /// foreach (var (type, count) in stats.MonstersByType)
    /// {
    ///     Console.WriteLine($"{type}: {count} kills");
    /// }
    /// </code>
    /// </example>
    public IReadOnlyDictionary<string, int> MonstersByType => _monstersByType;

    /// <summary>
    /// Gets the total damage dealt to enemies.
    /// </summary>
    /// <remarks>
    /// Accumulated from all successful attacks and abilities.
    /// Used for calculating <c>AverageDamagePerHit</c> metrics.
    /// </remarks>
    public long TotalDamageDealt { get; private set; }

    /// <summary>
    /// Gets the total damage received from enemies.
    /// </summary>
    /// <remarks>
    /// Accumulated from all hits taken by the player.
    /// Used for tracking survivability and damage mitigation.
    /// </remarks>
    public long TotalDamageReceived { get; private set; }

    /// <summary>
    /// Gets the number of critical hits landed.
    /// </summary>
    /// <remarks>
    /// Incremented when an attack or ability results in a critical hit.
    /// Used for calculating <c>CriticalHitRate</c> metrics.
    /// </remarks>
    public int CriticalHits { get; private set; }

    /// <summary>
    /// Gets the number of attacks that missed.
    /// </summary>
    /// <remarks>
    /// Incremented when an attack fails to hit the target.
    /// Used for calculating <c>MissRate</c> and <c>HitRate</c> metrics.
    /// </remarks>
    public int AttacksMissed { get; private set; }

    /// <summary>
    /// Gets the number of abilities used.
    /// </summary>
    /// <remarks>
    /// Incremented when the player activates any ability or skill.
    /// Includes both combat and non-combat abilities.
    /// </remarks>
    public int AbilitiesUsed { get; private set; }

    /// <summary>
    /// Gets the number of times the player has died.
    /// </summary>
    /// <remarks>
    /// Incremented when the player's health reaches zero.
    /// Used for calculating kill/death ratio in combat rating.
    /// </remarks>
    public int DeathCount { get; private set; }

    /// <summary>
    /// Gets the number of bosses killed.
    /// </summary>
    /// <remarks>
    /// Incremented separately from regular monsters when a boss is defeated.
    /// Boss kills contribute to combat rating calculation.
    /// </remarks>
    public int BossesKilled { get; private set; }

    /// <summary>
    /// Gets the total number of attacks made.
    /// </summary>
    /// <remarks>
    /// Includes both hits and misses. Used as the denominator for
    /// calculating critical hit rate and miss rate metrics.
    /// </remarks>
    public int TotalAttacks { get; private set; }

    // ═══════════════════════════════════════════════════════════════════════════
    // EXPLORATION STATISTICS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the number of rooms discovered.
    /// </summary>
    /// <remarks>
    /// Incremented when the player enters a room for the first time.
    /// Tracks exploration progress through the dungeon.
    /// </remarks>
    public int RoomsDiscovered { get; private set; }

    /// <summary>
    /// Gets the number of secrets found.
    /// </summary>
    /// <remarks>
    /// Incremented when the player discovers hidden areas, passages,
    /// or secret items within the game world.
    /// </remarks>
    public int SecretsFound { get; private set; }

    /// <summary>
    /// Gets the number of traps triggered.
    /// </summary>
    /// <remarks>
    /// Incremented when the player triggers a trap and takes damage or
    /// suffers negative effects. Used with <see cref="TrapsAvoided"/>
    /// to calculate trap avoidance rate.
    /// </remarks>
    public int TrapsTriggered { get; private set; }

    /// <summary>
    /// Gets the number of traps successfully avoided.
    /// </summary>
    /// <remarks>
    /// Incremented when the player successfully detects and avoids a trap.
    /// Used to calculate trap avoidance rate metrics.
    /// </remarks>
    public int TrapsAvoided { get; private set; }

    /// <summary>
    /// Gets the number of doors opened.
    /// </summary>
    /// <remarks>
    /// Incremented when the player opens or unlocks a door.
    /// Includes both locked and unlocked doors.
    /// </remarks>
    public int DoorsOpened { get; private set; }

    /// <summary>
    /// Gets the number of chests opened.
    /// </summary>
    /// <remarks>
    /// Incremented when the player opens a treasure chest or container.
    /// Tracks loot collection activity.
    /// </remarks>
    public int ChestsOpened { get; private set; }

    // ═══════════════════════════════════════════════════════════════════════════
    // PROGRESSION STATISTICS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the total experience points earned.
    /// </summary>
    /// <remarks>
    /// Accumulated from all XP sources: monster kills, quests, exploration.
    /// This is the lifetime total, not current XP toward next level.
    /// </remarks>
    public long TotalXPEarned { get; private set; }

    /// <summary>
    /// Gets the number of levels gained.
    /// </summary>
    /// <remarks>
    /// Incremented each time the player levels up.
    /// For a level 10 character who started at level 1, this would be 9.
    /// </remarks>
    public int LevelsGained { get; private set; }

    /// <summary>
    /// Gets the number of items found (picked up).
    /// </summary>
    /// <remarks>
    /// Incremented when the player picks up items from the world.
    /// Does not include items received from quests or purchased from vendors.
    /// </remarks>
    public int ItemsFound { get; private set; }

    /// <summary>
    /// Gets the number of items crafted.
    /// </summary>
    /// <remarks>
    /// Incremented when the player successfully crafts an item.
    /// Tracks crafting activity and productivity.
    /// </remarks>
    public int ItemsCrafted { get; private set; }

    /// <summary>
    /// Gets the total gold earned.
    /// </summary>
    /// <remarks>
    /// Accumulated from all gold sources: loot, quest rewards, item sales.
    /// This is lifetime earnings, not current gold balance.
    /// </remarks>
    public long GoldEarned { get; private set; }

    /// <summary>
    /// Gets the total gold spent.
    /// </summary>
    /// <remarks>
    /// Accumulated from all gold expenditures: purchases, repairs, services.
    /// Used with <see cref="GoldEarned"/> to calculate net gold balance.
    /// </remarks>
    public long GoldSpent { get; private set; }

    /// <summary>
    /// Gets the number of quests completed.
    /// </summary>
    /// <remarks>
    /// Incremented when the player successfully completes a quest.
    /// Does not include failed or abandoned quests.
    /// </remarks>
    public int QuestsCompleted { get; private set; }

    /// <summary>
    /// Gets the number of puzzles solved.
    /// </summary>
    /// <remarks>
    /// Incremented when the player successfully solves a puzzle.
    /// Includes all puzzle types: riddles, mechanisms, etc.
    /// </remarks>
    public int PuzzlesSolved { get; private set; }

    /// <summary>
    /// Gets the number of resources gathered.
    /// </summary>
    /// <remarks>
    /// Incremented when the player harvests resources from resource nodes.
    /// Tracks gathering activity for crafting materials.
    /// </remarks>
    public int ResourcesGathered { get; private set; }

    // ═══════════════════════════════════════════════════════════════════════════
    // TIME STATISTICS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the total playtime across all sessions.
    /// </summary>
    /// <remarks>
    /// Accumulated from each play session via <see cref="RecordPlaytime(TimeSpan)"/>.
    /// Used to calculate average session length and display total time played.
    /// </remarks>
    public TimeSpan TotalPlaytime { get; private set; }

    /// <summary>
    /// Gets the date and time the player first played (UTC).
    /// </summary>
    /// <remarks>
    /// Set when the PlayerStatistics is created via <see cref="Create(Guid)"/>.
    /// This value never changes after initial creation.
    /// </remarks>
    public DateTime FirstPlayed { get; private set; }

    /// <summary>
    /// Gets the date and time the player last played (UTC).
    /// </summary>
    /// <remarks>
    /// Updated when playtime is recorded or a new session starts.
    /// Used to show when the player was last active.
    /// </remarks>
    public DateTime LastPlayed { get; private set; }

    /// <summary>
    /// Gets the total number of play sessions.
    /// </summary>
    /// <remarks>
    /// Incremented via <see cref="StartNewSession"/>. The first session
    /// is counted when the statistics are created.
    /// </remarks>
    public int SessionCount { get; private set; }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Private constructor for factory pattern and EF Core.
    /// </summary>
    /// <remarks>
    /// Use <see cref="Create(Guid)"/> to create new instances.
    /// </remarks>
    private PlayerStatistics() { }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new PlayerStatistics instance for a player.
    /// </summary>
    /// <param name="playerId">The ID of the player these statistics belong to.</param>
    /// <returns>A new PlayerStatistics instance with initialized values.</returns>
    /// <remarks>
    /// <para>
    /// Initializes all counters to zero and sets <see cref="FirstPlayed"/>
    /// and <see cref="LastPlayed"/> to the current UTC time. The initial
    /// <see cref="SessionCount"/> is set to 1.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var player = new Player("Hero");
    /// var stats = PlayerStatistics.Create(player.Id);
    /// player.InitializeStatistics(stats);
    /// </code>
    /// </example>
    public static PlayerStatistics Create(Guid playerId)
    {
        var now = DateTime.UtcNow;
        return new PlayerStatistics
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            FirstPlayed = now,
            LastPlayed = now,
            SessionCount = 1
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // INCREMENT METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Increments a named statistic by the specified amount.
    /// </summary>
    /// <param name="statName">The name of the statistic to increment (case-insensitive).</param>
    /// <param name="amount">The amount to increment by (must be non-negative, default 1).</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="amount"/> is negative.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="statName"/> is not a recognized statistic name.
    /// </exception>
    /// <remarks>
    /// <para>Supported statistic names (case-insensitive):</para>
    /// <para><b>Combat:</b> monstersKilled, damageDealt, damageReceived, criticalHits,
    /// attacksMissed, abilitiesUsed, deathCount, bossesKilled, totalAttacks</para>
    /// <para><b>Exploration:</b> roomsDiscovered, secretsFound, trapsTriggered,
    /// trapsAvoided, doorsOpened, chestsOpened</para>
    /// <para><b>Progression:</b> xpEarned, levelsGained, itemsFound, itemsCrafted,
    /// goldEarned, goldSpent, questsCompleted, puzzlesSolved, resourcesGathered</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// stats.IncrementStat("damageDealt", 25);
    /// stats.IncrementStat("monstersKilled"); // Defaults to 1
    /// </code>
    /// </example>
    public void IncrementStat(string statName, int amount = 1)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount, nameof(amount));

        switch (statName.ToLowerInvariant())
        {
            // Combat statistics
            case "monsterskilled":
                MonstersKilled += amount;
                break;
            case "damagedealt":
                TotalDamageDealt += amount;
                break;
            case "damagereceived":
                TotalDamageReceived += amount;
                break;
            case "criticalhits":
                CriticalHits += amount;
                break;
            case "attacksmissed":
                AttacksMissed += amount;
                break;
            case "abilitiesused":
                AbilitiesUsed += amount;
                break;
            case "deathcount":
                DeathCount += amount;
                break;
            case "bosseskilled":
                BossesKilled += amount;
                break;
            case "totalattacks":
                TotalAttacks += amount;
                break;

            // Exploration statistics
            case "roomsdiscovered":
                RoomsDiscovered += amount;
                break;
            case "secretsfound":
                SecretsFound += amount;
                break;
            case "trapstriggered":
                TrapsTriggered += amount;
                break;
            case "trapsavoided":
                TrapsAvoided += amount;
                break;
            case "doorsopened":
                DoorsOpened += amount;
                break;
            case "chestsopened":
                ChestsOpened += amount;
                break;

            // Progression statistics
            case "xpearned":
                TotalXPEarned += amount;
                break;
            case "levelsgained":
                LevelsGained += amount;
                break;
            case "itemsfound":
                ItemsFound += amount;
                break;
            case "itemscrafted":
                ItemsCrafted += amount;
                break;
            case "goldearned":
                GoldEarned += amount;
                break;
            case "goldspent":
                GoldSpent += amount;
                break;
            case "questscompleted":
                QuestsCompleted += amount;
                break;
            case "puzzlessolved":
                PuzzlesSolved += amount;
                break;
            case "resourcesgathered":
                ResourcesGathered += amount;
                break;

            default:
                throw new ArgumentException($"Unknown statistic: {statName}", nameof(statName));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RECORDING METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Records a monster kill, tracking both total and by type.
    /// </summary>
    /// <param name="monsterType">The type/name of the monster killed.</param>
    /// <remarks>
    /// <para>
    /// Increments <see cref="MonstersKilled"/> by 1 and updates
    /// <see cref="MonstersByType"/> with the normalized type name.
    /// </para>
    /// <para>
    /// Monster types are normalized to lowercase for consistent tracking.
    /// "Goblin" and "goblin" are treated as the same type.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// stats.RecordMonsterKill("Goblin");
    /// stats.RecordMonsterKill("skeleton");
    /// // stats.MonstersByType["goblin"] == 1
    /// // stats.MonstersByType["skeleton"] == 1
    /// </code>
    /// </example>
    public void RecordMonsterKill(string monsterType)
    {
        MonstersKilled++;
        var normalizedType = monsterType.ToLowerInvariant();
        _monstersByType.TryGetValue(normalizedType, out var count);
        _monstersByType[normalizedType] = count + 1;
    }

    /// <summary>
    /// Records session playtime.
    /// </summary>
    /// <param name="sessionTime">The duration of the session to add.</param>
    /// <remarks>
    /// <para>
    /// Adds the session time to <see cref="TotalPlaytime"/> and updates
    /// <see cref="LastPlayed"/> to the current UTC time.
    /// </para>
    /// <para>
    /// This method should be called at the end of a play session or
    /// periodically during gameplay to track active time.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // At the end of a session
    /// var sessionDuration = DateTime.UtcNow - sessionStartTime;
    /// stats.RecordPlaytime(sessionDuration);
    /// </code>
    /// </example>
    public void RecordPlaytime(TimeSpan sessionTime)
    {
        TotalPlaytime += sessionTime;
        LastPlayed = DateTime.UtcNow;
    }

    /// <summary>
    /// Starts a new play session, incrementing the session count.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Increments <see cref="SessionCount"/> by 1 and updates
    /// <see cref="LastPlayed"/> to the current UTC time.
    /// </para>
    /// <para>
    /// Call this method when the player starts a new game session
    /// (e.g., when they log in or resume playing).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // When player starts a new session
    /// stats.StartNewSession();
    /// </code>
    /// </example>
    public void StartNewSession()
    {
        SessionCount++;
        LastPlayed = DateTime.UtcNow;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // QUERY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a statistic value by name.
    /// </summary>
    /// <param name="statName">The name of the statistic to retrieve (case-insensitive).</param>
    /// <returns>The statistic value, or 0 if the statistic name is not recognized.</returns>
    /// <remarks>
    /// <para>
    /// Returns the current value of the named statistic. For time-based
    /// statistics like playtime, returns the value in seconds.
    /// </para>
    /// <para>
    /// Unrecognized statistic names return 0 rather than throwing an exception,
    /// allowing safe querying of statistics that may not exist.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var kills = stats.GetStatistic("monstersKilled");
    /// var playtime = stats.GetStatistic("playtimeSeconds");
    /// var unknown = stats.GetStatistic("notAReal Stat"); // Returns 0
    /// </code>
    /// </example>
    public long GetStatistic(string statName)
    {
        return statName.ToLowerInvariant() switch
        {
            // Combat statistics
            "monsterskilled" => MonstersKilled,
            "damagedealt" => TotalDamageDealt,
            "damagereceived" => TotalDamageReceived,
            "criticalhits" => CriticalHits,
            "attacksmissed" => AttacksMissed,
            "abilitiesused" => AbilitiesUsed,
            "deathcount" => DeathCount,
            "bosseskilled" => BossesKilled,
            "totalattacks" => TotalAttacks,

            // Exploration statistics
            "roomsdiscovered" => RoomsDiscovered,
            "secretsfound" => SecretsFound,
            "trapstriggered" => TrapsTriggered,
            "trapsavoided" => TrapsAvoided,
            "doorsopened" => DoorsOpened,
            "chestsopened" => ChestsOpened,

            // Progression statistics
            "xpearned" => TotalXPEarned,
            "levelsgained" => LevelsGained,
            "itemsfound" => ItemsFound,
            "itemscrafted" => ItemsCrafted,
            "goldearned" => GoldEarned,
            "goldspent" => GoldSpent,
            "questscompleted" => QuestsCompleted,
            "puzzlessolved" => PuzzlesSolved,
            "resourcesgathered" => ResourcesGathered,

            // Time statistics
            "playtimeseconds" => (long)TotalPlaytime.TotalSeconds,
            "sessioncount" => SessionCount,

            // Unknown statistic - return 0 for safe querying
            _ => 0
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a summary string for display.
    /// </summary>
    /// <returns>A string describing the statistics summary.</returns>
    /// <example>
    /// <code>
    /// Console.WriteLine(stats.ToString());
    /// // Output: PlayerStatistics (127 kills, 45 rooms, Level +9)
    /// </code>
    /// </example>
    public override string ToString() =>
        $"PlayerStatistics ({MonstersKilled} kills, {RoomsDiscovered} rooms, Level +{LevelsGained})";
}
