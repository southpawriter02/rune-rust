using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a crafting material available for recipe-based creation.
/// Used as input to the Bone-Setter's Antidote Craft ability (Tier 2).
/// </summary>
/// <remarks>
/// <para>Crafting materials are acquired through salvage and loot. Each material
/// has a type, quantity, and quality rating that affects crafted output quality.</para>
/// <para>For the Basic Antidote recipe, the required materials are:</para>
/// <list type="bullet">
/// <item>2 <see cref="CraftingMaterialType.PlantFiber"/></item>
/// <item>1 <see cref="CraftingMaterialType.MineralPowder"/></item>
/// </list>
/// <para>If all materials are Quality 3 or higher, a +1 quality bonus is applied
/// to the crafted Antidote.</para>
/// </remarks>
public sealed record CraftingMaterial
{
    /// <summary>
    /// Minimum valid quality rating for a crafting material.
    /// </summary>
    private const int MinQuality = 1;

    /// <summary>
    /// Maximum valid quality rating for a crafting material.
    /// </summary>
    private const int MaxQuality = 5;

    /// <summary>
    /// Minimum valid quantity for a crafting material.
    /// </summary>
    private const int MinQuantity = 1;

    /// <summary>
    /// The type classification of this crafting material.
    /// </summary>
    public CraftingMaterialType Type { get; init; }

    /// <summary>
    /// The quantity of this material available.
    /// Must be at least 1.
    /// </summary>
    public int Quantity { get; init; }

    /// <summary>
    /// Quality rating from 1 (poor salvage) to 5 (pristine/master crafted).
    /// Affects crafted output quality when all materials are Quality 3+.
    /// </summary>
    public int Quality { get; init; }

    /// <summary>
    /// Creates a new crafting material with validation.
    /// </summary>
    /// <param name="type">The type classification of the material.</param>
    /// <param name="quantity">The quantity available. Must be at least 1.</param>
    /// <param name="quality">Quality rating (1–5). Affects crafted output quality.</param>
    /// <returns>A new <see cref="CraftingMaterial"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="quantity"/> is less than 1, or
    /// <paramref name="quality"/> is outside the 1–5 range.
    /// </exception>
    public static CraftingMaterial Create(CraftingMaterialType type, int quantity, int quality)
    {
        if (quantity < MinQuantity)
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity,
                $"Quantity must be at least {MinQuantity}.");

        if (quality < MinQuality || quality > MaxQuality)
            throw new ArgumentOutOfRangeException(nameof(quality), quality,
                $"Quality must be between {MinQuality} and {MaxQuality}.");

        return new CraftingMaterial
        {
            Type = type,
            Quantity = quantity,
            Quality = quality
        };
    }
}
