namespace RuneAndRust.Domain.Entities;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

/// <summary>
/// Stores category-specific examination text for objects.
/// </summary>
/// <remarks>
/// <para>
/// Each descriptor targets a specific combination of:
/// </para>
/// <list type="bullet">
///   <item><description>Object category (Door, Machinery, etc.)</description></item>
///   <item><description>Object type within category (LockedDoor, Console, etc.)</description></item>
///   <item><description>Examination layer (1 = Cursory, 2 = Detailed, 3 = Expert)</description></item>
///   <item><description>Optional biome affinity (null = universal)</description></item>
/// </list>
/// <para>
/// Multiple descriptors can exist for the same criteria with different
/// weights, allowing randomized variety in examination text.
/// </para>
/// <para>
/// Descriptors at Layer 3 may reveal puzzle hints (v0.15.6e integration).
/// </para>
/// </remarks>
public class ObjectDescriptor : IEntity
{
    /// <summary>
    /// Gets the unique identifier for this entity.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the unique string identifier for this descriptor.
    /// </summary>
    /// <remarks>
    /// Format: "{type}-{layer}-{index}" (e.g., "locked-door-cursory-01")
    /// </remarks>
    public string DescriptorId { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the object category this descriptor applies to.
    /// </summary>
    public ObjectCategory ObjectCategory { get; private set; }

    /// <summary>
    /// Gets the specific object type within the category.
    /// </summary>
    /// <remarks>
    /// Examples: "LockedDoor", "BlastDoor", "Console", "Terminal"
    /// </remarks>
    public string ObjectType { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the examination layer this descriptor targets (1, 2, or 3).
    /// </summary>
    /// <remarks>
    /// Layer 1 = Cursory (DC 0), Layer 2 = Detailed (DC 12), Layer 3 = Expert (DC 18)
    /// </remarks>
    public int Layer { get; private set; }

    /// <summary>
    /// Gets the DC required to unlock this descriptor's layer.
    /// </summary>
    /// <remarks>
    /// Layer 1: DC 0 (auto), Layer 2: DC 12, Layer 3: DC 18
    /// </remarks>
    public int RequiredDc { get; private set; }

    /// <summary>
    /// Gets the biome affinity for this descriptor, or null if universal.
    /// </summary>
    /// <remarks>
    /// When set, this descriptor is only used in the specified biome.
    /// When null, the descriptor is available in all biomes.
    /// Biome-specific descriptors are preferred over universal ones.
    /// </remarks>
    public string? BiomeAffinity { get; private set; }

    /// <summary>
    /// Gets the examination text displayed to the player.
    /// </summary>
    public string DescriptorText { get; private set; } = string.Empty;

    /// <summary>
    /// Gets whether this descriptor reveals a puzzle hint (Layer 3 only).
    /// </summary>
    /// <remarks>
    /// When true, examining at Layer 3 triggers hint integration (v0.15.6e).
    /// </remarks>
    public bool RevealsHint { get; private set; }

    /// <summary>
    /// Gets the puzzle solution ID revealed by this descriptor, if any.
    /// </summary>
    /// <remarks>
    /// Links to PuzzleHint definitions (v0.15.6e).
    /// Only relevant when RevealsHint is true.
    /// </remarks>
    public string? RevealsSolutionId { get; private set; }

    /// <summary>
    /// Gets the weight for variant selection (higher = more likely).
    /// </summary>
    /// <remarks>
    /// Default: 1. Multiple descriptors with the same criteria use
    /// weighted random selection for variety.
    /// </remarks>
    public int Weight { get; private set; } = 1;

    /// <summary>
    /// Gets whether this descriptor is universal (not biome-specific).
    /// </summary>
    public bool IsUniversal => BiomeAffinity is null;

    /// <summary>
    /// Gets the layer name for display purposes.
    /// </summary>
    public string LayerName => Layer switch
    {
        1 => "Cursory",
        2 => "Detailed",
        3 => "Expert",
        _ => "Unknown"
    };

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private ObjectDescriptor() { }

    /// <summary>
    /// Creates a new object descriptor.
    /// </summary>
    /// <param name="descriptorId">Unique identifier for this descriptor.</param>
    /// <param name="category">The object category.</param>
    /// <param name="objectType">The specific object type.</param>
    /// <param name="layer">The examination layer (1, 2, or 3).</param>
    /// <param name="descriptorText">The examination text.</param>
    /// <param name="biomeAffinity">Optional biome restriction.</param>
    /// <param name="weight">Selection weight (default: 1).</param>
    /// <param name="revealsHint">Whether this reveals a puzzle hint.</param>
    /// <param name="solutionId">The puzzle solution ID to reveal.</param>
    /// <returns>A new ObjectDescriptor instance.</returns>
    /// <exception cref="ArgumentException">Thrown when IDs or text are null/whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when layer or weight is invalid.</exception>
    public static ObjectDescriptor Create(
        string descriptorId,
        ObjectCategory category,
        string objectType,
        int layer,
        string descriptorText,
        string? biomeAffinity = null,
        int weight = 1,
        bool revealsHint = false,
        string? solutionId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descriptorId);
        ArgumentException.ThrowIfNullOrWhiteSpace(objectType);
        ArgumentException.ThrowIfNullOrWhiteSpace(descriptorText);
        ArgumentOutOfRangeException.ThrowIfLessThan(layer, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(layer, 3);
        ArgumentOutOfRangeException.ThrowIfLessThan(weight, 1);

        var requiredDc = layer switch
        {
            1 => 0,
            2 => 12,
            3 => 18,
            _ => 0
        };

        return new ObjectDescriptor
        {
            Id = Guid.NewGuid(),
            DescriptorId = descriptorId.ToLowerInvariant(),
            ObjectCategory = category,
            ObjectType = objectType,
            Layer = layer,
            RequiredDc = requiredDc,
            BiomeAffinity = biomeAffinity,
            DescriptorText = descriptorText,
            RevealsHint = revealsHint,
            RevealsSolutionId = solutionId,
            Weight = weight
        };
    }

    /// <summary>
    /// Checks if this descriptor matches the specified biome.
    /// </summary>
    /// <param name="biomeId">The biome to check.</param>
    /// <returns>True if this descriptor is valid for the biome.</returns>
    /// <remarks>
    /// Universal descriptors (BiomeAffinity = null) match all biomes.
    /// Biome-specific descriptors only match their specified biome.
    /// </remarks>
    public bool MatchesBiome(string? biomeId)
    {
        // Universal descriptors match all biomes
        if (IsUniversal)
        {
            return true;
        }

        // Biome-specific descriptors match only their biome
        return string.Equals(BiomeAffinity, biomeId, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Returns a string representation of this descriptor for debugging.
    /// </summary>
    /// <returns>A formatted string showing key descriptor values.</returns>
    public override string ToString() =>
        $"ObjectDescriptor({DescriptorId}, {ObjectCategory}/{ObjectType}, Layer {Layer}" +
        $"{(BiomeAffinity is not null ? $", Biome={BiomeAffinity}" : "")})";
}
