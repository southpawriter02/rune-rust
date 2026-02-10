// ═══════════════════════════════════════════════════════════════════════════════
// IDamageService.cs
// Interface for damage pipeline integration with protective Skjaldmær abilities.
// Version: 0.20.1c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service for applying damage with all protective abilities integrated.
/// Handles damage reduction (Unbreakable) and lethal protection (The Wall Lives).
/// </summary>
/// <remarks>
/// <para>
/// The damage pipeline applies reductions in order:
/// 1. Unbreakable (-3, minimum 1 damage)
/// 2. The Wall Lives lethal protection (cap damage to leave ≥1 HP)
/// </para>
/// </remarks>
public interface IDamageService
{
    /// <summary>
    /// Calculates final damage after all reductions and protections.
    /// </summary>
    /// <param name="unlockedAbilities">Character's unlocked abilities (for Unbreakable check).</param>
    /// <param name="wallState">Active The Wall Lives state, or null if not active.</param>
    /// <param name="currentHp">Character's current HP before damage.</param>
    /// <param name="incomingDamage">Raw damage before reductions.</param>
    /// <returns>Final damage to apply after all reductions.</returns>
    int CalculateFinalDamage(
        IReadOnlyList<SkjaldmaerAbilityId> unlockedAbilities,
        TheWallLivesState? wallState,
        int currentHp,
        int incomingDamage);

    /// <summary>
    /// Checks if damage would be lethal and prevents if protected by The Wall Lives.
    /// </summary>
    /// <param name="wallState">Active The Wall Lives state, or null.</param>
    /// <param name="currentHp">Character's current HP.</param>
    /// <param name="damageAmount">Proposed damage amount.</param>
    /// <returns>Modified damage (capped at safe level if protected, unchanged otherwise).</returns>
    int CheckLethalDamageProtection(TheWallLivesState? wallState, int currentHp, int damageAmount);
}
