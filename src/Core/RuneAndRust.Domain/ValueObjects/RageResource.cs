// ═══════════════════════════════════════════════════════════════════════════════
// RageResource.cs
// Value object representing the Berserkr's Rage resource (0–100).
// Tracks current Rage, generation sources, and provides threshold classification.
// Version: 0.20.5a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the Rage resource for the Berserkr specialization.
/// </summary>
/// <remarks>
/// <para>
/// Rage is generated through combat interactions (taking damage, bloodying enemies)
/// and spent on powerful active abilities like Fury Strike. Unlike Shadow Essence
/// which regenerates passively in darkness, Rage is purely combat-driven and
/// decays out of combat.
/// </para>
/// <para>
/// Rage generation rates:
/// </para>
/// <list type="bullet">
///   <item><description><b>Pain is Fuel:</b> +5 Rage per damage instance received</description></item>
///   <item><description><b>Blood Scent:</b> +10 Rage when any enemy becomes bloodied</description></item>
///   <item><description><b>Out-of-combat decay:</b> −10 Rage per decay tick</description></item>
/// </list>
/// <para>
/// Rage thresholds determine mechanical effects and Corruption risk:
/// </para>
/// <list type="bullet">
///   <item><description><b>0–19 (Calm):</b> No bonuses</description></item>
///   <item><description><b>20–39 (Irritated):</b> Minor combat awareness</description></item>
///   <item><description><b>40–59 (Angry):</b> Growing aggression</description></item>
///   <item><description><b>60–79 (Furious):</b> Significant bonuses</description></item>
///   <item><description><b>80–99 (Enraged):</b> Peak power, Corruption risk</description></item>
///   <item><description><b>100 (Berserk):</b> Maximum power and risk</description></item>
/// </list>
/// <example>
/// <code>
/// var rage = RageResource.Create();
/// rage.Gain(25, "Pain is Fuel");
/// // rage.CurrentRage = 25, rage.GetRageLevel() = RageLevel.Irritated
///
/// rage.GainFromDamage(15);
/// // rage.CurrentRage = 30 (+5, not based on damage amount)
///
/// rage.GainFromBloodied();
/// // rage.CurrentRage = 40 (+10), rage.GetRageLevel() = RageLevel.Angry
///
/// var spent = rage.Spend(20);
/// // spent = true, rage.CurrentRage = 20
/// </code>
/// </example>
/// </remarks>
/// <seealso cref="RageLevel"/>
/// <seealso cref="RageThreshold"/>
/// <seealso cref="BerserkrAbilityId"/>
public sealed record RageResource
{
    // ─────────────────────────────────────────────────────────────────────────
    // Constants
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Default maximum Rage capacity.</summary>
    public const int DefaultMaxRage = 100;

    /// <summary>Rage gained per damage instance via Pain is Fuel.</summary>
    public const int PainIsFuelGain = 5;

    /// <summary>Rage gained when an enemy becomes bloodied via Blood Scent.</summary>
    public const int BloodScentGain = 10;

    /// <summary>Rage cost for Fury Strike.</summary>
    public const int FuryStrikeCost = 20;

    /// <summary>Rage lost per out-of-combat decay tick.</summary>
    public const int OutOfCombatDecay = 10;

    /// <summary>Minimum Rage threshold for Enraged state (Corruption risk).</summary>
    public const int EnragedThreshold = 80;

    // ─────────────────────────────────────────────────────────────────────────
    // Properties
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Current Rage points (0 to <see cref="MaxRage"/>).
    /// </summary>
    public int CurrentRage { get; private set; }

    /// <summary>
    /// Maximum Rage capacity. Defaults to <see cref="DefaultMaxRage"/>.
    /// </summary>
    public int MaxRage { get; init; } = DefaultMaxRage;

    /// <summary>
    /// When Rage was last gained. Used for decay timing.
    /// </summary>
    public DateTime? LastGainedAt { get; private set; }

    /// <summary>
    /// Source description of the most recent Rage gain.
    /// </summary>
    public string? GainSource { get; private set; }

    /// <summary>
    /// Whether the Berserkr is in the Enraged state (80+ Rage).
    /// Abilities used in this state may trigger Corruption.
    /// </summary>
    public bool IsEnraged => CurrentRage >= EnragedThreshold;

    // ─────────────────────────────────────────────────────────────────────────
    // Factory Methods
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new RageResource at zero Rage (default starting state).
    /// </summary>
    /// <returns>A new resource initialized to 0/<see cref="DefaultMaxRage"/>.</returns>
    public static RageResource Create() =>
        new() { CurrentRage = 0, MaxRage = DefaultMaxRage };

    /// <summary>
    /// Creates a RageResource with a custom maximum capacity.
    /// </summary>
    /// <param name="maxRage">Maximum Rage capacity. Must be positive.</param>
    /// <returns>A new resource initialized to 0/<paramref name="maxRage"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxRage"/> is not positive.</exception>
    public static RageResource Create(int maxRage)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxRage, 1);
        return new RageResource { CurrentRage = 0, MaxRage = maxRage };
    }

    /// <summary>
    /// Creates a RageResource at a specific Rage value (for testing/restoration).
    /// </summary>
    /// <param name="currentRage">Initial Rage value.</param>
    /// <param name="maxRage">Maximum Rage capacity.</param>
    /// <returns>A new resource at the specified values.</returns>
    public static RageResource CreateAt(int currentRage, int maxRage = DefaultMaxRage)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxRage, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(currentRage);

        return new RageResource
        {
            CurrentRage = Math.Min(currentRage, maxRage),
            MaxRage = maxRage
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Resource Operations
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Attempts to spend the specified amount of Rage.
    /// </summary>
    /// <param name="amount">Number of Rage points to spend. Must be positive.</param>
    /// <returns>
    /// <c>true</c> if sufficient Rage was available and spent;
    /// <c>false</c> if insufficient Rage (no partial spend occurs).
    /// </returns>
    public bool Spend(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(amount, 1);

        if (CurrentRage < amount)
            return false;

        CurrentRage -= amount;
        return true;
    }

    /// <summary>
    /// Adds Rage from a named source, capped at <see cref="MaxRage"/>.
    /// </summary>
    /// <param name="amount">Number of Rage points to add. Must be positive.</param>
    /// <param name="source">Description of the Rage source (e.g., "Pain is Fuel").</param>
    public void Gain(int amount, string source)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(amount, 1);
        ArgumentException.ThrowIfNullOrWhiteSpace(source);

        CurrentRage = Math.Min(CurrentRage + amount, MaxRage);
        LastGainedAt = DateTime.UtcNow;
        GainSource = source;
    }

    /// <summary>
    /// Gains Rage from taking damage (Pain is Fuel passive).
    /// Always adds <see cref="PainIsFuelGain"/> regardless of damage amount.
    /// </summary>
    /// <param name="damageReceived">Damage amount received (must be positive, used for validation only).</param>
    public void GainFromDamage(int damageReceived)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(damageReceived, 1);

        Gain(PainIsFuelGain, "Pain is Fuel");
    }

    /// <summary>
    /// Gains Rage from an enemy becoming bloodied (Blood Scent passive).
    /// Always adds <see cref="BloodScentGain"/>.
    /// </summary>
    public void GainFromBloodied()
    {
        Gain(BloodScentGain, "Blood Scent");
    }

    /// <summary>
    /// Applies out-of-combat Rage decay, reducing by <see cref="OutOfCombatDecay"/>.
    /// Rage cannot decay below zero.
    /// </summary>
    public void DecayOutOfCombat()
    {
        CurrentRage = Math.Max(CurrentRage - OutOfCombatDecay, 0);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Classification
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Classifies the current Rage into a named <see cref="RageLevel"/>.
    /// </summary>
    /// <returns>The current Rage level classification.</returns>
    public RageLevel GetRageLevel() => CurrentRage switch
    {
        >= 100 => RageLevel.Berserk,
        >= 80 => RageLevel.Enraged,
        >= 60 => RageLevel.Furious,
        >= 40 => RageLevel.Angry,
        >= 20 => RageLevel.Irritated,
        _ => RageLevel.Calm
    };

    /// <summary>
    /// Checks whether the specified Rage level threshold is currently met.
    /// </summary>
    /// <param name="level">The Rage level to check against.</param>
    /// <returns><c>true</c> if the current Rage is at or above the threshold for the given level.</returns>
    public bool IsAtThreshold(RageLevel level) => GetRageLevel() >= level;

    // ─────────────────────────────────────────────────────────────────────────
    // Display
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Gets a formatted status string showing current Rage and level.
    /// </summary>
    /// <returns>Formatted string (e.g., "45/100 [Angry]").</returns>
    public string GetStatusString() =>
        $"{CurrentRage}/{MaxRage} [{GetRageLevel()}]";

    /// <summary>
    /// Gets a formatted Rage value string.
    /// </summary>
    /// <returns>Formatted string (e.g., "45/100").</returns>
    public string GetFormattedValue() =>
        $"{CurrentRage}/{MaxRage}";

    /// <summary>
    /// Calculates current Rage as a percentage of maximum.
    /// </summary>
    /// <returns>Integer percentage (0–100).</returns>
    public int GetPercentage() =>
        MaxRage > 0 ? (int)((double)CurrentRage / MaxRage * 100) : 0;

    /// <summary>
    /// Returns a diagnostic representation of this resource.
    /// </summary>
    public override string ToString() =>
        $"Rage({CurrentRage}/{MaxRage}, Level={GetRageLevel()}, Enraged={IsEnraged})";
}
