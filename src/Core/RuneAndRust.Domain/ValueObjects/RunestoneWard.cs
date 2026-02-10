// ═══════════════════════════════════════════════════════════════════════════════
// RunestoneWard.cs
// Immutable value object representing a protective ward created by the
// Rúnasmiðr specialization. Absorbs incoming damage until destroyed.
// Version: 0.20.2a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a protective ward created by the Rúnasmiðr's Runestone Ward ability.
/// </summary>
/// <remarks>
/// <para>
/// The ward absorbs incoming damage until its absorption capacity is depleted.
/// Default absorption capacity is 10 HP. Only one ward can be active per
/// character at a time; creating a new ward replaces any existing one.
/// </para>
/// <para>
/// This is an immutable value object — the <see cref="AbsorbDamage"/> method
/// returns a tuple containing the absorbed amount, remaining damage, and the
/// updated ward instance.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var ward = RunestoneWard.Create(ownerId);
/// var (absorbed, remaining, updated) = ward.AbsorbDamage(7);
/// // absorbed = 7, remaining = 0, updated.RemainingAbsorption = 3
///
/// var (absorbed2, remaining2, updated2) = updated.AbsorbDamage(5);
/// // absorbed2 = 3, remaining2 = 2, updated2.RemainingAbsorption = 0
/// // updated2.IsDestroyed == true
/// </code>
/// </example>
public sealed record RunestoneWard
{
    /// <summary>
    /// Default damage absorption capacity for a Runestone Ward.
    /// </summary>
    public const int DefaultAbsorption = 10;

    /// <summary>
    /// Default width for visual bar rendering.
    /// </summary>
    public const int DefaultBarWidth = 10;

    /// <summary>
    /// Gets the unique identifier for this ward instance.
    /// </summary>
    public Guid WardId { get; init; }

    /// <summary>
    /// Gets the ID of the character who owns this ward.
    /// </summary>
    public Guid OwnerId { get; init; }

    /// <summary>
    /// Gets the maximum damage this ward can absorb.
    /// </summary>
    public int MaxAbsorption { get; init; }

    /// <summary>
    /// Gets the remaining absorption capacity.
    /// </summary>
    public int RemainingAbsorption { get; init; }

    /// <summary>
    /// Gets the timestamp when this ward was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets whether this ward has been destroyed (remaining absorption is 0 or less).
    /// </summary>
    public bool IsDestroyed => RemainingAbsorption <= 0;

    /// <summary>
    /// Creates a new Runestone Ward with default absorption (10 HP).
    /// </summary>
    /// <param name="ownerId">ID of the character creating the ward.</param>
    /// <returns>A new ward at full absorption capacity.</returns>
    public static RunestoneWard Create(Guid ownerId) => new()
    {
        WardId = Guid.NewGuid(),
        OwnerId = ownerId,
        MaxAbsorption = DefaultAbsorption,
        RemainingAbsorption = DefaultAbsorption,
        CreatedAt = DateTime.UtcNow
    };

    /// <summary>
    /// Absorbs incoming damage, reducing the ward's remaining capacity.
    /// </summary>
    /// <param name="damage">Amount of incoming damage. Must be positive.</param>
    /// <returns>
    /// A tuple of (absorbed, remainingDamage, updatedWard):
    /// <list type="bullet">
    ///   <item><description>absorbed: amount of damage the ward absorbed</description></item>
    ///   <item><description>remainingDamage: damage that passes through to the character</description></item>
    ///   <item><description>updatedWard: the ward with reduced absorption</description></item>
    /// </list>
    /// </returns>
    public (int Absorbed, int RemainingDamage, RunestoneWard UpdatedWard) AbsorbDamage(int damage)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(damage);

        var absorbed = Math.Min(damage, RemainingAbsorption);
        var remainingDamage = damage - absorbed;
        var updatedWard = this with { RemainingAbsorption = RemainingAbsorption - absorbed };

        return (absorbed, remainingDamage, updatedWard);
    }

    /// <summary>
    /// Gets a visual bar representation of remaining ward absorption.
    /// </summary>
    /// <param name="width">Width of the bar in characters. Defaults to 10.</param>
    /// <returns>A string like "██████░░░░" showing remaining vs depleted absorption.</returns>
    public string GetVisualBar(int width = DefaultBarWidth)
    {
        if (MaxAbsorption <= 0) return new string('░', width);

        var filledCount = (int)Math.Round((double)RemainingAbsorption / MaxAbsorption * width);
        filledCount = Math.Clamp(filledCount, 0, width);
        var emptyCount = width - filledCount;

        return new string('█', filledCount) + new string('░', emptyCount);
    }

    /// <summary>
    /// Returns a human-readable representation of the ward state.
    /// </summary>
    public override string ToString() =>
        $"Runestone Ward: {RemainingAbsorption}/{MaxAbsorption} [{GetVisualBar()}]";
}
