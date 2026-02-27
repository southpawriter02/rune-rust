namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the Aether Resonance resource for the Seiðkona (Seeress) specialization.
/// Aether Resonance builds through casting and is NOT consumed (except by the Unraveling
/// capstone), creating an escalating risk-reward tension unique among specializations.
/// </summary>
/// <remarks>
/// <para>Aether Resonance is a building resource (0–10) that increases with each Aether cast:</para>
/// <list type="bullet">
/// <item>Built from: Seiðr Bolt (+1 per cast), future T2/T3 abilities</item>
/// <item>NOT consumed by abilities (unlike Rage or Medical Supplies)</item>
/// <item>Only reset by: death, Unraveling capstone (v0.20.8c), or specific mechanics</item>
/// <item>Persists across combats — strategic resource management puzzle</item>
/// </list>
/// <para>Corruption risk scales with Resonance level via probability-based d100 checks:</para>
/// <list type="bullet">
/// <item>0–4 (Safe): 0% risk — no Corruption check performed</item>
/// <item>5–7 (Risky): 5% risk — d100 ≤ 5 triggers +1 Corruption</item>
/// <item>8–9 (Dangerous): 15% risk — d100 ≤ 15 triggers +1 Corruption</item>
/// <item>10 (Critical): 25% risk — d100 ≤ 25 triggers +1 Corruption</item>
/// </list>
/// <para>Uses mutable <c>private set</c> properties (like <see cref="RageResource"/>) because
/// Resonance is mutated every combat turn via Seiðr Bolt casts. This is an intentional
/// deviation from strict immutability for combat performance.</para>
/// </remarks>
public sealed record AetherResonanceResource
{
    /// <summary>
    /// Default maximum Aether Resonance value.
    /// </summary>
    public const int DefaultMaxResonance = 10;

    /// <summary>
    /// Minimum Aether Resonance value.
    /// </summary>
    private const int MinResonance = 0;

    /// <summary>
    /// Resonance threshold at which Corruption risk begins (5+).
    /// </summary>
    private const int CorruptionRiskThreshold = 5;

    /// <summary>
    /// Resonance threshold for the Dangerous risk tier (8+).
    /// </summary>
    private const int DangerousThreshold = 8;

    /// <summary>
    /// Corruption risk percentage at the Risky tier (Resonance 5–7).
    /// </summary>
    private const int RiskyRiskPercent = 5;

    /// <summary>
    /// Corruption risk percentage at the Dangerous tier (Resonance 8–9).
    /// </summary>
    private const int DangerousRiskPercent = 15;

    /// <summary>
    /// Corruption risk percentage at the Critical tier (Resonance 10).
    /// </summary>
    private const int CriticalRiskPercent = 25;

    /// <summary>
    /// Current Aether Resonance value (0 to <see cref="MaxResonance"/>).
    /// Represents the Seiðkona's current attunement to raw Aetheric energy.
    /// Higher values unlock stronger abilities but increase Corruption probability.
    /// </summary>
    public int CurrentResonance { get; private set; }

    /// <summary>
    /// Maximum Aether Resonance value (default 10).
    /// Defines the hard cap for Resonance accumulation.
    /// </summary>
    public int MaxResonance { get; init; } = DefaultMaxResonance;

    /// <summary>
    /// UTC timestamp of the last Resonance modification.
    /// Used for UI display and audit trails.
    /// </summary>
    public DateTime? LastModifiedAt { get; private set; }

    /// <summary>
    /// Descriptive source of the last Resonance build or reset.
    /// Used for logging and combat log display.
    /// </summary>
    public string? LastModificationSource { get; private set; }

    /// <summary>
    /// Creates a new AetherResonanceResource at zero Resonance with default maximum.
    /// </summary>
    /// <returns>A new resource initialized to 0/10 Resonance.</returns>
    public static AetherResonanceResource Create()
    {
        return new AetherResonanceResource
        {
            MaxResonance = DefaultMaxResonance,
            CurrentResonance = MinResonance,
            LastModifiedAt = DateTime.UtcNow,
            LastModificationSource = "Initialized"
        };
    }

    /// <summary>
    /// Creates a new AetherResonanceResource at zero Resonance with a custom maximum.
    /// </summary>
    /// <param name="maxResonance">Maximum Resonance value. Must be positive.</param>
    /// <returns>A new resource initialized to 0/maxResonance Resonance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxResonance"/> is not positive.</exception>
    public static AetherResonanceResource Create(int maxResonance)
    {
        if (maxResonance <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxResonance), maxResonance,
                "Maximum Resonance must be positive.");

        return new AetherResonanceResource
        {
            MaxResonance = maxResonance,
            CurrentResonance = MinResonance,
            LastModifiedAt = DateTime.UtcNow,
            LastModificationSource = "Initialized"
        };
    }

    /// <summary>
    /// Creates a new AetherResonanceResource at a specific Resonance value.
    /// Used primarily for testing and save/load scenarios.
    /// </summary>
    /// <param name="currentResonance">Starting Resonance value (clamped to 0–maxResonance).</param>
    /// <param name="maxResonance">Maximum Resonance value. Must be positive.</param>
    /// <returns>A new resource initialized to the specified values.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxResonance"/> is not positive.</exception>
    public static AetherResonanceResource CreateAt(int currentResonance, int maxResonance)
    {
        if (maxResonance <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxResonance), maxResonance,
                "Maximum Resonance must be positive.");

        return new AetherResonanceResource
        {
            MaxResonance = maxResonance,
            CurrentResonance = Math.Clamp(currentResonance, MinResonance, maxResonance),
            LastModifiedAt = DateTime.UtcNow,
            LastModificationSource = "Initialized"
        };
    }

    /// <summary>
    /// Builds Aether Resonance from a specified source, capped at <see cref="MaxResonance"/>.
    /// Unlike Rage's <c>Gain</c> method, Resonance is never spent by normal abilities —
    /// it only builds, creating escalating Corruption risk.
    /// </summary>
    /// <param name="amount">Amount of Resonance to build. Must be positive.</param>
    /// <param name="source">Descriptive source of the Resonance build (e.g., "Seiðr Bolt cast").</param>
    /// <returns>The actual amount of Resonance gained (may be less than requested if capped).</returns>
    public int Build(int amount, string source)
    {
        if (amount <= 0)
            return 0;

        var previousResonance = CurrentResonance;
        CurrentResonance = Math.Min(CurrentResonance + amount, MaxResonance);
        LastModifiedAt = DateTime.UtcNow;
        LastModificationSource = source;
        return CurrentResonance - previousResonance;
    }

    /// <summary>
    /// Resets Aether Resonance to zero. Called by the Unraveling capstone (v0.20.8c)
    /// or upon character death.
    /// </summary>
    /// <param name="source">Descriptive source of the reset (e.g., "Unraveling capstone", "Character death").</param>
    public void Reset(string source)
    {
        CurrentResonance = MinResonance;
        LastModifiedAt = DateTime.UtcNow;
        LastModificationSource = source;
    }

    /// <summary>
    /// Gets the Corruption risk percentage for the current Resonance level.
    /// </summary>
    /// <returns>
    /// 0 (Resonance 0–4), 5 (Resonance 5–7), 15 (Resonance 8–9), or 25 (Resonance 10).
    /// </returns>
    public int GetCorruptionRiskPercent()
    {
        return CurrentResonance switch
        {
            >= DefaultMaxResonance => CriticalRiskPercent,
            >= DangerousThreshold => DangerousRiskPercent,
            >= CorruptionRiskThreshold => RiskyRiskPercent,
            _ => 0
        };
    }

    /// <summary>
    /// Gets whether the Seiðkona is in the Corruption risk range (Resonance 5+).
    /// When true, active Aether abilities will trigger probability-based Corruption checks.
    /// </summary>
    public bool IsInCorruptionRange => CurrentResonance >= CorruptionRiskThreshold;

    /// <summary>
    /// Gets whether the Seiðkona is at maximum Resonance (10), the Critical tier.
    /// At maximum Resonance, the Unraveling capstone becomes most effective and
    /// Corruption risk is at its highest (25%).
    /// </summary>
    public bool IsAtMaxResonance => CurrentResonance >= MaxResonance;

    /// <summary>
    /// Classifies the current Resonance level into a named tier for display and logging.
    /// </summary>
    /// <returns>
    /// "Safe" (0–4), "Risky" (5–7), "Dangerous" (8–9), or "Critical" (10).
    /// </returns>
    public string GetResonanceLevel()
    {
        return CurrentResonance switch
        {
            >= DefaultMaxResonance => "Critical",
            >= DangerousThreshold => "Dangerous",
            >= CorruptionRiskThreshold => "Risky",
            _ => "Safe"
        };
    }

    /// <summary>
    /// Gets a formatted status string for combat log display.
    /// </summary>
    /// <returns>A string in the format "Aether Resonance: 7/10 [Risky — 5% Corruption risk]".</returns>
    public string GetStatusString()
    {
        var riskPercent = GetCorruptionRiskPercent();
        var riskDisplay = riskPercent > 0
            ? $"{GetResonanceLevel()} — {riskPercent}% Corruption risk"
            : "Safe — no Corruption risk";
        return $"Aether Resonance: {CurrentResonance}/{MaxResonance} [{riskDisplay}]";
    }

    /// <summary>
    /// Gets a formatted Resonance value for UI display.
    /// </summary>
    /// <returns>A string in the format "7/10".</returns>
    public string GetFormattedValue()
    {
        return $"{CurrentResonance}/{MaxResonance}";
    }
}
