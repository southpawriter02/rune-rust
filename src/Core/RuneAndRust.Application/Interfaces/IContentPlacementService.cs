using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for placing content in rooms based on template slots and difficulty.
/// </summary>
public interface IContentPlacementService
{
    /// <summary>
    /// Determines if a slot should be filled based on its probability and the seeded random.
    /// </summary>
    /// <param name="slot">The template slot to evaluate.</param>
    /// <param name="position">The room position for seeded random.</param>
    /// <returns>True if the slot should be filled.</returns>
    bool ShouldFillSlot(TemplateSlot slot, Position3D position);

    /// <summary>
    /// Selects a monster tier based on difficulty and slot constraints.
    /// </summary>
    /// <param name="difficulty">The room's difficulty rating.</param>
    /// <param name="slot">The template slot with constraints.</param>
    /// <param name="position">The room position for seeded random.</param>
    /// <returns>The selected monster tier ID.</returns>
    string SelectMonsterTier(DifficultyRating difficulty, TemplateSlot slot, Position3D position);

    /// <summary>
    /// Determines the quantity of monsters to spawn within the slot's range.
    /// </summary>
    /// <param name="slot">The template slot with min/max quantities.</param>
    /// <param name="position">The room position for seeded random.</param>
    /// <returns>The number of monsters to spawn.</returns>
    int DetermineMonsterQuantity(TemplateSlot slot, Position3D position);

    /// <summary>
    /// Selects an item rarity based on difficulty and loot quality multiplier.
    /// </summary>
    /// <param name="difficulty">The room's difficulty rating.</param>
    /// <param name="slot">The template slot with constraints.</param>
    /// <param name="position">The room position for seeded random.</param>
    /// <returns>The selected item rarity ID.</returns>
    string SelectItemRarity(DifficultyRating difficulty, TemplateSlot slot, Position3D position);

    /// <summary>
    /// Determines the quantity of items to spawn within the slot's range.
    /// </summary>
    /// <param name="slot">The template slot with min/max quantities.</param>
    /// <param name="position">The room position for seeded random.</param>
    /// <returns>The number of items to spawn.</returns>
    int DetermineItemQuantity(TemplateSlot slot, Position3D position);

    /// <summary>
    /// Gets the monster level bonus based on difficulty.
    /// </summary>
    /// <param name="difficulty">The room's difficulty rating.</param>
    /// <returns>Level bonus to add to monster base level.</returns>
    int GetMonsterLevelBonus(DifficultyRating difficulty);
}
