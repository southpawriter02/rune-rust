// ═══════════════════════════════════════════════════════════════════════════════
// TheWallLivesState.cs
// Immutable value object tracking the active state of The Wall Lives capstone
// ability, including duration management and lethal damage prevention.
// Version: 0.20.1c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the state of The Wall Lives capstone ability effect for the Skjaldmær.
/// </summary>
/// <remarks>
/// <para>
/// The Wall Lives is a Capstone ability (4 AP cost) that prevents the character
/// from dropping below 1 HP for 3 turns. It is usable once per combat.
/// </para>
/// <para>
/// This is an immutable value object. State transitions (activate/tick/deactivate)
/// produce new instances via factory methods and <c>with</c> expressions.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var state = TheWallLivesState.Activate();
/// // state.IsActive == true, state.TurnsRemaining == 3
/// var ticked = state.Tick();
/// // ticked.TurnsRemaining == 2, ticked.IsActive == true
/// int capped = state.PreventLethalDamage(currentHp: 5, incomingDamage: 10);
/// // capped == 4 (leaves 1 HP)
/// </code>
/// </example>
public sealed record TheWallLivesState
{
    /// <summary>Default duration in turns when The Wall Lives is activated.</summary>
    public const int DefaultDuration = 3;

    /// <summary>AP cost to activate The Wall Lives.</summary>
    public const int ActivationCost = 4;

    /// <summary>Minimum HP that The Wall Lives will preserve.</summary>
    public const int MinimumHp = 1;

    /// <summary>Gets whether The Wall Lives effect is currently active.</summary>
    public bool IsActive { get; init; }

    /// <summary>Gets the number of turns remaining before the effect expires.</summary>
    public int TurnsRemaining { get; init; }

    /// <summary>Gets the timestamp when The Wall Lives was activated.</summary>
    public DateTime? ActivatedAt { get; init; }

    /// <summary>
    /// Creates an active The Wall Lives state with full duration.
    /// </summary>
    /// <returns>An active TheWallLivesState with <see cref="DefaultDuration"/> turns remaining.</returns>
    public static TheWallLivesState Activate() => new()
    {
        IsActive = true,
        TurnsRemaining = DefaultDuration,
        ActivatedAt = DateTime.UtcNow
    };

    /// <summary>
    /// Creates an inactive (deactivated) The Wall Lives state.
    /// </summary>
    /// <returns>An inactive TheWallLivesState with 0 turns remaining.</returns>
    public static TheWallLivesState Deactivate() => new()
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
    public TheWallLivesState Tick()
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
    /// Prevents lethal damage while effect is active.
    /// </summary>
    /// <param name="currentHp">Character's current HP before damage.</param>
    /// <param name="incomingDamage">Damage being applied.</param>
    /// <returns>
    /// Actual damage that should be applied. If the damage would reduce HP below 1,
    /// it is capped to leave HP at exactly 1. Returns unchanged damage if the effect
    /// is inactive or the damage is not lethal.
    /// </returns>
    public int PreventLethalDamage(int currentHp, int incomingDamage)
    {
        if (!IsActive)
            return incomingDamage;

        var resultingHp = currentHp - incomingDamage;

        // If damage would be lethal, cap damage to leave 1 HP
        if (resultingHp < MinimumHp)
        {
            var cappedDamage = currentHp - MinimumHp;
            return Math.Max(cappedDamage, 0);
        }

        return incomingDamage;
    }

    /// <summary>
    /// Gets remaining duration in turns.
    /// </summary>
    /// <returns>Number of turns until effect expires, or 0 if inactive.</returns>
    public int GetRemainingDuration() => IsActive ? TurnsRemaining : 0;

    /// <inheritdoc />
    public override string ToString() =>
        IsActive
            ? $"The Wall Lives [ACTIVE]: {TurnsRemaining} turns remaining"
            : "The Wall Lives [INACTIVE]";
}
