using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Stores layered descriptions for flora and fauna by biome.
/// Supports ambient observations and harvesting mechanics.
/// </summary>
public class FloraFaunaDescriptor : IEntity
{
    public Guid Id { get; private set; }
    public FloraFaunaCategory Category { get; private set; }
    public string SpeciesName { get; private set; }
    public string? ScientificName { get; private set; }
    public ExaminationLayer Layer { get; private set; }
    public int RequiredDC { get; private set; }
    public Biome Biome { get; private set; }
    public string DescriptorText { get; private set; }
    public string? AlchemicalUse { get; private set; }
    public int? HarvestDC { get; private set; }
    public string? HarvestRisk { get; private set; }
    public int Weight { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private FloraFaunaDescriptor()
    {
        SpeciesName = null!;
        DescriptorText = null!;
    } // For EF Core

    public FloraFaunaDescriptor(
        FloraFaunaCategory category,
        string speciesName,
        Biome biome,
        ExaminationLayer layer,
        string descriptorText,
        string? scientificName = null,
        int requiredDC = 0,
        string? alchemicalUse = null,
        int? harvestDC = null,
        string? harvestRisk = null,
        int weight = 1)
    {
        if (string.IsNullOrWhiteSpace(speciesName))
            throw new ArgumentException("Species name cannot be empty", nameof(speciesName));
        if (string.IsNullOrWhiteSpace(descriptorText))
            throw new ArgumentException("Descriptor text cannot be empty", nameof(descriptorText));
        if (weight < 1)
            throw new ArgumentOutOfRangeException(nameof(weight), "Weight must be at least 1");

        Id = Guid.NewGuid();
        Category = category;
        SpeciesName = speciesName;
        ScientificName = scientificName;
        Layer = layer;
        RequiredDC = layer switch
        {
            ExaminationLayer.Cursory => 0,
            ExaminationLayer.Detailed => requiredDC > 0 ? requiredDC : 12,
            ExaminationLayer.Expert => requiredDC > 0 ? requiredDC : 18,
            _ => requiredDC
        };
        Biome = biome;
        DescriptorText = descriptorText;
        AlchemicalUse = alchemicalUse;
        HarvestDC = harvestDC;
        HarvestRisk = harvestRisk;
        Weight = weight;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a cursory flora descriptor.
    /// </summary>
    public static FloraFaunaDescriptor CreateFloraCursory(
        string speciesName,
        Biome biome,
        string descriptorText) =>
        new(FloraFaunaCategory.Flora, speciesName, biome, ExaminationLayer.Cursory, descriptorText);

    /// <summary>
    /// Creates a detailed flora descriptor with alchemical information.
    /// </summary>
    public static FloraFaunaDescriptor CreateFloraDetailed(
        string speciesName,
        Biome biome,
        string descriptorText,
        string? alchemicalUse = null) =>
        new(FloraFaunaCategory.Flora, speciesName, biome, ExaminationLayer.Detailed, descriptorText,
            alchemicalUse: alchemicalUse);

    /// <summary>
    /// Creates an expert flora descriptor with full harvesting information.
    /// </summary>
    public static FloraFaunaDescriptor CreateFloraExpert(
        string speciesName,
        Biome biome,
        string descriptorText,
        string? scientificName = null,
        string? alchemicalUse = null,
        int? harvestDC = null,
        string? harvestRisk = null) =>
        new(FloraFaunaCategory.Flora, speciesName, biome, ExaminationLayer.Expert, descriptorText,
            scientificName, alchemicalUse: alchemicalUse, harvestDC: harvestDC, harvestRisk: harvestRisk);

    /// <summary>
    /// Creates a fauna observation descriptor.
    /// </summary>
    public static FloraFaunaDescriptor CreateFaunaObservation(
        string speciesName,
        Biome biome,
        string descriptorText,
        ExaminationLayer layer = ExaminationLayer.Cursory) =>
        new(FloraFaunaCategory.Fauna, speciesName, biome, layer, descriptorText);

    /// <summary>
    /// Creates an expert fauna observation with scientific name.
    /// </summary>
    public static FloraFaunaDescriptor CreateFaunaExpert(
        string speciesName,
        Biome biome,
        string descriptorText,
        string scientificName) =>
        new(FloraFaunaCategory.Fauna, speciesName, biome, ExaminationLayer.Expert, descriptorText, scientificName);

    /// <summary>
    /// Indicates if this species can be harvested.
    /// </summary>
    public bool IsHarvestable => HarvestDC.HasValue;

    public override string ToString() => $"{Category}: {SpeciesName} ({Biome}, Layer {(int)Layer})";
}
