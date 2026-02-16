using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Defines the contract for Bone-Setter Medical Supplies resource management.
/// Handles supply validation, spending, acquisition, and healing bonus calculation.
/// </summary>
/// <remarks>
/// <para>Named <c>IBoneSetterMedicalSuppliesService</c> to avoid collision with any
/// future general-purpose medical or inventory services.</para>
/// <para>Key characteristics distinguishing this from <see cref="IBerserkrRageService"/>:</para>
/// <list type="bullet">
/// <item>Immutable operations — all spend/add methods return new <see cref="MedicalSuppliesResource"/>
/// instances rather than mutating in-place</item>
/// <item>No automatic regeneration — supplies are finite consumables acquired via salvage,
/// purchase, or crafting</item>
/// <item>Quality-based bonuses — each supply item's quality rating (1–5) directly affects
/// healing effectiveness via <c>Quality - 1</c> bonus formula</item>
/// <item>No Corruption interaction — the Bone-Setter follows the Coherent path</item>
/// </list>
/// <para>Key responsibilities:</para>
/// <list type="bullet">
/// <item>Initialize and manage per-character Medical Supplies inventories</item>
/// <item>Validate supply availability before ability execution</item>
/// <item>Spend supplies during healing ability usage (Field Dressing: 1, Emergency Surgery: 2)</item>
/// <item>Add supplies from salvage, purchase, or crafting sources</item>
/// <item>Calculate healing bonuses based on supply quality</item>
/// </list>
/// </remarks>
public interface IBoneSetterMedicalSuppliesService
{
    /// <summary>
    /// Initializes the Medical Supplies resource for a Bone-Setter player.
    /// Creates an empty inventory with default capacity.
    /// </summary>
    /// <param name="player">The Bone-Setter player to initialize.</param>
    /// <remarks>
    /// Should be called when a player selects the Bone-Setter specialization
    /// or at the start of a new game session. Calling on a player who already
    /// has supplies will replace the existing inventory.
    /// </remarks>
    void InitializeSupplies(Player player);

    /// <summary>
    /// Gets the current Medical Supplies resource for a character.
    /// </summary>
    /// <param name="player">The player to query.</param>
    /// <returns>
    /// The player's <see cref="MedicalSuppliesResource"/> if initialized;
    /// null if the player has no Medical Supplies resource.
    /// </returns>
    MedicalSuppliesResource? GetSupplies(Player player);

    /// <summary>
    /// Validates whether the player has at least one supply of any type available to spend.
    /// </summary>
    /// <param name="player">The Bone-Setter player to check.</param>
    /// <returns>True if the player has at least one supply item in inventory.</returns>
    bool ValidateSupplyAvailability(Player player);

    /// <summary>
    /// Validates whether the player has at least one supply of the specified type available.
    /// </summary>
    /// <param name="player">The Bone-Setter player to check.</param>
    /// <param name="type">The specific supply type to check for.</param>
    /// <returns>True if the player has at least one item of the specified type.</returns>
    bool ValidateSupplyAvailability(Player player, MedicalSupplyType type);

    /// <summary>
    /// Spends one supply of any available type (lowest quality first) and updates
    /// the player's inventory.
    /// </summary>
    /// <param name="player">The Bone-Setter player spending a supply.</param>
    /// <returns>
    /// The <see cref="MedicalSupplyItem"/> that was consumed, including its quality
    /// for healing bonus calculation; null if no supplies are available.
    /// </returns>
    /// <remarks>
    /// This method consumes the lowest-quality supply first to preserve higher-quality
    /// items for situations where maximum healing is needed. The player's
    /// <see cref="Player.MedicalSupplies"/> is updated with the new immutable instance.
    /// </remarks>
    MedicalSupplyItem? SpendSupply(Player player);

    /// <summary>
    /// Spends one supply of the specified type and updates the player's inventory.
    /// </summary>
    /// <param name="player">The Bone-Setter player spending a supply.</param>
    /// <param name="type">The specific supply type to consume.</param>
    /// <returns>
    /// The <see cref="MedicalSupplyItem"/> that was consumed; null if no supply
    /// of the specified type is available.
    /// </returns>
    MedicalSupplyItem? SpendSupply(Player player, MedicalSupplyType type);

    /// <summary>
    /// Adds a new supply item to the player's inventory.
    /// </summary>
    /// <param name="player">The Bone-Setter player receiving the supply.</param>
    /// <param name="item">The supply item to add.</param>
    /// <returns>True if the item was successfully added; false if inventory is at capacity.</returns>
    bool AddSupply(Player player, MedicalSupplyItem item);

    /// <summary>
    /// Calculates the healing bonus from a supply item's quality rating.
    /// Formula: Quality - 1 (range: 0 for poor to 4 for superior).
    /// </summary>
    /// <param name="item">The supply item to calculate the bonus for.</param>
    /// <returns>The healing bonus value (0–4).</returns>
    int CalculateQualityBonus(MedicalSupplyItem item);

    /// <summary>
    /// Gets the highest quality supply of a specific type from the player's inventory.
    /// Used to preview healing potential before ability execution.
    /// </summary>
    /// <param name="player">The Bone-Setter player to query.</param>
    /// <param name="type">The supply type to search for.</param>
    /// <returns>
    /// The highest quality <see cref="MedicalSupplyItem"/> of the specified type;
    /// null if no supplies of that type are available.
    /// </returns>
    MedicalSupplyItem? GetHighestQualitySupply(Player player, MedicalSupplyType type);

    /// <summary>
    /// Gets the highest quality supply of any type from the player's inventory.
    /// </summary>
    /// <param name="player">The Bone-Setter player to query.</param>
    /// <returns>
    /// The highest quality <see cref="MedicalSupplyItem"/> in inventory;
    /// null if inventory is empty.
    /// </returns>
    MedicalSupplyItem? GetHighestQualitySupply(Player player);
}
