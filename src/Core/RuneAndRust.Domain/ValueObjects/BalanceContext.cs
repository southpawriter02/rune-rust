namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Captures all contextual factors affecting a balance check.
/// </summary>
/// <remarks>
/// <para>
/// BalanceContext combines the surface properties with environmental and
/// character-specific modifiers to calculate the final DC. It also tracks
/// whether this is part of a longer traverse requiring multiple checks.
/// </para>
/// <para>
/// <b>DC Calculation:</b>
/// <list type="bullet">
///   <item><description>Surface DC (width + stability + condition)</description></item>
///   <item><description>+ Wind modifier (0-2)</description></item>
///   <item><description>+ Encumbrance modifier (0-2)</description></item>
///   <item><description>- Balance pole (-1)</description></item>
/// </list>
/// </para>
/// <para>
/// <b>v0.15.2f:</b> Initial implementation of balance context.
/// </para>
/// </remarks>
/// <param name="Surface">The surface being balanced upon.</param>
/// <param name="WindModifier">DC modifier from wind conditions (+0 to +2).</param>
/// <param name="EncumbranceModifier">DC modifier from character load (+0 to +2).</param>
/// <param name="HasBalancePole">Whether character has a balance pole (-1 DC).</param>
/// <param name="IsLongTraverse">Whether this requires multiple checks.</param>
/// <param name="CheckNumber">Current check number if IsLongTraverse (1-based).</param>
/// <param name="TotalChecksRequired">Total checks needed if IsLongTraverse.</param>
public readonly record struct BalanceContext(
    BalanceSurface Surface,
    int WindModifier = 0,
    int EncumbranceModifier = 0,
    bool HasBalancePole = false,
    bool IsLongTraverse = false,
    int CheckNumber = 1,
    int TotalChecksRequired = 1)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Minimum possible DC for balance checks.
    /// </summary>
    public const int MinimumDc = 1;

    /// <summary>
    /// Balance pole DC reduction.
    /// </summary>
    public const int BalancePoleDcReduction = 1;

    // ═══════════════════════════════════════════════════════════════════════════
    // DC CALCULATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the total DC for the balance check.
    /// </summary>
    public int FinalDc
    {
        get
        {
            var dc = Surface.SurfaceDc + WindModifier + EncumbranceModifier;

            // Balance pole reduces DC by 1
            if (HasBalancePole)
            {
                dc -= BalancePoleDcReduction;
            }

            // Minimum DC is 1
            return Math.Max(MinimumDc, dc);
        }
    }

    /// <summary>
    /// Gets the total modifier applied to the surface DC.
    /// </summary>
    public int TotalModifier => WindModifier + EncumbranceModifier - (HasBalancePole ? BalancePoleDcReduction : 0);

    // ═══════════════════════════════════════════════════════════════════════════
    // QUERY PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the fall height if balance is lost.
    /// </summary>
    public int FallHeight => Surface.HeightAboveGround;

    /// <summary>
    /// Indicates whether falling would cause damage.
    /// </summary>
    public bool FallIsDangerous => Surface.FallCausesDamage;

    /// <summary>
    /// Calculates progress percentage for long traverses.
    /// </summary>
    public double TraverseProgress => IsLongTraverse
        ? (double)(CheckNumber - 1) / TotalChecksRequired * 100
        : 0;

    /// <summary>
    /// Indicates if this is the final check in a long traverse.
    /// </summary>
    public bool IsFinalCheck => !IsLongTraverse || CheckNumber >= TotalChecksRequired;

    /// <summary>
    /// Indicates if there are modifiers applied.
    /// </summary>
    public bool HasModifiers => WindModifier != 0 || EncumbranceModifier != 0 || HasBalancePole;

    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a display string for the balance context.
    /// </summary>
    /// <returns>Formatted context description for UI display.</returns>
    public string ToDisplayString()
    {
        var modifiers = new List<string>();

        if (Surface.StabilityModifier > 0)
        {
            modifiers.Add($"{Surface.StabilityDescription} (+{Surface.StabilityModifier} DC)");
        }

        if (Surface.ConditionModifier > 0)
        {
            modifiers.Add($"{Surface.Condition.ToString().ToLowerInvariant()} (+{Surface.ConditionModifier} DC)");
        }

        if (WindModifier > 0)
        {
            modifiers.Add($"Wind (+{WindModifier} DC)");
        }

        if (EncumbranceModifier > 0)
        {
            modifiers.Add($"Encumbered (+{EncumbranceModifier} DC)");
        }

        if (HasBalancePole)
        {
            modifiers.Add("Balance Pole (-1 DC)");
        }

        var modString = modifiers.Count > 0
            ? $"\nModifiers: {string.Join(", ", modifiers)}"
            : "";

        var traverseInfo = IsLongTraverse
            ? $"\nProgress: Check {CheckNumber} of {TotalChecksRequired}"
            : "";

        return $"DC {FinalDc} ({Surface.WidthDescription}){modString}{traverseInfo}";
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a simple context with just surface and no modifiers.
    /// </summary>
    /// <param name="surface">The surface to balance on.</param>
    /// <returns>A new balance context.</returns>
    public static BalanceContext ForSurface(BalanceSurface surface)
    {
        return new BalanceContext(surface);
    }

    /// <summary>
    /// Creates context with wind modifier.
    /// </summary>
    /// <param name="surface">The surface to balance on.</param>
    /// <param name="windModifier">Wind DC modifier.</param>
    /// <returns>A new balance context with wind.</returns>
    public static BalanceContext WithWind(BalanceSurface surface, int windModifier)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(windModifier, nameof(windModifier));
        return new BalanceContext(surface, WindModifier: windModifier);
    }

    /// <summary>
    /// Creates context for a long traverse requiring multiple checks.
    /// </summary>
    /// <param name="surface">The surface to balance on.</param>
    /// <param name="totalChecks">Total number of checks required.</param>
    /// <param name="currentCheck">Current check number (1-based).</param>
    /// <returns>A new balance context for long traverse.</returns>
    public static BalanceContext LongTraverse(BalanceSurface surface, int totalChecks, int currentCheck = 1)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(totalChecks, 2, nameof(totalChecks));
        ArgumentOutOfRangeException.ThrowIfLessThan(currentCheck, 1, nameof(currentCheck));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(currentCheck, totalChecks, nameof(currentCheck));

        return new BalanceContext(
            surface,
            IsLongTraverse: true,
            CheckNumber: currentCheck,
            TotalChecksRequired: totalChecks);
    }
}
