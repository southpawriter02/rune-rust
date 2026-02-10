// ═══════════════════════════════════════════════════════════════════════════════
// IncorporealState.cs
// Immutable value object tracking the incorporeal transformation state from
// the Myrk-gengr Merge with Darkness ability. Manages duration, extensions,
// and corruption accrual.
// Version: 0.20.4c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the incorporeal transformation state granted by
/// <see cref="Enums.MyrkgengrAbilityId.MergeWithDarkness"/>.
/// </summary>
/// <remarks>
/// <para>
/// When active, the character becomes incorporeal — able to phase through
/// solid objects and immune to physical attacks. The base duration is 1 turn,
/// extendable up to <see cref="MaxExtensions"/> additional turns at a cost
/// of extra Essence and Corruption per extension.
/// </para>
/// <para>
/// This value object is immutable. All state transitions return new instances.
/// </para>
/// <example>
/// <code>
/// var state = IncorporealState.Create();
/// // state.IsActive = true, RemainingTurns = 1
///
/// var extended = state.Extend(corruptionGain: 1);
/// // extended.RemainingTurns = 2, ExtensionCount = 1
///
/// var ticked = extended.TickDown();
/// // ticked.RemainingTurns = 1
/// </code>
/// </example>
/// </remarks>
/// <seealso cref="Enums.MyrkgengrAbilityId"/>
/// <seealso cref="Enums.IncorporealRestriction"/>
public sealed record IncorporealState
{
    // ─────────────────────────────────────────────────────────────────────────
    // Constants
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Base duration of incorporeal form in turns.</summary>
    public const int BaseDuration = 1;

    /// <summary>Maximum number of times incorporeal form can be extended.</summary>
    public const int MaxExtensions = 2;

    /// <summary>Additional Essence cost per extension.</summary>
    public const int ExtensionEssenceCost = 15;

    // ─────────────────────────────────────────────────────────────────────────
    // Properties
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Whether the incorporeal form is currently active.
    /// </summary>
    public bool IsActive { get; private init; }

    /// <summary>
    /// Number of turns remaining in incorporeal form.
    /// </summary>
    public int RemainingTurns { get; private init; }

    /// <summary>
    /// Number of times the form has been extended beyond the base duration.
    /// </summary>
    public int ExtensionCount { get; private init; }

    /// <summary>
    /// Total corruption accrued during this incorporeal session.
    /// </summary>
    public int CorruptionAccrued { get; private init; }

    /// <summary>
    /// When the incorporeal form was entered.
    /// </summary>
    public DateTime EnteredAt { get; private init; }

    /// <summary>
    /// Whether the form can be extended further.
    /// </summary>
    public bool CanExtend => IsActive && ExtensionCount < MaxExtensions;

    // ─────────────────────────────────────────────────────────────────────────
    // Factory Methods
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new active incorporeal state with the base duration.
    /// </summary>
    /// <returns>A new active incorporeal state lasting 1 turn.</returns>
    public static IncorporealState Create() => new()
    {
        IsActive = true,
        RemainingTurns = BaseDuration,
        ExtensionCount = 0,
        CorruptionAccrued = 0,
        EnteredAt = DateTime.UtcNow
    };

    /// <summary>
    /// Creates an inactive (ended) incorporeal state.
    /// </summary>
    /// <returns>An inactive incorporeal state.</returns>
    public static IncorporealState CreateInactive() => new()
    {
        IsActive = false,
        RemainingTurns = 0,
        ExtensionCount = 0,
        CorruptionAccrued = 0
    };

    // ─────────────────────────────────────────────────────────────────────────
    // State Transitions
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Extends the incorporeal form by one additional turn, accruing corruption.
    /// </summary>
    /// <param name="corruptionGain">Amount of corruption gained from the extension.</param>
    /// <returns>A new state with extended duration and accumulated corruption.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the form is not active or max extensions have been reached.
    /// </exception>
    public IncorporealState Extend(int corruptionGain)
    {
        if (!IsActive)
            throw new InvalidOperationException("Cannot extend an inactive incorporeal state.");

        if (ExtensionCount >= MaxExtensions)
            throw new InvalidOperationException(
                $"Cannot extend beyond {MaxExtensions} extensions.");

        ArgumentOutOfRangeException.ThrowIfNegative(corruptionGain);

        return this with
        {
            RemainingTurns = RemainingTurns + 1,
            ExtensionCount = ExtensionCount + 1,
            CorruptionAccrued = CorruptionAccrued + corruptionGain
        };
    }

    /// <summary>
    /// Decrements the remaining turns by one. If turns reach zero, the form ends.
    /// </summary>
    /// <returns>A new state with decremented turns, or an inactive state if expired.</returns>
    public IncorporealState TickDown()
    {
        if (!IsActive)
            return this;

        var newTurns = RemainingTurns - 1;

        if (newTurns <= 0)
            return End();

        return this with { RemainingTurns = newTurns };
    }

    /// <summary>
    /// Immediately ends the incorporeal form.
    /// </summary>
    /// <returns>A new inactive state preserving corruption and extension data.</returns>
    public IncorporealState End() => this with
    {
        IsActive = false,
        RemainingTurns = 0
    };

    // ─────────────────────────────────────────────────────────────────────────
    // Display
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns a diagnostic representation of the incorporeal state.
    /// </summary>
    public override string ToString() =>
        $"IncorporealState(Active={IsActive}, Turns={RemainingTurns}, " +
        $"Extensions={ExtensionCount}/{MaxExtensions}, Corruption={CorruptionAccrued})";
}
