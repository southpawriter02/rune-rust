using RuneAndRust.Core.Entities;

namespace RuneAndRust.Core.Models.Combat;

/// <summary>
/// Result of a completed combat encounter.
/// Contains victory state, rewards, and summary for the player.
/// </summary>
/// <param name="Victory">Whether the player won the combat.</param>
/// <param name="XpEarned">Experience points earned from the encounter.</param>
/// <param name="LootFound">Items dropped by defeated enemies.</param>
/// <param name="Summary">Human-readable summary of the combat outcome.</param>
public record CombatResult(
    bool Victory,
    int XpEarned,
    List<Item> LootFound,
    string Summary
);
