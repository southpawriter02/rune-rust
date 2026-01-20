using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Defines a single phase of a boss encounter.
/// </summary>
/// <remarks>
/// <para>
/// BossPhase represents a distinct stage in a boss fight with unique:
/// <list type="bullet">
///   <item><description>Health threshold triggering the phase</description></item>
///   <item><description>Available abilities during this phase</description></item>
///   <item><description>Combat behavior pattern</description></item>
///   <item><description>Stat modifications (buffs/penalties)</description></item>
///   <item><description>Optional summon configuration</description></item>
/// </list>
/// </para>
/// <para>
/// Phases are ordered by <see cref="HealthThreshold"/> in descending order.
/// Phase 1 (highest threshold) is always active at fight start.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create an enraged final phase
/// var phase = BossPhase.Create(
///         phaseNumber: 3,
///         name: "Enraged",
///         healthThreshold: 25,
///         behavior: BossBehavior.Enraged)
///     .WithAbilities("devastating-slam", "berserk-charge")
///     .WithStatModifier(StatModifier.Create("Attack", 50))
///     .WithTransitionText("The boss lets out a furious roar!");
/// </code>
/// </example>
public class BossPhase
{
    // ═══════════════════════════════════════════════════════════════
    // IDENTITY
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the sequential phase number (1-based).
    /// </summary>
    /// <remarks>
    /// Phase 1 is always the starting phase. Higher numbers indicate
    /// later phases triggered as boss health decreases.
    /// </remarks>
    public int PhaseNumber { get; private set; }

    /// <summary>
    /// Gets the display name for this phase.
    /// </summary>
    /// <remarks>
    /// Used for combat log messages and UI display.
    /// Examples: "Defensive Stance", "Enraged", "Final Form"
    /// </remarks>
    public string Name { get; private set; } = string.Empty;

    // ═══════════════════════════════════════════════════════════════
    // THRESHOLD
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the health percentage threshold that activates this phase.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When boss health drops to or below this percentage, this phase activates.
    /// Value range: 0-100, where 100 means always active (starting phase).
    /// </para>
    /// <para>
    /// Example thresholds:
    /// <list type="bullet">
    ///   <item><description>100: Starting phase</description></item>
    ///   <item><description>75: First transition at 75% health</description></item>
    ///   <item><description>50: Mid-fight phase at 50%</description></item>
    ///   <item><description>25: Enraged/final phase at 25%</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public int HealthThreshold { get; private set; }

    // ═══════════════════════════════════════════════════════════════
    // BEHAVIOR
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the combat behavior pattern for this phase.
    /// </summary>
    /// <remarks>
    /// Determines how the boss prioritizes actions during this phase.
    /// See <see cref="BossBehavior"/> for available patterns.
    /// </remarks>
    public BossBehavior Behavior { get; private set; }

    // ═══════════════════════════════════════════════════════════════
    // ABILITIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the ability IDs available during this phase.
    /// </summary>
    /// <remarks>
    /// References ability definitions. Abilities may be phase-exclusive
    /// or carried over from previous phases depending on design.
    /// </remarks>
    public IReadOnlyList<string> AbilityIds => _abilityIds.AsReadOnly();
    private readonly List<string> _abilityIds = new();

    // ═══════════════════════════════════════════════════════════════
    // STAT MODIFIERS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets stat modifiers active during this phase.
    /// </summary>
    /// <remarks>
    /// Modifiers are applied when entering the phase and removed when leaving.
    /// Used for phase-specific buffs like increased damage or defense.
    /// </remarks>
    public IReadOnlyList<StatModifier> StatModifiers => _statModifiers.AsReadOnly();
    private readonly List<StatModifier> _statModifiers = new();

    // ═══════════════════════════════════════════════════════════════
    // TRANSITION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the text displayed when transitioning to this phase.
    /// </summary>
    /// <remarks>
    /// Shown in combat log when the boss enters this phase.
    /// Example: "The Skeleton King's eyes glow with fury!"
    /// </remarks>
    public string? TransitionText { get; private set; }

    /// <summary>
    /// Gets the effect ID triggered during phase transition.
    /// </summary>
    /// <remarks>
    /// Optional visual/mechanical effect played when entering the phase.
    /// Can reference status effects, zone effects, or visual effects.
    /// </remarks>
    public string? TransitionEffectId { get; private set; }

    // ═══════════════════════════════════════════════════════════════
    // SUMMONING
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the summon configuration for this phase.
    /// </summary>
    /// <remarks>
    /// If valid, the boss will summon minions according to this configuration.
    /// Check <see cref="SummonConfiguration.IsValid"/> before using.
    /// </remarks>
    public SummonConfiguration SummonConfig { get; private set; }

    /// <summary>
    /// Gets whether this phase has summoning enabled.
    /// </summary>
    public bool HasSummoning => SummonConfig.IsValid;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS & FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Private constructor for controlled creation.
    /// </summary>
    private BossPhase() { }

    /// <summary>
    /// Creates a new boss phase with required properties.
    /// </summary>
    /// <param name="phaseNumber">The sequential phase number (1-based).</param>
    /// <param name="name">Display name for the phase.</param>
    /// <param name="healthThreshold">Health percentage threshold (0-100).</param>
    /// <param name="behavior">Combat behavior pattern for this phase.</param>
    /// <returns>A new <see cref="BossPhase"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="phaseNumber"/> is less than 1,
    /// or <paramref name="healthThreshold"/> is not between 0 and 100.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="name"/> is null or whitespace.
    /// </exception>
    /// <example>
    /// <code>
    /// var phase = BossPhase.Create(1, "Initial Phase", 100, BossBehavior.Tactical);
    /// </code>
    /// </example>
    public static BossPhase Create(
        int phaseNumber,
        string name,
        int healthThreshold,
        BossBehavior behavior)
    {
        if (phaseNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(phaseNumber), phaseNumber, "Phase number must be at least 1.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Phase name cannot be null or empty.", nameof(name));
        }

        if (healthThreshold < 0 || healthThreshold > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(healthThreshold), healthThreshold, "Health threshold must be between 0 and 100.");
        }

        return new BossPhase
        {
            PhaseNumber = phaseNumber,
            Name = name,
            HealthThreshold = healthThreshold,
            Behavior = behavior
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // FLUENT CONFIGURATION METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Adds abilities available during this phase.
    /// </summary>
    /// <param name="abilityIds">The ability definition IDs to add.</param>
    /// <returns>This instance for method chaining.</returns>
    /// <example>
    /// <code>
    /// phase.WithAbilities("power-strike", "shield-bash", "rally");
    /// </code>
    /// </example>
    public BossPhase WithAbilities(params string[] abilityIds)
    {
        _abilityIds.AddRange(abilityIds.Where(id => !string.IsNullOrWhiteSpace(id)));
        return this;
    }

    /// <summary>
    /// Adds a stat modifier for this phase.
    /// </summary>
    /// <param name="modifier">The stat modifier to apply during this phase.</param>
    /// <returns>This instance for method chaining.</returns>
    /// <example>
    /// <code>
    /// phase.WithStatModifier(StatModifier.Create("Defense", -20)); // Reduced defense
    /// </code>
    /// </example>
    public BossPhase WithStatModifier(StatModifier modifier)
    {
        _statModifiers.Add(modifier);
        return this;
    }

    /// <summary>
    /// Adds multiple stat modifiers for this phase.
    /// </summary>
    /// <param name="modifiers">The stat modifiers to apply during this phase.</param>
    /// <returns>This instance for method chaining.</returns>
    public BossPhase WithStatModifiers(IEnumerable<StatModifier> modifiers)
    {
        _statModifiers.AddRange(modifiers);
        return this;
    }

    /// <summary>
    /// Sets the transition text displayed when entering this phase.
    /// </summary>
    /// <param name="text">The transition text to display.</param>
    /// <returns>This instance for method chaining.</returns>
    /// <example>
    /// <code>
    /// phase.WithTransitionText("The dragon unfurls its wings and takes flight!");
    /// </code>
    /// </example>
    public BossPhase WithTransitionText(string text)
    {
        TransitionText = text;
        return this;
    }

    /// <summary>
    /// Sets the effect triggered during phase transition.
    /// </summary>
    /// <param name="effectId">The effect definition ID to trigger.</param>
    /// <returns>This instance for method chaining.</returns>
    /// <example>
    /// <code>
    /// phase.WithTransitionEffect("boss-enrage-aura");
    /// </code>
    /// </example>
    public BossPhase WithTransitionEffect(string effectId)
    {
        TransitionEffectId = effectId;
        return this;
    }

    /// <summary>
    /// Sets the summon configuration for this phase.
    /// </summary>
    /// <param name="config">The summon configuration.</param>
    /// <returns>This instance for method chaining.</returns>
    /// <example>
    /// <code>
    /// phase.WithSummonConfig(SummonConfiguration.Create("skeleton-minion", count: 2)
    ///     .WithIntervalTurns(3)
    ///     .WithMaxActive(6));
    /// </code>
    /// </example>
    public BossPhase WithSummonConfig(SummonConfiguration config)
    {
        SummonConfig = config;
        return this;
    }

    // ═══════════════════════════════════════════════════════════════
    // VALIDATION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this phase configuration is valid.
    /// </summary>
    /// <remarks>
    /// A valid phase has a positive phase number, non-empty name,
    /// and health threshold between 0 and 100.
    /// </remarks>
    public bool IsValid =>
        PhaseNumber >= 1 &&
        !string.IsNullOrWhiteSpace(Name) &&
        HealthThreshold >= 0 &&
        HealthThreshold <= 100;

    /// <inheritdoc />
    public override string ToString() => $"Phase {PhaseNumber}: {Name} (≤{HealthThreshold}%)";
}
