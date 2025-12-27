using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Core.Models;

/// <summary>
/// Result of an attribute upgrade operation.
/// Follows the domain-specific Result pattern used throughout the codebase.
/// </summary>
/// <remarks>See: v0.4.0b (The Growth) for Progression system implementation.</remarks>
/// <param name="Success">Whether the upgrade succeeded.</param>
/// <param name="Message">A human-readable description of the result.</param>
/// <param name="Attribute">The attribute that was upgraded (null if failed).</param>
/// <param name="OldValue">The attribute value before upgrade.</param>
/// <param name="NewValue">The attribute value after upgrade.</param>
/// <param name="PpSpent">The Progression Points spent on this upgrade.</param>
public record AttributeUpgradeResult(
    bool Success,
    string Message,
    CharacterAttribute? Attribute = null,
    int OldValue = 0,
    int NewValue = 0,
    int PpSpent = 0)
{
    /// <summary>
    /// Creates a successful upgrade result.
    /// </summary>
    /// <param name="message">Description of the successful upgrade.</param>
    /// <param name="attribute">The attribute that was upgraded.</param>
    /// <param name="oldValue">The previous attribute value.</param>
    /// <param name="newValue">The new attribute value.</param>
    /// <param name="cost">The PP spent on the upgrade.</param>
    /// <returns>A successful AttributeUpgradeResult.</returns>
    public static AttributeUpgradeResult Ok(
        string message,
        CharacterAttribute attribute,
        int oldValue,
        int newValue,
        int cost)
        => new(true, message, attribute, oldValue, newValue, cost);

    /// <summary>
    /// Creates a failed upgrade result.
    /// </summary>
    /// <param name="reason">The reason for the failure.</param>
    /// <returns>A failed AttributeUpgradeResult.</returns>
    public static AttributeUpgradeResult Failure(string reason)
        => new(false, reason);
}
