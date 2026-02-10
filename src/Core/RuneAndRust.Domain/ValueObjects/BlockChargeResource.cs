namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the special Block Charge resource for the Skjaldmær specialization.
/// Block Charges power defensive reactions and modify Max HP through the Bulwark passive.
/// </summary>
/// <remarks>
/// <para>Block Charges are a finite resource (max 3) that fuel the Skjaldmær's
/// most potent defensive abilities:</para>
/// <list type="bullet">
/// <item>Intercept (1 charge) — redirect attack from ally</item>
/// <item>Guardian's Sacrifice (2 charges) — absorb all damage from ally</item>
/// </list>
/// <para>Charges restore fully on any rest (short or long). The Bulwark passive
/// grants +5 Max HP per charge held, creating tension between spending charges
/// for abilities and holding them for passive resilience.</para>
/// </remarks>
public sealed record BlockChargeResource
{
    /// <summary>
    /// Default maximum number of Block Charges.
    /// </summary>
    public const int DefaultMaxCharges = 3;

    /// <summary>
    /// HP bonus granted per Block Charge held by the Bulwark passive ability.
    /// </summary>
    public const int BulwarkHpBonusPerCharge = 5;

    /// <summary>
    /// Current number of Block Charges available (0 to <see cref="MaxCharges"/>).
    /// Represents available defensive reactions ready to be executed.
    /// </summary>
    public int CurrentCharges { get; private set; }

    /// <summary>
    /// Maximum Block Charges (default 3).
    /// Defines the hard limit for charge accumulation.
    /// </summary>
    public int MaxCharges { get; init; } = DefaultMaxCharges;

    /// <summary>
    /// UTC timestamp when charges were last restored to maximum.
    /// Used for UI display and audit trails.
    /// </summary>
    public DateTime? LastRestoredAt { get; private set; }

    /// <summary>
    /// Creates a new BlockChargeResource at full charges.
    /// </summary>
    /// <param name="maxCharges">Maximum charge count (default 3).</param>
    /// <returns>A new resource initialized to maximum charges.</returns>
    public static BlockChargeResource CreateFull(int maxCharges = DefaultMaxCharges)
    {
        return new BlockChargeResource
        {
            MaxCharges = maxCharges,
            CurrentCharges = maxCharges,
            LastRestoredAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Attempts to spend the specified number of Block Charges.
    /// </summary>
    /// <param name="amount">Number of charges to spend (must be positive).</param>
    /// <returns>
    /// True if the specified amount was available and successfully spent;
    /// False if insufficient charges were available (no partial spend).
    /// </returns>
    /// <remarks>
    /// Spending is atomic — if not enough charges are available,
    /// <see cref="CurrentCharges"/> remains unchanged and False is returned.
    /// </remarks>
    public bool Spend(int amount)
    {
        if (amount <= 0 || CurrentCharges < amount)
            return false;

        CurrentCharges -= amount;
        return true;
    }

    /// <summary>
    /// Restores the specified number of Block Charges toward maximum.
    /// </summary>
    /// <param name="amount">Number of charges to restore (must be positive).</param>
    /// <remarks>
    /// Restoration is capped at <see cref="MaxCharges"/>; excess is lost.
    /// Updates <see cref="LastRestoredAt"/> timestamp.
    /// </remarks>
    public void Restore(int amount)
    {
        if (amount <= 0)
            return;

        CurrentCharges = Math.Min(CurrentCharges + amount, MaxCharges);
        LastRestoredAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Fully restores all Block Charges to maximum.
    /// </summary>
    /// <remarks>
    /// Standard behavior when resting (short or long rest).
    /// Updates <see cref="LastRestoredAt"/> timestamp.
    /// </remarks>
    public void RestoreAll()
    {
        CurrentCharges = MaxCharges;
        LastRestoredAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Calculates the Max HP bonus provided by the Bulwark passive ability.
    /// </summary>
    /// <returns>
    /// HP bonus equal to <see cref="CurrentCharges"/> × <see cref="BulwarkHpBonusPerCharge"/>.
    /// Range: 0 (no charges) to 15 (3 charges at default max).
    /// </returns>
    public int GetBulwarkHpBonus() => CurrentCharges * BulwarkHpBonusPerCharge;

    /// <summary>
    /// Determines if this resource has been modified from its initial full state.
    /// </summary>
    /// <returns>True if current charges differ from maximum.</returns>
    public bool IsModified() => CurrentCharges != MaxCharges;
}
