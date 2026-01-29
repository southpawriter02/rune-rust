// =============================================================================
// CombatArmorState.cs
// =============================================================================
// v0.16.2f - Combat Integration
// =============================================================================
// Value object representing the complete combat state derived from equipped
// armor and the character's proficiency level. Encapsulates all effective
// penalties, Galdr interference, and display warnings for UI consumption.
// =============================================================================

using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the complete combat state derived from equipped armor and proficiency.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="CombatArmorState"/> aggregates all armor-related combat modifiers into a single
/// immutable snapshot. This includes:
/// </para>
/// <list type="bullet">
/// <item><description>Effective armor penalties from proficiency calculation</description></item>
/// <item><description>Galdr/WITS interference (blocking or penalty)</description></item>
/// <item><description>Stealth disadvantage status</description></item>
/// <item><description>Display warnings for UI feedback</description></item>
/// </list>
/// <para>
/// This value object is produced by <c>IArmorCombatModifierService.GetCombatState()</c>
/// and consumed by combat services and UI components.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var state = combatModifierService.GetCombatState("mystic", ArmorCategory.Heavy);
/// 
/// if (state.GaldrBlocked)
/// {
///     Console.WriteLine("Cannot cast Galdr in heavy armor!");
/// }
/// else if (state.GaldrPenalty != 0)
/// {
///     Console.WriteLine($"Galdr casting penalty: {state.GaldrPenalty}");
/// }
/// 
/// // Display warnings to player
/// foreach (var warning in state.DisplayWarnings)
/// {
///     Console.WriteLine($"⚠ {warning}");
/// }
/// </code>
/// </example>
/// <seealso cref="EffectiveArmorPenalties"/>
/// <seealso cref="ArmorCategory"/>
public readonly record struct CombatArmorState(
    EffectiveArmorPenalties EffectivePenalties,
    bool GaldrBlocked,
    int GaldrPenalty,
    bool StealthDisadvantage,
    IReadOnlyList<string> DisplayWarnings)
{
    // =========================================================================
    // Computed Properties
    // =========================================================================

    /// <summary>
    /// Gets whether any penalty applies (armor, Galdr, or stealth).
    /// </summary>
    /// <value>
    /// <c>true</c> if any penalty is active; <c>false</c> if wearing armor imposes no penalties.
    /// </value>
    public bool HasAnyPenalty =>
        EffectivePenalties.HasAnyPenalty ||
        GaldrPenalty != 0 ||
        GaldrBlocked ||
        StealthDisadvantage;

    /// <summary>
    /// Gets whether any bonus applies (from high proficiency).
    /// </summary>
    /// <value>
    /// <c>true</c> if the character has armor-based bonuses (e.g., Master defense bonus).
    /// </value>
    public bool HasAnyBonus => EffectivePenalties.HasDefenseBonus;

    /// <summary>
    /// Gets whether Galdr casting is possible (not blocked).
    /// </summary>
    /// <value>
    /// <c>true</c> if Galdr can be cast (possibly with penalty); <c>false</c> if blocked.
    /// </value>
    public bool CanCastGaldr => !GaldrBlocked;

    /// <summary>
    /// Gets the attack modifier from armor penalties/bonuses.
    /// </summary>
    public int AttackModifier => EffectivePenalties.AttackModifier;

    /// <summary>
    /// Gets the defense modifier from armor penalties/bonuses.
    /// </summary>
    public int DefenseModifier => EffectivePenalties.DefenseModifier;

    // =========================================================================
    // Static Properties
    // =========================================================================

    /// <summary>
    /// Gets the default state representing no armor or no penalties.
    /// </summary>
    /// <remarks>
    /// Use this for characters without equipped armor or when armor has no effect.
    /// </remarks>
    public static CombatArmorState None => new(
        EffectivePenalties: EffectiveArmorPenalties.None,
        GaldrBlocked: false,
        GaldrPenalty: 0,
        StealthDisadvantage: false,
        DisplayWarnings: Array.Empty<string>());

    // =========================================================================
    // Factory Methods
    // =========================================================================

    /// <summary>
    /// Creates a new <see cref="CombatArmorState"/> with all combat modifiers.
    /// </summary>
    /// <param name="penalties">The calculated effective armor penalties.</param>
    /// <param name="galdrBlocked">Whether Galdr casting is blocked.</param>
    /// <param name="galdrPenalty">The Galdr/WITS penalty (ignored if blocked).</param>
    /// <param name="stealthDisadvantage">Whether stealth has disadvantage.</param>
    /// <param name="warnings">Display warnings for UI.</param>
    /// <returns>A new <see cref="CombatArmorState"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="warnings"/> is null.</exception>
    /// <example>
    /// <code>
    /// var state = CombatArmorState.Create(
    ///     penalties: effectivePenalties,
    ///     galdrBlocked: true,
    ///     galdrPenalty: 0,
    ///     stealthDisadvantage: true,
    ///     warnings: new[] { "Galdr blocked by heavy armor" });
    /// </code>
    /// </example>
    public static CombatArmorState Create(
        EffectiveArmorPenalties penalties,
        bool galdrBlocked,
        int galdrPenalty,
        bool stealthDisadvantage,
        IReadOnlyList<string> warnings)
    {
        ArgumentNullException.ThrowIfNull(warnings);

        return new CombatArmorState(
            EffectivePenalties: penalties,
            GaldrBlocked: galdrBlocked,
            GaldrPenalty: galdrBlocked ? 0 : galdrPenalty, // Clear penalty if blocked
            StealthDisadvantage: stealthDisadvantage,
            DisplayWarnings: warnings);
    }

    // =========================================================================
    // Formatting Methods
    // =========================================================================

    /// <summary>
    /// Formats the state for combat log display.
    /// </summary>
    /// <returns>
    /// A concise string representation suitable for combat logs:
    /// "Galdr BLOCKED", "Galdr -2", or the penalty summary.
    /// </returns>
    /// <example>
    /// <code>
    /// var state = combatModifierService.GetCombatState("mystic", ArmorCategory.Heavy);
    /// Console.WriteLine($"[Combat] Armor: {state.FormatForCombatLog()}");
    /// // Output: "[Combat] Armor: Galdr BLOCKED"
    /// </code>
    /// </example>
    public string FormatForCombatLog()
    {
        if (GaldrBlocked)
            return "Galdr BLOCKED";

        if (GaldrPenalty != 0)
            return $"Galdr {GaldrPenalty:+0;-0;0}";

        if (!HasAnyPenalty && !HasAnyBonus)
            return "None";

        return EffectivePenalties.FormatSummary();
    }

    /// <summary>
    /// Formats a detailed multi-line summary for UI tooltips.
    /// </summary>
    /// <returns>A detailed breakdown of all combat modifiers.</returns>
    public string FormatDetailedSummary()
    {
        var lines = new List<string>();

        if (GaldrBlocked)
        {
            lines.Add("⛔ Galdr casting BLOCKED");
        }
        else if (GaldrPenalty != 0)
        {
            lines.Add($"🔮 Galdr penalty: {GaldrPenalty:+0;-0;0}");
        }

        if (AttackModifier != 0)
        {
            lines.Add($"⚔️ Attack: {AttackModifier:+0;-0;0}");
        }

        if (DefenseModifier != 0)
        {
            lines.Add($"🛡️ Defense: {DefenseModifier:+0;-0;0}");
        }

        if (StealthDisadvantage)
        {
            lines.Add("👁️ Stealth disadvantage");
        }

        if (EffectivePenalties.EffectivePenalties.MovementPenalty != 0)
        {
            lines.Add($"🏃 Movement: {EffectivePenalties.EffectivePenalties.MovementPenalty:+0;-0;0}");
        }

        if (lines.Count == 0)
        {
            lines.Add("✓ No armor penalties");
        }

        return string.Join("\n", lines);
    }

    /// <summary>
    /// Returns a string representation for debugging.
    /// </summary>
    /// <returns>A debug-friendly string representation.</returns>
    public override string ToString()
    {
        return $"CombatArmorState(GaldrBlocked={GaldrBlocked}, GaldrPenalty={GaldrPenalty}, " +
               $"Attack={AttackModifier}, Defense={DefenseModifier}, Stealth={StealthDisadvantage})";
    }
}
