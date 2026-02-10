using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Defines the contract for Rúnasmiðr specialization ability operations.
/// Handles Tier 1 and Tier 2 ability execution including rune inscription, passive identification,
/// ward creation, empowered inscriptions, runic traps, and crafting cost reduction.
/// </summary>
/// <remarks>
/// <para>Tier 1 abilities (0 PP, free on selection):</para>
/// <list type="bullet">
/// <item>Inscribe Rune (Active) — enhance weapon (+2 damage) or armor (+1 Defense) for 10 turns</item>
/// <item>Read the Marks (Passive) — auto-identify Jötun technology</item>
/// <item>Runestone Ward (Active) — create ward absorbing up to 10 damage</item>
/// </list>
/// <para>Tier 2 abilities (8 PP required, 4 PP each):</para>
/// <list type="bullet">
/// <item>Empowered Inscription (Active) — add +1d6 elemental damage to weapon for 10 turns</item>
/// <item>Runic Trap (Active) — place invisible trap dealing 3d6 damage, max 3 active</item>
/// <item>Dvergr Techniques (Passive) — 20% crafting cost and time reduction</item>
/// </list>
/// <para>Follows the same interface pattern as <see cref="ISkjaldmaerAbilityService"/>
/// for consistency across specialization implementations.</para>
/// </remarks>
public interface IRunasmidrAbilityService
{
    /// <summary>
    /// Executes the Inscribe Rune ability on a target item.
    /// </summary>
    /// <param name="player">The Rúnasmiðr player.</param>
    /// <param name="targetItemId">The equipment item ID to inscribe.</param>
    /// <param name="isWeapon">True for weapon enhancement (+damage), false for armor protection (+defense).</param>
    /// <returns>
    /// The created <see cref="InscribedRune"/> if successful; null if prerequisites not met.
    /// </returns>
    /// <remarks>
    /// Prerequisites: Inscribe Rune unlocked, 3+ AP available, 1+ Rune Charge available.
    /// On success: deducts 3 AP and 1 Rune Charge, creates rune with 10-turn duration.
    /// Replaces any existing rune on the same item.
    /// </remarks>
    InscribedRune? ExecuteInscribeRune(Player player, Guid targetItemId, bool isWeapon);

    /// <summary>
    /// Executes the Runestone Ward ability.
    /// </summary>
    /// <param name="player">The Rúnasmiðr player.</param>
    /// <returns>
    /// The created <see cref="RunestoneWard"/> if successful; null if prerequisites not met.
    /// </returns>
    /// <remarks>
    /// Prerequisites: Runestone Ward unlocked, 2+ AP available, 1+ Rune Charge available.
    /// On success: deducts 2 AP and 1 Rune Charge, creates ward with 10 absorption.
    /// Replaces any existing ward on the player.
    /// </remarks>
    RunestoneWard? ExecuteRunestoneWard(Player player);

    // ===== Tier 2 Abilities (v0.20.2b) =====

    /// <summary>
    /// Executes the Empowered Inscription ability on a target weapon.
    /// </summary>
    /// <param name="player">The Rúnasmiðr player.</param>
    /// <param name="targetItemId">The weapon item ID to empower.</param>
    /// <param name="elementalTypeId">The elemental type ("fire", "cold", "lightning", or "aetheric").</param>
    /// <returns>
    /// The created <see cref="EmpoweredRune"/> if successful; null if prerequisites not met.
    /// </returns>
    /// <remarks>
    /// Prerequisites: Empowered Inscription unlocked (8+ PP invested), 4+ AP available, 2+ Rune Charges.
    /// On success: deducts 4 AP and 2 Rune Charges, creates empowered rune with 10-turn duration.
    /// Replaces any existing empowered rune on the same weapon. Stacks with base Inscribe Rune.
    /// </remarks>
    EmpoweredRune? ExecuteEmpoweredInscription(Player player, Guid targetItemId, string elementalTypeId);

    /// <summary>
    /// Executes the Runic Trap ability at a specified position.
    /// </summary>
    /// <param name="player">The Rúnasmiðr player.</param>
    /// <param name="positionX">The X grid coordinate for trap placement.</param>
    /// <param name="positionY">The Y grid coordinate for trap placement.</param>
    /// <returns>
    /// The created <see cref="RunicTrap"/> if successful; null if prerequisites not met.
    /// </returns>
    /// <remarks>
    /// Prerequisites: Runic Trap unlocked (8+ PP invested), 3+ AP available, 2+ Rune Charges, fewer than 3 active traps.
    /// On success: deducts 3 AP and 2 Rune Charges, creates trap with DC 14, 3d6 damage, 1-hour expiration.
    /// </remarks>
    RunicTrap? ExecuteRunicTrap(Player player, int positionX, int positionY);

    // ===== Query Methods =====

    /// <summary>
    /// Gets all active runes for a player.
    /// </summary>
    /// <param name="player">The Rúnasmiðr player.</param>
    /// <returns>Read-only list of active runes, or empty if none.</returns>
    IReadOnlyList<InscribedRune> GetActiveRunes(Player player);

    /// <summary>
    /// Gets the player's active ward, if any.
    /// </summary>
    /// <param name="player">The Rúnasmiðr player.</param>
    /// <returns>The active ward, or null if no ward is active.</returns>
    RunestoneWard? GetActiveWard(Player player);

    /// <summary>
    /// Gets all active empowered runes for a player.
    /// </summary>
    /// <param name="player">The Rúnasmiðr player.</param>
    /// <returns>Read-only list of active empowered runes, or empty if none.</returns>
    IReadOnlyList<EmpoweredRune> GetEmpoweredRunes(Player player);

    /// <summary>
    /// Gets all active (non-triggered, non-expired) traps for a player.
    /// </summary>
    /// <param name="player">The Rúnasmiðr player.</param>
    /// <returns>Read-only list of active traps, or empty if none.</returns>
    IReadOnlyList<RunicTrap> GetActiveTraps(Player player);

    // ===== Tier Checks =====

    /// <summary>
    /// Checks if the player meets Tier 2 unlock requirements (8+ PP invested).
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player has 8 or more PP invested in the Rúnasmiðr tree.</returns>
    bool CanUnlockTier2(Player player);

    /// <summary>
    /// Checks if the player meets Tier 3 unlock requirements (16+ PP invested).
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player has 16 or more PP invested in the Rúnasmiðr tree.</returns>
    bool CanUnlockTier3(Player player);

    /// <summary>
    /// Checks if the player meets Capstone unlock requirements (24+ PP invested).
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player has 24 or more PP invested in the Rúnasmiðr tree.</returns>
    bool CanUnlockCapstone(Player player);

    /// <summary>
    /// Gets total Progression Points invested in the Rúnasmiðr ability tree.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>Total PP invested.</returns>
    int GetPPInvested(Player player);
}

