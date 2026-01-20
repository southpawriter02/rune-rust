using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Defines a boss encounter with multiple phases and unique mechanics.
/// </summary>
/// <remarks>
/// <para>
/// BossDefinition extends the monster system with phase-based encounters:
/// <list type="bullet">
///   <item><description>Multiple combat phases triggered by health thresholds</description></item>
///   <item><description>Phase-specific abilities, behaviors, and stat modifiers</description></item>
///   <item><description>Summon configurations for spawning minions</description></item>
///   <item><description>Dedicated loot tables with guaranteed and rare drops</description></item>
/// </list>
/// </para>
/// <para>
/// Bosses reference a <see cref="BaseMonsterDefinitionId"/> for base stats,
/// then apply phase-specific modifications during combat.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var boss = BossDefinition.Create(
///         bossId: "skeleton-king",
///         name: "The Skeleton King",
///         description: "An ancient ruler risen from death",
///         baseMonsterDefinitionId: "skeleton-elite")
///     .WithTitleText("Lord of the Undead Crypt")
///     .WithPhase(phase1)
///     .WithPhase(phase2)
///     .WithLoot(BossLootEntry.Guaranteed("gold", 500))
///     .WithLoot(BossLootEntry.Create("crown-of-bones", 0.25));
/// </code>
/// </example>
public class BossDefinition
{
    // ═══════════════════════════════════════════════════════════════
    // IDENTITY
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique identifier for this boss.
    /// </summary>
    /// <remarks>
    /// Used for referencing the boss in encounters, quests, and triggers.
    /// Format: lowercase with hyphens (e.g., "skeleton-king", "volcanic-wyrm").
    /// </remarks>
    public string BossId { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the display name for this boss.
    /// </summary>
    /// <remarks>
    /// Shown in combat UI, logs, and encounter notifications.
    /// Examples: "The Skeleton King", "Volcanic Wyrm"
    /// </remarks>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the description text for this boss.
    /// </summary>
    /// <remarks>
    /// Background lore and flavor text for bestiary entries and tooltips.
    /// </remarks>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the title text displayed during boss introduction.
    /// </summary>
    /// <remarks>
    /// Dramatic subtitle shown when encountering the boss.
    /// Example: "Lord of the Undead Crypt"
    /// </remarks>
    public string? TitleText { get; private set; }

    // ═══════════════════════════════════════════════════════════════
    // BASE MONSTER
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the base monster definition ID for stats.
    /// </summary>
    /// <remarks>
    /// References a standard monster definition that provides base stats
    /// (HP, attack, defense, etc.). Phase modifiers are applied on top.
    /// </remarks>
    public string BaseMonsterDefinitionId { get; private set; } = string.Empty;

    // ═══════════════════════════════════════════════════════════════
    // PHASES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the combat phases for this boss.
    /// </summary>
    /// <remarks>
    /// Phases are ordered by <see cref="BossPhase.HealthThreshold"/> descending.
    /// Use <see cref="GetPhaseForHealth"/> to determine the active phase.
    /// </remarks>
    public IReadOnlyList<BossPhase> Phases => _phases.AsReadOnly();
    private readonly List<BossPhase> _phases = new();

    /// <summary>
    /// Gets the number of phases in this boss encounter.
    /// </summary>
    public int PhaseCount => _phases.Count;

    /// <summary>
    /// Gets whether this boss has multiple phases.
    /// </summary>
    public bool HasMultiplePhases => _phases.Count > 1;

    // ═══════════════════════════════════════════════════════════════
    // LOOT
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the loot table for this boss.
    /// </summary>
    /// <remarks>
    /// Contains both guaranteed drops and chance-based rare items.
    /// Loot is rolled when the boss is defeated.
    /// </remarks>
    public IReadOnlyList<BossLootEntry> Loot => _loot.AsReadOnly();
    private readonly List<BossLootEntry> _loot = new();

    /// <summary>
    /// Gets the guaranteed loot entries (100% drop chance).
    /// </summary>
    public IEnumerable<BossLootEntry> GuaranteedLoot => _loot.Where(l => l.IsGuaranteed);

    /// <summary>
    /// Gets the chance-based loot entries (less than 100% drop chance).
    /// </summary>
    public IEnumerable<BossLootEntry> ChanceLoot => _loot.Where(l => !l.IsGuaranteed);

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS & FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Private constructor for controlled creation.
    /// </summary>
    private BossDefinition() { }

    /// <summary>
    /// Creates a new boss definition with required properties.
    /// </summary>
    /// <param name="bossId">Unique identifier for the boss.</param>
    /// <param name="name">Display name for the boss.</param>
    /// <param name="description">Description text for the boss.</param>
    /// <param name="baseMonsterDefinitionId">Reference to base monster for stats.</param>
    /// <returns>A new <see cref="BossDefinition"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when any required string parameter is null or whitespace.
    /// </exception>
    /// <example>
    /// <code>
    /// var boss = BossDefinition.Create(
    ///     "shadow-lich",
    ///     "Shadow Lich",
    ///     "A master of dark magic who cheated death itself",
    ///     "lich-base");
    /// </code>
    /// </example>
    public static BossDefinition Create(
        string bossId,
        string name,
        string description,
        string baseMonsterDefinitionId)
    {
        if (string.IsNullOrWhiteSpace(bossId))
        {
            throw new ArgumentException("Boss ID cannot be null or empty.", nameof(bossId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(baseMonsterDefinitionId))
        {
            throw new ArgumentException("Base monster definition ID cannot be null or empty.", nameof(baseMonsterDefinitionId));
        }

        return new BossDefinition
        {
            BossId = bossId.ToLowerInvariant(),
            Name = name,
            Description = description ?? string.Empty,
            BaseMonsterDefinitionId = baseMonsterDefinitionId.ToLowerInvariant()
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // FLUENT CONFIGURATION METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Sets the title text for boss introduction.
    /// </summary>
    /// <param name="title">The dramatic title to display.</param>
    /// <returns>This instance for method chaining.</returns>
    /// <example>
    /// <code>
    /// boss.WithTitleText("Ancient Guardian of the Flame");
    /// </code>
    /// </example>
    public BossDefinition WithTitleText(string title)
    {
        TitleText = title;
        return this;
    }

    /// <summary>
    /// Adds a combat phase to this boss.
    /// </summary>
    /// <param name="phase">The phase configuration to add.</param>
    /// <returns>This instance for method chaining.</returns>
    /// <remarks>
    /// Phases are automatically sorted by health threshold descending
    /// when <see cref="GetPhaseForHealth"/> is called.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="phase"/> is null.
    /// </exception>
    /// <example>
    /// <code>
    /// boss.WithPhase(BossPhase.Create(1, "Normal", 100, BossBehavior.Tactical))
    ///     .WithPhase(BossPhase.Create(2, "Enraged", 50, BossBehavior.Aggressive));
    /// </code>
    /// </example>
    public BossDefinition WithPhase(BossPhase phase)
    {
        ArgumentNullException.ThrowIfNull(phase);
        _phases.Add(phase);
        return this;
    }

    /// <summary>
    /// Adds multiple combat phases to this boss.
    /// </summary>
    /// <param name="phases">The phase configurations to add.</param>
    /// <returns>This instance for method chaining.</returns>
    public BossDefinition WithPhases(IEnumerable<BossPhase> phases)
    {
        _phases.AddRange(phases.Where(p => p != null));
        return this;
    }

    /// <summary>
    /// Adds a loot entry to this boss.
    /// </summary>
    /// <param name="lootEntry">The loot entry to add.</param>
    /// <returns>This instance for method chaining.</returns>
    /// <example>
    /// <code>
    /// boss.WithLoot(BossLootEntry.Guaranteed("gold", 1000))
    ///     .WithLoot(BossLootEntry.Create("legendary-sword", 0.10));
    /// </code>
    /// </example>
    public BossDefinition WithLoot(BossLootEntry lootEntry)
    {
        if (lootEntry.IsValid)
        {
            _loot.Add(lootEntry);
        }
        return this;
    }

    /// <summary>
    /// Adds multiple loot entries to this boss.
    /// </summary>
    /// <param name="lootEntries">The loot entries to add.</param>
    /// <returns>This instance for method chaining.</returns>
    public BossDefinition WithLoot(IEnumerable<BossLootEntry> lootEntries)
    {
        _loot.AddRange(lootEntries.Where(l => l.IsValid));
        return this;
    }

    // ═══════════════════════════════════════════════════════════════
    // PHASE MANAGEMENT
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the active phase for a given health percentage.
    /// </summary>
    /// <param name="healthPercent">Current health as a percentage (0-100).</param>
    /// <returns>
    /// The appropriate <see cref="BossPhase"/> for the health level,
    /// or null if no phases are configured.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Returns the phase with the highest threshold that is still at or above
    /// the current health percentage. Phases are evaluated in descending
    /// threshold order.
    /// </para>
    /// <para>
    /// Example with phases at 100%, 75%, 50%, 25%:
    /// <list type="bullet">
    ///   <item><description>Health 80% → Phase 1 (threshold 100)</description></item>
    ///   <item><description>Health 60% → Phase 2 (threshold 75)</description></item>
    ///   <item><description>Health 40% → Phase 3 (threshold 50)</description></item>
    ///   <item><description>Health 10% → Phase 4 (threshold 25)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var currentPhase = boss.GetPhaseForHealth(45);
    /// if (currentPhase != null)
    /// {
    ///     Console.WriteLine($"Boss is in phase: {currentPhase.Name}");
    /// }
    /// </code>
    /// </example>
    public BossPhase? GetPhaseForHealth(int healthPercent)
    {
        if (_phases.Count == 0)
        {
            return null;
        }

        // Clamp health to valid range
        healthPercent = Math.Clamp(healthPercent, 0, 100);

        // Sort phases by threshold descending and find the active one
        // The active phase is the one with the highest threshold <= current health
        // OR the lowest threshold phase if health is below all thresholds
        var sortedPhases = _phases.OrderByDescending(p => p.HealthThreshold).ToList();

        // Find the phase whose threshold we've crossed (health <= threshold)
        foreach (var phase in sortedPhases)
        {
            if (healthPercent <= phase.HealthThreshold)
            {
                // Continue looking for lower thresholds we've also crossed
                continue;
            }
            else
            {
                // We haven't crossed this threshold yet, so the previous one is active
                var index = sortedPhases.IndexOf(phase);
                return index > 0 ? sortedPhases[index - 1] : sortedPhases[0];
            }
        }

        // Health is at or below the lowest threshold - return lowest phase
        return sortedPhases.Last();
    }

    /// <summary>
    /// Gets a specific phase by number.
    /// </summary>
    /// <param name="phaseNumber">The phase number (1-based).</param>
    /// <returns>The phase, or null if not found.</returns>
    /// <example>
    /// <code>
    /// var phase2 = boss.GetPhase(2);
    /// </code>
    /// </example>
    public BossPhase? GetPhase(int phaseNumber)
    {
        return _phases.FirstOrDefault(p => p.PhaseNumber == phaseNumber);
    }

    /// <summary>
    /// Gets the starting phase (highest health threshold).
    /// </summary>
    /// <returns>The first phase, or null if no phases configured.</returns>
    public BossPhase? GetStartingPhase()
    {
        return _phases.OrderByDescending(p => p.HealthThreshold).FirstOrDefault();
    }

    /// <summary>
    /// Gets the final phase (lowest health threshold).
    /// </summary>
    /// <returns>The final phase, or null if no phases configured.</returns>
    public BossPhase? GetFinalPhase()
    {
        return _phases.OrderBy(p => p.HealthThreshold).FirstOrDefault();
    }

    // ═══════════════════════════════════════════════════════════════
    // VALIDATION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this boss definition is valid.
    /// </summary>
    /// <remarks>
    /// A valid boss has non-empty IDs, name, at least one phase,
    /// and all phases are individually valid.
    /// </remarks>
    public bool IsValid =>
        !string.IsNullOrWhiteSpace(BossId) &&
        !string.IsNullOrWhiteSpace(Name) &&
        !string.IsNullOrWhiteSpace(BaseMonsterDefinitionId) &&
        _phases.Count > 0 &&
        _phases.All(p => p.IsValid);

    /// <summary>
    /// Validates the phase configuration and returns any issues found.
    /// </summary>
    /// <returns>A list of validation error messages, empty if valid.</returns>
    public IReadOnlyList<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(BossId))
        {
            errors.Add("Boss ID is required.");
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            errors.Add("Name is required.");
        }

        if (string.IsNullOrWhiteSpace(BaseMonsterDefinitionId))
        {
            errors.Add("Base monster definition ID is required.");
        }

        if (_phases.Count == 0)
        {
            errors.Add("At least one phase is required.");
        }

        // Check for duplicate phase numbers
        var duplicatePhaseNumbers = _phases
            .GroupBy(p => p.PhaseNumber)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        foreach (var phaseNum in duplicatePhaseNumbers)
        {
            errors.Add($"Duplicate phase number: {phaseNum}");
        }

        // Check that phase 1 has threshold 100
        var phase1 = _phases.FirstOrDefault(p => p.PhaseNumber == 1);
        if (phase1 != null && phase1.HealthThreshold != 100)
        {
            errors.Add($"Phase 1 should have health threshold of 100, found {phase1.HealthThreshold}.");
        }

        // Check for invalid individual phases
        foreach (var phase in _phases.Where(p => !p.IsValid))
        {
            errors.Add($"Phase {phase.PhaseNumber} is invalid.");
        }

        return errors;
    }

    /// <inheritdoc />
    public override string ToString() => $"{Name} ({BossId}) - {PhaseCount} phase(s)";
}
