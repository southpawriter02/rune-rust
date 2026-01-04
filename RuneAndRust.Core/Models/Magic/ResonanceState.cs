using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Models.Magic;

/// <summary>
/// Tracks a Mystic's personal Aetheric Resonance—their attunement to ambient Flux.
/// </summary>
public class ResonanceState
{
    public const int MinResonance = 0;
    public const int MaxResonance = 100;
    public const int DefaultDecayRate = 10;

    // Threshold boundaries
    public const int DimUpperBound = 24;
    public const int SteadyUpperBound = 49;
    public const int BrightUpperBound = 74;
    public const int BlazingUpperBound = 99;

    /// <summary>
    /// Current resonance value (0-100).
    /// </summary>
    public int CurrentValue { get; set; } = MinResonance;

    /// <summary>
    /// Amount of resonance lost per rest period.
    /// </summary>
    public int DecayRate { get; set; } = DefaultDecayRate;

    /// <summary>
    /// Number of times this character has triggered Overflow.
    /// Used to calculate Soul Fracture risk.
    /// </summary>
    public int OverflowCount { get; set; } = 0;

    /// <summary>
    /// Whether the character is currently in Overflow state (1 turn of +50% potency).
    /// </summary>
    public bool IsOverflowActive { get; set; } = false;

    /// <summary>
    /// Current threshold based on resonance value.
    /// </summary>
    public ResonanceThreshold Threshold => CurrentValue switch
    {
        >= MaxResonance => ResonanceThreshold.Overflow,
        >= 75 => ResonanceThreshold.Blazing,
        >= 50 => ResonanceThreshold.Bright,
        >= 25 => ResonanceThreshold.Steady,
        _ => ResonanceThreshold.Dim
    };

    /// <summary>
    /// Potency multiplier based on current threshold.
    /// </summary>
    public decimal PotencyModifier => Threshold switch
    {
        ResonanceThreshold.Overflow => 1.50m,
        ResonanceThreshold.Blazing => 1.30m,
        ResonanceThreshold.Bright => 1.15m,
        ResonanceThreshold.Steady => 1.00m,
        ResonanceThreshold.Dim => 0.90m,
        _ => 1.00m
    };

    /// <summary>
    /// True if resonance is at or above Bright threshold (elevated risk).
    /// </summary>
    public bool IsElevatedRisk => CurrentValue >= 50;

    /// <summary>
    /// True if resonance is at or above Blazing threshold (high risk).
    /// </summary>
    public bool IsHighRisk => CurrentValue >= 75;

    /// <summary>
    /// True if resonance has reached maximum (Overflow imminent).
    /// </summary>
    public bool IsAtMaximum => CurrentValue >= MaxResonance;

    /// <summary>
    /// Percentage of maximum resonance (0-100).
    /// </summary>
    public int PercentFull => CurrentValue;

    /// <summary>
    /// Resets resonance to minimum value.
    /// </summary>
    public void Reset()
    {
        CurrentValue = MinResonance;
        IsOverflowActive = false;
    }
}
