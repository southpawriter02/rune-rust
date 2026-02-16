using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the Rage resource for the Berserkr specialization.
/// Rage fuels offensive abilities and grants escalating combat bonuses at higher levels,
/// but risks Corruption accumulation through the Heretical Path mechanics.
/// </summary>
/// <remarks>
/// <para>Rage is a dynamic resource (0–100) that fluctuates during combat:</para>
/// <list type="bullet">
/// <item>Gained from: taking damage (Pain is Fuel +5), bloodying enemies (Blood Scent +10),
///   and combat actions</item>
/// <item>Spent on: Fury Strike (20 Rage), Unstoppable (15 Rage), Intimidating Presence (10 Rage)</item>
/// <item>Decays: -10 per round outside combat</item>
/// </list>
/// <para>Unlike <see cref="BlockChargeResource"/> and <see cref="RuneChargeResource"/>,
/// RageResource uses mutable properties for frequent in-combat updates. This is an intentional
/// design deviation from strict immutability for combat performance.</para>
/// <para>Rage levels (see <see cref="RageLevel"/>) determine passive Attack bonuses
/// and Corruption risk thresholds. The Enraged threshold (80+) is particularly significant
/// as it triggers Corruption evaluation on most ability uses.</para>
/// </remarks>
public sealed record RageResource
{
    /// <summary>
    /// Default maximum Rage value.
    /// </summary>
    public const int DefaultMaxRage = 100;

    /// <summary>
    /// Rage gained per damage instance from the Pain is Fuel passive ability.
    /// </summary>
    public const int PainIsFuelGain = 5;

    /// <summary>
    /// Rage gained when an enemy becomes bloodied from the Blood Scent passive ability.
    /// </summary>
    public const int BloodScentGain = 10;

    /// <summary>
    /// Rage cost for the Fury Strike active ability.
    /// </summary>
    public const int FuryStrikeCost = 20;

    /// <summary>
    /// Rage lost per round when out of combat (natural decay).
    /// </summary>
    public const int OutOfCombatDecay = 10;

    /// <summary>
    /// Rage threshold at which the Berserkr becomes Enraged, triggering Corruption risk.
    /// </summary>
    public const int EnragedThreshold = 80;

    /// <summary>
    /// Current Rage value (0 to <see cref="MaxRage"/>).
    /// Represents the Berserkr's current fury intensity.
    /// </summary>
    public int CurrentRage { get; private set; }

    /// <summary>
    /// Maximum Rage value (default 100).
    /// Defines the hard cap for Rage accumulation.
    /// </summary>
    public int MaxRage { get; init; } = DefaultMaxRage;

    /// <summary>
    /// UTC timestamp of the last Rage modification.
    /// Used for UI display and audit trails.
    /// </summary>
    public DateTime? LastModifiedAt { get; private set; }

    /// <summary>
    /// Descriptive source of the last Rage gain or expenditure.
    /// Used for logging and combat log display.
    /// </summary>
    public string? LastModificationSource { get; private set; }

    /// <summary>
    /// Creates a new RageResource at zero Rage with default maximum.
    /// </summary>
    /// <returns>A new resource initialized to 0/100 Rage.</returns>
    public static RageResource Create()
    {
        return new RageResource
        {
            MaxRage = DefaultMaxRage,
            CurrentRage = 0,
            LastModifiedAt = DateTime.UtcNow,
            LastModificationSource = "Initialized"
        };
    }

    /// <summary>
    /// Creates a new RageResource at zero Rage with a custom maximum.
    /// </summary>
    /// <param name="maxRage">Maximum Rage value. Must be positive.</param>
    /// <returns>A new resource initialized to 0/maxRage Rage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxRage"/> is not positive.</exception>
    public static RageResource Create(int maxRage)
    {
        if (maxRage <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxRage), maxRage, "Maximum Rage must be positive.");

        return new RageResource
        {
            MaxRage = maxRage,
            CurrentRage = 0,
            LastModifiedAt = DateTime.UtcNow,
            LastModificationSource = "Initialized"
        };
    }

    /// <summary>
    /// Creates a new RageResource at a specific Rage value.
    /// Used primarily for testing and save/load scenarios.
    /// </summary>
    /// <param name="currentRage">Starting Rage value (clamped to 0–maxRage).</param>
    /// <param name="maxRage">Maximum Rage value. Must be positive.</param>
    /// <returns>A new resource initialized to the specified values.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxRage"/> is not positive.</exception>
    public static RageResource CreateAt(int currentRage, int maxRage)
    {
        if (maxRage <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxRage), maxRage, "Maximum Rage must be positive.");

        return new RageResource
        {
            MaxRage = maxRage,
            CurrentRage = Math.Clamp(currentRage, 0, maxRage),
            LastModifiedAt = DateTime.UtcNow,
            LastModificationSource = "Initialized"
        };
    }

    /// <summary>
    /// Adds Rage from a specified source, capped at <see cref="MaxRage"/>.
    /// </summary>
    /// <param name="amount">Amount of Rage to gain. Must be positive.</param>
    /// <param name="source">Descriptive source of the Rage gain (e.g., "Pain is Fuel", "Blood Scent").</param>
    /// <returns>The actual amount of Rage gained (may be less than requested if capped).</returns>
    public int Gain(int amount, string source)
    {
        if (amount <= 0)
            return 0;

        var previousRage = CurrentRage;
        CurrentRage = Math.Min(CurrentRage + amount, MaxRage);
        LastModifiedAt = DateTime.UtcNow;
        LastModificationSource = source;
        return CurrentRage - previousRage;
    }

    /// <summary>
    /// Adds Rage from taking damage via the Pain is Fuel passive.
    /// Gains <see cref="PainIsFuelGain"/> (5) Rage per damage instance.
    /// </summary>
    /// <param name="damageTaken">Amount of damage taken. Must be positive to trigger gain.</param>
    /// <returns>The actual amount of Rage gained.</returns>
    public int GainFromDamage(int damageTaken)
    {
        if (damageTaken <= 0)
            return 0;

        return Gain(PainIsFuelGain, "Pain is Fuel");
    }

    /// <summary>
    /// Adds Rage from an enemy becoming bloodied via the Blood Scent passive.
    /// Gains <see cref="BloodScentGain"/> (10) Rage.
    /// </summary>
    /// <returns>The actual amount of Rage gained.</returns>
    public int GainFromBloodied()
    {
        return Gain(BloodScentGain, "Blood Scent");
    }

    /// <summary>
    /// Attempts to spend the specified amount of Rage.
    /// </summary>
    /// <param name="amount">Amount of Rage to spend. Must be positive.</param>
    /// <returns>True if the amount was available and successfully spent; False otherwise.</returns>
    /// <remarks>
    /// Spending is atomic — if not enough Rage is available,
    /// <see cref="CurrentRage"/> remains unchanged and False is returned.
    /// </remarks>
    public bool Spend(int amount)
    {
        if (amount <= 0 || CurrentRage < amount)
            return false;

        CurrentRage -= amount;
        LastModifiedAt = DateTime.UtcNow;
        LastModificationSource = "Spent";
        return true;
    }

    /// <summary>
    /// Decays Rage by <see cref="OutOfCombatDecay"/> (10) per round outside combat.
    /// Rage cannot drop below 0.
    /// </summary>
    /// <returns>The actual amount of Rage lost.</returns>
    public int DecayOutOfCombat()
    {
        var previousRage = CurrentRage;
        CurrentRage = Math.Max(CurrentRage - OutOfCombatDecay, 0);
        LastModifiedAt = DateTime.UtcNow;
        LastModificationSource = "Out of Combat Decay";
        return previousRage - CurrentRage;
    }

    /// <summary>
    /// Classifies the current Rage level based on threshold ranges.
    /// </summary>
    /// <returns>The <see cref="RageLevel"/> corresponding to the current Rage value.</returns>
    /// <remarks>
    /// Thresholds: Calm (0–19), Irritated (20–39), Angry (40–59),
    /// Furious (60–79), Enraged (80–99), Berserk (100).
    /// </remarks>
    public RageLevel GetRageLevel()
    {
        return CurrentRage switch
        {
            >= 100 => RageLevel.Berserk,
            >= 80 => RageLevel.Enraged,
            >= 60 => RageLevel.Furious,
            >= 40 => RageLevel.Angry,
            >= 20 => RageLevel.Irritated,
            _ => RageLevel.Calm
        };
    }

    /// <summary>
    /// Checks if the current Rage is at or above the specified level's minimum threshold.
    /// </summary>
    /// <param name="level">The Rage level to check against.</param>
    /// <returns>True if current Rage meets or exceeds the threshold for the specified level.</returns>
    public bool IsAtThreshold(RageLevel level)
    {
        return GetRageLevel() >= level;
    }

    /// <summary>
    /// Gets whether the Berserkr is Enraged (Rage at 80+), the critical threshold
    /// for Corruption risk evaluation.
    /// </summary>
    public bool IsEnraged => CurrentRage >= EnragedThreshold;

    /// <summary>
    /// Gets a formatted status string for combat log display.
    /// </summary>
    /// <returns>A string in the format "Rage: 75/100 [Furious]".</returns>
    public string GetStatusString()
    {
        return $"Rage: {CurrentRage}/{MaxRage} [{GetRageLevel()}]";
    }

    /// <summary>
    /// Gets a formatted Rage value for UI display.
    /// </summary>
    /// <returns>A string in the format "75/100".</returns>
    public string GetFormattedValue()
    {
        return $"{CurrentRage}/{MaxRage}";
    }

    /// <summary>
    /// Gets the current Rage as a percentage of maximum (0–100).
    /// </summary>
    /// <returns>The percentage of Rage filled.</returns>
    public int GetPercentage()
    {
        return MaxRage == 0 ? 0 : (int)((double)CurrentRage / MaxRage * 100);
    }
}
