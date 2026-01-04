using System;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models.Magic;

namespace RuneAndRust.Core.Events;

/// <summary>
/// Published when a character's Aetheric Resonance changes significantly.
/// </summary>
public record ResonanceChangedEvent(
    Guid CharacterId,
    string CharacterName,
    int OldValue,
    int NewValue,
    int ChangeAmount,
    string Source,
    ResonanceThreshold OldThreshold,
    ResonanceThreshold NewThreshold)
{
    /// <summary>
    /// True if a threshold boundary was crossed.
    /// </summary>
    public bool ThresholdChanged => OldThreshold != NewThreshold;

    /// <summary>
    /// True if resonance increased.
    /// </summary>
    public bool IsIncrease => ChangeAmount > 0;

    /// <summary>
    /// True if resonance decreased.
    /// </summary>
    public bool IsDecrease => ChangeAmount < 0;

    /// <summary>
    /// True if resonance hit maximum.
    /// </summary>
    public bool HitMaximum => NewValue >= ResonanceState.MaxResonance;

    /// <summary>
    /// True if resonance hit minimum.
    /// </summary>
    public bool HitMinimum => NewValue <= ResonanceState.MinResonance;

    /// <summary>
    /// Direction of the threshold change (up, down, or none).
    /// </summary>
    public string ThresholdDirection => ThresholdChanged
        ? (NewThreshold > OldThreshold ? "Ascending" : "Descending")
        : "Stable";
}
