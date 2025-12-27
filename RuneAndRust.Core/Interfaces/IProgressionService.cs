using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Models;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Service for managing character attribute progression.
/// Handles spending Progression Points (PP) to upgrade character attributes.
/// </summary>
/// <remarks>See: v0.4.0b (The Growth) for implementation details.</remarks>
public interface IProgressionService
{
    /// <summary>
    /// Attempts to upgrade a character's attribute by spending Progression Points.
    /// </summary>
    /// <param name="character">The character to upgrade.</param>
    /// <param name="attribute">The attribute to upgrade.</param>
    /// <returns>An AttributeUpgradeResult indicating success or failure with details.</returns>
    AttributeUpgradeResult UpgradeAttribute(Entities.Character character, CharacterAttribute attribute);

    /// <summary>
    /// Gets the PP cost to upgrade a specific attribute for a character.
    /// </summary>
    /// <param name="character">The character whose attribute cost is being queried.</param>
    /// <param name="attribute">The attribute to check.</param>
    /// <returns>The PP cost, or int.MaxValue if the attribute is at cap.</returns>
    int GetUpgradeCost(Entities.Character character, CharacterAttribute attribute);

    /// <summary>
    /// Checks if a character can afford and is eligible to upgrade an attribute.
    /// </summary>
    /// <param name="character">The character to check.</param>
    /// <param name="attribute">The attribute to check.</param>
    /// <returns>True if the upgrade is possible; false otherwise.</returns>
    bool CanUpgrade(Entities.Character character, CharacterAttribute attribute);
}
