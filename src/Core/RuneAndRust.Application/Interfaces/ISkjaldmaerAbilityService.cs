// ═══════════════════════════════════════════════════════════════════════════════
// ISkjaldmaerAbilityService.cs
// Interface for Skjaldmær-specific ability operations across all tiers.
// Version: 0.20.1c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Defines operations for Skjaldmær specialization abilities across all tiers.
/// </summary>
/// <remarks>
/// <para>
/// <b>Tier 1:</b> Shield Wall, Intercept, Bulwark (v0.20.1a)
/// </para>
/// <para>
/// <b>Tier 2:</b> Hold the Line, Counter-Shield, Rally (v0.20.1b)
/// </para>
/// <para>
/// <b>Tier 3:</b> Unbreakable, Guardian's Sacrifice (v0.20.1c)
/// </para>
/// <para>
/// <b>Capstone:</b> The Wall Lives (v0.20.1c)
/// </para>
/// </remarks>
public interface ISkjaldmaerAbilityService
{
    // ═══════ Tier 1: Foundational Defenses (v0.20.1a) ═══════

    /// <summary>
    /// Activates Shield Wall stance at the specified position.
    /// </summary>
    /// <param name="position">Grid position for stance activation.</param>
    /// <returns>The activated ShieldWallState.</returns>
    ShieldWallState ActivateShieldWall((int X, int Y) position);

    /// <summary>
    /// Deactivates an active Shield Wall stance.
    /// </summary>
    /// <returns>An inactive ShieldWallState.</returns>
    ShieldWallState DeactivateShieldWall();

    /// <summary>
    /// Calculates total defense bonus for a character with active Shield Wall.
    /// </summary>
    /// <param name="shieldWallState">Current Shield Wall state.</param>
    /// <returns>Defense bonus (0 if inactive).</returns>
    int CalculateDefenseBonus(ShieldWallState? shieldWallState);

    /// <summary>
    /// Calculates Bulwark Max HP bonus based on current charges.
    /// </summary>
    /// <param name="blockCharges">Current Block Charge resource.</param>
    /// <returns>Max HP bonus from Bulwark passive.</returns>
    int CalculateBulwarkHpBonus(BlockChargeResource? blockCharges);

    // ═══════ Tier 2: Advanced Techniques (v0.20.1b) ═══════

    /// <summary>
    /// Activates Hold the Line at the specified position for 2 turns.
    /// Enemies cannot move through the blocked position while active.
    /// </summary>
    /// <param name="position">Grid position to block.</param>
    /// <returns>An active HoldTheLineState.</returns>
    HoldTheLineState ActivateHoldTheLine((int X, int Y) position);

    /// <summary>
    /// Advances the Hold the Line effect by one turn.
    /// </summary>
    /// <param name="state">Current Hold the Line state.</param>
    /// <returns>Updated state with decremented turns (deactivated at 0).</returns>
    HoldTheLineState TickHoldTheLine(HoldTheLineState state);

    /// <summary>
    /// Force-deactivates an active Hold the Line effect.
    /// </summary>
    /// <returns>An inactive HoldTheLineState.</returns>
    HoldTheLineState DeactivateHoldTheLine();

    /// <summary>
    /// Executes a Counter-Shield reaction, rolling 1d6 damage against a melee attacker.
    /// </summary>
    /// <param name="skjaldmaerId">ID of the Skjaldmær triggering the reaction.</param>
    /// <param name="attackerId">ID of the melee attacker being countered.</param>
    /// <param name="attackerDefense">The attacker's defense value (for logging).</param>
    /// <returns>A result containing the damage roll and attacker information.</returns>
    CounterShieldResult ExecuteCounterShield(Guid skjaldmaerId, Guid attackerId, int attackerDefense);

    /// <summary>
    /// Activates Rally, granting +2 save bonus to all specified allies.
    /// </summary>
    /// <param name="casterId">ID of the Skjaldmær casting Rally.</param>
    /// <param name="allyIds">IDs of allies within Rally's 6-space radius.</param>
    /// <returns>List of active RallyBuff instances applied to allies.</returns>
    IReadOnlyList<RallyBuff> ActivateRally(Guid casterId, IReadOnlyList<Guid> allyIds);

    /// <summary>
    /// Consumes a Rally buff after a saving throw has been made.
    /// </summary>
    /// <param name="buff">The buff to consume.</param>
    /// <returns>A consumed (inactive) RallyBuff.</returns>
    RallyBuff ConsumeRallyBuff(RallyBuff buff);

    /// <summary>
    /// Calculates the total Rally save bonus for a specific character.
    /// </summary>
    /// <param name="buffs">All active Rally buffs in play.</param>
    /// <param name="allyId">Character to calculate bonus for.</param>
    /// <returns>Total save bonus from all active Rally buffs targeting this character.</returns>
    int GetAllySaveBonus(IReadOnlyList<RallyBuff> buffs, Guid allyId);

    /// <summary>
    /// Checks whether Tier 2 abilities can be unlocked based on PP invested.
    /// </summary>
    /// <param name="ppInvested">Total PP invested in the Skjaldmær tree.</param>
    /// <returns>True if the 8 PP threshold is met.</returns>
    bool CanUnlockTier2(int ppInvested);

    /// <summary>
    /// Calculates total PP invested from a list of unlocked abilities.
    /// </summary>
    /// <param name="unlockedAbilities">Currently unlocked abilities.</param>
    /// <returns>Total PP cost of all unlocked abilities.</returns>
    int CalculatePPInvested(IReadOnlyList<SkjaldmaerAbilityId> unlockedAbilities);

    // ═══════ Tier 3: Master Defenses (v0.20.1c) ═══════

    /// <summary>
    /// Gets damage reduction from Unbreakable passive.
    /// </summary>
    /// <param name="unlockedAbilities">Currently unlocked abilities.</param>
    /// <returns>Damage reduction value (3 if Unbreakable is unlocked, 0 otherwise).</returns>
    int GetDamageReduction(IReadOnlyList<SkjaldmaerAbilityId> unlockedAbilities);

    /// <summary>
    /// Checks whether Tier 3 abilities can be unlocked based on PP invested.
    /// </summary>
    /// <param name="ppInvested">Total PP invested in the Skjaldmær tree.</param>
    /// <returns>True if the 16 PP threshold is met.</returns>
    bool CanUnlockTier3(int ppInvested);

    // ═══════ Capstone: The Wall Lives (v0.20.1c) ═══════

    /// <summary>
    /// Activates The Wall Lives capstone ability.
    /// </summary>
    /// <returns>An active TheWallLivesState with 3 turns remaining.</returns>
    TheWallLivesState ActivateTheWallLives();

    /// <summary>
    /// Advances The Wall Lives effect by one turn.
    /// </summary>
    /// <param name="state">Current The Wall Lives state.</param>
    /// <returns>Updated state with decremented turns (deactivated at 0).</returns>
    TheWallLivesState TickTheWallLives(TheWallLivesState state);

    /// <summary>
    /// Checks if the capstone ability can be used this combat.
    /// </summary>
    /// <param name="hasUsedCapstoneThisCombat">Whether the capstone has already been used.</param>
    /// <returns>True if capstone is available (not already used this combat).</returns>
    bool CanUseCapstone(bool hasUsedCapstoneThisCombat);

    /// <summary>
    /// Checks whether Capstone ability can be unlocked based on PP invested.
    /// </summary>
    /// <param name="ppInvested">Total PP invested in the Skjaldmær tree.</param>
    /// <returns>True if the 24 PP threshold is met.</returns>
    bool CanUnlockCapstone(int ppInvested);
}
