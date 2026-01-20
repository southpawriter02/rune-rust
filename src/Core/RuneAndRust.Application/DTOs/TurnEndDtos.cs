namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Contains all changes that occurred during turn-end processing.
/// </summary>
public record TurnEndResult(
    int NewTurnCount,
    IReadOnlyList<TurnResourceChangeDto> ResourceChanges,
    IReadOnlyList<CooldownChangeDto> CooldownChanges,
    IReadOnlyList<string> AbilitiesNowReady)
{
    /// <summary>
    /// Gets whether any changes occurred during turn-end processing.
    /// </summary>
    public bool HasChanges =>
        ResourceChanges.Count > 0 ||
        CooldownChanges.Count > 0 ||
        AbilitiesNowReady.Count > 0;

    /// <summary>
    /// Creates an empty turn-end result with no changes.
    /// </summary>
    public static TurnEndResult Empty(int turnCount) => new(
        turnCount,
        Array.Empty<TurnResourceChangeDto>(),
        Array.Empty<CooldownChangeDto>(),
        Array.Empty<string>());
}

/// <summary>
/// Represents a resource change during turn processing.
/// </summary>
/// <param name="ResourceName">The display name of the resource.</param>
/// <param name="ResourceAbbreviation">The abbreviated name of the resource.</param>
/// <param name="PreviousValue">The resource value before the change.</param>
/// <param name="NewValue">The resource value after the change.</param>
/// <param name="MaxValue">The maximum value of the resource.</param>
/// <param name="ChangeType">The type of change (Regeneration, Decay).</param>
/// <param name="Color">The display color for the resource.</param>
public record TurnResourceChangeDto(
    string ResourceName,
    string ResourceAbbreviation,
    int PreviousValue,
    int NewValue,
    int MaxValue,
    string ChangeType,
    string Color)
{
    /// <summary>
    /// Gets the delta (change amount) of this resource change.
    /// </summary>
    public int Delta => NewValue - PreviousValue;
}

/// <summary>
/// Represents a cooldown change during turn processing.
/// </summary>
/// <param name="AbilityName">The name of the ability.</param>
/// <param name="PreviousCooldown">The cooldown value before the turn.</param>
/// <param name="NewCooldown">The cooldown value after the turn.</param>
/// <param name="IsNowReady">Whether the ability is now ready to use.</param>
public record CooldownChangeDto(
    string AbilityName,
    int PreviousCooldown,
    int NewCooldown,
    bool IsNowReady);
