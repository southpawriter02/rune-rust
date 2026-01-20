using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Stores layered examination text for objects.
/// Each object type can have multiple descriptors for different layers (Cursory, Detailed, Expert).
/// </summary>
public class ExaminationDescriptor : IEntity
{
    public Guid Id { get; private set; }
    public ObjectCategory ObjectCategory { get; private set; }
    public string ObjectType { get; private set; }
    public ExaminationLayer Layer { get; private set; }
    public int RequiredDC { get; private set; }
    public Biome? BiomeAffinity { get; private set; }
    public string DescriptorText { get; private set; }
    public bool RevealsHint { get; private set; }
    public string? RevealsSolutionId { get; private set; }
    public int Weight { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private ExaminationDescriptor()
    {
        ObjectType = null!;
        DescriptorText = null!;
    } // For EF Core

    public ExaminationDescriptor(
        ObjectCategory objectCategory,
        string objectType,
        ExaminationLayer layer,
        string descriptorText,
        int requiredDC = 0,
        Biome? biomeAffinity = null,
        bool revealsHint = false,
        string? revealsSolutionId = null,
        int weight = 1)
    {
        if (string.IsNullOrWhiteSpace(objectType))
            throw new ArgumentException("Object type cannot be empty", nameof(objectType));
        if (string.IsNullOrWhiteSpace(descriptorText))
            throw new ArgumentException("Descriptor text cannot be empty", nameof(descriptorText));
        if (weight < 1)
            throw new ArgumentOutOfRangeException(nameof(weight), "Weight must be at least 1");

        Id = Guid.NewGuid();
        ObjectCategory = objectCategory;
        ObjectType = objectType;
        Layer = layer;
        RequiredDC = layer switch
        {
            ExaminationLayer.Cursory => 0,
            ExaminationLayer.Detailed => requiredDC > 0 ? requiredDC : 12,
            ExaminationLayer.Expert => requiredDC > 0 ? requiredDC : 18,
            _ => requiredDC
        };
        BiomeAffinity = biomeAffinity;
        DescriptorText = descriptorText;
        RevealsHint = revealsHint;
        RevealsSolutionId = revealsSolutionId;
        Weight = weight;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a Cursory (Layer 1) descriptor - always visible, no check required.
    /// </summary>
    public static ExaminationDescriptor CreateCursory(
        ObjectCategory category,
        string objectType,
        string descriptorText,
        Biome? biomeAffinity = null) =>
        new(category, objectType, ExaminationLayer.Cursory, descriptorText, 0, biomeAffinity);

    /// <summary>
    /// Creates a Detailed (Layer 2) descriptor - requires DC 12 WITS check.
    /// </summary>
    public static ExaminationDescriptor CreateDetailed(
        ObjectCategory category,
        string objectType,
        string descriptorText,
        Biome? biomeAffinity = null,
        bool revealsHint = false) =>
        new(category, objectType, ExaminationLayer.Detailed, descriptorText, 12, biomeAffinity, revealsHint);

    /// <summary>
    /// Creates an Expert (Layer 3) descriptor - requires DC 18 WITS check.
    /// </summary>
    public static ExaminationDescriptor CreateExpert(
        ObjectCategory category,
        string objectType,
        string descriptorText,
        Biome? biomeAffinity = null,
        bool revealsHint = false,
        string? revealsSolutionId = null) =>
        new(category, objectType, ExaminationLayer.Expert, descriptorText, 18, biomeAffinity, revealsHint, revealsSolutionId);

    public override string ToString() => $"{ObjectCategory}/{ObjectType} (Layer {(int)Layer})";
}
