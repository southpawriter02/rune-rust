// ═══════════════════════════════════════════════════════════════════════════════
// HoldTheLineState.cs
// Immutable value object tracking the active state of the Hold the Line ability,
// including movement blocking logic and duration management.
// Version: 0.20.1b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the state of an active Hold the Line effect for the Skjaldmær.
/// </summary>
/// <remarks>
/// <para>
/// Hold the Line is a Tier 2 active ability (3 AP cost) that prevents enemies
/// from moving through the Skjaldmær's space for 2 turns. Allies pass freely.
/// The effect ends early if the Skjaldmær moves from the blocked position.
/// </para>
/// <para>
/// This is an immutable value object. State transitions (activate/tick/deactivate)
/// produce new instances via factory methods and <c>with</c> expressions.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var state = HoldTheLineState.Activate((5, 5));
/// // state.IsActive == true, state.TurnsRemaining == 2
/// var ticked = state.Tick();
/// // ticked.TurnsRemaining == 1, ticked.IsActive == true
/// var expired = ticked.Tick();
/// // expired.TurnsRemaining == 0, expired.IsActive == false
/// </code>
/// </example>
public sealed record HoldTheLineState
{
    /// <summary>Default duration in turns when Hold the Line is activated.</summary>
    public const int DefaultDuration = 2;

    /// <summary>AP cost to activate Hold the Line.</summary>
    public const int ActivationCost = 3;

    /// <summary>Gets whether the Hold the Line effect is currently active.</summary>
    public bool IsActive { get; init; }

    /// <summary>Gets the number of turns remaining before the effect expires.</summary>
    public int TurnsRemaining { get; init; }

    /// <summary>Gets the position where Hold the Line was activated (blocked position).</summary>
    public (int X, int Y) BlockedPosition { get; init; }

    /// <summary>Gets the timestamp when Hold the Line was activated.</summary>
    public DateTime? ActivatedAt { get; init; }

    /// <summary>
    /// Creates an active Hold the Line state at the specified position.
    /// </summary>
    /// <param name="position">Grid position that enemies cannot move through.</param>
    /// <returns>An active HoldTheLineState with <see cref="DefaultDuration"/> turns remaining.</returns>
    public static HoldTheLineState Activate((int X, int Y) position) => new()
    {
        IsActive = true,
        TurnsRemaining = DefaultDuration,
        BlockedPosition = position,
        ActivatedAt = DateTime.UtcNow
    };

    /// <summary>
    /// Creates an inactive (deactivated) Hold the Line state.
    /// </summary>
    /// <returns>An inactive HoldTheLineState with 0 turns remaining.</returns>
    public static HoldTheLineState Deactivate() => new()
    {
        IsActive = false,
        TurnsRemaining = 0,
        ActivatedAt = null
    };

    /// <summary>
    /// Advances the effect by one turn. Deactivates automatically when turns reach 0.
    /// </summary>
    /// <returns>
    /// A new instance with decremented turns. If turns reach 0, the state is inactive.
    /// Returns self unchanged if already inactive.
    /// </returns>
    public HoldTheLineState Tick()
    {
        if (!IsActive)
            return this;

        var newTurns = TurnsRemaining - 1;
        return this with
        {
            TurnsRemaining = newTurns,
            IsActive = newTurns > 0
        };
    }

    /// <summary>
    /// Gets whether the effect has expired (inactive or no turns remaining).
    /// </summary>
    /// <returns>True if the effect is no longer active.</returns>
    public bool IsExpired() => !IsActive || TurnsRemaining <= 0;

    /// <summary>
    /// Gets the list of positions that are blocked by this effect.
    /// </summary>
    /// <returns>
    /// A single-element list containing <see cref="BlockedPosition"/> if active;
    /// an empty list if inactive.
    /// </returns>
    public IReadOnlyList<(int X, int Y)> GetBlockedPositions()
    {
        if (!IsActive)
            return Array.Empty<(int X, int Y)>();

        return new[] { BlockedPosition };
    }

    /// <summary>
    /// Determines whether movement from one position to another is blocked
    /// by this Hold the Line effect.
    /// </summary>
    /// <param name="destination">The destination position to check.</param>
    /// <returns>True if the destination matches the blocked position while active.</returns>
    public bool IsMovementBlocked((int X, int Y) destination)
    {
        if (!IsActive)
            return false;

        return destination == BlockedPosition;
    }

    /// <summary>
    /// Checks whether the effect should terminate early because the Skjaldmær
    /// has moved away from the blocked position.
    /// </summary>
    /// <param name="currentPosition">The Skjaldmær's current position.</param>
    /// <returns>True if the Skjaldmær has moved and the effect should end.</returns>
    public bool ShouldTerminateEffect((int X, int Y) currentPosition)
    {
        if (!IsActive)
            return false;

        return currentPosition != BlockedPosition;
    }

    /// <inheritdoc />
    public override string ToString() =>
        IsActive
            ? $"Hold the Line [ACTIVE] at ({BlockedPosition.X}, {BlockedPosition.Y}): " +
              $"{TurnsRemaining} turns remaining"
            : "Hold the Line [INACTIVE]";
}
