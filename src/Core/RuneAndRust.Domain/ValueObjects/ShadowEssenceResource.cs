// ═══════════════════════════════════════════════════════════════════════════════
// ShadowEssenceResource.cs
// Immutable value object representing the Shadow Essence resource pool for the
// Myrk-gengr specialization. Tracks current/max essence, generation from
// darkness, and spending for abilities.
// Version: 0.20.4a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the Shadow Essence resource for the Myrk-gengr specialization.
/// </summary>
/// <remarks>
/// <para>
/// Shadow Essence is the Myrk-gengr's special resource, generated through
/// proximity to darkness and spent to fuel shadow abilities. The resource
/// has a maximum capacity of 50 by default and starts fully charged.
/// </para>
/// <para>
/// Essence generation rates by light level:
/// </para>
/// <list type="bullet">
///   <item><description><b>Darkness:</b> +5 per turn (base generation rate)</description></item>
///   <item><description><b>DimLight:</b> +3 per turn</description></item>
///   <item><description><b>NormalLight:</b> +0 per turn</description></item>
///   <item><description><b>BrightLight:</b> +0 per turn</description></item>
///   <item><description><b>Sunlight:</b> +0 per turn</description></item>
/// </list>
/// <para>
/// This value object is immutable. All mutation methods return new instances,
/// preserving the original for audit trails and rollback support.
/// </para>
/// <example>
/// <code>
/// var resource = ShadowEssenceResource.CreateDefault();
/// // resource.CurrentEssence = 50, resource.MaxEssence = 50
///
/// var (success, remaining) = resource.TrySpend(10);
/// // success = true, remaining.CurrentEssence = 40
///
/// var afterDarkness = remaining.GenerateFromDarkness(LightLevelType.Darkness);
/// // afterDarkness.CurrentEssence = 45 (base generation: +5 in Darkness)
/// </code>
/// </example>
/// </remarks>
/// <seealso cref="LightLevelType"/>
/// <seealso cref="MyrkgengrAbilityId"/>
public sealed record ShadowEssenceResource
{
    // ─────────────────────────────────────────────────────────────────────────
    // Properties
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Current amount of Shadow Essence available.
    /// </summary>
    public int CurrentEssence { get; private init; }

    /// <summary>
    /// Maximum Shadow Essence capacity.
    /// </summary>
    public int MaxEssence { get; private init; } = 50;

    /// <summary>
    /// Base generation rate per turn when in Darkness.
    /// </summary>
    public int GenerationRate { get; private init; } = 5;

    /// <summary>
    /// Timestamp of the last essence generation event, if any.
    /// </summary>
    public DateTime? LastGeneratedAt { get; private init; }

    /// <summary>
    /// Running count of darkness-based generation events (for diagnostics).
    /// </summary>
    public int DarknessGenerationCount { get; private init; }

    /// <summary>
    /// Whether any essence is available.
    /// </summary>
    public bool HasEssence => CurrentEssence > 0;

    /// <summary>
    /// Whether the resource is at maximum capacity.
    /// </summary>
    public bool IsFullyCharged => CurrentEssence >= MaxEssence;

    // ─────────────────────────────────────────────────────────────────────────
    // Factory Methods
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a default ShadowEssenceResource at full capacity (50/50).
    /// </summary>
    /// <returns>A new resource initialized to default values.</returns>
    public static ShadowEssenceResource CreateDefault() => new()
    {
        CurrentEssence = 50,
        MaxEssence = 50,
        GenerationRate = 5,
        DarknessGenerationCount = 0
    };

    /// <summary>
    /// Creates a ShadowEssenceResource with custom max essence at full capacity.
    /// </summary>
    /// <param name="maxEssence">Maximum essence capacity. Must be positive.</param>
    /// <returns>A new resource initialized to the specified max.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxEssence"/> is not positive.</exception>
    public static ShadowEssenceResource Create(int maxEssence)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxEssence, 1);

        return new ShadowEssenceResource
        {
            CurrentEssence = maxEssence,
            MaxEssence = maxEssence,
            GenerationRate = 5,
            DarknessGenerationCount = 0
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Spending
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Attempts to spend the specified amount of Shadow Essence.
    /// </summary>
    /// <param name="amount">Number of essence points to spend. Must be positive.</param>
    /// <returns>
    /// A tuple of (success, newResource). If successful, returns the updated resource.
    /// If insufficient essence, returns (false, this) unchanged.
    /// </returns>
    public (bool Success, ShadowEssenceResource Resource) TrySpend(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(amount, 1);

        if (CurrentEssence < amount)
            return (false, this);

        return (true, this with { CurrentEssence = CurrentEssence - amount });
    }

    /// <summary>
    /// Determines if spending the specified amount would succeed.
    /// </summary>
    /// <param name="amount">Number of essence points to check. Must be non-negative.</param>
    /// <returns><c>true</c> if sufficient essence is available; otherwise, <c>false</c>.</returns>
    public bool CanSpend(int amount) => amount >= 0 && CurrentEssence >= amount;

    // ─────────────────────────────────────────────────────────────────────────
    // Generation
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Generates a specific amount of Shadow Essence, capped at <see cref="MaxEssence"/>.
    /// </summary>
    /// <param name="amount">Number of essence points to generate. Must be positive.</param>
    /// <returns>A new resource with generated essence.</returns>
    public ShadowEssenceResource Generate(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(amount, 1);

        var newEssence = Math.Min(CurrentEssence + amount, MaxEssence);
        return this with
        {
            CurrentEssence = newEssence,
            LastGeneratedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Generates Shadow Essence based on the current light level.
    /// </summary>
    /// <remarks>
    /// <para>Generation amounts by light level:</para>
    /// <list type="bullet">
    ///   <item><description><b>Darkness:</b> +5 (full generation rate)</description></item>
    ///   <item><description><b>DimLight:</b> +3</description></item>
    ///   <item><description><b>NormalLight/BrightLight/Sunlight:</b> +0</description></item>
    /// </list>
    /// </remarks>
    /// <param name="lightLevel">Current light level at character position.</param>
    /// <returns>A new resource with generated essence (if applicable).</returns>
    public ShadowEssenceResource GenerateFromDarkness(LightLevelType lightLevel)
    {
        var generationAmount = lightLevel switch
        {
            LightLevelType.Darkness => GenerationRate,    // +5
            LightLevelType.DimLight => 3,                 // +3
            _ => 0                                        // No generation in bright environments
        };

        if (generationAmount == 0)
            return this;

        var newEssence = Math.Min(CurrentEssence + generationAmount, MaxEssence);
        return this with
        {
            CurrentEssence = newEssence,
            LastGeneratedAt = DateTime.UtcNow,
            DarknessGenerationCount = DarknessGenerationCount + 1
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Restoration
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Fully restores essence to <see cref="MaxEssence"/>.
    /// Typically called after long rest completion.
    /// </summary>
    /// <returns>A new resource at full capacity.</returns>
    public ShadowEssenceResource RestoreAll() => this with
    {
        CurrentEssence = MaxEssence,
        LastGeneratedAt = DateTime.UtcNow
    };

    /// <summary>
    /// Restores the specified number of essence points, capped at <see cref="MaxEssence"/>.
    /// Typically used for short rest partial restoration.
    /// </summary>
    /// <param name="amount">Number of essence points to restore. Must be positive.</param>
    /// <returns>A new resource with essence restored.</returns>
    public ShadowEssenceResource Restore(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(amount, 1);
        return this with
        {
            CurrentEssence = Math.Min(CurrentEssence + amount, MaxEssence),
            LastGeneratedAt = DateTime.UtcNow
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Query Helpers
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Determines if the given light level qualifies as "shadow" (Darkness or DimLight).
    /// </summary>
    /// <param name="lightLevel">Light level to check.</param>
    /// <returns><c>true</c> if the light level is Darkness or DimLight.</returns>
    public static bool IsInShadow(LightLevelType lightLevel) =>
        lightLevel <= LightLevelType.DimLight;

    // ─────────────────────────────────────────────────────────────────────────
    // Display
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Gets the current Shadow Essence value formatted as a display string.
    /// </summary>
    /// <returns>Formatted string (e.g., "40/50").</returns>
    public string GetFormattedValue() => $"{CurrentEssence}/{MaxEssence}";

    /// <summary>
    /// Calculates percentage of maximum Shadow Essence.
    /// </summary>
    /// <returns>Integer percentage (0–100).</returns>
    public int GetPercentage() => MaxEssence > 0
        ? (int)((double)CurrentEssence / MaxEssence * 100)
        : 0;

    /// <summary>
    /// Returns a human-readable representation of the essence state.
    /// </summary>
    public override string ToString() =>
        $"ShadowEssence({CurrentEssence}/{MaxEssence}, Gen={GenerationRate}/turn)";
}
