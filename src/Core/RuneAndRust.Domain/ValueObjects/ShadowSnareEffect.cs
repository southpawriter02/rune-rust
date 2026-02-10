// ═══════════════════════════════════════════════════════════════════════════════
// ShadowSnareEffect.cs
// Immutable value object representing the Shadow Snare root effect applied
// to a target. Tracks duration, save DC, escape attempts, and break conditions.
// Version: 0.20.4c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents an active Shadow Snare effect binding a target in place.
/// </summary>
/// <remarks>
/// <para>
/// Shadow Snare roots a target for a base duration of 2 turns. Each turn,
/// the target may attempt a save (DC 14) to escape. The snare can also
/// break if the caster moves, is incapacitated, or the effect is dispelled.
/// </para>
/// <para>
/// This value object is immutable. All state transitions return new instances.
/// </para>
/// <example>
/// <code>
/// var snare = ShadowSnareEffect.Create(casterId, targetId);
/// // snare.IsActive = true, RemainingTurns = 2, SaveDC = 14
///
/// var (updated, escaped) = snare.AttemptEscape(rollResult: 16);
/// // escaped = true, updated.BrokeCondition = SaveSucceeded
/// </code>
/// </example>
/// </remarks>
/// <seealso cref="MyrkgengrAbilityId"/>
/// <seealso cref="BreakCondition"/>
public sealed record ShadowSnareEffect
{
    // ─────────────────────────────────────────────────────────────────────────
    // Constants
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Base duration of Shadow Snare in turns.</summary>
    public const int BaseDuration = 2;

    /// <summary>Difficulty class for escape saves.</summary>
    public const int DefaultSaveDC = 14;

    // ─────────────────────────────────────────────────────────────────────────
    // Properties
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// ID of the character who cast the snare.
    /// </summary>
    public Guid CasterId { get; private init; }

    /// <summary>
    /// ID of the target character bound by the snare.
    /// </summary>
    public Guid TargetId { get; private init; }

    /// <summary>
    /// Number of turns remaining on the snare.
    /// </summary>
    public int RemainingTurns { get; private init; }

    /// <summary>
    /// Difficulty class required to escape the snare.
    /// </summary>
    public int SaveDC { get; private init; }

    /// <summary>
    /// Number of escape attempts made by the target.
    /// </summary>
    public int EscapeAttempts { get; private init; }

    /// <summary>
    /// Whether the snare is currently active and binding the target.
    /// </summary>
    public bool IsActive { get; private init; }

    /// <summary>
    /// The condition that caused the snare to break, if it has been broken.
    /// Null while the snare is still active.
    /// </summary>
    public BreakCondition? BrokeCondition { get; private init; }

    /// <summary>
    /// The shadow binding strength holding the target.
    /// </summary>
    public ShadowBinding Binding { get; private init; } = ShadowBinding.Create();

    /// <summary>
    /// When the snare was applied.
    /// </summary>
    public DateTime AppliedAt { get; private init; }

    // ─────────────────────────────────────────────────────────────────────────
    // Factory Methods
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new active Shadow Snare effect on a target.
    /// </summary>
    /// <param name="casterId">ID of the caster.</param>
    /// <param name="targetId">ID of the target to bind.</param>
    /// <returns>A new active Shadow Snare with default duration and save DC.</returns>
    public static ShadowSnareEffect Create(Guid casterId, Guid targetId)
    {
        return new ShadowSnareEffect
        {
            CasterId = casterId,
            TargetId = targetId,
            RemainingTurns = BaseDuration,
            SaveDC = DefaultSaveDC,
            EscapeAttempts = 0,
            IsActive = true,
            BrokeCondition = null,
            Binding = ShadowBinding.Create(),
            AppliedAt = DateTime.UtcNow
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // State Transitions
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Processes an escape attempt by the target.
    /// </summary>
    /// <param name="saveRoll">The target's save roll (including modifiers).</param>
    /// <returns>
    /// A tuple of (updated snare state, whether the target escaped).
    /// </returns>
    public (ShadowSnareEffect Snare, bool Escaped) AttemptEscape(int saveRoll)
    {
        if (!IsActive)
            return (this, false);

        var newAttemptCount = EscapeAttempts + 1;

        if (saveRoll >= SaveDC)
        {
            // Target escapes
            return (this with
            {
                IsActive = false,
                EscapeAttempts = newAttemptCount,
                BrokeCondition = BreakCondition.SaveSucceeded,
                Binding = Binding.Weaken()
            }, true);
        }

        // Failed escape – binding holds
        return (this with
        {
            EscapeAttempts = newAttemptCount
        }, false);
    }

    /// <summary>
    /// Decrements the remaining turns by one. If turns reach zero, the snare ends.
    /// </summary>
    /// <returns>A new state with decremented turns, or an inactive state if expired.</returns>
    public ShadowSnareEffect TickDown()
    {
        if (!IsActive)
            return this;

        var newTurns = RemainingTurns - 1;

        if (newTurns <= 0)
        {
            return this with
            {
                IsActive = false,
                RemainingTurns = 0,
                BrokeCondition = BreakCondition.DurationExpired
            };
        }

        return this with { RemainingTurns = newTurns };
    }

    /// <summary>
    /// Breaks the snare due to a specific condition.
    /// </summary>
    /// <param name="condition">The condition causing the break.</param>
    /// <returns>A new inactive snare with the specified break condition.</returns>
    public ShadowSnareEffect Break(BreakCondition condition)
    {
        return this with
        {
            IsActive = false,
            RemainingTurns = 0,
            BrokeCondition = condition
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Display
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns a diagnostic representation of the snare effect.
    /// </summary>
    public override string ToString() =>
        $"ShadowSnare(Active={IsActive}, Turns={RemainingTurns}, " +
        $"DC={SaveDC}, Escapes={EscapeAttempts}, Broke={BrokeCondition})";
}

/// <summary>
/// Represents the strength of the shadow binding holding a target.
/// </summary>
/// <remarks>
/// Tracks the binding's integrity, which weakens on each escape attempt.
/// </remarks>
public sealed record ShadowBinding
{
    /// <summary>Default binding strength.</summary>
    public const int DefaultStrength = 100;

    /// <summary>Strength reduction per escape attempt.</summary>
    public const int WeakenAmount = 25;

    /// <summary>
    /// Current binding strength as a percentage (0–100).
    /// </summary>
    public int Strength { get; private init; }

    /// <summary>
    /// Whether the binding is still holding.
    /// </summary>
    public bool IsHolding => Strength > 0;

    /// <summary>
    /// Creates a binding at full strength.
    /// </summary>
    public static ShadowBinding Create() => new()
    {
        Strength = DefaultStrength
    };

    /// <summary>
    /// Reduces binding strength by the weaken amount (minimum 0).
    /// </summary>
    /// <returns>A new binding with reduced strength.</returns>
    public ShadowBinding Weaken() => this with
    {
        Strength = Math.Max(0, Strength - WeakenAmount)
    };

    /// <summary>
    /// Returns a diagnostic representation of the binding.
    /// </summary>
    public override string ToString() => $"ShadowBinding(Strength={Strength}%)";
}
