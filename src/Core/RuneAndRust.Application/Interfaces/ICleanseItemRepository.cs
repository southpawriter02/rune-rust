using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Repository for loading cleanse item definitions.
/// </summary>
public interface ICleanseItemRepository
{
    /// <summary>Gets a cleanse item by its ID.</summary>
    CleanseItem? GetById(string itemId);

    /// <summary>Checks if an item ID is a cleanse item.</summary>
    bool IsCleanseItem(string itemId);

    /// <summary>Gets all cleanse items.</summary>
    IReadOnlyList<CleanseItem> GetAll();
}
