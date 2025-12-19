namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Defines the contract for stat calculation services.
/// Provides basic modifier operations and attribute value clamping.
/// </summary>
public interface IStatCalculationService
{
    /// <summary>
    /// Applies a modifier to a base value.
    /// </summary>
    /// <param name="baseValue">The original value.</param>
    /// <param name="modifier">The modifier to apply (positive or negative).</param>
    /// <returns>The modified value (baseValue + modifier).</returns>
    int ApplyModifier(int baseValue, int modifier);

    /// <summary>
    /// Clamps an attribute value to ensure it stays within valid bounds.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <param name="min">The minimum allowed value (default: 1).</param>
    /// <param name="max">The maximum allowed value (default: 10).</param>
    /// <returns>The clamped value, guaranteed to be within [min, max].</returns>
    int ClampAttribute(int value, int min = 1, int max = 10);
}
