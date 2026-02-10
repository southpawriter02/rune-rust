// ═══════════════════════════════════════════════════════════════════════════════
// BlockChargeResource.cs
// Immutable value object representing the Block Charge resource for the
// Skjaldmær specialization. Manages charge spending, restoration, and
// Bulwark HP bonus calculations.
// Version: 0.20.1a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the Block Charge resource for the Skjaldmær specialization.
/// </summary>
/// <remarks>
/// <para>
/// Block Charges are a defensive resource that powers Skjaldmær abilities.
/// The resource has a maximum of 3 charges by default, starts fully charged,
/// and restores to maximum on rest completion.
/// </para>
/// <para>
/// This is an immutable value object — all mutation methods return new instances.
/// The <see cref="GetBulwarkHpBonus"/> method calculates the Max HP bonus granted
/// by the Bulwark passive ability (+5 HP per charge held).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var charges = BlockChargeResource.CreateDefault();
/// var (success, remaining) = charges.TrySpend(1);
/// // success = true, remaining.CurrentCharges = 2
/// int bonus = remaining.GetBulwarkHpBonus(); // 10
/// </code>
/// </example>
public sealed record BlockChargeResource
{
    /// <summary>
    /// Default maximum number of Block Charges.
    /// </summary>
    public const int DefaultMaxCharges = 3;

    /// <summary>
    /// HP bonus per Block Charge held for the Bulwark passive.
    /// </summary>
    public const int BulwarkHpPerCharge = 5;

    /// <summary>
    /// Gets the current number of Block Charges available.
    /// </summary>
    public int CurrentCharges { get; init; }

    /// <summary>
    /// Gets the maximum number of Block Charges this resource can hold.
    /// </summary>
    public int MaxCharges { get; init; }

    /// <summary>
    /// Gets the timestamp of the last restoration event, if any.
    /// </summary>
    public DateTime? LastRestoredAt { get; init; }

    /// <summary>
    /// Gets whether any charges are available to spend.
    /// </summary>
    public bool HasCharges => CurrentCharges > 0;

    /// <summary>
    /// Gets whether charges are at maximum capacity.
    /// </summary>
    public bool IsFullyCharged => CurrentCharges >= MaxCharges;

    /// <summary>
    /// Creates a default BlockChargeResource at full capacity (3/3).
    /// </summary>
    /// <returns>A new resource initialized to default values.</returns>
    public static BlockChargeResource CreateDefault() => new()
    {
        CurrentCharges = DefaultMaxCharges,
        MaxCharges = DefaultMaxCharges,
        LastRestoredAt = null
    };

    /// <summary>
    /// Creates a BlockChargeResource with custom max charges at full capacity.
    /// </summary>
    /// <param name="maxCharges">Maximum charge capacity. Must be positive.</param>
    /// <returns>A new resource initialized to the specified max.</returns>
    public static BlockChargeResource Create(int maxCharges)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxCharges, 1);
        return new BlockChargeResource
        {
            CurrentCharges = maxCharges,
            MaxCharges = maxCharges,
            LastRestoredAt = null
        };
    }

    /// <summary>
    /// Attempts to spend the specified number of charges.
    /// </summary>
    /// <param name="amount">Number of charges to spend. Must be positive.</param>
    /// <returns>
    /// A tuple of (success, newResource). If successful, returns the updated resource.
    /// If insufficient charges, returns (false, this) unchanged.
    /// </returns>
    public (bool Success, BlockChargeResource Resource) TrySpend(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

        if (CurrentCharges < amount)
            return (false, this);

        return (true, this with { CurrentCharges = CurrentCharges - amount });
    }

    /// <summary>
    /// Restores the specified number of charges, capped at <see cref="MaxCharges"/>.
    /// </summary>
    /// <param name="amount">Number of charges to restore. Must be positive.</param>
    /// <returns>A new resource with charges restored.</returns>
    public BlockChargeResource Restore(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

        var newCharges = Math.Min(CurrentCharges + amount, MaxCharges);
        return this with
        {
            CurrentCharges = newCharges,
            LastRestoredAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Fully restores charges to <see cref="MaxCharges"/>.
    /// Typically called after rest completion.
    /// </summary>
    /// <returns>A new resource at full capacity.</returns>
    public BlockChargeResource RestoreAll() => this with
    {
        CurrentCharges = MaxCharges,
        LastRestoredAt = DateTime.UtcNow
    };

    /// <summary>
    /// Calculates the Bulwark passive Max HP bonus based on charges held.
    /// </summary>
    /// <returns>HP bonus: <see cref="CurrentCharges"/> × 5. Maximum +15 HP at 3 charges.</returns>
    public int GetBulwarkHpBonus() => CurrentCharges * BulwarkHpPerCharge;

    /// <summary>
    /// Returns a human-readable representation of the charge state.
    /// </summary>
    public override string ToString() => $"Block Charges: {CurrentCharges}/{MaxCharges}";
}
