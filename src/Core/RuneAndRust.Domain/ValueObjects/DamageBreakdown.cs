// ═══════════════════════════════════════════════════════════════════════════════
// DamageBreakdown.cs
// Immutable value object decomposing damage into typed components for
// Umbral Strike and other multi-damage-type abilities.
// Version: 0.20.4b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Decomposes damage into physical and shadow components for abilities
/// that deal multiple damage types.
/// </summary>
/// <remarks>
/// <para>
/// Used by Umbral Strike to separate weapon damage (physical) from
/// shadow damage (2d6 base, +1d6 on critical hit). Shadow damage
/// bypasses non-magical armor and is effective against Light-wielders.
/// </para>
/// <example>
/// <code>
/// var breakdown = DamageBreakdown.Create(9, 7, 0, "shadow");
/// // breakdown.GetTotal() == 16
/// // breakdown.PhysicalDamage == 9, breakdown.ShadowDamage == 7
/// </code>
/// </example>
/// </remarks>
public sealed record DamageBreakdown
{
    // ─────────────────────────────────────────────────────────────────────────
    // Properties
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Physical weapon damage component.</summary>
    public int PhysicalDamage { get; private init; }

    /// <summary>Shadow damage component (2d6 base, +1d6 on crit).</summary>
    public int ShadowDamage { get; private init; }

    /// <summary>Bonus damage from clone consumption or other modifiers.</summary>
    public int BonusDamage { get; private init; }

    /// <summary>Damage type identifier for the shadow component.</summary>
    public string DamageTypeId { get; private init; } = "shadow";

    // ─────────────────────────────────────────────────────────────────────────
    // Factory
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new damage breakdown.
    /// </summary>
    /// <param name="physicalDamage">Physical weapon damage. Must be non-negative.</param>
    /// <param name="shadowDamage">Shadow damage. Must be non-negative.</param>
    /// <param name="bonusDamage">Bonus damage from modifiers. Must be non-negative.</param>
    /// <param name="damageTypeId">Damage type identifier. Defaults to "shadow".</param>
    /// <returns>A new DamageBreakdown.</returns>
    public static DamageBreakdown Create(
        int physicalDamage,
        int shadowDamage,
        int bonusDamage = 0,
        string damageTypeId = "shadow")
    {
        ArgumentOutOfRangeException.ThrowIfNegative(physicalDamage);
        ArgumentOutOfRangeException.ThrowIfNegative(shadowDamage);
        ArgumentOutOfRangeException.ThrowIfNegative(bonusDamage);

        return new DamageBreakdown
        {
            PhysicalDamage = physicalDamage,
            ShadowDamage = shadowDamage,
            BonusDamage = bonusDamage,
            DamageTypeId = damageTypeId
        };
    }

    /// <summary>
    /// Creates an empty damage breakdown (0 damage).
    /// </summary>
    public static DamageBreakdown None => new()
    {
        PhysicalDamage = 0,
        ShadowDamage = 0,
        BonusDamage = 0,
        DamageTypeId = "shadow"
    };

    // ─────────────────────────────────────────────────────────────────────────
    // Query
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Calculates total damage across all components.
    /// </summary>
    public int GetTotal() => PhysicalDamage + ShadowDamage + BonusDamage;

    // ─────────────────────────────────────────────────────────────────────────
    // Display
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns a diagnostic representation of this breakdown.
    /// </summary>
    public override string ToString() =>
        $"Damage({PhysicalDamage} phys + {ShadowDamage} shadow + {BonusDamage} bonus = {GetTotal()})";
}
