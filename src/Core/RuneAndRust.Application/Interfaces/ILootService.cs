using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for generating and managing loot drops from defeated monsters.
/// </summary>
public interface ILootService
{
    /// <summary>
    /// Generates loot from a defeated monster using its loot multiplier.
    /// </summary>
    /// <param name="monster">The defeated monster.</param>
    /// <returns>A LootDrop containing generated items and currency.</returns>
    LootDrop GenerateLoot(Monster monster);

    /// <summary>
    /// Generates loot from a monster definition with a specified multiplier.
    /// </summary>
    /// <param name="definition">The monster definition containing the loot table.</param>
    /// <param name="lootMultiplier">Multiplier applied to drop chances and currency amounts.</param>
    /// <returns>A LootDrop containing generated items and currency.</returns>
    LootDrop GenerateLoot(MonsterDefinition definition, float lootMultiplier = 1.0f);

    /// <summary>
    /// Collects all dropped loot from a room and adds it to the player's inventory.
    /// </summary>
    /// <param name="player">The player collecting the loot.</param>
    /// <param name="room">The room containing the dropped loot.</param>
    /// <returns>A LootDrop containing the collected items and currency for display.</returns>
    LootDrop CollectLoot(Player player, Room room);

    /// <summary>
    /// Gets a currency definition by ID.
    /// </summary>
    /// <param name="currencyId">The currency identifier.</param>
    /// <returns>The currency definition or null if not found.</returns>
    CurrencyDefinition? GetCurrency(string currencyId);

    /// <summary>
    /// Gets all available currency definitions.
    /// </summary>
    /// <returns>A read-only list of all currency definitions.</returns>
    IReadOnlyList<CurrencyDefinition> GetAllCurrencies();
}
