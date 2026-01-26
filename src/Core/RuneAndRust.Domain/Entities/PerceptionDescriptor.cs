using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Stores perception success text templates for different hidden element types.
/// Used to generate discovery narratives when hidden elements are revealed.
/// </summary>
public class PerceptionDescriptor : IEntity
{
    public Guid Id { get; private set; }
    public HiddenElementType PerceptionType { get; private set; }
    public PerceptionSuccessLevel SuccessLevel { get; private set; }
    public int RequiredDC { get; private set; }
    public Biome? BiomeAffinity { get; private set; }
    public string DescriptorText { get; private set; }
    public int Weight { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private PerceptionDescriptor()
    {
        DescriptorText = null!;
    } // For EF Core

    public PerceptionDescriptor(
        HiddenElementType perceptionType,
        PerceptionSuccessLevel successLevel,
        int requiredDC,
        string descriptorText,
        Biome? biomeAffinity = null,
        int weight = 1)
    {
        if (string.IsNullOrWhiteSpace(descriptorText))
            throw new ArgumentException("Descriptor text cannot be empty", nameof(descriptorText));
        if (requiredDC < 1)
            throw new ArgumentOutOfRangeException(nameof(requiredDC), "Required DC must be at least 1");
        if (weight < 1)
            throw new ArgumentOutOfRangeException(nameof(weight), "Weight must be at least 1");

        Id = Guid.NewGuid();
        PerceptionType = perceptionType;
        SuccessLevel = successLevel;
        RequiredDC = requiredDC;
        DescriptorText = descriptorText;
        BiomeAffinity = biomeAffinity;
        Weight = weight;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a standard perception descriptor for trap detection.
    /// </summary>
    public static PerceptionDescriptor CreateTrapDetection(
        string descriptorText,
        int requiredDC = 15,
        Biome? biomeAffinity = null) =>
        new(HiddenElementType.Trap, PerceptionSuccessLevel.Standard, requiredDC, descriptorText, biomeAffinity);

    /// <summary>
    /// Creates an expert perception descriptor for trap detection with additional context.
    /// </summary>
    public static PerceptionDescriptor CreateExpertTrapDetection(
        string descriptorText,
        int requiredDC = 20,
        Biome? biomeAffinity = null) =>
        new(HiddenElementType.Trap, PerceptionSuccessLevel.Expert, requiredDC, descriptorText, biomeAffinity);

    /// <summary>
    /// Creates a perception descriptor for secret door detection.
    /// </summary>
    public static PerceptionDescriptor CreateSecretDoorDetection(
        string descriptorText,
        int requiredDC = 16,
        Biome? biomeAffinity = null) =>
        new(HiddenElementType.SecretDoor, PerceptionSuccessLevel.Standard, requiredDC, descriptorText, biomeAffinity);

    /// <summary>
    /// Creates a perception descriptor for hidden cache detection.
    /// </summary>
    public static PerceptionDescriptor CreateCacheDetection(
        string descriptorText,
        int requiredDC = 14,
        Biome? biomeAffinity = null) =>
        new(HiddenElementType.HiddenCache, PerceptionSuccessLevel.Standard, requiredDC, descriptorText, biomeAffinity);

    public override string ToString() => $"{PerceptionType} ({SuccessLevel}, DC {RequiredDC})";
}
