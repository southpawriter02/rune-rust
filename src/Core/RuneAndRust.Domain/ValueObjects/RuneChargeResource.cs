namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the special Rune Charge resource for the Rúnasmiðr specialization.
/// Rune Charges power inscription abilities and ward creation.
/// </summary>
/// <remarks>
/// <para>Rune Charges are a renewable resource (max 5) that fuel the Rúnasmiðr's
/// core abilities:</para>
/// <list type="bullet">
/// <item>Inscribe Rune (1 charge) — enhance weapon or armor</item>
/// <item>Runestone Ward (1 charge) — create damage-absorbing ward</item>
/// <item>Empowered Inscription (2 charges) — add elemental damage (Tier 2)</item>
/// <item>Runic Trap (2 charges) — create triggered trap (Tier 2)</item>
/// <item>Living Runes (3 charges) — animate rune constructs (Tier 3)</item>
/// <item>Word of Unmaking (4 charges) — dispel all magic (Capstone)</item>
/// </list>
/// <para>Charges are generated through crafting activities (1 per standard craft,
/// 2 per complex craft) and restored fully during rest. This creates a gameplay
/// loop linking the Rúnasmiðr's crafting identity to combat readiness.</para>
/// <para>Follows the same sealed record pattern as <see cref="BlockChargeResource"/>
/// for consistency across specialization resource systems.</para>
/// </remarks>
public sealed record RuneChargeResource
{
    /// <summary>
    /// Default maximum number of Rune Charges.
    /// </summary>
    public const int DefaultMaxCharges = 5;

    /// <summary>
    /// Standard charges generated per crafting action.
    /// </summary>
    public const int StandardCraftChargeGain = 1;

    /// <summary>
    /// Charges generated per complex crafting action.
    /// </summary>
    public const int ComplexCraftChargeGain = 2;

    /// <summary>
    /// Current number of Rune Charges available (0 to <see cref="MaxCharges"/>).
    /// Represents available charges ready to fuel inscription and ward abilities.
    /// </summary>
    public int CurrentCharges { get; private set; }

    /// <summary>
    /// Maximum Rune Charges (default 5).
    /// Defines the hard limit for charge accumulation.
    /// </summary>
    public int MaxCharges { get; init; } = DefaultMaxCharges;

    /// <summary>
    /// UTC timestamp when charges were last generated or restored.
    /// Used for UI display and audit trails.
    /// </summary>
    public DateTime? LastGeneratedAt { get; private set; }

    /// <summary>
    /// Creates a new RuneChargeResource at full charges.
    /// </summary>
    /// <param name="maxCharges">Maximum charge count (default 5).</param>
    /// <returns>A new resource initialized to maximum charges.</returns>
    public static RuneChargeResource CreateFull(int maxCharges = DefaultMaxCharges)
    {
        return new RuneChargeResource
        {
            MaxCharges = maxCharges,
            CurrentCharges = maxCharges,
            LastGeneratedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Attempts to spend the specified number of Rune Charges.
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
    /// Generates Rune Charges from a crafting action.
    /// </summary>
    /// <param name="isComplexCraft">
    /// True for complex crafts (2 charges); false for standard crafts (1 charge).
    /// </param>
    /// <returns>The number of charges actually generated (may be less if near max).</returns>
    /// <remarks>
    /// Generation is capped at <see cref="MaxCharges"/>; excess is lost.
    /// Updates <see cref="LastGeneratedAt"/> timestamp.
    /// </remarks>
    public int GenerateFromCrafting(bool isComplexCraft)
    {
        var amount = isComplexCraft ? ComplexCraftChargeGain : StandardCraftChargeGain;
        var previousCharges = CurrentCharges;
        CurrentCharges = Math.Min(CurrentCharges + amount, MaxCharges);
        LastGeneratedAt = DateTime.UtcNow;
        return CurrentCharges - previousCharges;
    }

    /// <summary>
    /// Generates the specified number of Rune Charges.
    /// </summary>
    /// <param name="amount">Number of charges to generate (must be positive).</param>
    /// <remarks>
    /// Generation is capped at <see cref="MaxCharges"/>; excess is lost.
    /// Updates <see cref="LastGeneratedAt"/> timestamp.
    /// </remarks>
    public void Generate(int amount)
    {
        if (amount <= 0)
            return;

        CurrentCharges = Math.Min(CurrentCharges + amount, MaxCharges);
        LastGeneratedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Fully restores all Rune Charges to maximum.
    /// </summary>
    /// <remarks>
    /// Standard behavior when resting (short or long rest).
    /// Updates <see cref="LastGeneratedAt"/> timestamp.
    /// </remarks>
    public void RestoreAll()
    {
        CurrentCharges = MaxCharges;
        LastGeneratedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if the player can afford a specific charge cost.
    /// </summary>
    /// <param name="amount">The number of charges required.</param>
    /// <returns>True if <see cref="CurrentCharges"/> is greater than or equal to the amount.</returns>
    public bool CanAfford(int amount) => amount > 0 && CurrentCharges >= amount;

    /// <summary>
    /// Gets the current charge level as a percentage of maximum.
    /// </summary>
    /// <returns>
    /// Percentage from 0.0 to 1.0 representing charge fullness.
    /// Returns 0.0 if <see cref="MaxCharges"/> is 0.
    /// </returns>
    public double GetChargePercentage() =>
        MaxCharges > 0 ? (double)CurrentCharges / MaxCharges : 0.0;

    /// <summary>
    /// Determines if this resource has been modified from its initial full state.
    /// </summary>
    /// <returns>True if current charges differ from maximum.</returns>
    public bool IsModified() => CurrentCharges != MaxCharges;
}
