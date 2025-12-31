using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Events;

/// <summary>
/// Event published when a character's corruption value changes.
/// Tracks tier transitions and the "Lost" state.
/// </summary>
/// <remarks>
/// See: v0.4.3d (The Backlash) for implementation details.
/// </remarks>
/// <param name="CharacterId">The unique identifier of the character.</param>
/// <param name="CharacterName">The display name of the character.</param>
/// <param name="PreviousCorruption">Corruption value before the change.</param>
/// <param name="NewCorruption">Corruption value after the change.</param>
/// <param name="PreviousLevel">Corruption level tier before the change.</param>
/// <param name="NewLevel">Corruption level tier after the change.</param>
/// <param name="Source">Description of what caused the corruption change.</param>
public record CorruptionChangedEvent(
    Guid CharacterId,
    string CharacterName,
    int PreviousCorruption,
    int NewCorruption,
    CorruptionLevel PreviousLevel,
    CorruptionLevel NewLevel,
    string Source)
{
    /// <summary>
    /// Computed: Whether the corruption tier changed.
    /// </summary>
    public bool TierChanged => PreviousLevel != NewLevel;

    /// <summary>
    /// Computed: Whether the character became "Lost" to corruption.
    /// </summary>
    public bool BecameLost => NewLevel == CorruptionLevel.Lost && PreviousLevel != CorruptionLevel.Lost;

    /// <summary>
    /// Computed: Amount of corruption added (positive) or removed (negative).
    /// </summary>
    public int CorruptionDelta => NewCorruption - PreviousCorruption;

    /// <summary>
    /// Computed: Whether corruption was added (vs purged).
    /// </summary>
    public bool WasCorruptionGained => CorruptionDelta > 0;
}
